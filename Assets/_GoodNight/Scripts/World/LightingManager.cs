using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>Iluminación global: interruptores, energía, parpadeos, ambiente por acto, luces de emergencia y amanecer.</summary>
    public class LightingManager : MonoBehaviour
    {
        public static LightingManager I;
        public RoomLight[] lights;
        public Light moon;
        public Light[] emergency;
        public Light sun;
        public Renderer[] skyRenderers;
        public Material skyNight, skyDawn;

        readonly Dictionary<string, bool> switches = new Dictionary<string, bool>();
        public Mood mood = Mood.Warm;
        bool emergencyOn; float pulse;

        void Awake() { I = this; }

        void Start()
        {
            if (PowerPanel.I) PowerPanel.I.Changed += ApplyAll;
            ApplyAll();
        }

        public bool LightsPowered => PowerPanel.I == null || PowerPanel.I.On(Circuit.Lights);
        public bool ZoneSwitch(string zone) { bool v; return !switches.TryGetValue(zone, out v) || v; }
        public void SetZone(string zone, bool on) { switches[zone] = on; ApplyAll(); }
        public void ToggleZone(string zone) { SetZone(zone, !ZoneSwitch(zone)); }

        public void ApplyAll()
        {
            if (lights == null) return;
            bool powered = LightsPowered;
            foreach (var l in lights) if (l) l.Apply(powered && ZoneSwitch(l.zone));
        }

        public void ForceOn(string zone, bool v)
        {
            foreach (var l in lights) if (l && l.zone == zone) l.forcedOn = v;
            ApplyAll();
        }

        public void Flicker(string zone, float seconds)
        {
            foreach (var l in lights) if (l && l.zone == zone) l.flicker = seconds;
        }

        public void SetMood(Mood m)
        {
            mood = m;
            switch (m)
            {
                case Mood.Warm: RenderSettings.ambientLight = new Color(0.07f, 0.065f, 0.065f); RenderSettings.fogColor = new Color(0.02f, 0.02f, 0.03f); RenderSettings.fogDensity = 0.07f; break;
                case Mood.Night: RenderSettings.ambientLight = new Color(0.04f, 0.045f, 0.065f); RenderSettings.fogColor = new Color(0.01f, 0.012f, 0.02f); RenderSettings.fogDensity = 0.09f; break;
                case Mood.Dark: RenderSettings.ambientLight = new Color(0.022f, 0.025f, 0.04f); RenderSettings.fogColor = new Color(0.005f, 0.006f, 0.01f); RenderSettings.fogDensity = 0.11f; break;
                case Mood.Emergency: RenderSettings.ambientLight = new Color(0.03f, 0.015f, 0.015f); RenderSettings.fogColor = new Color(0.02f, 0.005f, 0.005f); RenderSettings.fogDensity = 0.11f; break;
                case Mood.Dawn: RenderSettings.ambientLight = new Color(0.32f, 0.27f, 0.25f); RenderSettings.fogColor = new Color(0.55f, 0.45f, 0.42f); RenderSettings.fogDensity = 0.035f; break;
            }
            SetEmergency(m == Mood.Emergency);
            if (sun) sun.enabled = m == Mood.Dawn;
            if (moon) moon.enabled = m != Mood.Dawn;
            if (skyRenderers != null && skyNight && skyDawn)
                foreach (var r in skyRenderers) if (r) r.sharedMaterial = m == Mood.Dawn ? skyDawn : skyNight;
        }

        public void SetEmergency(bool on)
        {
            emergencyOn = on;
            if (emergency != null) foreach (var e in emergency) if (e) e.enabled = on;
        }

        void Update()
        {
            bool anyFlicker = false;
            if (lights != null)
                foreach (var l in lights)
                    if (l && l.flicker > 0f) { l.flicker -= Time.deltaTime; anyFlicker = true; }
            if (anyFlicker) ApplyAll();

            if (emergencyOn && emergency != null)
            {
                pulse += Time.deltaTime * 2.2f;
                float v = 0.35f + 0.65f * (0.5f + 0.5f * Mathf.Sin(pulse));
                foreach (var e in emergency) if (e) e.intensity = 1.6f * v;
            }
        }
    }
}
