using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>
    /// Consola de aislamiento (habitación 06). Muestra sensores de movimiento (con "ecos") y el estado de energía.
    /// Los monitores CRT de la sala muestran las cámaras: las congeladas (REC, hora vieja) no son de fiar.
    /// Botones: AISLAR COCINA · AISLAR DORMITORIO · (si descubriste la verdad) AISLAR 06.
    /// </summary>
    public class IsolationStation : FocusStation
    {
        public static readonly HashSet<string> Motion = new HashSet<string>();
        public PushButton isoKitchen, isoBedroom, isoSelf;
        public TextMesh screen;
        public bool selfOptionVisible;

        void Start()
        {
            if (isoKitchen) isoKitchen.Pressed = b => Press("cocina");
            if (isoBedroom) isoBedroom.Pressed = b => Press("dormitorio");
            if (isoSelf) isoSelf.Pressed = b => Press("secreta");
        }

        public override string Hint => Settings.T("Compara sensores y cámaras · [Clic] aislar · [Esc] alejarse", "Compare sensors and cameras · [Click] isolate · [Esc] step back");

        // ---- interfaz a pantalla completa (18/10): sensores + cámaras COCINA/DORMITORIO lado a lado + botones grandes
        RectTransform ui; RawImage feedK, feedB; Text title, power, sensors, camK, camB, help;
        UIButton bK, bB, bS, bExit;
        public override bool ConsumesMouse => ui != null && ui.gameObject.activeSelf;
        public override void OnEnter() { BuildUI(); }
        public override void OnExit() { if (ui) ui.gameObject.SetActive(false); }

        public override void OnFocusTick()
        {
            if (ui == null) return;
            if (!ui.gameObject.activeSelf) ui.gameObject.SetActive(true);
            bS.SetActive(selfOptionVisible);
            if (bK.Tick()) Click(isoKitchen, "cocina");
            if (bB.Tick()) Click(isoBedroom, "dormitorio");
            if (selfOptionVisible && bS.Tick()) Click(isoSelf, "secreta");
            if (bExit.Tick() && FocusSystem.I) FocusSystem.I.Exit();
            Refresh();
        }

        void Click(PushButton b, string zone) { if (b != null) b.Click(); else Press(zone); }

        void BuildUI()
        {
            if (ui != null || UIManager.I == null) return;
            ui = UIKit.Rect("Aislamiento_UI", UIManager.I.Panels); UIKit.Stretch(ui);
            var bg = UIKit.Img("Fondo", ui, new Color(0.01f, 0.015f, 0.012f, 1f)); UIKit.Stretch(bg.rectTransform);
            title = UIKit.Txt("Titulo", ui, 34, TextAnchor.MiddleCenter, new Color(0.55f, 1f, 0.6f));
            UIKit.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -22), new Vector2(1600, 46));
            title.text = Settings.T("GUARDIÁN · CONSOLA DE AISLAMIENTO", "GUARDIAN · ISOLATION CONSOLE");
            power = UIKit.Txt("Energia", ui, 26, TextAnchor.MiddleCenter, Color.white);
            UIKit.Place(power.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -70), new Vector2(1600, 36));
            // sensores (izquierda)
            var sb = UIKit.Img("SensoresFondo", ui, new Color(0.04f, 0.06f, 0.05f, 1f));
            UIKit.Place(sb.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-700, -120), new Vector2(380, 470));
            sensors = UIKit.Txt("Sensores", sb.rectTransform, 30, TextAnchor.UpperLeft, new Color(0.55f, 1f, 0.6f)); UIKit.Stretch(sensors.rectTransform, 22, 16, 16, 18);
            // cámaras
            feedK = Feed("Cam_Cocina", new Vector2(-200, -120), out camK);
            feedB = Feed("Cam_Dormitorio", new Vector2(420, -120), out camB);
            help = UIKit.Txt("Ayuda", ui, 24, TextAnchor.UpperCenter, new Color(0.88f, 0.9f, 0.86f));
            UIKit.Place(help.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 160), new Vector2(1700, 120));
            bK = new UIButton(ui, Settings.T("AISLAR COCINA", "ISOLATE KITCHEN"), new Vector2(0.5f, 0f), new Vector2(-560, 90), new Vector2(330, 80), 26);
            bB = new UIButton(ui, Settings.T("AISLAR DORMITORIO", "ISOLATE BEDROOM"), new Vector2(0.5f, 0f), new Vector2(-190, 90), new Vector2(330, 80), 26);
            bS = new UIButton(ui, Settings.T("AISLAR 06 (AQUÍ)", "ISOLATE 06 (HERE)"), new Vector2(0.5f, 0f), new Vector2(180, 90), new Vector2(330, 80), 26);
            bExit = new UIButton(ui, Settings.T("SALIR  [Esc]", "EXIT  [Esc]"), new Vector2(0.5f, 0f), new Vector2(560, 90), new Vector2(260, 80), 24);
            ui.gameObject.SetActive(false);
        }

        RawImage Feed(string name, Vector2 pos, out Text label)
        {
            var fr = UIKit.Img(name, ui, new Color(0.16f, 0.17f, 0.19f, 1f));
            UIKit.Place(fr.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), pos, new Vector2(600, 470));
            var raw = UIKit.Raw("Imagen", fr.rectTransform); UIKit.Stretch(raw.rectTransform, 8, 8, 8, 52);
            label = UIKit.Txt("Rotulo", fr.rectTransform, 26, TextAnchor.LowerCenter, Color.white); UIKit.Stretch(label.rectTransform, 8, 10, 8, 0);
            return raw;
        }

        void Refresh()
        {
            bool ok = PowerPanel.I == null || PowerPanel.I.SecurityComplete;
            bool blink = Mathf.FloorToInt(Time.unscaledTime * 2f) % 2 == 0;
            power.text = ok ? Settings.T("<color=#6dff8a>ENERGÍA OK · ALARMA + CÁMARAS + CERRADURAS</color>", "<color=#6dff8a>POWER OK · ALARM + CAMERAS + LOCKS</color>")
                            : Settings.T("<color=#ff6655>ENERGÍA INSUFICIENTE · enciende ALARMA + CÁMARAS + CERRADURAS en el cuadro de la cocina (7)</color>", "<color=#ff6655>LOW POWER · turn on ALARM + CAMERAS + LOCKS at the kitchen panel (7)</color>");
            var sbd = new System.Text.StringBuilder(Settings.T("<b>SENSORES DE MOVIMIENTO</b>\n\n", "<b>MOTION SENSORS</b>\n\n"));
            foreach (var z in new[] { "sala", "cocina", "pasillo", "dormitorio" })
            {
                var rz = RoomZone.Get(z);
                string n = rz != null ? rz.DisplayName.ToUpperInvariant() : z;
                sbd.Append(Motion.Contains(z) ? "<color=#ff5544>●</color> " : "<color=#335533>○</color> ").Append(n).Append("\n\n");
            }
            sbd.Append(Settings.T("<size=21>● fijo = algo real\n● parpadeo con ritmo fijo = ECO</size>", "<size=21>● steady = something real\n● steady-rhythm blink = ECHO</size>"));
            sensors.text = sbd.ToString();
            var sys = SecurityCameraSystem.I;
            CamTo(feedK, camK, sys != null ? sys.ByZone("cocina") : null, Settings.T("COCINA", "KITCHEN"), blink);
            CamTo(feedB, camB, sys != null ? sys.ByZone("dormitorio") : null, Settings.T("DORMITORIO", "BEDROOM"), blink);
            help.text = Settings.T("Compara: ¿el sensor marca movimiento <b>fijo</b>? ¿la cámara está en <b>DIRECTO</b>? Aísla solo la zona donde coinciden.  ·  REC con hora vieja = cámara CONGELADA.",
                                   "Compare: is the sensor <b>steady</b>? is the camera <b>LIVE</b>? Only isolate the zone where both agree.  ·  REC with an old time = FROZEN camera.");
            bK.interactable = bB.interactable = bS.interactable = ok;
        }

        static void CamTo(RawImage img, Text label, SecurityCam c, string zone, bool blink)
        {
            var sys = SecurityCameraSystem.I;
            bool powered = SecurityCameraSystem.Powered;
            if (c != null && sys != null && powered && !c.frozen) sys.Request(c);
            img.texture = c != null && powered ? c.rt : null;
            img.color = img.texture != null ? Color.white : Color.black;
            string st = c == null || !powered ? Settings.T("<color=#888888>SIN SEÑAL</color>", "<color=#888888>NO SIGNAL</color>")
                      : c.frozen ? "<color=#ff4433>" + (blink ? "● " : "   ") + "REC " + c.frozenStamp + "</color>"
                      : "<color=#55ff77>" + (blink ? "● " : "   ") + Settings.T("DIRECTO", "LIVE") + "</color>";
            label.text = (c != null ? c.camName + " · " : "") + zone + "   " + st;
        }

        public override string Instructions => Settings.T(
            "<b>CONSOLA DE AISLAMIENTO</b>\nSENSORES: punto <b>fijo</b> = algo real · parpadeo con ritmo fijo = <b>ECO</b>.\nCÁMARAS (monitores): <b>DIRECTO</b> = imagen real · <b>REC</b> con hora vieja = <b>CONGELADA</b>.\nAísla solo la zona donde coincidan un sensor fijo y una cámara en directo.",
            "<b>ISOLATION CONSOLE</b>\nSENSORS: <b>steady</b> dot = something real · blinking at a steady rhythm = <b>ECHO</b>.\nCAMERAS (monitors): <b>LIVE</b> = real image · <b>REC</b> with an old time = <b>FROZEN</b>.\nOnly isolate the zone where a steady sensor and a live camera agree.");

        void Press(string zone)
        {
            if (PowerPanel.I != null && !PowerPanel.I.SecurityComplete)
            {
                if (AudioManager.I) AudioManager.I.UI(AudioManager.I.error, 0.8f);
                return;
            }
            if (FocusSystem.I) FocusSystem.I.Exit();
            GameEvents.Raise("isolate:" + zone);
        }

        void Update()
        {
            if (isoSelf) isoSelf.gameObject.SetActive(selfOptionVisible);
            if (screen == null) return;
            bool ok = PowerPanel.I == null || PowerPanel.I.SecurityComplete;
            var sb = new System.Text.StringBuilder();
            sb.Append(ok ? Settings.T("ENERGÍA: OK", "POWER: OK") : Settings.T("ENERGÍA: INSUFICIENTE\nA+C+L EN EL CUADRO", "POWER: LOW\nA+C+L AT PANEL")).Append('\n');
            sb.Append(Settings.T("SENSORES:", "SENSORS:")).Append('\n');
            foreach (var z in new[] { "sala", "cocina", "pasillo", "dormitorio" })
            {
                var rz = RoomZone.Get(z);
                string n = rz != null ? rz.DisplayName.ToUpperInvariant() : z;
                sb.Append(Motion.Contains(z) ? "● " : "○ ").Append(n).Append('\n');
            }
            screen.text = sb.ToString();
            if (isoKitchen) isoKitchen.interactable = ok;
            if (isoBedroom) isoBedroom.interactable = ok;
            if (isoSelf) isoSelf.interactable = ok;
        }
    }
}
