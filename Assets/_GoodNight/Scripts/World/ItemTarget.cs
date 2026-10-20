using UnityEngine;

namespace GNight
{
    /// <summary>Lugar donde se usa un objeto del inventario (cerradura del armario, etc.).</summary>
    public class ItemTarget : Interactable
    {
        public string itemId;
        public string useEs = "Usar llave", useEn = "Use key";
        public string lookEs = "Examinar", lookEn = "Examine";
        [TextArea] public string noItemEs, noItemEn;
        public string eventId;
        public bool consume = true, used;
        public override bool CanInteract => !used;
        public override string Prompt => Inventory.Has(itemId) ? Settings.T(useEs, useEn) : Settings.T(lookEs, lookEn);
        public override void Interact()
        {
            if (!Inventory.Has(itemId)) { if (HUDView.I) HUDView.I.Say("", Settings.T(noItemEs, noItemEn), 4f); return; }
            used = true;
            if (consume) Inventory.Remove(itemId);
            if (AudioManager.I) AudioManager.I.At(AudioManager.I.lockClunk, transform.position, 1f);
            if (FPRig.I) FPRig.I.Reach(transform.position);
            GameEvents.Raise(eventId);
        }
    }
}
