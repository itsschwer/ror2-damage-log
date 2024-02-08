using BepInEx;
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
        public const string Version = "1.0.0";

        internal static new Config Config { get; private set; }
        internal static void ReloadConfig() => RequestConfigReload?.Invoke();
        private static System.Action RequestConfigReload;

        internal static Data Data { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity Message")]
        private void Awake()
        {
            Log.Init(Logger);
            Config = new Config(base.Config);
            Data = new Data();
            new HarmonyLib.Harmony(Info.Metadata.GUID).PatchAll();
            RequestConfigReload = base.Config.Reload;
            Log.Message($"~awake.");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity Message")]
        private void OnEnable()
        {
            Run.onRunStartGlobal += OnRunStartOrDestroy;
            Run.onRunDestroyGlobal += OnRunStartOrDestroy;
            CharacterBody.onBodyStartGlobal += TrackBody;
            HUD.shouldHudDisplay += DamageLogUI.Init;
            Stage.onStageStartGlobal += OnStageStart;
            Log.Message($"~enabled.");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity Message")]
        private void OnDisable()
        {
            Run.onRunStartGlobal -= OnRunStartOrDestroy;
            Run.onRunDestroyGlobal -= OnRunStartOrDestroy;
            CharacterBody.onBodyStartGlobal -= TrackBody;
            HUD.shouldHudDisplay -= DamageLogUI.Init;
            Stage.onStageStartGlobal -= OnStageStart;
            Log.Message($"~disabled.");
        }

        private static void OnRunStartOrDestroy(Run _)
            => Data.ClearAll();
        private static void OnStageStart(Stage _)
            => Data.ClearBossLogs();


        private static void TrackBody(CharacterBody body)
        {
            if (body.isPlayerControlled) {
                new DamageLog(Util.LookUpBodyNetworkUser(body), body);
            }
            // Tracking bosses for clients...
            else if (Config.TrackBosses && body.isBoss) {
                Log.Debug($"{nameof(TrackBody)}> Found {body.name}");
                new DamageLog(body);
            }
        }

        internal static void TrackBoss(BossGroup boss, CharacterMaster member)
        {
            if (!Config.TrackBosses) return;
            CharacterBody body = member.GetBody();
            if (body != null) {
                Log.Debug($"{nameof(TrackBoss)}> Discovered and found {member.name} | {body.name} | {boss.name}");
                new DamageLog(body);
            }
            // Tracking bosses for clients...
            else {
                Log.Debug($"{nameof(TrackBoss)}> Discovered {member.name} | {boss.name}");
                member.onBodyStart += TrackBoss;
            }
        }

        private static void TrackBoss(CharacterBody body)
        {
            // Tracking bosses for clients...
            body.master.onBodyStart -= TrackBoss;
            Log.Debug($"{nameof(TrackBoss)}> Found {body.master.name} | {body.name}");

            new DamageLog(body);
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
            Debug.GiveItem(user, RoR2Content.Items.AlienHead, 10);
            Debug.GiveItem(user, RoR2Content.Items.SprintBonus, 10);
        }
#endif
    }
}
