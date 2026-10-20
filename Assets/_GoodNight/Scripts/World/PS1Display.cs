using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Imagen retro: la cámara del mundo renderiza a una textura de resolución reducida (según calidad)
    /// que el HUD estira. Boost = resolución completa (paneles y lectura de cerca).
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class PS1Display : MonoBehaviour
    {
        public static bool Boost;
        Camera cam; RenderTexture rt; int lastW, lastH, lastTarget = -1;

        void Start()
        {
            cam = GetComponent<Camera>();
            QualitySettings.pixelLightCount = Mathf.Max(QualitySettings.pixelLightCount, 8);
            QualitySettings.shadowDistance = Mathf.Max(QualitySettings.shadowDistance, 30f);
            Rebuild();
        }

        int Target => Boost ? Mathf.Max(720, Settings.RenderHeight) : Settings.RenderHeight;

        void Rebuild()
        {
            lastW = Screen.width; lastH = Screen.height; lastTarget = Target;
            int h = Mathf.Min(Mathf.Max(200, lastTarget), Mathf.Max(200, Screen.height));
            int w = Mathf.Max(1, Mathf.RoundToInt(h * (float)Screen.width / Mathf.Max(1, Screen.height)));
            Release();
            rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32) { filterMode = Settings.Quality == 2 ? FilterMode.Bilinear : FilterMode.Point, name = "World_RT" };
            rt.Create();
            cam.targetTexture = rt;
            if (UIManager.I) UIManager.I.SetWorldTexture(rt);
        }

        void Update()
        {
            if (Screen.width != lastW || Screen.height != lastH || Target != lastTarget) Rebuild();
        }

        void Release()
        {
            if (rt == null) return;
            cam.targetTexture = null; rt.Release(); Destroy(rt); rt = null;
        }

        void OnDestroy() { if (cam != null) Release(); }
    }
}
