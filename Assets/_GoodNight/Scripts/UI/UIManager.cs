using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>Crea el Canvas, ordena las capas y administra la pila de paneles y el cursor.</summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager I;
        public static bool ModalOpen => I != null && I.stack.Count > 0;

        public RectTransform World, FX, HUD, Panels, Fade, Overlay, Top;
        readonly List<UIPanel> stack = new List<UIPanel>();
        int openedFrame = -1;

        void Awake()
        {
            I = this;
            var cgo = new GameObject("GN_Canvas");
            cgo.transform.SetParent(transform, false);
            var canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 20;
            var sc = cgo.AddComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1920, 1080); sc.matchWidthOrHeight = 0.5f;
            var root = (RectTransform)cgo.transform;

            World = Layer("World", root); FX = Layer("FX", root); HUD = Layer("HUD", root); Panels = Layer("Panels", root);
            Fade = Layer("Fade", root); Overlay = Layer("Overlay", root); Top = Layer("Top", root);

            // Orden fijo de construcción
            var hud = GetComponent<HUDView>(); if (hud) hud.Build(this);
            foreach (var p in GetComponents<UIPanel>()) { p.Build(p is MenuPanel || p is DeathPanel || p is CreditsPanel || p is PausePanel || p is OptionsPanel ? Top : Panels); p.SetVisible(false); }
        }

        static RectTransform Layer(string n, RectTransform parent)
        {
            var r = UIKit.Rect(n, parent); UIKit.Stretch(r); return r;
        }

        public T Get<T>() where T : UIPanel => GetComponent<T>();

        public void Open(UIPanel p)
        {
            if (p == null || stack.Contains(p)) return;
            stack.Add(p); p.SetVisible(true); if (p.Root) p.Root.SetAsLastSibling(); p.OnOpen();
            openedFrame = Time.frameCount;
            if (HUDView.I) HUDView.I.SetPrompt(null);
        }

        public void Close(UIPanel p)
        {
            if (p == null || !stack.Contains(p)) return;
            stack.Remove(p); p.SetVisible(false); p.OnClose();
            openedFrame = Time.frameCount;
        }

        public void CloseAll() { for (int i = stack.Count - 1; i >= 0; i--) Close(stack[i]); }
        public UIPanel TopPanel => stack.Count > 0 ? stack[stack.Count - 1] : null;
        public bool IsTop(UIPanel p) => TopPanel == p;

        void Update()
        {
            bool playing = GameManager.I != null && GameManager.I.Playing;
            var top = TopPanel;
            if (top != null)
            {
                if (Time.frameCount != openedFrame)
                {
                    if (Inp.Esc()) top.OnEsc();
                    else if (top.CloseWithInteract && Inp.Interact()) Close(top);
                    else if (top is PhonePanel && Inp.Phone()) Close(top);
                    else top.Tick();
                }
            }
            else if (playing && !PlayerController.CutsceneLock)
            {
                if (Inp.Esc()) Open(Get<PausePanel>());
                else if (Inp.Phone()) Open(Get<PhonePanel>());
            }

            bool locked = playing && stack.Count == 0;
            var want = locked ? CursorLockMode.Locked : CursorLockMode.None;
            if (Cursor.lockState != want) { Cursor.lockState = want; Cursor.visible = !locked; }
        }

        public void SetWorldTexture(Texture t)
        {
            var hud = HUDView.I; if (hud != null) hud.SetWorld(t);
        }
    }
}
