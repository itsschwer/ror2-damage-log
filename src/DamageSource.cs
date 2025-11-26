using RoR2;
using UnityEngine;

namespace DamageLog
{
    public sealed class DamageSource
    {
        private static Texture _PlanetPortrait;
        private static Texture PlanetPortrait => _PlanetPortrait ??= LegacyResourcesAPI.Load<Texture>("Textures/BodyIcons/texUnidentifiedKillerIcon");

        private static Texture _SotVPortrait;
        private static Texture SotVPortrait => _SotVPortrait ??= UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Texture>("RoR2/DLC1/UI/texVoidExpansionIcon.png").WaitForCompletion();

        // texEliteCollectiveSharedIcon.png appears to be the ally cooldown reduction icon, which is a slightly darker blue?
        private static Sprite _CollectiveEliteIcon;
        private static Sprite CollectiveEliteIcon => _CollectiveEliteIcon ??= UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>("RoR2/DLC3/Collective/texEliteCollectiveIcon.png").WaitForCompletion();




        public readonly string identifier;

        public readonly string attackerName;
        public readonly Texture attackerPortrait;
        public readonly Color attackerPortraitColor;
        public readonly Sprite eliteIcon;
        public readonly Color eliteIconColor;

        public readonly bool isPlayerDamage;
        public readonly bool isFallDamage;
        public readonly bool isVoidFogDamage;

        public readonly float timeStart;
#pragma warning disable IDE1006 // Naming rule violation: must begin with upper case character
        public float time { get; private set; }
        public int hits { get; private set; }
        public float totalDamage { get; private set; }
        public float totalDamagePercent { get; private set; }
        public float remainingHpPercent { get; private set; }
#pragma warning restore IDE1006 // Naming rule violation: must begin with upper case character

        public DamageSource(DamageDealtMessage e)
        {
            CharacterBody body = e.attacker?.GetComponent<CharacterBody>();
            isPlayerDamage = body?.isPlayerControlled ?? false;
            isFallDamage = e.IsFallDamage();
            isVoidFogDamage = e.IsVoidFogDamage();

            identifier = e.GenerateIdentifier();
            attackerName = e.GetAttackerName();
            GetAttackerPortrait(e, out attackerPortrait, out attackerPortraitColor);
            GetEliteIcon(body, out eliteIcon, out eliteIconColor);

            timeStart = Time.time;
            time = timeStart;
            hits = 1;
            totalDamage = e.damage;
            UpdateHpDamagePercent(e.victim, e.damage);
        }

        public DamageSource Add(DamageDealtMessage e)
        {
            time = Time.time;
            hits++;
            totalDamage += e.damage;
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
                remainingHpPercent = UnityEngine.Networking.NetworkServer.active ?
                    health.combinedHealthFraction :
                    ((health.combinedHealth - latestHitDamage) / health.fullCombinedHealth);
                totalDamagePercent = totalDamage / health.fullCombinedHealth;
            }
            else Plugin.Logger.LogWarning($"Could not {nameof(UpdateHpDamagePercent)}");
        }




        private static void GetAttackerPortrait(DamageDealtMessage e, out Texture portrait, out Color color)
        {
            portrait = PlanetPortrait;
            color = Color.white;

            if (e.attacker != null) {
                Texture attackerPortrait = e.attacker.GetComponent<CharacterBody>()?.portraitIcon;

                if (attackerPortrait == null || attackerPortrait == PlanetPortrait) attackerPortrait = GetAlternativeAttackerPortrait(e.attacker, e.GetAttackerName(), ref color);
                if (attackerPortrait != null) portrait = attackerPortrait;
            }
            else if (e.IsFallDamage()) {
                portrait = RoR2Content.Artifacts.weakAssKneesArtifactDef.smallIconSelectedSprite.texture;
            }
            else if (e.IsVoidFogDamage()) {
                portrait = RoR2Content.Buffs.VoidFogMild.iconSprite.texture;
                color = DamageColor.FindColor(DamageColorIndex.Void);
            }
#if NETSTANDARD2_1
            else if (e.IsMeridianLightningDamage()) {
                portrait = DLC2Content.Buffs.lunarruin.iconSprite.texture;
            }
#endif
        }

        private static Texture GetAlternativeAttackerPortrait(GameObject attacker, string attackerName, ref Color color)
        {
            if (attackerName == Language.GetString("SHRINE_BLOOD_NAME")) {
                color = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Blood);
                return attacker?.GetComponent<ShrineBloodBehavior>()?.symbolTransform?.GetComponent<MeshRenderer>()?.material?.mainTexture;
            }
            if (attackerName == Language.GetString("VOID_CHEST_NAME")
             || attackerName == Language.GetString("VOID_TRIPLE_NAME")) {
                return SotVPortrait;
            }
            if (attackerName == Language.GetString("ARTIFACTSHELL_BODY_NAME"))
                return RoR2Content.Items.ArtifactKey.pickupIconSprite.texture;
            return null;
        }

        private static void GetEliteIcon(CharacterBody body, out Sprite icon, out Color color)
        {
            BuffDef buff = GetEliteBuffDef(body);
            icon = buff?.iconSprite;
            color = buff?.buffColor ?? Color.white;

            // it appears that the elite buff itself does not have an icon,
            // but the cooldown reduction buff given to allies does;
            // a little bit unintuitive compared to pre-existing elites
            if (icon == null && buff?.eliteDef?.modifierToken == "ELITE_MODIFIER_COLLECTIVE") icon = CollectiveEliteIcon;
            else if (buff?.eliteDef?.modifierToken == "ELITE_MODIFIER_COLLECTIVE") Plugin.Logger.LogWarning($"collective elite buff has been assigned an icon! '{icon}'");
        }

        private static BuffDef GetEliteBuffDef(CharacterBody body)
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
    }
}
