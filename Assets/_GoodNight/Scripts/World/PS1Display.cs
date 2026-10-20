using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>Look PS1: render interno de baja resolución (point filter) mostrado por el HUD.</summary>
    [RequireComponent(typeof(Camera))]
    public class PS1Display : MonoBehaviour
    {
        public int targetHeight = 240;
        Camera cam; RenderTexture rt; int lastW, lastH, lastT;
        void Start()
        {
            cam = GetComponent<Camera>();
            QualitySettings.pixelLightCount = Mathf.Max(QualitySettings.pixelLightCount, 8);
            QualitySettings.shadowDistance = Mathf.Max(QualitySettings.shadowDistance, 30f);
            Rebuild();
        }
        void Rebuild()
        {
            lastW = Screen.width; lastH = Screen.height; lastT = targetHeight;
            int h = Mathf.Max(120, targetHeight);
            int w = Mathf.Max(1, Mathf.RoundToInt(h * (float)Screen.width / Mathf.Max(1, Screen.height)));
            Release();
            rt = new RenderTexture(w, h, 24) { filterMode = FilterMode.Point, name = "PS1_RT" };
            rt.Create();
            cam.targetTexture = rt;
            if (UIManager.I) UIManager.I.SetWorldTexture(rt);
        }
        void Update() { if (Screen.width != lastW || Screen.height != lastH || targetHeight != lastT) Rebuild(); }
        void Release() { if (rt == null) return; cam.targetTexture = null; rt.Release(); Destroy(rt); rt = null; }
        void OnDestroy() { if (cam != null) Release(); }
    }
}
