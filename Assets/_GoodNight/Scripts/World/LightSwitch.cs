using UnityEngine;

namespace GNight
{
    /// <summary>Interruptor de luz de una habitación.</summary>
    public class LightSwitch : Interactable
    {
        public string zone;
        public Transform toggle;
        public override string Prompt => Settings.T("Usar interruptor", "Flip switch");
        public override void Interact()
        {
            if (AudioManager.I) AudioManager.I.At(AudioManager.I.click, transform.position, 0.7f);
            if (LightingManager.I) LightingManager.I.ToggleZone(zone);
            if (toggle) toggle.localRotation = Quaternion.Euler(LightingManager.I != null && LightingManager.I.ZoneSwitch(zone) ? -15f : 15f, 0, 0);
            if (LightingManager.I != null && !LightingManager.I.LightsPowered && HUDView.I)
                HUDView.I.Say("", Settings.T("No hay corriente en el circuito de iluminación.", "There's no power on the lighting circuit."), 2.5f);
        }
    }
}
