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
            ((RectTransform)obj.transform).sizeDelta = Vector2.one * ((Plugin.Config.PortraitSize > 0) ? Plugin.Config.PortraitSize : parent.sizeDelta.x);
            return obj.GetComponent<DamageSourceUI>().Create(obj);
        }

        private DamageSourceUI Create(GameObject root)
        {
            RectTransform obj = (RectTransform)root.transform;
            float width = ((RectTransform)obj.parent).sizeDelta.x;

            tooltip = root.AddComponent<TooltipProvider>();
            portrait = root.GetComponent<RawImage>();

            elite = AddChild<Image>(obj, "elite");
            AnchorTopLeft(elite.rectTransform);
            elite.raycastTarget = false;
            elite.rectTransform.sizeDelta = Vector2.one * Plugin.Config.EliteIconSize;

            damage = AddChild<HGTextMeshProUGUI>(obj, "damage");
            AnchorTopRight(damage.rectTransform);
            damage.raycastTarget = false;
            damage.alignment = TextAlignmentOptions.TopRight;
            damage.rectTransform.sizeDelta = Vector2.one * (width - Plugin.Config.EliteIconSize);
            damage.fontSize = Plugin.Config.DamageTextSize;

            hits = AddChild<HGTextMeshProUGUI>(obj, "hits");
            AnchorBottomLeft(hits.rectTransform);
            hits.raycastTarget = false;
            hits.alignment = TextAlignmentOptions.BottomLeft;
            hits.rectTransform.sizeDelta = Vector2.one * (width / 2);
            hits.fontSize = Plugin.Config.PortraitTextSize;

            time = AddChild<HGTextMeshProUGUI>(obj, "time");
            AnchorBottomRight(time.rectTransform);
            time.raycastTarget = false;
            time.alignment = TextAlignmentOptions.BottomRight;
            time.rectTransform.sizeDelta = Vector2.one * (width / 2);
            time.fontSize = Plugin.Config.PortraitTextSize;

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
            portrait.color = src.attackerColor;

            if (src.eliteIcon != null) {
                elite.sprite = src.eliteIcon;
                elite.color = src.eliteColor;
                elite.enabled = true;
            }
            else {
                elite.enabled = false;
            }

            damage.SetText($"<style=cIsDamage>-{src.damagePercent:0.0%}</style>");

            bool singleHit = (src.hits == 1);
            if (singleHit) hits.SetText("");
            else hits.SetText($"<style=cStack>×{src.hits}</style>");

            time.SetText($"<style=cSub>{hitTime:0.00s}</style>");

            tooltip.titleColor = src.isPlayerDamage ? ColorCatalog.GetColor(ColorCatalog.ColorIndex.HardDifficulty)
                                  : src.isFallDamage ? ColorCatalog.GetColor(ColorCatalog.ColorIndex.NormalDifficulty)
                                  : src.isVoidFogDamage ? ColorCatalog.GetColor(ColorCatalog.ColorIndex.VoidItem)
                                  : ColorCatalog.GetColor(ColorCatalog.ColorIndex.Tier3ItemDark);
            tooltip.titleToken = src.attackerName;
            tooltip.bodyToken = GenerateTooltipString(src);
        }

        public DamageSourceUI Clear()
        {
            portrait.gameObject.SetActive(false);
            return this;
        }

        private static string GenerateTooltipString(DamageSource src)
        {
            System.Text.StringBuilder sb = new();

            sb.Append($"Dealt <style=cIsHealth>{src.damage:0.0}</style> damage");
            if (src.hits == 1) sb.Append($" <style=cEvent>({src.hpPercent:0.0%} health remaining)</style>");
            else sb.Append($" in <style=cStack>{src.hits} hits</style> over <style=cSub>{(src.time - src.timeStart):0.00s}</style>");
            sb.AppendLine(".");

            if (Plugin.Config.ShowIdentifier) {
                sb.AppendLine();
                sb.AppendLine($"<style=cIsDamage>{src.identifier}</style>");
            }

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
