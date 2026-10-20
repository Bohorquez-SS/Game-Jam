using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>Mensajes del teléfono: historial, notificaciones y mensajes "anómalos" que desaparecen.</summary>
    public class PhoneSystem : MonoBehaviour
    {
        public static PhoneSystem I;
        public readonly List<PhoneMessage> Messages = new List<PhoneMessage>();
        public event System.Action Changed;
        public int Unread { get; private set; }

        public static string Mom => Settings.T("Mamá", "Mom");
        public static string Unknown => Settings.T("Número desconocido", "Unknown number");
        public const string Guardian = "GUARDIÁN";

        void Awake() { I = this; }

        public PhoneMessage Receive(string from, string text, bool notify = true, float vanishAfter = -1f)
        {
            var m = new PhoneMessage { from = from, time = GameClock.Text, text = text, fromUnknown = from == Unknown, fromSystem = from == Guardian };
            Messages.Add(m);
            Unread++;
            if (notify)
            {
                if (AudioManager.I) AudioManager.I.UI(AudioManager.I.vibrate, 0.9f);
                if (HUDView.I) HUDView.I.Toast(from + " · " + m.time, text);
            }
            if (Changed != null) Changed();
            if (vanishAfter > 0f) StartCoroutine(Vanish(m, vanishAfter));
            return m;
        }

        /// <summary>Agrega al historial sin notificar (reconstrucción al cargar un punto de control).</summary>
        public void AddHistory(string from, string time, string text)
        {
            Messages.Add(new PhoneMessage { from = from, time = time, text = text, fromUnknown = from == Unknown, fromSystem = from == Guardian });
            if (Changed != null) Changed();
        }

        IEnumerator Vanish(PhoneMessage m, float delay)
        {
            yield return new WaitForSeconds(delay);
            Messages.Remove(m);
            if (Unread > 0) Unread--;
            if (AudioManager.I) AudioManager.I.UI(AudioManager.I.staticBurst, 0.35f);
            if (Changed != null) Changed();
        }

        public void MarkRead() { Unread = 0; }
    }
}
