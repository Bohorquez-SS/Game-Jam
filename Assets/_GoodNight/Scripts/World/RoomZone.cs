using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>Zona/habitación de la casa (para alarma, IA, eventos y aislamiento).</summary>
    public class RoomZone : MonoBehaviour
    {
        public string id;
        public string nameEs, nameEn;
        public Vector3 size = Vector3.one;
        public string DisplayName => Settings.T(nameEs, nameEn);
        public bool Contains(Vector3 p)
        {
            Vector3 d = p - transform.position;
            // (18/10) la zona está centrada a 1,5 m de altura y el jugador/la IA tienen el pivote en los PIES (y≈0):
            // con "d.y > -1" nunca coincidía nadie → no se disparaban los eventos de zona (p. ej. entrar a la 06).
            return Mathf.Abs(d.x) <= size.x * 0.5f && Mathf.Abs(d.z) <= size.z * 0.5f && d.y > -1.9f && d.y < 2.2f;
        }

        public static RoomZone[] All;
        public static string At(Vector3 p)
        {
            if (All == null) return null;
            foreach (var z in All) if (z != null && z.Contains(p)) return z.id;
            return null;
        }
        public static RoomZone Get(string id)
        {
            if (All == null) return null;
            foreach (var z in All) if (z != null && z.id == id) return z;
            return null;
        }
    }
}
