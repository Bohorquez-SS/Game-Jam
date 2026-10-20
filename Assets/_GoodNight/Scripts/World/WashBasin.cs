using UnityEngine;

namespace GNight
{
    /// <summary>Lavabo del baño: [E] Lavarse la cara (una sola vez, cap. 1). La escena la dirige StoryManager ("wash_face").</summary>
    public class WashBasin : Interactable
    {
        public Transform standPose;     // dónde se para el jugador (mirando al espejo)
        public Transform tap;           // grifo (origen del sonido del agua)
        public override string Prompt => Settings.T("Lavarse la cara", "Wash your face");
        public override bool CanInteract => StoryManager.I != null && StoryManager.I.WashAvailable;
        public override void Interact()
        {
            if (!CanInteract) return;
            GameEvents.Raise("wash_face");
        }
    }
}
