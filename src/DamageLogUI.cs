﻿using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace DamageLog
{
    internal sealed class DamageLogUI : MonoBehaviour
    {
        private static HUD hud;

        /// <remarks>
        /// Subscribes to <see cref="HUD.shouldHudDisplay"/>, which functionally appears to be invoked at the start of each stage.
        /// </remarks>
        /// <param name="hud"></param>
        /// <param name="shouldDisplay"></param>
        public static void Init(HUD hud, ref bool shouldDisplay)
        {
            if (DamageLogUI.hud != null) return;

            Log.Debug($"Adding to HUD.");
            hud.gameObject.AddComponent<DamageLogUI>();
            DamageLogUI.hud = hud;
            DamageLog.ClearBossLogs();
        }

        internal static void MoveToGameEndReportPanel(GameEndReportPanelController panel)
        {
            DamageLogUI ui = hud.gameObject.GetComponent<DamageLogUI>();
            if (ui == null) {
                Log.Warning("Failed to move canvas (missing).");
                return;
            }

            ui.gameObject.transform.SetParent(panel.transform);
            ui.enabled = false;
            ui.canvas.enabled = true;
            Log.Debug("Moved canvas.");
        }

        internal static void DisplayPlayerDamageLog(NetworkUser user)
        {
            DamageLogUI ui = hud.gameObject.GetComponent<DamageLogUI>();
            if (ui == null) {
                Log.Warning("Failed to update canvas (missing).");
                return;
            }

            if (!DamageLog.UserLogs.TryGetValue(user, out DamageLog log)) {
                Log.Warning($"Failed to find damage log for {user.userName}.");
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
            Plugin.ReloadConfig();
            user = hud?.localUserViewer?.currentNetworkUser;
            CreateCanvas(parent);
            CreateText();
            if (!Plugin.Config.SimpleTextMode) CreateLayout();
            Log.Debug($"Created canvas.");
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

            for (int i = 0; i < Plugin.Config.EntryMaxCount; i++) {
                uiEntries.Add(DamageSourceUI.Create((RectTransform)gameObject.transform).Clear());
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
            // Scoreboard visibility logic from RoR2UI.HUD.Update()
            bool visible = !Plugin.Config.OnlyShowWithScoreboard || (hud.localUserViewer?.inputPlayer != null && hud.localUserViewer.inputPlayer.GetButton("info"));
            canvas.enabled = visible;
            if (!visible) return;

            bool shiftKey = Input.GetKey("left shift") || Input.GetKey("right shift");
            bool cycleUser = Input.GetKeyDown(Plugin.Config.CycleUserKey);
            bool cycleBoss = Plugin.Config.TrackBosses && Input.GetKeyDown(Plugin.Config.CycleBossKey);

            if (cycleUser) {
                if (!showingBoss) user = CycleUser(user, shiftKey);
                showingBoss = false;
            }
            else if (cycleBoss && DamageLog.BossLogs.Count > 0) {
                if (showingBoss) bossIndex = CycleCollectionIndex(bossIndex, DamageLog.BossLogs, shiftKey);
                showingBoss = true;
            }

            if ((!showingBoss && DamageLog.UserLogs.TryGetValue(user, out DamageLog log)) || (showingBoss && TryGetDamageLog(bossIndex, DamageLog.BossLogs, out log))) {
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

            List<DamageSource> entries = log.GetEntries();
            for (int i = 0; i < uiEntries.Count; i++) {
                if (i >= entries.Count) { uiEntries[i].Clear(); continue; }

                float endTime = (log.timeOfDeath > 0) ? log.timeOfDeath : Time.time;
                if (log.TryPrune(entries[i], endTime, i)) { uiEntries[i].Clear(); continue; }

                uiEntries[i].Display(entries[i], endTime - entries[i].time);
            }
        }

        private static string GenerateTextLog(DamageLog log)
        {
            System.Text.StringBuilder sb = new();
            string target = string.IsNullOrWhiteSpace(log.targetDisplayStyle) ? log.targetDisplayName : $"<style={log.targetDisplayStyle}>{log.targetDisplayName}</style>";
            sb.AppendLine($"<style=cWorldEvent>Damage Log <{target}></style>");

            if (!Plugin.Config.SimpleTextMode) return sb.ToString();

            int i = -1; // incremented before check
            float endTime = (log.timeOfDeath > 0) ? log.timeOfDeath : Time.time;
            foreach (DamageSource src in log.GetEntries()) {
                i++;
                if (log.TryPrune(src, endTime, i)) continue;

                string style = src.isPlayerDamage ? "cDeath" : src.isFallDamage ? "cHumanObjective" : src.isVoidFogDamage ? "cIsVoid" : "";
                if (string.IsNullOrEmpty(style)) sb.Append(src.attackerName);
                else sb.Append($"<style={style}>{src.attackerName}</style>");

                bool singleHit = (src.hits == 1);
                if (!singleHit) sb.Append($"<style=cStack>×{src.hits}</style>");
                sb.Append($" · <style=cIsDamage>-{src.totalDamagePercent:0.0%}</style>");
                if (singleHit) sb.Append($" <style=cEvent>({src.remainingHpPercent:0.0%})</style>");

                sb.AppendLine($" · <style=cSub>{(endTime - src.time):0.00s}</style>");
            }

            return sb.ToString();
        }




        private static NetworkUser CycleUser(NetworkUser current, bool reverse)
        {
            if (DamageLog.UserLogs.Count <= 0) return null;

            int i = (current == null) ? 0 : NetworkUser.readOnlyInstancesList.IndexOf(current);
            i = CycleCollectionIndex(i,NetworkUser.readOnlyInstancesList , reverse);
            NetworkUser user = NetworkUser.readOnlyInstancesList[i];

            if (DamageLog.UserLogs.ContainsKey(user)) return user;
            // Probably fine
            return CycleUser(user, reverse);
        }

        private static int CycleCollectionIndex(int index, System.Collections.ICollection collection, bool reverse) {
            if (reverse) index--;
            else index++;

            if (index < 0) index = collection.Count - 1;
            else if (index >= collection.Count) index = 0;

            return index;
        }

        private static bool TryGetDamageLog<TKey>(int index, Dictionary<TKey, DamageLog> dictionary, out DamageLog log) {
            log = null;
            if (index < 0) return false;
            if (index >= dictionary.Count) return false;

            log = dictionary.ElementAt(index).Value;
            return true;
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
