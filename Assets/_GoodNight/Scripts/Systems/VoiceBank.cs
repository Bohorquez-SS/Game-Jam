using UnityEngine;

namespace GNight
{
    /// <summary>
    /// (18/10) Voces habladas: cada línea de subtítulo busca un clip en Resources/Voices/&lt;hash&gt;.wav
    /// (hash FNV-1a del texto en español, igual que el generador Tools/Voices/gen.py). Si no existe, el juego sigue igual
    /// (subtítulo + murmullo provisional). Para reemplazar una voz por una grabación real: mismo nombre de archivo
    /// (ver VOICE_LINES.md).
    /// </summary>
    public static class VoiceBank
    {
        public static float EndTime;
        public static float Remaining => Mathf.Max(0f, EndTime - Time.time);

        public static string Key(string s)
        {
            uint h = 2166136261;
            foreach (char c in s) { h ^= c; h *= 16777619; }
            return h.ToString("x8");
        }

        /// <summary>Reproduce la voz de la línea (si existe). Devuelve su duración (0 si no hay clip).</summary>
        public static float TryPlay(string text)
        {
            if (string.IsNullOrEmpty(text) || AudioManager.I == null) return 0f;
            var clip = Resources.Load<AudioClip>("Voices/" + Key(text));
            if (clip == null) return 0f;
            AudioManager.I.PlayLine(clip);
            EndTime = Time.time + clip.length;
            return clip.length;
        }

        /// <summary>Espera al menos 'min' segundos y, si hay una voz sonando, a que termine (no se cortan frases).</summary>
        public static WaitForSeconds Wait(float min) => new WaitForSeconds(Mathf.Max(min, Remaining + 0.25f));
    }
}
