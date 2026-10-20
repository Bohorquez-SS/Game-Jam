using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Modo "acercarse a un aparato": la cámara viaja a un punto de vista del panel (teclado, cuadro eléctrico,
    /// monitor, videograbadora…), el cursor se libera y se hace clic en sus botones 3D.
    /// </summary>
    public class FocusSystem : MonoBehaviour
    {
        public static FocusSystem I;
        public static bool Active => I != null && I.station != null;
        public static FocusStation Station => I != null ? I.station : null;
        public Camera mainCam;

        FocusStation station;
        Vector3 fromPos; Quaternion fromRot; Vector3 savedLocalPos; Quaternion savedLocalRot;
        float t; bool leaving; int enterFrame;
        Clickable hover;

        void Awake() { I = this; }

        public void Enter(FocusStation st)
        {
            if (station != null || st == null || st.view == null) return;
            station = st; leaving = false; t = 0f; enterFrame = Time.frameCount;
            var cam = mainCam.transform;
            savedLocalPos = cam.localPosition; savedLocalRot = cam.localRotation;
            fromPos = cam.position; fromRot = cam.rotation;
            PlayerController.ExternalCam = true;
            PS1Display.Boost = true;
            if (FPRig.I) FPRig.I.Reach(st.view.position + st.view.forward * 0.4f);
            st.OnEnter();
            GameEvents.Raise("focus:" + st.stationId);
        }

        public void Exit()
        {
            if (station == null || leaving) return;
            leaving = true; t = 0f;
            var cam = mainCam.transform;
            fromPos = cam.position; fromRot = cam.rotation;
            station.OnExit();
            if (HUDView.I) { HUDView.I.SetPrompt(null); HUDView.I.SetFocusHint(null); }
        }

        void Update()
        {
            if (station == null) return;
            var cam = mainCam.transform;
            t += Time.deltaTime / 0.55f;
            float k = Ease.InOut(t);
            if (!leaving)
            {
                cam.position = Vector3.Lerp(fromPos, station.view.position, k);
                cam.rotation = Quaternion.Slerp(fromRot, station.view.rotation, k);
                if (t >= 1f) Tick();
            }
            else
            {
                Vector3 toPos = cam.parent.TransformPoint(savedLocalPos);
                Quaternion toRot = cam.parent.rotation * savedLocalRot;
                cam.position = Vector3.Lerp(fromPos, toPos, k);
                cam.rotation = Quaternion.Slerp(fromRot, toRot, k);
                if (t >= 1f)
                {
                    cam.localPosition = savedLocalPos; cam.localRotation = savedLocalRot;
                    var st = station; station = null;
                    PlayerController.ExternalCam = false;
                    PS1Display.Boost = false;
                    GameEvents.Raise("unfocus:" + st.stationId);
                }
            }
        }

        void Tick()
        {
            station.OnFocusTick();
            if (station == null || leaving) return;
            // botones bajo el ratón
            Ray ray = MouseRay();
            Clickable c = null;
            if (Physics.Raycast(ray, out RaycastHit hit, 3f, ~0, QueryTriggerInteraction.Collide))
                c = hit.collider.GetComponentInParent<Clickable>();
            if (c != null && !c.interactable) c = null;
            if (c != hover) { if (hover) hover.SetHover(false); hover = c; if (hover) hover.SetHover(true); }
            if (HUDView.I)
            {
                HUDView.I.SetPrompt(hover != null ? hover.Label : null, false);
                HUDView.I.SetFocusHint(station.Hint);
            }
            if (hover != null && Inp.Click()) hover.Click();
            if (Time.frameCount != enterFrame && station.canExit && (Inp.Esc() || Inp.RightClick() || Inp.Q())) Exit();
        }

        public Ray MouseRay()
        {
            Vector2 m = Inp.MousePos();
            var rt = mainCam.targetTexture;
            if (rt != null) m = new Vector2(m.x / Mathf.Max(1, Screen.width) * rt.width, m.y / Mathf.Max(1, Screen.height) * rt.height);
            return mainCam.ScreenPointToRay(m);
        }
    }
}
