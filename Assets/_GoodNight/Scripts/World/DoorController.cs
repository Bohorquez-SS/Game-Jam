using UnityEngine;

namespace GNight
{
    /// <summary>Puerta con bisagra (este objeto = bisagra). Animada, con sonidos, bloqueo y ganchos para la historia.</summary>
    public class DoorController : Interactable
    {
        public string id = "door";
        public string displayEs = "puerta", displayEn = "door";
        public float openAngle = 95f, speed = 2.6f;
        public bool isOpen;
        public bool locked;
        public string lockedEs = "Está cerrada.", lockedEn = "It's locked.";
        public AudioClip openClip, closeClip, lockedClip;
        /// <summary>Antes de abrir (jugador). Devuelve true para cancelar.</summary>
        public System.Func<bool> BeforeOpen;

        Quaternion closedRot; float t; bool init;
        public Quaternion OpenRotation { get { Init(); return closedRot * Quaternion.Euler(0, openAngle, 0); } }
        public float OpenAmount => t;

        void Init() { if (init) return; init = true; closedRot = transform.localRotation; t = isOpen ? 1 : 0; Apply(); }
        void Awake() { Init(); }

        string Name => Settings.T(displayEs, displayEn);
        public override string Prompt => locked ? Settings.T("Abrir ", "Open ") + Name : (isOpen ? Settings.T("Cerrar ", "Close ") : Settings.T("Abrir ", "Open ")) + Name;

        public override void Interact()
        {
            if (!isOpen && BeforeOpen != null && BeforeOpen()) return;
            if (locked && !isOpen)
            {
                if (AudioManager.I) AudioManager.I.At(lockedClip, transform.position, 0.8f);
                if (HUDView.I) HUDView.I.Say("", Settings.T(lockedEs, lockedEn), 2.5f);
                return;
            }
            SetOpen(!isOpen);
        }

        public void SetOpen(bool open, bool instant = false)
        {
            Init();
            if (open != isOpen && !instant && AudioManager.I) AudioManager.I.At(open ? openClip : closeClip, transform.position, 0.9f);
            bool changed = open != isOpen;
            isOpen = open;
            if (instant) { t = open ? 1 : 0; Apply(); }
            if (changed) GameEvents.Raise((open ? "door_opened:" : "door_closed:") + id);
        }

        void Update()
        {
            float target = isOpen ? 1 : 0;
            if (!Mathf.Approximately(t, target)) { t = Mathf.MoveTowards(t, target, Time.deltaTime * speed); Apply(); }
        }

        void Apply() { transform.localRotation = closedRot * Quaternion.Euler(0, openAngle * Mathf.SmoothStep(0, 1, t), 0); }
    }
}
