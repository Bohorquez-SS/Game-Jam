using UnityEngine;

namespace GNight
{
    /// <summary>Raycast desde la cámara: muestra "[E] acción" solo si el objeto está cerca y a la vista.</summary>
    public class InteractionSystem : MonoBehaviour
    {
        public static InteractionSystem I;
        public Transform cam;
        public float reach = 2.3f;
        public IInteractable Current { get; private set; }

        void Awake() { I = this; }

        void Update()
        {
            Current = null;
            var hud = HUDView.I;
            if (hud == null) return;
            if (PlayerController.MoveBlocked) { hud.SetPrompt(null); return; }

            if (HidingSpot.Current != null)
            {
                hud.SetPrompt(Settings.T("Salir del escondite", "Leave hiding spot"));
                if (Inp.Interact()) HidingSpot.Current.Interact();
                return;
            }

            if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, reach, ~0, QueryTriggerInteraction.Ignore))
            {
                var mb = hit.collider.GetComponentInParent<Interactable>();
                if (mb != null && mb.enabled && mb.CanInteract) Current = mb;
            }

            if (Current != null)
            {
                hud.SetPrompt(Current.Prompt);
                if (Inp.Interact()) Current.Interact();
            }
            else hud.SetPrompt(null);
        }
    }
}
