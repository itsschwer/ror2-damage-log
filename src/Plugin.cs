using BepInEx;
using RoR2;

namespace DamageIndicator
{
    [BepInPlugin(GUID, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = Author + "." + Name;
        public const string Author = "itsschwer";
        public const string Name = "DamageIndicator";
        public const string Version = "0.0.0";

        private void Awake()
        {
            Log.Init(Logger);
            Log.Message($"{Plugin.GUID}> awake.");
        }

        private void OnEnable()
        {
            Log.Message($"{Plugin.GUID}> enabled.");
        }

        private void OnDisable()
        {
            Log.Message($"{Plugin.GUID}> disabled.");
        }
    }
}
