using RoR2;
using UnityEngine;

namespace DamageLog
{
    public sealed class DamageSource
    {
        private static Texture _PlanetPortrait;
        public static Texture PlanetPortrait {
            get {
                if (_PlanetPortrait == null) {
                    _PlanetPortrait = LegacyResourcesAPI.Load<Texture>("Textures/BodyIcons/texUnidentifiedKillerIcon");
                }
                return _PlanetPortrait;
            }
        }

        public readonly string identifier;

        public readonly Texture attackerPortrait;
        public readonly Color attackerColor;
        public readonly string attackerName;
        public readonly Sprite eliteIcon;
        public readonly Color eliteColor;

        public readonly bool isPlayerDamage;
        public readonly bool isFallDamage;
        public readonly bool isVoidFogDamage;

        public readonly float timeStart;
#pragma warning disable IDE1006 // Naming rule violation: must begin with upper case character
        public float time { get; private set; }
        public int hits { get; private set; }
        public float damage { get; private set; }
        public float damagePercent { get; private set; }
        public float hpPercent { get; private set; }
#pragma warning restore IDE1006 // Naming rule violation: must begin with upper case character

        public DamageSource(DamageDealtMessage e)
        {
            CharacterBody body = e.attacker?.GetComponent<CharacterBody>();
            isPlayerDamage = body?.isPlayerControlled ?? false;
            isFallDamage = IsFallDamage(e);
            isVoidFogDamage = IsVoidFogDamage(e);

            identifier = GenerateIdentifier(e.attacker, isFallDamage, isVoidFogDamage);
            GetAttackerInfo(e.attacker, isFallDamage, isVoidFogDamage, out attackerName, out attackerPortrait, out attackerColor);
            GetEliteIcon(body, out eliteIcon, out eliteColor);

            if (identifier == "??") identifier += $" | {e.damageType} | {e.damageColorIndex} | {e.damage}";

            timeStart = Time.time;
            time = timeStart;
            hits = 1;
            damage = e.damage;
            UpdateHpDamagePercent(e.victim, e.damage);
        }

        public DamageSource Add(DamageDealtMessage e)
        {
            time = Time.time;
            hits++;
            damage += e.damage;
            UpdateHpDamagePercent(e.victim, e.damage);
            return this;
        }

        /// <remarks>
        /// <see cref="HealthComponent"/> may(?) not be updated to account for damage when called on non-host.
        /// </remarks>
        /// <param name="victim"></param>
        private void UpdateHpDamagePercent(GameObject victim, float latestHitDamage)
        {
            HealthComponent health = victim?.GetComponent<HealthComponent>();
            if (health != null) {
                hpPercent = UnityEngine.Networking.NetworkServer.active ?
                    health.combinedHealthFraction :
                    ((health.combinedHealth - latestHitDamage) / health.fullCombinedHealth);
                damagePercent = damage / health.fullCombinedHealth;
            }
        }

        public static bool IsFallDamage(DamageDealtMessage e) => e.damageType.HasFlag(DamageType.FallDamage);

        public static bool IsVoidFogDamage(DamageDealtMessage e)
        {
            // RoR2.FogDamageController.FixedUpdate()
            return (e.damageType == (DamageType.BypassArmor | DamageType.BypassBlock)
                && e.damageColorIndex == DamageColorIndex.Void
                && e.attacker == null);
            // Could use position to differentiate void fog instances?
        }

        public static void GetAttackerInfo(GameObject attacker, bool isFallDamage, bool isVoidFogDamage, out string name, out Texture portrait, out Color color)
        {
            name = Language.GetString("UNIDENTIFIED_KILLER_NAME");
            portrait = PlanetPortrait;
            color = Color.white;

            if (attacker != null) {
                string attackerName = Util.GetBestBodyName(attacker);
                Texture attackerPortrait = attacker.GetComponent<CharacterBody>()?.portraitIcon;

                if (!string.IsNullOrEmpty(attackerName)) name = attackerName;
                if (attackerPortrait == null) attackerPortrait = GetAlternativePortrait(attackerName, ref color, attacker);
                if (attackerPortrait != null) portrait = attackerPortrait;
            }
            else if (isFallDamage) {
                name = "The Ground";
                portrait = RoR2Content.Artifacts.weakAssKneesArtifactDef.smallIconSelectedSprite.texture;
            }
            else if (isVoidFogDamage) {
                name = "Void Fog";
                portrait = RoR2Content.Buffs.VoidFogMild.iconSprite.texture;
                color = DamageColor.FindColor(DamageColorIndex.Void);
            }
        }

        public static Texture GetAlternativePortrait(string attackerName, ref Color color, GameObject attacker = null)
        {
            if (attackerName == Language.GetString("SHRINE_BLOOD_NAME")) {
                color = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Blood);
                return attacker?.GetComponent<ShrineBloodBehavior>()?.symbolTransform?.GetComponent<MeshRenderer>()?.material?.mainTexture;
            }
            if (attackerName == Language.GetString("VOID_CHEST_NAME")
             || attackerName == Language.GetString("VOID_TRIPLE_NAME")) {
                color = DamageColor.FindColor(DamageColorIndex.Void);
                return DLC1Content.Buffs.EliteVoid.iconSprite.texture;
            }
            if (attackerName == Language.GetString("ARTIFACTSHELL_BODY_NAME"))
                return RoR2Content.Items.ArtifactKey.pickupIconSprite.texture;
            return null;
        }

        public static void GetEliteIcon(CharacterBody body, out Sprite icon, out Color color)
        {
            BuffDef buff = GetEliteBuffDef(body);
            icon = buff?.iconSprite;
            color = buff?.buffColor ?? Color.white;
        }

        public static BuffDef GetEliteBuffDef(CharacterBody body)
        {
            BuffDef def = null;
            if (body == null || !body.isElite) return def;

            // Logic from RoR2.Util.GetBestBodyName()
            BuffIndex[] eliteBuffIndices = BuffCatalog.eliteBuffIndices;
            foreach (BuffIndex buffIndex in eliteBuffIndices) {
                if (body.HasBuff(buffIndex)) {
                    def = BuffCatalog.GetBuffDef(buffIndex);
                }
            }

            return def;
        }

        public static string GenerateIdentifier(GameObject attacker, bool isFallDamage, bool isVoidFogDamage)
        {
            string identifier = "??";
            if (attacker != null) identifier = attacker.GetInstanceID().ToString();
            if (isFallDamage) identifier = "fall_damage";
            if (isVoidFogDamage) identifier = "void_fog_damage";

            // Include name to differentiate when an attacker becomes elite (e.g. Voidtouched)
            if (attacker != null) {
                GetAttackerInfo(attacker, isFallDamage, isVoidFogDamage, out string name, out _, out _);
                identifier += '·' + name;
            }

            return identifier;
        }
    }
}
