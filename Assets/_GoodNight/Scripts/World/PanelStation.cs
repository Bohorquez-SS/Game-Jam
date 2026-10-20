using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>Estación que abre un panel de UI (cámaras, alarma, cuadro eléctrico, aislamiento).</summary>
    public class PanelStation : Interactable
    {
        public enum Kind { Cameras, Alarm, Power, Isolation }
        public Kind kind;
        public string promptEs = "Usar", promptEn = "Use";
        public bool available = true;
        public override bool CanInteract => available;
        public override string Prompt => Settings.T(promptEs, promptEn);
        public override void Interact()
        {
            var ui = UIManager.I;
            switch (kind)
            {
                case Kind.Cameras: ui.Open(ui.Get<CameraMonitorPanel>()); break;
                case Kind.Alarm: ui.Open(ui.Get<KeypadPanel>()); break;
                case Kind.Power: ui.Open(ui.Get<PowerPanelUI>()); break;
                case Kind.Isolation: ui.Open(ui.Get<IsolationPanel>()); break;
            }
            GameEvents.Raise("station_opened:" + kind);
        }
    }
}
