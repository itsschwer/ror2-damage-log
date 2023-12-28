using RoR2;
using RoR2.UI;
using UnityEngine;

namespace DamageLog
{
    public class DamageLogUI : MonoBehaviour
    {
        private static HUD hud;

        public static void Init(HUD hud, ref bool shouldDisplay)
        {
            if (DamageLogUI.hud != null) return;

            hud.gameObject.AddComponent<DamageLogUI>();
            DamageLogUI.hud = hud;
        }

        private new GameObject gameObject;
        private HGTextMeshProUGUI text;

        private void Awake() => CreateUI();
        // Based on Bubbet's implementation, for debugging, to be replaced
        // https://github.com/Bubbet/Risk-Of-Rain-Mods/blob/8ae184f/DamageHistory/DamageHistoryHUD.cs
        private void CreateUI()
        {
            CreateCanvas();
            CreateText();
        }

        private void CreateCanvas()
        {
            gameObject = new GameObject("DamageLogUI", typeof(Canvas));
            gameObject.transform.SetParent(hud.mainContainer.transform);
            AnchorTopRightCorner(gameObject.GetComponent<RectTransform>());
        }

        private void CreateText()
        {
            GameObject obj = new GameObject("Text", typeof(RectTransform));
            obj.transform.SetParent(gameObject.transform);
            AnchorTopRightCorner(obj.GetComponent<RectTransform>());

            text = obj.AddComponent<HGTextMeshProUGUI>();
            text.fontSize = 14;
            text.SetText("Damage Log");
        }

        private void Update()
        {
            NetworkUser user = hud.localUserViewer?.currentNetworkUser;
            if (user == null) return;
            if (!DamageLog.Logs.TryGetValue(user, out DamageLog log)) return;

            text.SetText(GenerateTextLog(log));
        }

        private string GenerateTextLog(DamageLog log)
        {
            System.Text.StringBuilder sb = new();
            sb.AppendLine($"Damage Log <{log.user.masterController.GetDisplayName()}>");

            float endTime = (log.timeOfDeath > 0) ? log.timeOfDeath : Time.time;
            foreach (DamageLog.DamageSource s in log.GetEntries()) {
                string style = s.isFallDamage ? "cSub" : s.isVoidFogDamage ? "cIsVoid" : "";
                if (string.IsNullOrEmpty(style)) {
                    sb.Append(s.attackerName);
                }
                else {
                    sb.Append($"<style={style}>{s.attackerName}</style>");
                }

                if (s.hits != 1) {
                    sb.Append($"<style=cStack>×{s.hits}</style>");
                    sb.Append($" · <style=cIsHealth>-{s.damagePercent : 0.0%}</style>");
                }
                else {
                    sb.Append($" · <style=cIsHealth>{s.hpPercentOld : 0.0%} <style=cSub>></style> {s.hpPercentNow : 0.0%}</style>");
                }

                sb.AppendLine($" · <style=cSub>{endTime - s.time : 0.00s}</style>");
            }

            return sb.ToString();
        }

        private static void AnchorTopRightCorner(RectTransform rect)
        {
            rect.pivot = Vector2.one;
            rect.anchoredPosition = Vector2.zero;
            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;
        }
    }
}
