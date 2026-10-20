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
            // con el teléfono abierto, [E] es del teléfono (abrir conversación / pista): no tocar el mundo
            if (Phone3D.IsOpen) { hud.SetPrompt(null); return; }

            // Escondido: [E] sale. Va ANTES de MoveBlocked, porque estar escondido ya bloquea el movimiento
            // (antes este caso nunca se alcanzaba y no se podía salir con E).
            if (HidingSpot.Current != null)
            {
                bool busy = PlayerController.CutsceneLock || PlayerController.TransitionLock || UIManager.ModalOpen || InspectSystem.Active || FocusSystem.Active
                            || (PhoneSystem.I != null && PhoneSystem.I.Ringing);   // si suena, E contesta y no te saca
                hud.SetPrompt(busy ? null : Settings.T("Salir del escondite", "Leave hiding spot"));
                if (!busy && Inp.Interact()) HidingSpot.Current.Interact();
                return;
            }

            if (PlayerController.MoveBlocked) { hud.SetPrompt(null); return; }
            // si suena el teléfono, [E] contesta (Phone3D) y no debe activar también lo que se esté mirando
            if (PhoneSystem.I != null && PhoneSystem.I.Ringing) { hud.SetPrompt(Settings.T("Contestar el teléfono", "Answer the phone")); return; }

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
