using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DamageLog.Unity {
    /// <summary>
    /// Makeshift component for testing UI generation in a Unity project.
    /// <br/>Usage: attach to a Canvas.
    /// </summary>
    public sealed class TestUI : MonoBehaviour {
        private new GameObject gameObject;

        private void Awake(){
            CreateCanvas();
            CreateDebug();
            CreateText();
        }

        private void CreateCanvas()
        {
            gameObject = new GameObject("DamageLogUI", typeof(Canvas), typeof(GraphicRaycaster));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(transform);
            ResetRectTransform(rect);

            AnchorStretchRight(rect);
            rect.pivot = Vector2.one;

            Vector2 offsetTopRight = new Vector2(4, 12);
            rect.localPosition -= (Vector3)offsetTopRight;
            rect.sizeDelta = new Vector2(110, 0) - offsetTopRight;
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

            tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.color = Color.black;
            tmp.SetText("I can eat glass.");
        }




        private void CreateDebug() {
            GameObject obj = new GameObject("DamageLogDebug", typeof(RectTransform));
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(gameObject.transform);
            ResetRectTransform(rect);

            AnchorStretchStretch(rect);
            rect.pivot = Vector2.one;

            var x = obj.AddComponent<Image>();
            x.color = new Color(1, 1, 1, 0.6f);
        }

        TextMeshProUGUI tmp;
        private void Update() {
            if (!tmp) return;

            Vector2 size = tmp.rectTransform.sizeDelta;
            if (size.y != tmp.preferredHeight) {
                tmp.rectTransform.sizeDelta = new Vector2(size.x, tmp.preferredHeight);
            }
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
