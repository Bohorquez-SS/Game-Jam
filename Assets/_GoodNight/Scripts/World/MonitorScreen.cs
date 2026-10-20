using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>Pantalla CRT del mundo que muestra una cámara de vigilancia.</summary>
    public class MonitorScreen : MonoBehaviour
    {
        public Renderer screen;
        public Material offMat;
        public SecurityCam source;
        public bool on;
        public string requireZone; // solo renderiza si el jugador está en esta zona
        Material liveMat;

        void Start()
        {
            if (screen) { liveMat = new Material(Shader.Find("Unlit/Texture")); }
            Apply();
        }

        public void SetOn(bool v) { on = v; Apply(); }

        void Apply()
        {
            if (screen == null) return;
            if (on && source != null && source.rt != null && liveMat != null) { liveMat.mainTexture = source.rt; screen.sharedMaterial = liveMat; }
            else screen.sharedMaterial = offMat;
        }

        void Update()
        {
            if (!on || source == null || SecurityCameraSystem.I == null) return;
            if (!string.IsNullOrEmpty(requireZone) && PlayerController.I != null && RoomZone.At(PlayerController.I.transform.position) != requireZone) return;
            SecurityCameraSystem.I.Request(source);
            if (liveMat != null && liveMat.mainTexture != source.rt) Apply();
        }
    }
}
