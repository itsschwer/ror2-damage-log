using BepInEx;
using HarmonyLib;
using RoR2;
using RoR2.UI;

namespace DamageLog
{
    [BepInPlugin(GUID, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = Author + "." + Name;
        public const string Author = "itsschwer";
        public const string Name = "DamageLog";
        public const string Version = "0.0.0";

        internal static new Config Config { get; private set; }
        internal static void ReloadConfig() => RequestConfigReload?.Invoke();
        private static System.Action RequestConfigReload;

        private void Awake()
        {
            Log.Init(Logger);
            Config = new Config(base.Config);
            new Harmony(Info.Metadata.GUID).PatchAll();
            RequestConfigReload = base.Config.Reload;
            Log.Message($"{Plugin.GUID}> awake.");
        }

        private void OnEnable()
        {
            CharacterBody.onBodyStartGlobal += TrackBody;
            HUD.shouldHudDisplay += DamageLogUI.Init;
            Log.Message($"{Plugin.GUID}> enabled.");
        }

        private void OnDisable()
        {
            DamageLog.ClearAll();
            CharacterBody.onBodyStartGlobal -= TrackBody;
            HUD.shouldHudDisplay -= DamageLogUI.Init;
            Log.Message($"{Plugin.GUID}> disabled.");
        }

        private static void TrackBody(CharacterBody body)
        {
            if (!body.isPlayerControlled) return;

            new DamageLog(FindBodyUser(body));
        }

        public static NetworkUser FindBodyUser(CharacterBody body)
        {
            if (body == null) return null;

            foreach (NetworkUser user in NetworkUser.readOnlyInstancesList) {
                if (user.GetCurrentBody() == body) return user;
            }

            return null;
        }
    }
}
