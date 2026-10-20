using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>Luz de una habitación (techo o lámpara) controlada por interruptor y por el circuito de iluminación.</summary>
    public class RoomLight : MonoBehaviour
    {
        public string zone;
        public Light lightSource;
        public Renderer bulb;
        public Material onMat, offMat;
        [HideInInspector] public bool forcedOn; // anomalía: se enciende sola
        [HideInInspector] public float flicker;
        float baseIntensity = -1f;

        public void Apply(bool on)
        {
            if (lightSource == null) return;
            if (baseIntensity < 0) baseIntensity = lightSource.intensity;
            bool final = on || forcedOn;
            if (flicker > 0f) final = final && Random.value > 0.45f;
            lightSource.enabled = final;
            if (bulb) bulb.sharedMaterial = final ? onMat : offMat;
        }
    }
}
