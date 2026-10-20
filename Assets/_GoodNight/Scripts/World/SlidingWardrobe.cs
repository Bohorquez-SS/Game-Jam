using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>Armario del pasillo que se desliza y revela la puerta metálica (circuito 06).</summary>
    public class SlidingWardrobe : MonoBehaviour
    {
        public Vector3 slideOffset = new Vector3(-1.1f, 0, 0);
        public float duration = 4f;
        public bool opened;
        Vector3 basePos; bool moving; float t;
        void Awake() { basePos = transform.localPosition; }
        public void Open(bool instant)
        {
            if (opened) return;
            opened = true;
            if (instant) { transform.localPosition = basePos + slideOffset; return; }
            moving = true; t = 0f;
            if (AudioManager.I) AudioManager.I.At(AudioManager.I.mechanism, transform.position, 1f);
        }
        void Update()
        {
            if (!moving) return;
            t += Time.deltaTime / duration;
            transform.localPosition = basePos + slideOffset * Mathf.SmoothStep(0, 1, Mathf.Clamp01(t));
            if (t >= 1f) { moving = false; GameEvents.Raise("wardrobe_opened"); }
        }
    }
}
