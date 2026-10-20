using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Renderiza cada cámara solo cuando un monitor la está mostrando (y a medio framerate) con luz IR propia,
    /// para que la imagen sea legible aunque la casa esté oscura. Trucos de render: grabación antigua
    /// (puerta del baño abierta) y figuras que solo existen en los monitores (capa GN_CamOnly).
    /// </summary>
    public class SecurityCameraSystem : MonoBehaviour
    {
        public static SecurityCameraSystem I;
        public SecurityCam[] cams;
        public SecurityCam hallCam;
        public int width = 768, height = 576;   // antes 320x240: no se distinguían personas ni objetos

        [Header("Grabación antigua (cámara del pasillo)")]
        public bool recordingMode = true;
        public DoorController recordingDoor;

        readonly HashSet<SecurityCam> requested = new HashSet<SecurityCam>();
        int frame;
        bool doorOverridden; Quaternion savedDoorRot;

        void Awake()
        {
            I = this;
            foreach (var c in cams)
            {
                if (c == null || c.cam == null) continue;
                c.rt = new RenderTexture(width, height, 16) { filterMode = FilterMode.Bilinear, name = "RT_" + c.camName };
                c.rt.Create();
                c.cam.targetTexture = c.rt;
                c.cam.enabled = false;
                c.cam.cullingMask = ~(GNLayers.Mask(GNLayers.FP) | GNLayers.Mask(GNLayers.Tape) | GNLayers.Mask(GNLayers.Mirror));
                if (c.irLight) c.irLight.enabled = false;
                var sc = c;
                RenderTricks.Add(c.cam, () => { if (sc.irLight) sc.irLight.enabled = true; Begin(sc); }, () => { if (sc.irLight) sc.irLight.enabled = false; End(sc); });
            }
        }

        void OnDestroy() { foreach (var c in cams) if (c != null && c.rt != null) c.rt.Release(); RenderTricks.ClearAll(); }

        public static bool Powered => PowerPanel.I == null || PowerPanel.I.On(Circuit.Cameras);

        public void Request(SecurityCam c) { if (c != null) requested.Add(c); }
        public SecurityCam ByZone(string zone) { foreach (var c in cams) if (c != null && c.zoneId == zone) return c; return null; }
        public int IndexOf(SecurityCam c) { for (int i = 0; i < cams.Length; i++) if (cams[i] == c) return i; return -1; }

        /// <summary>Fuerza un render inmediato (para congelar una imagen).</summary>
        public void Snapshot(SecurityCam c) { if (c != null && c.cam != null) c.cam.Render(); }

        void LateUpdate()
        {
            frame++;
            for (int i = 0; i < cams.Length; i++)
            {
                var c = cams[i];
                if (c == null || c.cam == null) continue;
                c.cam.enabled = !c.frozen && requested.Contains(c) && ((frame + i) % 2 == 0);
            }
            requested.Clear();
        }

        void Begin(SecurityCam c)
        {
            if (c == hallCam && recordingMode && recordingDoor != null)
            {
                savedDoorRot = recordingDoor.transform.localRotation;
                recordingDoor.transform.localRotation = recordingDoor.OpenRotation;
                doorOverridden = true;
            }
        }

        void End(SecurityCam c)
        {
            if (doorOverridden && recordingDoor != null) { recordingDoor.transform.localRotation = savedDoorRot; doorOverridden = false; }
        }
    }
}
