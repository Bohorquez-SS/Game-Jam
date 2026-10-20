using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Capa en primera persona: una segunda cámara (solo capa GN_FirstPerson) renderiza a resolución completa
    /// las manos, el teléfono, la linterna y los objetos inspeccionados, encima de la imagen retro del mundo.
    /// También anima la mano derecha al interactuar.
    /// </summary>
    public class FPRig : MonoBehaviour
    {
        public static FPRig I;
        public Camera fpCam;
        public Transform rightHand, leftHand;
        public Transform flashlightModel;
        public Light flashlightLight;
        [Header("Poses locales (respecto a la cámara)")]
        public Vector3 rightRest = new Vector3(0.24f, -0.42f, 0.32f);
        public Vector3 rightFlash = new Vector3(0.2f, -0.2f, 0.36f);
        public Vector3 rightHidden = new Vector3(0.3f, -0.7f, 0.2f);

        RenderTexture rt; int lastW, lastH;
        Vector3 rightTarget; Quaternion rightRot;
        float reachT = -1f; Vector3 reachLocal;
        float swayX, swayY;

        void Awake()
        {
            I = this;
            rightTarget = rightHidden;
            if (rightHand) rightHand.localPosition = rightHidden;
        }

        void Start() { Rebuild(); }

        void Rebuild()
        {
            lastW = Screen.width; lastH = Screen.height;
            if (rt != null) { fpCam.targetTexture = null; rt.Release(); Destroy(rt); }
            rt = new RenderTexture(Mathf.Max(64, Screen.width), Mathf.Max(64, Screen.height), 24, RenderTextureFormat.ARGB32) { name = "FP_RT", antiAliasing = 1 };
            rt.Create();
            ClearRT();
            fpCam.targetTexture = rt;
            if (HUDView.I) HUDView.I.SetFP(rt);
        }

        /// <summary>Limpia la capa en primera persona cada frame (algunos backends no la limpian si la cámara no dibuja nada).</summary>
        void ClearRT()
        {
            if (rt == null) return;
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            RenderTexture.active = prev;
        }

        /// <summary>Animación breve de la mano hacia un punto del mundo (abrir, pulsar, tomar).</summary>
        public void Reach(Vector3 worldPoint)
        {
            if (rightHand == null || Phone3D.IsOpen) return;
            Vector3 local = fpCam.transform.InverseTransformPoint(worldPoint);
            local = Vector3.ClampMagnitude(local, 0.55f);
            local.z = Mathf.Clamp(local.z, 0.25f, 0.5f);
            reachLocal = Vector3.Lerp(rightRest, local, 0.75f);
            reachT = 0f;
        }

        void LateUpdate()
        {
            if (Screen.width != lastW || Screen.height != lastH) Rebuild();
            ClearRT();
            if (rightHand == null) return;
            bool flash = FlashlightController.I != null && FlashlightController.I.IsOn;
            bool hide = InspectSystem.Active || FocusSystem.Active || PlayerController.CutsceneLock || HidingSpot.Current != null;
            Vector3 baseTarget = hide ? rightHidden : (flash ? rightFlash : rightHidden);
            if (flashlightModel) flashlightModel.gameObject.SetActive(flash && !hide);

            // balanceo con el movimiento de la cámara
            Vector2 look = PlayerController.LookBlocked ? Vector2.zero : Inp.Look();
            swayX = Mathf.Lerp(swayX, -look.x * 0.004f, Time.deltaTime * 6f);
            swayY = Mathf.Lerp(swayY, -look.y * 0.004f, Time.deltaTime * 6f);
            var pc = PlayerController.I;
            float bob = pc != null && pc.IsMoving ? Mathf.Sin(Time.time * (pc.IsRunning ? 11f : 7.5f)) * 0.008f : Mathf.Sin(Time.time * 1.6f) * 0.003f;
            Vector3 p = baseTarget + new Vector3(swayX, swayY + bob, 0);

            if (reachT >= 0f)
            {
                reachT += Time.deltaTime / 0.42f;
                float k = reachT < 0.5f ? Ease.Out(reachT * 2f) : 1f - Ease.InOut((reachT - 0.5f) * 2f);
                p = Vector3.Lerp(p, reachLocal, k);
                if (reachT >= 1f) reachT = -1f;
            }
            rightHand.localPosition = Vector3.Lerp(rightHand.localPosition, p, Time.deltaTime * 14f);
            rightHand.localRotation = Quaternion.Slerp(rightHand.localRotation, Quaternion.Euler(flash ? -6f : 10f, -8f, -12f), Time.deltaTime * 10f);
        }
    }
}
