using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GNight
{
    /// <summary>Cámara de vigilancia: Camera + RenderTexture de baja resolución.</summary>
    public class SecurityCam : MonoBehaviour
    {
        public string camName = "CAM 01";
        public string zoneId;
        public Camera cam;
        [HideInInspector] public RenderTexture rt;
    }
}
