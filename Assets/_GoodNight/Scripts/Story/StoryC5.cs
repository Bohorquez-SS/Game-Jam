using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>CAPÍTULO 5 — ¿A QUIÉN DEJASTE SALIR? (02:13).
    /// 1) Con la entidad suelta, restaurar ALARMA + CÁMARAS + CERRADURAS (7/7, sin luces ni 06).
    /// 2) En la consola de aislamiento: sensores (uno es un ECO rítmico) y cámaras (la del dormitorio está CONGELADA: REC 23:46).
    /// 3) Aislar: COCINA → final A · 06 (tú mismo, si descubriste la verdad) → final B · DORMITORIO → muerte (aislaste una grabación).</summary>
    public partial class StoryManager
    {
        IEnumerator C5_Start(bool fromCheckpoint)
        {
            if (fromCheckpoint)
            {
                PlayerController.CutsceneLock = true;
                H.SetFade(1f);
                R.player.SetPose(R.poseSecret, -1f);
                LightingManager.I.SetEmergency(true);
                yield return H.FadeTo(0f, 1.5f);
                PlayerController.CutsceneLock = false;
            }
            GameClock.Set(2, 13);
            yield return H.ShowTitle(ChapterLabel, 2.5f);
            IsolationStation.Motion.Clear();
            R.isolation.selfOptionVisible = false; R.isolation.decisionReady = false;
            O.Set(T("Activa el protocolo de aislamiento de GUARDIÁN", "Activate GUARDIAN's isolation protocol"),
                  T("En el cuadro de la cocina: ALARMA + CÁMARAS + CERRADURAS (3+2+2 = 7). Luces y 06 apagados. El Sujeto 0 anda suelto: escucha sus pasos.", "At the kitchen panel: ALARM + CAMERAS + LOCKS (3+2+2 = 7). Lights and 06 off. Subject 0 is loose: listen to its steps."),
                  T("Activa el protocolo de aislamiento (cuadro de la cocina)", "Activate the isolation protocol (kitchen panel)"),
                  T("Regresa a la consola de la habitación 06", "Go back to the room 06 console"));
            yield return Wait(2f);
            // (18/10) un ruido al otro lado de la casa: la entidad empieza LEJOS (dormitorio) y patrulla solo la mitad oeste
            // (dormitorio · pasillo · sala · baño). El camino 06 → cocina queda libre si se espera el momento.
            A.At(A.metalBang != null ? A.metalBang : A.knock, R.spawnBedroom.position + Vector3.up * 1.2f, 0.75f, 25f);
            CameraFX.Shake(0.08f);
            yield return Wait(1.2f);
            H.Say(You, T("Eso fue en mi cuarto... Papá la llamaba SUJETO 0. La primera copia. La que salió mal.", "That was in my room... Dad called it SUBJECT 0. The first copy. The one that went wrong."), 5f);
            P.Receive(PhoneSystem.Guardian, T("SUJETO 0 · SIN IDENTIDAD · EN MOVIMIENTO\nZONA: DORMITORIO", "SUBJECT 0 · NO IDENTITY · MOVING\nZONE: BEDROOM"), true, -1f, false);
            yield return Wait(5f);
            if (Has("c5Power") || stage != Stage.C5_Final) yield break;   // ya restauraste la energía: no volver a soltarla
            E.telegraph = true;
            E.OnCaught = () => GameManager.I.Die(T("\"El Sujeto 0 no tiene cara. Buscaba la tuya.\"", "\"Subject 0 has no face. It was looking for yours.\""));
            E.Spawn(R.spawnBedroom.position, R.spawnBedroom.eulerAngles.y, false, new[] { 0, 1, 2, 7, 8, 9, 7, 2, 3, 6, 3, 2, 1 }, AIState.Patrol);
            E.patrolSpeed = 0.95f; E.chaseSpeed = 2.5f; E.sight = 8f; E.sightFlashlightOff = 5f; E.hearRun = 7f;
            E.loseTime = 3f; E.searchTime = 6f;   // te pierde antes y vuelve a patrullar: intenso pero justo
            A.SetTension(0.45f);
            yield return Wait(3f);
            H.Hint(T("Escucha sus pasos. Cuando se aleje hacia el oeste, ve a la cocina por el pasillo.", "Listen to its steps. When it moves away to the west, go to the kitchen through the hallway."), 6f);
            P.Receive(PhoneSystem.Unknown, T("Él también cree que es el verdadero.", "He also thinks he's the real one."));
        }

        // =====================================================================================
        //  (18/10) CAPÍTULO 5 — SECUENCIA DETERMINISTA
        //  C5_Final:    energía (cuadro de la cocina) con el Sujeto 0 suelto. NADA se cierra mientras estás en la cocina.
        //  → "Protocolo preparado": el Sujeto 0 se retira. Al volver a la 06 (y SOLO entonces, con el jugador lejos)
        //    GUARDIÁN pone la cocina en cuarentena: el doble está ahí y se ve por las cámaras. Checkpoint C5_Decision.
        //  C5_Decision: consola → revelación (dos identidades) → AISLAR AL DOBLE (final A) / AISLARME A MÍ MISMO (final B).
        //  Cada paso comprueba su bandera: ninguna corrutina vieja puede volver a cerrar puertas.
        // =====================================================================================
        void C5_Event(string id)
        {
            if (id.StartsWith("inspected:")) { C4_Event(id); return; }
            if (stage == Stage.C5_Final && id == "power_changed" && !Has("c5Power") && PowerPanel.I.SecurityComplete)
            {
                Set("c5Power");
                O.Check(0);
                StartCoroutine(C5_PowerRestored());
            }
            else if (id == "focus:isolation" && Has("c5Ready") && Set("c5Reveal")) StartCoroutine(C5_Reveal());
            else if (id.StartsWith("isolate:") && !Has("c5Ready"))
                H.Say("GUARDIÁN", T("Protocolo no preparado. Restaure ALARMA + CÁMARAS + CERRADURAS en el cuadro de la cocina.", "Protocol not ready. Restore ALARM + CAMERAS + LOCKS at the kitchen panel."), 3.5f);
            else if (id.StartsWith("isolate:") && Set("isolated"))
            {
                string z = id.Substring(8);
                O.Clear();
                R.isolation.decisionReady = false;
                if (z == "secreta") StartCoroutine(EndingManager.I.PlayB());   // aislarme a mí mismo
                else StartCoroutine(EndingManager.I.PlayA());                   // aislar al doble (cocina)
            }
        }

        IEnumerator C5_PowerRestored()
        {
            A.UI(A.check, 0.8f);
            A.PlayVoice(Voice.Guardian, null, 0.7f);
            H.Say("GUARDIÁN", T("Alarma, cámaras y cerraduras en línea. Protocolo de aislamiento: PREPARADO.", "Alarm, cameras and locks online. Isolation protocol: READY."), 4f);
            H.Hint(T("Protocolo preparado. Regresa a la consola de GUARDIÁN (habitación 06).", "Protocol ready. Go back to the GUARDIAN console (room 06)."), 6f);
            O.Set(T("Regresa a la consola de GUARDIÁN", "Go back to the GUARDIAN console"),
                  T("La consola está en la habitación 06 (pasillo, puerta metálica). El Sujeto 0 se está retirando.", "The console is in room 06 (hallway, metal door). Subject 0 is retreating."),
                  T("Activa el protocolo de aislamiento (cuadro de la cocina)", "Activate the isolation protocol (kitchen panel)"),
                  T("Regresa a la consola de la habitación 06", "Go back to the room 06 console"));
            O.Check(0);
            // el Sujeto 0 se retira YA (aunque te estuviera persiguiendo), lejos del camino a la 06
            if (E.gameObject.activeInHierarchy && E.State != AIState.Scripted)
            {
                E.OnCaught = null;
                bool returned = false;
                E.OnReturned = () => returned = true;
                E.ReturnTo(R.nodeBedroom);
                A.SetTension(0.2f);
                for (float t = 0f; !returned && t < 15f; t += Time.deltaTime) yield return null;
            }
            // esperar a que el jugador esté en la 06: la cuarentena de la cocina NUNCA ocurre con el jugador dentro
            while (stage == Stage.C5_Final && !InRoom06()) yield return null;
            if (stage != Stage.C5_Final || Has("isolated")) yield break;
            C5_SetupDecision();
            SaveStage(Stage.C5_Decision);
            yield return Wait(1.5f);
            H.Say(You, T("Las cámaras... hay alguien en la cocina.", "The cameras... there's someone in the kitchen."), 3.5f);
        }

        bool InRoom06()
        {
            var p = R.player.transform.position;
            return RoomZone.At(p) == "secreta" || (p.x > HouseZone06.X0 + 0.1f && p.x < HouseZone06.X1 && p.z > HouseZone06.Z0 + 0.35f && p.z < HouseZone06.Z1);
        }

        /// <summary>Estado de la decisión (también al cargar el checkpoint): cocina en cuarentena con el doble, cámaras en DIRECTO, consola lista.</summary>
        void C5_SetupDecision()
        {
            Set("c5Power"); Set("c5Ready");
            E.Despawn();
            R.doppelBedRoot.gameObject.SetActive(false);                 // nadie durmiendo en el dormitorio
            var bedCam = SecurityCameraSystem.I.ByZone("dormitorio");
            var kitchenCam = SecurityCameraSystem.I.ByZone("cocina");
            var hallCam = SecurityCameraSystem.I.ByZone("pasillo");
            if (bedCam) bedCam.frozen = false;                            // las cámaras muestran SOLO el presente
            // cuarentena: el jugador está en la 06, lejos de la cocina
            A.At(A.shutter, R.doorKitchen.transform.position, 0.8f);
            foreach (var sh in R.shuttersKitchen) sh.SetDown(true);
            R.doorKitchen.SetOpen(false, true);
            R.doorKitchen.locked = true;
            R.doorKitchen.lockedEs = "Cuarentena de GUARDIÁN. Del otro lado alguien respira... con tu misma respiración.";
            R.doorKitchen.lockedEn = "GUARDIAN quarantine. Someone's breathing on the other side... the same way you breathe.";
            E.ShowAt(R.spawnKitchen.position, R.spawnKitchen.eulerAngles.y, "LookAround", true);   // ÚNICA instancia del doble
            var an = E.Anim; if (an && kitchenCam) { an.lookTarget = kitchenCam.transform.position; an.lookWeight = 1f; }
            var feeds = new[] { kitchenCam, hallCam, bedCam };
            for (int i = 0; i < R.secretMonitors.Length; i++)
            {
                var m = R.secretMonitors[i];
                m.showStatic = false; m.message = null; m.overrideTexture = null; m.source = feeds[i % feeds.Length]; m.SetOn(true);
            }
            R.isolation.selfOptionVisible = true;
            R.isolation.decisionReady = true;
            O.Set(T("Decide qué identidad aislar", "Decide which identity to isolate"),
                  T("Usa la consola de GUARDIÁN. Solo una identidad puede salir de la casa.", "Use the GUARDIAN console. Only one identity can leave the house."),
                  T("Usa la consola de GUARDIÁN (habitación 06)", "Use the GUARDIAN console (room 06)"),
                  T("Decide qué identidad aislar", "Decide which identity to isolate"));
            StartCoroutine(C5_Sensors());
            StartCoroutine(C5_KitchenVoice());
            P.Receive(PhoneSystem.Unknown, T("Deja de fingir que eres yo.", "Stop pretending to be me."));
        }

        /// <summary>Checkpoint C5_Decision: empiezas en la 06 con el protocolo preparado.</summary>
        IEnumerator C5_DecisionStart()
        {
            PlayerController.CutsceneLock = true;
            H.SetFade(1f);
            R.player.SetPose(R.poseSecret, -1f);
            LightingManager.I.SetEmergency(true);
            C5_SetupDecision();
            yield return H.FadeTo(0f, 1.5f);
            PlayerController.CutsceneLock = false;
        }

        /// <summary>La revelación, la primera vez que usas la consola con el protocolo preparado.</summary>
        IEnumerator C5_Reveal()
        {
            A.SetAmbience(0.1f);
            yield return Wait(0.6f);
            A.UI(A.sting, 0.6f);
            A.PlayVoice(Voice.Guardian, null, 0.8f);
            H.Say("GUARDIÁN", T("Dos identidades coincidentes detectadas. Verificación de origen: fallida.", "Two matching identities detected. Origin verification: failed."), 4.5f);
            yield return Wait(4.8f);
            H.Say(You, T("Es igual a mí... Yo estaba aquí. Yo nunca salí. Entonces, ¿quién entró por la puerta?", "He looks just like me... I was here. I never left. Then who came in through the door?"), 5f);
            yield return Wait(5.3f);
            A.PlayVoice(Voice.Guardian, null, 0.8f);
            H.Say("GUARDIÁN", T("Protocolo de aislamiento requerido. Solo una identidad puede abandonar la casa. Advertencia: la decisión es irreversible.", "Isolation protocol required. Only one identity may leave the house. Warning: the decision is irreversible."), 5.5f);
            yield return Wait(5.8f);
            H.Say(You, T("¿Y si yo soy la copia? ¿Cómo sé que mis recuerdos son reales?", "What if I'm the copy? How do I know my memories are real?"), 4.5f);
            A.SetAmbience(0.6f);
        }

        /// <summary>Sensores: movimiento real (la entidad/el doble). Sin ecos falsos: solo el presente.</summary>
        IEnumerator C5_Sensors()
        {
            while ((stage == Stage.C5_Final || stage == Stage.C5_Decision) && !Has("isolated"))
            {
                IsolationStation.Motion.Clear();
                if (E.gameObject.activeInHierarchy && E.Zone != null) IsolationStation.Motion.Add(E.Zone);
                yield return null;
            }
            IsolationStation.Motion.Clear();
        }

        IEnumerator C5_KitchenVoice()
        {
            string[] es = { "¡Escúchame! ¡Yo soy el verdadero!", "¡Tú tienes mis recuerdos!", "¿Mamá? La puerta no abre... yo vivo aquí.", "¡No puedes encerrarme!", "¡Pregúntale a papá qué hizo!" };
            string[] en = { "Listen to me! I'm the real one!", "You have my memories!", "Mom? The door won't open... I live here.", "You can't lock me in!", "Ask dad what he did!" };
            int i = 0;
            yield return Wait(8f);
            while ((stage == Stage.C5_Final || stage == Stage.C5_Decision) && !Has("isolated"))
            {
                var p = R.spawnKitchen.position + Vector3.up * 1.6f;
                if (Vector3.Distance(R.player.transform.position, p) < 9f && !FocusSystem.Active)
                {
                    A.PlayVoice(Voice.You, p, 0.7f);
                    Knock(R.doorKitchen.transform.position, 0.6f);
                    H.Say(T("Una voz igual a la tuya", "A voice just like yours"), T(es[i % es.Length], en[i % en.Length]), 3.5f);
                    i++;
                }
                yield return Wait(Random.Range(12f, 18f));
            }
        }

        // =====================================================================================
        //  ANOMALÍAS (eventos menores de una sola vez, por zona)
        // =====================================================================================
        void RegisterAnomalies()
        {
            var hem = HorrorEventManager.I; if (hem == null) return;
            // la puerta del baño se cierra sola cuando pasas (cap. 3)
            hem.OnEnterZone("bathDoorSlam", "pasillo", () => stage == Stage.C3_Cintas && Has("got_tape_a") && R.doorBathroom.isOpen, () =>
            {
                R.doorBathroom.SetOpen(false);
                A.At(A.doorClose, R.doorBathroom.transform.position, 1f);
            });
            // susurro con tu nombre en la cocina (cap. 3)
            hem.OnEnterZone("kitchenWhisper", "cocina", () => stage == Stage.C3_Cintas, () =>
            {
                A.At(A.whisper, R.player.transform.position + R.player.transform.forward * -1.2f + Vector3.up * 1.5f, 0.6f);
            });
            // la linterna falla al pasar por el armario (cap. 4)
            hem.OnEnterZone("flashFail", "pasillo", () => stage == Stage.C4_Protocolo && Has("gotKey"), () =>
            {
                R.flashlight.Flicker(1.8f);
                A.At(A.breathClose, R.player.transform.position + Vector3.up * 1.6f - R.player.transform.forward, 0.4f);
            });
            // pasos encima del techo del dormitorio (cap. 4)
            hem.OnEnterZone("ceilingSteps", "dormitorio", () => stage == Stage.C4_Protocolo, () => StartCoroutine(CeilingSteps()));
            // la foto del pasillo está boca abajo (cap. 5)
            hem.OnEnterZone("hallPhoto", "pasillo", () => stage == Stage.C5_Final && R.photoHall != null, () =>
            {
                R.photoHall.transform.localRotation *= Quaternion.Euler(0, 0, 180f);
                R.photoHall.textEs = "Alguien la dio vuelta. Somos papá y yo... pero mi cara está rayada con uña.";
                R.photoHall.textEn = "Someone turned it upside down. It's dad and me... but my face has been scratched out.";
                A.At(A.woodCreak, R.photoHall.transform.position, 0.5f);
            });
        }

        IEnumerator CeilingSteps()
        {
            Vector3 c = R.player.transform.position + Vector3.up * 2.9f;
            for (int i = 0; i < 6; i++) { A.PlaySteps(c + new Vector3(i * 0.5f - 1.5f, 0, 0.3f * (i % 2)), 0.6f, true); yield return Wait(0.7f); }
        }
    }
}
