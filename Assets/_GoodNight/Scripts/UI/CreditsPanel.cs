using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    public class CreditsPanel : UIPanel
    {
        RectTransform scroll; Text body; float y;
        public override void Build(RectTransform layer)
        {
            root = UIKit.Rect("Credits", layer); UIKit.Stretch(root);
            var bg = UIKit.Img("Bg", root, Color.black); UIKit.Stretch(bg.rectTransform);
            body = UIKit.Txt("Body", root, 34, TextAnchor.UpperCenter, new Color(0.88f, 0.86f, 0.8f));
            scroll = body.rectTransform;
            UIKit.Place(scroll, new Vector2(0.5f, 0), new Vector2(0.5f, 1), Vector2.zero, new Vector2(1400, 2200));
        }
        public void Roll()
        {
            body.text = "<size=96>GOOD NIGHT</size>\n\n\n" +
                Settings.T("Un juego de terror psicológico", "A psychological horror game") + "\n\n\n" +
                Settings.T("DISEÑO Y DESARROLLO", "DESIGN & DEVELOPMENT") + "\n" + Settings.T("Equipo GOOD NIGHT — Game Jam", "GOOD NIGHT team — Game Jam") + "\n\n" +
                Settings.T("PROGRAMACIÓN · NIVELES · NARRATIVA", "PROGRAMMING · LEVELS · NARRATIVE") + "\nKevin Mollo " + Settings.T("y equipo", "& team") + "\n\n" +
                Settings.T("ASISTENCIA DE DESARROLLO", "DEVELOPMENT ASSISTANCE") + "\nClaude (Anthropic)\n\n" +
                Settings.T("MOTOR", "ENGINE") + "\nUnity 6.3 LTS\n\n" +
                Settings.T("Texturas y sonidos generados proceduralmente para este proyecto.", "Textures and sounds generated procedurally for this project.") + "\n\n\n\n" +
                Settings.T("Gracias por jugar.", "Thanks for playing.") + "\n\n\n" +
                "<color=#8a1a14>" + Settings.T("Buenas noches.", "Good night.") + "</color>";
            y = -200f;
            UIManager.I.Open(this);
        }
        public override void OnEsc() { Checkpoint.ReloadAt(Stage.None); }
        void Update()
        {
            if (!IsOpen) return;
            y += Time.unscaledDeltaTime * 70f;
            scroll.anchoredPosition = new Vector2(0, y);
            if (y > 2400f) Checkpoint.ReloadAt(Stage.None);
        }
    }
}
