using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Audio: interfaz 2D, efectos 3D en el mundo, voces provisionales con subtítulos, ambientes por capas
    /// (lluvia, viento, casa) y una capa de tensión que sube y baja. Silencio intencional = ambience a 0.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager I;

        [Header("Interfaz")]
        public AudioClip vibrate, beep, click, error, keyBeep, pageFlip, pocket, phoneTap, phoneRing, callEnd, messagePop, check, pickup, leverClick;
        [Header("Mundo")]
        public AudioClip sting, breaker, mechanism, lockClunk, drawer, knock, windowKnock, metalBang, shutter, spark, whisper, heartbeat, breath, breathClose,
                         staticBurst, steps1, steps2, steps3, heavyStep1, heavyStep2, crack, entityScream, hitLow, tensionRise, handleRattle, woodCreak,
                         plates, birds, carArrive, carDoor, vhsInsert, tapeHiss, tvStatic, doorCreak, doorClose, thunder;
        [Header("Sustos y escenas (18/10)")]
        public AudioClip water, scareHit, scareHit2, doorSlow, momOutside, scareMirror, scareCatch, creakNear, knockWall, whisperFar;
        [Header("Voces provisionales (reemplazables por grabaciones reales)")]
        public AudioClip voiceMom, voiceDad, voiceGuardian, voiceYou, voiceEntity;
        [Header("Fuentes")]
        public AudioSource music, tension;
        public AudioSource[] ambience;

        AudioSource ui, voice;
        float[] ambienceBase;
        float ambienceScale = 1f, musicTarget, tensionTarget;

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
            if (music) musicTarget = music.volume;
            if (tension) { tension.volume = 0f; tensionTarget = 0f; }
            // (18/10) lluvia ~18 % más baja: sigue presente pero deja oír diálogos, pasos y golpes
            if (ambience != null && ambience.Length > 0 && ambience[0] != null && ambienceBase != null) ambienceBase[0] *= RainScale;
            if (GetComponent<AmbientDread>() == null) gameObject.AddComponent<AmbientDread>();
        }

        /// <summary>(18/10, pedido del equipo) sin loop constante de lluvia: la lluvia VISUAL sigue; el audio 2D queda en 0.</summary>
        public const float RainScale = 0f;
        /// <summary>Lluvia suave solo a través de una ventana abierta.</summary>
        public const float RainOpenScale = 0.4f;
        float proxTension, chaseVol;
        AudioSource chase;

        void Start()
        {
            // capa de persecución (percusión + pulso grave, generada): entra rápido al perseguir, se apaga despacio al perderte
            var clip = Resources.Load<AudioClip>("chase_loop");
            if (clip != null)
            {
                chase = gameObject.AddComponent<AudioSource>();
                chase.clip = clip; chase.loop = true; chase.spatialBlend = 0f; chase.volume = 0f; chase.playOnAwake = false; chase.Play();
            }
        }

        void LateUpdate()
        {
            if (chase == null) return;
            var e = EnemyAI.I;
            bool hunting = e != null && e.gameObject.activeInHierarchy && (e.State == AIState.Chase || e.State == AIState.Notice);
            float target = hunting ? 0.5f : (e != null && e.gameObject.activeInHierarchy && e.State == AIState.Search ? 0.18f : 0f);
            chaseVol = Mathf.MoveTowards(chaseVol, target, Time.unscaledDeltaTime * (target > chaseVol ? 1.2f : 0.12f));
            chase.volume = chaseVol * Settings.Sfx;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (ambience != null)
                for (int i = 0; i < ambience.Length; i++)
                    if (ambience[i]) ambience[i].volume = Mathf.MoveTowards(ambience[i].volume, ambienceBase[i] * ambienceScale * Settings.Sfx, dt * 0.25f);
            if (music) music.volume = Mathf.MoveTowards(music.volume, musicTarget * Settings.Sfx, dt * 0.12f);
            // presencia de la entidad: el latido sube poco a poco cuanto más cerca está (sin pisar la tensión de la historia)
            var e = EnemyAI.I; var pc = PlayerController.I;
            float prox = 0f;
            if (e != null && pc != null && e.gameObject.activeInHierarchy && e.State != AIState.Inactive && e.State != AIState.Scripted)
                prox = Mathf.Clamp01(1f - (Vector3.Distance(e.transform.position, pc.transform.position) - 1.5f) / 7f) * 0.5f;
            proxTension = Mathf.MoveTowards(proxTension, prox, dt * 0.25f);
            float tt = Mathf.Max(tensionTarget, proxTension) * (chaseVol > 0.3f ? 0.5f : 1f);   // no apilar dos pistas de tensión fuertes
            if (tension) tension.volume = Mathf.MoveTowards(tension.volume, tt * Settings.Sfx, dt * 0.35f);
        }

        public void UI(AudioClip c, float v = 1f) { if (c) ui.PlayOneShot(c, v * Settings.Sfx); }

        public AudioSource At(AudioClip c, Vector3 p, float v = 1f, float maxDist = 18f, float pitch = -1f)
        {
            if (c == null) return null;
            var go = new GameObject("SFX_" + c.name);
            go.transform.position = p;
            var a = go.AddComponent<AudioSource>();
            a.clip = c; a.spatialBlend = 1f; a.volume = v * Settings.Sfx;
            a.rolloffMode = AudioRolloffMode.Linear; a.minDistance = 1.2f; a.maxDistance = maxDist; a.dopplerLevel = 0f;
            a.pitch = pitch > 0f ? pitch : Random.Range(0.95f, 1.05f);
            a.Play();
            Destroy(go, c.length / Mathf.Max(0.5f, a.pitch) + 0.2f);
            return a;
        }

        public void PlaySteps(Vector3 p, float v = 0.8f, bool heavy = false)
        {
            if (heavy && heavyStep1) { At(Random.value < 0.5f ? heavyStep1 : heavyStep2, p, v); return; }
            var clips = new[] { steps1, steps2, steps3 };
            At(clips[Random.Range(0, 3)], p, v);
        }

        /// <summary>Voz provisional (murmullo sintetizado); el texto va en los subtítulos.</summary>
        // ---- voces habladas (VoiceBank): reemplazan al murmullo provisional de la misma línea
        Vector3? lastVoicePos; float lastVoiceTime = -10f; AudioSource lastVoiceSrc, lineSrc3D;
        public void PlayLine(AudioClip clip)
        {
            if (clip == null) return;
            bool recent = Time.time - lastVoiceTime < 0.3f;
            if (recent && lastVoiceSrc != null) lastVoiceSrc.Stop();           // fuera el murmullo
            voice.Stop();
            if (lineSrc3D != null) lineSrc3D.Stop();                           // nunca dos voces a la vez
            if (recent && lastVoicePos.HasValue)
            {
                lineSrc3D = At(clip, lastVoicePos.Value, 1f, 16f, 1f);           // personaje en la casa: audio espacial
                if (lineSrc3D) { lineSrc3D.minDistance = 2f; lineSrc3D.priority = 0; }
            }
            else { voice.clip = clip; voice.volume = 0.95f * Settings.Sfx; voice.pitch = 1f; voice.Play(); }
        }

        public void PlayVoice(Voice v, Vector3? at = null, float volume = 0.7f)
        {
            lastVoiceTime = Time.time; lastVoicePos = at; lastVoiceSrc = null;
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
            if (at.HasValue) lastVoiceSrc = At(c, at.Value, volume, 14f);
            else { voice.Stop(); voice.clip = c; voice.volume = volume * Settings.Sfx; voice.pitch = Random.Range(0.97f, 1.03f); voice.Play(); lastVoiceSrc = voice; }
        }

        /// <summary>Misma frase, misma entonación (como una grabación): pitch fijo.</summary>
        public void PlayVoiceExact(Voice v, Vector3 at, float volume = 0.8f)
        {
            AudioClip c = v == Voice.Mom ? voiceMom : (v == Voice.You ? voiceYou : voiceEntity);
            lastVoiceTime = Time.time; lastVoicePos = at;
            lastVoiceSrc = At(c, at, volume, 14f, 1f);
        }

        public void SetAmbience(float scale) { ambienceScale = scale; }
        public void SetMusic(float vol) { musicTarget = vol; }
        /// <summary>0..1 capa de tensión (drone grave).</summary>
        public void SetTension(float v) { tensionTarget = Mathf.Clamp01(v) * 0.55f; }

        public IEnumerator Silence(float seconds)
        {
            SetAmbience(0.15f);
            yield return new WaitForSeconds(seconds);
            SetAmbience(1f);
        }
    }
}
