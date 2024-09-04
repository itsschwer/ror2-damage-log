using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DamageLog
{
    internal static class Compatibility
    {
        internal static string ApplicationVersion => $"RoR2v{UnityEngine.Application.version} ({UnityEngine.Application.buildGUID})";
        internal static string PluginVersion => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        internal static bool DamageInfoChanged()
        {
            try {
                DamageInfoChanged_Test();
                return false;
            }
            catch (MissingFieldException) {
                Plugin.Logger.LogError($"{Plugin.Name}v{PluginVersion} is not compatible with {ApplicationVersion}\nPlease refer to the README on the mod page for guidance.");
                return true;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void DamageInfoChanged_Test()
        {
            RoR2.DamageInfo d = new();
            d.damageType.ToString();
        }
    }
}
