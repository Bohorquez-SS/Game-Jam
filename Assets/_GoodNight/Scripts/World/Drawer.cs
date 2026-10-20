using UnityEngine;

namespace GNight
{
    /// <summary>Cajón que se desliza. Puede contener un documento.</summary>
    public class Drawer : Interactable
    {
        public string id = "drawer";
        public Transform slide;
        public Vector3 openOffset = new Vector3(0, 0, 0.3f);
        public bool isOpen;
        public DocumentData content;
        public string clueId, clueEs, clueEn;
        bool collected; Vector3 basePos; float t; bool init;

        void Init() { if (init) return; init = true; basePos = slide.localPosition; }
        void Awake() { Init(); }

        public override string Prompt => isOpen ? Settings.T("Cerrar cajón", "Close drawer") : Settings.T("Abrir cajón", "Open drawer");

        public override void Interact()
        {
            isOpen = !isOpen;
            if (AudioManager.I) AudioManager.I.At(AudioManager.I.drawer, transform.position, 0.8f);
            if (isOpen)
            {
                GameEvents.Raise("drawer_opened:" + id);
                if (content != null && !string.IsNullOrEmpty(content.id) && !collected) Collect(true);
            }
        }

        public void Collect(bool show)
        {
            collected = true;
            if (JournalManager.I) JournalManager.I.AddDocument(content);
            if (!string.IsNullOrEmpty(clueId) && JournalManager.I) JournalManager.I.AddClue(clueId, Settings.T(clueEs, clueEn));
            if (show && UIManager.I) UIManager.I.Get<DocumentPanel>().Show(content);
            GameEvents.Raise("doc:" + content.id);
        }

        void Update()
        {
            t = Mathf.MoveTowards(t, isOpen ? 1 : 0, Time.deltaTime * 4f);
            slide.localPosition = basePos + openOffset * Mathf.SmoothStep(0, 1, t);
        }
    }
}
