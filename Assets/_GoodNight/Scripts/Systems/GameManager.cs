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
        public void Die(string quote) { Die(quote, null); }

        /// <summary>Muerte con origen del ataque (p. ej. la puerta principal): la entidad se abalanza desde ahí.</summary>
        public void Die(string quote, Vector3? lungeFrom)
        {
            if (DebugAutoPlay.Active) { Debug.Log("[AUTOPLAY] (muerte ignorada) " + quote); return; }
            if (dead) return;
            dead = true;
            StartCoroutine(DieCo(quote, lungeFrom));
        }

        IEnumerator DieCo(string quote, Vector3? lungeFrom)
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
            var am = AudioManager.I;
            Vector3 start = Vector3.zero, end = Vector3.zero;
            if (e != null)
            {
                var cam = r.player.cam;
                Vector3 f = cam.forward; f.y = 0; if (f.sqrMagnitude < 0.01f) f = r.player.transform.forward; f.Normalize();
                float py = r.player.transform.position.y;
                if (lungeFrom.HasValue) { Vector3 d = lungeFrom.Value - cam.position; d.y = 0; if (d.sqrMagnitude > 0.04f) f = d.normalized; }
                start = lungeFrom ?? cam.position + f * 1.9f; start.y = py;
                end = cam.position + f * 0.5f; end.y = py;
                e.ShowAt(start, Quaternion.LookRotation(-f).eulerAngles.y, "Attack", false);
                e.LookAtPlayer(1f);
            }
            // un instante de silencio antes del golpe
            if (am) { am.SetAmbience(0.05f); am.UI(am.breathClose, 0.7f); }
            for (float t = 0; t < 0.22f; t += Time.deltaTime)
            {
                if (e != null) r.player.LookAtPoint(e.head != null ? e.head.position : e.transform.position + Vector3.up * 1.6f, Time.deltaTime * 14f);
                yield return null;
            }
            // se abalanza: sonido sincronizado (capas a volumen moderado, sin saturar), sacudida, destello rojo
            if (am)
            {
                am.UI(am.scareHit2 != null ? am.scareHit2 : (am.scareHit != null ? am.scareHit : am.sting), 0.85f);
                am.UI(am.entityScream != null ? am.entityScream : am.sting, 0.7f);
                am.UI(am.hitLow, 0.6f);
            }
            CameraFX.Shake(0.9f); CameraFX.FovKick(-14f);
            if (PostFX.I) PostFX.I.Shock(1f);
            if (HUDView.I) { HUDView.I.Flash(new Color(0.55f, 0.02f, 0.02f, 0.45f), 0.25f); HUDView.I.Say(Settings.T("Tú", "You"), Settings.T("¡No—!", "No—!"), 1.2f); }
            for (float t = 0; t < 0.75f; t += Time.deltaTime)
            {
                if (e != null)
                {
                    e.transform.position = Vector3.Lerp(start, end, Ease.Out(t / 0.32f));
                    r.player.LookAtPoint(e.head != null ? e.head.position : e.transform.position + Vector3.up * 1.6f, Time.deltaTime * 16f);
                }
                yield return null;
            }
            if (am) am.SetAmbience(1f);
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
