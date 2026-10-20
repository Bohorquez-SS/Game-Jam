using UnityEngine;

namespace GNight
{
    /// <summary>Elemento clicable dentro de un aparato.</summary>
    public abstract class Clickable : MonoBehaviour
    {
        public string labelEs, labelEn;
        public bool interactable = true;
        public Renderer highlight;
        public virtual string Label => Settings.T(labelEs, labelEn);
        public abstract void Click();
        Color baseCol; bool hasCol;
        public virtual void SetHover(bool on)
        {
            if (highlight == null) return;
            var m = highlight.material;
            string prop = m.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
            if (!hasCol) { baseCol = m.GetColor(prop); hasCol = true; }
            m.SetColor(prop, on ? baseCol * 1.6f : baseCol);
        }
    }
}
