using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    public class OptionsPanel : UIPanel
    {
        UISlider master, sfx, sens, bright;
        UIButton quality, vhs, full, lang, back;
        public override void Build(RectTransform layer)
        {
            root = UIKit.Rect("Options", layer); UIKit.Stretch(root);
            var dim = UIKit.Img("Dim", root, new Color(0.02f, 0.02f, 0.03f, 0.96f)); UIKit.Stretch(dim.rectTransform);
            var t = UIKit.Txt("Title", root, 56, TextAnchor.MiddleCenter, Color.white);
            UIKit.Center(t.rectTransform, new Vector2(0, 380), new Vector2(800, 90)); t.text = Settings.T("OPCIONES", "OPTIONS");
            master = new UISlider(root, Settings.T("Volumen general", "Master volume"), new Vector2(0, 270), Settings.Master);
            sfx = new UISlider(root, Settings.T("Volumen de efectos", "Effects volume"), new Vector2(0, 210), Settings.Sfx);
            sens = new UISlider(root, Settings.T("Sensibilidad del ratón", "Mouse sensitivity"), new Vector2(0, 150), Mathf.InverseLerp(0.3f, 5f, Settings.Sensitivity));
            bright = new UISlider(root, Settings.T("Brillo", "Brightness"), new Vector2(0, 90), Settings.Brightness);
            quality = new UIButton(root, "", new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(560, 58), 26);
            quality.SetActive(false);   // (18/10) sin selector de calidad: siempre ALTA
            vhs = new UIButton(root, "", new Vector2(0.5f, 0.5f), new Vector2(0, -60), new Vector2(560, 58), 26);
            full = new UIButton(root, "", new Vector2(0.5f, 0.5f), new Vector2(0, -130), new Vector2(560, 58), 26);
            lang = new UIButton(root, "", new Vector2(0.5f, 0.5f), new Vector2(0, -200), new Vector2(560, 58), 26);
            back = new UIButton(root, Settings.T("Volver", "Back"), new Vector2(0.5f, 0.5f), new Vector2(0, -300), new Vector2(320, 60), 28);
        }
        public override void OnOpen() { master.Value = Settings.Master; sfx.Value = Settings.Sfx; bright.Value = Settings.Brightness; Labels(); }
        void Labels()
        {
            quality.SetText(Settings.T("Calidad gráfica: ", "Graphics quality: ") + Settings.QualityName);
            vhs.SetText(Settings.T("Efecto VHS: ", "VHS effect: ") + Settings.VhsName);
            full.SetText(Settings.T("Pantalla completa: ", "Fullscreen: ") + (Settings.Fullscreen ? Settings.T("Sí", "Yes") : "No"));
            lang.SetText(Settings.T("Subtítulos: Español", "Subtitles: English"));
        }
        public override void Tick()
        {
            if (master.Tick()) { Settings.Master = master.Value; AudioListener.volume = Settings.Master; }
            if (sfx.Tick()) Settings.Sfx = sfx.Value;
            if (sens.Tick()) Settings.Sensitivity = Mathf.Lerp(0.3f, 5f, sens.Value);
            if (bright.Tick()) Settings.Brightness = bright.Value;
            if (vhs.Tick()) { Settings.Vhs = (Settings.Vhs + 1) % 3; Settings.Apply(); Labels(); }
            if (full.Tick()) { Settings.Fullscreen = !Settings.Fullscreen; Settings.Apply(); Labels(); }
            if (lang.Tick()) { Settings.Language = Settings.Language == Lang.ES ? Lang.EN : Lang.ES; Settings.Apply(); Labels(); }
            if (back.Tick()) OnEsc();
        }
        public override void OnEsc() { Settings.Save(); UIManager.I.Close(this); }
    }
}
