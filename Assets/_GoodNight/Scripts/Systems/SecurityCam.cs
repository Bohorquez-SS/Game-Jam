using UnityEngine;

namespace GNight
{
    /// <summary>Cámara de vigilancia: Camera + RenderTexture + luz infrarroja (solo durante su render).</summary>
    public class SecurityCam : MonoBehaviour
    {
        public string camName = "CAM 01";
        public string zoneId;
        public Camera cam;
        public Light irLight;
        [HideInInspector] public RenderTexture rt;
        /// <summary>Imagen congelada (archivo / "eco"): deja de actualizarse y muestra una hora vieja.</summary>
        public bool frozen;
        public string frozenStamp = "23:46";
    }
}
