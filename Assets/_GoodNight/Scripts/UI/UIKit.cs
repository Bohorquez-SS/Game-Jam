using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>Ayudantes para construir la UI por código (sin EventSystem).</summary>
    public static class UIKit
    {
        static Font font;
        public static Font Font { get { if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); return font; } }

        public static readonly Color Panel = new Color(0.05f, 0.055f, 0.07f, 0.94f);
        public static readonly Color Accent = new Color(0.95f, 0.82f, 0.55f);
        public static readonly Color Green = new Color(0.45f, 1f, 0.55f);
        public static readonly Color Red = new Color(0.9f, 0.18f, 0.15f);
        public static readonly Color Muted = new Color(0.6f, 0.6f, 0.65f);

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static Image Img(string name, Transform parent, Color c)
        {
            var r = Rect(name, parent);
            var i = r.gameObject.AddComponent<Image>();
            i.color = c; i.raycastTarget = false;
            return i;
        }

        public static RawImage Raw(string name, Transform parent)
        {
            var r = Rect(name, parent);
            var i = r.gameObject.AddComponent<RawImage>();
            i.raycastTarget = false;
            return i;
        }

        public static Text Txt(string name, Transform parent, int size, TextAnchor anchor, Color c, bool shadow = true)
        {
            var r = Rect(name, parent);
            var t = r.gameObject.AddComponent<Text>();
            t.font = Font; t.fontSize = size; t.alignment = anchor; t.color = c;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true; t.raycastTarget = false; t.text = "";
            if (shadow)
            {
                var s = r.gameObject.AddComponent<Shadow>();
                s.effectColor = new Color(0, 0, 0, 0.85f); s.effectDistance = new Vector2(2, -2);
            }
            return t;
        }

        public static void Stretch(RectTransform rt, float l = 0, float b = 0, float r = 0, float t = 0)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(l, b); rt.offsetMax = new Vector2(-r, -t);
        }

        public static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor; rt.pivot = pivot;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
        }

        public static void Center(RectTransform rt, Vector2 pos, Vector2 size) => Place(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);

        public static bool Over(RectTransform rt) =>
            rt != null && rt.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(rt, Inp.MousePos(), null);
    }
}
