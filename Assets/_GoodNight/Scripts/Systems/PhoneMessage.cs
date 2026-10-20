using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    public class PhoneMessage
    {
        public string thread;      // "mama", "unknown", "guardian"
        public string time, text;
        public bool outgoing;      // enviado "desde este teléfono"
        public bool isCall;        // registro de llamada
        public bool isNew;
    }
}
