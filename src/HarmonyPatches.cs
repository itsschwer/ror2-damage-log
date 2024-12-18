using HarmonyLib;

namespace DamageLog
{
    [HarmonyPatch]
    internal static class HarmonyPatches
    {
        [HarmonyPostfix, HarmonyPatch(typeof(RoR2.UI.GameEndReportPanelController), nameof(RoR2.UI.GameEndReportPanelController.Awake))]
        private static void GameEndReportPanelController_Awake(RoR2.UI.GameEndReportPanelController __instance) {
            try { DamageLogUI.MoveToGameEndReportPanel(__instance); }
            catch (System.Exception e) { Plugin.Logger.LogError(e); }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RoR2.UI.GameEndReportPanelController), nameof(RoR2.UI.GameEndReportPanelController.SetPlayerInfo))]
        private static void GameEndReportPanelController_SetPlayerInfo(RoR2.RunReport.PlayerInfo playerInfo) {
            try { DamageLogUI.DisplayPlayerDamageLog(playerInfo?.networkUser); }
            catch (System.Exception e) { Plugin.Logger.LogError(e); }
        }




        [HarmonyPostfix, HarmonyPatch(typeof(RoR2.BossGroup), nameof(RoR2.BossGroup.OnMemberDiscovered))]
        private static void BossGroup_OnMemberDiscovered(RoR2.BossGroup __instance, RoR2.CharacterMaster memberMaster) {
            try { Plugin.TrackBoss(__instance, memberMaster); }
            catch (System.Exception e) { Plugin.Logger.LogError(e); }
        }
    }
}
