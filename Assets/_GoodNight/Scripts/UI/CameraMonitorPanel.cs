using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>Monitor de vigilancia de la sala: grabación antigua / transmisión en directo.</summary>
    public class CameraMonitorPanel : UIPanel
    {
        public Texture2D noiseTex;
        RawImage feed, noise;
        Text camName, stamp, signal, log, center;
        Image dot;
        UIButton prev, next, live, logBtn, exit;
        int index = 1; // empieza en CAM 02 PASILLO
        float interference, blink;
        bool showLog;
        readonly HashSet<int> unavailable = new HashSet<int>();

        public int Index => index;

        public override void Build(RectTransform layer)
        {
            root = UIKit.Rect("Cameras", layer); UIKit.Stretch(root);
            var bg = UIKit.Img("Bg", root, new Color(0.02f, 0.02f, 0.02f, 0.97f)); UIKit.Stretch(bg.rectTransform);
            feed = UIKit.Raw("Feed", root); UIKit.Center(feed.rectTransform, new Vector2(0, 60), new Vector2(1120, 840));
            noise = UIKit.Raw("Noise", feed.rectTransform); UIKit.Stretch(noise.rectTransform); noise.texture = noiseTex; noise.color = new Color(1, 1, 1, 0.12f);
            camName = UIKit.Txt("Cam", feed.rectTransform, 30, TextAnchor.UpperLeft, Color.white);
            UIKit.Stretch(camName.rectTransform, 24, 0, 0, 18);
            stamp = UIKit.Txt("Stamp", feed.rectTransform, 30, TextAnchor.UpperRight, Color.white);
            UIKit.Stretch(stamp.rectTransform, 0, 0, 60, 18);
            dot = UIKit.Img("Dot", feed.rectTransform, UIKit.Red);
            UIKit.Place(dot.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-24, -24), new Vector2(22, 22));
            signal = UIKit.Txt("Signal", feed.rectTransform, 24, TextAnchor.LowerLeft, UIKit.Green);
            UIKit.Stretch(signal.rectTransform, 24, 16, 0, 0);
            center = UIKit.Txt("Center", feed.rectTransform, 46, TextAnchor.MiddleCenter, Color.white);
            UIKit.Stretch(center.rectTransform);
            log = UIKit.Txt("Log", feed.rectTransform, 24, TextAnchor.MiddleLeft, UIKit.Green);
            UIKit.Stretch(log.rectTransform, 60, 60, 60, 60);
            prev = new UIButton(root, "◄", new Vector2(0.5f, 0), new Vector2(-520, 60), new Vector2(90, 60), 30);
            next = new UIButton(root, "►", new Vector2(0.5f, 0), new Vector2(-420, 60), new Vector2(90, 60), 30);
            live = new UIButton(root, "", new Vector2(0.5f, 0), new Vector2(-110, 60), new Vector2(420, 60), 24);
            logBtn = new UIButton(root, "", new Vector2(0.5f, 0), new Vector2(230, 60), new Vector2(240, 60), 24);
            exit = new UIButton(root, Settings.T("Salir [Esc]", "Exit [Esc]"), new Vector2(0.5f, 0), new Vector2(480, 60), new Vector2(220, 60), 24);
        }

        public override void OnOpen() { showLog = false; GameEvents.Raise("camera_monitor_opened"); }
        public override void OnClose() { GameEvents.Raise("camera_monitor_closed"); }

        public void SetUnavailable(int camIndex, bool v) { if (v) unavailable.Add(camIndex); else unavailable.Remove(camIndex); }
        public void Interfere(float s) { interference = Mathf.Max(interference, s); }
        public void ForceIndex(int i) { index = i; }

        void Update()
        {
            if (!IsOpen) return;
            var sys = SecurityCameraSystem.I;
            if (sys == null || sys.cams.Length == 0) return;
            var cam = sys.cams[index];
            bool down = unavailable.Contains(index);
            if (!down) sys.Request(cam);
            feed.texture = down ? null : cam.rt;
            feed.color = down ? new Color(0.08f, 0.08f, 0.08f) : Color.white;

            bool recording = sys.recordingMode;
            blink += Time.unscaledDeltaTime;
            interference = Mathf.Max(0f, interference - Time.unscaledDeltaTime);
            noise.uvRect = new Rect(Random.value, Random.value, 2f, 2f);
            noise.color = new Color(1, 1, 1, down ? 0.6f : (0.1f + interference * 0.8f));

            camName.text = cam.camName;
            stamp.text = recording ? "REC  23/08  23:12:" + (Mathf.FloorToInt(blink) % 60).ToString("00") : "LIVE  " + GameClock.Text;
            dot.color = recording ? UIKit.Red : UIKit.Green;
            dot.enabled = blink % 1f < 0.5f;
            signal.text = down ? "" : (recording ? Settings.T("SEÑAL: ARCHIVO", "SIGNAL: ARCHIVE") : Settings.T("SEÑAL: OK", "SIGNAL: OK"));
            center.text = down ? cam.camName + Settings.T(": NO DISPONIBLE", ": UNAVAILABLE") : "";
            log.text = showLog ? Settings.T(
                "REGISTRO GUARDIÁN\n\n> Reproduciendo archivo: CAM02_2019-08-23_23-12.gnv\n> Modo: GRABACIÓN (bucle)\n> Transmisión en directo: DESACTIVADA\n> Motivo: energía insuficiente en circuito CÁMARAS",
                "GUARDIAN LOG\n\n> Playing file: CAM02_2019-08-23_23-12.gnv\n> Mode: RECORDING (loop)\n> Live feed: DISABLED\n> Reason: insufficient power on CAMERAS circuit") : "";
            if (showLog) feed.color = new Color(0.15f, 0.15f, 0.15f);
            live.SetText(recording ? Settings.T("TRANSMISIÓN EN DIRECTO", "SWITCH TO LIVE") : Settings.T("EN DIRECTO", "LIVE"));
            live.interactable = recording;
            logBtn.SetText(showLog ? Settings.T("Ocultar registro", "Hide log") : Settings.T("Ver registro", "View log"));
        }

        public override void Tick()
        {
            var sys = SecurityCameraSystem.I; if (sys == null) return;
            if (prev.Tick() || Inp.Left()) { index = (index + sys.cams.Length - 1) % sys.cams.Length; Interfere(0.3f); }
            if (next.Tick() || Inp.Right()) { index = (index + 1) % sys.cams.Length; Interfere(0.3f); }
            if (logBtn.Tick()) { showLog = !showLog; if (showLog) GameEvents.Raise("camera_log_viewed"); }
            if (live.Tick())
            {
                if (!SecurityCameraSystem.Powered)
                {
                    if (AudioManager.I) AudioManager.I.UI(AudioManager.I.error);
                    showLog = false;
                    HUDView.I.Say("", Settings.T("ERROR: ENERGÍA INSUFICIENTE.", "ERROR: INSUFFICIENT POWER."), 3f);
                    GameEvents.Raise("camera_live_failed");
                }
                else
                {
                    sys.recordingMode = false; Interfere(0.8f);
                    GameEvents.Raise("camera_live");
                }
            }
            if (exit.Tick()) UIManager.I.Close(this);
        }
    }
}
