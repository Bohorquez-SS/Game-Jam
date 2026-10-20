using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>Audio 2D (interfaz, voces provisionales, música) y control de ambientes.</summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager I;

        [Header("Interfaz")]
        public AudioClip vibrate, beep, click, error, keyBeep, pageFlip;
        [Header("Mundo")]
        public AudioClip sting, breaker, mechanism, lockClunk, drawer, knock, windowKnock, metalBang, shutter, spark, whisper, heartbeat, breath, staticBurst, steps1, steps2, steps3;
        [Header("Voces provisionales (sustituir por grabaciones reales)")]
        public AudioClip voiceMom, voiceDad, voiceGuardian, voiceYou, voiceEntity;
        [Header("Fuentes")]
        public AudioSource music;
        public AudioSource[] ambience;

        AudioSource ui, voice;
        float[] ambienceBase;
        float ambienceScale = 1f, musicTarget;

        void Awake()
        {
            I = this;
            ui = gameObject.AddComponent<AudioSource>(); ui.playOnAwake = false; ui.spatialBlend = 0f;
            voice = gameObject.AddComponent<AudioSource>(); voice.playOnAwake = false; voice.spatialBlend = 0f;
            if (ambience != null)
            {
                ambienceBase = new float[ambience.Length];
                for (int i = 0; i < ambience.Length; i++) if (ambience[i]) ambienceBase[i] = ambience[i].volume;
            }
            if (music) { musicTarget = music.volume; }
        }

        void Update()
        {
            if (ambience != null)
                for (int i = 0; i < ambience.Length; i++)
                    if (ambience[i]) ambience[i].volume = Mathf.MoveTowards(ambience[i].volume, ambienceBase[i] * ambienceScale, Time.unscaledDeltaTime * 0.3f);
            if (music) music.volume = Mathf.MoveTowards(music.volume, musicTarget, Time.unscaledDeltaTime * 0.15f);
        }

        public void UI(AudioClip c, float v = 1f) { if (c) ui.PlayOneShot(c, v * Settings.Sfx); }

        public void At(AudioClip c, Vector3 p, float v = 1f)
        {
            if (c == null) return;
            var go = new GameObject("SFX_" + c.name);
            go.transform.position = p;
            var a = go.AddComponent<AudioSource>();
            a.clip = c; a.spatialBlend = 1f; a.volume = v * Settings.Sfx;
            a.rolloffMode = AudioRolloffMode.Linear; a.minDistance = 1.5f; a.maxDistance = 18f; a.dopplerLevel = 0f;
            a.pitch = Random.Range(0.94f, 1.06f);
            a.Play();
            Destroy(go, c.length / Mathf.Max(0.5f, a.pitch) + 0.2f);
        }

        public void PlaySteps(Vector3 p, float v = 0.8f)
        {
            var clips = new[] { steps1, steps2, steps3 };
            At(clips[Random.Range(0, 3)], p, v);
        }

        /// <summary>Voz provisional (murmullo sintetizado) — los subtítulos llevan el texto.</summary>
        public void PlayVoice(Voice v, Vector3? at = null, float volume = 0.7f)
        {
            AudioClip c = null;
            switch (v)
            {
                case Voice.Mom: c = voiceMom; break;
                case Voice.Dad: c = voiceDad; break;
                case Voice.Guardian: c = voiceGuardian; break;
                case Voice.You: c = voiceYou; break;
                case Voice.Entity: c = voiceEntity; break;
            }
            if (c == null) return;
            if (at.HasValue) At(c, at.Value, volume);
            else { voice.Stop(); voice.clip = c; voice.volume = volume * Settings.Sfx; voice.pitch = Random.Range(0.95f, 1.05f); voice.Play(); }
        }

        public void SetAmbience(float scale) { ambienceScale = scale; }
        public void SetMusic(float vol) { musicTarget = vol; }
    }
}
