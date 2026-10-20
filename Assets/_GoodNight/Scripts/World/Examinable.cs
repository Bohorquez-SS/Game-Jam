using UnityEngine;

namespace GNight
{
    /// <summary>Objeto que solo se observa (descripción en subtítulo).</summary>
    public class Examinable : Interactable
    {
        public string nameEs = "objeto", nameEn = "object";
        [TextArea] public string textEs, textEn;
        public string eventId;
        public override string Prompt => Settings.T("Examinar ", "Examine ") + Settings.T(nameEs, nameEn);
        public override void Interact()
        {
            if (HUDView.I) HUDView.I.Say("", Settings.T(textEs, textEn), 4.5f);
            if (!string.IsNullOrEmpty(eventId)) GameEvents.Raise(eventId);
        }
    }
}
