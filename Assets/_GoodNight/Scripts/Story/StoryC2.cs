using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>CAPÍTULO 2 — ALGUIEN ESTÁ DENTRO (00:18).
    /// Despertar · pasos en el pasillo · el cerrojo · la voz falsa de mamá (y una llamada que no hiciste) · cámaras en modo ARCHIVO
    /// (la puerta del baño "abierta") · fusible F2 y carga 7 · DIRECTO: la figura en el pasillo · "no mires detrás de ti" · persecución.</summary>
    public partial class StoryManager
    {
        // ------------------------------------------------------------------ despertar
        IEnumerator C2_Wake()
        {
            PlayerController.CutsceneLock = true;
            H.SetFade(1f);
            R.player.SetPose(R.poseBedLie, 0.72f);
            GameClock.Set(0, 18);
            A.SetAmbience(0.5f);
            yield return Wait(1.0f);
            // algo te despierta: pasos pesados en el pasillo
            StartCoroutine(HallFootsteps(0.7f, 7, true));
            yield return Wait(2.2f);
            yield return H.FadeTo(0.4f, 1.5f);
            yield return H.Blink(2);
            yield return H.FadeTo(0f, 1.2f);
            yield return H.ShowTitle(ChapterLabel, 2.2f);
            yield return R.player.MoveCamera(R.poseBedSit.position, R.poseBedSit.eulerAngles.y, 12f, 0.95f, 0f, 1.6f);
            yield return Wait(0.6f);
            H.Say(You, T("Pasos... en el pasillo.", "Footsteps... in the hallway."), 3f);
            yield return VoiceBank.Wait(2.5f);
            P.Receive(PhoneSystem.Mama, MomBolt);
            GameClock.Set(0, 21);
            yield return Wait(2f);
            yield return R.player.MoveCamera(R.poseBedSide.position, R.poseBedSide.eulerAngles.y, 0f, R.player.standEye, 0f, 1.0f);
            R.player.ReleaseEyeOverride();
            PlayerController.CutsceneLock = false;
            O.Set(T("Revisa el cerrojo sin hacer ruido", "Check the deadbolt without making noise"),
                  T("La puerta principal tiene un cerrojo interior. Agáchate para no hacer ruido.", "The front door has an inside deadbolt. Crouch to stay quiet."),
                  T("Revisa el cerrojo de la puerta principal", "Check the front door deadbolt"));
            H.Hint(T("Mantén [Ctrl] para agacharte y moverte en silencio", "Hold [Ctrl] to crouch and move quietly"), 6f);
            StartCoroutine(C2_NoiseWatch());
        }

        IEnumerator C2_NoiseWatch()
        {
            float warn = 0f; int strikes = 0;
            while (stage == Stage.C2_Despertar && !Has("boltScene"))
            {
                warn -= Time.deltaTime;
                string z = RoomZone.At(R.player.transform.position);
                if (R.player.IsRunning && (z == "sala" || z == "pasillo") && warn <= 0f)
                {
                    warn = 6f; strikes++;
                    // algo responde a tu ruido desde afuera
                    Knock(R.frontDoorOutside.transform.position, 0.6f + strikes * 0.15f);
                    H.Say(You, strikes < 2 ? T("Shhh... me escuchó.", "Shhh... it heard me.") : T("Está justo del otro lado de la puerta.", "It's right on the other side of the door."), 2.5f);
                    A.SetTension(0.25f * strikes);
                }
                yield return null;
            }
        }

        void C2_WakeEvent(string id)
        {
            if (id == "bolt_closed" && Set("boltScene")) StartCoroutine(C2_BoltScene());
            else if (id == "bolt_opened" && Has("boltDanger"))
            {
                // abrir el cerrojo NO mata: la voz se alegra... y la puerta ya se puede abrir
                A.PlayVoiceExact(Voice.Mom, R.frontDoorOutside.transform.position, 0.8f);
                H.Say(T("¿Mamá? (afuera)", "Mom? (outside)"), T("Eso es, mijo... ahora la puerta. Ábreme.", "That's it, sweetie... now the door. Let me in."), 3.5f);
                A.SetTension(0.6f);
            }
            else if (id == "bolt_closed" && Has("boltDanger")) A.SetTension(0.2f);
            else if (id == "door_opened:front" && Has("boltDanger") && Set("openedToMom")) StartCoroutine(C2_OpenDoorDeath());
        }

        // ------------------------------------------------------------------ la voz detrás de la puerta
        IEnumerator C2_BoltScene()
        {
            O.Check(0);
            yield return Wait(1.6f);
            var src = R.frontDoorOutside.transform.position;
            Knock(src, 1f); yield return Wait(0.45f); Knock(src, 1f); yield return Wait(0.45f); Knock(src, 1f);
            CameraFX.Shake(0.08f);
            yield return Wait(1.6f);
            A.PlayVoiceExact(Voice.Mom, src, 0.9f);
            H.Say(T("¿Mamá? (afuera)", "Mom? (outside)"), T("¿Cariño? Soy yo. Me olvidé las llaves... ábreme, hace frío.", "Honey? It's me. I forgot my keys... open up, it's cold."), 4.5f);
            Set("boltDanger");
            R.bolt.SetKeyLocked(false);   // desde adentro la llave se puede girar: la decisión es REAL (cerrojo + puerta)
            O.Set(T("¿Mamá?", "Mom?"), T("Mamá dijo que estaba en el trabajo hasta la mañana. Puedes abrir el cerrojo y la puerta... o alejarte.", "Mom said she'd be at work until morning. You can unlock the bolt and the door... or step away."), T("Decide si debes abrir la puerta", "Decide whether to open the door"));
            yield return Wait(5f);
            // tu teléfono la está llamando... sin que lo toques
            A.UI(A.phoneTap, 0.6f);
            P.CallLog(PhoneSystem.Mama, "00:23", true, "0:07");
            yield return Wait(1.5f);
            P.Receive(PhoneSystem.Mama, MomWork);
            GameClock.Set(0, 24);
            yield return Wait(2.5f);
            H.Say(You, T("Yo no la llamé. Y si está en el trabajo... ¿quién está en la puerta?", "I didn't call her. And if she's at work... who's at the door?"), 4.5f);
            yield return VoiceBank.Wait(3.5f);
            A.PlayVoiceExact(Voice.Mom, src, 1f);
            H.Say(T("¿Mamá? (afuera)", "Mom? (outside)"), T("¿Por qué no me abres? ...Sé que estás despierto.", "Why won't you open? ...I know you're awake."), 4f);
            A.At(A.handleRattle, src + Vector3.forward * 0.5f, 1f);
            yield return Wait(2.6f);
            if (Has("openedToMom")) yield break;
            // la voz empieza a deformarse (mismo timbre, cada vez más grave y extraño)
            A.PlayVoiceExact(Voice.Mom, src, 0.9f);
            H.Say(T("¿Mamá? (afuera)", "Mom? (outside)"), T("Abre la puerta... te estoy diciendo que abras.", "Open the door... I'm telling you to open it."), 3.5f);
            yield return VoiceBank.Wait(3.8f);
            if (Has("openedToMom")) yield break;
            A.At(A.handleRattle, src + Vector3.forward * 0.5f, 1f);
            Knock(src, 1f);
            A.PlayVoiceExact(Voice.Mom, src, 1f);
            H.Say(T("¿Mamá? (afuera)", "Mom? (outside)"), T("¡ABRE LA PUERTA!", "OPEN THE DOOR!"), 2.5f);
            CameraFX.Shake(0.12f);
            yield return Wait(4f);
            // silencio... y algo rodea la casa
            yield return A.Silence(2.5f);
            A.PlaySteps(src + new Vector3(-2f, 0, -0.5f), 0.7f, true);
            yield return Wait(1.5f);
            R.windowOutside[1].PlayOneShot(A.windowKnock, Settings.Sfx * 0.8f);
            yield return Wait(3f);
            if (Has("openedToMom")) yield break;
            flags.Remove("boltDanger");
            R.bolt.SetClosed(true); R.bolt.SetKeyLocked(true);
            P.Receive(PhoneSystem.Mama, MomCameras);
            GameClock.Set(0, 31);
            PowerPanel.I.fuseOK = false;
            SaveStage(Stage.C2_Camaras);
            yield return Wait(1.5f);
            yield return C2_CamerasStart(false);
        }

        /// <summary>Abrir a la "mamá": la puerta se abre despacio, no hay nadie... y aparece. Muerte justa: vuelves al despertar del cap. 2.</summary>
        IEnumerator C2_OpenDoorDeath()
        {
            PlayerController.CutsceneLock = true;
            if (Phone3D.I) Phone3D.I.Close();
            O.Clear();
            var door = R.doorFront;
            var outside = R.frontDoorOutside.transform.position;
            Vector3 doorway = door.transform.position + (outside - door.transform.position) * 0.35f;
            A.SetTension(0f);
            A.SetAmbience(0.05f);
            if (A.doorSlow != null) A.At(A.doorSlow, door.transform.position, 0.9f); else A.At(A.doorCreak, door.transform.position, 0.9f);
            // mirar hacia afuera: lluvia, la calle vacía
            for (float t = 0; t < 2.2f; t += Time.deltaTime) { R.player.LookAtPoint(outside + Vector3.up * 1.5f, Time.deltaTime * 2.5f); yield return null; }
            H.Say(You, T("¿Ma...? No hay nadie.", "Mom...? There's no one."), 2f);
            A.At(A.thunder, outside, 0.35f);
            for (float t = 0; t < 1.6f; t += Time.deltaTime) { R.player.LookAtPoint(outside + Vector3.up * 1.4f, Time.deltaTime * 2f); yield return null; }
            // la voz, ahora pegada a tu oído
            A.PlayVoiceExact(Voice.Mom, R.player.EyePosition + R.player.transform.right * 0.4f, 0.75f);
            H.Say(T("¿Mamá?", "Mom?"), T("Gracias por abrirme.", "Thank you for letting me in."), 1.6f);
            yield return VoiceBank.Wait(0.9f);
            PlayerController.CutsceneLock = false;
            GameManager.I.Die(T("\"Mamá estaba en el trabajo. Lo sabías.\"", "\"Mom was at work. You knew that.\""), outside + (doorway - outside) * 0.2f);
        }

        // ------------------------------------------------------------------ cámaras
        IEnumerator C2_CamerasStart(bool fromCheckpoint)
        {
            if (fromCheckpoint)
            {
                PlayerController.CutsceneLock = true;
                H.SetFade(1f);
                R.player.SetPose(R.poseSalaDoor, -1f);
                yield return H.FadeTo(0f, 1.5f);
                PlayerController.CutsceneLock = false;
            }
            SecurityCameraSystem.I.recordingMode = true;
            O.Set(T("Revisa las cámaras de seguridad", "Check the security cameras"),
                  T("El monitor está en el escritorio de la sala, junto a la pared.", "The monitor is on the living room desk, by the wall."),
                  T("Usa el monitor de la sala", "Use the living room monitor"));
        }

        void C2_CamEvent(string id)
        {
            if (id == "focus:cameras" && Set("camOpened")) StartCoroutine(C2_CameraDoubt());
            else if (id == "zone:bano" && Has("camDoubt") && Set("bathChecked"))
            {
                O.Check(0);
                H.Say(You, T("Nadie. Y la puerta estaba cerrada... la cámara mostraba otra cosa.", "No one. And the door was closed... the camera showed something else."), 4f);
            }
            else if (id == "camera_live_failed" && Set("needPower")) StartCoroutine(C2_PowerObjective());
            else if (id == "focus:breaker" && Has("needPower")) O.Check(0);
            else if (id == "collected:fuse") { O.Check(1); H.Hint(T("Fusible de repuesto obtenido.", "Spare fuse obtained."), 3f); H.Say(You, T("Un fusible F2. Ahora, al cuadro eléctrico.", "An F2 fuse. Now, the breaker panel."), 3f); }
            else if (id == "installed:fuse") O.Check(2);
            else if (id == "power_changed")
            {
                var p = PowerPanel.I;
                if (Has("needPower") && !p.On(Circuit.Alarm) && Set("alarmOffWarn"))
                    H.Say(You, T("La alarma se apagó. Mamá dijo que NUNCA la deje sin corriente.", "The alarm went off. Mom said NEVER leave it without power."), 3.5f);
                if (!p.On(Circuit.Lights) && Set("lightsOff")) StartCoroutine(C2_LightsOff());
                if (Has("needPower") && p.On(Circuit.Cameras) && p.On(Circuit.Alarm) && Set("powerOk"))
                {
                    O.Check(3);
                    StartCoroutine(C2_AfterPower());
                }
            }
            else if (id == "power_tripped" && Has("needPower"))
            {
                if (Set("tripHint")) H.Say(You, T("Saltó el diferencial. Demasiada carga: el cuadro aguanta 7.", "The breaker tripped. Too much load: the panel only holds 7."), 3.5f);
                if (Set("lightsOff")) StartCoroutine(C2_LightsOff());
            }
            else if (id == "camera_live" && Has("powerOk") && Set("ghost")) StartCoroutine(C2_GhostSequence());
        }

        IEnumerator C2_CameraDoubt()
        {
            yield return Wait(1.2f);
            R.cameraDesk.Select(1);
            yield return Wait(2.6f);
            H.Say(You, T("La puerta del baño está abierta... yo la dejé cerrada.", "The bathroom door is open... I left it closed."), 3.5f);
            yield return VoiceBank.Wait(2f);
            H.Say(You, T("Arriba dice REC. ¿Es una grabación?", "It says REC up there. Is this a recording?"), 3f);
            Set("camDoubt");
            yield return WaitFocusClosed();
            O.Set(T("Descubre por qué la cámara muestra algo distinto", "Find out why the camera shows something different"),
                  T("Mira la puerta del baño con tus propios ojos. Después prueba el botón ARCHIVO/DIRECTO del monitor.", "Look at the bathroom door with your own eyes. Then try the monitor's ARCHIVE/LIVE button."),
                  T("Revisa el baño", "Check the bathroom"), T("Pulsa ARCHIVO/DIRECTO en el monitor", "Press ARCHIVE/LIVE on the monitor"));
        }

        IEnumerator C2_PowerObjective()
        {
            yield return WaitFocusClosed();
            O.Set(T("Restaura la energía de las cámaras", "Restore power to the cameras"),
                  T("El fusible F2 está fundido. Papá dejó una nota junto al cuadro eléctrico de la cocina.", "Fuse F2 is blown. Dad left a note next to the kitchen breaker panel."),
                  T("Revisa el cuadro eléctrico (cocina)", "Check the breaker panel (kitchen)"),
                  T("Encuentra un fusible de repuesto", "Find a spare fuse"),
                  T("Cambia el fusible quemado F2 (clic sobre él)", "Replace the blown F2 fuse (click it)"),
                  T("Alimenta CÁMARAS sin pasar de 7 (la alarma debe seguir activa)", "Power CAMERAS within 7 (keep the alarm on)"));
            if (PowerPanel.I.fuseOK) { O.Check(1); O.Check(2); }
        }

        IEnumerator C2_LightsOff()
        {
            yield return WaitFocusClosed();
            yield return Wait(0.5f);
            if (!R.flashlight.HasFlashlight)
                H.Say(You, T("Se apagaron las luces... había una linterna en la encimera.", "The lights went out... there was a flashlight on the counter."), 3.5f);
        }

        IEnumerator C2_AfterPower()
        {
            yield return WaitFocusClosed();
            O.Set(T("Revisa la transmisión en directo", "Check the live feed"), T("Vuelve al monitor de la sala y pulsa ARCHIVO/DIRECTO.", "Go back to the living room monitor and press ARCHIVE/LIVE."),
                  T("Vuelve al monitor de la sala", "Return to the living room monitor"));
        }

        // ------------------------------------------------------------------ la figura en la cámara
        IEnumerator C2_GhostSequence()
        {
            O.Check(0);
            var desk = R.cameraDesk;
            desk.Select(1);
            yield return Wait(3f);
            // figura que SOLO existe en la cámara (capa GN_CamOnly)
            R.camGhost.gameObject.SetActive(true);
            R.camGhost.SetPositionAndRotation(R.ghostFar.position, R.ghostFar.rotation);
            A.UI(A.staticBurst, 0.4f);
            A.SetTension(0.5f);
            yield return Wait(2.8f);
            H.Say(You, T("Hay... ¿hay alguien en el pasillo?", "Is... is someone in the hallway?"), 3f);
            float t = 0f;
            while (FocusSystem.Active && t < 8f) { t += Time.deltaTime; yield return null; }
            if (!FocusSystem.Active)
            {
                R.camGhost.gameObject.SetActive(false);
                H.Hint(T("El pasillo está vacío.", "The hallway is empty."), 2.5f);
                while (!FocusSystem.Active) yield return null;
                yield return Wait(0.6f);
            }
            desk.Select(1);
            R.camGhost.gameObject.SetActive(true);
            R.camGhost.SetPositionAndRotation(R.ghostNear.position, R.ghostNear.rotation);
            A.UI(A.staticBurst, 0.6f);
            yield return Wait(2.4f);
            R.camGhost.gameObject.SetActive(false);
            desk.unavailable = true;
            A.UI(A.staticBurst, 0.8f);
            yield return Wait(1.6f);
            if (FocusSystem.Active) FocusSystem.I.Exit();
            desk.unavailable = false;
            yield return Wait(0.6f);
            // la puerta del baño se abre lentamente...
            R.doorBathroom.speed = 0.45f;
            R.doorBathroom.SetOpen(true);
            yield return Wait(1.2f);
            P.Receive(PhoneSystem.Unknown, T("No mires detrás de ti.", "Don't look behind you."), true, -1f, false);
            // SUSTO: está detrás de ti
            Vector3 behind = R.player.transform.position - R.player.transform.forward * 1.3f;
            E.ShowAt(behind, Quaternion.LookRotation(R.player.transform.position - behind).eulerAngles.y, "Stare");
            E.LookAtPlayer(1f);
            Vector3 headP = behind + Vector3.up * 1.75f;
            float w = 0f; bool looked = false;
            while (w < 7f) { w += Time.deltaTime; if (R.player.AngleTo(headP) < 35f) { looked = true; break; } yield return null; }
            if (looked)
            {
                A.UI(A.sting, 1f); A.UI(A.entityScream, 0.6f);
                CameraFX.Shake(0.6f); CameraFX.FovKick(-8f); PostFX.I.Shock(0.8f);
                yield return Wait(0.35f);
                LightingManager.I.Flicker(null, 0.8f);
                yield return Wait(0.3f);
            }
            else { A.At(A.whisper, headP, 0.8f); yield return Wait(1.2f); }
            E.Despawn();
            R.doorBathroom.speed = 2.6f;
            A.SetTension(0.3f);
            SaveStage(Stage.C2_Persecucion);
            yield return Wait(2.5f);
            yield return C2_Chase(false);
        }

        // ------------------------------------------------------------------ primera persecución
        IEnumerator C2_Chase(bool fromCheckpoint)
        {
            if (fromCheckpoint)
            {
                PlayerController.CutsceneLock = true;
                H.SetFade(1f);
                R.player.SetPose(R.poseSalaMonitor, -1f);
                yield return H.FadeTo(0f, 1.5f);
                PlayerController.CutsceneLock = false;
            }
            O.Set(T("Escóndete", "Hide"),
                  T("Armario del dormitorio, alacena de la cocina... Si te ve, corre y rompe la línea de visión.", "Bedroom wardrobe, kitchen pantry... If it sees you, run and break line of sight."),
                  T("Que no te encuentre", "Don't let it find you"));
            A.At(A.heavyStep1, R.spawnHallEnd.position, 0.9f);
            yield return Wait(1f);
            E.telegraph = true;
            E.OnCaught = () => GameManager.I.Die(T("\"No todas las voces quieren ayudarte.\"", "\"Not every voice wants to help you.\""));
            E.Spawn(R.spawnHallEnd.position, R.spawnHallEnd.eulerAngles.y, false, R.routeChase1, AIState.Patrol);
            E.patrolSpeed = 1.05f;
            A.SetTension(0.8f);
            H.Hint(T("Escóndete. Agachado haces menos ruido.", "Hide. Crouching makes less noise."), 5f);
            float time = 0f; bool breathDone = false;
            while (time < 70f || E.State == AIState.Chase || E.State == AIState.Notice || E.State == AIState.InspectHide)
            {
                if (stage != Stage.C2_Persecucion) yield break;
                time += Time.deltaTime;
                // si te escondes en el armario, se para frente a la puerta y respira... y se va
                if (!breathDone && HidingSpot.Current == R.wardrobeHide && E.State == AIState.Patrol && time > 25f)
                {
                    breathDone = true;
                    E.Investigate(R.wardrobeHide.outside.position);
                }
                yield return null;
            }
            bool returned = false;
            E.OnReturned = () => returned = true;
            E.ReturnTo(R.nodeBathroom);
            for (float rt = 0f; !returned && rt < 30f; rt += Time.deltaTime) yield return null;
            E.Despawn();
            A.SetTension(0f);
            yield return Wait(2f);
            R.doorBathroom.SetOpen(false);
            A.At(A.doorClose, R.doorBathroom.transform.position, 0.8f);
            yield return Wait(2f);
            P.Receive(PhoneSystem.Unknown, T("Bien. Ahora sabes que está dentro.", "Good. Now you know it's inside."));
            GameClock.Set(1, 4);
            SaveStage(Stage.C3_Cintas);
            yield return Wait(4f);
            yield return C3_Start(false);
        }
    }
}
