using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>Cama: dormir (con confirmación).</summary>
    public class Bed : Interactable
    {
        public override string Prompt => Settings.T("Dormir", "Sleep");
        public override bool CanInteract => StoryManager.I == null || StoryManager.I.BedAvailable;
        public override void Interact()
        {
            if (StoryManager.I == null) return;
            if (!StoryManager.I.CanSleep)
            {
                HUDView.I.Say(Settings.T("Tú", "You"), Settings.T("Todavía no. Mamá me pidió que asegurara la casa.", "Not yet. Mom asked me to lock up the house."), 3f);
                return;
            }
            UIManager.I.Get<ConfirmPanel>().Ask(Settings.T("¿INTENTAR DORMIR?", "TRY TO SLEEP?"), () => GameEvents.Raise("bed_sleep"));
        }
    }
}
