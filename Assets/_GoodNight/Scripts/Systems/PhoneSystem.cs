using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Conversaciones del teléfono: mamá, número desconocido y GUARDIÁN. Mensajes entrantes y salientes,
    /// registros de llamadas, mensajes que se borran solos y llamadas entrantes.
    /// </summary>
    public class PhoneSystem : MonoBehaviour
    {
        public static PhoneSystem I;
        public readonly List<PhoneMessage> Messages = new List<PhoneMessage>();
        public event System.Action Changed;
        public event System.Action<PhoneMessage> Arrived;

        public const string Mama = "mama", Unknown = "unknown", Guardian = "guardian";
        public static string Name(string thread)
        {
            switch (thread)
            {
                case Mama: return Settings.T("Mamá", "Mom");
                case Unknown: return Settings.T("Número desconocido", "Unknown number");
                default: return "GUARDIÁN";
            }
        }

        // llamada entrante
        public bool Ringing { get; private set; }
        public bool InCall { get; private set; }
        public string CallThread { get; private set; }
        public float CallTime { get; private set; }
        bool answered;

        void Awake() { I = this; }

        public PhoneMessage Receive(string thread, string text, bool notify = true, float vanishAfter = -1f, bool autoShow = true)
        {
            var m = new PhoneMessage { thread = thread, time = GameClock.Text, text = text, isNew = true };
            Messages.Add(m);
            if (notify)
            {
                if (AudioManager.I) AudioManager.I.UI(AudioManager.I.vibrate, 0.9f);
                if (Phone3D.I) Phone3D.I.Notify(m, autoShow);
                else if (HUDView.I) HUDView.I.Toast(Name(thread) + " · " + m.time, text);
            }
            Raise(m);
            if (vanishAfter > 0f) StartCoroutine(Vanish(m, vanishAfter));
            return m;
        }

        /// <summary>Mensaje que aparece como enviado desde este teléfono (sin que el jugador lo escriba).</summary>
        public PhoneMessage Outgoing(string thread, string time, string text)
        {
            var m = new PhoneMessage { thread = thread, time = time, text = text, outgoing = true, isNew = true };
            InsertByTime(m);
            Raise(m);
            return m;
        }

        public void CallLog(string thread, string time, bool outgoing, string duration)
        {
            var m = new PhoneMessage
            {
                thread = thread, time = time, outgoing = outgoing, isCall = true,
                text = (outgoing ? Settings.T("Llamada saliente", "Outgoing call") : Settings.T("Llamada entrante", "Incoming call")) + "  ·  " + duration
            };
            InsertByTime(m);
            Raise(m);
        }

        /// <summary>Historial sin notificar (reconstrucción al cargar un punto de control).</summary>
        public void AddHistory(string thread, string time, string text, bool outgoing = false)
        {
            InsertByTime(new PhoneMessage { thread = thread, time = time, text = text, outgoing = outgoing });
            if (Changed != null) Changed();
        }

        void InsertByTime(PhoneMessage m)
        {
            int key = Key(m.time), i = Messages.Count;
            while (i > 0 && Key(Messages[i - 1].time) > key) i--;
            Messages.Insert(i, m);
        }

        static int Key(string t)
        {
            int h = int.Parse(t.Substring(0, 2)), mi = int.Parse(t.Substring(3, 2));
            if (h < 12) h += 24; // la noche cruza medianoche
            return h * 60 + mi;
        }

        void Raise(PhoneMessage m)
        {
            if (Changed != null) Changed();
            if (Arrived != null) Arrived(m);
        }

        IEnumerator Vanish(PhoneMessage m, float delay)
        {
            yield return new WaitForSeconds(delay);
            m.text = Settings.T("<i>Este mensaje fue eliminado.</i>", "<i>This message was deleted.</i>");
            if (Changed != null) Changed();
            if (AudioManager.I) AudioManager.I.UI(AudioManager.I.staticBurst, 0.25f);
            yield return new WaitForSeconds(2.5f);
            Messages.Remove(m);
            if (Changed != null) Changed();
        }

        public List<PhoneMessage> Thread(string thread)
        {
            var l = new List<PhoneMessage>();
            foreach (var m in Messages) if (m.thread == thread) l.Add(m);
            return l;
        }

        public PhoneMessage Last(string thread)
        {
            for (int i = Messages.Count - 1; i >= 0; i--) if (Messages[i].thread == thread) return Messages[i];
            return null;
        }

        public bool HasNew(string thread) { foreach (var m in Messages) if (m.thread == thread && m.isNew) return true; return false; }
        public void MarkRead(string thread) { foreach (var m in Messages) if (m.thread == thread) m.isNew = false; if (Changed != null) Changed(); }
        public int UnreadCount { get { int n = 0; foreach (var m in Messages) if (m.isNew) n++; return n; } }

        // ------------------------------------------------------------------ llamadas
        /// <summary>Hace sonar el teléfono. Espera a que el jugador conteste (Tab / E) y devuelve.</summary>
        public IEnumerator Ring(string thread, float maxWait = 40f)
        {
            Ringing = true; answered = false; CallThread = thread;
            if (Changed != null) Changed();
            if (Phone3D.I) Phone3D.I.ShowCall();
            float t = 0f, ringT = 0f;
            while (!answered && t < maxWait)
            {
                t += Time.deltaTime; ringT -= Time.deltaTime;
                if (ringT <= 0f) { ringT = 2.4f; if (AudioManager.I) { AudioManager.I.UI(AudioManager.I.phoneRing, 0.7f); AudioManager.I.UI(AudioManager.I.vibrate, 0.6f); } }
                yield return null;
            }
            Ringing = false; InCall = true; CallTime = 0f;
            if (Changed != null) Changed();
        }

        public void Answer() { if (Ringing) answered = true; }

        public void EndCall(string duration)
        {
            InCall = false; Ringing = false;
            if (AudioManager.I) AudioManager.I.UI(AudioManager.I.callEnd, 0.7f);
            CallLog(CallThread, GameClock.Text, false, duration);
        }

        void Update() { if (InCall) CallTime += Time.deltaTime; }
    }
}
