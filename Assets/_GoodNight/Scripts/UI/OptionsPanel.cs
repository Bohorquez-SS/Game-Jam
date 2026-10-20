using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    public class OptionsPanel : UIPanel
    {
        UISlider master, sfx, sens;
        UIButton full, lang, back;
        public override void Build(RectTransform layer)
        {
            root = UIKit.Rect("Options", layer); UIKit.Stretch(root);
            var dim = UIKit.Img("Dim", root, new Color(0.02f, 0.02f, 0.03f, 0.96f)); UIKit.Stretch(dim.rectTransform);
            var t = UIKit.Txt("Title", root, 56, TextAnchor.MiddleCenter, Color.white);
            UIKit.Center(t.rectTransform, new Vector2(0, 300), new Vector2(800, 90)); t.text = Settings.T("OPCIONES", "OPTIONS");
            master = new UISlider(root, Settings.T("Volumen general", "Master volume"), new Vector2(0, 170), Settings.Master);
            sfx = new UISlider(root, Settings.T("Volumen de efectos", "Effects volume"), new Vector2(0, 100), Settings.Sfx);
            sens = new UISlider(root, Settings.T("Sensibilidad del ratón", "Mouse sensitivity"), new Vector2(0, 30), Mathf.InverseLerp(0.3f, 5f, Settings.Sensitivity));
            full = new UIButton(root, "", new Vector2(0.5f, 0.5f), new Vector2(0, -60), new Vector2(520, 60), 26);
            lang = new UIButton(root, "", new Vector2(0.5f, 0.5f), new Vector2(0, -135), new Vector2(520, 60), 26);
            back = new UIButton(root, Settings.T("Volver", "Back"), new Vector2(0.5f, 0.5f), new Vector2(0, -240), new Vector2(320, 60), 28);
        }
        public override void OnOpen() { master.Value = Settings.Master; sfx.Value = Settings.Sfx; Labels(); }
        void Labels()
        {
            full.SetText(Settings.T("Pantalla completa: ", "Fullscreen: ") + (Settings.Fullscreen ? Settings.T("Sí", "Yes") : "No"));
            lang.SetText(Settings.T("Subtítulos: Español", "Subtitles: English"));
        }
        public override void Tick()
        {
            if (master.Tick()) { Settings.Master = master.Value; AudioListener.volume = Settings.Master; }
            if (sfx.Tick()) Settings.Sfx = sfx.Value;
            if (sens.Tick()) Settings.Sensitivity = Mathf.Lerp(0.3f, 5f, sens.Value);
            if (full.Tick()) { Settings.Fullscreen = !Settings.Fullscreen; Settings.Apply(); Labels(); }
            if (lang.Tick()) { Settings.Language = Settings.Language == Lang.ES ? Lang.EN : Lang.ES; Labels(); }
            if (back.Tick()) OnEsc();
        }
        public override void OnEsc() { Settings.Save(); UIManager.I.Close(this); }
    }
}
