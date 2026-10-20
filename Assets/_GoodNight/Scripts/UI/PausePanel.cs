using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    public class PausePanel : UIPanel
    {
        UIButton resume, options, menu, quit;
        public override void Build(RectTransform layer)
        {
            root = UIKit.Rect("Pause", layer); UIKit.Stretch(root);
            var dim = UIKit.Img("Dim", root, new Color(0, 0, 0, 0.75f)); UIKit.Stretch(dim.rectTransform);
            var t = UIKit.Txt("Title", root, 64, TextAnchor.MiddleCenter, Color.white);
            UIKit.Center(t.rectTransform, new Vector2(0, 230), new Vector2(800, 100)); t.text = Settings.T("PAUSA", "PAUSED");
            resume = new UIButton(root, Settings.T("Continuar", "Resume"), new Vector2(0.5f, 0.5f), new Vector2(0, 80), new Vector2(420, 62), 30);
            options = new UIButton(root, Settings.T("Opciones", "Options"), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(420, 62), 30);
            menu = new UIButton(root, Settings.T("Menú principal", "Main menu"), new Vector2(0.5f, 0.5f), new Vector2(0, -80), new Vector2(420, 62), 30);
            quit = new UIButton(root, Settings.T("Salir del juego", "Quit game"), new Vector2(0.5f, 0.5f), new Vector2(0, -160), new Vector2(420, 62), 30);
        }
        public override void OnOpen() { Time.timeScale = 0f; AudioListener.pause = true; }
        public override void OnClose() { Time.timeScale = 1f; AudioListener.pause = false; }
        public override void Tick()
        {
            if (resume.Tick()) UIManager.I.Close(this);
            else if (options.Tick()) UIManager.I.Open(UIManager.I.Get<OptionsPanel>());
            else if (menu.Tick()) { UIManager.I.Close(this); Checkpoint.ReloadAt(Stage.None); }
            else if (quit.Tick()) GameManager.Quit();
        }
    }
}
