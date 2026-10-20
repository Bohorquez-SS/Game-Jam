using UnityEngine;

namespace GNight
{
    /// <summary>Linterna para recoger.</summary>
    public class FlashlightPickup : Interactable
    {
        public override string Prompt => Settings.T("Tomar linterna", "Take flashlight");
        public override void Interact()
        {
            if (FlashlightController.I) FlashlightController.I.Give(true);
            if (HUDView.I) HUDView.I.Hint(Settings.T("[F] Encender / apagar linterna", "[F] Toggle flashlight"), 5f);
            GameEvents.Raise("flashlight_taken");
            gameObject.SetActive(false);
        }
    }
}
