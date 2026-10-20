using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GNight
{
    /// <summary>
    /// HUD cinematográfico: imagen del mundo (retro) + capa en primera persona (nítida), viñeta suave, grano VHS,
    /// prompt contextual, objetivo temporal, avisos, subtítulos, títulos, panel de inspección y fundidos.
    /// </summary>
    public class HUDView : MonoBehaviour
    {
        public static HUDView I;
        public Texture2D vignetteTex, grainTex, scanTex;

        RawImage world, fp, grain, scan, vignette;
        Image fade, crosshair, toastBg, bannerBg, hintBg, inspectBg, flash;
        Text prompt, subtitle, title, objTitle, objItems, toastText, bannerText, hintText, focusHint, inspectTitle, inspectBody, inspectHint, chapter;
        CanvasGroup objGroup, inspectGroup;
        RectTransform toastRT;
        Coroutine subCo, toastCo, bannerCo, hintCo, objCo, flashCo;
        float grainT, objAlpha;

        public void Build(UIManager ui)
        {
            I = this;
            world = UIKit.Raw("WorldView", ui.World); UIKit.Stretch(world.rectTransform); world.enabled = false;
            fp = UIKit.Raw("FirstPerson", ui.World); UIKit.Stretch(fp.rectTransform); fp.enabled = false;

            vignette = UIKit.Raw("Vignette", ui.FX); UIKit.Stretch(vignette.rectTransform); vignette.texture = vignetteTex; vignette.color = new Color(1, 1, 1, 0.35f);
            if (vignetteTex == null) vignette.enabled = false;
            scan = UIKit.Raw("Scanlines", ui.FX); UIKit.Stretch(scan.rectTransform); scan.texture = scanTex; scan.color = new Color(1, 1, 1, 0.05f);
            if (scanTex == null) scan.enabled = false;
            grain = UIKit.Raw("Grain", ui.FX); UIKit.Stretch(grain.rectTransform); grain.texture = grainTex; grain.color = new Color(1, 1, 1, 0.035f);
            if (grainTex == null) grain.enabled = false;
            flash = UIKit.Img("Flash", ui.FX, new Color(1, 1, 1, 0)); UIKit.Stretch(flash.rectTransform);

            crosshair = UIKit.Img("Crosshair", ui.HUD, new Color(1, 1, 1, 0.55f));
            UIKit.Center(crosshair.rectTransform, Vector2.zero, new Vector2(4, 4));

            prompt = UIKit.Txt("Prompt", ui.HUD, 26, TextAnchor.MiddleCenter, new Color(0.95f, 0.93f, 0.88f));
            UIKit.Center(prompt.rectTransform, new Vector2(0, -70), new Vector2(1100, 44));

            // objetivo: aparece unos segundos al cambiar (siempre consultable en el teléfono)
            var ob = UIKit.Rect("Objective", ui.HUD);
            UIKit.Place(ob, new Vector2(0, 1), new Vector2(0, 1), new Vector2(46, -40), new Vector2(700, 180));
            objGroup = ob.gameObject.AddComponent<CanvasGroup>(); objGroup.alpha = 0f;
            objTitle = UIKit.Txt("Title", ob, 22, TextAnchor.UpperLeft, UIKit.Accent);
            UIKit.Place(objTitle.rectTransform, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(700, 34));
            objItems = UIKit.Txt("Items", ob, 20, TextAnchor.UpperLeft, new Color(0.9f, 0.9f, 0.9f));
            UIKit.Place(objItems.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(2, -36), new Vector2(700, 140));

            chapter = UIKit.Txt("Chapter", ui.HUD, 18, TextAnchor.UpperRight, new Color(1, 1, 1, 0.35f));
            UIKit.Place(chapter.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-40, -30), new Vector2(500, 30));

            // notificación de respaldo (si el teléfono no está disponible)
            toastBg = UIKit.Img("Toast", ui.HUD, new Color(0.06f, 0.065f, 0.085f, 0.92f));
            toastRT = toastBg.rectTransform;
            UIKit.Place(toastRT, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-40, -70), new Vector2(520, 130));
            toastText = UIKit.Txt("Text", toastRT, 20, TextAnchor.UpperLeft, Color.white);
            UIKit.Stretch(toastText.rectTransform, 16, 10, 16, 10);
            toastBg.gameObject.SetActive(false);

            bannerBg = UIKit.Img("Guardian", ui.HUD, new Color(0f, 0.03f, 0.01f, 0.88f));
            UIKit.Place(bannerBg.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -110), new Vector2(1000, 170));
            bannerText = UIKit.Txt("Text", bannerBg.rectTransform, 28, TextAnchor.MiddleCenter, UIKit.Green);
            UIKit.Stretch(bannerText.rectTransform, 16, 10, 16, 10);
            bannerBg.gameObject.SetActive(false);

            hintBg = UIKit.Img("Hint", ui.HUD, new Color(0, 0, 0, 0.5f));
            UIKit.Place(hintBg.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 230), new Vector2(820, 50));
            hintText = UIKit.Txt("Text", hintBg.rectTransform, 22, TextAnchor.MiddleCenter, new Color(0.92f, 0.92f, 0.92f));
            UIKit.Stretch(hintText.rectTransform);
            hintBg.gameObject.SetActive(false);

            focusHint = UIKit.Txt("FocusHint", ui.HUD, 19, TextAnchor.LowerCenter, new Color(1, 1, 1, 0.6f));
            UIKit.Place(focusHint.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 26), new Vector2(1400, 30));

            // inspección
            var ins = UIKit.Rect("Inspect", ui.HUD); UIKit.Stretch(ins);
            inspectGroup = ins.gameObject.AddComponent<CanvasGroup>(); inspectGroup.alpha = 0f;
            inspectBg = UIKit.Img("Panel", ins, new Color(0.02f, 0.02f, 0.03f, 0.78f));
            UIKit.Place(inspectBg.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-50, 0), new Vector2(560, 720));
            inspectTitle = UIKit.Txt("Title", inspectBg.rectTransform, 28, TextAnchor.UpperLeft, UIKit.Accent);
            UIKit.Place(inspectTitle.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(28, -24), new Vector2(510, 40));
            inspectBody = UIKit.Txt("Body", inspectBg.rectTransform, 22, TextAnchor.UpperLeft, new Color(0.92f, 0.9f, 0.86f));
            UIKit.Stretch(inspectBody.rectTransform, 28, 30, 24, 80);
            inspectHint = UIKit.Txt("Hint", ins, 19, TextAnchor.LowerCenter, new Color(1, 1, 1, 0.6f));
            UIKit.Place(inspectHint.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 26), new Vector2(1400, 30));

            fade = UIKit.Img("Fade", ui.Fade, Color.black); UIKit.Stretch(fade.rectTransform); fade.color = new Color(0, 0, 0, 0);

            subtitle = UIKit.Txt("Subtitle", ui.Overlay, 30, TextAnchor.LowerCenter, Color.white);
            UIKit.Place(subtitle.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 110), new Vector2(1500, 200));
            title = UIKit.Txt("Title", ui.Overlay, 64, TextAnchor.MiddleCenter, new Color(0.92f, 0.9f, 0.84f, 0f));
            UIKit.Center(title.rectTransform, Vector2.zero, new Vector2(1700, 420));
        }

        void Update()
        {
            if (crosshair == null) return;
            bool playing = GameManager.I != null && GameManager.I.Playing;
            bool free = playing && !UIManager.ModalOpen && !PlayerController.CutsceneLock && HidingSpot.Current == null && !InspectSystem.Active && !FocusSystem.Active;
            crosshair.enabled = free && !Phone3D.IsOpen;
            prompt.enabled = playing && !UIManager.ModalOpen;
            objAlpha = Mathf.MoveTowards(objAlpha, objCo != null && playing && !InspectSystem.Active ? 1f : 0f, Time.unscaledDeltaTime * 2f);
            objGroup.alpha = objAlpha;
            inspectGroup.alpha = Mathf.MoveTowards(inspectGroup.alpha, InspectSystem.Active ? 1f : 0f, Time.unscaledDeltaTime * 6f);
            chapter.text = playing && StoryManager.I != null ? StoryManager.I.ChapterLabel : "";
            int vhs = Settings.Vhs;
            if (grain) { grain.enabled = grainTex != null && vhs > 0; grain.color = new Color(1, 1, 1, vhs == 2 ? 0.07f : 0.035f); }
            if (scan) { scan.enabled = scanTex != null && vhs > 0; scan.color = new Color(1, 1, 1, vhs == 2 ? 0.09f : 0.045f); }
            if (grain != null && grain.enabled)
            {
                grainT += Time.unscaledDeltaTime;
                if (grainT > 0.05f) { grainT = 0; grain.uvRect = new Rect(Random.value, Random.value, 3f, 2f); }
            }
        }

        public void SetWorld(Texture t) { if (world == null) return; world.texture = t; world.enabled = t != null; }
        public void SetFP(Texture t) { if (fp == null) return; fp.texture = t; fp.enabled = t != null; }

        public void SetPrompt(string s, bool withKey = true)
        {
            if (prompt == null) return;
            prompt.text = string.IsNullOrEmpty(s) ? "" : (withKey ? "[E]  " + s : s);
        }
        public void SetFocusHint(string s) { if (focusHint) focusHint.text = s ?? ""; }

        public void ShowInspect(string t, string body, string hint)
        {
            inspectTitle.text = t ?? ""; inspectBody.text = body ?? ""; inspectHint.text = hint ?? "";
            inspectBg.enabled = !string.IsNullOrEmpty(body) || !string.IsNullOrEmpty(t);
        }

        // ---------------- fundidos / títulos / destellos
        public void SetFade(float a) { fade.color = new Color(0, 0, 0, a); }
        public float FadeAlpha => fade.color.a;
        public IEnumerator FadeTo(float target, float duration)
        {
            float s = fade.color.a;
            for (float t = 0; t < duration; t += Time.deltaTime) { SetFade(Mathf.Lerp(s, target, t / duration)); yield return null; }
            SetFade(target);
        }
        /// <summary>Parpadeo de ojos (al despertar).</summary>
        public IEnumerator Blink(int times)
        {
            for (int i = 0; i < times; i++)
            {
                yield return FadeTo(0.2f, 0.35f);
                yield return FadeTo(1f, 0.25f);
                yield return new WaitForSeconds(0.3f);
            }
            yield return FadeTo(0f, 1.2f);
        }
        public void Flash(Color c, float duration)
        {
            if (flashCo != null) StopCoroutine(flashCo);
            flashCo = StartCoroutine(FlashCo(c, duration));
        }
        IEnumerator FlashCo(Color c, float d)
        {
            for (float t = 0; t < d; t += Time.unscaledDeltaTime) { flash.color = new Color(c.r, c.g, c.b, c.a * (1f - t / d)); yield return null; }
            flash.color = new Color(1, 1, 1, 0); flashCo = null;
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
        public void ShowObjective(string head, string items, bool announce)
        {
            objTitle.text = head ?? ""; objItems.text = items ?? "";
            if (string.IsNullOrEmpty(head)) { if (objCo != null) StopCoroutine(objCo); objCo = null; return; }
            if (!announce && objCo == null) return;
            if (objCo != null) StopCoroutine(objCo);
            objCo = StartCoroutine(ObjCo(announce));
        }
        IEnumerator ObjCo(bool announce)
        {
            if (announce && AudioManager.I) AudioManager.I.UI(AudioManager.I.check, 0.25f);
            yield return new WaitForSeconds(7f);
            objCo = null;
        }

        // ---------------- notificación de respaldo
        public void Toast(string header, string body)
        {
            if (toastCo != null) StopCoroutine(toastCo);
            toastCo = StartCoroutine(ToastCo(header, body));
        }
        IEnumerator ToastCo(string header, string body)
        {
            toastBg.gameObject.SetActive(true);
            toastText.text = "<size=16><color=#9fb4c8>" + header + "</color></size>\n" + body + "\n<size=15><color=#888888>" + Settings.T("[Tab] teléfono", "[Tab] phone") + "</color></size>";
            yield return new WaitForSeconds(5f);
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
            if (AudioManager.I) AudioManager.I.UI(AudioManager.I.beep, 0.7f);
            bannerBg.gameObject.SetActive(true);
            bannerText.text = "<size=18>GUARDIÁN</size>\n" + text;
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
