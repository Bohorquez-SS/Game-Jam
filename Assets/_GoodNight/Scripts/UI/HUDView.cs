using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>HUD permanente: imagen del mundo, viñeta/grano, prompt, objetivo, avisos, subtítulos, títulos y fundidos.</summary>
    public class HUDView : MonoBehaviour
    {
        public static HUDView I;
        public Texture2D vignetteTex, grainTex;

        RawImage world, grain;
        Image fade, crosshair, toastBg, bannerBg, hintBg;
        Text prompt, subtitle, title, objTitle, objItems, toastText, bannerText, hintText, clockText;
        GameObject objPanel;
        RectTransform toastRT;
        Coroutine subCo, toastCo, bannerCo, hintCo;
        float grainT;

        public void Build(UIManager ui)
        {
            I = this;
            world = UIKit.Raw("WorldView", ui.World); UIKit.Stretch(world.rectTransform); world.enabled = false;

            var vig = UIKit.Raw("Vignette", ui.FX); UIKit.Stretch(vig.rectTransform); vig.texture = vignetteTex; vig.color = new Color(1, 1, 1, 0.85f);
            if (vignetteTex == null) vig.enabled = false;
            grain = UIKit.Raw("Grain", ui.FX); UIKit.Stretch(grain.rectTransform); grain.texture = grainTex; grain.color = new Color(1, 1, 1, 0.06f);
            if (grainTex == null) grain.enabled = false;

            crosshair = UIKit.Img("Crosshair", ui.HUD, new Color(1, 1, 1, 0.5f));
            UIKit.Center(crosshair.rectTransform, Vector2.zero, new Vector2(5, 5));

            prompt = UIKit.Txt("Prompt", ui.HUD, 32, TextAnchor.MiddleCenter, new Color(0.93f, 0.9f, 0.84f));
            UIKit.Center(prompt.rectTransform, new Vector2(0, -80), new Vector2(1000, 50));

            // Objetivo (arriba izquierda)
            var ob = UIKit.Img("Objective", ui.HUD, new Color(0, 0, 0, 0.38f));
            UIKit.Place(ob.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(36, -36), new Vector2(620, 170));
            objPanel = ob.gameObject;
            objTitle = UIKit.Txt("Title", ob.rectTransform, 25, TextAnchor.UpperLeft, UIKit.Accent);
            UIKit.Place(objTitle.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -12), new Vector2(580, 64));
            objItems = UIKit.Txt("Items", ob.rectTransform, 22, TextAnchor.UpperLeft, new Color(0.88f, 0.88f, 0.88f));
            UIKit.Place(objItems.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -76), new Vector2(580, 120));
            objPanel.SetActive(false);

            clockText = UIKit.Txt("Clock", ui.HUD, 22, TextAnchor.UpperRight, new Color(1, 1, 1, 0.45f));
            UIKit.Place(clockText.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-36, -30), new Vector2(300, 40));

            // Notificación del teléfono (arriba derecha)
            toastBg = UIKit.Img("Toast", ui.HUD, new Color(0.06f, 0.065f, 0.085f, 0.95f));
            toastRT = toastBg.rectTransform;
            UIKit.Place(toastRT, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-36, -80), new Vector2(560, 170));
            toastText = UIKit.Txt("Text", toastRT, 22, TextAnchor.UpperLeft, Color.white);
            UIKit.Stretch(toastText.rectTransform, 18, 12, 18, 12);
            toastBg.gameObject.SetActive(false);

            // Banner GUARDIÁN
            bannerBg = UIKit.Img("Guardian", ui.HUD, new Color(0f, 0.03f, 0f, 0.9f));
            UIKit.Place(bannerBg.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -120), new Vector2(980, 190));
            bannerText = UIKit.Txt("Text", bannerBg.rectTransform, 30, TextAnchor.MiddleCenter, UIKit.Green);
            UIKit.Stretch(bannerText.rectTransform, 16, 10, 16, 10);
            bannerBg.gameObject.SetActive(false);

            // Pista de controles
            hintBg = UIKit.Img("Hint", ui.HUD, new Color(0, 0, 0, 0.55f));
            UIKit.Place(hintBg.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 260), new Vector2(760, 56));
            hintText = UIKit.Txt("Text", hintBg.rectTransform, 24, TextAnchor.MiddleCenter, new Color(0.9f, 0.9f, 0.9f));
            UIKit.Stretch(hintText.rectTransform);
            hintBg.gameObject.SetActive(false);

            fade = UIKit.Img("Fade", ui.Fade, Color.black); UIKit.Stretch(fade.rectTransform); fade.color = new Color(0, 0, 0, 0);

            subtitle = UIKit.Txt("Subtitle", ui.Overlay, 34, TextAnchor.LowerCenter, Color.white);
            UIKit.Place(subtitle.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 120), new Vector2(1500, 200));
            title = UIKit.Txt("Title", ui.Overlay, 66, TextAnchor.MiddleCenter, new Color(0.92f, 0.9f, 0.84f, 0f));
            UIKit.Center(title.rectTransform, Vector2.zero, new Vector2(1700, 400));
        }

        void Update()
        {
            if (crosshair == null) return;
            bool playing = GameManager.I != null && GameManager.I.Playing;
            bool show = playing && !UIManager.ModalOpen && !PlayerController.CutsceneLock && HidingSpot.Current == null;
            crosshair.enabled = show;
            prompt.enabled = playing && !UIManager.ModalOpen;
            objPanel.transform.localScale = playing ? Vector3.one : Vector3.zero;
            clockText.text = playing ? GameClock.Text : "";
            if (grain != null && grain.enabled)
            {
                grainT += Time.unscaledDeltaTime;
                if (grainT > 0.05f) { grainT = 0; grain.uvRect = new Rect(Random.value, Random.value, 3f, 2f); }
            }
        }

        public void SetWorld(Texture t) { if (world == null) return; world.texture = t; world.enabled = t != null; }
        public void SetPrompt(string s) { if (prompt != null) prompt.text = string.IsNullOrEmpty(s) ? "" : "[E] " + s; }

        // ---------------- fundidos y títulos
        public void SetFade(float a) { fade.color = new Color(0, 0, 0, a); }
        public float FadeAlpha => fade.color.a;
        public IEnumerator FadeTo(float target, float duration)
        {
            float s = fade.color.a;
            for (float t = 0; t < duration; t += Time.deltaTime) { SetFade(Mathf.Lerp(s, target, t / duration)); yield return null; }
            SetFade(target);
        }

        public IEnumerator ShowTitle(string text, float hold)
        {
            title.text = text;
            for (float t = 0; t < 1f; t += Time.deltaTime) { TitleAlpha(t); yield return null; }
            TitleAlpha(1f);
            yield return new WaitForSeconds(hold);
            for (float t = 1f; t > 0f; t -= Time.deltaTime) { TitleAlpha(t); yield return null; }
            TitleAlpha(0f);
        }
        public void SetTitle(string text, float alpha) { title.text = text; TitleAlpha(alpha); }
        void TitleAlpha(float a) { var c = title.color; c.a = a; title.color = c; }

        // ---------------- subtítulos
        public void Say(string speaker, string text, float duration)
        {
            if (subCo != null) StopCoroutine(subCo);
            subCo = StartCoroutine(SayCo(speaker, text, duration));
        }
        IEnumerator SayCo(string speaker, string text, float duration)
        {
            subtitle.text = string.IsNullOrEmpty(speaker) ? text : "<color=#f2c66d>" + speaker + ":</color> " + text;
            yield return new WaitForSeconds(duration);
            subtitle.text = ""; subCo = null;
        }
        public void ClearSubtitle() { if (subCo != null) StopCoroutine(subCo); subCo = null; subtitle.text = ""; }

        // ---------------- objetivo
        public void ShowObjective(string head, string items)
        {
            objPanel.SetActive(!string.IsNullOrEmpty(head));
            objTitle.text = head;
            objItems.text = items;
        }

        // ---------------- notificación
        public void Toast(string header, string body)
        {
            if (toastCo != null) StopCoroutine(toastCo);
            toastCo = StartCoroutine(ToastCo(header, body));
        }
        IEnumerator ToastCo(string header, string body)
        {
            toastBg.gameObject.SetActive(true);
            toastText.text = "<size=18><color=#9fb4c8>" + header + "</color></size>\n" + body + "\n<size=17><color=#888888>" +
                             Settings.T("[Tab] abrir teléfono", "[Tab] open phone") + "</color></size>";
            Vector2 a = new Vector2(620, -80), b = new Vector2(-36, -80);
            for (float t = 0; t < 1f; t += Time.unscaledDeltaTime / 0.3f) { toastRT.anchoredPosition = Vector2.Lerp(a, b, Mathf.SmoothStep(0, 1, t)); yield return null; }
            toastRT.anchoredPosition = b;
            yield return new WaitForSeconds(7f);
            toastBg.gameObject.SetActive(false); toastCo = null;
        }

        // ---------------- GUARDIÁN
        public void Banner(string text, float duration)
        {
            if (bannerCo != null) StopCoroutine(bannerCo);
            bannerCo = StartCoroutine(BannerCo(text, duration));
        }
        IEnumerator BannerCo(string text, float duration)
        {
            if (AudioManager.I) AudioManager.I.UI(AudioManager.I.beep, 0.8f);
            bannerBg.gameObject.SetActive(true);
            bannerText.text = "<size=20>GUARDIÁN</size>\n" + text;
            yield return new WaitForSeconds(duration);
            bannerBg.gameObject.SetActive(false); bannerCo = null;
        }

        // ---------------- pista de controles
        public void Hint(string text, float duration)
        {
            if (hintCo != null) StopCoroutine(hintCo);
            hintCo = StartCoroutine(HintCo(text, duration));
        }
        IEnumerator HintCo(string text, float duration)
        {
            hintBg.gameObject.SetActive(true); hintText.text = text;
            yield return new WaitForSeconds(duration);
            hintBg.gameObject.SetActive(false); hintCo = null;
        }
    }
}
