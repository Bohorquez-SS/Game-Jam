using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>Ranura de la videograbadora.</summary>
    public class VcrSlot : Clickable
    {
        public System.Action Clicked;
        public override void Click() { if (Clicked != null) Clicked(); }
    }
}
