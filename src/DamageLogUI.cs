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

        /// <summary>
        /// Awake() is too early for accessing hud members.
        /// </summary>
        private void Start() => CreateUI();
        private void CreateUI()
        {
            CreateCanvas();
            CreateText();

            Log.Debug($"{Plugin.GUID}> Canvas created.");
        }

        private void CreateCanvas()
        {
            gameObject = new GameObject("DamageLogUI", typeof(Canvas));
            gameObject.transform.SetParent(hud.mainContainer.transform);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            FixRectTransform(rect);
            AnchorStretchRight(rect, 110);

            const float offsetTop = 12;
            rect.localPosition = new Vector3(rect.localPosition.x, rect.localPosition.y - offsetTop, rect.localPosition.z);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y - offsetTop);
        }

        private void CreateText()
        {
            GameObject obj = new GameObject("Text", typeof(RectTransform));
            obj.transform.SetParent(gameObject.transform);
            RectTransform rect = obj.GetComponent<RectTransform>();
            FixRectTransform(rect);
            AnchorStretchStretch(rect);

            text = obj.AddComponent<HGTextMeshProUGUI>();
            text.fontSize = 12;
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
            sb.AppendLine($"<style=cWorldEvent>Damage Log <{log.user.masterController.GetDisplayName()}></style>");

            int i = -1; // incremented before check
            float endTime = (log.timeOfDeath > 0) ? log.timeOfDeath : Time.time;
            foreach (DamageLog.DamageSource s in log.GetEntries()) {
                i++;
                if (log.TryPrune(s, endTime, i)) continue;

                string style = s.isFallDamage ? "cHumanObjective" : s.isVoidFogDamage ? "cIsVoid" : "";
                if (string.IsNullOrEmpty(style)) {
                    sb.Append(s.attackerName);
                }
                else {
                    sb.Append($"<style={style}>{s.attackerName}</style>");
                }

                bool singleHit = (s.hits != 1);
                if (!singleHit) sb.Append($"<style=cStack>×{s.hits}</style>");
                sb.Append($" · <style=cIsHealth>-{s.damagePercent:0.0%}</style>");
                if (singleHit) sb.Append($" <style=cEvent>({s.hpPercent:0.0%})</style>");

                sb.AppendLine($" · <style=cSub>{(endTime - s.time):0.00s}</style>");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Fixes the local position, local scale, local rotation, and layer of a RectTransform to 'normal' values after parenting to a Canvas (previously existed as a Transform game object).
        /// <br/>Alternatively, pass worldPositionStays: false to Transform.SetParent() and manually update gameObject.layer.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        private static RectTransform FixRectTransform(RectTransform rect)
        {
            rect.localPosition = new Vector3(rect.localPosition.x, rect.localPosition.y, 0);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.gameObject.layer = LayerMask.NameToLayer("UI");
            return rect;
        }

        private static RectTransform AnchorStretchRight(RectTransform rect, float width = 100)
        {
            rect.pivot = Vector2.one;
            rect.anchoredPosition = Vector2.zero;
            rect.anchorMin = new Vector2(1, 0);
            rect.anchorMax = Vector2.one;
            rect.sizeDelta =  new Vector2(width, 0);
            return rect;
        }

        private static RectTransform AnchorStretchStretch(RectTransform rect)
        {
            rect.pivot = Vector2.one;
            rect.anchoredPosition = Vector2.zero;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            return rect;
        }
    }
}
