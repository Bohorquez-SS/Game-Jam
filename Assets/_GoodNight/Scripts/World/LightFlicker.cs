using System.Collections;
using UnityEngine;

namespace GNight
{
    [RequireComponent(typeof(Light))]
    public class LightFlicker : MonoBehaviour
    {
        public float amount = 0.3f, speed = 8f; Light l; float baseI, seed;
        void Awake() { l = GetComponent<Light>(); baseI = l.intensity; seed = Random.value * 100f; }
        void Update() { float n = Mathf.PerlinNoise(seed, Time.time * speed); l.intensity = baseI * (1f + amount * (n * 2f - 1f)); }
    }
}
