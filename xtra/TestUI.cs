﻿using UnityEngine;
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
            // CreateDebug();
            CreateText();
            if (useLayout) CreateLayout();
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
            rect.sizeDelta = new Vector2(92, 0) - offsetTopRight;
        }

        private void CreateText()
        {
            GameObject obj = new GameObject("DamageLogText", typeof(RectTransform));
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(gameObject.transform);
            ResetRectTransform(rect);

            AnchorTopStretch(rect);
            rect.pivot = Vector2.one;
            rect.sizeDelta = useLayout ? new Vector2(((RectTransform)gameObject.transform).sizeDelta.x, 0) : Vector2.zero;

            tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = textSize;
            tmp.color = Color.yellow;
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




        [SerializeField] bool useLayout = true;
        public float textSize = 12;
        public float spacing = 12;
        public float portraitSize = -1;
        public float eliteIconSize = 32f;
        public float portraitTextSize = 18;
        public float damageTextSize = 20;
        [SerializeField] int count = 5;

        private void CreateLayout() {
            VerticalLayoutGroup layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childForceExpandHeight = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childControlWidth = false;
            layout.spacing = spacing;

            for (int i = 0; i < count; i++) {
                TestElementUI.Create((RectTransform)gameObject.transform, this);
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
            rect.anchorMin = Vector2.right;
            rect.anchorMax = Vector2.one;
            return rect;
        }

        public static RectTransform AnchorTopStretch(RectTransform rect)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.anchorMin = Vector2.up;
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
