using UnityEngine;

namespace GNight
{
    /// <summary>Cerrojo interior de la puerta principal.</summary>
    public class DeadBolt : Interactable
    {
        public DoorController door;
        public Transform boltVisual;
        public bool closed;
        public bool interactable = true;
        public AudioClip clip;
        Vector3 basePos;

        void Awake() { if (boltVisual) basePos = boltVisual.localPosition; Apply(); }
        public override bool CanInteract => interactable;
        public override string Prompt => closed ? Settings.T("Abrir cerrojo", "Unlock deadbolt") : Settings.T("Cerrar cerrojo", "Lock deadbolt");

        public override void Interact()
        {
            SetClosed(!closed);
            if (AudioManager.I) AudioManager.I.At(clip, transform.position, 1f);
            GameEvents.Raise(closed ? "bolt_closed" : "bolt_opened");
        }

        public void SetClosed(bool c) { closed = c; Apply(); }

        void Apply()
        {
            if (boltVisual) boltVisual.localPosition = basePos + new Vector3(closed ? -0.05f : 0f, 0, 0);
            if (door) door.locked = closed || GuardianLock || KeyLocked;
        }

        /// <summary>Cerradura con llave (se echa al cerrar la puerta en el Acto 1).</summary>
        public bool KeyLocked;
        public void SetKeyLocked(bool v)
        {
            KeyLocked = v;
            if (door && !GuardianLock) { door.lockedEs = "Está cerrada con llave."; door.lockedEn = "It's locked."; }
            Apply();
        }

        /// <summary>GUARDIÁN bloquea la puerta (Acto 3).</summary>
        public bool GuardianLock;
        public void SetGuardianLock(bool v)
        {
            GuardianLock = v;
            if (door)
            {
                door.lockedEs = v ? "La cerradura electrónica no responde. GUARDIÁN la mantiene bloqueada." : "Está cerrada.";
                door.lockedEn = v ? "The electronic lock won't respond. GUARDIAN is keeping it locked." : "It's locked.";
            }
            Apply();
        }
    }
}
