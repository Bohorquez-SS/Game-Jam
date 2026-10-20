using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Objeto que se toma y se examina de cerca (girar con el ratón, acercar con la rueda).
    /// Puede guardarse en el inventario, registrarse como documento y dejar una pista en el teléfono.
    /// </summary>
    public class Inspectable : Interactable
    {
        public string id;
        public string titleEs = "Objeto", titleEn = "Object";
        [TextArea] public string textEs, textEn;
        [Tooltip("Texto que aparece cuando el objeto está dado vuelta (reverso).")]
        [TextArea] public string backTextEs, backTextEn;
        public string verbEs = "Examinar", verbEn = "Examine";
        public Transform visual;
        public Vector3 presentEuler = Vector3.zero;
        public float targetSize = 0.24f;
        [Header("Inventario / diario")]
        public bool collect;
        public string invNameEs, invNameEn;
        public bool isDocument;
        public string clueId, clueEs, clueEn;
        [Tooltip("La pista solo se anota al ver el reverso.")]
        public bool clueOnBack;
        public bool available = true;
        [HideInInspector] public bool seen, seenBack;

        public string Title => Settings.T(titleEs, titleEn);
        public string Text => Settings.T(textEs, textEn);
        public string BackText => string.IsNullOrEmpty(backTextEs) ? null : Settings.T(backTextEs, backTextEn);
        public override string Prompt => Settings.T(verbEs, verbEn) + " " + Settings.T(titleEs, titleEn).ToLowerInvariant();
        public override bool CanInteract => available && !InspectSystem.Active;

        public override void Interact() { if (InspectSystem.I) InspectSystem.I.Inspect(this); }

        public void GiveClue()
        {
            if (!string.IsNullOrEmpty(clueId) && JournalManager.I) JournalManager.I.AddClue(clueId, Settings.T(clueEs, clueEn));
        }
    }
}
