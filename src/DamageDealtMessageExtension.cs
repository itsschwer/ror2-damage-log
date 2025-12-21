using RoR2;

namespace DamageLog
{
    public static class DamageDealtMessageExtension
    {
        public static bool IsFallDamage(this DamageDealtMessage e)
        {
            return (e.damageType & DamageType.FallDamage) != 0;
        }

        public static bool IsVoidFogDamage(this DamageDealtMessage e)
        {
            // RoR2.FogDamageController.FixedUpdate()
            return (e.damageType == (DamageType.BypassArmor | DamageType.BypassBlock)
                 && e.damageColorIndex == DamageColorIndex.Void
                 && e.attacker == null);
        }

#if NETSTANDARD2_1
        public static bool IsMeridianLightningDamage(this DamageDealtMessage e)
        {
            // Extracted using damage identifier
            return (e.damageType == (DamageTypeCombo.Generic | DamageType.AOE | DamageType.LunarRuin | DamageTypeExtended.DamagePercentOfMaxHealth | DamageTypeExtended.ApplyBuffPermanently));
        }

        public static bool IsWarpedEchoDamage(this DamageDealtMessage e)
        {
            // Meridian Lightning overrides Warped Echo's DamageColorIndex.Void...
            return e.damageColorIndex == DamageColorIndex.DelayedDamage;
        }
#endif

        public static string GetAttackerName(this DamageDealtMessage e)
        {
            string name = Language.GetString("UNIDENTIFIED_KILLER_NAME");

            if (e.attacker != null) {
                string attackerName = Util.GetBestBodyName(e.attacker);

                if (!string.IsNullOrEmpty(attackerName)) name = attackerName;
            }
            else if (e.IsFallDamage()) {
                name = "The Ground";
            }
            else if (e.IsVoidFogDamage()) {
                name = "Void Fog";
            }
            else if (e.IsMeridianLightningDamage()) {
                name = "Meridian Lightning"; // Language.GetString("MAP_MERIDIAN_TITLE");
            }
#if NETSTANDARD2_1
            else if (e.IsWarpedEchoDamage()) {
                name = Language.GetString(DLC2Content.Items.DelayedDamage.nameToken);
            }
#endif

            return name;
        }

        public static string GenerateIdentifier(this DamageDealtMessage e)
        {
            string identifier = $"?? | {e.damageType} | {e.damageColorIndex} | {e.damage}";

            if (e.IsFallDamage()) identifier = "fall_damage";
            if (e.IsVoidFogDamage()) identifier = "void_fog_damage"; // Could use e.position (or probably just the instance id) to differentiate void fog instances?
            if (e.attacker != null) identifier = $"{e.attacker.GetInstanceID()} | {e.GetAttackerName()}"; // Include name to differentiate when an attacker becomes elite (e.g. Voidtouched)
                                                                                                          // ^ Is it worth adding a GetComponent (CharacterBody?) call (+ checks?) to replace InstanceId with NetworkInstanceId?
#if NETSTANDARD2_1
            else if (e.IsMeridianLightningDamage()) identifier = "meridian_lightning";
#endif

            return identifier;
        }
    }
}
