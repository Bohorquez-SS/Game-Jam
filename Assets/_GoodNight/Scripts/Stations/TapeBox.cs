using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>Caja de cinta clicable (para ordenar).</summary>
    public class TapeBox : Clickable
    {
        public System.Action Clicked;
        public override void Click() { if (Clicked != null) Clicked(); }
    }
}
