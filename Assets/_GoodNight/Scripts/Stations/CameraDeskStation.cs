using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Escritorio de vigilancia: monitor CRT + selector físico de 5 botones (CAM 01–04 y ARCHIVO/DIRECTO).
    /// Modo archivo = grabación vieja del 23/08 (con registro). Directo requiere energía en el circuito de cámaras.
    /// </summary>
    public class CameraDeskStation : FocusStation
    {
        public MonitorScreen monitor;
        public PushButton[] camButtons = new PushButton[4];
        public PushButton modeButton;
        public int index = 1;
        [HideInInspector] public bool unavailable;
        float interf;

        void Start()
        {
            for (int i = 0; i < camButtons.Length; i++) if (camButtons[i]) { int k = i; camButtons[i].Pressed = b => Select(k); }
            if (modeButton) modeButton.Pressed = b => ToggleMode();
            Select(index);
        }

        public override string Hint => Settings.T("[Clic] botones: CAM 1–4 · ARCHIVO/DIRECTO · [1-4] cámara · [Esc] alejarse", "[Click] buttons: CAM 1–4 · ARCHIVE/LIVE · [1-4] camera · [Esc] step back");

        public void Select(int i)
        {
            var sys = SecurityCameraSystem.I; if (sys == null || sys.cams.Length == 0) return;
            index = Mathf.Clamp(i, 0, sys.cams.Length - 1);
            monitor.source = sys.cams[index];
            interf = 0.25f;
            if (AudioManager.I) AudioManager.I.UI(AudioManager.I.staticBurst, 0.18f);
            GameEvents.Raise("cam_select:" + sys.cams[index].zoneId);
        }

        void ToggleMode()
        {
            var sys = SecurityCameraSystem.I; if (sys == null) return;
            if (sys.recordingMode)
            {
                if (!SecurityCameraSystem.Powered)
                {
                    if (AudioManager.I) AudioManager.I.UI(AudioManager.I.error);
                    monitor.message = Settings.T("<color=#ff5544>ERROR: ENERGÍA INSUFICIENTE</color>\n<size=18>CIRCUITO CÁMARAS · F2</size>", "<color=#ff5544>ERROR: INSUFFICIENT POWER</color>\n<size=18>CAMERA CIRCUIT · F2</size>");
                    interf = 1.2f;
                    GameEvents.Raise("camera_live_failed");
                    return;
                }
                sys.recordingMode = false; interf = 0.8f;
                GameEvents.Raise("camera_live");
            }
            else { sys.recordingMode = true; GameEvents.Raise("camera_archive"); }
        }

        public override void OnEnter() { GameEvents.Raise("camera_monitor_opened"); }
        public override void OnExit() { GameEvents.Raise("camera_monitor_closed"); }

        public override void OnFocusTick()
        {
            int d = Inp.Digit();
            if (d >= 1 && d <= 4) { if (camButtons[d - 1]) camButtons[d - 1].Click(); }
        }

        void Update()
        {
            if (monitor == null || SecurityCameraSystem.I == null) return;
            var sys = SecurityCameraSystem.I;
            interf -= Time.deltaTime;
            bool rec = sys.recordingMode;
            monitor.recLabel = rec;
            monitor.stampOverride = rec ? "23/08  23:12" : null;
            monitor.showStatic = unavailable || interf > 0f;
            if (unavailable) monitor.message = (monitor.source != null ? monitor.source.camName : "CAM") + Settings.T(": NO DISPONIBLE", ": UNAVAILABLE");
            else if (interf <= -2.5f && monitor.message != null && monitor.message.Contains("ERROR")) monitor.message = null;
            else if (!monitor.showStatic && rec && FocusSystem.Station == this)
                monitor.message = Settings.T("<size=16>ARCHIVO · CAM02_2019-08-23_23-12.gnv · BUCLE</size>", "<size=16>ARCHIVE · CAM02_2019-08-23_23-12.gnv · LOOP</size>");
            else if (!monitor.showStatic && (monitor.message == null || !monitor.message.Contains("ERROR"))) monitor.message = null;
        }
    }
}
