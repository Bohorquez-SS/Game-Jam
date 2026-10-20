using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Objetivo actual: siempre visible en el HUD («OBJETIVO ACTUAL» con [ ] / [x]) y en el teléfono.
    /// Pistas opcionales de 3 niveles (HintBook): se desbloquean si el jugador lleva tiempo con el mismo objetivo y se
    /// revelan una a una en la app Objetivos del teléfono. Si no hay entrada en HintBook se usa la pista del Set como nivel 1.
    /// </summary>
    public class ObjectiveManager : MonoBehaviour
    {
        public static ObjectiveManager I;
        public string Title { get; private set; }
        public string Hint { get; private set; }
        public readonly List<string> Items = new List<string>();
        public readonly List<bool> Done = new List<bool>();
        public event System.Action Changed;
        public float hintDelay = 100f;                                  // compatibilidad: nivel 1 sin HintBook
        public static readonly float[] HintTimes = { 45f, 100f, 170f };   // segundos atascado para cada nivel
        float setTime;
        HintBook.Entry entry;
        int noticed;

        /// <summary>Pistas ya reveladas por el jugador (0..3).</summary>
        public int HintsShown { get; private set; }
        public bool Investigation => entry != null && entry.investigate;
        public int HintLevels => entry != null ? 3 : (string.IsNullOrEmpty(Hint) ? 0 : 1);
        /// <summary>Niveles disponibles ahora mismo según el tiempo atascado.</summary>
        public int HintsAvailable
        {
            get
            {
                int n = 0; float t = Time.time - setTime;
                for (int i = 0; i < HintLevels; i++) if (t >= HintTimes[i]) n = i + 1;
                return n;
            }
        }
        public float SecondsToNextHint => HintsAvailable >= HintLevels ? -1f : Mathf.Max(0f, HintTimes[HintsAvailable] - (Time.time - setTime));
        public string HintText(int level)
        {
            if (entry != null) return Settings.T(entry.es[level], entry.en[level]);
            return level == 0 ? Hint : null;
        }
        public bool HintUnlocked => HintsAvailable > 0;

        /// <summary>Revela el siguiente nivel de pista si ya está disponible.</summary>
        public bool RevealNextHint()
        {
            if (HintsShown >= HintsAvailable) return false;
            HintsShown++;
            GameEvents.Raise("hint_revealed:" + HintsShown);
            if (Changed != null) Changed();
            return true;
        }

        void Awake() { I = this; }

        void Update()
        {
            if (string.IsNullOrEmpty(Title)) return;
            int av = HintsAvailable;
            if (av > noticed)
            {
                noticed = av;
                if (av > HintsShown && HUDView.I && GameManager.I != null && GameManager.I.Playing)
                    HUDView.I.HintNotice(Settings.T("Pista disponible · [Tab] → Objetivos", "Hint available · [Tab] → Objectives"));
                if (Changed != null) Changed();
            }
        }

        public void Set(string title, string hint, params string[] items)
        {
            bool same = title == Title;
            Title = title; Hint = hint;
            Items.Clear(); Done.Clear();
            foreach (var i in items) { Items.Add(i); Done.Add(false); }
            var e = HintBook.Find(title, items.Length > 0 ? items[0] : null);
            if (!same || e != entry) { setTime = Time.time; HintsShown = 0; noticed = 0; }
            entry = e;
            Refresh(true);
        }

        public void Check(int index, bool value = true)
        {
            if (index < 0 || index >= Done.Count || Done[index] == value) return;
            Done[index] = value; Refresh(true);
            if (value && AudioManager.I) AudioManager.I.UI(AudioManager.I.check, 0.35f);
        }

        public void SetItem(int index, string text)
        {
            if (index < 0 || index >= Items.Count || Items[index] == text) return;
            Items[index] = text; Refresh(false);
        }

        public bool AllDone { get { foreach (var d in Done) if (!d) return false; return true; } }
        public void Clear() { Title = null; Hint = null; entry = null; HintsShown = 0; Items.Clear(); Done.Clear(); Refresh(false); }

        public string ItemsText()
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < Items.Count; i++)
                sb.Append(Done[i] ? "<color=#8a8a8a>[x] " + Items[i] + "</color>" : "[  ] " + Items[i]).Append('\n');
            return sb.ToString();
        }

        void Refresh(bool announce)
        {
            if (HUDView.I) HUDView.I.ShowObjective(string.IsNullOrEmpty(Title) ? null : Title, ItemsText(), announce);
            if (Changed != null) Changed();
        }
    }
}
