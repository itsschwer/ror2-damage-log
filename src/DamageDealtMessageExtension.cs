﻿using RoR2;

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

            return name;
        }

        public static string GenerateIdentifier(this DamageDealtMessage e)
        {
            string identifier = "??";

            if (e.attacker != null) identifier = e.attacker.GetInstanceID().ToString(); // Replace with NetworkInstanceId?
            if (e.IsFallDamage()) identifier = "fall_damage";
            if (e.IsVoidFogDamage()) identifier = "void_fog_damage";

            // Include name to differentiate when an attacker becomes elite (e.g. Voidtouched)
            if (e.attacker != null)
            {
                identifier += '·' + e.GetAttackerName();
            }

            // Could use e.position to differentiate void fog instances?

            return identifier;
        }
    }
}