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
        public const string Version = "1.2.7";

        internal static new BepInEx.Logging.ManualLogSource Logger { get; private set; }

        internal static new Config Config { get; private set; }

        internal static Data Data { get; private set; }

        private void Awake()
        {
            // Use Plugin.GUID instead of Plugin.Name as source name
            BepInEx.Logging.Logger.Sources.Remove(base.Logger);
            Logger = BepInEx.Logging.Logger.CreateLogSource(Plugin.GUID);

            Config = new Config(base.Config);
            Data = new Data();

            if (Compatibility.DamageInfoChanged()) { DestroyImmediate(this); return; }

            new HarmonyLib.Harmony(Info.Metadata.GUID).PatchAll();

            Logger.LogMessage("~awake.");
        }

        private void OnEnable()
        {
            HUD.shouldHudDisplay += DamageLogUI.Instantiate;
            CharacterBody.onBodyStartGlobal += TrackUser;
            Run.onRunStartGlobal += OnRunStartOrDestroy;
            Run.onRunDestroyGlobal += OnRunStartOrDestroy;
            Stage.onStageStartGlobal += OnStageStart;

            Logger.LogMessage("~enabled.");
        }

        private void OnDisable()
        {
            HUD.shouldHudDisplay -= DamageLogUI.Instantiate;
            CharacterBody.onBodyStartGlobal -= TrackUser;
            Run.onRunStartGlobal -= OnRunStartOrDestroy;
            Run.onRunDestroyGlobal -= OnRunStartOrDestroy;
            Stage.onStageStartGlobal -= OnStageStart;

            Logger.LogMessage("~disabled.");
        }

        private static void OnRunStartOrDestroy(Run _) => Data.ClearAll();
        private static void OnStageStart(Stage _)      => Data.ClearBossLogs();


        private static void TrackUser(CharacterBody body)
        {
            if (!body.isPlayerControlled) return;
            NetworkUser user = Util.LookUpBodyNetworkUser(body);
            if (user == null) return;

            DamageLog log = new DamageLog(user, body);
            Data.AddUserLog(user, log);
        }

        internal static void TrackBoss(BossGroup boss, CharacterMaster member)
        {
            if (!Config.TrackBosses) return;

            /* May be unreliable on clients?
             * Not all discovered bodies could be found/tracked when testing as client,
             * but when testing as host, other clients seemed fine?
             * ∴ May be affected by host connection/performance/other mods?
             */

            Logger.LogDebug($"{nameof(TrackBoss)}> Discovered {member.name} <{member.netId.Value:x8}> | {boss.name}");
            member.onBodyStart += TrackBoss;
        }

        private static void TrackBoss(CharacterBody body)
        {
            body.master.onBodyStart -= TrackBoss;
            if (BossDamageLog.IsIgnoredBossSubtitle(body.subtitleNameToken)) return;

            Logger.LogDebug($"{nameof(TrackBoss)}> Found {body.master.name} <{body.master.netId.Value:x8}> | {body.name} <{body.netId.Value:x8}>");
            Data.AddBossLog(new BossDamageLog(body));
        }
    }
}
