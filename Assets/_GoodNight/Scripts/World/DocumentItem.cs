using UnityEngine;

namespace GNight
{
    /// <summary>Documento/fotografía en el mundo: se lee y queda guardado en el teléfono.</summary>
    public class DocumentItem : Interactable
    {
        public DocumentData doc;
        public string verbEs = "Examinar documento", verbEn = "Examine document";
        public string clueId, clueEs, clueEn;
        public override string Prompt => Settings.T(verbEs, verbEn);

        public override void Interact()
        {
            if (JournalManager.I) JournalManager.I.AddDocument(doc);
            if (!string.IsNullOrEmpty(clueId) && JournalManager.I) JournalManager.I.AddClue(clueId, Settings.T(clueEs, clueEn));
            UIManager.I.Get<DocumentPanel>().Show(doc);
            GameEvents.Raise("doc:" + doc.id);
        }
    }
}
