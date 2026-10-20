using UnityEngine;

namespace GNight
{
    /// <summary>Linterna (F). Encendida aumenta la distancia a la que la entidad te ve.</summary>
    public class FlashlightController : MonoBehaviour
    {
        public static FlashlightController I;
        public Light beam;
        public AudioClip click;
        public bool HasFlashlight { get; private set; }
        public bool IsOn => HasFlashlight && beam != null && beam.enabled;
        float flickerT;

        void Awake() { I = this; if (beam) beam.enabled = false; }

        public void Give(bool turnOn)
        {
            HasFlashlight = true;
            if (beam) beam.enabled = turnOn;
        }

        void Update()
        {
            if (!HasFlashlight || beam == null) return;
            if (!PlayerController.MoveBlocked && Inp.Flashlight())
            {
                beam.enabled = !beam.enabled;
                if (AudioManager.I) AudioManager.I.UI(click, 0.6f);
            }
            if (flickerT > 0f)
            {
                flickerT -= Time.deltaTime;
                beam.intensity = Random.value > 0.5f ? 2.2f : 0.2f;
                if (flickerT <= 0f) beam.intensity = 2.2f;
            }
        }

        /// <summary>Parpadeo breve (presencia cerca).</summary>
        public void Flicker(float seconds) { flickerT = seconds; }
    }
}
