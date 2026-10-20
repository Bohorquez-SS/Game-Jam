using UnityEngine;

namespace GNight
{
    /// <summary>Computadora del dormitorio.</summary>
    public class Computer : Interactable
    {
        public Renderer screen; public Material onMat, offMat; public Light glow; public bool on = true;
        void Start() { SetOn(on); }
        public override string Prompt => on ? Settings.T("Apagar computadora", "Turn off computer") : Settings.T("Encender computadora", "Turn on computer");
        public override void Interact()
        {
            SetOn(!on);
            if (AudioManager.I) AudioManager.I.At(AudioManager.I.click, transform.position, 0.7f);
            GameEvents.Raise(on ? "pc_on" : "pc_off");
        }
        public void SetOn(bool v) { on = v; if (screen) screen.sharedMaterial = on ? onMat : offMat; if (glow) glow.enabled = on; }
    }
}
