using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>Deslizador horizontal 0..1 manejado con el ratón.</summary>
    public class UISlider
    {
        public RectTransform rt; Image fill; Text label; string title;
        public float Value;
        bool dragging;

        public UISlider(Transform parent, string title, Vector2 pos, float value)
        {
            this.title = title; Value = value;
            var bg = UIKit.Img("Slider_" + title, parent, new Color(0.12f, 0.12f, 0.14f, 1f));
            rt = bg.rectTransform;
            UIKit.Center(rt, pos + new Vector2(120, 0), new Vector2(420, 26));
            fill = UIKit.Img("Fill", rt, UIKit.Accent);
            fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = new Vector2(value, 1);
            fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;
            label = UIKit.Txt("Label", parent, 26, TextAnchor.MiddleRight, Color.white);
            UIKit.Center(label.rectTransform, pos + new Vector2(-280, 0), new Vector2(360, 40));
            Refresh();
        }

        public bool Tick()
        {
            if (!rt.gameObject.activeInHierarchy) return false;
            if (Inp.Click() && UIKit.Over(rt)) dragging = true;
            if (!Inp.ClickHeld()) dragging = false;
            if (!dragging) return false;
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, Inp.MousePos(), null, out local);
            float w = rt.rect.width;
            Value = Mathf.Clamp01((local.x + w * 0.5f) / w);
            Refresh();
            return true;
        }

        void Refresh()
        {
            fill.rectTransform.anchorMax = new Vector2(Value, 1);
            label.text = title + "  " + Mathf.RoundToInt(Value * 100) + "%";
        }
    }
}
