using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>Cuadro eléctrico: elegir circuitos sin superar 7 unidades.</summary>
    public class PowerPanelUI : UIPanel
    {
        UIButton[] toggles; UIButton apply, exit;
        Text load, info, sticker;
        Image bar;
        bool[] pending = new bool[5];

        public override void Build(RectTransform layer)
        {
            root = UIKit.Rect("Power", layer); UIKit.Stretch(root);
            var dim = UIKit.Img("Dim", root, new Color(0, 0, 0, 0.6f)); UIKit.Stretch(dim.rectTransform);
            var box = UIKit.Img("Box", root, new Color(0.17f, 0.18f, 0.17f, 1f));
            UIKit.Center(box.rectTransform, Vector2.zero, new Vector2(900, 860));
            var title = UIKit.Txt("Title", box.rectTransform, 34, TextAnchor.UpperCenter, Color.white);
            UIKit.Place(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(860, 50));
            title.text = Settings.T("CUADRO ELÉCTRICO — RESPALDO", "BREAKER PANEL — BACKUP");
            toggles = new UIButton[5];
            for (int i = 0; i < 5; i++) toggles[i] = new UIButton(box.rectTransform, "", new Vector2(0.5f, 1), new Vector2(0, -120 - i * 82), new Vector2(760, 68), 26);
            var barBg = UIKit.Img("BarBg", box.rectTransform, new Color(0.05f, 0.05f, 0.05f, 1f));
            UIKit.Place(barBg.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -540), new Vector2(760, 30));
            bar = UIKit.Img("Bar", barBg.rectTransform, UIKit.Green);
            bar.rectTransform.anchorMin = Vector2.zero; bar.rectTransform.offsetMin = bar.rectTransform.offsetMax = Vector2.zero;
            load = UIKit.Txt("Load", box.rectTransform, 28, TextAnchor.MiddleCenter, Color.white);
            UIKit.Place(load.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -585), new Vector2(760, 40));
            info = UIKit.Txt("Info", box.rectTransform, 24, TextAnchor.MiddleCenter, UIKit.Accent);
            UIKit.Place(info.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -630), new Vector2(820, 40));
            sticker = UIKit.Txt("Sticker", box.rectTransform, 20, TextAnchor.MiddleCenter, new Color(0.85f, 0.8f, 0.6f));
            UIKit.Place(sticker.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -675), new Vector2(820, 50));
            sticker.text = Settings.T("Etiqueta escrita a mano: \"Seguridad completa = alarma + cámaras + cerraduras. Las luces pueden esperar. — P.\"",
                                      "Handwritten label: \"Full security = alarm + cameras + locks. The lights can wait. — D.\"");
            apply = new UIButton(box.rectTransform, Settings.T("ACTIVAR", "ENGAGE"), new Vector2(0.5f, 0), new Vector2(-150, 50), new Vector2(260, 60), 28);
            exit = new UIButton(box.rectTransform, Settings.T("Salir [Esc]", "Exit [Esc]"), new Vector2(0.5f, 0), new Vector2(150, 50), new Vector2(260, 60), 24);
        }

        public override void OnOpen()
        {
            var p = PowerPanel.I;
            for (int i = 0; i < 5; i++) pending[i] = p != null && p.state[i];
            info.text = p != null && p.Tripped ? Settings.T("DIFERENCIAL DISPARADO — sin energía", "BREAKER TRIPPED — no power") : "";
            Refresh();
        }

        public override void Tick()
        {
            var p = PowerPanel.I; if (p == null) return;
            int d = Inp.Digit();
            for (int i = 0; i < 5; i++)
            {
                bool usable = i < 4 || p.c06Known;
                toggles[i].interactable = usable;
                if ((toggles[i].Tick() || (d == i + 1 && usable))) { pending[i] = !pending[i]; if (AudioManager.I) AudioManager.I.UI(AudioManager.I.click, 0.7f); }
            }
            if (apply.Tick() || Inp.Enter())
            {
                if (p.Apply(pending))
                {
                    info.text = p.SecurityComplete ? Settings.T("SEGURIDAD COMPLETA", "FULL SECURITY") : Settings.T("Configuración aplicada", "Configuration applied");
                    if (AudioManager.I) AudioManager.I.UI(AudioManager.I.lockClunk, 0.8f);
                }
                else
                {
                    info.text = Settings.T("<color=#ff5544>¡SOBRECARGA! El diferencial saltó. Todo quedó sin energía.</color>", "<color=#ff5544>OVERLOAD! The breaker tripped. Everything lost power.</color>");
                    for (int i = 0; i < 5; i++) pending[i] = false;
                }
            }
            if (exit.Tick()) UIManager.I.Close(this);
            Refresh();
        }

        void Refresh()
        {
            var p = PowerPanel.I; if (p == null) return;
            for (int i = 0; i < 5; i++)
            {
                string name = i == 4 && !p.c06Known ? Settings.T("Circuito 06 — sin etiqueta (?)", "Circuit 06 — unlabeled (?)") : PowerPanel.Name((Circuit)i);
                bool live = p.state[i] && !p.Tripped;
                toggles[i].SetText("[" + (i + 1) + "]  " + name + "   (" + p.cost[i] + ")      " +
                    (pending[i] ? "<color=#6dff8a>● ON</color>" : "<color=#ff6655>○ OFF</color>") +
                    (live ? "" : "  <size=18><color=#888>" + Settings.T("(sin corriente)", "(no power)") + "</color></size>"));
            }
            int l = p.Load(pending);
            float f = Mathf.Clamp01(l / (float)p.capacity);
            bar.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(l / 9f), 1);
            bar.color = l > p.capacity ? UIKit.Red : (l == p.capacity ? UIKit.Accent : UIKit.Green);
            load.text = Settings.T("CONSUMO: ", "LOAD: ") + l + " / " + p.capacity + (l > p.capacity ? Settings.T("  — SUPERA LA CAPACIDAD", "  — OVER CAPACITY") : "");
        }
    }
}
