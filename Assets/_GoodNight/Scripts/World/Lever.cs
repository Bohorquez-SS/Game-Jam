using UnityEngine;

namespace GNight
{
    /// <summary>Palanca / interruptor del cuadro eléctrico.</summary>
    public class Lever : Clickable
    {
        public bool on;
        public Transform handle;
        public float angle = 32f;
        public Renderer led;
        public Material ledOn, ledOff, ledFault;
        public System.Action<Lever> Toggled;
        [HideInInspector] public bool fault;
        float cur;
        public override void Click()
        {
            on = !on;
            if (AudioManager.I) AudioManager.I.UI(AudioManager.I.leverClick, 0.8f);
            if (Toggled != null) Toggled(this);
        }
        public void Set(bool v, bool f) { on = v; fault = f; }
        void Update()
        {
            cur = Mathf.MoveTowards(cur, on ? 1f : 0f, Time.deltaTime * 10f);
            if (handle) handle.localRotation = Quaternion.Euler(Mathf.Lerp(angle, -angle, cur), 0, 0);
            if (led) led.sharedMaterial = fault ? ledFault : (on ? ledOn : ledOff);
        }
    }
}
