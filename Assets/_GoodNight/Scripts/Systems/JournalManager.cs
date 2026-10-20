using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>Diario del teléfono: pistas cortas y documentos ya examinados (se pueden volver a abrir).</summary>
    public class JournalManager : MonoBehaviour
    {
        public static JournalManager I;
        public readonly List<string> ClueIds = new List<string>();
        public readonly List<string> Clues = new List<string>();
        public readonly List<Inspectable> Documents = new List<Inspectable>();
        public event System.Action Changed;

        void Awake() { I = this; }

        public void AddClue(string id, string text, bool notify = true)
        {
            if (ClueIds.Contains(id)) return;
            ClueIds.Add(id); Clues.Add(text);
            if (notify && HUDView.I) HUDView.I.Hint(Settings.T("Pista anotada en el teléfono", "Clue saved to your phone"), 2.5f);
            if (Changed != null) Changed();
        }

        public void AddDocument(Inspectable d)
        {
            if (d == null || Documents.Contains(d)) return;
            Documents.Add(d);
            if (Changed != null) Changed();
        }

        public bool HasDocument(string id) { foreach (var d in Documents) if (d != null && d.id == id) return true; return false; }
        public bool HasClue(string id) => ClueIds.Contains(id);
    }
}
