using UnityEngine;

namespace GNight
{
    /// <summary>Base práctica para interactuables MonoBehaviour.</summary>
    public abstract class Interactable : MonoBehaviour, IInteractable
    {
        public virtual string Prompt => Settings.T("Interactuar", "Interact");
        public virtual bool CanInteract => true;
        public abstract void Interact();
    }
}
