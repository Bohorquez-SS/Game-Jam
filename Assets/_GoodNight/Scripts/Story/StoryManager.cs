using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Director de la historia. Avanza por EVENTOS del mundo (GameEvents) y estados, no por temporizadores.
    /// Cada punto de control (Stage) reconstruye el estado del mundo de forma acumulativa.
    ///
    /// ACTO 1 (23:47) asegurar la casa → golpes → alarma 2308 → mensaje anómalo → dormir
    /// ACTO 2 (00:18) cerrojo + voz falsa → cámaras (grabación antigua) → cuadro eléctrico → figura en cámara → supervivencia
    /// ACTO 3 (01:30) habitación oculta (plano + circuito 06) → grabación del padre → el doble → aislamiento → salida
    /// </summary>
    public class StoryManager : MonoBehaviour
    {
        public static StoryManager I;
        public Stage stage = Stage.None;

        readonly HashSet<string> flags = new HashSet<string>();
        bool Has(string f) => flags.Contains(f);
        bool Set(string f) => flags.Add(f); // true si es la primera vez

        GameRefs R => GameRefs.I;
        HUDView H => HUDView.I;
        PhoneSystem P => PhoneSystem.I;
        ObjectiveManager O => ObjectiveManager.I;
        AudioManager A => AudioManager.I;

        public bool CanSleep => stage == Stage.Act1 && Has("sleepReady");
        public bool BedAvailable => stage == Stage.Act1;

        string You => Settings.T("Tú", "You");
        static string T(string es, string en) => Settings.T(es, en);

        float clockT;
        void Awake() { I = this; }
        void Update()
        {
            if (stage == Stage.None || PlayerController.CutsceneLock) return;
            clockT += Time.deltaTime;
            if (clockT >= 45f) { clockT = 0f; GameClock.Add(1); }
        }
        void OnEnable() { GameEvents.OnEvent += OnEvent; }
        void OnDisable() { GameEvents.OnEvent -= OnEvent; }

        // =====================================================================================
        //  ARRANQUE Y RECONSTRUCCIÓN DEL MUNDO
        // =====================================================================================
        public void Begin(Stage s)
        {
            stage = s;
            ApplyWorld(s);
            RegisterAnomalies();
            switch (s)
            {
                case Stage.Act1: StartCoroutine(Act1Intro()); break;
                case Stage.Act2Wake: StartCoroutine(Act2Wake()); break;
                case Stage.Act2Cameras: StartCoroutine(Act2CamerasStart()); break;
                case Stage.Act2Survival: StartCoroutine(Survival(true)); break;
                case Stage.Act3Search: StartCoroutine(Act3Search(true)); break;
                case Stage.Act3Final: StartCoroutine(FinalStart(true)); break;
            }
        }

        void ApplyWorld(Stage s)
        {
            var lm = LightingManager.I;
            if (s == Stage.Act1) { lm.SetMood(Mood.Warm); return; }

            // --- Acto 1 completado
            foreach (var w in R.windows) w.SetOpen(false, true);
            R.doorFront.SetOpen(false, true); R.bolt.SetKeyLocked(true);
            R.computer.SetOn(false);
            AlarmSystem.I.Armed = true;
            JournalManager.I.AddDocument(R.docReceipt);
            JournalManager.I.AddClue("pin", T("PIN de la alarma: 2308 (día en que cambiaron la cerradura).", "Alarm PIN: 2308 (the day the lock was changed)."));
            P.AddHistory(PhoneSystem.Mom, "23:47", MomIntro);
            P.AddHistory(PhoneSystem.Mom, "23:58", MomAlarm);
            P.AddHistory(PhoneSystem.Mom, "00:04", MomGoodnight);
            P.AddHistory(PhoneSystem.Mom, "00:18", MomBolt);
            lm.SetZone("sala", false); lm.SetZone("cocina", false); lm.SetZone("dormitorio", false); lm.SetZone("bano", false);
            lm.SetMood(Mood.Night);
            GameClock.Set(0, 18);
            R.bolt.SetClosed(false);
            if (s == Stage.Act2Wake) return;

            // --- cerrojo superado
            R.bolt.SetClosed(true);
            P.AddHistory(PhoneSystem.Mom, "00:24", MomWork);
            P.AddHistory(PhoneSystem.Mom, "00:31", MomCameras);
            GameClock.Set(0, 31);
            if (s == Stage.Act2Cameras) return;

            // --- cámaras y energía resueltas
            PowerPanel.I.SetSilently(true, true, true, false, false);
            SecurityCameraSystem.I.recordingMode = false;
            R.flashlight.Give(true);
            if (R.flashlightPickup) R.flashlightPickup.gameObject.SetActive(false);
            JournalManager.I.AddClue("rec", T("La cámara del pasillo reproducía una grabación del 23/08.", "The hallway camera was playing a recording from 08/23."));
            JournalManager.I.AddClue("power", T("Seguridad completa = Alarma 3 + Cámaras 2 + Cerraduras 2 = 7.", "Full security = Alarm 3 + Cameras 2 + Locks 2 = 7."));
            P.AddHistory(PhoneSystem.Unknown, "01:02", T("No mires detrás de ti.", "Don't look behind you."));
            lm.SetMood(Mood.Dark);
            GameClock.Set(1, 2);
            if (s == Stage.Act2Survival) return;

            // --- sobreviviste
            P.AddHistory(PhoneSystem.Unknown, "01:30", T("Bien. Ahora sabes que está dentro.", "Good. Now you know it's inside."));
            P.AddHistory(PhoneSystem.Unknown, "01:31", T("Si quieres salir, busca la habitación que tu padre cerró.", "If you want to get out, find the room your father sealed."));
            P.AddHistory(You, "01:31", T("¿Qué habitación?", "What room?"));
            P.AddHistory(PhoneSystem.Unknown, "01:32", T("La que no aparece en la casa.", "The one that isn't in the house."));
            R.bolt.SetGuardianLock(true);
            GameClock.Set(1, 32);
            if (s == Stage.Act3Search) return;

            // --- habitación oculta y el doble
            JournalManager.I.AddDocument(R.docPlan);
            JournalManager.I.AddDocument(R.docFatherNote);
            JournalManager.I.AddDocument(R.docIsolation);
            PowerPanel.I.c06Known = true;
            R.wardrobe.Open(true);
            R.doorMetal.locked = false; R.doorMetal.SetOpen(true, true);
            R.auxLever.available = false; R.recorder.available = false;
            foreach (var m in R.secretMonitors) m.SetOn(true);
            PowerPanel.I.SetTrippedSilently();
            lm.SetMood(Mood.Emergency);
            R.doppelInBed.gameObject.SetActive(false);
            GameClock.Set(2, 13);
        }

        // =====================================================================================
        //  TEXTOS
        // =====================================================================================
        static string MomIntro => T("Cariño, me toca turno de noche. No voy a llegar hasta mañana temprano.\nAntes de dormir, por favor cierra la puerta principal, revisa las ventanas y activa la alarma.\nTe quiero <color=#ff5a6e>♥</color>",
                                    "Honey, I got the night shift. I won't be home until early tomorrow.\nBefore bed, please lock the front door, check the windows and set the alarm.\nLove you <color=#ff5a6e>♥</color>");
        static string MomAlarm => T("Ah, casi lo olvido. Activa la alarma. El código es el día en que cambiamos la cerradura.", "Oh, almost forgot. Set the alarm. The code is the day we changed the lock.");
        static string MomGoodnight => T("Perfecto. Ahora descansa. Buenas noches <color=#ff5a6e>♥</color>", "Perfect. Now get some rest. Good night <color=#ff5a6e>♥</color>");
        static string MomBolt => T("¿Todo bien? Me olvidé de preguntarte si cerraste el cerrojo interior.", "Everything ok? I forgot to ask if you locked the inside deadbolt.");
        static string MomWork => T("Estoy en el trabajo. ¿Por qué me estás llamando?", "I'm at work. Why are you calling me?");
        static string MomCameras => T("Si escuchas algo raro, revisa las cámaras de la sala. Tu papá las instaló hace años.", "If you hear anything weird, check the cameras in the living room. Your dad installed them years ago.");

        // =====================================================================================
        //  ACTO 1 — ANTES DE DORMIR
        // =====================================================================================
        IEnumerator Act1Intro()
        {
            PlayerController.CutsceneLock = true;
            H.SetFade(1f);
            GameClock.Set(23, 47);
            R.player.SetPose(R.poseDesk, 1.15f);
            yield return new WaitForSeconds(1.5f);
            A.UI(A.vibrate, 0.6f);
            yield return new WaitForSeconds(1.2f);
            yield return H.ShowTitle(T("23:47 — SÁBADO", "23:47 — SATURDAY"), 2f);
            yield return H.FadeTo(0f, 3f);
            yield return new WaitForSeconds(0.8f);
            P.Receive(PhoneSystem.Mom, MomIntro);
            yield return new WaitForSeconds(2f);
            PlayerController.CutsceneLock = false;
            H.Hint(T("[Tab] Teléfono · [E] Interactuar · [Shift] Correr · [Ctrl] Agacharse", "[Tab] Phone · [E] Interact · [Shift] Run · [Ctrl] Crouch"), 7f);
            O.Set(T("Asegura la casa antes de dormir", "Lock up the house before bed"),
                  T("La puerta principal está en la sala. Hay ventanas en tu dormitorio, la sala y la cocina.", "The front door is in the living room. There are windows in your bedroom, the living room and the kitchen."),
                  T("Cierra la puerta principal", "Close the front door"), WindowsText());
        }

        int WindowsClosed() { int n = 0; foreach (var w in R.windows) if (!w.isOpen) n++; return n; }
        string WindowsText() => T("Cierra las ventanas", "Close the windows") + " (" + WindowsClosed() + "/" + R.windows.Length + ")";

        IEnumerator WindowKnocks(int lastIndex)
        {
            yield return new WaitForSeconds(1.2f);
            var src = R.windowOutside[lastIndex];
            for (int i = 0; i < 3; i++) { src.PlayOneShot(A.windowKnock, Settings.Sfx); yield return new WaitForSeconds(0.75f); }
            H.Say("", T("TOC... TOC... TOC...", "KNOCK... KNOCK... KNOCK..."), 2.5f);
            yield return new WaitForSeconds(2.5f);
            H.Say(You, T("¿Hola...?", "Hello...?"), 2f);
            // Si el jugador se aleja, otro golpe desde otra dirección
            Vector3 start = R.player.transform.position;
            float t = 0f;
            while (t < 9f && Vector3.Distance(start, R.player.transform.position) < 3f) { t += Time.deltaTime; yield return null; }
            int other = (lastIndex + 1) % R.windowOutside.Length;
            R.windowOutside[other].PlayOneShot(A.windowKnock, Settings.Sfx);
            Set("knocksDone");
            yield return new WaitForSeconds(2f);
            CheckAct1Secure();
        }

        void CheckAct1Secure()
        {
            if (stage != Stage.Act1 || !Has("frontClosed") || WindowsClosed() < R.windows.Length || !Has("knocksDone")) return;
            if (!Set("alarmMsg")) return;
            StartCoroutine(AlarmMessage());
        }

        IEnumerator AlarmMessage()
        {
            yield return new WaitForSeconds(1.5f);
            P.Receive(PhoneSystem.Mom, MomAlarm);
            O.Set(T("Encuentra el código de la alarma", "Find the alarm code"),
                  T("El panel de la alarma está en el pasillo. Mamá guarda los recibos en la cocina.", "The alarm panel is in the hallway. Mom keeps receipts in the kitchen."),
                  T("Encuentra el código (formato DDMM)", "Find the code (DDMM format)"), T("Activa la alarma en el pasillo", "Arm the alarm in the hallway"));
            if (JournalManager.I.HasDocument("receipt")) O.Check(0);
            if (AlarmSystem.I.Armed && Set("armed")) { O.Check(0); O.Check(1); StartCoroutine(AfterAlarm()); }
        }

        IEnumerator AfterAlarm()
        {
            while (UIManager.I.Get<KeypadPanel>().IsOpen) yield return null;
            yield return new WaitForSeconds(0.6f);
            H.Say(You, T("Qué raro... debe estar fallando.", "That's weird... it must be glitching."), 3f);
            yield return new WaitForSeconds(3.5f);
            P.Receive(PhoneSystem.Mom, MomGoodnight);
            Set("sleepReady");
            O.Set(T("Regresa a tu habitación y acuéstate", "Go back to your room and lie down"), T("Tu cama está en el dormitorio.", "Your bed is in the bedroom."), T("Acuéstate", "Lie down"));
            HorrorEventManager.I.OnEnterZone("hallFlicker", "pasillo", () => stage == Stage.Act1, () => StartCoroutine(HallAnomaly()));
            if (HorrorEventManager.I.PlayerZone == "pasillo") StartCoroutine(HallAnomaly());
        }

        IEnumerator HallAnomaly()
        {
            if (!Set("hallAnomaly")) yield break;
            LightingManager.I.Flicker("pasillo", 2.5f);
            A.UI(A.staticBurst, 0.3f);
            yield return new WaitForSeconds(1.5f);
            P.Receive(PhoneSystem.Unknown, T("No olvides cerrar los ojos.", "Don't forget to close your eyes."), true, 6f);
        }

        IEnumerator SleepTransition()
        {
            PlayerController.CutsceneLock = true;
            O.Clear();
            yield return H.FadeTo(1f, 2f);
            A.SetAmbience(1.3f);
            yield return new WaitForSeconds(1f);
            H.Say("", T("(tic... tac... la lluvia no se detiene)", "(tick... tock... the rain won't stop)"), 3.5f);
            yield return new WaitForSeconds(4f);
            A.SetAmbience(1f);
            Checkpoint.Save(Stage.Act2Wake);
            stage = Stage.Act2Wake;
            LightingManager.I.SetZone("sala", false); LightingManager.I.SetZone("cocina", false);
            LightingManager.I.SetZone("dormitorio", false); LightingManager.I.SetZone("bano", false);
            LightingManager.I.SetMood(Mood.Night);
            R.bolt.SetClosed(false);
            yield return Act2Wake();
        }

        // =====================================================================================
        //  ACTO 2 — LA CASA EMPIEZA A MENTIR
        // =====================================================================================
        IEnumerator Act2Wake()
        {
            PlayerController.CutsceneLock = true;
            H.SetFade(1f);
            GameClock.Set(0, 18);
            R.player.SetPose(R.poseBed, 0.72f);
            yield return H.ShowTitle("00:18", 1.5f);
            yield return H.FadeTo(0f, 2.5f);
            // pasos en el pasillo
            yield return HallFootsteps();
            yield return new WaitForSeconds(1f);
            P.Receive(PhoneSystem.Mom, MomBolt);
            yield return new WaitForSeconds(2.5f);
            yield return H.FadeTo(1f, 0.5f);
            R.player.SetPose(R.poseBedSide, -1f);
            yield return H.FadeTo(0f, 0.5f);
            PlayerController.CutsceneLock = false;
            O.Set(T("Comprueba el cerrojo sin hacer ruido", "Check the deadbolt without making noise"),
                  T("El cerrojo interior está en la puerta principal, en la sala.", "The inside deadbolt is on the front door, in the living room."),
                  T("Revisa el cerrojo de la puerta principal", "Check the front door deadbolt"));
            H.Hint(T("Mantén [Ctrl] para agacharte y moverte sin ruido", "Hold [Ctrl] to crouch and move quietly"), 6f);
            StartCoroutine(NoiseWatch());
        }

        IEnumerator HallFootsteps()
        {
            var path = R.hallStepPath;
            if (path == null || path.Length < 2) yield break;
            for (int i = 0; i < 8; i++)
            {
                float f = i / 7f;
                Vector3 p = Vector3.Lerp(path[0].position, path[path.Length - 1].position, f);
                A.PlaySteps(p, 0.9f);
                yield return new WaitForSeconds(0.6f);
            }
        }

        IEnumerator NoiseWatch()
        {
            while (stage == Stage.Act2Wake && !Has("boltScene"))
            {
                if (R.player.IsRunning && Set("noiseWarn"))
                {
                    A.At(A.steps1, R.frontDoorOutside.transform.position, 0.8f);
                    H.Say("", T("(Algo se detuvo al oír tus pasos...)", "(Something stopped when it heard your steps...)"), 3f);
                }
                yield return null;
            }
        }

        IEnumerator BoltScene()
        {
            O.Check(0);
            var src = R.frontDoorOutside.transform.position;
            yield return new WaitForSeconds(0.8f);
            Set("boltDanger");
            A.PlayVoice(Voice.Mom, src, 0.9f);
            H.Say(T("Voz al otro lado de la puerta", "Voice behind the door"), T("¿Por qué cerraste? Soy mamá.", "Why did you lock it? It's Mom."), 3.5f);
            yield return new WaitForSeconds(3.5f);
            P.Receive(PhoneSystem.Mom, MomWork);
            yield return new WaitForSeconds(3.5f);
            H.Say(You, T("...Mamá está en el trabajo.", "...Mom is at work."), 2.5f);
            A.At(A.knock, src, 0.8f);
            yield return new WaitForSeconds(3f);
            A.PlayVoice(Voice.Mom, src, 0.9f);
            H.Say(T("Voz al otro lado de la puerta", "Voice behind the door"), T("Abre, cariño. Tengo frío.", "Open up, sweetie. I'm cold."), 3.5f);
            yield return new WaitForSeconds(4.5f);
            A.At(A.breath, src, 0.9f);
            A.PlayVoice(Voice.Entity, src, 0.7f);
            H.Say(T("Voz al otro lado de la puerta", "Voice behind the door"), T("<i>Sé que estás ahí.</i>", "<i>I know you're there.</i>"), 3.5f);
            yield return new WaitForSeconds(4f);
            for (int i = 0; i < 3; i++) { A.At(A.knock, src, 1f); yield return new WaitForSeconds(0.45f); }
            yield return new WaitForSeconds(5f);
            if (stage != Stage.Act2Wake) yield break;
            flags.Remove("boltDanger");
            stage = Stage.Act2Cameras;
            Checkpoint.Save(Stage.Act2Cameras);
            yield return new WaitForSeconds(2f);
            yield return Act2CamerasBody();
        }

        IEnumerator Act2CamerasStart()
        {
            PlayerController.CutsceneLock = true;
            H.SetFade(1f);
            R.player.SetPose(R.poseSalaDoor, -1f);
            yield return H.FadeTo(0f, 1.5f);
            PlayerController.CutsceneLock = false;
            yield return Act2CamerasBody();
        }

        IEnumerator Act2CamerasBody()
        {
            P.Receive(PhoneSystem.Mom, MomCameras);
            O.Set(T("Revisa las cámaras de seguridad", "Check the security cameras"),
                  T("El monitor está en el escritorio de la sala.", "The monitor is on the desk in the living room."),
                  T("Usa el monitor de la sala", "Use the living room monitor"));
            yield break;
        }

        IEnumerator CameraDoubt()
        {
            yield return new WaitForSeconds(2f);
            H.Say(You, T("Un momento... esa puerta está cerrada.", "Wait... that door is closed."), 3f);
            O.Set(T("Descubre por qué la cámara muestra algo diferente", "Find out why the camera shows something different"),
                  T("Mira la fecha del monitor y el registro del sistema.", "Check the monitor's date and the system log."),
                  T("Revisa el registro del monitor", "Check the monitor log"), T("Activa la transmisión en directo", "Switch to the live feed"));
        }

        void PowerObjective()
        {
            O.Set(T("Restaura la energía de las cámaras", "Restore power to the cameras"),
                  T("Alarma 3 + Cámaras 2 + Cerraduras 2 = 7. Las luces tendrán que apagarse.", "Alarm 3 + Cameras 2 + Locks 2 = 7. The lights will have to go off."),
                  T("Encuentra el cuadro eléctrico (cocina)", "Find the breaker panel (kitchen)"),
                  T("Alimenta las cámaras sin superar 7 unidades", "Power the cameras without exceeding 7 units"));
            JournalManager.I.AddClue("rec", T("La cámara del pasillo reproducía una grabación del 23/08.", "The hallway camera was playing a recording from 08/23."));
        }

        IEnumerator GhostSequence()
        {
            var cams = SecurityCameraSystem.I;
            var mon = UIManager.I.Get<CameraMonitorPanel>();
            mon.ForceIndex(1);
            yield return new WaitForSeconds(2.5f);
            if (!mon.IsOpen) { while (!mon.IsOpen) yield return null; yield return new WaitForSeconds(1.5f); }
            mon.ForceIndex(1); mon.Interfere(1f);
            cams.ghost.position = R.ghostFar.position; cams.ghost.rotation = R.ghostFar.rotation;
            cams.ghostVisible = true;
            A.UI(A.staticBurst, 0.5f);
            yield return new WaitForSeconds(2.5f);
            H.Say(You, T("Hay... ¿hay alguien en el pasillo?", "Is... is someone in the hallway?"), 3f);
            // El jugador puede cerrar y mirar al pasillo real (vacío). Al volver, la figura está más cerca.
            float t = 0f;
            while (mon.IsOpen && t < 7f) { t += Time.deltaTime; yield return null; }
            if (!mon.IsOpen)
            {
                cams.ghostVisible = false;
                while (!mon.IsOpen) yield return null;
                mon.ForceIndex(1);
                yield return new WaitForSeconds(0.6f);
            }
            mon.Interfere(1.2f);
            cams.ghost.position = R.ghostNear.position; cams.ghost.rotation = R.ghostNear.rotation;
            cams.ghostVisible = true;
            A.UI(A.staticBurst, 0.6f);
            yield return new WaitForSeconds(2.2f);
            cams.ghostVisible = false;
            mon.SetUnavailable(1, true);
            A.UI(A.staticBurst, 0.8f);
            yield return new WaitForSeconds(2f);
            UIManager.I.Close(mon);
            mon.SetUnavailable(1, false);
            stage = Stage.Act2Survival;
            Checkpoint.Save(Stage.Act2Survival);
            yield return Survival(false);
        }

        IEnumerator Survival(bool fromCheckpoint)
        {
            if (fromCheckpoint)
            {
                PlayerController.CutsceneLock = true;
                H.SetFade(1f);
                R.player.SetPose(R.poseSalaMonitor, -1f);
                yield return H.FadeTo(0f, 1.5f);
                PlayerController.CutsceneLock = false;
            }
            // Una puerta se abre lentamente...
            R.doorBathroom.speed = 0.5f;
            R.doorBathroom.SetOpen(true);
            A.At(A.breath, R.doorBathroom.transform.position, 0.8f);
            yield return new WaitForSeconds(1.2f);
            P.Receive(PhoneSystem.Unknown, T("No mires detrás de ti.", "Don't look behind you."));
            yield return new WaitForSeconds(1.5f);
            R.doorBathroom.speed = 2.6f;
            var e = R.enemy;
            e.OnCaught = () => GameManager.I.Die(T("\"No todas las voces quieren ayudarte.\"", "\"Not every voice wants to help you.\""));
            e.Spawn(R.spawnBathroom.position, R.spawnBathroom.eulerAngles.y, false, R.routeAct2);
            O.Set(T("Sobrevive", "Survive"),
                  T("Escóndete en el armario del dormitorio o en la despensa de la cocina, o agáchate detrás del sofá. Correr hace ruido. Apagar la linterna [F] te hace menos visible.",
                    "Hide in the bedroom wardrobe or the kitchen pantry, or crouch behind the sofa. Running makes noise. Turning off the flashlight [F] makes you harder to see."),
                  T("Que no te encuentre", "Don't let it find you"));
            H.Hint(T("Escóndete. Agachado haces menos ruido.", "Hide. Crouching makes less noise."), 5f);
            A.SetMusic(0.3f);
            float time = 0f;
            while (time < 55f || e.State == AIState.Chase || e.State == AIState.Investigate)
            {
                time += Time.deltaTime;
                if (stage != Stage.Act2Survival) yield break;
                yield return null;
            }
            bool returned = false;
            e.OnReturned = () => returned = true;
            e.ReturnTo(R.nodeBathroom);
            float rt = 0f;
            while (!returned && rt < 30f)
            {
                rt += Time.deltaTime;
                if (e.State == AIState.Chase) { yield return null; continue; }
                yield return null;
            }
            e.Despawn();
            R.doorBathroom.SetOpen(false);
            A.SetMusic(0.12f);
            O.Check(0);
            yield return new WaitForSeconds(2f);
            stage = Stage.Act3Search;
            Checkpoint.Save(Stage.Act3Search);
            yield return Act3Search(false);
        }

        // =====================================================================================
        //  ACTO 3 — LA VERDAD
        // =====================================================================================
        IEnumerator Act3Search(bool fromCheckpoint)
        {
            if (fromCheckpoint)
            {
                PlayerController.CutsceneLock = true;
                H.SetFade(1f);
                R.player.SetPose(R.poseSalaMonitor, -1f);
                yield return H.FadeTo(0f, 1.5f);
                PlayerController.CutsceneLock = false;
            }
            else
            {
                P.Receive(PhoneSystem.Unknown, T("Bien. Ahora sabes que está dentro.", "Good. Now you know it's inside."));
                yield return new WaitForSeconds(4f);
            }
            GameClock.Set(1, 31);
            P.Receive(PhoneSystem.Unknown, T("Si quieres salir, busca la habitación que tu padre cerró.", "If you want to get out, find the room your father sealed."));
            yield return new WaitForSeconds(3f);
            H.Say(You, T("¿Qué habitación?", "What room?"), 2.5f);
            P.AddHistory(You, GameClock.Text, T("¿Qué habitación?", "What room?"));
            yield return new WaitForSeconds(3f);
            P.Receive(PhoneSystem.Unknown, T("La que no aparece en la casa.", "The one that isn't in the house."));
            yield return new WaitForSeconds(2.5f);
            A.PlayVoice(Voice.Guardian);
            H.Banner(T("Por su seguridad, regrese al dormitorio.", "For your safety, return to the bedroom."), 4f);
            R.bolt.SetGuardianLock(true);
            A.UI(A.lockClunk, 0.8f);
            O.Set(T("Encuentra la habitación oculta", "Find the hidden room"),
                  T("La alarma dijo \"06 zonas\", pero la casa tiene cinco. Revisa los cajones de la sala.", "The alarm said \"06 zones\", but the house has five. Check the living room drawers."),
                  T("Busca pistas sobre una habitación que no existe", "Look for clues about a room that doesn't exist"));
            if (JournalManager.I.HasDocument("plan")) OnPlanFound();
        }

        void OnPlanFound()
        {
            if (!Set("planFound")) return;
            PowerPanel.I.c06Known = true;
            JournalManager.I.AddClue("06", T("El plano antiguo muestra una habitación detrás del armario del pasillo, marcada \"06\". El cuadro eléctrico tiene un circuito 06.",
                                             "The old floor plan shows a room behind the hallway wardrobe, marked \"06\". The breaker panel has a circuit 06."));
            O.Set(T("Encuentra la habitación oculta", "Find the hidden room"),
                  T("El circuito 06 necesita 2 unidades: apaga otro circuito para liberar energía.", "Circuit 06 needs 2 units: switch another circuit off to free power."),
                  T("Lee el plano antiguo", "Read the old floor plan"), T("Activa el circuito 06 en el cuadro eléctrico", "Engage circuit 06 at the breaker panel"));
            O.Check(0);
        }

        IEnumerator WardrobeMoved()
        {
            O.Check(1);
            H.Say("", T("(Un mecanismo antiguo se mueve en el pasillo...)", "(An old mechanism grinds in the hallway...)"), 3.5f);
            yield return new WaitForSeconds(4.5f);
            R.doorMetal.locked = false;
            O.Set(T("Entra en la habitación oculta", "Enter the hidden room"), T("El armario del pasillo se movió.", "The hallway wardrobe moved."), T("Abre la puerta metálica", "Open the metal door"));
        }

        IEnumerator RecorderPart1()
        {
            O.Set(T("Escucha la grabación", "Listen to the recording"), "", T("Reproduce la grabación", "Play the recording"));
            A.PlayVoice(Voice.Dad);
            H.Say(T("Voz del padre", "Father's voice"), T("Si estás escuchando esto, significa que el sistema ha vuelto a activarse.", "If you're hearing this, it means the system has come back online."), 4.5f);
            yield return new WaitForSeconds(4.8f);
            A.PlayVoice(Voice.Dad);
            H.Say(T("Voz del padre", "Father's voice"), T("Lo instalé para protegernos, pero empezó a registrar personas que no existían...", "I installed it to protect us, but it started logging people who didn't exist..."), 4.5f);
            yield return new WaitForSeconds(3.5f);
            A.UI(A.staticBurst, 0.8f);
            H.Say("", T("ERROR: EQUIPO SIN ENERGÍA.", "ERROR: EQUIPMENT HAS NO POWER."), 3f);
            R.auxLever.available = true;
            O.Set(T("Restaura la energía del equipo", "Restore power to the equipment"),
                  T("Hay una palanca de energía auxiliar en la pared.", "There's an auxiliary power lever on the wall."),
                  T("Activa el generador auxiliar", "Turn on the auxiliary generator"));
        }

        IEnumerator Reveal()
        {
            PlayerController.CutsceneLock = true;
            PlayerController.CutsceneLockLook = true;
            O.Clear();
            if (R.bedroomLamp) { R.bedroomLamp.forcedOn = true; LightingManager.I.ApplyAll(); }
            A.At(A.mechanism, R.auxLever.transform.position, 0.7f);
            yield return new WaitForSeconds(1.2f);
            Vector3 lookTarget = R.secretMonitors[0].transform.position;
            for (int i = 0; i < R.secretMonitors.Length; i++)
            {
                R.secretMonitors[i].SetOn(true);
                A.At(A.click, R.secretMonitors[i].transform.position, 0.9f);
                A.At(A.staticBurst, R.secretMonitors[i].transform.position, 0.4f);
                float t = 0f;
                while (t < 1.1f) { t += Time.deltaTime; R.player.LookAtPoint(R.secretMonitors[i].transform.position, Time.deltaTime * 3f); yield return null; }
            }
            // el último monitor: el dormitorio... alguien en tu cama
            float w = 0f;
            while (w < 2.5f) { w += Time.deltaTime; R.player.LookAtPoint(R.secretMonitors[3].transform.position, Time.deltaTime * 3f); yield return null; }
            // se incorpora
            Quaternion lying = R.doppelTorso.localRotation, sitting = Quaternion.identity;
            for (float t = 0; t < 1f; t += Time.deltaTime / 2.5f) { R.doppelTorso.localRotation = Quaternion.Slerp(lying, sitting, Mathf.SmoothStep(0, 1, t)); yield return null; }
            A.UI(A.heartbeat, 0.8f);
            yield return new WaitForSeconds(0.6f);
            var camPos = SecurityCameraSystem.I.ByZone("dormitorio").transform.position;
            Quaternion h0 = R.doppelHead.rotation, h1 = Quaternion.LookRotation(camPos - R.doppelHead.position);
            for (float t = 0; t < 1f; t += Time.deltaTime / 1.2f) { R.doppelHead.rotation = Quaternion.Slerp(h0, h1, t); yield return null; }
            yield return new WaitForSeconds(1f);
            A.PlayVoice(Voice.You);
            H.Say(You, T("¿Por qué estás usando mi cara?", "Why are you wearing my face?"), 3.5f);
            yield return new WaitForSeconds(4f);
            A.PlayVoice(Voice.Dad);
            H.Say(T("Voz del padre", "Father's voice"), T("No confíes en las voces.", "Don't trust the voices."), 3f);
            yield return new WaitForSeconds(3.2f);
            A.PlayVoice(Voice.Dad);
            H.Say(T("Voz del padre", "Father's voice"), T("Puede imitarte, pero no puede estar en dos lugares al mismo tiempo.", "It can imitate you, but it can't be in two places at once."), 4.5f);
            yield return new WaitForSeconds(4.8f);
            A.PlayVoice(Voice.Guardian);
            H.Banner(T("DOS IDENTIDADES IDÉNTICAS DETECTADAS.", "TWO IDENTICAL IDENTITIES DETECTED."), 3.5f);
            yield return new WaitForSeconds(3.6f);
            A.PlayVoice(Voice.Guardian);
            H.Banner(T("INICIANDO PROTOCOLO DE ELIMINACIÓN DE DUPLICADOS.", "INITIATING DUPLICATE ELIMINATION PROTOCOL."), 4f);
            PowerPanel.I.Trip();
            LightingManager.I.SetMood(Mood.Emergency);
            yield return new WaitForSeconds(2f);
            // el doble se levanta: ahora es la entidad
            R.doppelInBed.gameObject.SetActive(false);
            stage = Stage.Act3Final;
            Checkpoint.Save(Stage.Act3Final);
            JournalManager.I.AddDocument(R.docIsolation);
            PlayerController.CutsceneLockLook = false;
            PlayerController.CutsceneLock = false;
            yield return FinalStart(false);
        }

        // =====================================================================================
        //  PUZLE FINAL
        // =====================================================================================
        IEnumerator FinalStart(bool fromCheckpoint)
        {
            if (fromCheckpoint)
            {
                PlayerController.CutsceneLock = true;
                H.SetFade(1f);
                R.player.SetPose(R.poseSecret, -1f);
                yield return H.FadeTo(0f, 1.5f);
                PlayerController.CutsceneLock = false;
            }
            var e = R.enemy;
            e.OnCaught = () => GameManager.I.Die(T("\"Puede imitarte. No dejes que te reemplace.\"", "\"It can imitate you. Don't let it replace you.\""));
            e.Spawn(R.spawnBed.position, R.spawnBed.eulerAngles.y, true, R.routeFinal);
            O.Set(T("Aísla la presencia y escapa de la casa", "Isolate the presence and escape the house"),
                  T("Nota del padre: la alarma registra ecos; solo la cámara en directo dice la verdad. Solo funciona con seguridad completa.",
                    "Father's note: the alarm logs echoes; only the live camera tells the truth. It only works with full security."),
                  T("Restablece la seguridad completa en el cuadro eléctrico (cocina)", "Restore full security at the breaker panel (kitchen)"),
                  T("Ubícala con la alarma y confírmala con las cámaras", "Locate it with the alarm and confirm with the cameras"),
                  T("Aíslala desde el panel de la habitación oculta", "Isolate it from the hidden room panel"));
            if (PowerPanel.I.SecurityComplete) O.Check(0);
            StartCoroutine(MotionReadings());
        }

        IEnumerator MotionReadings()
        {
            string echo = "sala"; float echoT = 0f;
            string[] candidates = { "sala", "cocina", "pasillo", "dormitorio" };
            while (stage == Stage.Act3Final)
            {
                IsolationPanel.Motion.Clear();
                if (AlarmSystem.I.Powered && R.enemy.gameObject.activeSelf)
                {
                    string real = R.enemy.Zone;
                    echoT -= 1f;
                    if (echoT <= 0f || echo == real) { echo = candidates[Random.Range(0, candidates.Length)]; echoT = Random.Range(6f, 11f); }
                    if (real != null && System.Array.IndexOf(candidates, real) >= 0) IsolationPanel.Motion.Add(real);
                    if (echo != real) IsolationPanel.Motion.Add(echo);
                }
                yield return new WaitForSeconds(1f);
            }
        }

        IEnumerator Isolate(string zone)
        {
            if (!Set("isolating")) yield break;
            var shutters = zone == "cocina" ? R.shuttersKitchen : R.shuttersBedroom;
            O.Check(1);
            foreach (var s in shutters) s.SetDown(true);
            A.PlayVoice(Voice.Guardian);
            H.Banner(T("AISLANDO ZONA: ", "ISOLATING ZONE: ") + RoomZone.Get(zone).DisplayName.ToUpperInvariant() + "...", 3f);
            yield return new WaitForSeconds(3.2f);
            bool success = R.enemy.gameObject.activeSelf && R.enemy.Zone == zone;
            if (!success)
            {
                A.UI(A.error, 1f);
                H.Banner(T("AISLAMIENTO FALLIDO: ZONA VACÍA.", "ISOLATION FAILED: ZONE EMPTY."), 3f);
                foreach (var s in shutters) s.SetDown(false);
                R.enemy.Investigate(R.player.transform.position);
                H.Hint(T("¡Te oyó! Escóndete en el casillero.", "It heard you! Hide in the locker."), 4f);
                yield return new WaitForSeconds(3f);
                flags.Remove("isolating");
                yield break;
            }
            // ÉXITO
            stage = Stage.None; // evita otros eventos
            O.Check(2);
            R.enemy.OnCaught = null;
            R.enemy.Despawn();
            Vector3 bang = shutters[0].transform.position - Vector3.up * 1.4f;
            for (int i = 0; i < 4; i++) { A.At(A.metalBang, bang, 1f); yield return new WaitForSeconds(0.35f); }
            A.PlayVoice(Voice.Mom, bang, 1f);
            H.Say(T("Voz de mamá", "Mom's voice"), T("¡Hijo! ¡Ábreme, por favor!", "Sweetie! Open up, please!"), 3f);
            yield return new WaitForSeconds(3f);
            for (int i = 0; i < 3; i++) { A.At(A.metalBang, bang, 1f); yield return new WaitForSeconds(0.3f); }
            A.PlayVoice(Voice.Dad, bang, 1f);
            H.Say(T("Voz de papá", "Dad's voice"), T("Hijo, soy yo. Déjame salir.", "Son, it's me. Let me out."), 3f);
            yield return new WaitForSeconds(3f);
            for (int i = 0; i < 5; i++) { A.At(A.metalBang, bang, 1f); yield return new WaitForSeconds(0.22f); }
            A.PlayVoice(Voice.You, bang, 1f);
            H.Say(T("Tu propia voz", "Your own voice"), T("¡Ábreme! ¡Soy tú!", "Open up! It's me! I'm you!"), 3.5f);
            yield return new WaitForSeconds(4.5f);
            A.SetAmbience(0.6f);
            yield return new WaitForSeconds(2f);
            A.PlayVoice(Voice.Guardian);
            H.Banner(T("PRESENCIA AISLADA.\nPROTOCOLO COMPLETADO.", "PRESENCE ISOLATED.\nPROTOCOL COMPLETE."), 5f);
            R.bolt.SetGuardianLock(false);
            R.bolt.SetKeyLocked(false);
            R.bolt.SetClosed(false);
            R.doorFront.locked = false;
            A.UI(A.lockClunk, 1f);
            Set("exitOpen");
            O.Set(T("Sal de la casa", "Leave the house"), T("La puerta principal está en la sala.", "The front door is in the living room."), T("Abre la puerta principal", "Open the front door"));
            R.doorFront.BeforeOpen = () => { StartCoroutine(EndingManager.I.Play()); return true; };
            HorrorEventManager.I.OnEnterZone("momArriving", "sala", () => Has("exitOpen"),
                () => P.Receive(PhoneSystem.Mom, T("Ya estoy llegando. ¿Estás bien?", "I'm almost home. Are you okay?")));
        }

        // =====================================================================================
        //  EVENTOS
        // =====================================================================================
        void OnEvent(string id)
        {
            switch (stage)
            {
                case Stage.Act1: Act1Event(id); break;
                case Stage.Act2Wake: if (id == "bolt_closed" && Set("boltScene")) StartCoroutine(BoltScene());
                    else if (id == "bolt_opened" && Has("boltDanger")) GameManager.I.Die(T("\"No todas las voces quieren ayudarte.\"", "\"Not every voice wants to help you.\""));
                    break;
                case Stage.Act2Cameras: Act2CamEvent(id); break;
                case Stage.Act3Search: Act3Event(id); break;
                case Stage.Act3Final:
                    if (id == "power_changed" && PowerPanel.I.SecurityComplete) O.Check(0);
                    else if (id.StartsWith("isolate:")) StartCoroutine(Isolate(id.Substring(8)));
                    break;
            }
        }

        void Act1Event(string id)
        {
            if (id == "door_closed:front" && Set("frontClosed"))
            {
                A.At(A.lockClunk, R.doorFront.transform.position, 1f);
                R.bolt.SetKeyLocked(true);
                O.Check(0);
                CheckAct1Secure();
            }
            else if (id.StartsWith("window_"))
            {
                if (Has("knocksStarted")) { O.SetItem(1, WindowsText()); return; }
                O.SetItem(1, WindowsText());
                if (WindowsClosed() == R.windows.Length)
                {
                    O.Check(1);
                    Set("knocksStarted");
                    int last = 0;
                    for (int i = 0; i < R.windows.Length; i++) if (id == "window_closed:" + R.windows[i].id) last = i;
                    StartCoroutine(WindowKnocks(last));
                }
                else O.Check(1, false);
            }
            else if (id == "station_opened:Alarm" && Has("alarmMsg") && Set("alarmHint") && !JournalManager.I.HasDocument("receipt"))
                H.Say(You, T("Mamá siempre guarda los recibos en la cocina.", "Mom always keeps the receipts in the kitchen."), 3.5f);
            else if (id == "doc:receipt")
            {
                JournalManager.I.AddClue("pin", T("Recibo del cerrajero: cerradura cambiada el 23 de agosto → PIN 2308 (DDMM).", "Locksmith receipt: lock changed on August 23 → PIN 2308 (DDMM)."));
                if (Has("alarmMsg")) O.Check(0);
            }
            else if (id == "alarm_armed")
            {
                if (!Has("alarmMsg")) { Set("armedEarly"); return; } // se continúa cuando llegue el mensaje de mamá
                if (!Set("armed")) return;
                O.Check(0); O.Check(1);
                StartCoroutine(AfterAlarm());
            }
            else if (id == "bed_sleep" && CanSleep && Set("slept")) StartCoroutine(SleepTransition());
        }

        void Act2CamEvent(string id)
        {
            if (id == "camera_monitor_opened" && Set("camOpened")) StartCoroutine(CameraDoubt());
            else if (id == "camera_log_viewed" && Has("camOpened")) { O.Check(0); JournalManager.I.AddClue("rec", T("La cámara del pasillo reproducía una grabación del 23/08.", "The hallway camera was playing a recording from 08/23.")); }
            else if (id == "camera_live_failed" && Set("needPower")) PowerObjective();
            else if (id == "station_opened:Power" && Has("needPower")) O.Check(0);
            else if (id == "power_changed")
            {
                if (!PowerPanel.I.On(Circuit.Lights)) LightingManager.I.SetMood(Mood.Dark);
                if (PowerPanel.I.SecurityComplete && Set("powerOk"))
                {
                    O.Check(1);
                    JournalManager.I.AddClue("power", T("Seguridad completa = Alarma 3 + Cámaras 2 + Cerraduras 2 = 7.", "Full security = Alarm 3 + Cameras 2 + Locks 2 = 7."));
                    if (!Has("ghost")) StartCoroutine(AfterPower());
                }
            }
            else if (id == "power_tripped" && Has("needPower"))
            {
                LightingManager.I.SetMood(Mood.Dark);
                H.Say(You, T("Se fue todo... tengo que volver a encenderlo sin pasarme de 7.", "Everything's out... I need to turn it back on without going over 7."), 3.5f);
            }
            else if (id == "camera_live" && Set("ghost")) StartCoroutine(GhostSequence());
        }

        IEnumerator AfterPower()
        {
            while (UIManager.I.Get<PowerPanelUI>().IsOpen) yield return null;
            if (!R.flashlight.HasFlashlight)
                H.Say(You, T("Está todo oscuro... había una linterna en la encimera.", "It's pitch black... there was a flashlight on the counter."), 3.5f);
            O.Set(T("Revisa la transmisión en directo", "Check the live feed"), T("Vuelve al monitor de la sala.", "Go back to the living room monitor."),
                  T("Vuelve al monitor de la sala", "Return to the living room monitor"));
        }

        void Act3Event(string id)
        {
            if (id == "doc:plan") OnPlanFound();
            else if (id == "power_changed" && PowerPanel.I.On(Circuit.C06) && Has("planFound") && !R.wardrobe.opened) R.wardrobe.Open(false);
            else if (id == "power_changed" && PowerPanel.I.On(Circuit.C06) && !Has("planFound"))
                H.Say(You, T("Ese circuito no parece hacer nada... ¿o sí?", "That circuit doesn't seem to do anything... or does it?"), 3f);
            else if (id == "wardrobe_opened" && Set("wardrobeMoved")) StartCoroutine(WardrobeMoved());
            else if (id == "door_opened:metal" && Set("metalOpen")) { LightingManager.I.SetMood(Mood.Emergency); O.Set(T("Investiga la habitación oculta", "Investigate the hidden room"), "", T("Reproduce la grabación", "Play the recording")); }
            else if (id == "recorder_play" && Set("rec1")) StartCoroutine(RecorderPart1());
            else if (id == "aux_on" && Set("aux")) StartCoroutine(Reveal());
        }

        // =====================================================================================
        //  ANOMALÍAS AMBIENTALES (una sola vez cada una)
        // =====================================================================================
        void RegisterAnomalies()
        {
            var h = HorrorEventManager.I;
            // Acto 2: sombra en la ventana de la sala
            h.OnEnterZone("shadowWindow", "sala", () => stage == Stage.Act2Cameras, () => StartCoroutine(ShadowPass()));
            // Acto 2: golpe en una pared interior (la habitación oculta)
            h.OnEnterZone("wallKnock", "pasillo", () => stage == Stage.Act2Cameras && Has("camOpened"), () =>
            { R.secretWall.PlayOneShot(A.knock, Settings.Sfx); H.Say("", T("(Un golpe... desde dentro de la pared.)", "(A knock... from inside the wall.)"), 3f); });
            // Acto 3: la lámpara del dormitorio se enciende sola
            h.OnEnterZone("lampOn", "pasillo", () => stage == Stage.Act3Search, () => { if (R.bedroomLamp) { R.bedroomLamp.forcedOn = true; LightingManager.I.ApplyAll(); A.At(A.click, R.bedroomLamp.transform.position, 0.8f); } });
            // Acto 3: pasos en el baño vacío
            h.OnEnterZone("bathSteps", "pasillo", () => stage == Stage.Act3Search && HorrorEventManager.I.Fired("lampOn"), () => StartCoroutine(BathSteps()));
            // Acto 3: voz de mamá desde la cocina
            h.OnEnterZone("momKitchen", "sala", () => stage == Stage.Act3Search, () =>
            { A.PlayVoice(Voice.Mom, R.kitchenVoice.transform.position, 0.8f); H.Say(T("Voz desde la cocina", "Voice from the kitchen"), T("Hijo... ven a la cocina. Ya llegué.", "Sweetie... come to the kitchen. I'm home."), 3.5f); });
            // Acto 3: la puerta del dormitorio se cierra sola
            h.OnEnterZone("doorSlam", "cocina", () => stage == Stage.Act3Search && R.doorBedroom.isOpen, () => R.doorBedroom.SetOpen(false));
        }

        IEnumerator ShadowPass()
        {
            var s = R.shadowFigure; if (s == null) yield break;
            s.gameObject.SetActive(true);
            for (float t = 0; t < 1f; t += Time.deltaTime / 1.6f) { s.position = Vector3.Lerp(R.shadowStart.position, R.shadowEnd.position, t); yield return null; }
            s.gameObject.SetActive(false);
        }

        IEnumerator BathSteps()
        {
            var p = R.bathroomInside.transform.position;
            for (int i = 0; i < 5; i++) { A.PlaySteps(p + new Vector3(Random.Range(-0.6f, 0.6f), 0, Random.Range(-0.6f, 0.6f)), 0.8f); yield return new WaitForSeconds(0.7f); }
        }
    }
}
