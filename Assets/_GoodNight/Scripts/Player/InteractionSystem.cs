using UnityEngine;

namespace GNight
{
    /// <summary>Raycast desde la cámara: muestra "[E] acción" solo si el objeto está cerca y a la vista.</summary>
    public class InteractionSystem : MonoBehaviour
    {
        public static InteractionSystem I;
        public Transform cam;
        public float reach = 2.5f;
        /// <summary>Radio de "ayuda de puntería" para objetos pequeños (cintas, fusible, llaves, papeles).</summary>
        public float assist = 0.11f;
        static readonly RaycastHit[] hits = new RaycastHit[24];
        static readonly Collider[] near = new Collider[32];
        public IInteractable Current { get; private set; }

        void Awake() { I = this; }

        void Update()
        {
            Current = null;
            var hud = HUDView.I;
            if (hud == null) return;
            // con el teléfono abierto, [E] es del teléfono (abrir conversación / pista): no tocar el mundo
            if (Phone3D.IsOpen) { hud.SetPrompt(null); return; }

            // Escondido: [E] sale. Va ANTES de MoveBlocked, porque estar escondido ya bloquea el movimiento
            // (antes este caso nunca se alcanzaba y no se podía salir con E).
            if (HidingSpot.Current != null)
            {
                bool busy = PlayerController.CutsceneLock || PlayerController.TransitionLock || UIManager.ModalOpen || InspectSystem.Active || FocusSystem.Active
                            || (PhoneSystem.I != null && PhoneSystem.I.Ringing);   // si suena, E contesta y no te saca
                hud.SetPrompt(busy ? null : Settings.T("Salir del escondite", "Leave hiding spot"));
                if (!busy && Inp.Interact()) HidingSpot.Current.Interact();
                return;
            }

            if (PlayerController.MoveBlocked) { hud.SetPrompt(null); return; }
            // si suena el teléfono, [E] contesta (Phone3D) y no debe activar también lo que se esté mirando
            if (PhoneSystem.I != null && PhoneSystem.I.Ringing) { hud.SetPrompt(Settings.T("Contestar el teléfono", "Answer the phone")); return; }

            Current = Pick();

            if (Current != null)
            {
                hud.SetPrompt(Current.Prompt);
                if (Inp.Interact()) Current.Interact();
            }
            else hud.SetPrompt(null);
        }
    
        /// <summary>
        /// Selección cómoda (18/10):
        ///  1) RaycastAll ordenado: se ignoran colisiones "ajenas" muy cercanas (cuerpo del mueble que envuelve un cajón,
        ///     objeto no disponible dentro de un cajón cerrado) si justo detrás hay algo interactivo.
        ///  2) Prioridad a objetos pequeños: si cerca de la línea de mira hay un objeto examinable disponible (p. ej. la cinta
        ///     dentro de un cajón abierto), gana sobre el cajón/mueble; el frente del cajón sigue funcionando apuntándole directo.
        /// </summary>
        Interactable Pick()
        {
            Vector3 o = cam.position, d = cam.forward;
            int n = Physics.RaycastNonAlloc(o, d, hits, reach, ~0, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, 0, n, HitCmp.I);
            Interactable best = null; float bestDist = reach; float solidDist = -1f;
            for (int i = 0; i < n; i++)
            {
                var h = hits[i];
                var mb = h.collider.GetComponentInParent<Interactable>();
                if (mb != null && mb.enabled && mb.CanInteract) { best = mb; bestDist = h.distance; break; }
                if (solidDist < 0f) solidDist = h.distance;
                if (h.distance > solidDist + 0.12f) break;   // pared u objeto sólido de verdad: no mirar a través
            }
            if (best is Inspectable) return best;
            // cajón abierto: si el rayo atraviesa un objeto de su contenido, gana el objeto (no hace falta cerrar el cajón)
            if (best is Drawer dw && dw.isOpen && dw.contents != null)
            {
                var ray = new Ray(o, d);
                foreach (var content in dw.contents)
                {
                    if (content == null || !content.gameObject.activeInHierarchy || !content.CanInteract) continue;
                    var ccol = content.GetComponentInChildren<Collider>();
                    Bounds cb = ccol != null ? ccol.bounds : new Bounds(content.transform.position, Vector3.one * 0.1f);
                    cb.Expand(0.06f);
                    if (cb.IntersectRay(ray, out float cdist) && cdist < reach) return content;
                }
            }
            // ayuda de puntería: examinables cerca del rayo, antes de lo que se golpeó (o un poco detrás si es un cajón abierto/mueble)
            float maxD = best != null ? bestDist + 0.18f : (solidDist >= 0f ? solidDist + 0.1f : reach);
            Vector3 a = o + d * 0.3f, b = o + d * Mathf.Min(reach, maxD);
            int m = Physics.OverlapCapsuleNonAlloc(a, b, assist, near, ~0, QueryTriggerInteraction.Ignore);
            Inspectable small = null; float smallScore = float.MaxValue, smallAlong = 0f;
            for (int i = 0; i < m; i++)
            {
                var it = near[i].GetComponentInParent<Inspectable>();
                if (it == null || !it.enabled || !it.CanInteract) continue;
                Vector3 c = near[i].bounds.center;
                float along = Vector3.Dot(c - o, d);
                if (along < 0.2f || along > maxD + 0.1f) continue;
                float off = Vector3.Cross(d, c - o).magnitude;          // distancia a la línea de mira
                // línea de visión: nada sólido (que no sea el propio objeto o su mueble) entre el ojo y el objeto
                if (Physics.Linecast(o, c, out RaycastHit lh, ~0, QueryTriggerInteraction.Ignore))
                {
                    var lhIt = lh.collider.GetComponentInParent<Inspectable>();
                    bool own = lhIt == it || lh.distance > Vector3.Distance(o, c) - 0.06f;
                    var lhDrawer = lh.collider.GetComponentInParent<Drawer>();
                    if (!own && !(lhDrawer != null && lhDrawer.isOpen)) continue;
                }
                if (off < smallScore) { smallScore = off; small = it; smallAlong = along; }
            }
            // el frente de un cajón apuntado directamente sigue siendo "Cerrar cajón"; el objeto gana si está delante del punto golpeado
            if (small != null && (best == null || (best is Drawer ? smallAlong <= bestDist + 0.03f : smallScore < assist * 0.8f))) return small;
            return best;
        }

        class HitCmp : System.Collections.Generic.IComparer<RaycastHit>
        {
            public static readonly HitCmp I = new HitCmp();
            public int Compare(RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance);
        }
}
}
