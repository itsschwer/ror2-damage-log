using HarmonyLib;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DamageLog
{
    [HarmonyPatch]
    public class DamageLogUI : MonoBehaviour
    {
        private static HUD hud;

        public static void Init(HUD hud, ref bool shouldDisplay)
        {
            if (DamageLogUI.hud != null) return;

            hud.gameObject.AddComponent<DamageLogUI>();
            DamageLogUI.hud = hud;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameEndReportPanelController), nameof(GameEndReportPanelController.Awake))]
        private static void OnGameEndPanel(GameEndReportPanelController __instance)
        {
            DamageLogUI ui = hud.gameObject.GetComponent<DamageLogUI>();
            if (ui?.gameObject != null) {
                ui.gameObject.transform.SetParent(__instance.transform);
                ui.enabled = false;
                ui.canvas.enabled = true;
                Log.Debug($"{Plugin.GUID}> moved canvas.");
            }
            else {
                Log.Warning($"{Plugin.GUID}> failed to move canvas (missing).");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameEndReportPanelController), nameof(GameEndReportPanelController.SetPlayerInfo))]
        private static void OnGameEndSetPlayerInfo(RunReport.PlayerInfo playerInfo)
        {
            if (playerInfo?.networkUser == null) return;
            DamageLogUI ui = hud.gameObject.GetComponent<DamageLogUI>();
            if (ui == null) return;

            if (DamageLog.Logs.TryGetValue(playerInfo.networkUser, out DamageLog log)) ui.text.SetText(GenerateTextLog(log));
        }

        private new GameObject gameObject;
        private Canvas canvas;
        private HGTextMeshProUGUI text;
#if DEBUG
        private TooltipProvider tooltip;
#endif

        /// <summary>
        /// Awake() is too early for accessing hud members.
        /// </summary>
        private void Start() => CreateUI(hud.mainContainer);

        private void CreateUI(GameObject parent)
        {
            Plugin.ReloadConfig();
            CreateCanvas(parent);
            CreateText();
            Log.Debug($"{Plugin.GUID}> created canvas.");
        }

        private void CreateCanvas(GameObject parent)
        {
            gameObject = new GameObject("DamageLogUI", typeof(Canvas), typeof(GraphicRaycaster));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent.transform);
            ResetRectTransform(rect);

            AnchorStretchRight(rect);
            rect.pivot = Vector2.one;

            Vector2 offsetTopRight = new Vector2(4, 12);
            rect.localPosition -= (Vector3)offsetTopRight;
            rect.sizeDelta = new Vector2(110, 0) - offsetTopRight;

            canvas = gameObject.GetComponent<Canvas>();
        }

        private void CreateText()
        {
            GameObject obj = new GameObject("DamageLogText", typeof(RectTransform));
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(gameObject.transform);
            ResetRectTransform(rect);

            AnchorTopStretch(rect);
            rect.pivot = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            text = obj.AddComponent<HGTextMeshProUGUI>();
            text.fontSize = 12;
            text.SetText("Damage Log");
#if DEBUG
            tooltip = obj.AddComponent<TooltipProvider>();
#endif
        }

        private void Update()
        {
            if (!TryGetDamageLog(out DamageLog log)) return;

            // RoR2UI.HUD.Update()
            bool visible = !Plugin.Config.OnlyShowWithScoreboard || (hud.localUserViewer?.inputPlayer != null && hud.localUserViewer.inputPlayer.GetButton("info"));
            canvas.enabled = visible;
            if (!visible) return;

            text.SetText(GenerateTextLog(log));
            Vector2 size = text.rectTransform.sizeDelta;
            if (size.y != text.preferredHeight) text.rectTransform.sizeDelta = new Vector2(size.x, text.preferredHeight);
#if DEBUG
            var entries = log.GetEntries();
            if (entries.Count > 0) {
                var src = entries[0];
                tooltip.titleColor = src.isPlayerDamage ? ColorCatalog.GetColor(ColorCatalog.ColorIndex.HardDifficulty)
                                   : src.isFallDamage ? ColorCatalog.GetColor(ColorCatalog.ColorIndex.NormalDifficulty)
                                   : src.isVoidFogDamage ? ColorCatalog.GetColor(ColorCatalog.ColorIndex.VoidItem)
                                   : ColorCatalog.GetColor(ColorCatalog.ColorIndex.Tier3ItemDark);
                tooltip.titleToken = src.attackerName;
                tooltip.bodyToken = GenerateTooltipString(src);
            }
#endif
        }

        private static bool TryGetDamageLog(out DamageLog value)
        {
            NetworkUser user = hud?.localUserViewer?.currentNetworkUser;
            if (user == null) { value = null; return false; }
            return DamageLog.Logs.TryGetValue(user, out value);
        }

        private static string GenerateTextLog(DamageLog log)
        {
            System.Text.StringBuilder sb = new();
            sb.AppendLine($"<style=cWorldEvent>Damage Log <{log.user.masterController.GetDisplayName()}></style>");

            int i = -1; // incremented before check
            float endTime = (log.timeOfDeath > 0) ? log.timeOfDeath : Time.time;
            foreach (DamageLog.DamageSource src in log.GetEntries()) {
                i++;
                if (log.TryPrune(src, endTime, i)) continue;

                string style = src.isPlayerDamage ? "cDeath" : src.isFallDamage ? "cHumanObjective" : src.isVoidFogDamage ? "cIsVoid" : "";
                if (string.IsNullOrEmpty(style)) sb.Append(src.attackerName);
                else sb.Append($"<style={style}>{src.attackerName}</style>");

                bool singleHit = (src.hits == 1);
                if (!singleHit) sb.Append($"<style=cStack>×{src.hits}</style>");
                sb.Append($" · <style=cIsHealth>-{src.damagePercent:0.0%}</style>");
                if (singleHit) sb.Append($" <style=cEvent>({src.hpPercent:0.0%})</style>");

                sb.AppendLine($" · <style=cSub>{(endTime - src.time):0.00s}</style>");
            }

            return sb.ToString();
        }

        private static string GenerateTooltipString(DamageLog.DamageSource src)
        {
            System.Text.StringBuilder sb = new();

            sb.Append($"Dealt <style=cIsHealth>{src.damage:0.0}</style> damage");
            if (src.hits == 1) sb.Append($" <style=cEvent>({src.hpPercent:0.0%} health remaining)</style>");
            else sb.Append($" in <style=cStack>{src.hits} hits</style> over <style=cSub>{(src.time - src.timeStart):0.00s}</style>");
            sb.AppendLine(".");
#if DEBUG
            sb.AppendLine();
            sb.AppendLine($"<style=cIsDamage>{src.identifier}</style>");
#endif
            return sb.ToString();
        }

        /// <summary>
        /// Resets the RectTransform's local transform and sets its layer to "UI".
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static RectTransform ResetRectTransform(RectTransform rect)
        {
            rect.localPosition = Vector3.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.gameObject.layer = LayerMask.NameToLayer("UI");
            return rect;
        }

        public static RectTransform AnchorStretchRight(RectTransform rect)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.anchorMin = new Vector2(1, 0);
            rect.anchorMax = Vector2.one;
            return rect;
        }

        public static RectTransform AnchorTopStretch(RectTransform rect)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = Vector2.one;
            return rect;
        }

        public static RectTransform AnchorStretchStretch(RectTransform rect)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            return rect;
        }
    }
}
