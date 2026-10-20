using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Iluminación doméstica por acto: cálida y visible (cap. 1), noche (2), fallos y contraste (3), fría (4),
    /// emergencia roja (5) y amanecer. Interruptores por habitación, circuito de iluminación, parpadeos.
    /// La oscuridad se usa en momentos concretos, nunca en toda la partida.
    /// </summary>
    public class LightingManager : MonoBehaviour
    {
        public static LightingManager I;
        public RoomLight[] lights;
        public Light moon, sun, streetLamp;
        public Light[] emergency;
        public Renderer[] skyRenderers;
        public Material skyNight, skyDawn;

        readonly Dictionary<string, bool> switches = new Dictionary<string, bool>();
        public Mood mood = Mood.Warm;
        float moodScale = 1f;
        bool emergencyOn; float pulse;

        void Awake() { I = this; }
        void Start() { if (PowerPanel.I) PowerPanel.I.Changed += ApplyAll; ApplyAll(); }

        public bool LightsPowered => PowerPanel.I == null || PowerPanel.I.On(Circuit.Lights);
        public bool ZoneSwitch(string zone) { bool v; return !switches.TryGetValue(zone, out v) || v; }
        public void SetZone(string zone, bool on) { switches[zone] = on; ApplyAll(); }
        public void ToggleZone(string zone) { SetZone(zone, !ZoneSwitch(zone)); }

        public void ApplyAll()
        {
            if (lights == null) return;
            bool powered = LightsPowered;
            foreach (var l in lights)
                if (l) l.Apply((l.alwaysPowered || powered) && ZoneSwitch(l.zone), l.alwaysPowered ? Mathf.Max(0.85f, moodScale) : moodScale);
        }

        public void ForceOn(string zone, bool v) { foreach (var l in lights) if (l && l.zone == zone) l.forcedOn = v; ApplyAll(); }
        public void ForceOff(string zone, bool v) { foreach (var l in lights) if (l && l.zone == zone) l.forcedOff = v; ApplyAll(); }
        public void Flicker(string zone, float seconds) { foreach (var l in lights) if (l && (zone == null || l.zone == zone)) l.flicker = seconds; }

        public void SetMood(Mood m)
        {
            mood = m;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear;
            switch (m)
            {
                case Mood.Warm:
                    Amb(new Color(0.16f, 0.15f, 0.17f), new Color(0.13f, 0.11f, 0.1f), new Color(0.07f, 0.06f, 0.05f)); moodScale = 1f;
                    Fog(new Color(0.03f, 0.035f, 0.05f), 14f, 45f); break;
                case Mood.Night:
                    Amb(new Color(0.12f, 0.13f, 0.17f), new Color(0.1f, 0.095f, 0.1f), new Color(0.05f, 0.05f, 0.05f)); moodScale = 0.9f;
                    Fog(new Color(0.02f, 0.025f, 0.04f), 12f, 40f); break;
                case Mood.Dark:
                    Amb(new Color(0.08f, 0.095f, 0.14f), new Color(0.06f, 0.065f, 0.08f), new Color(0.03f, 0.03f, 0.04f)); moodScale = 0.8f;
                    Fog(new Color(0.015f, 0.02f, 0.03f), 10f, 34f); break;
                case Mood.Emergency:
                    Amb(new Color(0.12f, 0.06f, 0.07f), new Color(0.09f, 0.045f, 0.05f), new Color(0.04f, 0.02f, 0.02f)); moodScale = 0.65f;
                    Fog(new Color(0.03f, 0.01f, 0.012f), 10f, 34f); break;
                case Mood.Dawn:
                    Amb(new Color(0.45f, 0.38f, 0.4f), new Color(0.35f, 0.3f, 0.28f), new Color(0.15f, 0.12f, 0.1f)); moodScale = 1f;
                    Fog(new Color(0.6f, 0.5f, 0.48f), 20f, 70f); break;
            }
            SetEmergency(m == Mood.Emergency);
            if (sun) sun.enabled = m == Mood.Dawn;
            if (moon) moon.enabled = m != Mood.Dawn;
            if (skyRenderers != null && skyNight && skyDawn)
                foreach (var r in skyRenderers) if (r) r.sharedMaterial = m == Mood.Dawn ? skyDawn : skyNight;
            if (PostFX.I) PostFX.I.SetMood(m);
            ApplyAll();
        }

        static void Amb(Color sky, Color eq, Color ground) { RenderSettings.ambientSkyColor = sky; RenderSettings.ambientEquatorColor = eq; RenderSettings.ambientGroundColor = ground; }
        static void Fog(Color c, float start, float end) { RenderSettings.fogColor = c; RenderSettings.fogStartDistance = start; RenderSettings.fogEndDistance = end; }

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
                pulse += Time.deltaTime * 2.0f;
                float v = 0.45f + 0.55f * (0.5f + 0.5f * Mathf.Sin(pulse));
                foreach (var e in emergency) if (e) e.intensity = 2.2f * v;
            }
        }
    }
}
