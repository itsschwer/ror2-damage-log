using HarmonyLib;

namespace DamageLog
{
    [HarmonyPatch]
    internal static class HarmonyPatches
    {
        [HarmonyPostfix, HarmonyPatch(typeof(RoR2.UI.GameEndReportPanelController), nameof(RoR2.UI.GameEndReportPanelController.Awake))]
        private static void GameEndReportPanelController_Awake(RoR2.UI.GameEndReportPanelController __instance)
            => DamageLogUI.MoveToGameEndReportPanel(__instance);

        [HarmonyPostfix, HarmonyPatch(typeof(RoR2.UI.GameEndReportPanelController), nameof(RoR2.UI.GameEndReportPanelController.SetPlayerInfo))]
        private static void GameEndReportPanelController_SetPlayerInfo(RoR2.RunReport.PlayerInfo playerInfo)
            => DamageLogUI.DisplayPlayerDamageLog(playerInfo?.networkUser);




        [HarmonyPostfix, HarmonyPatch(typeof(RoR2.BossGroup), nameof(RoR2.BossGroup.OnMemberDiscovered))]
        private static void BossGroup_OnMemberDiscovered(RoR2.BossGroup __instance, RoR2.CharacterMaster memberMaster)
            => Plugin.TrackBoss(__instance, memberMaster);
    }
}
