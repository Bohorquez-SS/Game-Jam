using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>
    /// Pantalla CRT del mundo que muestra una cámara (o una cinta) con interfaz de vigilancia en un Canvas del mundo:
    /// nombre, LIVE/REC, hora, señal, mensajes. Solo pide render si el jugador está cerca.
    /// </summary>
    public class MonitorScreen : MonoBehaviour
    {
        public Renderer screen;
        public Material offMat, staticMat;
        public SecurityCam source;
        public Texture overrideTexture;  // cintas VHS
        public bool on;
        public string requireZone;
        public float overlayScale = 0.0011f;
        [HideInInspector] public string message;   // texto grande central (NO DISPONIBLE, etc.)
        [HideInInspector] public bool showStatic;
        [HideInInspector] public string stampOverride; // hora a mostrar (cintas/archivo)
        [HideInInspector] public bool recLabel;     // REC en lugar de LIVE
        Material liveMat; Text tl, tr, center, bottom;

        void Start()
        {
            liveMat = new Material(Shader.Find("Unlit/Texture"));
            BuildOverlay();
            Apply();
        }

        void BuildOverlay()
        {
            if (screen == null) return;
            var go = new GameObject("Overlay", typeof(RectTransform));
            go.transform.SetParent(screen.transform, false);
            go.transform.localPosition = new Vector3(0, 0, -0.01f);
            go.transform.localRotation = Quaternion.identity;
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(400, 300);
            rt.localScale = new Vector3(1f / 400f, 1f / 300f, 1f);
            var c = go.AddComponent<Canvas>(); c.renderMode = RenderMode.WorldSpace;
            tl = UIKit.Txt("TL", rt, 22, TextAnchor.UpperLeft, Color.white, false); UIKit.Stretch(tl.rectTransform, 12, 0, 0, 8);
            tr = UIKit.Txt("TR", rt, 22, TextAnchor.UpperRight, Color.white, false); UIKit.Stretch(tr.rectTransform, 0, 0, 12, 8);
            center = UIKit.Txt("C", rt, 30, TextAnchor.MiddleCenter, Color.white, false); UIKit.Stretch(center.rectTransform, 10, 10, 10, 10);
            bottom = UIKit.Txt("B", rt, 18, TextAnchor.LowerLeft, UIKit.Green, false); UIKit.Stretch(bottom.rectTransform, 12, 8, 12, 0);
        }

        public void SetOn(bool v) { on = v; Apply(); }

        void Apply()
        {
            if (screen == null) return;
            Texture t = overrideTexture != null ? overrideTexture : (source != null ? source.rt : null);
            if (on && showStatic && staticMat) screen.sharedMaterial = staticMat;
            else if (on && t != null) { liveMat.mainTexture = t; screen.sharedMaterial = liveMat; }
            else screen.sharedMaterial = offMat;
        }

        void Update()
        {
            bool near = string.IsNullOrEmpty(requireZone) || PlayerController.I == null || RoomZone.At(PlayerController.I.transform.position) == requireZone;
            if (on && near && source != null && overrideTexture == null && SecurityCameraSystem.I != null && !showStatic) SecurityCameraSystem.I.Request(source);
            Apply();
            if (tl == null) return;
            bool show = on && near;
            tl.enabled = tr.enabled = center.enabled = bottom.enabled = show;
            if (!show) return;
            bool blink = Mathf.FloorToInt(Time.time * 2f) % 2 == 0;
            bool rec = recLabel || (source != null && source.frozen);
            tl.text = overrideTexture != null ? "PLAY ►" : (source != null ? source.camName : "");
            string stamp = !string.IsNullOrEmpty(stampOverride) ? stampOverride : (source != null && source.frozen ? source.frozenStamp : GameClock.Text);
            tr.text = (rec ? "<color=#ff4433>" + (blink ? "● " : "  ") + "REC</color>  " : "<color=#55ff77>" + (blink ? "● " : "  ") + "LIVE</color>  ") + stamp;
            center.text = message ?? "";
            bottom.text = "";
        }
    }
}
