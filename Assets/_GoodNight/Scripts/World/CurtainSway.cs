using System.Collections;
using UnityEngine;

namespace GNight
{
    public class CurtainSway : MonoBehaviour
    {
        [Range(0, 1)] public float wind; public float maxAngle = 14f; Quaternion baseRot; float seed;
        void Awake() { baseRot = transform.localRotation; seed = Random.value * 50f; }
        void Update() { float n = Mathf.PerlinNoise(seed, Time.time * 0.8f); transform.localRotation = baseRot * Quaternion.Euler(wind * Mathf.Max(0f, n - 0.25f) * maxAngle, 0f, 0f); }
    }
}
