using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DamageLog.Unity {
    public class TestElementUI : MonoBehaviour {
        private RectTransform _rectTransform;
        private RectTransform rectTransform {
            get {
                if (_rectTransform == null) {
                    _rectTransform = GetComponent<RectTransform>();
                }
                return _rectTransform;
            }
        }

        public static TestElementUI Create(RectTransform parent, Sprite portrait) {
            GameObject obj = new GameObject("portrait", typeof(RawImage), typeof(TestElementUI));
            obj.transform.SetParent(parent, false);
            ((RectTransform)obj.transform).sizeDelta = Vector2.one * parent.sizeDelta.x;
            return obj.GetComponent<TestElementUI>().BuildUI(obj, portrait);
        }

        private TestElementUI BuildUI(GameObject root, Sprite portrait) {
            RawImage img = root.GetComponent<RawImage>();
            img.texture = portrait.texture;
            
            float width = ((RectTransform)rectTransform.parent).sizeDelta.x;
            const float eliteIconSize = 32f;
            const float timeTextHeight = 16f;
            const float timeFontSize = 13;

            var elite = AddChild<Image>(rectTransform, "elite");
            AnchorTopLeft(elite.rectTransform);
            elite.raycastTarget = false;
            elite.rectTransform.sizeDelta = Vector2.one * eliteIconSize;
            elite.sprite = portrait;

            var time = AddChild<TextMeshProUGUI>(rectTransform, "time");
            AnchorBottomRight(time.rectTransform);
            time.raycastTarget = false;
            time.alignment = TextAlignmentOptions.BottomRight;
            time.rectTransform.sizeDelta = new Vector2(width / 2, timeTextHeight);
            time.fontSize = timeFontSize;
            time.SetText("1.15s");

            var damage = AddChild<TextMeshProUGUI>(rectTransform, "damage");
            AnchorTopRight(damage.rectTransform);
            damage.raycastTarget = false;
            damage.alignment = TextAlignmentOptions.TopRight;
            damage.rectTransform.sizeDelta = new Vector2(width - eliteIconSize, timeTextHeight);
            damage.fontSize = timeFontSize;
            damage.SetText("-34.3%");

            var hits = AddChild<TextMeshProUGUI>(rectTransform, "hits");
            AnchorBottomLeft(hits.rectTransform);
            hits.raycastTarget = false;
            hits.alignment = TextAlignmentOptions.BottomLeft;
            hits.rectTransform.sizeDelta = new Vector2(width - eliteIconSize, timeTextHeight);
            hits.fontSize = timeFontSize;
            hits.SetText("×2");

            return this;
        }

        private T AddChild<T>(RectTransform parent, string name) where T : Component {
            GameObject obj = new GameObject(name, typeof(T));
            obj.transform.SetParent(parent);
            TestUI.ResetRectTransform((RectTransform)obj.transform);
            return obj.GetComponent<T>();
        }

        private static void AnchorTopLeft(RectTransform rect) => Anchor(rect, Vector2.up);
        private static void AnchorTopRight(RectTransform rect) => Anchor(rect, Vector2.one);
        private static void AnchorBottomLeft(RectTransform rect) => Anchor(rect, Vector2.zero);
        private static void AnchorBottomRight(RectTransform rect) => Anchor(rect, Vector2.right);
        private static void Anchor(RectTransform rect, Vector2 anchor)
            { rect.pivot = rect.anchorMin = rect.anchorMax = anchor; }
    }
}
