using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>Final: la puerta se abre al amanecer, una última voz, GOOD NIGHT, créditos y menú.</summary>
    public class EndingManager : MonoBehaviour
    {
        public static EndingManager I;
        bool playing;
        void Awake() { I = this; }

        public IEnumerator Play()
        {
            if (playing) yield break;
            playing = true;
            var r = GameRefs.I; var h = HUDView.I; var a = AudioManager.I;
            PlayerController.CutsceneLock = true;
            PlayerController.CutsceneLockLook = true;
            ObjectiveManager.I.Clear();
            r.doorFront.BeforeOpen = null;
            r.doorFront.locked = false;
            r.doorFront.speed = 0.9f;
            r.doorFront.SetOpen(true);
            LightingManager.I.SetMood(Mood.Dawn);
            a.SetAmbience(0.25f);
            a.SetMusic(0f);
            // caminar hacia la luz
            Vector3 from = r.poseExitStart.position, to = r.poseExitEnd.position;
            r.player.SetPose(r.poseExitStart, -1f);
            for (float t = 0; t < 1f; t += Time.deltaTime / 6f)
            {
                r.player.transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0, 1, t));
                r.player.LookAtPoint(to + (to - from).normalized * 10f + Vector3.up * 1.4f, Time.deltaTime * 2f);
                yield return null;
            }
            yield return new WaitForSeconds(1.5f);
            yield return h.FadeTo(1f, 5f);
            yield return new WaitForSeconds(1.2f);
            a.PlayVoice(Voice.You, r.player.transform.position - r.player.transform.forward * 1.5f, 0.6f);
            h.Say("", Settings.T("<i>¿Seguro que encerraste al correcto?</i>", "<i>Are you sure you locked up the right one?</i>"), 4f);
            yield return new WaitForSeconds(5f);
            yield return h.ShowTitle("GOOD NIGHT", 3f);
            Checkpoint.ClearSave();
            UIManager.I.Get<CreditsPanel>().Roll();
        }
    }
}
