using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Aparato que se enciende y apaga con [E] (ventilador, lámparas, radio…). Lo que controla:
    /// · spinners: aspas que aceleran/frenan · hum: sonido en bucle que se funde · lamp: luz real (RoomLight.userOff)
    /// Siempre suena el clic del interruptor; si la lámpara no tiene corriente lo dice en vez de fingir que funciona.
    /// </summary>
    public class Toggleable : Interactable
    {
        public string nounEs = "aparato", nounEn = "device";
        public bool on = true;
        public Spinner[] spinners;
        public AudioSource hum;
        public RoomLight lamp;
        public Transform knob;            // pieza que se mueve al pulsar (opcional)
        public Vector3 knobOffset = new Vector3(0, -0.004f, 0);
        float humVol = -1f; Vector3 knobBase;

        public override string Prompt => on ? Settings.T("Apagar " + nounEs, "Turn off " + nounEn) : Settings.T("Encender " + nounEs, "Turn on " + nounEn);

        void Start()
        {
            if (hum) humVol = hum.volume;
            if (knob) knobBase = knob.localPosition;
            Apply(true);
        }

        public override void Interact()
        {
            on = !on;
            if (AudioManager.I) AudioManager.I.At(AudioManager.I.click, transform.position, 0.75f);
            Apply(false);
            GameEvents.Raise((on ? "toggled_on:" : "toggled_off:") + name);
            if (on && lamp != null && !lamp.alwaysPowered && LightingManager.I != null && !LightingManager.I.LightsPowered && HUDView.I)
                HUDView.I.Say("", Settings.T("La lámpara no enciende: no hay corriente en las luces.", "The lamp won't turn on: there's no power on the lights."), 2.5f);
        }

        public void SetOn(bool v) { on = v; Apply(false); }

        void Apply(bool instant)
        {
            if (spinners != null) foreach (var s in spinners) if (s) s.on = on;
            if (lamp) { lamp.userOff = !on; if (LightingManager.I) LightingManager.I.ApplyAll(); }
            if (knob) knob.localPosition = knobBase + (on ? Vector3.zero : knobOffset);
            if (instant && hum && humVol >= 0f) hum.volume = on ? humVol : 0f;
        }

        void Update()
        {
            if (hum && humVol >= 0f) hum.volume = Mathf.MoveTowards(hum.volume, on ? humVol : 0f, Time.deltaTime * humVol * 1.2f);
        }
    }
}
