using BepInEx;
using HarmonyLib;
using RoR2;
using RoR2.UI;

namespace DamageLog
{
    [BepInPlugin(GUID, Name, Version)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string GUID = Author + "." + Name;
        public const string Author = "itsschwer";
        public const string Name = "DamageLog";
        public const string Version = "0.1.0";

        internal static new Config Config { get; private set; }
        internal static void ReloadConfig() => RequestConfigReload?.Invoke();
        private static System.Action RequestConfigReload;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity Message")]
        private void Awake()
        {
            Log.Init(Logger);
            Config = new Config(base.Config);
            new Harmony(Info.Metadata.GUID).PatchAll();
            RequestConfigReload = base.Config.Reload;
            Log.Message($"{Plugin.GUID}> awake.");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity Message")]
        private void OnEnable()
        {
            CharacterBody.onBodyStartGlobal += TrackBody;
            HUD.shouldHudDisplay += DamageLogUI.Init;
            Log.Message($"{Plugin.GUID}> enabled.");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity Message")]
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




#if DEBUG
        private int idx;
        private float cd;
        private void Update()
        {
            cd -= UnityEngine.Time.deltaTime;
            if (cd < 0 && UnityEngine.Input.GetKeyDown(Plugin.Config.ChangeStageKey)) {
                ChangeStage();
                cd = 5;
            }
        }

        private void ChangeStage()
        {
            if (!UnityEngine.Networking.NetworkServer.active || !Run.instance) return;
            string[] stages = ["artifactworld", "goolake", "frozenwall", "sulfurpools", "voidstage", "arena" ];
            idx++; if (idx >= stages.Length) idx = 0;
            Run.instance.GenerateStageRNG();
            UnityEngine.Networking.NetworkManager.singleton.ServerChangeScene(stages[idx]);
        }
#endif
    }
}
