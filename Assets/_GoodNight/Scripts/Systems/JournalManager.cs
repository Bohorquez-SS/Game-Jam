using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>Diario: pistas cortas y documentos encontrados.</summary>
    public class JournalManager : MonoBehaviour
    {
        public static JournalManager I;
        public readonly List<string> ClueIds = new List<string>();
        public readonly List<string> Clues = new List<string>();
        public readonly List<DocumentData> Documents = new List<DocumentData>();
        public event System.Action Changed;

        void Awake() { I = this; }

        public void AddClue(string id, string text)
        {
            if (ClueIds.Contains(id)) return;
            ClueIds.Add(id); Clues.Add(text);
            if (HUDView.I) HUDView.I.Hint(Settings.T("Pista anotada en el teléfono", "Clue saved to your phone"), 2.5f);
            if (Changed != null) Changed();
        }

        public bool AddDocument(DocumentData d)
        {
            if (d == null || HasDocument(d.id)) return false;
            Documents.Add(d);
            if (Changed != null) Changed();
            return true;
        }

        public bool HasDocument(string id) { foreach (var d in Documents) if (d.id == id) return true; return false; }
        public bool HasClue(string id) => ClueIds.Contains(id);
    }
}
