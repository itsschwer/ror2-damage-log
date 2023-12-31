using HarmonyLib;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DamageLog
{
    internal sealed class DamageSourceUI : MonoBehaviour
    {
        private RawImage portrait;
        private Image elite;
        private HGTextMeshProUGUI damage;
        private HGTextMeshProUGUI hits;
        private HGTextMeshProUGUI time;
        private TooltipProvider tooltip;

        public static DamageSourceUI Create(RectTransform parent)
        {
            GameObject obj = new GameObject("DamageEntry", typeof(DamageSourceUI), typeof(RawImage));
            obj.transform.SetParent(parent, false);
            ((RectTransform)obj.transform).sizeDelta = Vector2.one * parent.sizeDelta.x;
            return obj.GetComponent<DamageSourceUI>().Create(obj);
        }

        private DamageSourceUI Create(GameObject root)
        {
            RectTransform obj = (RectTransform)root.transform;
            float width = ((RectTransform)obj.parent).sizeDelta.x;
            const float eliteIconSize = 32f;
            const float timeTextHeight = 16f;
            const float timeFontSize = 13;

            tooltip = root.AddComponent<TooltipProvider>();
            portrait = root.GetComponent<RawImage>();

            elite = AddChild<Image>(obj, "elite");
            AnchorTopLeft(elite.rectTransform);
            elite.raycastTarget = false;
            elite.rectTransform.sizeDelta = Vector2.one * eliteIconSize;

            damage = AddChild<HGTextMeshProUGUI>(obj, "damage");
            AnchorTopRight(damage.rectTransform);
            damage.raycastTarget = false;
            damage.alignment = TextAlignmentOptions.TopRight;
            damage.rectTransform.sizeDelta = new Vector2(width - eliteIconSize, timeTextHeight);
            damage.fontSize = timeFontSize;

            hits = AddChild<HGTextMeshProUGUI>(obj, "hits");
            AnchorBottomLeft(hits.rectTransform);
            hits.raycastTarget = false;
            hits.alignment = TextAlignmentOptions.BottomLeft;
            hits.rectTransform.sizeDelta = new Vector2(width - eliteIconSize, timeTextHeight);
            hits.fontSize = timeFontSize;

            time = AddChild<HGTextMeshProUGUI>(obj, "time");
            AnchorBottomRight(time.rectTransform);
            time.raycastTarget = false;
            time.alignment = TextAlignmentOptions.BottomRight;
            time.rectTransform.sizeDelta = new Vector2(width / 2, timeTextHeight);
            time.fontSize = timeFontSize;

            return this;
        }

        private static T AddChild<T>(RectTransform parent, string name) where T : Component
        {
            GameObject obj = new GameObject(name, typeof(T));
            obj.transform.SetParent(parent);
            DamageLogUI.ResetRectTransform((RectTransform)obj.transform);
            return obj.GetComponent<T>();
        }




        public void Display(DamageSource src, float hitTime)
        {
            if (src == null) { Clear(); return; }

            portrait.gameObject.SetActive(true);
            portrait.texture = src.attackerPortrait;
            elite.sprite = src.eliteIcon;
            damage.SetText($"-{src.damagePercent:0.0%}");
            hits.SetText($"{src.hits}");
            time.SetText($"{hitTime:0.00s}");

            tooltip.titleColor = src.isPlayerDamage ? ColorCatalog.GetColor(ColorCatalog.ColorIndex.HardDifficulty)
                                  : src.isFallDamage ? ColorCatalog.GetColor(ColorCatalog.ColorIndex.NormalDifficulty)
                                  : src.isVoidFogDamage ? ColorCatalog.GetColor(ColorCatalog.ColorIndex.VoidItem)
                                  : ColorCatalog.GetColor(ColorCatalog.ColorIndex.Tier3ItemDark);
            tooltip.titleToken = src.attackerName;
            tooltip.bodyToken = GenerateTooltipString(src);
        }

        public void Clear()
        {
            portrait.gameObject.SetActive(false);
            portrait.texture = null;
            elite.sprite = null;
            damage.SetText("");
            hits.SetText("");
            time.SetText("");
        }

        private static string GenerateTooltipString(DamageSource src)
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




        private static void AnchorTopLeft(RectTransform rect) => Anchor(rect, Vector2.up);
        private static void AnchorTopRight(RectTransform rect) => Anchor(rect, Vector2.one);
        private static void AnchorBottomLeft(RectTransform rect) => Anchor(rect, Vector2.zero);
        private static void AnchorBottomRight(RectTransform rect) => Anchor(rect, Vector2.right);
        private static void Anchor(RectTransform rect, Vector2 anchor)
            { rect.pivot = rect.anchorMin = rect.anchorMax = anchor; }
    }
}
