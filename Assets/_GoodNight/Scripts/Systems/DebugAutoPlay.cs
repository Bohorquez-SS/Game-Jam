using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// PRUEBA AUTOMÁTICA (solo editor / build de desarrollo). F12 en una partida nueva: recorre los 5 capítulos disparando
    /// los mismos eventos que produce el jugador (x3 de velocidad, jugador invulnerable) y deja en la consola cada paso.
    /// Sirve para detectar errores de lógica sin jugar todo a mano. F11 = igual pero termina con el final A.
    /// </summary>
    public class DebugAutoPlay : MonoBehaviour
    {
        public static bool Active;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameRefs R => GameRefs.I;
        StoryManager S => StoryManager.I;
        bool endingA;

        void Update()
        {
            if (Active) return;
            if (Input.GetKeyDown(KeyCode.F12)) { endingA = false; StartCoroutine(Run()); }
            if (Input.GetKeyDown(KeyCode.F11)) { endingA = true; StartCoroutine(Run()); }
        }

        void OnDestroy() { Active = false; Time.timeScale = 1f; }

        static void Log(string s) { Debug.Log("<color=#7fd>[AUTOPLAY]</color> " + s); }

        IEnumerator WaitStage(Stage s, float timeout)
        {
            for (float t = 0; t < timeout; t += Time.unscaledDeltaTime) { if (S.stage == s) yield break; yield return null; }
            Debug.LogError("[AUTOPLAY] No se llegó a " + s + " (sigue en " + S.stage + ")");
        }

        IEnumerator W(float s) { yield return new WaitForSeconds(s); }

        void Collect(Inspectable it)
        {
            if (it == null) { Debug.LogError("[AUTOPLAY] Inspectable nulo"); return; }
            Inventory.Add(it.id, it.invNameEs, it.invNameEn, it);
            GameEvents.Raise("inspected:" + it.id);
            GameEvents.Raise("collected:" + it.id);
        }

        IEnumerator Focus(FocusStation st, float stay)
        {
            FocusSystem.I.Enter(st);
            yield return W(stay);
            if (FocusSystem.Active) FocusSystem.I.Exit();
            yield return W(1f);
        }

        IEnumerator Run()
        {
            Active = true;
            Time.timeScale = 3f;
            Log("Inicio · etapa " + S.stage);
            if (S.stage == Stage.None) { Debug.LogError("[AUTOPLAY] Empieza una partida primero (Nueva partida)."); Active = false; Time.timeScale = 1f; yield break; }

            // ---------------- CAPÍTULO 1
            if (S.stage == Stage.C1_Noche)
            {
                while (PlayerController.CutsceneLock) yield return null;
                yield return W(1f);
                R.computer.SetOn(false); Log("PC apagada");
                R.doorFront.SetOpen(false); Log("Puerta principal cerrada");
                foreach (var w in R.windows) { w.SetOpen(false); yield return W(0.5f); }
                Log("Ventanas cerradas");
                yield return W(8f);
                Collect(R.receipt); GameEvents.Raise("inspect_back:receipt");
                AlarmSystem.I.TryCode("1111"); yield return W(1f);
                AlarmSystem.I.TryCode("2308"); Log("Alarma activada");
                yield return W(14f);
                R.player.SetPose(R.poseBedSide, -1f);
                if (!S.CanSleep) Debug.LogError("[AUTOPLAY] CanSleep es falso tras la alarma");
                GameEvents.Raise("bed_sleep"); Log("A dormir");
                yield return WaitStage(Stage.C2_Despertar, 30f);
            }
            // ---------------- CAPÍTULO 2
            if (S.stage == Stage.C2_Despertar)
            {
                yield return W(14f);
                R.player.SetPose(R.poseSalaDoor, -1f);
                R.bolt.SetClosed(true); GameEvents.Raise("bolt_closed"); Log("Cerrojo cerrado → escena de la voz");
                yield return WaitStage(Stage.C2_Camaras, 60f);
            }
            if (S.stage == Stage.C2_Camaras)
            {
                R.player.SetPose(R.poseSalaMonitor, -1f);
                yield return Focus(R.cameraDesk, 8f); Log("Cámaras revisadas");
                GameEvents.Raise("zone:bano");
                GameEvents.Raise("camera_live_failed"); Log("DIRECTO falló → energía");
                yield return W(2f);
                GameEvents.Raise("focus:breaker");
                Collect(R.fuse);
                PowerPanel.I.InstallFuse(); GameEvents.Raise("installed:fuse"); Log("Fusible colocado");
                PowerPanel.I.Apply(new[] { true, true, true, false, false }); Log("Carga 7: A+C+L");
                yield return W(2f);
                SecurityCameraSystem.I.recordingMode = false;
                GameEvents.Raise("camera_live"); Log("DIRECTO → figura");
                FocusSystem.I.Enter(R.cameraDesk);
                yield return WaitStage(Stage.C2_Persecucion, 60f);
            }
            if (S.stage == Stage.C2_Persecucion)
            {
                Log("Persecución 1 (jugador fuera de la casa)"); yield return W(2f); R.player.SetPose(R.poseExitEnd, -1f);
                yield return WaitStage(Stage.C3_Cintas, 150f);
            }
            // ---------------- CAPÍTULO 3
            if (S.stage == Stage.C3_Cintas)
            {
                yield return W(12f);
                Collect(R.tapeA); yield return W(1f); Collect(R.tapeB); yield return W(6f); Collect(R.tapeC); Log("3 cintas");
                R.player.SetPose(R.poseSalaMonitor, -1f);
                FocusSystem.I.Enter(R.vcr);
                for (int i = 0; i < 3; i++) { R.vcr.Insert(); yield return W(17f); }
                Log("Cintas vistas: " + R.vcr.WatchedCount);
                foreach (var id in R.vcr.correctOrder)
                    foreach (var t in R.vcr.tapes) if (t.id == id && t.box) { var b = t.box.GetComponent<TapeBox>(); if (b && b.Clicked != null) b.Clicked(); yield return W(0.5f); }
                Log("Orden C→A→B · reconstruida=" + R.vcr.Reconstructed);
                yield return W(3f);
                GameEvents.Raise("inspected:photo_family");
                yield return WaitStage(Stage.C3_Persecucion, 40f);
            }
            if (S.stage == Stage.C3_Persecucion)
            {
                Log("Persecución 2 (jugador fuera de la casa)"); yield return W(2f); R.player.SetPose(R.poseExitEnd, -1f);
                yield return WaitStage(Stage.C4_Protocolo, 160f);
            }
            // ---------------- CAPÍTULO 4
            if (S.stage == Stage.C4_Protocolo)
            {
                yield return W(6f);
                Collect(R.key);
                Inventory.Remove("key"); GameEvents.Raise("wardrobe_unlocked"); Log("Armario abierto");
                yield return W(8f);
                PowerPanel.I.Apply(new[] { true, false, true, false, true }); Log("Circuito 06 con energía");
                yield return W(2f);
                R.player.SetPose(R.poseSecret, -1f);
                GameEvents.Raise("zone:secreta");
                yield return W(3f);
                GameEvents.Raise("inspected:fatherNotes"); GameEvents.Raise("inspected:isolationDoc");
                Collect(R.reel);
                Inventory.Remove("reel"); GameEvents.Raise("reel_loaded"); Log("Grabación de papá");
                yield return W(36f);
                GameEvents.Raise("aux_lever"); Log("Energía auxiliar → el doble en la cama");
                yield return WaitStage(Stage.C5_Final, 60f);
            }
            // ---------------- CAPÍTULO 5
            if (S.stage == Stage.C5_Final)
            {
                yield return W(8f);
                R.player.SetPose(R.poseExitEnd, -1f);
                PowerPanel.I.Apply(new[] { true, true, true, false, false }); Log("A+C+L restaurados");
                yield return W(45f);
                R.player.SetPose(R.poseSecret, -1f);
                GameEvents.Raise("zone:secreta");
                Log("Verdades: " + S.TruthCount + "/4 · opción 06 visible=" + R.isolation.selfOptionVisible);
                yield return W(4f);
                GameEvents.Raise(endingA || !R.isolation.selfOptionVisible ? "isolate:cocina" : "isolate:secreta");
                Log("Aislamiento elegido → final " + (endingA ? "A" : "B"));
                yield return W(60f);
            }
            Log("FIN de la prueba automática");
            Time.timeScale = 1f;
            Active = false;
        }
#endif
    }
}
