using UnityEngine;

namespace GNight
{
    /// <summary>Utilidades de interpolación.</summary>
    public static class Ease
    {
        public static float InOut(float t) { t = Mathf.Clamp01(t); return t * t * (3f - 2f * t); }
        public static float Out(float t) { t = Mathf.Clamp01(t); return 1f - (1f - t) * (1f - t); }
        public static float In(float t) { t = Mathf.Clamp01(t); return t * t; }
        public static float OutBack(float t) { t = Mathf.Clamp01(t); const float c = 1.6f; float u = t - 1f; return 1f + (c + 1f) * u * u * u + c * u * u; }
    }
}
