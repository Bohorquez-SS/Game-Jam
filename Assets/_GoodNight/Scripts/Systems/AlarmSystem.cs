using UnityEngine;

namespace GNight
{
    /// <summary>Alarma GUARDIÁN: PIN 2308 (DDMM del cambio de cerradura). Al armarse informa zonas y OCUPANTES.</summary>
    public class AlarmSystem : MonoBehaviour
    {
        public static AlarmSystem I;
        public string code = "2308";
        public bool Armed;
        public int occupants = 2;
        public string motionZone = "PASILLO";
        public TextMesh screen;

        void Awake() { I = this; }

        public bool Powered => PowerPanel.I == null || PowerPanel.I.On(Circuit.Alarm);

        public string StatusLine => Settings.T("ZONAS: 06  OCUPANTES: ", "ZONES: 06  OCCUPANTS: ") + occupants + "\n" + Settings.T("MOV: ", "MOTION: ") + motionZone;

        public bool TryCode(string c)
        {
            if (!Powered) return false;
            if (c == code) { Armed = true; GameEvents.Raise("alarm_armed"); return true; }
            GameEvents.Raise("alarm_wrong");
            return false;
        }
    }
}
