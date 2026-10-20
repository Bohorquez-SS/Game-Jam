using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Espejo con "reflejo" del protagonista: una cámara al otro lado del plano renderiza a la superficie
    /// (volteada en X) y un cuerpo con tu ropa (capa GN_Mirror, invisible para el resto) imita tus movimientos.
    /// Anomalías: el reflejo va con retraso, o se queda mirándote cuando te das vuelta.
    /// </summary>
    public class MirrorReflection : MonoBehaviour
    {
        public Renderer surface;
        public Camera mirrorCam;
        public Transform body;          // modelo del doble (capa GN_Mirror)
        public CharacterAnim bodyAnim;
        public string zone = "bano";
        public float lag;               // segundos de retraso (0 = normal)
        public bool stare;              // la cabeza del reflejo sigue mirando al jugador
        [Header("Lavabo + susto del espejo (18/10)")]
        public WashBasin basin;
        public Transform scare;         // figura que SOLO aparece en el reflejo (capa GN_Mirror)
        public CharacterAnim scareAnim;
        public Transform scareFace;     // rostro (quad con textura) que aparece pegado al espejo
        RenderTexture rt; Material mat;
        readonly Queue<KeyValuePair<float, Pose>> history = new Queue<KeyValuePair<float, Pose>>();

        void Start()
        {
            rt = new RenderTexture(256, 320, 16) { name = "RT_Mirror" };
            rt.Create();
            mirrorCam.targetTexture = rt; mirrorCam.enabled = false;
            mirrorCam.cullingMask = ~(GNLayers.Mask(GNLayers.FP) | GNLayers.Mask(GNLayers.CamOnly) | GNLayers.Mask(GNLayers.Tape));
            mat = new Material(Shader.Find("Unlit/Texture")) { mainTexture = rt, mainTextureScale = new Vector2(-1, 1), mainTextureOffset = new Vector2(1, 0) };
            surface.sharedMaterial = mat;
            if (body) GNLayers.SetLayerRecursive(body.gameObject, GNLayers.Mirror);
            if (scare) { GNLayers.SetLayerRecursive(scare.gameObject, GNLayers.Mirror); scare.gameObject.SetActive(false); }
            if (scareFace) scareFace.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            var pc = PlayerController.I;
            bool active = pc != null && RoomZone.At(pc.transform.position) == zone;
            mirrorCam.enabled = active;
            if (body) body.gameObject.SetActive(active);
            if (!active) { history.Clear(); return; }
            Transform m = surface.transform;
            Vector3 n = -m.forward;           // la cara visible del quad mira hacia -forward
            Vector3 p0 = m.position;
            // cámara en el ojo reflejado, mirando hacia la habitación real (eso ES el reflejo)
            Vector3 eye = pc.EyePosition;
            float dEye = Vector3.Dot(eye - p0, n);
            Vector3 re = eye - 2f * dEye * n;
            Vector3 f = Vector3.Reflect(pc.cam.forward, n);
            if (Vector3.Dot(f, n) < 0.25f) f = (f + n * 0.6f).normalized; // siempre hacia la habitación
            mirrorCam.transform.position = re;
            mirrorCam.transform.rotation = Quaternion.LookRotation(f, Vector3.up);
            mirrorCam.nearClipPlane = Mathf.Max(0.05f, Mathf.Abs(dEye) / Mathf.Max(0.25f, Vector3.Dot(f, n)) + 0.02f);
            // el "cuerpo" está donde realmente estás (solo lo ve la cámara del espejo), con retraso opcional
            history.Enqueue(new KeyValuePair<float, Pose>(Time.time, new Pose(pc.transform.position, pc.transform.rotation)));
            Pose use = history.Peek().Value;
            while (history.Count > 1 && Time.time - history.Peek().Key > lag) { history.Dequeue(); use = history.Peek().Value; }
            if (lag <= 0.001f) use = new Pose(pc.transform.position, pc.transform.rotation);
            if (body) { body.position = use.position; body.rotation = use.rotation; }
            if (bodyAnim)
            {
                bodyAnim.Play(pc.IsMoving ? "Walk" : "Idle", 0.2f, 0.9f);
                if (stare) { bodyAnim.lookTarget = re; bodyAnim.lookWeight = 1f; } else bodyAnim.lookTarget = null;
            }
        }

        void OnDestroy() { if (rt) rt.Release(); }
    }
}
