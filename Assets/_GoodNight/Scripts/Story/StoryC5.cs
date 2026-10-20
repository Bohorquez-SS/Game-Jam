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
            R.isolation.selfOptionVisible = false;
            O.Set(T("Aísla al duplicado", "Isolate the duplicate"),
                  T("GUARDIÁN necesita ALARMA + CÁMARAS + CERRADURAS (3+2+2 = 7). Luces y 06 tendrán que quedarse apagados.", "GUARDIAN needs ALARM + CAMERAS + LOCKS (3+2+2 = 7). Lights and 06 must stay off."),
                  T("Restaura alarma, cámaras y cerraduras en el cuadro", "Restore alarm, cameras and locks at the panel"),
                  T("Vuelve a la consola de la habitación 06", "Return to the room 06 console"),
                  T("Decide a quién aislar", "Decide who to isolate"));
            yield return Wait(2f);
            // la entidad anda suelta mientras la casa está a oscuras
            E.telegraph = true;
            E.OnCaught = () => GameManager.I.Die(T("\"En la oscuridad, las dos caras eran la misma.\"", "\"In the dark, both faces were the same.\""));
            E.Spawn(R.spawnHallEnd.position, R.spawnHallEnd.eulerAngles.y, false, R.routeFinal, AIState.Patrol);
            E.patrolSpeed = 1.1f;
            A.SetTension(0.6f);
            yield return Wait(3f);
            P.Receive(PhoneSystem.Unknown, T("Él también cree que es el verdadero.", "He also thinks he's the real one."));
        }

        void C5_Event(string id)
        {
            if (id.StartsWith("inspected:")) { C4_Event(id); return; }
            if (id == "power_changed" && !Has("c5Power") && PowerPanel.I.SecurityComplete)
            {
                Set("c5Power");
                O.Check(0);
                StartCoroutine(C5_PowerRestored());
            }
            else if (id == "zone:secreta" && Has("c5Ready") && Set("c5Back")) O.Check(1);
            else if (id.StartsWith("isolate:") && !Has("c5Ready"))
                H.Say("GUARDIÁN", T("Consola iniciando. Espere.", "Console starting. Please wait."), 2.5f);
            else if (id.StartsWith("isolate:") && Set("isolated"))
            {
                string z = id.Substring(8);
                O.Check(1); O.Check(2);
                if (z == "cocina") StartCoroutine(EndingManager.I.PlayA());
                else if (z == "secreta") StartCoroutine(EndingManager.I.PlayB());
                else StartCoroutine(C5_WrongIsolation());
            }
        }

        IEnumerator C5_PowerRestored()
        {
            A.UI(A.check, 0.8f);
            A.PlayVoice(Voice.Guardian, null, 0.7f);
            H.Say("GUARDIÁN", T("Sistemas de seguridad en línea. Consola de aislamiento disponible.", "Security systems online. Isolation console available."), 4f);
            // la entidad se retira cuando deja de perseguirte
            for (float t = 0f; t < 25f && (E.State == AIState.Chase || E.State == AIState.Notice || E.State == AIState.InspectHide); t += Time.deltaTime) yield return null;
            bool returned = false;
            E.OnReturned = () => returned = true;
            E.ReturnTo(R.nodeHallEnd);
            for (float t = 0f; !returned && t < 20f; t += Time.deltaTime) yield return null;
            E.Despawn();
            A.SetTension(0.25f);

            // cámara del dormitorio: congelada en la imagen de las 23:46 (la cama ocupada)
            var bedCam = SecurityCameraSystem.I.ByZone("dormitorio");
            var kitchenCam = SecurityCameraSystem.I.ByZone("cocina");
            var hallCam = SecurityCameraSystem.I.ByZone("pasillo");
            if (bedCam)
            {
                R.doppelBedRoot.gameObject.SetActive(true);
                R.doppelBed.Play("LieIdle", 0f);
                yield return null;
                SecurityCameraSystem.I.Snapshot(bedCam);
                R.doppelBedRoot.gameObject.SetActive(false);
                bedCam.frozen = true; bedCam.frozenStamp = "23:46";
            }
            // el otro está en la cocina: GUARDIÁN baja las persianas y la sella
            A.At(A.shutter, R.doorKitchen.transform.position, 1f);
            foreach (var s in R.shuttersKitchen) s.SetDown(true);
            R.doorKitchen.SetOpen(false);
            R.doorKitchen.locked = true;
            R.doorKitchen.lockedEs = "No abre. GUARDIÁN la bloqueó. Del otro lado alguien respira.";
            R.doorKitchen.lockedEn = "It won't open. GUARDIAN locked it. Someone's breathing on the other side.";
            E.ShowAt(R.spawnKitchen.position, R.spawnKitchen.eulerAngles.y, "LookAround", true);
            var feeds = new[] { kitchenCam, bedCam, hallCam };
            for (int i = 0; i < R.secretMonitors.Length; i++)
            {
                var m = R.secretMonitors[i];
                m.showStatic = false; m.message = null; m.overrideTexture = null; m.source = feeds[i % feeds.Length]; m.SetOn(true);
            }
            R.isolation.selfOptionVisible = TruthCount >= 3;
            Set("c5Ready");
            StartCoroutine(C5_Sensors());
            StartCoroutine(C5_KitchenVoice());
            if (RoomZone.At(R.player.transform.position) == "secreta" && Set("c5Back")) O.Check(1);
            if (R.isolation.selfOptionVisible)
                StartCoroutine(SayLater(6f, You, T("Hay un tercer botón en la consola... AISLAR 06. Aquí dentro. Donde estoy yo.", "There's a third button on the console... ISOLATE 06. In here. Where I am."), 5f));
        }

        /// <summary>Sensores: la cocina marca movimiento real y estable; el dormitorio parpadea a ritmo fijo (eco).</summary>
        IEnumerator C5_Sensors()
        {
            float t = 0f;
            while (stage == Stage.C5_Final && !Has("isolated"))
            {
                t += Time.deltaTime;
                IsolationStation.Motion.Clear();
                if (E.gameObject.activeInHierarchy) IsolationStation.Motion.Add(E.Zone);
                if (t % 2f < 0.35f) IsolationStation.Motion.Add("dormitorio");
                yield return null;
            }
            IsolationStation.Motion.Clear();
        }

        IEnumerator C5_KitchenVoice()
        {
            string[] es = { "¿Hay alguien? La puerta no abre.", "¿Mamá? ¿Ya llegaste?", "Escucho a alguien en el pasillo... ¿quién eres?", "Por favor. Yo vivo aquí." };
            string[] en = { "Is someone there? The door won't open.", "Mom? Are you home?", "I hear someone in the hallway... who are you?", "Please. I live here." };
            int i = 0;
            yield return Wait(8f);
            while (stage == Stage.C5_Final && !Has("isolated"))
            {
                var p = R.spawnKitchen.position + Vector3.up * 1.6f;
                if (Vector3.Distance(R.player.transform.position, p) < 9f)
                {
                    A.PlayVoice(Voice.You, p, 0.7f);
                    Knock(R.doorKitchen.transform.position, 0.6f);
                    H.Say(T("Una voz igual a la tuya", "A voice just like yours"), T(es[i % es.Length], en[i % en.Length]), 3.5f);
                    i++;
                }
                yield return Wait(Random.Range(12f, 18f));
            }
        }

        IEnumerator C5_WrongIsolation()
        {
            A.At(A.metalBang, R.doorBedroom.transform.position, 1f);
            foreach (var s in R.shuttersBedroom) s.SetDown(true);
            yield return Wait(2f);
            A.PlayVoice(Voice.Guardian, null, 0.8f);
            H.Say("GUARDIÁN", T("Dormitorio aislado. Ocupantes en la zona: cero.", "Bedroom isolated. Occupants in zone: zero."), 4f);
            yield return Wait(4f);
            H.Say(You, T("Cero... era una grabación.", "Zero... it was a recording."), 3f);
            LightingManager.I.SetEmergency(false);
            R.flashlight.Flicker(3f);
            yield return Wait(2.5f);
            Vector3 sp = R.doorMetal.transform.position; sp.y = R.player.transform.position.y;
            E.Spawn(sp, Quaternion.LookRotation(R.player.transform.position - sp).eulerAngles.y, false, R.routeFinal, AIState.Patrol);
            E.ForceChase();
            E.OnCaught = () => GameManager.I.Die(T("\"Aislaste un recuerdo. Lo real seguía suelto.\"", "\"You isolated a memory. The real thing was still loose.\""));
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
