using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>Panel de aislamiento de la habitación secreta (puzle final).</summary>
    public class IsolationPanel : UIPanel
    {
        public static readonly HashSet<string> Motion = new HashSet<string>();
        RawImage[] feeds = new RawImage[4];
        Text[] labels = new Text[4];
        Text power, header;
        UIButton isoKitchen, isoBedroom, exit;
        static readonly string[] Zones = { "sala", "cocina", "pasillo", "dormitorio" };

        public override void Build(RectTransform layer)
        {
            root = UIKit.Rect("Isolation", layer); UIKit.Stretch(root);
            var bg = UIKit.Img("Bg", root, new Color(0.03f, 0.01f, 0.01f, 0.97f)); UIKit.Stretch(bg.rectTransform);
            header = UIKit.Txt("Header", root, 32, TextAnchor.UpperCenter, UIKit.Green);
            UIKit.Place(header.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(1600, 90));
            for (int i = 0; i < 4; i++)
            {
                int r = i / 2, c = i % 2;
                feeds[i] = UIKit.Raw("Feed" + i, root);
                UIKit.Center(feeds[i].rectTransform, new Vector2(-330 + c * 660, 200 - r * 400), new Vector2(560, 360));
                labels[i] = UIKit.Txt("Label" + i, feeds[i].rectTransform, 26, TextAnchor.UpperLeft, Color.white);
                UIKit.Stretch(labels[i].rectTransform, 14, 0, 0, 10);
            }
            power = UIKit.Txt("Power", root, 26, TextAnchor.MiddleCenter, Color.white);
            UIKit.Place(power.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 120), new Vector2(1600, 40));
            isoKitchen = new UIButton(root, Settings.T("AISLAR COCINA", "ISOLATE KITCHEN"), new Vector2(0.5f, 0), new Vector2(-360, 55), new Vector2(330, 62), 26);
            isoBedroom = new UIButton(root, Settings.T("AISLAR DORMITORIO", "ISOLATE BEDROOM"), new Vector2(0.5f, 0), new Vector2(0, 55), new Vector2(330, 62), 26);
            exit = new UIButton(root, Settings.T("Salir [Esc]", "Exit [Esc]"), new Vector2(0.5f, 0), new Vector2(360, 55), new Vector2(330, 62), 24);
        }

        void Update()
        {
            if (!IsOpen) return;
            var sys = SecurityCameraSystem.I;
            header.text = Settings.T("PROTOCOLO DE AISLAMIENTO — GUARDIÁN\n<size=22>Alarma: localiza movimiento · Cámaras: confirma · Cerraduras: encierra</size>",
                                     "ISOLATION PROTOCOL — GUARDIAN\n<size=22>Alarm: locate motion · Cameras: confirm · Locks: contain</size>");
            for (int i = 0; i < 4; i++)
            {
                var cam = sys != null ? sys.ByZone(Zones[i]) : null;
                if (cam != null) { sys.Request(cam); feeds[i].texture = cam.rt; }
                bool m = Motion.Contains(Zones[i]);
                var z = RoomZone.Get(Zones[i]);
                labels[i].text = (cam != null ? cam.camName : "") + "  " + (z != null ? z.DisplayName.ToUpperInvariant() : "") +
                                 (m ? Settings.T("   <color=#ff5544>● MOVIMIENTO</color>", "   <color=#ff5544>● MOTION</color>") : "");
            }
            bool ok = PowerPanel.I != null && PowerPanel.I.SecurityComplete;
            power.text = ok ? Settings.T("<color=#6dff8a>ENERGÍA: SEGURIDAD COMPLETA</color>", "<color=#6dff8a>POWER: FULL SECURITY</color>")
                            : Settings.T("<color=#ff5544>ENERGÍA INSUFICIENTE — se requiere ALARMA + CÁMARAS + CERRADURAS en el cuadro eléctrico</color>",
                                         "<color=#ff5544>INSUFFICIENT POWER — ALARM + CAMERAS + LOCKS required at the breaker panel</color>");
            isoKitchen.interactable = ok; isoBedroom.interactable = ok;
        }

        public override void Tick()
        {
            if (isoKitchen.Tick()) { UIManager.I.Close(this); GameEvents.Raise("isolate:cocina"); }
            else if (isoBedroom.Tick()) { UIManager.I.Close(this); GameEvents.Raise("isolate:dormitorio"); }
            else if (exit.Tick()) UIManager.I.Close(this);
        }
    }
}
