using UnityEngine;

namespace GNight
{
    /// <summary>Revisar un lugar (bolsillo del abrigo, debajo de la cama): muestra el objeto escondido para examinarlo y guardarlo.</summary>
    public class SearchSpot : Interactable
    {
        public string promptEs = "Revisar", promptEn = "Search";
        public Inspectable hidden;
        [TextArea] public string emptyEs = "No hay nada más.", emptyEn = "Nothing else here.";
        public bool available = true;
        bool found;
        public override bool CanInteract => available;
        public override string Prompt => Settings.T(promptEs, promptEn);
        public override void Interact()
        {
            if (FPRig.I) FPRig.I.Reach(transform.position);
            if (found || hidden == null || Inventory.Has(hidden.id)) { if (HUDView.I) HUDView.I.Say("", Settings.T(emptyEs, emptyEn), 2.5f); return; }
            found = true;
            if (AudioManager.I) AudioManager.I.At(AudioManager.I.pocket, transform.position, 0.8f);
            if (InspectSystem.I) InspectSystem.I.Inspect(hidden);
            GameEvents.Raise("search:" + hidden.id);
        }
    }
}
