using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>Objetivo actual. Se muestra unos segundos al cambiar y siempre queda en el teléfono.</summary>
    public class ObjectiveManager : MonoBehaviour
    {
        public static ObjectiveManager I;
        public string Title { get; private set; }
        public string Hint { get; private set; }
        public readonly List<string> Items = new List<string>();
        public readonly List<bool> Done = new List<bool>();
        public event System.Action Changed;
        public float hintDelay = 100f;
        float setTime;

        void Awake() { I = this; }

        public bool HintUnlocked => !string.IsNullOrEmpty(Hint) && Time.time - setTime > hintDelay;

        public void Set(string title, string hint, params string[] items)
        {
            Title = title; Hint = hint; setTime = Time.time;
            Items.Clear(); Done.Clear();
            foreach (var i in items) { Items.Add(i); Done.Add(false); }
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
        public void Clear() { Title = null; Hint = null; Items.Clear(); Done.Clear(); Refresh(false); }

        public string ItemsText()
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < Items.Count; i++)
                sb.Append(Done[i] ? "<color=#7a7a7a>✓ " + Items[i] + "</color>" : "•  " + Items[i]).Append('\n');
            return sb.ToString();
        }

        void Refresh(bool announce)
        {
            if (HUDView.I) HUDView.I.ShowObjective(string.IsNullOrEmpty(Title) ? null : Title.ToUpperInvariant(), ItemsText(), announce);
            if (Changed != null) Changed();
        }
    }
}
