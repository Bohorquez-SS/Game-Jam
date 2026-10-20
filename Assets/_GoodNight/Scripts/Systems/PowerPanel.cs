using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GNight
{
    /// <summary>Cuadro eléctrico: capacidad 7. Alarma 3, Cámaras 2, Cerraduras 2, Iluminación 3, Circuito 06 = 2.</summary>
    public class PowerPanel : MonoBehaviour
    {
        public static PowerPanel I;
        public int capacity = 7;
        public readonly int[] cost = { 3, 2, 2, 3, 2 };
        public bool[] state = { true, false, false, true, false };
        public bool c06Known;
        public bool Tripped { get; private set; }
        public event System.Action Changed;

        public static string Name(Circuit c)
        {
            switch (c)
            {
                case Circuit.Alarm: return Settings.T("Alarma", "Alarm");
                case Circuit.Cameras: return Settings.T("Cámaras", "Cameras");
                case Circuit.Locks: return Settings.T("Cerraduras electrónicas", "Electronic locks");
                case Circuit.Lights: return Settings.T("Iluminación", "Lighting");
                default: return Settings.T("Circuito 06", "Circuit 06");
            }
        }

        void Awake() { I = this; }

        public bool On(Circuit c) => !Tripped && state[(int)c];
        public bool SecurityComplete => On(Circuit.Alarm) && On(Circuit.Cameras) && On(Circuit.Locks);

        public int Load(bool[] s) { int l = 0; for (int i = 0; i < s.Length; i++) if (s[i]) l += cost[i]; return l; }

        /// <summary>Aplica una configuración. Si supera la capacidad, salta el diferencial.</summary>
        public bool Apply(bool[] s)
        {
            if (Load(s) > capacity) { Trip(); return false; }
            for (int i = 0; i < state.Length; i++) state[i] = s[i];
            Tripped = false;
            if (Changed != null) Changed();
            GameEvents.Raise("power_changed");
            return true;
        }

        public void Trip()
        {
            Tripped = true;
            for (int i = 0; i < state.Length; i++) state[i] = false;
            if (AudioManager.I) AudioManager.I.UI(AudioManager.I.breaker, 1f);
            if (Changed != null) Changed();
            GameEvents.Raise("power_tripped");
        }

        public void SetSilently(bool alarm, bool cams, bool locks, bool lights, bool c06)
        {
            state[0] = alarm; state[1] = cams; state[2] = locks; state[3] = lights; state[4] = c06; Tripped = false;
            if (Changed != null) Changed();
        }

        public void SetTrippedSilently() { Tripped = true; for (int i = 0; i < state.Length; i++) state[i] = false; if (Changed != null) Changed(); }
    }
}
