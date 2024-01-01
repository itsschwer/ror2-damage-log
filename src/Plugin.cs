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

            new DamageLog(Util.LookUpBodyNetworkUser(body));
        }




#if DEBUG
        private int idx;
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
                else if (ctrlKey) GiveItem(user, RoR2Content.Items.ExtraLife);
                else if (shiftKey) SpawnRandomInteractable(user);
                else if (cd < 0) {
                    cd = 4;
                    ChangeStage();
                }
            }
        }

        private void ChangeStage()
        {
            string[] stages = [ "artifactworld", "goolake", "frozenwall", "sulfurpools", "voidstage", "arena" ];
            idx++; if (idx >= stages.Length) idx = 0;
            Run.instance.GenerateStageRNG();
            UnityEngine.Networking.NetworkManager.singleton.ServerChangeScene(stages[idx]);
        }

        private void GiveItems(NetworkUser user)
        {
            if (user?.master?.inventory == null) return;

            user.master.inventory.GiveItem(RoR2Content.Items.Medkit, 20);
            user.master.inventory.GiveItem(RoR2Content.Items.FallBoots, 100);
            user.master.inventory.GiveItem(RoR2Content.Items.SprintBonus, 8);
        }

        private void GiveItem(NetworkUser user, ItemDef item)
        {
            if (user?.master?.inventory == null) return;
            user.master.inventory.GiveItem(item, 1);
        }

        private void SpawnRandomInteractable(NetworkUser user)
        {
            if (user?.GetCurrentBody() == null) return;

            Director.Interactable interactable = (Director.Interactable)UnityEngine.Random.Range((int)Director.Interactable.ShrineBlood, (int)Director.Interactable.VoidTriple + 1);
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.B)) interactable = Director.Interactable.ShrineBlood;
            else if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.V)) interactable = Director.Interactable.VoidChest;
            else if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.P)) interactable = Director.Interactable.VoidTriple;

            Director.SpawnInteractable(interactable, user.GetCurrentBody());
        }
#endif
    }
}
