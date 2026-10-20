using UnityEngine;

namespace GNight
{
    /// <summary>Ventana de guillotina: la hoja sube/baja, cambia el volumen de la lluvia y mueve las cortinas.</summary>
    public class WindowController : Interactable
    {
        public string id = "window";
        public Transform sash;
        public float openOffset = 0.45f;
        public bool isOpen = true;
        public AudioSource rain;
        public float rainOpen = 0.85f, rainClosed = 0.28f;
        public AudioClip slideClip, latchClip;
        public CurtainSway[] curtains;
        Vector3 closedPos; float t; bool init;

        void Init() { if (init) return; init = true; closedPos = sash.localPosition; t = isOpen ? 1 : 0; Apply(); }
        void Awake() { Init(); }

        public override string Prompt => isOpen ? Settings.T("Cerrar ventana", "Close window") : Settings.T("Abrir ventana", "Open window");
        public override void Interact() { SetOpen(!isOpen); }

        public void SetOpen(bool open, bool instant = false)
        {
            Init();
            bool changed = open != isOpen;
            if (changed && !instant && AudioManager.I)
            {
                AudioManager.I.At(slideClip, sash.position, 0.8f);
                if (!open) AudioManager.I.At(latchClip, sash.position, 0.7f);
            }
            isOpen = open;
            if (instant) { t = open ? 1 : 0; Apply(); }
            if (changed) GameEvents.Raise((open ? "window_opened:" : "window_closed:") + id);
        }

        void Update() { t = Mathf.MoveTowards(t, isOpen ? 1 : 0, Time.deltaTime * 1.8f); Apply(); }

        void Apply()
        {
            sash.localPosition = closedPos + Vector3.up * openOffset * Mathf.SmoothStep(0, 1, t);
            if (rain) rain.volume = Mathf.Lerp(0f, rainOpen * AudioManager.RainOpenScale, t) * Settings.Sfx;   // (18/10) sin lluvia constante: solo se oye, suave, con la ventana abierta
            if (curtains != null) foreach (var c in curtains) if (c) c.wind = t;
        }
    }
}
