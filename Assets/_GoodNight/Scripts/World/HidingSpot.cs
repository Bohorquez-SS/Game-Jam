using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Escondite físico: se abre la puerta, la cámara entra, la puerta se cierra y solo se ve entre las rendijas.
    /// Salir mientras la entidad está justo delante = te atrapa. Puede reproducir una respiración DENTRO.
    /// </summary>
    public class HidingSpot : Interactable
    {
        public static HidingSpot Current;
        public static float HiddenSince;
        public string nameEs = "armario", nameEn = "wardrobe";
        public Transform inside, outside;
        public Transform[] doors;
        public float[] doorOpenAngles;
        public float insideEye = 1.5f, lookLimit = 45f;
        public AudioSource insideBreath;
        bool busy;
        Quaternion[] closed;

        void Awake()
        {
            Current = null;
            if (doors != null)
            {
                closed = new Quaternion[doors.Length];
                for (int i = 0; i < doors.Length; i++) if (doors[i]) closed[i] = doors[i].localRotation;
            }
        }

        public override string Prompt => Current == this ? Settings.T("Salir", "Get out") : Settings.T("Esconderse en el ", "Hide in the ") + Settings.T(nameEs, nameEn);
        public override bool CanInteract => !busy;
        public override void Interact() { if (!busy) StartCoroutine(Current == this ? Exit() : Enter()); }

        IEnumerator Doors(float from, float to, float dur)
        {
            if (doors == null) yield break;
            for (float t = 0; t < dur; t += Time.deltaTime)
            {
                float k = Ease.InOut(Mathf.Lerp(from, to, t / dur));
                for (int i = 0; i < doors.Length; i++) if (doors[i]) doors[i].localRotation = closed[i] * Quaternion.Euler(0, doorOpenAngles[i] * k, 0);
                yield return null;
            }
            for (int i = 0; i < doors.Length; i++) if (doors[i]) doors[i].localRotation = closed[i] * Quaternion.Euler(0, doorOpenAngles[i] * to, 0);
        }

        IEnumerator Enter()
        {
            busy = true;
            var pc = PlayerController.I;
            PlayerController.TransitionLock = true;
            if (AudioManager.I) AudioManager.I.At(AudioManager.I.woodCreak, transform.position, 0.45f);
            yield return Doors(0, 1, 0.35f);
            float py = pc.Pitch;
            yield return pc.MoveCamera(inside.position, inside.eulerAngles.y, 4f, insideEye, 0f, 0.65f, false);   // sin colisión mientras está escondido
            Current = this; HiddenSince = Time.time; pc.LookLimit = lookLimit;
            if (AudioManager.I) AudioManager.I.At(AudioManager.I.doorClose, transform.position, 0.3f);
            yield return Doors(1, 0, 0.4f);
            PlayerController.TransitionLock = false;
            GameEvents.Raise("hide_enter");
            busy = false;
        }

        IEnumerator Exit()
        {
            busy = true;
            var pc = PlayerController.I;
            bool danger = EnemyAI.I != null && EnemyAI.I.Guarding(this);
            PlayerController.TransitionLock = true;
            if (AudioManager.I) AudioManager.I.At(AudioManager.I.woodCreak, transform.position, 0.5f);
            yield return Doors(0, 1, 0.3f);
            if (danger)
            {
                // te saca del mueble (con colisión de nuevo) y la entidad te atrapa delante de la puerta
                pc.SetPose(outside.position, outside.eulerAngles.y, 0f, -1f); pc.LookLimit = 0f; pc.ReleaseEyeOverride();
                PlayerController.TransitionLock = false; busy = false; Current = null; GameEvents.Raise("hide_caught"); yield break;
            }
            yield return pc.MoveCamera(outside.position, outside.eulerAngles.y, 0f, pc.standEye, 0f, 0.55f);
            pc.LookLimit = 0f; Current = null; pc.ReleaseEyeOverride();
            yield return Doors(1, 0, 0.35f);
            PlayerController.TransitionLock = false;
            GameEvents.Raise("hide_exit");
            busy = false;
        }

        /// <summary>Sacar al jugador de golpe (muerte / cinemática).</summary>
        public static void ForceExit()
        {
            if (Current == null) return;
            var pc = PlayerController.I;
            if (pc) { pc.SetPose(Current.outside.position, Current.outside.eulerAngles.y, 0f, -1f); pc.LookLimit = 0f; }
            Current = null;
        }

        public void PlayInsideBreath(float volume)
        {
            if (insideBreath == null || AudioManager.I == null) return;
            insideBreath.clip = AudioManager.I.breathClose != null ? AudioManager.I.breathClose : AudioManager.I.breath;
            insideBreath.volume = volume * Settings.Sfx; insideBreath.loop = false; insideBreath.Play();
        }
    }
}
