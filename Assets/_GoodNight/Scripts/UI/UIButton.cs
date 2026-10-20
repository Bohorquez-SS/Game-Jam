using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>Botón dibujado con Image+Text; se actualiza desde el panel dueño.</summary>
    public class UIButton
    {
        public RectTransform rt; public Image bg; public Text label;
        public bool interactable = true;
        public Color normal = new Color(0.14f, 0.15f, 0.18f, 0.95f);
        public Color hover = new Color(0.32f, 0.27f, 0.18f, 1f);
        public Color off = new Color(0.08f, 0.08f, 0.09f, 0.7f);

        public UIButton(Transform parent, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fontSize = 28)
        {
            bg = UIKit.Img("Btn_" + text, parent, normal);
            rt = bg.rectTransform;
            UIKit.Place(rt, anchor, new Vector2(0.5f, 0.5f), pos, size);
            label = UIKit.Txt("Label", rt, fontSize, TextAnchor.MiddleCenter, Color.white);
            UIKit.Stretch(label.rectTransform, 6, 2, 6, 2);
            label.text = text;
        }

        /// <summary>Devuelve true el frame en que se hace clic.</summary>
        public bool Tick()
        {
            if (!rt.gameObject.activeInHierarchy) return false;
            bool over = interactable && UIKit.Over(rt);
            bg.color = !interactable ? off : (over ? hover : normal);
            label.color = interactable ? Color.white : UIKit.Muted;
            if (over && Inp.Click())
            {
                if (AudioManager.I) AudioManager.I.UI(AudioManager.I.click, 0.5f);
                return true;
            }
            return false;
        }

        public void SetText(string s) { label.text = s; }
        public void SetActive(bool a) { rt.gameObject.SetActive(a); }
    }
}
