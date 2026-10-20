using UnityEngine;
using UnityEngine.Rendering;

namespace GNight
{
    /// <summary>Muestra un objeto frente a la cámara (capa primera persona) para girarlo y leerlo.</summary>
    public class InspectSystem : MonoBehaviour
    {
        public static InspectSystem I;
        public static bool Active => I != null && I.current != null;
        public Camera fpCam;
        public Inspectable Current => current;

        Inspectable current;
        Transform pivot;
        float dist = 0.42f, yawAcc, pitchAcc, openT;
        int openedFrame;
        bool backShown;

        void Awake() { I = this; }

        public void Inspect(Inspectable it) { Begin(it, false); }
        public void InspectStored(Inspectable it) { Begin(it, true); }

        void Begin(Inspectable it, bool stored)
        {
            if (it == null || current != null) return;
            current = it; openedFrame = Time.frameCount; openT = 0f; yawAcc = 0f; pitchAcc = 0f; dist = 0.42f; backShown = false;
            Transform src = it.visual != null ? it.visual : it.transform;
            pivot = new GameObject("InspectPivot").transform;
            pivot.SetParent(fpCam.transform, false);
            var clone = Instantiate(src.gameObject);
            clone.SetActive(true);
            foreach (var mb in clone.GetComponentsInChildren<MonoBehaviour>(true)) Destroy(mb);
            foreach (var c in clone.GetComponentsInChildren<Collider>(true)) Destroy(c);
            foreach (var l in clone.GetComponentsInChildren<Light>(true)) Destroy(l);
            foreach (var a in clone.GetComponentsInChildren<AudioSource>(true)) Destroy(a);
            foreach (var r in clone.GetComponentsInChildren<Renderer>(true)) { r.shadowCastingMode = ShadowCastingMode.Off; r.enabled = true; }
            GNLayers.SetLayerRecursive(clone, GNLayers.FP);
            clone.transform.SetParent(pivot, false);
            clone.transform.localRotation = Quaternion.Euler(it.presentEuler);
            clone.transform.localScale = src.lossyScale;
            // centrar y escalar
            var rs = clone.GetComponentsInChildren<Renderer>();
            if (rs.Length > 0)
            {
                Bounds b = rs[0].bounds;
                for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
                float size = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
                float s = size > 0.0001f ? it.targetSize / size : 1f;
                clone.transform.localScale *= s;
                rs = clone.GetComponentsInChildren<Renderer>();
                b = rs[0].bounds; for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
                clone.transform.position -= b.center - pivot.position;
            }
            pivot.localPosition = new Vector3(0, -0.02f, dist);
            if (AudioManager.I) AudioManager.I.UI(it.isDocument ? AudioManager.I.pageFlip : AudioManager.I.pickup, 0.7f);
            if (FPRig.I != null && !stored) FPRig.I.Reach(it.transform.position);
            if (!it.seen) { it.seen = true; if (!it.clueOnBack) it.GiveClue(); }
            if (it.isDocument && JournalManager.I) JournalManager.I.AddDocument(it);
            Caption();
            GameEvents.Raise("inspect:" + it.id);
        }

        void Caption()
        {
            if (HUDView.I == null || current == null) return;
            string body = backShown && current.BackText != null ? current.BackText : current.Text;
            string hint = Settings.T("[Ratón] girar   [Rueda] acercar   [E] / [Esc] soltar", "[Mouse] rotate   [Wheel] zoom   [E] / [Esc] put down");
            if (current.BackText != null && !backShown) hint = Settings.T("Parece que hay algo escrito atrás…   ", "There seems to be something on the back…   ") + hint;
            HUDView.I.ShowInspect(current.Title, body, hint);
        }

        void Update()
        {
            if (current == null) return;
            openT += Time.deltaTime;
            Vector2 d = Inp.Look() * Settings.Sensitivity;
            Transform cam = fpCam.transform;
            pivot.Rotate(cam.up, -d.x * 2.2f, Space.World);
            pivot.Rotate(cam.right, d.y * 2.2f, Space.World);
            yawAcc += -d.x * 2.2f; pitchAcc += d.y * 2.2f;
            dist = Mathf.Clamp(dist - Inp.Wheel() * 0.05f, 0.22f, 0.65f);
            float intro = Ease.OutBack(Mathf.Clamp01(openT / 0.35f));
            pivot.localPosition = Vector3.Lerp(new Vector3(0.1f, -0.25f, 0.3f), new Vector3(0, -0.02f, dist), intro);

            // ¿está dado vuelta?
            float yawW = Mathf.Repeat(yawAcc + 180f, 360f) - 180f;
            float pitW = Mathf.Repeat(pitchAcc + 180f, 360f) - 180f;
            bool back = Mathf.Abs(yawW) > 100f || Mathf.Abs(pitW) > 100f;
            if (back != backShown)
            {
                backShown = back; Caption();
                if (back && !current.seenBack)
                {
                    current.seenBack = true;
                    if (current.clueOnBack) current.GiveClue();
                    GameEvents.Raise("inspect_back:" + current.id);
                }
            }
            if (Time.frameCount != openedFrame && openT > 0.25f && (Inp.Interact() || Inp.Esc() || Inp.RightClick()))
                End();
        }

        public void End()
        {
            if (current == null) return;
            var it = current;
            current = null;
            if (pivot) Destroy(pivot.gameObject);
            if (HUDView.I) HUDView.I.ShowInspect(null, null, null);
            if (it.collect && !Inventory.Has(it.id))
            {
                Inventory.Add(it.id, string.IsNullOrEmpty(it.invNameEs) ? it.titleEs : it.invNameEs, string.IsNullOrEmpty(it.invNameEn) ? it.titleEn : it.invNameEn, it);
                it.gameObject.SetActive(false);
                if (HUDView.I) HUDView.I.Hint(Settings.T("Guardado: ", "Taken: ") + it.Title, 2.5f);
                if (AudioManager.I) AudioManager.I.UI(AudioManager.I.pocket, 0.5f);
                GameEvents.Raise("collected:" + it.id);
            }
            GameEvents.Raise("inspected:" + it.id);
        }
    }
}
