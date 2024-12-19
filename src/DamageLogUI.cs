using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace DamageLog
{
    /// <summary>
    /// 
    /// <code>
    /// HUDSimple(Clone)
    ///  │ HUD
    ///  │ DamageLogUI [component]
    ///  └─ MainContainer
    ///      └─ DamageLogUI [game object]
    /// </code>
    /// </summary>
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

#if NETSTANDARD2_1
            {
#if DEBUG
                System.Text.StringBuilder sb = new("Target Transform Hierarchy\n");
                int level = 0;
                Transform logTransform = panel.transform;
                while (logTransform != null) {
                    sb.AppendLine($"{new string(' ', level * 4)}└─ {logTransform.name}");
                    logTransform = logTransform.parent;
                    level++;
                }

                Plugin.Logger.LogMessage(sb.ToString());
#endif
                Transform t = panel.transform;
                while (t != null) {
                    if (t.name.Contains("Logbook") || t.name.Contains("Pause")) {
                        Plugin.Logger.LogWarning("Blocked attempt to move canvas. This can safely be ignored if triggered by viewing Game End Report from Run History mid-run.");
                        return;
                    };
                    t = t.parent;
                }
            }
#endif

#if DEBUG
            Plugin.Logger.LogWarning(StackTraceUtility.ExtractStringFromException(new System.NullReferenceException())); // For similar stack trace output to Unity Log NRE
#endif

            ui.enabled = false;
            ui.canvas.transform.SetParent(panel.transform);
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

            if (user == null) {
                Plugin.Logger.LogWarning("Failed to display player damage log (null).");
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

        /// <remarks>
        /// <see cref="DamageLogUI"/> exists on the <see cref="HUD"/> game object, whereas <see cref="canvas"/> exists on a new nested child game object.
        /// </remarks>
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
            if (!Plugin.Config.SimpleTextMode) CreatePortraits();
            Plugin.Logger.LogDebug("Created canvas.");
        }

        private void CreateCanvas(GameObject parent)
        {
            canvas = new GameObject("DamageLogUI", typeof(Canvas), typeof(GraphicRaycaster)).GetComponent<Canvas>();
            RectTransform rect = canvas.GetComponent<RectTransform>();
            rect.SetParent(parent.transform);
            ResetRectTransform(rect);

            AnchorStretchRight(rect);
            rect.pivot = Vector2.one;
            rect.localPosition -= (Vector3)Plugin.Config.CanvasOffsetTopRight;
            rect.sizeDelta = (Vector2.right * Plugin.Config.CanvasWidth) - (Vector2.up * Plugin.Config.CanvasOffsetTopRight); ;
        }

        private void CreatePortraits()
        {
            VerticalLayoutGroup layout = canvas.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childForceExpandHeight = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childControlWidth = false;
            layout.spacing = Plugin.Config.PortraitSpacing;

            for (int i = 0; i < Plugin.Config.MaximumPortraitCount; i++) {
                uiEntries.Add(DamageSourceUI.Instantiate((RectTransform)canvas.transform).Hide());
            }
        }

        private void CreateText()
        {
            RectTransform rect = new GameObject("DamageLogText", typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(canvas.transform);
            ResetRectTransform(rect);

            AnchorTopStretch(rect);
            rect.pivot = Vector2.one;
            rect.sizeDelta = Plugin.Config.SimpleTextMode ? Vector2.zero : new Vector2(((RectTransform)canvas.transform).sizeDelta.x, 0);

            text = rect.gameObject.AddComponent<HGTextMeshProUGUI>();
            text.fontSize = Plugin.Config.TextSize;
            text.SetText("<style=cDeath>Damage Log <null></style>");
        }




        private void ListenForRebuild()
        {
            if (Input.GetKey(KeyCode.Home) && Input.GetKeyDown(KeyCode.End)) {
                Plugin.Logger.LogWarning("Rebuild input triggered, destroying DamageLogUI.");
                Destroy(this.canvas.gameObject);
                Destroy(this);
                DamageLogUI.hud = null;
            }
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

            ListenForRebuild();

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

                if (!Plugin.Config.CompactLines) sb.AppendLine();

                string style = src.isPlayerDamage ? "cDeath" : src.isFallDamage ? "cHumanObjective" : src.isVoidFogDamage ? "cIsVoid" : "";
                if (string.IsNullOrEmpty(style)) sb.Append(src.attackerName);
                else sb.Append($"<style={style}>{src.attackerName}</style>");

                bool singleHit = (src.hits == 1);
                if (!singleHit) sb.Append($"<style=cStack><nobr>×{src.hits}</nobr></style>");
                if (Plugin.Config.ShowRawDamageInsteadOfPercentage) sb.Append($" · <style=cIsHealth><nobr>{-1*src.totalDamage:0.0}</nobr></style>");
                else sb.Append($" · <style=cIsDamage><nobr>{-1*src.totalDamagePercent:0.0%}</nobr></style>");
                if (singleHit) sb.Append($" <style=cEvent><nobr>({src.remainingHpPercent:0.0%})</nobr></style>");

                if (!Plugin.Config.HideDamageTimer) sb.AppendLine($" · <style=cSub>{elapsedTime:0.00s}</style>");
                else sb.AppendLine();
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
