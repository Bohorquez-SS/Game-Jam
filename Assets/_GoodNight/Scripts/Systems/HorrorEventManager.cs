using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Eventos de terror de una sola vez, activados al entrar en una zona bajo una condición.
    /// Evita repetir sustos y permite consultar qué ya ocurrió.
    /// </summary>
    public class HorrorEventManager : MonoBehaviour
    {
        public static HorrorEventManager I;
        class ZoneEvent { public string id, zone; public System.Func<bool> cond; public System.Action act; }
        readonly HashSet<string> fired = new HashSet<string>();
        readonly List<ZoneEvent> events = new List<ZoneEvent>();
        public string PlayerZone { get; private set; }

        void Awake() { I = this; }

        public bool Fired(string id) => fired.Contains(id);
        public void MarkFired(string id) { fired.Add(id); }

        /// <summary>Ejecuta la acción una única vez.</summary>
        public bool Once(string id, System.Action act)
        {
            if (fired.Contains(id)) return false;
            fired.Add(id); if (act != null) act();
            return true;
        }

        public void OnEnterZone(string id, string zone, System.Func<bool> cond, System.Action act)
        {
            events.Add(new ZoneEvent { id = id, zone = zone, cond = cond, act = act });
        }

        void Update()
        {
            var p = PlayerController.I; if (p == null) return;
            string z = RoomZone.At(p.transform.position);
            if (z == PlayerZone) return;
            PlayerZone = z;
            GameEvents.Raise("zone:" + z);
            for (int i = 0; i < events.Count; i++)
            {
                var e = events[i];
                if (e.zone != z || fired.Contains(e.id)) continue;
                if (e.cond != null && !e.cond()) continue;
                fired.Add(e.id);
                e.act();
            }
        }
    }
}
