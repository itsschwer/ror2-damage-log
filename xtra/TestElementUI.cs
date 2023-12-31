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

        public static TestElementUI Create(RectTransform parent, TestUI options) {
            GameObject obj = new GameObject("portrait", typeof(RawImage), typeof(TestElementUI));
            obj.transform.SetParent(parent, false);
            ((RectTransform)obj.transform).sizeDelta = Vector2.one * ((options.portraitSize > 0) ? options.portraitSize : parent.sizeDelta.x);
            return obj.GetComponent<TestElementUI>().BuildUI(obj, options);
        }

        private TestElementUI BuildUI(GameObject root, TestUI options) {
            RawImage img = root.GetComponent<RawImage>();
            img.color = new Color(1, 1, 1, 0.7f);
            
            float width = ((RectTransform)rectTransform.parent).sizeDelta.x;

            var elite = AddChild<Image>(rectTransform, "elite");
            AnchorTopLeft(elite.rectTransform);
            elite.raycastTarget = false;
            elite.rectTransform.sizeDelta = Vector2.one * options.eliteIconSize;
            elite.color = new Color(0.2783f, 0.8353f, 1, 0.5f);

            var damage = AddChild<TextMeshProUGUI>(rectTransform, "damage");
            AnchorTopRight(damage.rectTransform);
            damage.raycastTarget = false;
            damage.alignment = TextAlignmentOptions.TopRight;
            damage.rectTransform.sizeDelta = Vector2.one * (width - options.eliteIconSize);
            damage.color = Color.blue;
            damage.fontSize = options.damageTextSize;
            damage.SetText("-45.7%");

            var hits = AddChild<TextMeshProUGUI>(rectTransform, "hits");
            AnchorBottomLeft(hits.rectTransform);
            hits.raycastTarget = false;
            hits.alignment = TextAlignmentOptions.BottomLeft;
            hits.rectTransform.sizeDelta = Vector2.one * (width / 2);
            hits.color = Color.blue;
            hits.fontSize = options.portraitTextSize;
            hits.SetText("×2");

            var time = AddChild<TextMeshProUGUI>(rectTransform, "time");
            AnchorBottomRight(time.rectTransform);
            time.raycastTarget = false;
            time.alignment = TextAlignmentOptions.BottomRight;
            time.rectTransform.sizeDelta = Vector2.one * (width / 2);
            time.color = Color.blue;
            time.fontSize = options.portraitTextSize;
            time.SetText("1.36s");

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
