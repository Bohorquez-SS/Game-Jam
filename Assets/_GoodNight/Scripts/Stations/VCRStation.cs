using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>
    /// Videograbadora + TV de la sala, con interfaz a pantalla completa (rediseño 18/10):
    ///  1) VER: clic en la tarjeta de una cinta que tengas → se reproduce en la pantalla (y en la TV).
    ///  2) ORDENAR (cuando viste las 3): clic en una cinta para seleccionarla → clic en una posición (1º, 2º, 3º).
    ///     Clic en una posición ocupada sin selección = devuelve la cinta. Se pueden intercambiar.
    ///  3) REPRODUCIR SECUENCIA comprueba el orden. Error → aviso + pista progresiva. Correcto → "tapes_reconstructed".
    /// Nunca se pierde una cinta: las vistas quedan registradas aunque salgan del inventario.
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
        readonly string[] seq = new string[3];
        string selected;
        int wrongTries;
        float feedbackT; string feedback;
        public bool Reconstructed { get; private set; }
        public int WatchedCount => watched.Count;
        TapeData playing; Coroutine playCo; float playT;

        // ---- interfaz
        RectTransform ui; RawImage feed; Text title, screenMsg, help, fb, counter, seqHead;
        UIButton[] cards = new UIButton[3]; UIButton[] slotBtns = new UIButton[3];
        UIButton check, stopBtn, exitBtn;
        public override bool ConsumesMouse => ui != null && ui.gameObject.activeSelf;

        public override string Instructions => null;   // la interfaz propia ya lo explica
        public override string Hint => Settings.T("[Clic] usar la videograbadora · [Esc] salir", "[Click] use the VCR · [Esc] leave");

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
                if (t.box) { t.box.SetActive(false); var c = t.box.GetComponent<TapeBox>(); if (c) { var tt = t; c.Clicked = () => OnCard(tt); } }
            }
            if (ejectBtn) ejectBtn.Pressed = b => Stop(true);
            if (slot) slot.Clicked = Insert;
        }

        // ------------------------------------------------------------------ estado
        bool Found(TapeData t) => Inventory.Has(t.id) || watched.Contains(t.id);
        int FoundCount() { int n = 0; foreach (var t in tapes) if (Found(t)) n++; return n; }
        bool OrderingPhase => watched.Count >= tapes.Length && !Reconstructed;
        TapeData ById(string id) => System.Array.Find(tapes, x => x.id == id);
        static string Day(TapeData t) => t.stamp.StartsWith("0") ? Settings.T("DOM", "SUN") : Settings.T("SÁB", "SAT");
        string Stamp(TapeData t) => Day(t) + " " + t.stamp;
        static string Place(string id)
        {
            switch (id)
            {
                case "tape_a": return Settings.T("cocina", "kitchen");
                case "tape_b": return Settings.T("pasillo", "hallway");
                default: return Settings.T("porche", "porch");
            }
        }

        TapeData NextTape()
        {
            foreach (var t in tapes) if (Inventory.Has(t.id) && t != playing) return t;
            return null;
        }

        /// <summary>Compatibilidad (ranura 3D / recorrido automático): reproduce la siguiente cinta que tengas.</summary>
        public void Insert()
        {
            var t = NextTape();
            if (t == null) { if (AudioManager.I) AudioManager.I.UI(AudioManager.I.error, 0.4f); return; }
            PlayTape(t);
        }

        void PlayTape(TapeData t)
        {
            Stop(false);
            if (AudioManager.I) AudioManager.I.At(AudioManager.I.vhsInsert, transform.position, 0.9f);
            playCo = StartCoroutine(Play(t));
        }

        IEnumerator Play(TapeData t)
        {
            playing = t; playT = 0f;
            tv.SetOn(true); tv.showStatic = true; tv.message = null;
            yield return new WaitForSeconds(0.9f);
            tv.showStatic = false;
            tv.overrideTexture = tapeRT; tv.recLabel = true; tv.stampOverride = Stamp(t);
            if (t.actors) t.actors.SetActive(true);
            if (t.cam) t.cam.enabled = true;
            if (t.actor) StartCoroutine(t.actor.Run());
            if (AudioManager.I && AudioManager.I.tapeHiss) AudioManager.I.At(AudioManager.I.tapeHiss, tv.transform.position, 0.5f);
            GameEvents.Raise("tape_play:" + t.id);
            while (playT < t.duration) { playT += Time.deltaTime; yield return null; }
            MarkWatched(t);
            Stop(false);
        }

        void MarkWatched(TapeData t)
        {
            if (watched.Contains(t.id)) return;
            watched.Add(t.id);
            if (JournalManager.I) JournalManager.I.AddClue("clue_" + t.id, Settings.T(t.clueEs, t.clueEn));
            Inventory.Remove(t.id);
            if (t.box) t.box.SetActive(true);
            GameEvents.Raise("tape_watched:" + t.id);
            if (watched.Count >= tapes.Length && !Reconstructed)
            {
                Feedback(Settings.T("Ya viste las tres. Estas grabaciones parecen estar fuera de orden...", "You've seen all three. These recordings seem out of order..."));
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
            if (Reconstructed) { tv.SetOn(true); tv.message = ReconText(); }
            else if (OrderingPhase) { tv.SetOn(true); tv.message = Settings.T("GUARDIÁN · RECONSTRUCCIÓN\n<size=18>Ordene las grabaciones (más antigua primero)</size>", "GUARDIAN · RECONSTRUCTION\n<size=18>Order the recordings (oldest first)</size>"); }
            else { tv.message = null; tv.SetOn(false); }
            if (sound && AudioManager.I) AudioManager.I.At(AudioManager.I.vhsInsert, transform.position, 0.6f);
        }

        static string ReconText() => Settings.T(
            "<color=#6dff8a>SECUENCIA RECONSTRUIDA</color>\n<size=17>22:10 INGRESO · SUJETO 1\n22:47 COCINA · SUJETO 1\n23:46 DORMITORIO · <color=#ff5544>SUJETO 2 (SIN INGRESO)</color>\n01:24 PASILLO · REGISTRO PENDIENTE</size>",
            "<color=#6dff8a>SEQUENCE RECONSTRUCTED</color>\n<size=17>22:10 ENTRY · SUBJECT 1\n22:47 KITCHEN · SUBJECT 1\n23:46 BEDROOM · <color=#ff5544>SUBJECT 2 (NO ENTRY)</color>\n01:24 HALLWAY · LOG PENDING</size>");

        // ------------------------------------------------------------------ acciones de la interfaz
        void OnCard(TapeData t)
        {
            if (Reconstructed) return;
            if (!Found(t)) { Feedback(Settings.T("Todavía no encontraste esa cinta.", "You haven't found that tape yet."), true); return; }
            if (!OrderingPhase) { PlayTape(t); return; }
            selected = selected == t.id ? null : t.id;
            if (AudioManager.I) AudioManager.I.UI(AudioManager.I.click, 0.6f);
            if (selected != null) Feedback(Settings.T("Cinta ", "Tape ") + t.letter + Settings.T(" seleccionada. Ahora haz clic en una posición.", " selected. Now click a position."));
        }

        void OnSlot(int i)
        {
            if (!OrderingPhase) { Feedback(Settings.T("Primero mira las tres cintas.", "Watch all three tapes first."), true); return; }
            if (selected != null)
            {
                int from = System.Array.IndexOf(seq, selected);
                string prev = seq[i];
                seq[i] = selected;
                if (from >= 0 && from != i) seq[from] = prev;          // intercambio
                selected = null;
                if (AudioManager.I) AudioManager.I.At(AudioManager.I.vhsInsert, transform.position, 0.5f);
            }
            else if (seq[i] != null)
            {
                selected = seq[i]; seq[i] = null;                       // la retira y queda seleccionada
                if (AudioManager.I) AudioManager.I.UI(AudioManager.I.click, 0.6f);
            }
        }

        void Check()
        {
            for (int i = 0; i < 3; i++) if (seq[i] == null) { Feedback(Settings.T("Coloca las tres cintas antes de reproducir.", "Place all three tapes before playing."), true); return; }
            bool ok = true;
            for (int i = 0; i < correctOrder.Length; i++) if (seq[i] != correctOrder[i]) ok = false;
            if (ok) Solve();
            else
            {
                wrongTries++;
                if (AudioManager.I) AudioManager.I.UI(AudioManager.I.error, 0.8f);
                StartCoroutine(WrongFX());
                string h = wrongTries == 1 ? Settings.T("Tal vez las fechas de las grabaciones indiquen el orden.", "Maybe the recording dates show the order.")
                         : wrongTries == 2 ? Settings.T("Debería comparar las horas y lo que ocurre en cada cinta.", "I should compare the times and what happens on each tape.")
                         : Settings.T("Alguien llega a casa antes de servirse agua... y una de las cintas es del DOMINGO, después de medianoche.", "Someone gets home before pouring water... and one tape is from SUNDAY, after midnight.");
                Feedback("<color=#ff6655>" + Settings.T("SECUENCIA INCONSISTENTE. ", "INCONSISTENT SEQUENCE. ") + "</color>" + h, true, 8f);
            }
        }

        void Solve()
        {
            Reconstructed = true; selected = null;
            for (int i = 0; i < 3; i++) seq[i] = correctOrder[i];
            if (AudioManager.I) AudioManager.I.UI(AudioManager.I.beep, 0.9f);
            Stop(false);
            Feedback(Settings.T("La secuencia encaja. GUARDIÁN reconstruyó la noche.", "The sequence fits. GUARDIAN rebuilt the night."), false, 20f);
            GameEvents.Raise("tapes_reconstructed");
        }

        IEnumerator WrongFX()
        {
            tv.showStatic = true; tv.message = Settings.T("<color=#ff5544>SECUENCIA INCONSISTENTE</color>", "<color=#ff5544>INCONSISTENT SEQUENCE</color>");
            yield return new WaitForSeconds(1.2f);
            if (playing == null) Stop(false);
        }

        void Feedback(string s, bool bad = false, float dur = 5f) { feedback = s; feedbackT = dur; if (fb) fb.color = bad ? new Color(1f, 0.75f, 0.6f) : new Color(0.85f, 1f, 0.85f); }

        public void ForceReconstructed() { Reconstructed = true; foreach (var t in tapes) watched.Add(t.id); }

        /// <summary>Recorrido automático: marca vistas y resuelve con el orden correcto.</summary>
        public void DebugSolve() { foreach (var t in tapes) MarkWatched(t); if (!Reconstructed) Solve(); }

        // ------------------------------------------------------------------ foco
        public override void OnEnter() { BuildUI(); GameEvents.Raise("vcr_opened"); }
        public override void OnExit() { if (ui) ui.gameObject.SetActive(false); selected = null; }

        public override void OnFocusTick()
        {
            if (slot != null)
            {
                var n = NextTape();
                slot.labelEs = n != null ? "Insertar cinta " + n.letter : (playing != null ? "Reproduciendo…" : "No tienes más cintas");
                slot.labelEn = n != null ? "Insert tape " + n.letter : (playing != null ? "Playing…" : "No more tapes");
            }
            if (ui == null) return;
            if (!ui.gameObject.activeSelf) ui.gameObject.SetActive(true);
            for (int i = 0; i < 3; i++) if (cards[i].Tick()) OnCard(tapes[i]);
            for (int i = 0; i < 3; i++) if (slotBtns[i].Tick()) OnSlot(i);
            if (check.Tick()) Check();
            if (stopBtn.Tick()) Stop(true);
            if (exitBtn.Tick() && FocusSystem.I) FocusSystem.I.Exit();
            Refresh();
        }

        void BuildUI()
        {
            if (ui != null || UIManager.I == null) return;
            ui = UIKit.Rect("Videograbadora_UI", UIManager.I.Panels); UIKit.Stretch(ui);
            var bg = UIKit.Img("Fondo", ui, new Color(0.01f, 0.012f, 0.016f, 0.93f)); UIKit.Stretch(bg.rectTransform);
            title = UIKit.Txt("Titulo", ui, 34, TextAnchor.MiddleCenter, UIKit.Accent);
            UIKit.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -24), new Vector2(1600, 50));
            title.text = Settings.T("VIDEOGRABADORA · LAS CINTAS DE PAPÁ", "VCR · DAD'S TAPES");
            // pantalla
            var frame = UIKit.Img("Marco", ui, new Color(0.16f, 0.17f, 0.19f, 1f));
            UIKit.Place(frame.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-330, -90), new Vector2(720, 470));
            feed = UIKit.Raw("Imagen", frame.rectTransform); UIKit.Stretch(feed.rectTransform, 10, 10, 10, 10);
            screenMsg = UIKit.Txt("Mensaje", feed.rectTransform, 30, TextAnchor.MiddleCenter, Color.white); UIKit.Stretch(screenMsg.rectTransform, 24, 24, 24, 24);
            // instrucciones
            var hb = UIKit.Img("AyudaFondo", ui, new Color(0.06f, 0.065f, 0.08f, 0.95f));
            UIKit.Place(hb.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(430, -90), new Vector2(740, 470));
            help = UIKit.Txt("Ayuda", hb.rectTransform, 25, TextAnchor.UpperLeft, new Color(0.92f, 0.92f, 0.88f)); UIKit.Stretch(help.rectTransform, 26, 20, 26, 20);
            // cintas
            counter = UIKit.Txt("Contador", ui, 26, TextAnchor.MiddleLeft, UIKit.Accent);
            UIKit.Place(counter.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-170, 452), new Vector2(1500, 36));
            for (int i = 0; i < 3; i++)
                cards[i] = new UIButton(ui, "", new Vector2(0.5f, 0f), new Vector2(-560 + i * 330, 370), new Vector2(310, 120), 22);
            // secuencia
            seqHead = UIKit.Txt("Secuencia", ui, 26, TextAnchor.MiddleLeft, UIKit.Accent);
            UIKit.Place(seqHead.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-170, 278), new Vector2(1500, 36));
            for (int i = 0; i < 3; i++)
                slotBtns[i] = new UIButton(ui, "", new Vector2(0.5f, 0f), new Vector2(-560 + i * 330, 200), new Vector2(310, 110), 22);
            check = new UIButton(ui, Settings.T("▶  REPRODUCIR SECUENCIA", "▶  PLAY SEQUENCE"), new Vector2(0.5f, 0f), new Vector2(560, 225), new Vector2(330, 72), 24);
            stopBtn = new UIButton(ui, Settings.T("■  DETENER CINTA", "■  STOP TAPE"), new Vector2(0.5f, 0f), new Vector2(560, 370), new Vector2(330, 72), 24);
            exitBtn = new UIButton(ui, Settings.T("SALIR  [Esc]", "EXIT  [Esc]"), new Vector2(0.5f, 0f), new Vector2(560, 130), new Vector2(330, 64), 24);
            fb = UIKit.Txt("Aviso", ui, 27, TextAnchor.MiddleCenter, Color.white);
            UIKit.Place(fb.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-170, 75), new Vector2(1300, 80));
            ui.gameObject.SetActive(false);
        }

        void Refresh()
        {
            bool blink = Mathf.FloorToInt(Time.unscaledTime * 2f) % 2 == 0;
            // pantalla
            if (playing != null && !tv.showStatic) { feed.texture = tapeRT; feed.color = Color.white; screenMsg.text = ""; }
            else
            {
                feed.texture = tv.showStatic && tv.staticMat != null ? tv.staticMat.mainTexture : null;
                feed.color = feed.texture != null ? Color.white : new Color(0.02f, 0.03f, 0.05f);
                feed.uvRect = tv.showStatic ? new Rect(Random.value, Random.value, 1, 1) : new Rect(0, 0, 1, 1);
                screenMsg.text = tv.showStatic ? "" : (Reconstructed ? ReconText().Replace("<size=17>", "<size=24>")
                    : OrderingPhase ? Settings.T("GUARDIÁN · RECONSTRUCCIÓN\n<size=22>Ordene las grabaciones: la más antigua primero</size>", "GUARDIAN · RECONSTRUCTION\n<size=22>Order the recordings: oldest first</size>")
                    : Settings.T("SIN SEÑAL\n<size=22>Elige una cinta abajo para verla</size>", "NO SIGNAL\n<size=22>Pick a tape below to watch it</size>"));
            }
            screenMsg.alignment = playing != null ? TextAnchor.UpperLeft : TextAnchor.MiddleCenter;
            if (playing != null) screenMsg.text = (blink ? "<color=#ff4433>● REC</color>   " : "<color=#ff4433>   REC</color>   ") + Stamp(playing);
            // instrucciones según la fase
            if (Reconstructed)
                help.text = Settings.T("<b>Secuencia reconstruida.</b>\n\nGUARDIÁN ordenó la noche. Hay un registro que no debería existir: un SUJETO 2 en el dormitorio, sin entrar por ninguna puerta.\n\nPuedes salir con <b>[Esc]</b>.",
                                       "<b>Sequence reconstructed.</b>\n\nGUARDIAN put the night in order. There's a log that shouldn't exist: a SUBJECT 2 in the bedroom, without coming in through any door.\n\nYou can leave with <b>[Esc]</b>.");
            else if (!OrderingPhase)
                help.text = Settings.T("<b>1 · MIRA LAS CINTAS</b>\nHaz <b>clic en una cinta</b> de abajo para reproducirla. Cada grabación dura unos segundos; fíjate en la <b>fecha, la hora</b> y en lo que ocurre.\n\n<b>2 · ORDÉNALAS</b>\nCuando hayas visto las tres se desbloquean las posiciones 1º · 2º · 3º.\n\nCintas vistas: <b>" + watched.Count + "/3</b>" + (FoundCount() < 3 ? "\n\n<color=#ffcc88>Te faltan cintas: búscalas por la casa.</color>" : ""),
                                       "<b>1 · WATCH THE TAPES</b>\n<b>Click a tape</b> below to play it. Each recording lasts a few seconds; pay attention to the <b>date, the time</b> and what happens.\n\n<b>2 · ORDER THEM</b>\nOnce you've watched all three, positions 1st · 2nd · 3rd unlock.\n\nTapes watched: <b>" + watched.Count + "/3</b>" + (FoundCount() < 3 ? "\n\n<color=#ffcc88>Tapes missing: search the house.</color>" : ""));
            else
                help.text = Settings.T("<b>Estas grabaciones parecen estar fuera de orden.</b>\nDebo reconstruir la secuencia de los acontecimientos.\n\n1. Haz <b>clic en una cinta</b> para seleccionarla (se ilumina).\n2. Haz <b>clic en una posición</b> (1º = la más antigua) para colocarla.\n3. Clic en una posición ocupada la retira. Puedes intercambiarlas.\n4. Pulsa <b>REPRODUCIR SECUENCIA</b> para comprobar.",
                                       "<b>These recordings seem out of order.</b>\nI need to rebuild the sequence of events.\n\n1. <b>Click a tape</b> to select it (it lights up).\n2. <b>Click a position</b> (1st = oldest) to place it.\n3. Clicking a filled position removes it. You can swap them.\n4. Press <b>PLAY SEQUENCE</b> to check.");
            counter.text = Settings.T("CINTAS ENCONTRADAS: ", "TAPES FOUND: ") + FoundCount() + "/3" + Settings.T("   ·   VISTAS: ", "   ·   WATCHED: ") + watched.Count + "/3";
            // tarjetas
            for (int i = 0; i < 3; i++)
            {
                var t = tapes[i]; var c = cards[i];
                bool found = Found(t), w = watched.Contains(t.id), placed = System.Array.IndexOf(seq, t.id) >= 0;
                string state = !found ? Settings.T("<color=#888888>No encontrada</color>", "<color=#888888>Not found</color>")
                             : !w ? (playing == t ? Settings.T("<color=#ff6655>● Reproduciendo…</color>", "<color=#ff6655>● Playing…</color>") : Settings.T("<color=#ffd27a>Sin ver · clic para ver</color>", "<color=#ffd27a>Not watched · click to play</color>"))
                             : "<color=#9dff9d>" + Stamp(t) + " · " + Place(t.id) + "</color>" + (OrderingPhase ? (placed ? Settings.T("\n(colocada)", "\n(placed)") : Settings.T("\nclic para seleccionar", "\nclick to select")) : "");
                c.SetText("<b>" + Settings.T("CINTA ", "TAPE ") + t.letter + "</b>  <size=18>«" + t.letter + " — " + Place(t.id) + "»</size>\n<size=20>" + state + "</size>");
                c.interactable = !Reconstructed && found && !(OrderingPhase && placed && selected != t.id);
                if (selected == t.id) c.bg.color = blink ? new Color(0.75f, 0.6f, 0.15f, 1f) : new Color(0.55f, 0.42f, 0.1f, 1f);
                else if (playing == t) c.bg.color = new Color(0.4f, 0.12f, 0.1f, 1f);
            }
            // posiciones
            seqHead.text = OrderingPhase || Reconstructed ? Settings.T("SECUENCIA  (1º = la más antigua)", "SEQUENCE  (1st = oldest)")
                                                          : Settings.T("SECUENCIA  <color=#888888>(bloqueada: mira las 3 cintas, " + watched.Count + "/3)</color>", "SEQUENCE  <color=#888888>(locked: watch all 3 tapes, " + watched.Count + "/3)</color>");
            string[] ord = { Settings.T("1º", "1st"), Settings.T("2º", "2nd"), Settings.T("3º", "3rd") };
            for (int i = 0; i < 3; i++)
            {
                var s = slotBtns[i]; var t = seq[i] != null ? ById(seq[i]) : null;
                s.SetText(ord[i] + "\n" + (t != null ? "<b>" + Settings.T("CINTA ", "TAPE ") + t.letter + "</b> <size=18>" + Stamp(t) + "</size>" : "<color=#777777>" + Settings.T("— vacío —", "— empty —") + "</color>"));
                s.interactable = OrderingPhase;
                if (OrderingPhase && selected != null && blink && s.bg.color != s.hover) s.bg.color = new Color(0.2f, 0.26f, 0.2f, 1f);
                if (Reconstructed) s.bg.color = new Color(0.12f, 0.3f, 0.16f, 1f);
            }
            bool full = seq[0] != null && seq[1] != null && seq[2] != null;
            check.interactable = OrderingPhase && full;
            check.SetActive(!Reconstructed);
            stopBtn.interactable = playing != null;
            if (feedbackT > 0f) { feedbackT -= Time.unscaledDeltaTime; fb.text = feedback; }
            else fb.text = OrderingPhase && selected == null && !full ? Settings.T("Selecciona una cinta y colócala en una posición.", "Select a tape and place it in a position.") : "";
        }
    }
}
