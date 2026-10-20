using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>Pregunta Sí/No.</summary>
    public class ConfirmPanel : UIPanel
    {
        Text question; UIButton yes, no; System.Action onYes;
        public override void Build(RectTransform layer)
        {
            root = UIKit.Rect("Confirm", layer); UIKit.Stretch(root);
            var dim = UIKit.Img("Dim", root, new Color(0, 0, 0, 0.55f)); UIKit.Stretch(dim.rectTransform);
            question = UIKit.Txt("Q", root, 54, TextAnchor.MiddleCenter, Color.white);
            UIKit.Center(question.rectTransform, new Vector2(0, 80), new Vector2(1400, 120));
            yes = new UIButton(root, Settings.T("Sí [E]", "Yes [E]"), new Vector2(0.5f, 0.5f), new Vector2(-160, -60), new Vector2(260, 70), 30);
            no = new UIButton(root, Settings.T("No [Esc]", "No [Esc]"), new Vector2(0.5f, 0.5f), new Vector2(160, -60), new Vector2(260, 70), 30);
        }
        public void Ask(string q, System.Action yesAction) { question.text = q; onYes = yesAction; UIManager.I.Open(this); }
        public override void Tick()
        {
            if (yes.Tick() || Inp.Interact()) { UIManager.I.Close(this); if (onYes != null) onYes(); }
            else if (no.Tick()) UIManager.I.Close(this);
        }
    }
}
