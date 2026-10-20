using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>Arranque (menú o punto de control), muerte, pausa y salida.</summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I;
        public bool Playing { get; private set; }
        bool dead;

        void Awake()
        {
            I = this;
            Settings.Load();
            Time.timeScale = 1f;
            AudioListener.pause = false;
            PlayerController.CutsceneLock = true;
            PlayerController.CutsceneLockLook = false;
        }

        IEnumerator Start()
        {
            yield return null;
            var s = Checkpoint.Pending;
            Checkpoint.Pending = Stage.None;
            if (s == Stage.None) ShowMenu();
            else StartGame(s);
        }

        void ShowMenu()
        {
            Playing = false;
            var r = GameRefs.I;
            PlayerController.CutsceneLock = true;
            if (r && r.poseMenu) r.player.SetPose(r.poseMenu, -1f);
            HUDView.I.SetFade(0f);
            GameClock.Set(23, 40);
            if (LightingManager.I) LightingManager.I.SetMood(Mood.Night);
            UIManager.I.Open(UIManager.I.Get<MenuPanel>());
            if (AudioManager.I) AudioManager.I.SetMusic(0.35f);
        }

        public void StartGame(Stage s)
        {
            if (s == Stage.None) s = Stage.Act1;
            Playing = true; dead = false;
            Checkpoint.Save(s);
            if (AudioManager.I) AudioManager.I.SetMusic(0.12f);
            StoryManager.I.Begin(s);
        }

        public void Die(string quote)
        {
            if (dead) return;
            dead = true;
            StartCoroutine(DieCo(quote));
        }

        IEnumerator DieCo(string quote)
        {
            PlayerController.CutsceneLock = true;
            UIManager.I.CloseAll();
            var r = GameRefs.I;
            HidingSpot.ForceExit();
            yield return null;
            if (r && r.enemy)
            {
                var cam = r.player.cam;
                Vector3 f = cam.forward; f.y = 0; if (f.sqrMagnitude < 0.01f) f = r.player.transform.forward; f.Normalize();
                Vector3 p = cam.position + f * 0.75f; p.y = r.player.transform.position.y;
                r.enemy.ShowAt(p, Quaternion.LookRotation(-f).eulerAngles.y);
            }
            if (AudioManager.I) AudioManager.I.UI(AudioManager.I.sting, 1f);
            yield return new WaitForSeconds(0.85f);
            HUDView.I.SetFade(1f);
            if (r && r.enemy) r.enemy.Despawn();
            UIManager.I.Get<DeathPanel>().Show(quote);
        }

        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
