using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>Base de las pantallas modales (teléfono, paneles, menús...).</summary>
    public abstract class UIPanel : MonoBehaviour
    {
        protected RectTransform root;
        public bool IsOpen { get; private set; }
        public RectTransform Root => root;
        public abstract void Build(RectTransform layer);
        public virtual void OnOpen() { }
        public virtual void OnClose() { }
        /// <summary>Esc por defecto cierra el panel.</summary>
        public virtual void OnEsc() { UIManager.I.Close(this); }
        public virtual bool CloseWithInteract => false;
        public void SetVisible(bool v) { IsOpen = v; if (root) root.gameObject.SetActive(v); }
        public virtual void Tick() { }
    }
}
