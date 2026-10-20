using UnityEngine;

namespace GNight
{
    /// <summary>Sacudidas y golpes de FOV para la cámara del jugador.</summary>
    public static class CameraFX
    {
        static float trauma, fovKick;
        public static void Shake(float amount) { trauma = Mathf.Clamp01(trauma + amount); }
        public static void FovKick(float degrees) { fovKick += degrees; }

        /// <summary>Devuelve el offset de rotación (grados) y avanza el decaimiento.</summary>
        public static Vector3 Tick(float dt, out float fov)
        {
            trauma = Mathf.MoveTowards(trauma, 0f, dt * 1.4f);
            fovKick = Mathf.Lerp(fovKick, 0f, dt * 6f);
            fov = fovKick;
            float s = trauma * trauma, t = Time.time * 28f;
            return new Vector3((Mathf.PerlinNoise(t, 1f) - 0.5f) * 9f * s, (Mathf.PerlinNoise(2f, t) - 0.5f) * 9f * s, (Mathf.PerlinNoise(t, 3f) - 0.5f) * 6f * s);
        }
        public static void Reset() { trauma = 0; fovKick = 0; }
    }
}
