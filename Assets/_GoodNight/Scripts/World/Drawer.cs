using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Cajón / puerta de mueble / tapa: se desliza o gira. Puede contener objetos examinables
    /// (solo se pueden tomar con el mueble abierto).
    /// </summary>
    public class Drawer : Interactable
    {
        public string id = "drawer";
        public Transform slide;
        public Vector3 openOffset = new Vector3(0, 0, 0.3f);
        public bool rotate;                       // true = gira (puerta / tapa)
        public Vector3 openEuler = new Vector3(0, -95f, 0);
        public bool isOpen;
        public string nounEs = "cajón", nounEn = "drawer";
        public bool locked;
        [TextArea] public string lockedEs = "Está trabado.", lockedEn = "It's stuck.";
        public Inspectable[] contents;
        Vector3 basePos; Quaternion baseRot; float t; bool init;

        void Init() { if (init) return; init = true; basePos = slide.localPosition; baseRot = slide.localRotation; }
        void Awake() { Init(); SetContents(isOpen); }

        public override string Prompt => (isOpen ? Settings.T("Cerrar ", "Close ") : Settings.T("Abrir ", "Open ")) + Settings.T(nounEs, nounEn);

        public override void Interact()
        {
            if (locked && !isOpen) { if (AudioManager.I) AudioManager.I.At(AudioManager.I.handleRattle, transform.position, 0.7f); if (HUDView.I) HUDView.I.Say("", Settings.T(lockedEs, lockedEn), 3f); return; }
            SetOpen(!isOpen);
            if (FPRig.I) FPRig.I.Reach(slide.position);
        }

        public void SetOpen(bool open, bool instant = false)
        {
            Init();
            if (open != isOpen && !instant && AudioManager.I) AudioManager.I.At(AudioManager.I.drawer, transform.position, 0.8f);
            isOpen = open;
            if (instant) { t = open ? 1f : 0f; Pose(); }
            SetContents(open);
            if (open) GameEvents.Raise("drawer_opened:" + id);
        }

        void SetContents(bool open)
        {
            if (contents == null) return;
            foreach (var c in contents) if (c) c.available = open;
        }

        void Update()
        {
            float target = isOpen ? 1 : 0;
            if (Mathf.Approximately(t, target)) return;
            t = Mathf.MoveTowards(t, target, Time.deltaTime * 3.5f);
            Pose();
        }

        void Pose()
        {
            float k = Mathf.SmoothStep(0, 1, t);
            if (rotate) slide.localRotation = baseRot * Quaternion.Euler(openEuler * k);
            else slide.localPosition = basePos + openOffset * k;
        }
    }
}
