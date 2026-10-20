using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GNight
{
    /// <summary>
    /// Renderiza las cámaras solo cuando alguien las está mirando (y a medio framerate).
    /// Permite "trucos" que solo existen en el monitor: grabación antigua (puerta abierta) y la figura fantasma.
    /// </summary>
    public class SecurityCameraSystem : MonoBehaviour
    {
        public static SecurityCameraSystem I;
        public SecurityCam[] cams;
        public SecurityCam hallCam;
        public int width = 192, height = 144;

        [Header("Grabación antigua (cámara del pasillo)")]
        public bool recordingMode = true;
        public DoorController recordingDoor;
        [Header("Figura que solo aparece en el monitor")]
        public Transform ghost;
        public Renderer[] ghostRenderers;
        public bool ghostVisible;

        readonly HashSet<SecurityCam> requested = new HashSet<SecurityCam>();
        int frame;
        bool doorOverridden; Quaternion savedDoorRot;

        void Awake()
        {
            I = this;
            foreach (var c in cams)
            {
                if (c == null || c.cam == null) continue;
                c.rt = new RenderTexture(width, height, 16) { filterMode = FilterMode.Point, name = "RT_" + c.camName };
                c.rt.Create();
                c.cam.targetTexture = c.rt;
                c.cam.enabled = false;
            }
            if (ghostRenderers != null) foreach (var r in ghostRenderers) if (r) r.enabled = false;
            Camera.onPreCull += BeginBuiltin;
            Camera.onPostRender += EndBuiltin;
            RenderPipelineManager.beginCameraRendering += BeginSRP;
            RenderPipelineManager.endCameraRendering += EndSRP;
        }

        void OnDestroy()
        {
            Camera.onPreCull -= BeginBuiltin;
            Camera.onPostRender -= EndBuiltin;
            RenderPipelineManager.beginCameraRendering -= BeginSRP;
            RenderPipelineManager.endCameraRendering -= EndSRP;
            foreach (var c in cams) if (c != null && c.rt != null) c.rt.Release();
        }

        public static bool Powered => PowerPanel.I == null || PowerPanel.I.On(Circuit.Cameras);

        /// <summary>Llamar cada frame mientras un monitor muestre esta cámara.</summary>
        public void Request(SecurityCam c) { if (c != null) requested.Add(c); }

        public SecurityCam ByZone(string zone) { foreach (var c in cams) if (c != null && c.zoneId == zone) return c; return null; }

        void LateUpdate()
        {
            frame++;
            for (int i = 0; i < cams.Length; i++)
            {
                var c = cams[i];
                if (c == null || c.cam == null) continue;
                c.cam.enabled = requested.Contains(c) && ((frame + i) % 2 == 0);
            }
            requested.Clear();
        }

        void BeginBuiltin(Camera c) { Begin(c); }
        void EndBuiltin(Camera c) { End(c); }
        void BeginSRP(ScriptableRenderContext ctx, Camera c) { Begin(c); }
        void EndSRP(ScriptableRenderContext ctx, Camera c) { End(c); }

        void Begin(Camera c)
        {
            if (hallCam == null || c != hallCam.cam) return;
            if (recordingMode && recordingDoor != null)
            {
                savedDoorRot = recordingDoor.transform.localRotation;
                recordingDoor.transform.localRotation = recordingDoor.OpenRotation;
                doorOverridden = true;
            }
            if (ghostVisible && ghostRenderers != null) foreach (var r in ghostRenderers) if (r) r.enabled = true;
        }

        void End(Camera c)
        {
            if (hallCam == null || c != hallCam.cam) return;
            if (doorOverridden && recordingDoor != null) { recordingDoor.transform.localRotation = savedDoorRot; doorOverridden = false; }
            if (ghostRenderers != null) foreach (var r in ghostRenderers) if (r) r.enabled = false;
        }
    }
}
