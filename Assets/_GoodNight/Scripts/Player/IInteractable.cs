using UnityEngine;

namespace GNight
{
    /// <summary>Contrato común de todo lo que se usa con [E].</summary>
    public interface IInteractable
    {
        string Prompt { get; }
        bool CanInteract { get; }
        void Interact();
    }
}
