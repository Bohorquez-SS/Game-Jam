using UnityEngine;

namespace GNight
{
    /// <summary>Botón físico que se hunde al pulsarlo.</summary>
    public class PushButton : Clickable
    {
        public string key;
        public float depth = 0.006f;
        public System.Action<PushButton> Pressed;
        public AudioClip clip;
        Vector3 basePos; float t = 1f; bool init;
        void Init() { if (init) return; init = true; basePos = transform.localPosition; }
        void Awake() { Init(); }
        public override void Click()
        {
            Init();
            t = 0f;
            if (AudioManager.I) AudioManager.I.UI(clip != null ? clip : AudioManager.I.keyBeep, 0.6f);
            if (Pressed != null) Pressed(this);
        }
        void Update()
        {
            if (t >= 1f) return;
            t += Time.deltaTime / 0.18f;
            float k = t < 0.4f ? t / 0.4f : 1f - (t - 0.4f) / 0.6f;
            transform.localPosition = basePos + transform.localRotation * Vector3.forward * depth * Mathf.Clamp01(k);
        }
    }
}
