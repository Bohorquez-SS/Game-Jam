using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>Escondite (armario/despensa/casillero). Dentro solo se mira entre las rendijas.</summary>
    public class HidingSpot : Interactable
    {
        public static HidingSpot Current;
        public static float HiddenSince;
        public string nameEs = "armario", nameEn = "wardrobe";
        public Transform inside, outside;
        public float insideEye = 1.5f, lookLimit = 50f;
        bool busy;

        void Awake() { Current = null; }

        public override string Prompt => Settings.T("Esconderse en el ", "Hide in the ") + Settings.T(nameEs, nameEn);
        public override bool CanInteract => !busy;
        public override void Interact() { if (!busy) StartCoroutine(Toggle()); }

        IEnumerator Toggle()
        {
            busy = true;
            var pc = PlayerController.I;
            PlayerController.TransitionLock = true;
            if (AudioManager.I) AudioManager.I.At(AudioManager.I.drawer, transform.position, 0.4f);
            yield return HUDView.I.FadeTo(1f, 0.2f);
            if (Current == this)
            {
                pc.SetPose(outside.position, outside.eulerAngles.y, 0f, -1f);
                pc.LookLimit = 0f; Current = null;
                GameEvents.Raise("hide_exit");
            }
            else
            {
                pc.SetPose(inside.position, inside.eulerAngles.y, 6f, insideEye);
                pc.LookLimit = lookLimit; Current = this; HiddenSince = Time.time;
                GameEvents.Raise("hide_enter");
            }
            yield return HUDView.I.FadeTo(0f, 0.25f);
            PlayerController.TransitionLock = false;
            busy = false;
        }

        /// <summary>Sacar al jugador de golpe (muerte, cinemática).</summary>
        public static void ForceExit()
        {
            if (Current == null) return;
            var pc = PlayerController.I;
            if (pc) { pc.SetPose(Current.outside.position, Current.outside.eulerAngles.y, 0f, -1f); pc.LookLimit = 0f; }
            Current = null;
        }
    }
}
