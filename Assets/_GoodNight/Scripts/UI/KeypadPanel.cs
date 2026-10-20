using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>Teclado de la alarma GUARDIÁN (ratón o números del teclado).</summary>
    public class KeypadPanel : UIPanel
    {
        Text display, status;
        UIButton[] keys; UIButton clear, ok, exit;
        string entry = "";
        float msgT;

        public override void Build(RectTransform layer)
        {
            root = UIKit.Rect("Keypad", layer); UIKit.Stretch(root);
            var dim = UIKit.Img("Dim", root, new Color(0, 0, 0, 0.6f)); UIKit.Stretch(dim.rectTransform);
            var box = UIKit.Img("Box", root, new Color(0.1f, 0.1f, 0.11f, 1f));
            UIKit.Center(box.rectTransform, Vector2.zero, new Vector2(560, 860));
            var scr = UIKit.Img("Screen", box.rectTransform, new Color(0.02f, 0.06f, 0.03f, 1f));
            UIKit.Place(scr.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(480, 220));
            display = UIKit.Txt("Display", scr.rectTransform, 30, TextAnchor.MiddleCenter, UIKit.Green, false);
            UIKit.Stretch(display.rectTransform, 10, 10, 10, 10);
            keys = new UIButton[10];
            for (int i = 1; i <= 9; i++)
            {
                int r = (i - 1) / 3, c = (i - 1) % 3;
                keys[i] = new UIButton(box.rectTransform, i.ToString(), new Vector2(0.5f, 1), new Vector2(-130 + c * 130, -320 - r * 110), new Vector2(110, 90), 40);
            }
            clear = new UIButton(box.rectTransform, "C", new Vector2(0.5f, 1), new Vector2(-130, -650), new Vector2(110, 90), 36);
            keys[0] = new UIButton(box.rectTransform, "0", new Vector2(0.5f, 1), new Vector2(0, -650), new Vector2(110, 90), 40);
            ok = new UIButton(box.rectTransform, "OK", new Vector2(0.5f, 1), new Vector2(130, -650), new Vector2(110, 90), 32);
            exit = new UIButton(box.rectTransform, Settings.T("Salir [Esc]", "Exit [Esc]"), new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(300, 56), 24);
            status = UIKit.Txt("Status", root, 26, TextAnchor.MiddleCenter, Color.white);
            UIKit.Center(status.rectTransform, new Vector2(0, -470), new Vector2(1000, 40));
        }

        public override void OnOpen() { entry = ""; msgT = 0; status.text = ""; Refresh(); }

        public override void Tick()
        {
            var alarm = AlarmSystem.I;
            if (alarm != null && alarm.Armed) { if (Inp.Click() || Inp.Enter()) UIManager.I.Close(this); exit.Tick(); Refresh(); return; }
            int d = Inp.Digit();
            for (int i = 0; i < 10; i++) if (keys[i].Tick()) d = i;
            if (d >= 0 && entry.Length < 4) { entry += d; Beep(); }
            if (clear.Tick() || Inp.Back()) { entry = entry.Length > 0 ? entry.Substring(0, entry.Length - 1) : ""; Beep(); }
            if ((ok.Tick() || Inp.Enter()) && entry.Length == 4) Submit();
            if (exit.Tick()) UIManager.I.Close(this);
            if (msgT > 0) { msgT -= Time.unscaledDeltaTime; if (msgT <= 0) entry = ""; }
            Refresh();
        }

        void Beep() { if (AudioManager.I) AudioManager.I.UI(AudioManager.I.keyBeep, 0.6f); }

        void Submit()
        {
            var alarm = AlarmSystem.I;
            if (alarm == null) return;
            if (!alarm.Powered) { msgT = 1.5f; if (AudioManager.I) AudioManager.I.UI(AudioManager.I.error); return; }
            if (alarm.TryCode(entry)) { if (AudioManager.I) AudioManager.I.UI(AudioManager.I.beep); }
            else { msgT = 1.6f; if (AudioManager.I) AudioManager.I.UI(AudioManager.I.error); }
        }

        void Refresh()
        {
            var alarm = AlarmSystem.I;
            if (alarm != null && !alarm.Powered) { display.text = Settings.T("SIN ENERGÍA", "NO POWER"); return; }
            if (alarm != null && alarm.Armed)
            {
                display.text = "<size=24>GUARDIÁN</size>\n" + Settings.T("ALARMA ACTIVADA\nZONAS PROTEGIDAS: 06\n<color=#ffcc55>MOVIMIENTO DETECTADO: PASILLO</color>",
                                                                      "ALARM ARMED\nPROTECTED ZONES: 06\n<color=#ffcc55>MOTION DETECTED: HALLWAY</color>");
                status.text = Settings.T("[clic] continuar", "[click] continue");
                return;
            }
            var sb = new StringBuilder();
            for (int i = 0; i < 4; i++) sb.Append(i < entry.Length ? entry[i].ToString() : "_").Append(' ');
            if (msgT > 0) display.text = Settings.T("<color=#ff5544>PIN INCORRECTO</color>", "<color=#ff5544>WRONG PIN</color>") + "\n\n" + sb;
            else display.text = Settings.T("INTRODUZCA PIN\nFORMATO DDMM", "ENTER PIN\nFORMAT DDMM") + "\n\n<size=44>" + sb + "</size>";
        }
    }
}
