using UnityEngine;

namespace GNight
{
    /// <summary>Aparato al que se acerca la cámara.</summary>
    public abstract class FocusStation : Interactable
    {
        public string stationId = "station";
        public Transform view;
        public string promptEs = "Usar", promptEn = "Use";
        public bool available = true;
        [HideInInspector] public bool canExit = true;
        public override bool CanInteract => available && !FocusSystem.Active;
        public override string Prompt => Settings.T(promptEs, promptEn);
        public override void Interact() { if (FocusSystem.I) FocusSystem.I.Enter(this); }
        public virtual string Hint => Settings.T("[Clic] usar   ·   [Esc] / [Clic derecho] alejarse", "[Click] use   ·   [Esc] / [Right click] step back");
        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void OnFocusTick() { }
        /// <summary>true mientras la estación muestra su propia interfaz en pantalla (los clics no van a los botones 3D).</summary>
        public virtual bool ConsumesMouse => false;
        /// <summary>Cómo funciona el aparato (cuadro de instrucciones en pantalla mientras se usa).</summary>
        public virtual string Instructions => null;
    }
}
