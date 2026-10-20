using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>Visor de documentos legible.</summary>
    public class DocumentPanel : UIPanel
    {
        Text title, body; RawImage image;
        public override bool CloseWithInteract => true;

        public override void Build(RectTransform layer)
        {
            root = UIKit.Rect("Document", layer); UIKit.Stretch(root);
            var dim = UIKit.Img("Dim", root, new Color(0, 0, 0, 0.7f)); UIKit.Stretch(dim.rectTransform);
            var paper = UIKit.Img("Paper", root, new Color(0.86f, 0.82f, 0.72f, 1f));
            UIKit.Center(paper.rectTransform, Vector2.zero, new Vector2(1000, 900));
            title = UIKit.Txt("Title", paper.rectTransform, 34, TextAnchor.UpperCenter, new Color(0.15f, 0.12f, 0.1f), false);
            UIKit.Place(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -36), new Vector2(900, 60));
            image = UIKit.Raw("Image", paper.rectTransform);
            UIKit.Place(image.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -110), new Vector2(560, 380));
            body = UIKit.Txt("Body", paper.rectTransform, 27, TextAnchor.UpperLeft, new Color(0.13f, 0.11f, 0.09f), false);
            UIKit.Stretch(body.rectTransform, 60, 70, 60, 110);
            var hint = UIKit.Txt("Hint", paper.rectTransform, 20, TextAnchor.LowerCenter, new Color(0.3f, 0.28f, 0.25f), false);
            UIKit.Place(hint.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(900, 30));
            hint.text = Settings.T("[E] / [Esc] / clic: cerrar", "[E] / [Esc] / click: close");
        }

        public void Show(DocumentData d)
        {
            if (d == null) return;
            title.text = d.Title;
            body.text = d.Body;
            image.texture = d.image;
            image.gameObject.SetActive(d.image != null);
            UIKit.Stretch(body.rectTransform, 60, 70, 60, d.image != null ? 510 : 110);
            if (AudioManager.I) AudioManager.I.UI(AudioManager.I.pageFlip, 0.7f);
            UIManager.I.Open(this);
        }

        public override void Tick() { if (Inp.Click()) UIManager.I.Close(this); }
    }
}
