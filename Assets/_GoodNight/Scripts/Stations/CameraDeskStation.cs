using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>
    /// Escritorio de vigilancia: monitor CRT + selector físico de 5 botones (CAM 01–04 y ARCHIVO/DIRECTO).
    /// Modo archivo = grabación vieja del 23/08 (con registro). Directo requiere energía en el circuito de cámaras.
    /// </summary>
    public class CameraDeskStation : FocusStation
    {
        public MonitorScreen monitor;
        public PushButton[] camButtons = new PushButton[4];
        public PushButton modeButton;
        public int index = 1;
        [HideInInspector] public bool unavailable;
        float interf, errT;

        // ---- interfaz de vigilancia a pantalla grande (se abre al acercarse al monitor)
        RectTransform ui; RawImage feed; Text camTitle, status, center, motion, help;
        UIButton[] uiCams = new UIButton[4]; UIButton uiMode, uiExit;
        static readonly string[] ZoneEs = { "SALA", "PASILLO", "COCINA", "DORMITORIO" }, ZoneEn = { "LIVING ROOM", "HALLWAY", "KITCHEN", "BEDROOM" };
        public override bool ConsumesMouse => ui != null && ui.gameObject.activeSelf;

        void Start()
        {
            for (int i = 0; i < camButtons.Length; i++) if (camButtons[i]) { int k = i; camButtons[i].Pressed = b => Select(k); }
            if (modeButton) modeButton.Pressed = b => ToggleMode();
            Select(index);
        }

        public override string Hint => Settings.T("[Clic] botones: CAM 1–4 · ARCHIVO/DIRECTO · [1-4] cámara · [Esc] alejarse", "[Click] buttons: CAM 1–4 · ARCHIVE/LIVE · [1-4] camera · [Esc] step back");

        public void Select(int i)
        {
            var sys = SecurityCameraSystem.I; if (sys == null || sys.cams.Length == 0) return;
            index = Mathf.Clamp(i, 0, sys.cams.Length - 1);
            monitor.source = sys.cams[index];
            interf = 0.25f;
            if (AudioManager.I) AudioManager.I.UI(AudioManager.I.staticBurst, 0.18f);
            GameEvents.Raise("cam_select:" + sys.cams[index].zoneId);
        }

        void ToggleMode()
        {
            var sys = SecurityCameraSystem.I; if (sys == null) return;
            if (sys.recordingMode)
            {
                if (!SecurityCameraSystem.Powered)
                {
                    if (AudioManager.I) AudioManager.I.UI(AudioManager.I.error);
                    monitor.message = Settings.T("<color=#ff5544>ERROR: ENERGÍA INSUFICIENTE</color>\n<size=18>CIRCUITO CÁMARAS · F2</size>", "<color=#ff5544>ERROR: INSUFFICIENT POWER</color>\n<size=18>CAMERA CIRCUIT · F2</size>");
                    interf = 1.2f; errT = 5f;
                    GameEvents.Raise("camera_live_failed");
                    return;
                }
                sys.recordingMode = false; interf = 0.8f;
                GameEvents.Raise("camera_live");
            }
            else { sys.recordingMode = true; GameEvents.Raise("camera_archive"); }
        }

        public override void OnEnter() { BuildUI(); GameEvents.Raise("camera_monitor_opened"); }
        public override void OnExit() { if (ui) ui.gameObject.SetActive(false); GameEvents.Raise("camera_monitor_closed"); }

        public override void OnFocusTick()
        {
            if (ui != null && !ui.gameObject.activeSelf) ui.gameObject.SetActive(true);
            int d = Inp.Digit();
            if (d >= 1 && d <= 4) Press(camButtons[d - 1], () => Select(d - 1));
            if (ui == null) return;
            for (int i = 0; i < 4; i++) { int k = i; if (uiCams[i].Tick()) Press(camButtons[k], () => Select(k)); }
            if (uiMode.Tick() || Inp.Space()) Press(modeButton, ToggleMode);
            if (uiExit.Tick() && FocusSystem.I) FocusSystem.I.Exit();
        }

        /// <summary>Pulsa el botón físico (animación + sonido + evento) o, si no existe, ejecuta la acción.</summary>
        static void Press(PushButton b, System.Action fallback) { if (b != null) b.Click(); else fallback(); }

        void BuildUI()
        {
            if (ui != null || UIManager.I == null) return;
            ui = UIKit.Rect("Vigilancia", UIManager.I.Panels); UIKit.Stretch(ui);
            var bg = UIKit.Img("Fondo", ui, new Color(0.01f, 0.012f, 0.016f, 0.94f)); UIKit.Stretch(bg.rectTransform);
            var frame = UIKit.Img("Marco", ui, new Color(0.16f, 0.17f, 0.19f, 1f));
            UIKit.Center(frame.rectTransform, new Vector2(0, 70), new Vector2(1180, 885));
            feed = UIKit.Raw("Imagen", frame.rectTransform); UIKit.Stretch(feed.rectTransform, 14, 14, 14, 14);
            camTitle = UIKit.Txt("Camara", feed.rectTransform, 34, TextAnchor.UpperLeft, Color.white); UIKit.Stretch(camTitle.rectTransform, 22, 0, 0, 16);
            status = UIKit.Txt("Estado", feed.rectTransform, 34, TextAnchor.UpperRight, Color.white); UIKit.Stretch(status.rectTransform, 0, 0, 22, 16);
            center = UIKit.Txt("Mensaje", feed.rectTransform, 44, TextAnchor.MiddleCenter, Color.white); UIKit.Stretch(center.rectTransform, 40, 40, 40, 40);
            motion = UIKit.Txt("Movimiento", feed.rectTransform, 32, TextAnchor.LowerLeft, new Color(1f, 0.35f, 0.25f)); UIKit.Stretch(motion.rectTransform, 22, 16, 0, 0);
            for (int i = 0; i < 4; i++)
                uiCams[i] = new UIButton(ui, "CAM 0" + (i + 1) + "\n<size=18>" + Settings.T(ZoneEs[i], ZoneEn[i]) + "</size>", new Vector2(0.5f, 0f), new Vector2(-690 + i * 200, 95), new Vector2(185, 82), 26);
            uiMode = new UIButton(ui, Settings.T("ARCHIVO / DIRECTO", "ARCHIVE / LIVE"), new Vector2(0.5f, 0f), new Vector2(225, 95), new Vector2(270, 82), 24);
            uiExit = new UIButton(ui, Settings.T("SALIR  [Esc]", "EXIT  [Esc]"), new Vector2(0.5f, 0f), new Vector2(570, 95), new Vector2(240, 82), 24);
            help = UIKit.Txt("Ayuda", ui, 22, TextAnchor.MiddleCenter, new Color(0.8f, 0.8f, 0.78f)); UIKit.Place(help.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(1600, 36));
            ui.gameObject.SetActive(false);
        }

        void UpdateUI()
        {
            if (ui == null || !ui.gameObject.activeSelf) return;
            var sys = SecurityCameraSystem.I;
            var src = monitor.source;
            if (src != null && sys != null && !monitor.showStatic) sys.Request(src);
            Texture t = monitor.showStatic && monitor.staticMat != null ? monitor.staticMat.mainTexture : (src != null ? src.rt : null);
            feed.texture = t; feed.color = t != null ? Color.white : Color.black;
            if (monitor.showStatic) feed.uvRect = new Rect(Random.value, Random.value, 1, 1); else feed.uvRect = new Rect(0, 0, 1, 1);
            bool blink = Mathf.FloorToInt(Time.time * 2f) % 2 == 0;
            bool rec = sys != null && sys.recordingMode || (src != null && src.frozen);
            camTitle.text = src != null ? src.camName : "";
            string stamp = !string.IsNullOrEmpty(monitor.stampOverride) ? monitor.stampOverride : (src != null && src.frozen ? src.frozenStamp : GameClock.Text);
            status.text = (rec ? "<color=#ff4433>" + (blink ? "● " : "   ") + "REC</color>   " : "<color=#55ff77>" + (blink ? "● " : "   ") + "LIVE</color>   ") + stamp;
            string msg = monitor.message ?? "";
            bool note = msg.Contains("<size=16>");     // nota pequeña (nombre del archivo): abajo, sin tapar la imagen
            string clean = System.Text.RegularExpressions.Regex.Replace(msg, "</?size[^>]*>", "");
            center.text = note ? "" : clean;
            var e = EnemyAI.I;
            bool mov = !rec && src != null && e != null && e.gameObject.activeInHierarchy && RoomZone.At(e.transform.position) == src.zoneId;
            motion.text = mov ? (blink ? Settings.T("(!) MOVIMIENTO DETECTADO", "(!) MOTION DETECTED") : "") : (note ? "<color=#cccccc><size=24>" + clean + "</size></color>" : "");
            for (int i = 0; i < 4; i++) uiCams[i].bg.color = i == index ? new Color(0.25f, 0.45f, 0.3f, 1f) : uiCams[i].bg.color;
            help.text = rec
                ? Settings.T("Estás viendo el ARCHIVO grabado (REC). Pulsa ARCHIVO / DIRECTO para ver la casa en vivo.  ·  [1-4] cámara  ·  [Espacio] archivo/directo",
                             "You're watching the recorded ARCHIVE (REC). Press ARCHIVE / LIVE to see the house live.  ·  [1-4] camera  ·  [Space] archive/live")
                : Settings.T("EN DIRECTO. Cambia de cámara para revisar cada habitación.  ·  [1-4] cámara  ·  [Espacio] archivo/directo",
                             "LIVE. Switch cameras to check every room.  ·  [1-4] camera  ·  [Space] archive/live");
        }

        void LateUpdate() { if (monitor != null) UpdateUI(); }

        void Update()
        {
            if (monitor == null || SecurityCameraSystem.I == null) return;
            var sys = SecurityCameraSystem.I;
            interf -= Time.deltaTime; errT -= Time.deltaTime;
            bool rec = sys.recordingMode;
            monitor.recLabel = rec;
            monitor.stampOverride = rec ? "23/08  23:12" : null;
            monitor.showStatic = unavailable || interf > 0f;
            if (errT > 0f && !unavailable) { /* deja el ERROR a la vista unos segundos */ }
            else if (unavailable) monitor.message = (monitor.source != null ? monitor.source.camName : "CAM") + Settings.T(": NO DISPONIBLE", ": UNAVAILABLE");
            else if (interf <= -2.5f && monitor.message != null && monitor.message.Contains("ERROR")) monitor.message = null;
            else if (!monitor.showStatic && rec && FocusSystem.Station == this)
                monitor.message = Settings.T("<size=16>ARCHIVO · CAM02_2019-08-23_23-12.gnv · BUCLE</size>", "<size=16>ARCHIVE · CAM02_2019-08-23_23-12.gnv · LOOP</size>");
            else if (!monitor.showStatic && (monitor.message == null || !monitor.message.Contains("ERROR"))) monitor.message = null;
        }
    }
}
