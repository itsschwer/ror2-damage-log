using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace DamageLog
{
    internal sealed class DamageLogUI : MonoBehaviour
    {
        private static HUD hud;

        /// <remarks>
        /// Subscribes to <see cref="HUD.shouldHudDisplay"/>, which is invoked in <see cref="HUD.UpdateHudVisibility"/>, which is called in <see cref="HUD.Update"/> (i.e. every frame).
        /// Therefore, a guard clause is used to prevent multiple <see cref="DamageLogUI"/> components being added to the same <see cref="HUD"/> instance.
        /// Functionally, this instantiates a <see cref="DamageLogUI"/> once at the start of each stage.
        /// </remarks>
        /// <param name="hud"></param>
        /// <param name="_"></param>
        public static void Instantiate(HUD hud, ref bool _)
        {
            if (DamageLogUI.hud != null) return;

            Plugin.Logger.LogDebug("Adding to HUD.");
            hud.gameObject.AddComponent<DamageLogUI>();
            DamageLogUI.hud = hud;
        }

        internal static void MoveToGameEndReportPanel(GameEndReportPanelController panel)
        {
            if (hud == null) {
                Plugin.Logger.LogWarning("Failed to move canvas (no HUD). This can safely be ignored if triggered by viewing Game End Report from Run History.");
                return;
            }

            DamageLogUI ui = hud.gameObject.GetComponent<DamageLogUI>();
            if (ui == null) {
                Plugin.Logger.LogWarning("Failed to move canvas (missing).");
                return;
            }

            ui.gameObject.transform.SetParent(panel.transform);
            ui.enabled = false;
            ui.canvas.enabled = true;
            Plugin.Logger.LogDebug("Moved canvas.");
        }

        internal static void DisplayPlayerDamageLog(NetworkUser user)
        {
            if (hud == null) {
                Plugin.Logger.LogWarning("Failed to update canvas (no HUD). This can safely be ignored if triggered by viewing Game End Report from Run History.");
                return;
            }

            DamageLogUI ui = hud.gameObject.GetComponent<DamageLogUI>();
            if (ui == null) {
                Plugin.Logger.LogWarning("Failed to update canvas (missing).");
                return;
            }

            if (!Plugin.Data.TryGetUserLog(user, out DamageLog log)) {
                Plugin.Logger.LogWarning($"Failed to find damage log for {user.userName}.");
                return;
            }

            ui.UpdateText(log);
            ui.UpdatePortraits(log);
        }




#pragma warning disable IDE1006 // Naming rule violation: must begin with upper case character
        public NetworkUser user { get; private set; }
#pragma warning restore IDE1006 // Naming rule violation: must begin with upper case character
        private int bossIndex = 0;
        private bool showingBoss = false;

        private new GameObject gameObject;
        private Canvas canvas;
        private HGTextMeshProUGUI text;
        private readonly List<DamageSourceUI> uiEntries = [];

        /// <remarks>
        /// Awake() is too early for accessing hud members.
        /// </remarks>
        private void Start() => CreateUI(hud.mainContainer);

        private void CreateUI(GameObject parent)
        {
            Plugin.Config.Reload();
            user = hud?.localUserViewer?.currentNetworkUser;
            if (user == null) Plugin.Logger.LogWarning("Failed to get HUD user (null).");
            CreateCanvas(parent);
            CreateText();
            if (!Plugin.Config.SimpleTextMode) CreateLayout();
            Plugin.Logger.LogDebug("Created canvas.");
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
            rect.sizeDelta = new Vector2(92, 0) - offsetTopRight;

            canvas = gameObject.GetComponent<Canvas>();
        }

        private void CreateLayout()
        {
            VerticalLayoutGroup layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childForceExpandHeight = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childControlWidth = false;
            layout.spacing = Plugin.Config.Spacing;

            for (int i = 0; i < Plugin.Config.MaximumPortraitCount; i++) {
                uiEntries.Add(DamageSourceUI.Create((RectTransform)gameObject.transform).Hide());
            }
        }

        private void CreateText()
        {
            GameObject obj = new GameObject("DamageLogText", typeof(RectTransform));
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(gameObject.transform);
            ResetRectTransform(rect);

            AnchorTopStretch(rect);
            rect.pivot = Vector2.one;
            rect.sizeDelta = Plugin.Config.SimpleTextMode ? Vector2.zero : new Vector2(((RectTransform)gameObject.transform).sizeDelta.x, 0);

            text = obj.AddComponent<HGTextMeshProUGUI>();
            text.fontSize = Plugin.Config.TextSize;
            text.SetText("<style=cDeath>Damage Log <null></style>");
        }




        /// <remarks>
        /// This component is disabled on game end.
        /// Call <see cref="UpdateText"/> and <see cref="UpdatePortraits"/>
        /// manually if the display needs to be updated.
        /// </remarks>
        private void Update()
        {
            // Scoreboard visibility logic from RoR2.UI.HUD.Update()
            bool visible = !Plugin.Config.OnlyShowWithScoreboard || (hud.localUserViewer?.inputPlayer != null && hud.localUserViewer.inputPlayer.GetButton("info"));
            canvas.enabled = visible;
            if (!visible) return;

            bool shiftKey = Input.GetKey("left shift") || Input.GetKey("right shift");
            bool cycleUser = Input.GetKeyDown(Plugin.Config.CycleUserKey);
            bool cycleBoss = Plugin.Config.TrackBosses && Input.GetKeyDown(Plugin.Config.CycleBossKey);

            if (cycleUser) {
                if (!showingBoss) user = Plugin.Data.CycleUser(user, shiftKey);
                showingBoss = false;
            }
            else if (cycleBoss && Plugin.Data.HasBossLogs) {
                if (showingBoss) bossIndex = Plugin.Data.CycleBossIndex(bossIndex, shiftKey);
                showingBoss = true;
            }

            if ((!showingBoss && user != null && Plugin.Data.TryGetUserLog(user, out DamageLog log))
              || (showingBoss && Plugin.Data.TryGetBossLog(bossIndex, out log))) {
                UpdateText(log);
                UpdatePortraits(log);
            }
        }

        private void UpdateText(DamageLog log)
        {
            text.SetText(GenerateTextLog(log));
            Vector2 size = text.rectTransform.sizeDelta;
            if (size.y != text.preferredHeight) text.rectTransform.sizeDelta = new Vector2(size.x, text.preferredHeight);
        }

        private void UpdatePortraits(DamageLog log)
        {
            if (Plugin.Config.SimpleTextMode) return;

            float now = log.Time;
            List<DamageSource> entries = log.GetEntries();
            for (int i = 0; i < uiEntries.Count; i++) {
                if (i >= entries.Count) { uiEntries[i].Hide(); continue; }

                float elapsedTime = now - entries[i].time;
                if (log.IsExpired(elapsedTime)) uiEntries[i].Hide();
                else uiEntries[i].Display(entries[i], elapsedTime);
            }
        }

        private static string GenerateTextLog(DamageLog log)
        {
            System.Text.StringBuilder sb = new();
            sb.AppendLine("<style=cWorldEvent>Damage Log");
            sb.AppendLine($"<{log.displayName}></style>");

            if (!Plugin.Config.SimpleTextMode) return sb.ToString();

            float now = log.Time;
            foreach (DamageSource src in log.GetEntries()) {
                float elapsedTime = now - src.time;
                if (log.IsExpired(elapsedTime)) continue;

                string style = src.isPlayerDamage ? "cDeath" : src.isFallDamage ? "cHumanObjective" : src.isVoidFogDamage ? "cIsVoid" : "";
                if (string.IsNullOrEmpty(style)) sb.Append(src.attackerName);
                else sb.Append($"<style={style}>{src.attackerName}</style>");

                bool singleHit = (src.hits == 1);
                if (!singleHit) sb.Append($"<style=cStack>×{src.hits}</style>");
                sb.Append($" · <style=cIsDamage>-{src.totalDamagePercent:0.0%}</style>");
                if (singleHit) sb.Append($" <style=cEvent>({src.remainingHpPercent:0.0%})</style>");

                sb.AppendLine($" · <style=cSub>{elapsedTime:0.00s}</style>");
            }

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

        private static RectTransform AnchorStretchRight(RectTransform rect)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.anchorMin = Vector2.right;
            rect.anchorMax = Vector2.one;
            return rect;
        }

        private static RectTransform AnchorTopStretch(RectTransform rect)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.anchorMin = Vector2.up;
            rect.anchorMax = Vector2.one;
            return rect;
        }
    }
}
