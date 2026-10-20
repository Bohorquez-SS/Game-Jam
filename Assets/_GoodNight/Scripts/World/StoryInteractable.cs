using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>Interactuable genérico que delega a la historia mediante un evento (grabadora, palanca, salida...).</summary>
    public class StoryInteractable : Interactable
    {
        public string promptEs = "Usar", promptEn = "Use";
        public string eventId;
        public bool available = true;
        public bool once;
        public override bool CanInteract => available;
        public override string Prompt => Settings.T(promptEs, promptEn);
        public override void Interact()
        {
            GameEvents.Raise(eventId);
            if (once) available = false;
        }
    }
}
