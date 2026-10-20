using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>Relámpagos con trueno.</summary>
    public class Lightning : MonoBehaviour
    {
        public Light flashLight; public AudioSource thunder; public Vector2 interval = new Vector2(16f, 34f); public float flashIntensity = 6f;
        public bool active = true; float baseI;
        IEnumerator Start()
        {
            if (flashLight == null) yield break;
            baseI = flashLight.intensity;
            yield return new WaitForSeconds(Random.Range(10f, 16f));
            while (true)
            {
                if (active && flashLight.enabled) yield return Flash();
                yield return new WaitForSeconds(Random.Range(interval.x, interval.y));
            }
        }
        public IEnumerator Flash()
        {
            float[] p = { 1f, 0.15f, 0.8f, 0.3f, 0.6f };
            int n = Random.Range(2, p.Length + 1);
            for (int i = 0; i < n; i++) { flashLight.intensity = Mathf.Lerp(baseI, flashIntensity, p[i]); yield return new WaitForSeconds(Random.Range(0.03f, 0.09f)); }
            flashLight.intensity = baseI;
            if (thunder) { yield return new WaitForSeconds(Random.Range(0.6f, 2f)); thunder.pitch = Random.Range(0.85f, 1.1f); thunder.volume = 0.8f * Settings.Sfx; thunder.Play(); }
        }
    }
}
