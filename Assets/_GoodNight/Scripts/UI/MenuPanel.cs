using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>Menú principal: Nueva partida, Continuar, Opciones, Salir.</summary>
    public class MenuPanel : UIPanel
    {
        UIButton newGame, cont, options, quit;
        public override void Build(RectTransform layer)
        {
            root = UIKit.Rect("Menu", layer); UIKit.Stretch(root);
            var shade = UIKit.Img("Shade", root, new Color(0, 0, 0, 0.45f)); UIKit.Stretch(shade.rectTransform);
            var title = UIKit.Txt("Title", root, 120, TextAnchor.MiddleLeft, new Color(0.92f, 0.9f, 0.84f));
            UIKit.Place(title.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(140, 250), new Vector2(1200, 180));
            title.text = "GOOD NIGHT";
            var sub = UIKit.Txt("Sub", root, 28, TextAnchor.MiddleLeft, UIKit.Muted);
            UIKit.Place(sub.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(146, 160), new Vector2(1200, 50));
            sub.text = Settings.T("Una noche. Una casa. Alguien más está adentro.", "One night. One house. Someone else is inside.");
            newGame = new UIButton(root, Settings.T("Nueva partida", "New game"), new Vector2(0, 0.5f), new Vector2(330, 40), new Vector2(380, 62), 30);
            cont = new UIButton(root, Settings.T("Continuar", "Continue"), new Vector2(0, 0.5f), new Vector2(330, -35), new Vector2(380, 62), 30);
            options = new UIButton(root, Settings.T("Opciones", "Options"), new Vector2(0, 0.5f), new Vector2(330, -110), new Vector2(380, 62), 30);
            quit = new UIButton(root, Settings.T("Salir", "Quit"), new Vector2(0, 0.5f), new Vector2(330, -185), new Vector2(380, 62), 30);
            var foot = UIKit.Txt("Foot", root, 20, TextAnchor.LowerLeft, UIKit.Muted);
            UIKit.Place(foot.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(146, 40), new Vector2(1400, 40));
            foot.text = Settings.T("WASD mover · Ratón mirar · Shift correr · Ctrl agacharse · E interactuar · Tab teléfono · F linterna · Esc pausa",
                                   "WASD move · Mouse look · Shift run · Ctrl crouch · E interact · Tab phone · F flashlight · Esc pause");
        }
        public override void OnOpen() { cont.interactable = Checkpoint.HasSave; }
        public override void OnEsc() { }
        public override void Tick()
        {
            if (newGame.Tick()) { UIManager.I.Close(this); GameManager.I.StartGame(Stage.Act1); }
            else if (cont.Tick()) { UIManager.I.Close(this); GameManager.I.StartGame(Checkpoint.Saved); }
            else if (options.Tick()) UIManager.I.Open(UIManager.I.Get<OptionsPanel>());
            else if (quit.Tick()) GameManager.Quit();
        }
    }
}
