using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>Objetivo actual con sub-tareas.</summary>
    public class ObjectiveManager : MonoBehaviour
    {
        public static ObjectiveManager I;
        public string Title { get; private set; }
        public string Hint { get; private set; }
        public readonly List<string> Items = new List<string>();
        public readonly List<bool> Done = new List<bool>();
        public event System.Action Changed;

        void Awake() { I = this; }

        public void Set(string title, string hint, params string[] items)
        {
            Title = title; Hint = hint;
            Items.Clear(); Done.Clear();
            foreach (var i in items) { Items.Add(i); Done.Add(false); }
            Refresh();
        }

        public void Check(int index, bool value = true)
        {
            if (index < 0 || index >= Done.Count || Done[index] == value) return;
            Done[index] = value; Refresh();
        }

        public void SetItem(int index, string text)
        {
            if (index < 0 || index >= Items.Count) return;
            Items[index] = text; Refresh();
        }

        public bool AllDone { get { foreach (var d in Done) if (!d) return false; return true; } }
        public void Clear() { Title = null; Hint = null; Items.Clear(); Done.Clear(); Refresh(); }

        public string ItemsText()
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < Items.Count; i++)
                sb.Append(Done[i] ? "<color=#7a7a7a>[x] " + Items[i] + "</color>" : "[  ] " + Items[i]).Append('\n');
            return sb.ToString();
        }

        void Refresh()
        {
            if (HUDView.I) HUDView.I.ShowObjective(string.IsNullOrEmpty(Title) ? null : Title.ToUpperInvariant(), ItemsText());
            if (Changed != null) Changed();
        }
    }
}
