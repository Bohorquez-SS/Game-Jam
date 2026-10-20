using UnityEngine;

namespace GNight
{
    /// <summary>Luz de una habitación. Las lámparas (alwaysPowered) van a otro circuito y no se apagan con el cuadro.</summary>
    public class RoomLight : MonoBehaviour
    {
        public string zone;
        public Light lightSource;
        public Renderer bulb;
        public Material onMat, offMat;
        public bool alwaysPowered;
        [HideInInspector] public bool forcedOn, forcedOff;
        [HideInInspector] public bool userOff;   // apagada a mano por el jugador (Toggleable)
        [HideInInspector] public float flicker;
        float baseIntensity = -1f;
        public float BaseIntensity { get { if (baseIntensity < 0 && lightSource) baseIntensity = lightSource.intensity; return baseIntensity; } }

        public void Apply(bool on, float moodScale)
        {
            if (lightSource == null) return;
            bool final = (on || forcedOn) && !forcedOff && !userOff;
            if (flicker > 0f) final = final && Random.value > 0.45f;
            lightSource.enabled = final;
            lightSource.intensity = BaseIntensity * moodScale;
            if (bulb) bulb.sharedMaterial = final ? onMat : offMat;
        }
    }
}
