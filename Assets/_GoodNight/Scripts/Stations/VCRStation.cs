using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Videograbadora + TV de la sala. [Clic] ranura: inserta la siguiente cinta del inventario y la reproduce.
    /// Después de ver las tres, se ordenan cronológicamente haciendo clic en sus cajas: GUARDIÁN reconstruye la noche.
    /// </summary>
    public class VCRStation : FocusStation
    {
        public MonitorScreen tv;
        public TapeData[] tapes;
        public VcrSlot slot;
        public PushButton ejectBtn;
        public RenderTexture tapeRT;
        public string[] correctOrder = { "tape_c", "tape_a", "tape_b" };
        readonly HashSet<string> watched = new HashSet<string>();
        readonly List<string> order = new List<string>();
        public bool Reconstructed { get; private set; }
        public int WatchedCount => watched.Count;
        TapeData playing; Coroutine playCo;

        void Start()
        {
            foreach (var t in tapes)
            {
                if (t.cam)
                {
                    t.cam.enabled = false; t.cam.targetTexture = tapeRT; t.cam.cullingMask = ~(GNLayers.Mask(GNLayers.FP) | GNLayers.Mask(GNLayers.Mirror) | GNLayers.Mask(GNLayers.CamOnly));
                    var ir = t.cam.GetComponentInChildren<Light>(true);
                    if (ir) { ir.enabled = false; RenderTricks.Add(t.cam, () => ir.enabled = true, () => ir.enabled = false); }
                }
                if (t.actors) { GNLayers.SetLayerRecursive(t.actors, GNLayers.Tape); t.actors.SetActive(false); }
                if (t.box) { t.box.SetActive(false); var c = t.box.GetComponent<TapeBox>(); if (c) { var tt = t; c.Clicked = () => Pick(tt); } }
            }
            if (ejectBtn) ejectBtn.Pressed = b => Stop(true);
            if (slot) slot.Clicked = Insert;
        }

        public override string Hint
        {
            get
            {
                if (watched.Count >= tapes.Length && !Reconstructed)
                    return Settings.T("Ordena las cintas: [Clic] en las cajas en orden cronológico · [Esc] alejarse", "Order the tapes: [Click] the boxes chronologically · [Esc] step back");
                return Settings.T("[Clic] en la ranura: insertar cinta · [Clic] EJECT · [Esc] alejarse", "[Click] the slot: insert tape · [Click] EJECT · [Esc] step back");
            }
        }

        public override void OnFocusTick()
        {
            // la ranura: clic → insertar la siguiente cinta disponible
            if (slot != null) slot.labelEs = NextTape() != null ? "Insertar cinta " + NextTape().letter : (playing != null ? "Reproduciendo…" : "No tienes más cintas");
            if (slot != null) slot.labelEn = NextTape() != null ? "Insert tape " + NextTape().letter : (playing != null ? "Playing…" : "No more tapes");
        }

        TapeData NextTape()
        {
            foreach (var t in tapes) if (Inventory.Has(t.id) && t != playing) return t;
            return null;
        }

        /// <summary>Llamado por la ranura (SocketSlot con itemId vacío y Installed → Insert).</summary>
        public void Insert()
        {
            var t = NextTape();
            if (t == null) { if (AudioManager.I) AudioManager.I.UI(AudioManager.I.error, 0.4f); return; }
            Stop(false);
            if (AudioManager.I) AudioManager.I.At(AudioManager.I.vhsInsert, transform.position, 0.9f);
            playCo = StartCoroutine(Play(t));
        }

        IEnumerator Play(TapeData t)
        {
            playing = t;
            tv.SetOn(true); tv.showStatic = true; tv.message = null;
            yield return new WaitForSeconds(0.9f);
            tv.showStatic = false;
            tv.overrideTexture = tapeRT; tv.recLabel = true; tv.stampOverride = t.dateLabel.Replace("23:59", t.stamp);
            if (t.actors) t.actors.SetActive(true);
            if (t.cam) t.cam.enabled = true;
            if (t.actor) StartCoroutine(t.actor.Run());
            if (AudioManager.I && AudioManager.I.tapeHiss) AudioManager.I.At(AudioManager.I.tapeHiss, tv.transform.position, 0.5f);
            GameEvents.Raise("tape_play:" + t.id);
            float time = 0f;
            while (time < t.duration) { time += Time.deltaTime; yield return null; }
            if (!watched.Contains(t.id))
            {
                watched.Add(t.id);
                if (JournalManager.I) JournalManager.I.AddClue("clue_" + t.id, Settings.T(t.clueEs, t.clueEn));
                Inventory.Remove(t.id);
                if (t.box) t.box.SetActive(true);
                GameEvents.Raise("tape_watched:" + t.id);
            }
            Stop(false);
            if (watched.Count >= tapes.Length && !Reconstructed)
            {
                tv.SetOn(true); tv.overrideTexture = null; tv.source = null;
                tv.message = Settings.T("GUARDIÁN · RECONSTRUCCIÓN\n<size=18>Ordene las grabaciones (más antigua primero)</size>", "GUARDIAN · RECONSTRUCTION\n<size=18>Order the recordings (oldest first)</size>");
                GameEvents.Raise("tapes_all_watched");
            }
        }

        void Stop(bool sound)
        {
            if (playCo != null) StopCoroutine(playCo);
            playCo = null;
            if (playing != null)
            {
                if (playing.cam) playing.cam.enabled = false;
                if (playing.actors) playing.actors.SetActive(false);
                playing = null;
            }
            tv.overrideTexture = null; tv.recLabel = false; tv.stampOverride = null; tv.showStatic = false;
            if (!Reconstructed && watched.Count < tapes.Length) { tv.message = null; tv.SetOn(false); }
            if (sound && AudioManager.I) AudioManager.I.At(AudioManager.I.vhsInsert, transform.position, 0.6f);
        }

        void Pick(TapeData t)
        {
            if (Reconstructed || watched.Count < tapes.Length || order.Contains(t.id)) return;
            order.Add(t.id);
            if (AudioManager.I) AudioManager.I.UI(AudioManager.I.click, 0.7f);
            tv.message = Settings.T("ORDEN: ", "ORDER: ") + string.Join(" → ", order.ConvertAll(id => System.Array.Find(tapes, x => x.id == id).letter).ToArray());
            if (order.Count < correctOrder.Length) return;
            bool ok = true;
            for (int i = 0; i < correctOrder.Length; i++) if (order[i] != correctOrder[i]) ok = false;
            if (ok)
            {
                Reconstructed = true;
                if (AudioManager.I) AudioManager.I.UI(AudioManager.I.beep, 0.9f);
                tv.message = Settings.T("<color=#6dff8a>SECUENCIA RECONSTRUIDA</color>\n<size=17>22:10 INGRESO · SUJETO 1\n22:47 COCINA · SUJETO 1\n23:46 DORMITORIO · <color=#ff5544>SUJETO 2 (SIN INGRESO)</color>\n01:24 PASILLO · REGISTRO PENDIENTE</size>",
                                        "<color=#6dff8a>SEQUENCE RECONSTRUCTED</color>\n<size=17>22:10 ENTRY · SUBJECT 1\n22:47 KITCHEN · SUBJECT 1\n23:46 BEDROOM · <color=#ff5544>SUBJECT 2 (NO ENTRY)</color>\n01:24 HALLWAY · LOG PENDING</size>");
                GameEvents.Raise("tapes_reconstructed");
            }
            else
            {
                if (AudioManager.I) AudioManager.I.UI(AudioManager.I.error, 0.8f);
                StartCoroutine(Wrong());
            }
        }

        IEnumerator Wrong()
        {
            tv.showStatic = true;
            tv.message = Settings.T("<color=#ff5544>SECUENCIA INCONSISTENTE</color>", "<color=#ff5544>INCONSISTENT SEQUENCE</color>");
            yield return new WaitForSeconds(1.5f);
            tv.showStatic = false; order.Clear();
            tv.message = Settings.T("GUARDIÁN · RECONSTRUCCIÓN\n<size=18>Ordene las grabaciones (más antigua primero)</size>", "GUARDIAN · RECONSTRUCTION\n<size=18>Order the recordings (oldest first)</size>");
        }

        public void ForceReconstructed() { Reconstructed = true; foreach (var t in tapes) watched.Add(t.id); }
    }
}
