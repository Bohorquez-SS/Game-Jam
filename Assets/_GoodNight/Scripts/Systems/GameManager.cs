using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>Arranque (menú o punto de control), muerte con jumpscare 3D, pausa y salida.</summary>
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
            Inventory.Clear();
            IsolationStation.Motion.Clear();
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
            if (AudioManager.I) AudioManager.I.SetMusic(0.3f);
        }

        public void StartGame(Stage s)
        {
            if (s == Stage.None) s = Stage.C1_Noche;
            Playing = true; dead = false;
            Checkpoint.Save(s);
            if (AudioManager.I) AudioManager.I.SetMusic(0f);
            StoryManager.I.Begin(s);
        }

        /// <summary>Muerte: la entidad aparece frente a la cámara, se abalanza, grito, sacudida, negro y pantalla de muerte.</summary>
        public void Die(string quote)
        {
            if (DebugAutoPlay.Active) { Debug.Log("[AUTOPLAY] (muerte ignorada) " + quote); return; }
            if (dead) return;
            dead = true;
            StartCoroutine(DieCo(quote));
        }

        IEnumerator DieCo(string quote)
        {
            PlayerController.CutsceneLock = true;
            if (Phone3D.I) Phone3D.I.Close();
            if (FocusSystem.Active) FocusSystem.I.Exit();
            if (InspectSystem.Active) InspectSystem.I.End();
            UIManager.I.CloseAll();
            var r = GameRefs.I;
            HidingSpot.ForceExit();
            yield return null;
            var e = r != null ? r.enemy : null;
            if (e != null)
            {
                var cam = r.player.cam;
                Vector3 f = cam.forward; f.y = 0; if (f.sqrMagnitude < 0.01f) f = r.player.transform.forward; f.Normalize();
                Vector3 p = cam.position + f * 0.9f; p.y = r.player.transform.position.y;
                e.ShowAt(p, Quaternion.LookRotation(-f).eulerAngles.y, "Attack", false);
                e.LookAtPlayer(1f);
            }
            if (AudioManager.I) { AudioManager.I.UI(AudioManager.I.entityScream != null ? AudioManager.I.entityScream : AudioManager.I.sting, 1f); AudioManager.I.UI(AudioManager.I.hitLow, 0.9f); }
            CameraFX.Shake(0.9f); CameraFX.FovKick(-12f);
            if (PostFX.I) PostFX.I.Shock(1f);
            for (float t = 0; t < 0.7f; t += Time.deltaTime)
            {
                if (e != null) r.player.LookAtPoint(e.head != null ? e.head.position : e.transform.position + Vector3.up * 1.7f, Time.deltaTime * 12f);
                yield return null;
            }
            HUDView.I.SetFade(1f);
            if (e != null) e.Despawn();
            if (AudioManager.I) AudioManager.I.SetTension(0f);
            yield return new WaitForSeconds(0.6f);
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
