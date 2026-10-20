using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>Reloj digital del mundo (muestra la hora de la ficción).</summary>
    [RequireComponent(typeof(TextMesh))]
    public class ClockDisplay : MonoBehaviour
    {
        TextMesh tm;
        void Awake() { tm = GetComponent<TextMesh>(); }
        void Update()
        {
            bool blink = Mathf.FloorToInt(Time.time) % 2 == 0;
            tm.text = GameClock.Hour.ToString("00") + (blink ? ":" : " ") + GameClock.Minute.ToString("00");
        }
    }
}
