using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>Teléfono (Tab): Mensajes, Objetivos, Pistas y Documentos.</summary>
    public class PhonePanel : UIPanel
    {
        RectTransform frame, docList;
        Text header, body;
        UIButton[] tabs;
        readonly List<UIButton> docButtons = new List<UIButton>();
        readonly List<DocumentData> docRefs = new List<DocumentData>();
        int tab;
        float slide;
        static readonly string[] TabEs = { "Mensajes", "Objetivos", "Pistas", "Documentos" };
        static readonly string[] TabEn = { "Messages", "Objectives", "Clues", "Documents" };

        public override void Build(RectTransform layer)
        {
            root = UIKit.Rect("Phone", layer); UIKit.Stretch(root);
            var dim = UIKit.Img("Dim", root, new Color(0, 0, 0, 0.35f)); UIKit.Stretch(dim.rectTransform);

            var body0 = UIKit.Img("Frame", root, new Color(0.02f, 0.02f, 0.025f, 1f));
            frame = body0.rectTransform;
            UIKit.Place(frame, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-90, 0), new Vector2(600, 960));
            var screen = UIKit.Img("Screen", frame, new Color(0.08f, 0.09f, 0.12f, 1f));
            UIKit.Stretch(screen.rectTransform, 16, 60, 16, 40);

            header = UIKit.Txt("Header", frame, 22, TextAnchor.MiddleCenter, new Color(0.75f, 0.8f, 0.9f));
            UIKit.Place(header.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -4), new Vector2(560, 36));

            tabs = new UIButton[4];
            for (int i = 0; i < 4; i++)
            {
                tabs[i] = new UIButton(screen.rectTransform, TabEs[i], new Vector2(0.5f, 1), new Vector2(-204 + i * 136, -32), new Vector2(130, 46), 19);
            }

            body = UIKit.Txt("Body", screen.rectTransform, 22, TextAnchor.UpperLeft, Color.white, false);
            UIKit.Stretch(body.rectTransform, 22, 20, 22, 70);
            body.verticalOverflow = VerticalWrapMode.Truncate;

            docList = UIKit.Rect("Docs", screen.rectTransform); UIKit.Stretch(docList, 20, 20, 20, 74);

            var hint = UIKit.Txt("Hint", frame, 18, TextAnchor.MiddleCenter, UIKit.Muted);
            UIKit.Place(hint.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 14), new Vector2(560, 30));
            hint.text = Settings.T("[1-4] pestañas · clic para abrir · [Tab] guardar", "[1-4] tabs · click to open · [Tab] put away");
        }

        public override void OnOpen()
        {
            slide = 0f;
            if (PhoneSystem.I) { PhoneSystem.I.MarkRead(); PhoneSystem.I.Changed += Refresh; }
            if (ObjectiveManager.I) ObjectiveManager.I.Changed += Refresh;
            if (JournalManager.I) JournalManager.I.Changed += Refresh;
            Refresh();
        }

        public override void OnClose()
        {
            if (PhoneSystem.I) PhoneSystem.I.Changed -= Refresh;
            if (ObjectiveManager.I) ObjectiveManager.I.Changed -= Refresh;
            if (JournalManager.I) JournalManager.I.Changed -= Refresh;
        }

        void Update()
        {
            if (!IsOpen) return;
            slide = Mathf.MoveTowards(slide, 1f, Time.unscaledDeltaTime * 4f);
            frame.anchoredPosition = new Vector2(Mathf.Lerp(700, -90, Mathf.SmoothStep(0, 1, slide)), 0);
            header.text = GameClock.Text + "        4G  |||";
        }

        public override void Tick()
        {
            int d = Inp.Digit();
            if (d >= 1 && d <= 4) { tab = d - 1; Refresh(); }
            for (int i = 0; i < tabs.Length; i++) if (tabs[i].Tick()) { tab = i; Refresh(); }
            if (tab == 3)
                for (int i = 0; i < docButtons.Count; i++)
                    if (docButtons[i].Tick()) UIManager.I.Get<DocumentPanel>().Show(docRefs[i]);
        }

        void Refresh()
        {
            if (!IsOpen) return;
            for (int i = 0; i < tabs.Length; i++)
            {
                tabs[i].SetText(Settings.T(TabEs[i], TabEn[i]));
                tabs[i].normal = i == tab ? new Color(0.32f, 0.27f, 0.18f, 1f) : new Color(0.14f, 0.15f, 0.18f, 0.95f);
            }
            body.gameObject.SetActive(tab != 3);
            docList.gameObject.SetActive(tab == 3);
            var sb = new StringBuilder();
            switch (tab)
            {
                case 0:
                    var msgs = PhoneSystem.I != null ? PhoneSystem.I.Messages : null;
                    if (msgs == null || msgs.Count == 0) sb.Append(Settings.T("<color=#888>Sin mensajes.</color>", "<color=#888>No messages.</color>"));
                    else
                    {
                        int start = Mathf.Max(0, msgs.Count - 7);
                        for (int i = start; i < msgs.Count; i++)
                        {
                            var m = msgs[i];
                            string col = m.fromSystem ? "#6dff8a" : (m.fromUnknown ? "#ff7070" : "#9fc4ff");
                            sb.Append("<color=").Append(col).Append("><b>").Append(m.from).Append("</b></color>  <color=#777>").Append(m.time).Append("</color>\n")
                              .Append(m.text).Append("\n\n");
                        }
                    }
                    break;
                case 1:
                    var o = ObjectiveManager.I;
                    if (o == null || string.IsNullOrEmpty(o.Title)) sb.Append("—");
                    else
                    {
                        sb.Append("<color=#f2c66d><b>").Append(o.Title.ToUpperInvariant()).Append("</b></color>\n\n").Append(o.ItemsText());
                        if (!string.IsNullOrEmpty(o.Hint))
                            sb.Append("\n<color=#9a9a9a>").Append(Settings.T("Pista opcional: ", "Optional hint: ")).Append(o.Hint).Append("</color>");
                    }
                    break;
                case 2:
                    var j = JournalManager.I;
                    if (j == null || j.Clues.Count == 0) sb.Append(Settings.T("<color=#888>Todavía no anotaste pistas.</color>", "<color=#888>No clues yet.</color>"));
                    else foreach (var c in j.Clues) sb.Append("• ").Append(c).Append("\n\n");
                    break;
                case 3:
                    RebuildDocs();
                    break;
            }
            body.text = sb.ToString();
        }

        void RebuildDocs()
        {
            foreach (var b in docButtons) Destroy(b.rt.gameObject);
            docButtons.Clear(); docRefs.Clear();
            var j = JournalManager.I;
            if (j == null) return;
            for (int i = 0; i < j.Documents.Count; i++)
            {
                var b = new UIButton(docList, j.Documents[i].Title, new Vector2(0.5f, 1), new Vector2(0, -30 - i * 64), new Vector2(520, 56), 22);
                docButtons.Add(b); docRefs.Add(j.Documents[i]);
            }
        }
    }
}
