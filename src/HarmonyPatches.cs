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
    }
}
