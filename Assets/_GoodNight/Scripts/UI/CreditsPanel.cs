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
                Settings.T("UN JUEGO DE", "A GAME BY") + "\n\n<size=44>KEVIN MOLLO\nSEBASTIÁN CONDORI\nSANTIAGO BORQUEZ\nDAVID BUSTOS</size>\n\n\n" +
                Settings.T("DESARROLLADO CON UNITY", "DEVELOPED WITH UNITY") + "\n\n" +
                Settings.T("CREADO PARA GAMEJAMPLUS 2026/27", "CREATED FOR GAMEJAMPLUS 2026/27") + "\n\n\n\n" +
                "<size=52>" + Settings.T("GRACIAS POR JUGAR", "THANK YOU FOR PLAYING") + "</size>\n\n\n" +
                "<color=#8a1a14>" + Settings.T("Buenas noches.", "Good night.") + "</color>\n\n\n\n" +
                "<size=22><color=#777777>" + Settings.T("RECURSOS Y LICENCIAS", "ASSETS & LICENSES") + "\n" +
                "KayKit — Kay Lousberg (CC0) · www.kaylousberg.com\n" +
                Settings.T("Voces sintéticas: Piper TTS (MIT) · voces «mls_10246» (CC BY 4.0, Multilingual LibriSpeech, OpenSLR) y «carlfm» (dominio público)", "Synthetic voices: Piper TTS (MIT) · voices \"mls_10246\" (CC BY 4.0, Multilingual LibriSpeech, OpenSLR) and \"carlfm\" (public domain)") + "\n" +
                Settings.T("Texturas, modelos y efectos de sonido generados por código para este proyecto.", "Textures, models and sound effects generated in code for this project.") + "\n" +
                Settings.T("Herramientas: Unity · C# · Blender · Python · Git y GitHub.", "Tools: Unity · C# · Blender · Python · Git and GitHub.") + "</color></size>";
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
