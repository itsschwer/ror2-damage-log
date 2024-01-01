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
        public readonly string attackerName;
        public readonly Sprite eliteIcon;

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
            isPlayerDamage = e.attacker?.GetComponent<CharacterBody>()?.isPlayerControlled ?? false;
            isFallDamage = IsFallDamage(e);
            isVoidFogDamage = IsVoidFogDamage(e);

            identifier = GenerateIdentifier(e.attacker, isFallDamage, isVoidFogDamage);
            GetAttackerNameAndPortrait(e.attacker, isFallDamage, isVoidFogDamage, out attackerName, out attackerPortrait);
            eliteIcon = GetEliteIcon(e.attacker?.GetComponent<CharacterBody>());

            timeStart = Time.time;
            time = timeStart;
            hits = 1;
            damage = e.damage;
            UpdateHpDamagePercent(e.victim);
        }

        public DamageSource Add(DamageDealtMessage e)
        {
            time = Time.time;
            hits++;
            damage += e.damage;
            UpdateHpDamagePercent(e.victim);
            return this;
        }

        private void UpdateHpDamagePercent(GameObject victim)
        {
            HealthComponent health = victim?.GetComponent<HealthComponent>();
            if (health != null) {
                hpPercent = health.combinedHealthFraction;
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

        public static void GetAttackerNameAndPortrait(GameObject attacker, bool isFallDamage, bool isVoidFogDamage, out string name, out Texture portrait)
        {
            name = Language.GetString("UNIDENTIFIED_KILLER_NAME");
            portrait = PlanetPortrait;

            if (attacker != null) {
                string attackerName = Util.GetBestBodyName(attacker);
                Texture attackerPortrait = attacker.GetComponent<CharacterBody>()?.portraitIcon;

                if (!string.IsNullOrEmpty(attackerName)) name = attackerName;
                if (attackerPortrait == null) attackerPortrait = GetAlternativePortrait(attackerName);
                if (attackerPortrait != null) portrait = attackerPortrait;
            }
            else if (isFallDamage) {
                name = "The Ground";
                portrait = RoR2Content.Artifacts.weakAssKneesArtifactDef.smallIconSelectedSprite.texture;
            }
            else if (isVoidFogDamage) {
                name = "Void Fog";
                portrait = RoR2Content.Buffs.VoidFogMild.iconSprite.texture;
            }
        }

        public static Texture GetAlternativePortrait(string attackerName)
        {
            if (attackerName == Language.GetString("SHRINE_BLOOD_NAME"))
                return null; // shrine of blood icon
            if (attackerName == Language.GetString("VOID_CHEST_NAME")
             || attackerName == Language.GetString("VOID_TRIPLE_NAME"))
                return DLC1Content.Buffs.EliteVoid.iconSprite.texture;
            if (attackerName == Language.GetString("POT2_BODY_NAME"))
                return RoR2Content.Buffs.ClayGoo.iconSprite.texture;
            if (attackerName == Language.GetString("SULFURPOD_BODY_NAME"))
                return RoR2Content.Buffs.Poisoned.iconSprite.texture;
            if (attackerName == Language.GetString("FUSIONCELL_BODY_NAME"))
                return null; // question mark
            if (attackerName == Language.GetString("ARTIFACTSHELL_BODY_NAME"))
                return RoR2Content.Items.ArtifactKey.pickupIconSprite.texture;
            return null;
        }

        public static Sprite GetEliteIcon(CharacterBody body) => GetEliteBuffDef(body)?.iconSprite;
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
                GetAttackerNameAndPortrait(attacker, isFallDamage, isVoidFogDamage, out string name, out _);
                identifier += '·' + name;
            }

            return identifier;
        }
    }
}
