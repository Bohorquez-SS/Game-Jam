using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Consola de aislamiento (habitación 06). Muestra sensores de movimiento (con "ecos") y el estado de energía.
    /// Los monitores CRT de la sala muestran las cámaras: las congeladas (REC, hora vieja) no son de fiar.
    /// Botones: AISLAR COCINA · AISLAR DORMITORIO · (si descubriste la verdad) AISLAR 06.
    /// </summary>
    public class IsolationStation : FocusStation
    {
        public static readonly HashSet<string> Motion = new HashSet<string>();
        public PushButton isoKitchen, isoBedroom, isoSelf;
        public TextMesh screen;
        public bool selfOptionVisible;

        void Start()
        {
            if (isoKitchen) isoKitchen.Pressed = b => Press("cocina");
            if (isoBedroom) isoBedroom.Pressed = b => Press("dormitorio");
            if (isoSelf) isoSelf.Pressed = b => Press("secreta");
        }

        public override string Hint => Settings.T("Compara sensores y cámaras · [Clic] aislar · [Esc] alejarse", "Compare sensors and cameras · [Click] isolate · [Esc] step back");

        public override string Instructions => Settings.T(
            "<b>CONSOLA DE AISLAMIENTO</b>\nSENSORES: punto <b>fijo</b> = algo real · parpadeo con ritmo fijo = <b>ECO</b>.\nCÁMARAS (monitores): <b>DIRECTO</b> = imagen real · <b>REC</b> con hora vieja = <b>CONGELADA</b>.\nAísla solo la zona donde coincidan un sensor fijo y una cámara en directo.",
            "<b>ISOLATION CONSOLE</b>\nSENSORS: <b>steady</b> dot = something real · blinking at a steady rhythm = <b>ECHO</b>.\nCAMERAS (monitors): <b>LIVE</b> = real image · <b>REC</b> with an old time = <b>FROZEN</b>.\nOnly isolate the zone where a steady sensor and a live camera agree.");

        void Press(string zone)
        {
            if (PowerPanel.I != null && !PowerPanel.I.SecurityComplete)
            {
                if (AudioManager.I) AudioManager.I.UI(AudioManager.I.error, 0.8f);
                return;
            }
            if (FocusSystem.I) FocusSystem.I.Exit();
            GameEvents.Raise("isolate:" + zone);
        }

        void Update()
        {
            if (isoSelf) isoSelf.gameObject.SetActive(selfOptionVisible);
            if (screen == null) return;
            bool ok = PowerPanel.I == null || PowerPanel.I.SecurityComplete;
            var sb = new System.Text.StringBuilder();
            sb.Append(ok ? Settings.T("ENERGÍA: OK", "POWER: OK") : Settings.T("ENERGÍA: INSUFICIENTE\nA+C+L EN EL CUADRO", "POWER: LOW\nA+C+L AT PANEL")).Append('\n');
            sb.Append(Settings.T("SENSORES:", "SENSORS:")).Append('\n');
            foreach (var z in new[] { "sala", "cocina", "pasillo", "dormitorio" })
            {
                var rz = RoomZone.Get(z);
                string n = rz != null ? rz.DisplayName.ToUpperInvariant() : z;
                sb.Append(Motion.Contains(z) ? "● " : "○ ").Append(n).Append('\n');
            }
            screen.text = sb.ToString();
            if (isoKitchen) isoKitchen.interactable = ok;
            if (isoBedroom) isoBedroom.interactable = ok;
            if (isoSelf) isoSelf.interactable = ok;
        }
    }
}
