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
        public const string Version = "0.2.3";

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
            Log.Message($"~awake.");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity Message")]
        private void OnEnable()
        {
            CharacterBody.onBodyStartGlobal += TrackBody;
            HUD.shouldHudDisplay += DamageLogUI.Init;
            Log.Message($"~enabled.");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity Message")]
        private void OnDisable()
        {
            DamageLog.ClearAll();
            CharacterBody.onBodyStartGlobal -= TrackBody;
            HUD.shouldHudDisplay -= DamageLogUI.Init;
            Log.Message($"~disabled.");
        }

        private static void TrackBody(CharacterBody body)
        {
            if (!body.isPlayerControlled) return;

            new DamageLog(Util.LookUpBodyNetworkUser(body));
        }

        /// <summary>
        /// Backup method for non-hosts.
        /// </summary>
        /// <remarks>
        /// Completely unreliable.
        /// </remarks>
        internal static void TrackNetworkUsers()
        {
            Log.Warning($"TrackNetworkUsers {NetworkUser.readOnlyInstancesList.Count}");
            if (UnityEngine.Networking.NetworkServer.active) return;

            for (int i = 0; i < NetworkUser.readOnlyInstancesList.Count; i++) {
                new DamageLog(NetworkUser.readOnlyInstancesList[i]);
            }
            Log.Warning("TrackNetworkUsers end");
        }




#if DEBUG
        private float cd;
        private void Update()
        {
            if (!UnityEngine.Networking.NetworkServer.active || !Run.instance) return;

            cd -= UnityEngine.Time.deltaTime;
            if (UnityEngine.Input.GetKeyDown(Plugin.Config.ChangeStageKey)) {
                NetworkUser user = FindObjectOfType<DamageLogUI>()?.user ?? LocalUserManager.GetFirstLocalUser().currentNetworkUser;
                bool ctrlKey = UnityEngine.Input.GetKey("left ctrl") || UnityEngine.Input.GetKey("right ctrl");
                bool shiftKey = UnityEngine.Input.GetKey("left shift") || UnityEngine.Input.GetKey("right shift");
                if (ctrlKey && shiftKey) GiveItems(user);
                else if (ctrlKey) Debug.GiveItem(user, RoR2Content.Items.ExtraLife);
                else if (shiftKey) Debug.SpawnInteractable(user);
                else if (cd < 0) { cd = 4; Debug.ChangeStage(); }
            }
        }

        private static void GiveItems(NetworkUser user)
        {
            Debug.GiveItem(user, RoR2Content.Items.Medkit, 20);
            Debug.GiveItem(user, RoR2Content.Items.FallBoots, 100);
            Debug.GiveItem(user, RoR2Content.Items.SprintBonus, 8);
        }
#endif
    }
}
