using UnityEngine;

namespace GNight
{
    /// <summary>Persiana metálica de aislamiento (escondida sobre el techo hasta que baja).</summary>
    public class Shutter : MonoBehaviour
    {
        public float drop = 2.4f, speed = 1.6f;
        public bool down;
        Vector3 upPos; bool init;
        void Init() { if (init) return; init = true; upPos = transform.localPosition; }
        void Awake() { Init(); }
        public bool Blocks => down;
        public void SetDown(bool d, bool instant = false)
        {
            Init();
            if (d != down && !instant && AudioManager.I) AudioManager.I.At(AudioManager.I.shutter, transform.position - Vector3.up * drop, 1f);
            down = d;
            if (instant) transform.localPosition = upPos - (down ? Vector3.up * drop : Vector3.zero);
        }
        void Update()
        {
            Vector3 target = upPos - (down ? Vector3.up * drop : Vector3.zero);
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, Time.deltaTime * speed);
        }
    }
}
