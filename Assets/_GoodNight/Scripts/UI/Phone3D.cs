using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>
    /// Teléfono físico en la mano del protagonista. [Tab] lo saca del bolsillo con animación; la pantalla es un
    /// Canvas en el mundo (capa primera persona, resolución completa) con apps: Mensajes (burbujas), Objetivos,
    /// Pistas y Objetos. Recibe notificaciones, llamadas y "se levanta solo" en mensajes importantes.
    /// </summary>
    public class Phone3D : MonoBehaviour
    {
        public static Phone3D I;
        public static bool IsOpen => I != null && I.state == 2;
        public static bool IsVisible => I != null && I.show > 0.05f;

        public Transform phoneRoot;       // modelo del teléfono (+ mano izquierda)
        public Transform screenAnchor;    // centro de la pantalla
        public Camera fpCam;
        public Texture2D wallpaper;
        public Vector3 shownPos = new Vector3(-0.055f, -0.05f, 0.30f), glancePos = new Vector3(-0.1f, -0.17f, 0.32f), hiddenPos = new Vector3(-0.08f, -0.55f, 0.24f);
        public Vector3 shownRot = new Vector3(-6f, 9f, 2f), hiddenRot = new Vector3(65f, 20f, 10f);

        // estado: 0 oculto, 1 vistazo, 2 abierto
        int state; float show, glanceT;
        int app, sel; string openThread; float scroll, targetScroll;
        RectTransform canvasRT, content, contentArea, tabBar, notif, callLayer;
        Text clock, statusRight, title, back, notifText, callName, callStatus, callHint;
        Image[] tabBg = new Image[4];
        Text[] tabLabel = new Text[4];
        readonly List<RectTransform> rows = new List<RectTransform>();
        bool dirty = true; float notifT; float battery = 0.78f;
        static readonly string[] Apps = { "Mensajes", "Objetivos", "Pistas", "Objetos" };
        static readonly string[] AppsEn = { "Messages", "Objectives", "Clues", "Items" };
        static readonly string[] Threads = { PhoneSystem.Mama, PhoneSystem.Unknown, PhoneSystem.Guardian };

        const float W = 360, H = 760;

        void Awake()
        {
            I = this;
            BuildUI();
            phoneRoot.localPosition = hiddenPos; phoneRoot.localRotation = Quaternion.Euler(hiddenRot);
            phoneRoot.gameObject.SetActive(false);
        }

        void Start()
        {
            if (PhoneSystem.I) PhoneSystem.I.Changed += () => dirty = true;
            if (ObjectiveManager.I) ObjectiveManager.I.Changed += () => dirty = true;
            if (JournalManager.I) JournalManager.I.Changed += () => dirty = true;
            Inventory.Changed += () => dirty = true;
            Settings.Changed += () => dirty = true;
        }

        // ================================================================== construcción de la UI
        void BuildUI()
        {
            var go = new GameObject("PhoneCanvas", typeof(RectTransform));
            go.transform.SetParent(screenAnchor, false);
            canvasRT = (RectTransform)go.transform;
            canvasRT.sizeDelta = new Vector2(W, H);
            canvasRT.localScale = Vector3.one * 0.0002f;
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = fpCam;
            var sc = go.AddComponent<CanvasScaler>(); sc.dynamicPixelsPerUnit = 4f;

            var bg = UIKit.Raw("Wallpaper", canvasRT); UIKit.Stretch(bg.rectTransform); bg.texture = wallpaper; bg.color = new Color(1, 1, 1, 1);
            // barra de estado
            clock = UIKit.Txt("Clock", canvasRT, 17, TextAnchor.MiddleLeft, Color.white, false);
            UIKit.Place(clock.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -4), new Vector2(120, 26));
            statusRight = UIKit.Txt("Status", canvasRT, 14, TextAnchor.MiddleRight, Color.white, false);
            UIKit.Place(statusRight.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -4), new Vector2(180, 26));
            var notch = UIKit.Img("Notch", canvasRT, Color.black);
            UIKit.Place(notch.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -6), new Vector2(18, 18));
            // título de la app
            var titleBar = UIKit.Img("TitleBar", canvasRT, new Color(0.07f, 0.08f, 0.11f, 0.92f));
            UIKit.Place(titleBar.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -32), new Vector2(W, 52));
            title = UIKit.Txt("Title", titleBar.rectTransform, 21, TextAnchor.MiddleCenter, Color.white, false);
            UIKit.Stretch(title.rectTransform, 60, 0, 60, 0);
            back = UIKit.Txt("Back", titleBar.rectTransform, 15, TextAnchor.MiddleLeft, new Color(0.45f, 0.65f, 1f), false);
            UIKit.Stretch(back.rectTransform, 14, 0, 0, 0);
            // área de contenido con máscara
            var ca = UIKit.Img("ContentArea", canvasRT, new Color(0, 0, 0, 0.25f));
            contentArea = ca.rectTransform;
            UIKit.Place(contentArea, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -84), new Vector2(W, H - 84 - 74));
            contentArea.gameObject.AddComponent<RectMask2D>();
            content = UIKit.Rect("Content", contentArea);
            content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1); content.pivot = new Vector2(0.5f, 1);
            content.sizeDelta = new Vector2(0, 10); content.anchoredPosition = Vector2.zero;
            // barra de apps
            var tb = UIKit.Img("TabBar", canvasRT, new Color(0.05f, 0.055f, 0.075f, 0.97f));
            tabBar = tb.rectTransform;
            UIKit.Place(tabBar, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(W, 74));
            for (int i = 0; i < 4; i++)
            {
                tabBg[i] = UIKit.Img("Tab" + i, tabBar, new Color(0, 0, 0, 0));
                UIKit.Place(tabBg[i].rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(6 + i * 87, 2), new Vector2(84, 60));
                tabLabel[i] = UIKit.Txt("L", tabBg[i].rectTransform, 13, TextAnchor.MiddleCenter, Color.white, false);
                UIKit.Stretch(tabLabel[i].rectTransform);
            }
            // notificación dentro del teléfono
            var nb = UIKit.Img("Notif", canvasRT, new Color(0.16f, 0.18f, 0.23f, 0.97f));
            notif = nb.rectTransform;
            UIKit.Place(notif, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, 80), new Vector2(W - 20, 84));
            notifText = UIKit.Txt("T", notif, 15, TextAnchor.MiddleLeft, Color.white, false);
            UIKit.Stretch(notifText.rectTransform, 14, 6, 14, 6);
            // capa de llamada
            var cl = UIKit.Img("Call", canvasRT, new Color(0.04f, 0.06f, 0.09f, 0.98f));
            callLayer = cl.rectTransform; UIKit.Stretch(callLayer);
            var av = UIKit.Img("Avatar", callLayer, new Color(0.35f, 0.42f, 0.6f));
            UIKit.Place(av.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -200), new Vector2(120, 120));
            callName = UIKit.Txt("Name", callLayer, 30, TextAnchor.MiddleCenter, Color.white, false);
            UIKit.Place(callName.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -310), new Vector2(W, 50));
            callStatus = UIKit.Txt("Status", callLayer, 18, TextAnchor.MiddleCenter, new Color(0.7f, 0.75f, 0.8f), false);
            UIKit.Place(callStatus.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -350), new Vector2(W, 40));
            callHint = UIKit.Txt("Hint", callLayer, 18, TextAnchor.MiddleCenter, new Color(0.5f, 1f, 0.6f), false);
            UIKit.Place(callHint.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f), new Vector2(0, 120), new Vector2(W, 40));
            callLayer.gameObject.SetActive(false);

            GNLayers.SetLayerRecursive(go, GNLayers.FP);
        }

        T Mk<T>(string n, Transform parent) where T : Graphic
        {
            var go = new GameObject(n, typeof(RectTransform));
            go.layer = GNLayers.FP;
            go.transform.SetParent(parent, false);
            var g = go.AddComponent<T>(); g.raycastTarget = false;
            return g;
        }

        Text MkText(Transform parent, int size, TextAnchor a, Color c)
        {
            var t = Mk<Text>("T", parent);
            t.font = UIKit.Font; t.fontSize = size; t.alignment = a; t.color = c; t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        // ================================================================== API
        public void Toggle()
        {
            var ps = PhoneSystem.I;
            if (state == 2) { Close(); return; }
            state = 2; glanceT = 0f;
            phoneRoot.gameObject.SetActive(true);
            if (AudioManager.I) AudioManager.I.UI(AudioManager.I.pocket, 0.6f);
            if (ps != null && ps.Ringing) ps.Answer();
            dirty = true;
            GameEvents.Raise("phone_opened");
        }

        public void Close()
        {
            if (state == 0) return;
            state = 0;
            if (AudioManager.I) AudioManager.I.UI(AudioManager.I.pocket, 0.5f);
            GameEvents.Raise("phone_closed");
        }

        /// <summary>Nuevo mensaje: vibra, muestra notificación y opcionalmente "levanta" el teléfono un momento.</summary>
        public void Notify(PhoneMessage m, bool autoShow)
        {
            notifText.text = "<b>" + PhoneSystem.Name(m.thread) + "</b>  <color=#9aa>" + m.time + "</color>\n" + Shorten(m.text, 80);
            notifT = 4.5f;
            if (state == 2) { if (openThread == m.thread) PhoneSystem.I.MarkRead(m.thread); dirty = true; return; }
            if (autoShow && !PlayerController.CutsceneLock && !FocusSystem.Active && !InspectSystem.Active)
            {
                app = 0; openThread = m.thread; state = 1; glanceT = 4.5f;
                phoneRoot.gameObject.SetActive(true);
                if (AudioManager.I) AudioManager.I.UI(AudioManager.I.pocket, 0.45f);
                dirty = true;
            }
            else if (HUDView.I) HUDView.I.Toast(PhoneSystem.Name(m.thread) + " · " + m.time, Shorten(m.text, 120));
        }

        public void ShowCall()
        {
            phoneRoot.gameObject.SetActive(true);
            if (state == 0) { state = 1; glanceT = 999f; }
            dirty = true;
        }

        public void OpenThread(string thread) { app = 0; openThread = thread; dirty = true; }

        static string Shorten(string s, int n)
        {
            s = s.Replace("\n", " ");
            return s.Length <= n ? s : s.Substring(0, n - 1) + "…";
        }

        // ================================================================== bucle
        void Update()
        {
            var ps = PhoneSystem.I;
            bool canUse = GameManager.I != null && GameManager.I.Playing && !UIManager.ModalOpen && !InspectSystem.Active && !FocusSystem.Active && !PlayerController.CutsceneLock;
            if (canUse && Inp.Phone()) Toggle();
            if (state == 2 && !canUse && !(ps != null && (ps.InCall || ps.Ringing))) Close();
            if (state == 1 && glanceT < 900f)
            {
                glanceT -= Time.deltaTime;
                if (glanceT <= 0f) state = 0;
            }
            if (state == 1 && ps != null && !ps.Ringing && !ps.InCall && glanceT > 900f) state = 0;
            if (ps != null && ps.Ringing && canUse && Inp.Interact()) { if (state != 2) Toggle(); else ps.Answer(); }

            float target = state == 0 ? 0f : 1f;
            show = Mathf.MoveTowards(show, target, Time.deltaTime * (state == 0 ? 3.2f : 2.6f));
            if (show <= 0.001f && state == 0) { if (phoneRoot.gameObject.activeSelf) phoneRoot.gameObject.SetActive(false); return; }

            float k = state == 0 ? Ease.InOut(show) : Ease.OutBack(show);
            Vector3 up = state == 1 ? glancePos : shownPos;
            float t = Time.time;
            var pc = PlayerController.I;
            float bob = pc != null && pc.IsMoving ? Mathf.Sin(t * 7.5f) * 0.006f : Mathf.Sin(t * 1.5f) * 0.002f;
            phoneRoot.localPosition = Vector3.LerpUnclamped(hiddenPos, up + new Vector3(0, bob, 0), k);
            phoneRoot.localRotation = Quaternion.SlerpUnclamped(Quaternion.Euler(hiddenRot), Quaternion.Euler(shownRot + new Vector3(Mathf.Sin(t * 0.9f) * 0.6f, 0, 0)), k);

            if (state == 2 && ps != null && !ps.InCall && !ps.Ringing) HandleInput();
            if (notifT > 0f) notifT -= Time.deltaTime;
            notif.anchoredPosition = Vector2.Lerp(notif.anchoredPosition, new Vector2(0, notifT > 0f ? -36 : 90), Time.deltaTime * 10f);
            clock.text = GameClock.Text;
            battery = Mathf.Max(0.12f, battery - Time.deltaTime * 0.00004f);
            statusRight.text = "4G  ▂▄▆  " + Mathf.RoundToInt(battery * 100) + "%";
            UpdateCall(ps);
            if (dirty) { dirty = false; Rebuild(); }
            scroll = Mathf.Lerp(scroll, targetScroll, Time.deltaTime * 12f);
            content.anchoredPosition = new Vector2(0, scroll);
        }

        void HandleInput()
        {
            int d = Inp.Digit();
            if (d >= 1 && d <= 4) { app = d - 1; openThread = null; sel = 0; Tap(); dirty = true; }
            float wheel = Inp.Wheel();
            int count = ListCount();
            bool list = app == 0 && openThread == null || app == 3;
            if (Mathf.Abs(wheel) > 0.01f)
            {
                if (list && count > 0) { sel = Mathf.Clamp(sel + (wheel < 0 ? 1 : -1), 0, count - 1); Tap(); dirty = true; }
                else targetScroll = Mathf.Clamp(targetScroll - wheel * 60f, 0, Mathf.Max(0, content.sizeDelta.y - contentArea.rect.height));
            }
            if (Inp.Interact() && list && count > 0)
            {
                Tap();
                if (app == 0) { openThread = Threads[sel]; PhoneSystem.I.MarkRead(openThread); dirty = true; }
                else
                {
                    var it = ItemAt(sel);
                    if (it != null) { Close(); InspectSystem.I.InspectStored(it); }
                }
            }
            if ((Inp.Back() || Inp.Q()) && openThread != null) { openThread = null; Tap(); dirty = true; }
        }

        void Tap() { if (AudioManager.I) AudioManager.I.UI(AudioManager.I.phoneTap, 0.5f); }

        void UpdateCall(PhoneSystem ps)
        {
            bool call = ps != null && (ps.Ringing || ps.InCall);
            if (callLayer.gameObject.activeSelf != call) callLayer.gameObject.SetActive(call);
            if (!call) return;
            callName.text = PhoneSystem.Name(ps.CallThread);
            if (ps.Ringing)
            {
                callStatus.text = Settings.T("Llamada entrante…", "Incoming call…");
                callHint.text = Mathf.FloorToInt(Time.time * 2) % 2 == 0 ? Settings.T("[E] Contestar", "[E] Answer") : "";
            }
            else
            {
                int s = Mathf.FloorToInt(ps.CallTime);
                callStatus.text = (s / 60).ToString("00") + ":" + (s % 60).ToString("00");
                callHint.text = "";
            }
        }

        // ================================================================== contenido
        int ListCount()
        {
            if (app == 0) return Threads.Length;
            if (app == 3) return Inventory.Items.Count + (JournalManager.I != null ? JournalManager.I.Documents.Count : 0);
            return 0;
        }

        Inspectable ItemAt(int i)
        {
            if (i < Inventory.Items.Count) return Inventory.Items[i].source;
            i -= Inventory.Items.Count;
            var j = JournalManager.I;
            return j != null && i < j.Documents.Count ? j.Documents[i] : null;
        }

        void Clear()
        {
            foreach (var r in rows) if (r) Destroy(r.gameObject);
            rows.Clear();
        }

        RectTransform Row(float y, float h, Color c)
        {
            var img = Mk<Image>("Row", content);
            img.color = c;
            var rt = img.rectTransform;
            rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1); rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -y); rt.sizeDelta = new Vector2(0, h);
            rows.Add(rt);
            return rt;
        }

        void Rebuild()
        {
            Clear();
            for (int i = 0; i < 4; i++)
            {
                tabLabel[i].text = (i + 1) + "\n" + Settings.T(Apps[i], AppsEn[i]);
                bool on = i == app;
                tabBg[i].color = on ? new Color(0.22f, 0.36f, 0.7f, 0.9f) : new Color(0, 0, 0, 0);
                tabLabel[i].color = on ? Color.white : new Color(0.65f, 0.68f, 0.75f);
            }
            if (PhoneSystem.I != null && PhoneSystem.I.UnreadCount > 0 && app != 0) tabLabel[0].text += " ●";
            back.text = "";
            float y = 8f;
            switch (app)
            {
                case 0:
                    if (openThread == null) y = BuildThreadList(y);
                    else y = BuildThread(openThread, y);
                    break;
                case 1: y = BuildObjectives(y); break;
                case 2: y = BuildClues(y); break;
                case 3: y = BuildItems(y); break;
            }
            content.sizeDelta = new Vector2(0, y + 10);
            float max = Mathf.Max(0, y + 10 - contentArea.rect.height);
            if (app == 0 && openThread != null) targetScroll = max;  // conversación: abajo del todo
            else targetScroll = Mathf.Clamp(targetScroll, 0, max);
            if (app == 0 && openThread == null || app == 3) targetScroll = Mathf.Clamp(sel * 76f - 200f, 0, max);
        }

        float BuildThreadList(float y)
        {
            title.text = Settings.T("Mensajes", "Messages");
            var ps = PhoneSystem.I;
            for (int i = 0; i < Threads.Length; i++)
            {
                string th = Threads[i];
                var r = Row(y, 72, i == sel ? new Color(0.2f, 0.28f, 0.45f, 0.75f) : new Color(1, 1, 1, 0.04f));
                var av = Mk<Image>("Av", r); av.color = th == PhoneSystem.Mama ? new Color(0.8f, 0.4f, 0.5f) : (th == PhoneSystem.Unknown ? new Color(0.35f, 0.35f, 0.38f) : new Color(0.2f, 0.6f, 0.3f));
                UIKit.Place(av.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(14, 0), new Vector2(46, 46));
                var al = MkText(av.rectTransform, 20, TextAnchor.MiddleCenter, Color.white); UIKit.Stretch(al.rectTransform);
                al.text = th == PhoneSystem.Mama ? "M" : (th == PhoneSystem.Unknown ? "?" : "G");
                var n = MkText(r, 18, TextAnchor.UpperLeft, Color.white);
                UIKit.Place(n.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(72, -10), new Vector2(200, 26));
                n.text = "<b>" + PhoneSystem.Name(th) + "</b>";
                var last = ps != null ? ps.Last(th) : null;
                var p = MkText(r, 14, TextAnchor.UpperLeft, new Color(0.7f, 0.72f, 0.78f));
                UIKit.Place(p.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(72, -38), new Vector2(250, 24));
                p.text = last == null ? "—" : Shorten((last.outgoing ? Settings.T("Tú: ", "You: ") : "") + StripTags(last.text), 34);
                var tm = MkText(r, 13, TextAnchor.UpperRight, new Color(0.6f, 0.62f, 0.68f));
                UIKit.Place(tm.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-12, -12), new Vector2(80, 20));
                tm.text = last != null ? last.time : "";
                if (ps != null && ps.HasNew(th))
                {
                    var dot = Mk<Image>("Dot", r); dot.color = new Color(0.3f, 0.6f, 1f);
                    UIKit.Place(dot.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-16, -10), new Vector2(12, 12));
                }
                y += 76;
            }
            Hint(y + 10, Settings.T("Rueda: elegir · E: abrir", "Wheel: select · E: open"));
            return y + 40;
        }

        float BuildThread(string th, float y)
        {
            title.text = PhoneSystem.Name(th);
            back.text = Settings.T("‹ [Q]", "‹ [Q]");
            var list = PhoneSystem.I.Thread(th);
            if (list.Count == 0) { Hint(y + 20, Settings.T("Sin mensajes.", "No messages.")); return y + 60; }
            foreach (var m in list)
            {
                if (m.isCall)
                {
                    var r = Row(y, 30, new Color(0, 0, 0, 0));
                    var t = MkText(r, 13, TextAnchor.MiddleCenter, new Color(0.6f, 0.65f, 0.7f)); UIKit.Stretch(t.rectTransform);
                    t.text = m.text + "  ·  " + m.time;
                    y += 34; continue;
                }
                var holder = Row(y, 10, new Color(0, 0, 0, 0));
                var bubble = Mk<Image>("Bubble", holder);
                bubble.color = m.outgoing ? new Color(0.18f, 0.42f, 0.9f) : new Color(0.2f, 0.22f, 0.27f);
                var txt = MkText(bubble.rectTransform, 16, TextAnchor.UpperLeft, Color.white);
                txt.text = m.text;
                float maxW = 250f;
                var tr = txt.rectTransform;
                tr.anchorMin = tr.anchorMax = new Vector2(0, 1); tr.pivot = new Vector2(0, 1);
                tr.sizeDelta = new Vector2(maxW - 24, 10);
                float pw = Mathf.Min(maxW - 24, txt.preferredWidth + 2);
                tr.sizeDelta = new Vector2(pw, 10);
                float ph = txt.preferredHeight;
                tr.sizeDelta = new Vector2(pw, ph);
                tr.anchoredPosition = new Vector2(12, -9);
                var br = bubble.rectTransform;
                br.anchorMin = br.anchorMax = new Vector2(m.outgoing ? 1 : 0, 1); br.pivot = new Vector2(m.outgoing ? 1 : 0, 1);
                br.sizeDelta = new Vector2(pw + 24, ph + 18);
                br.anchoredPosition = new Vector2(m.outgoing ? -12 : 12, 0);
                var tm = MkText(holder, 11, m.outgoing ? TextAnchor.UpperRight : TextAnchor.UpperLeft, new Color(0.6f, 0.62f, 0.68f));
                var tmr = tm.rectTransform; tmr.anchorMin = tmr.anchorMax = new Vector2(m.outgoing ? 1 : 0, 1); tmr.pivot = new Vector2(m.outgoing ? 1 : 0, 1);
                tmr.sizeDelta = new Vector2(150, 16); tmr.anchoredPosition = new Vector2(m.outgoing ? -16 : 16, -(ph + 20));
                tm.text = m.time + (m.outgoing ? "  ✓✓" : "");
                holder.sizeDelta = new Vector2(0, ph + 38);
                y += ph + 42;
            }
            PhoneSystem.I.MarkRead(th);
            return y;
        }

        float BuildObjectives(float y)
        {
            title.text = Settings.T("Objetivos", "Objectives");
            var o = ObjectiveManager.I;
            var r = Row(y, 10, new Color(0, 0, 0, 0));
            var t = MkText(r, 17, TextAnchor.UpperLeft, Color.white);
            UIKit.Stretch(t.rectTransform, 18, 0, 18, 0);
            if (o == null || string.IsNullOrEmpty(o.Title)) t.text = "<color=#888>—</color>";
            else
            {
                string s = "<color=#f2c66d><b>" + o.Title + "</b></color>\n\n" + o.ItemsText();
                if (!string.IsNullOrEmpty(o.Hint))
                    s += o.HintUnlocked ? "\n<color=#9ab>" + Settings.T("Pista: ", "Hint: ") + o.Hint + "</color>"
                                        : "\n<color=#667>" + Settings.T("(Si te atascas, aquí aparecerá una pista.)", "(If you get stuck, a hint will appear here.)") + "</color>";
                t.text = s;
            }
            t.rectTransform.sizeDelta = new Vector2(-36, 10);
            float h = t.preferredHeight;
            r.sizeDelta = new Vector2(0, h + 10);
            return y + h + 20;
        }

        float BuildClues(float y)
        {
            title.text = Settings.T("Pistas", "Clues");
            var j = JournalManager.I;
            if (j == null || j.Clues.Count == 0) { Hint(y + 20, Settings.T("Todavía no anotaste pistas.", "No clues yet.")); return y + 60; }
            for (int i = 0; i < j.Clues.Count; i++)
            {
                var r = Row(y, 10, new Color(1, 1, 1, 0.04f));
                var t = MkText(r, 15, TextAnchor.UpperLeft, Color.white);
                UIKit.Stretch(t.rectTransform, 14, 6, 14, 6);
                t.text = j.Clues[i];
                t.rectTransform.sizeDelta = new Vector2(-28, 10);
                float h = t.preferredHeight + 14;
                r.sizeDelta = new Vector2(0, h);
                UIKit.Stretch(t.rectTransform, 14, 6, 14, 6);
                y += h + 6;
            }
            return y;
        }

        float BuildItems(float y)
        {
            title.text = Settings.T("Objetos y documentos", "Items & documents");
            int n = ListCount();
            if (n == 0) { Hint(y + 20, Settings.T("No llevas nada.", "You carry nothing.")); return y + 60; }
            for (int i = 0; i < n; i++)
            {
                var it = ItemAt(i);
                bool inv = i < Inventory.Items.Count;
                var r = Row(y, 56, i == sel ? new Color(0.2f, 0.28f, 0.45f, 0.75f) : new Color(1, 1, 1, 0.04f));
                var t = MkText(r, 16, TextAnchor.MiddleLeft, Color.white);
                UIKit.Stretch(t.rectTransform, 16, 0, 16, 0);
                t.text = (inv ? "▣  " + Inventory.Items[i].Name : "▤  " + (it != null ? it.Title : "?"));
                y += 60;
            }
            Hint(y + 10, Settings.T("Rueda: elegir · E: examinar", "Wheel: select · E: examine"));
            return y + 40;
        }

        void Hint(float y, string s)
        {
            var r = Row(y, 26, new Color(0, 0, 0, 0));
            var t = MkText(r, 13, TextAnchor.MiddleCenter, new Color(0.55f, 0.58f, 0.65f)); UIKit.Stretch(t.rectTransform);
            t.text = s;
        }

        static string StripTags(string s) => System.Text.RegularExpressions.Regex.Replace(s, "<.*?>", "");
    }
}
