using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    public class DeathPanel : UIPanel
    {
        Text quote; UIButton retry, menu; float t; CanvasGroup group;
        public override void Build(RectTransform layer)
        {
            root = UIKit.Rect("Death", layer); UIKit.Stretch(root);
            group = root.gameObject.AddComponent<CanvasGroup>();
            var bg = UIKit.Img("Bg", root, Color.black); UIKit.Stretch(bg.rectTransform);
            var title = UIKit.Txt("Title", root, 110, TextAnchor.MiddleCenter, new Color(0.75f, 0.06f, 0.05f));
            UIKit.Center(title.rectTransform, new Vector2(0, 170), new Vector2(1400, 160)); title.text = "GOOD NIGHT";
            quote = UIKit.Txt("Quote", root, 34, TextAnchor.MiddleCenter, new Color(0.85f, 0.82f, 0.78f));
            UIKit.Center(quote.rectTransform, new Vector2(0, 50), new Vector2(1400, 80));
            retry = new UIButton(root, Settings.T("REINTENTAR DESDE EL ÚLTIMO PUNTO SEGURO", "RETRY FROM LAST SAFE POINT"), new Vector2(0.5f, 0.5f), new Vector2(0, -90), new Vector2(760, 66), 28);
            menu = new UIButton(root, Settings.T("Menú principal", "Main menu"), new Vector2(0.5f, 0.5f), new Vector2(0, -175), new Vector2(420, 60), 26);
        }
        public void Show(string q) { quote.text = q; t = 0f; UIManager.I.Open(this); }
        public override void OnEsc() { }
        void Update() { if (IsOpen && group) { t += Time.unscaledDeltaTime; group.alpha = Mathf.Clamp01(t / 1.2f); } }
        public override void Tick()
        {
            if (t < 1.2f) return;
            if (retry.Tick() || Inp.Enter()) Checkpoint.ReloadAt(Checkpoint.HasSave ? Checkpoint.Saved : Stage.C1_Noche);
            else if (menu.Tick()) Checkpoint.ReloadAt(Stage.None);
        }
    }
}
