using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Mantiene la textura de la fuente dinámica en el material de texto con profundidad (GNight/Text3D): cuando Unity
    /// reconstruye el atlas de la fuente, el material propio debe apuntar a la textura nueva.
    /// </summary>
    [RequireComponent(typeof(TextMesh))]
    public class Text3DDepth : MonoBehaviour
    {
        TextMesh tm; Renderer r;
        void Awake() { tm = GetComponent<TextMesh>(); r = GetComponent<Renderer>(); Sync(); Font.textureRebuilt += OnRebuilt; }
        void OnDestroy() { Font.textureRebuilt -= OnRebuilt; }
        void OnRebuilt(Font f) { if (tm != null && f == tm.font) Sync(); }
        void Sync()
        {
            if (tm == null || tm.font == null || r == null || r.sharedMaterial == null) return;
            var t = tm.font.material != null ? tm.font.material.mainTexture : null;
            if (t != null && r.sharedMaterial.mainTexture != t) r.sharedMaterial.mainTexture = t;
        }
    }
}
