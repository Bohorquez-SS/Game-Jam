using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GNight
{
    /// <summary>Alarma GUARDIÁN: PIN 2308 (DDMM del cambio de cerradura).</summary>
    public class AlarmSystem : MonoBehaviour
    {
        public static AlarmSystem I;
        public string code = "2308";
        public bool Armed;
        public TextMesh screen;
        float blink;

        void Awake() { I = this; }

        public bool Powered => PowerPanel.I == null || PowerPanel.I.On(Circuit.Alarm);

        public bool TryCode(string c)
        {
            if (!Powered) return false;
            if (c == code)
            {
                Armed = true;
                GameEvents.Raise("alarm_armed");
                return true;
            }
            GameEvents.Raise("alarm_wrong");
            return false;
        }

        void Update()
        {
            if (screen == null) return;
            blink += Time.deltaTime;
            if (!Powered) screen.text = blink % 1f < 0.5f ? "--:--" : "";
            else screen.text = Armed ? "GUARDIAN\nARMADO" : "GUARDIAN\nPIN ____";
            screen.color = !Powered ? new Color(0.4f, 0.1f, 0.1f) : (Armed ? new Color(0.3f, 1f, 0.4f) : new Color(1f, 0.35f, 0.25f));
        }
    }
}
