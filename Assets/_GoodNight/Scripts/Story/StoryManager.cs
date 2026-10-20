using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Director de GOOD NIGHT — 5 capítulos. Avanza por EVENTOS (GameEvents) y flags, nunca solo por temporizadores.
    /// Cada punto de control reconstruye el mundo de forma acumulativa (ApplyWorld).
    ///
    /// Cap. 1  Una noche normal          23:47  asegurar la casa · espejo · golpes · alarma (OCUPANTES: 2) · dormir
    /// Cap. 2  Alguien está dentro        00:18  cerrojo y voz falsa · cámaras · fusible · energía · figura · persecución
    /// Cap. 3  La noche que ya ocurrió    01:05  mensajes desde tu número · 3 cintas VHS · fotos · persecución con tu voz
    /// Cap. 4  El protocolo GUARDIÁN      01:40  llave · armario · habitación 06 · cinta del padre · el doble en tu cama
    /// Cap. 5  ¿A quién dejaste salir?    02:13  aislamiento deducible · dos finales
    ///
    /// Verdad (final B): el jugador es la copia. Pistas: espejo con retraso, alarma con 2 ocupantes,
    /// reconstrucción de cintas (SUJETO 2 sin ingreso), foto familiar donde ya no aparece.
    /// </summary>
    public partial class StoryManager : MonoBehaviour
    {
        public static StoryManager I;
        public Stage stage = Stage.None;

        readonly HashSet<string> flags = new HashSet<string>();
        bool Has(string f) => flags.Contains(f);
        bool Set(string f) => flags.Add(f);

        GameRefs R => GameRefs.I;
        HUDView H => HUDView.I;
        PhoneSystem P => PhoneSystem.I;
        ObjectiveManager O => ObjectiveManager.I;
        AudioManager A => AudioManager.I;
        EnemyAI E => R.enemy;

        public bool CanSleep => stage == Stage.C1_Noche && Has("sleepReady");
        public bool BedAvailable => stage == Stage.C1_Noche;
        public int TruthCount { get { int n = 0; foreach (var f in new[] { "truth_mirror", "truth_alarm", "truth_tapes", "truth_photo" }) if (Has(f)) n++; return n; } }

        public string ChapterLabel
        {
            get
            {
                switch (stage)
                {
                    case Stage.C1_Noche: return T("Capítulo 1 · Una noche normal", "Chapter 1 · A normal night");
                    case Stage.C2_Despertar: case Stage.C2_Camaras: case Stage.C2_Persecucion: return T("Capítulo 2 · Alguien está dentro", "Chapter 2 · Someone is inside");
                    case Stage.C3_Cintas: case Stage.C3_Persecucion: return T("Capítulo 3 · La noche que ya ocurrió", "Chapter 3 · The night that already happened");
                    case Stage.C4_Protocolo: return T("Capítulo 4 · El protocolo GUARDIÁN", "Chapter 4 · The GUARDIAN protocol");
                    case Stage.C5_Final: return T("Capítulo 5 · ¿A quién dejaste salir?", "Chapter 5 · Who did you let out?");
                    default: return "";
                }
            }
        }

        static string T(string es, string en) => Settings.T(es, en);
        string You => T("Tú", "You");
        float clockT;

        void Awake() { I = this; }
        void OnEnable() { GameEvents.OnEvent += OnEvent; }
        void OnDisable() { GameEvents.OnEvent -= OnEvent; }

        void Update()
        {
            if (stage == Stage.None || PlayerController.CutsceneLock) return;
            clockT += Time.deltaTime;
            if (clockT >= 40f) { clockT = 0f; GameClock.Add(1); }
            TickChapter();
        }

        // =====================================================================================
        //  ARRANQUE
        // =====================================================================================
        public void Begin(Stage s)
        {
            stage = s;
            ApplyWorld(s);
            RegisterAnomalies();
            switch (s)
            {
                case Stage.C1_Noche: StartCoroutine(C1_Intro()); break;
                case Stage.C2_Despertar: StartCoroutine(C2_Wake()); break;
                case Stage.C2_Camaras: StartCoroutine(C2_CamerasStart(true)); break;
                case Stage.C2_Persecucion: StartCoroutine(C2_Chase(true)); break;
                case Stage.C3_Cintas: StartCoroutine(C3_Start(true)); break;
                case Stage.C3_Persecucion: StartCoroutine(C3_Chase(true)); break;
                case Stage.C4_Protocolo: StartCoroutine(C4_Start(true)); break;
                case Stage.C5_Final: StartCoroutine(C5_Start(true)); break;
            }
        }

        void SaveStage(Stage s) { stage = s; Checkpoint.Save(s); PlayerPrefs.SetString("gn_flags", string.Join(",", new List<string>(flags).ToArray())); PlayerPrefs.Save(); }

        void LoadTruthFlags()
        {
            foreach (var f in PlayerPrefs.GetString("gn_flags", "").Split(','))
                if (f.StartsWith("truth_")) flags.Add(f);
        }

        /// <summary>Reconstruye el estado acumulado del mundo al empezar desde un punto de control.</summary>
        void ApplyWorld(Stage s)
        {
            var lm = LightingManager.I;
            R.doppelBedRoot.gameObject.SetActive(false);   // la cama empieza VACÍA
            R.camGhost.gameObject.SetActive(false);
            PowerPanel.I.fuseOK = true;
            if (s == Stage.C1_Noche) { lm.SetMood(Mood.Warm); PlayerPrefs.DeleteKey("gn_flags"); return; }
            LoadTruthFlags();

            // --- capítulo 1 completado
            foreach (var w in R.windows) w.SetOpen(false, true);
            R.doorFront.SetOpen(false, true); R.bolt.SetKeyLocked(true);
            R.computer.SetOn(false);
            AlarmSystem.I.Armed = true;
            P.Outgoing(PhoneSystem.Mama, "22:12", T("Ya llegué, ma.", "I'm home, mom."));
            P.AddHistory(PhoneSystem.Mama, "23:47", MomIntro);
            P.AddHistory(PhoneSystem.Mama, "23:58", MomAlarm);
            P.AddHistory(PhoneSystem.Mama, "00:04", MomGoodnight);
            JournalManager.I.AddClue("pin", T("PIN de la alarma: 2308 (día en que cambiaron la cerradura).", "Alarm PIN: 2308 (the day the lock was changed)."), false);
            JournalManager.I.AddClue("occupants", T("La alarma contó 2 ocupantes. Estoy solo.", "The alarm counted 2 occupants. I'm alone."), false);
            lm.SetZone("sala", false); lm.SetZone("cocina", false); lm.SetZone("bano", false);
            lm.SetMood(Mood.Night);
            GameClock.Set(0, 18);
            R.bolt.SetClosed(false);
            if (s == Stage.C2_Despertar) return;

            // --- cerrojo superado
            R.bolt.SetClosed(true);
            P.AddHistory(PhoneSystem.Mama, "00:21", MomBolt);
            P.AddHistory(PhoneSystem.Mama, "00:24", MomWork);
            P.AddHistory(PhoneSystem.Mama, "00:31", MomCameras);
            PowerPanel.I.fuseOK = false;
            GameClock.Set(0, 31);
            if (s == Stage.C2_Camaras) return;

            // --- cámaras y energía resueltas
            PowerPanel.I.fuseOK = true;
            PowerPanel.I.SetSilently(true, true, true, false, false);
            SecurityCameraSystem.I.recordingMode = false;
            R.flashlight.Give(true);
            if (R.flashlightPickup) R.flashlightPickup.gameObject.SetActive(false);
            if (R.fuse) R.fuse.gameObject.SetActive(false);
            P.AddHistory(PhoneSystem.Unknown, "00:52", T("No mires detrás de ti.", "Don't look behind you."));
            lm.SetMood(Mood.Dark);
            GameClock.Set(0, 52);
            if (s == Stage.C2_Persecucion) return;

            // --- primera persecución superada
            P.AddHistory(PhoneSystem.Unknown, "01:04", T("Bien. Ahora sabes que está dentro.", "Good. Now you know it's inside."));
            GameClock.Set(1, 5);
            if (s == Stage.C3_Cintas) return;

            // --- cintas
            P.AddHistory(PhoneSystem.Unknown, "01:06", T("Mira lo que pasó a las 22:10.", "Look at what happened at 22:10."));
            R.vcr.ForceReconstructed();
            foreach (var t in new[] { R.tapeA, R.tapeB, R.tapeC }) if (t) t.gameObject.SetActive(false);
            GameClock.Set(1, 24);
            if (s == Stage.C3_Persecucion) return;

            // --- segunda persecución superada
            P.AddHistory(PhoneSystem.Unknown, "01:38", T("La llave está en el abrigo de papá. El cuarto que no existe está detrás del armario.", "The key is in dad's coat. The room that doesn't exist is behind the wardrobe."));
            GameClock.Set(1, 40);
            if (s == Stage.C4_Protocolo) return;

            // --- habitación 06, cinta del padre y el doble
            PowerPanel.I.c06Known = true;
            R.wardrobe.Open(true);
            R.doorMetal.locked = false; R.doorMetal.SetOpen(true, true);
            if (R.key) R.key.gameObject.SetActive(false);
            if (R.wardrobeKeyhole) R.wardrobeKeyhole.used = true;
            R.recorder.available = false; R.auxLever.available = false;
            foreach (var m in R.secretMonitors) m.SetOn(true);
            PowerPanel.I.SetTrippedSilently();
            R.bolt.SetGuardianLock(true);
            lm.SetMood(Mood.Emergency);
            GameClock.Set(2, 13);
        }

        // =====================================================================================
        //  TEXTOS
        // =====================================================================================
        static string MomIntro => T("Cariño, me tocó turno de noche. No llego hasta mañana temprano.\nAntes de dormir cierra la puerta principal, revisa las ventanas y activa la alarma, ¿sí?\nTe quiero <color=#ff5a6e>♥</color>",
                                    "Honey, I got the night shift. I won't be back until early tomorrow.\nBefore bed lock the front door, check the windows and set the alarm, ok?\nLove you <color=#ff5a6e>♥</color>");
        static string MomAlarm => T("Ah, casi lo olvido. Activa la alarma. El código es el día en que cambiamos la cerradura.", "Oh, almost forgot. Set the alarm. The code is the day we changed the lock.");
        static string MomGoodnight => T("Perfecto. Ahora descansa. Buenas noches <color=#ff5a6e>♥</color>", "Perfect. Now get some rest. Good night <color=#ff5a6e>♥</color>");
        static string MomBolt => T("¿Todo bien? Me olvidé de preguntarte si cerraste el cerrojo de adentro.", "Everything ok? I forgot to ask if you locked the inside deadbolt.");
        static string MomWork => T("Estoy en el trabajo. ¿Por qué me estás llamando?", "I'm at work. Why are you calling me?");
        static string MomCameras => T("Si escuchas algo raro, revisa las cámaras de la sala. Tu papá las instaló hace años.", "If you hear anything weird, check the cameras in the living room. Your dad installed them years ago.");

        // =====================================================================================
        //  UTILIDADES DE ESCENA
        // =====================================================================================
        IEnumerator Wait(float s) { yield return new WaitForSeconds(s); }

        /// <summary>Espera a que el jugador mire hacia un punto (ángulo) o pase el tiempo.</summary>
        IEnumerator WaitLookAt(Vector3 p, float angle, float timeout)
        {
            for (float t = 0; t < timeout; t += Time.deltaTime)
            {
                if (R.player.AngleTo(p) < angle && !FocusSystem.Active) yield break;
                yield return null;
            }
        }

        IEnumerator WaitFocusClosed() { while (FocusSystem.Active) yield return null; }

        IEnumerator LockLook(Vector3 target, float duration, float speed = 3f)
        {
            PlayerController.CutsceneLockLook = true;
            for (float t = 0; t < duration; t += Time.deltaTime) { R.player.LookAtPoint(target, Time.deltaTime * speed); yield return null; }
            PlayerController.CutsceneLockLook = false;
        }

        void Knock(Vector3 p, float v = 1f) { A.At(A.knock, p, v); }

        IEnumerator HallFootsteps(float spacing = 0.6f, int count = 8, bool heavy = false)
        {
            var path = R.hallStepPath;
            if (path == null || path.Length < 2) yield break;
            for (int i = 0; i < count; i++)
            {
                Vector3 p = Vector3.Lerp(path[0].position, path[path.Length - 1].position, i / (float)(count - 1));
                A.PlaySteps(p, 0.9f, heavy);
                yield return new WaitForSeconds(spacing);
            }
        }

        // =====================================================================================
        //  EVENTOS
        // =====================================================================================
        void OnEvent(string id)
        {
            switch (stage)
            {
                case Stage.C1_Noche: C1_Event(id); break;
                case Stage.C2_Despertar: C2_WakeEvent(id); break;
                case Stage.C2_Camaras: C2_CamEvent(id); break;
                case Stage.C3_Cintas: C3_Event(id); break;
                case Stage.C4_Protocolo: C4_Event(id); break;
                case Stage.C5_Final: C5_Event(id); break;
            }
            if (id == "hide_caught" && E != null && E.gameObject.activeSelf) GameManager.I.Die(T("\"Te estaba esperando del otro lado de la puerta.\"", "\"It was waiting right outside the door.\""));
        }

        void TickChapter()
        {
            if (stage == Stage.C1_Noche) C1_Tick();
            else if (stage == Stage.C3_Cintas) C3_Tick();
        }
    }
}
