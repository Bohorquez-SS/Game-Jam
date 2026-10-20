using UnityEngine;

namespace GNight
{
    /// <summary>Teclado físico de la alarma GUARDIÁN (botones 3D que se hunden, pantalla verde).</summary>
    public class AlarmKeypadStation : FocusStation
    {
        public PushButton[] digits = new PushButton[10];
        public PushButton clearBtn, okBtn;
        public TextMesh screen;
        string entry = ""; float errT, okT;

        void Start()
        {
            for (int i = 0; i < 10; i++) if (digits[i]) { int d = i; digits[i].Pressed = b => Press(d); }
            if (clearBtn) clearBtn.Pressed = b => { entry = ""; };
            if (okBtn) okBtn.Pressed = b => Submit();
        }

        public override string Hint => Settings.T("[Clic] o [0-9] teclear · [Enter] confirmar · [Esc] alejarse", "[Click] or [0-9] type · [Enter] confirm · [Esc] step back");

        void Press(int d)
        {
            var a = AlarmSystem.I;
            if (a != null && a.Armed) return;
            if (entry.Length < 4) entry += d;
            if (entry.Length == 4) Submit();
        }

        void Submit()
        {
            var a = AlarmSystem.I;
            if (a == null || entry.Length < 4) return;
            if (!a.Powered) { errT = 1.5f; entry = ""; if (AudioManager.I) AudioManager.I.UI(AudioManager.I.error); return; }
            if (a.TryCode(entry)) { okT = 4f; if (AudioManager.I) AudioManager.I.UI(AudioManager.I.beep); }
            else { errT = 1.6f; if (AudioManager.I) AudioManager.I.UI(AudioManager.I.error); }
            entry = "";
        }

        public override void OnFocusTick()
        {
            int d = Inp.Digit();
            if (d >= 0) { if (digits[d]) digits[d].Click(); else Press(d); }
            if (Inp.Back()) entry = entry.Length > 0 ? entry.Substring(0, entry.Length - 1) : "";
            if (Inp.Enter()) Submit();
        }

        void Update()
        {
            if (screen == null) return;
            var a = AlarmSystem.I;
            errT -= Time.deltaTime; okT -= Time.deltaTime;
            if (a != null && !a.Powered) { screen.text = Mathf.FloorToInt(Time.time * 2) % 2 == 0 ? "-- : --" : ""; screen.color = new Color(0.6f, 0.15f, 0.1f); return; }
            if (a != null && a.Armed)
            {
                screen.color = new Color(0.35f, 1f, 0.45f);
                screen.text = okT > 0f ? Settings.T("ALARMA ACTIVADA", "ALARM ARMED") : a.StatusLine;
                return;
            }
            screen.color = errT > 0f ? new Color(1f, 0.3f, 0.2f) : new Color(0.35f, 1f, 0.45f);
            string s = ""; for (int i = 0; i < 4; i++) s += (i < entry.Length ? "*" : "_") + " ";
            screen.text = errT > 0f ? Settings.T("PIN INCORRECTO", "WRONG PIN") : Settings.T("PIN (DDMM)  ", "PIN (DDMM)  ") + s;
        }
    }
}
