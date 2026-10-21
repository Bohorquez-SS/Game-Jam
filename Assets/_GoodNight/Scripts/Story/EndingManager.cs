using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Dos finales.
    /// A «Buenos días» — aíslas al doble; sales al amanecer y GUARDIÁN informa original contenido / reconstrucción liberada.
    /// B «Buenas noches» — te aíslas tú y sale la otra identidad; el análisis final de origen es inconcluso.
    /// </summary>
    public class EndingManager : MonoBehaviour
    {
        public static EndingManager I;
        bool playing;
        void Awake() { I = this; }

        static string T(string es, string en) => Settings.T(es, en);

        void Prepare()
        {
            PlayerController.CutsceneLock = true;
            PlayerController.CutsceneLockLook = true;
            if (Phone3D.I) Phone3D.I.Close();
            if (FocusSystem.Active) FocusSystem.I.Exit();
            ObjectiveManager.I.Clear();
            AudioManager.I.SetTension(0f);
        }

        IEnumerator CarArrives()
        {
            var r = GameRefs.I; var a = AudioManager.I;
            if (r.momCar == null || r.carStart == null || r.carStop == null) yield break;
            r.momCar.gameObject.SetActive(true);
            foreach (var l in r.carLights) if (l) l.enabled = true;
            a.At(a.carArrive, r.carStop.position, 1f, 40f);
            for (float t = 0; t < 1f; t += Time.deltaTime / 5f)
            {
                r.momCar.position = Vector3.Lerp(r.carStart.position, r.carStop.position, Mathf.SmoothStep(0, 1, t));
                r.momCar.rotation = Quaternion.Slerp(r.carStart.rotation, r.carStop.rotation, t);
                yield return null;
            }
            yield return new WaitForSeconds(0.8f);
            a.At(a.carDoor, r.carStop.position, 1f, 40f);
        }

        /// <summary>Muestra una cámara de seguridad a pantalla completa (corte de vigilancia).</summary>
        IEnumerator CamCut(SecurityCam c, float seconds, string banner)
        {
            if (c == null) yield break;
            c.frozen = false;
            var ui = UIManager.I; var h = HUDView.I;
            AudioManager.I.UI(AudioManager.I.staticBurst, 0.6f);
            h.SetFP(null);
            for (float t = 0; t < seconds; t += Time.deltaTime)
            {
                SecurityCameraSystem.I.Request(c);
                ui.SetWorldTexture(c.rt);
                if (t > 0.1f && banner != null) { h.Banner(banner, seconds - t); banner = null; }
                yield return null;
            }
        }

        // ------------------------------------------------------------------ FINAL A
        public IEnumerator PlayA()
        {
            if (playing) yield break;
            playing = true;
            Prepare();
            var r = GameRefs.I; var h = HUDView.I; var a = AudioManager.I; var e = r.enemy;
            a.PlayVoice(Voice.Guardian, null, 0.8f);
            h.Say("GUARDIÁN", T("Identidad secundaria seleccionada. Sellando la cocina.", "Secondary identity selected. Sealing the kitchen."), 3.5f);
            yield return VoiceBank.Wait(3.6f);
            a.At(a.metalBang, r.doorKitchen.transform.position, 1f);
            CameraFX.Shake(0.3f);
            yield return new WaitForSeconds(0.8f);
            a.PlayVoice(Voice.You, r.spawnKitchen.position + Vector3.up * 1.6f, 0.9f);
            h.Say(T("Una voz igual a la tuya", "A voice just like yours"), T("¡Espera! ¡TÚ NO ERES EL ORIGINAL!", "Wait! YOU'RE NOT THE ORIGINAL!"), 3.5f);
            for (int i = 0; i < 6; i++) { a.At(a.knock, r.doorKitchen.transform.position, 1f); yield return new WaitForSeconds(0.35f); }
            yield return new WaitForSeconds(1.2f);
            a.PlayVoice(Voice.Dad, null, 0.6f);
            h.Say(T("Papá (grabación)", "Dad (recording)"), T("Las copias conservan los recuerdos. Incluso recuerdan haber nacido.", "The copies keep the memories. They even remember being born."), 4.5f);
            yield return VoiceBank.Wait(4.8f);
            a.PlayVoice(Voice.Guardian, null, 0.8f);
            h.Say("GUARDIÁN", T("Identidad aislada. Salida autorizada.", "Identity isolated. Exit authorized."), 3.5f);
            yield return h.FadeTo(1f, 3f);
            GameClock.Set(6, 0);
            LightingManager.I.SetEmergency(false);
            LightingManager.I.SetMood(Mood.Dawn);
            r.bolt.SetGuardianLock(false); r.bolt.SetKeyLocked(false); r.bolt.SetClosed(false);
            yield return h.ShowTitle("06:00", 2f);
            // salir al amanecer
            r.doorFront.BeforeOpen = null; r.doorFront.locked = false; r.doorFront.speed = 0.9f;
            r.player.SetPose(r.poseExitStart, -1f);
            a.SetAmbience(0.25f); a.At(a.birds, r.poseExitEnd.position, 0.8f, 40f);
            yield return h.FadeTo(0f, 2f);
            r.doorFront.SetOpen(true);
            yield return Walk(r.poseExitStart.position, r.poseExitEnd.position, 6f);
            yield return CarArrives();
            yield return new WaitForSeconds(1f);
            a.PlayVoice(Voice.Mom, r.carStop.position + Vector3.up * 1.5f, 0.8f);
            h.Say(T("Mamá", "Mom"), T("¡Cariño! ¿Qué haces afuera? Estás helado... y pálido.", "Honey! What are you doing outside? You're freezing... and so pale."), 4f);
            yield return VoiceBank.Wait(4.5f);
            a.PlayVoice(Voice.Mom, r.carStop.position + Vector3.up * 1.5f, 0.8f);
            h.Say(T("Mamá", "Mom"), T("Vamos adentro. Te hago el desayuno.", "Let's go inside. I'll make you breakfast."), 3.5f);
            yield return VoiceBank.Wait(4f);
            // corte: la cámara de la cocina. El que quedó encerrado golpea y grita.
            e.ShowAt(r.spawnKitchen.position, r.spawnKitchen.eulerAngles.y, "Attack", true);
            var kc = SecurityCameraSystem.I.ByZone("cocina");
            if (kc) { var an = e.Anim; if (an) { an.lookTarget = kc.transform.position; an.lookWeight = 1f; } }
            StartCoroutine(TrappedVoice(r, h, a));
            yield return CamCut(kc, 7.5f, T("COCINA · SUJETO ORIGINAL: CONTENIDO · RECONSTRUCCIÓN: LIBERADA", "KITCHEN · ORIGINAL SUBJECT: CONTAINED · RECONSTRUCTION: RELEASED"));
            // vuelta al exterior: mamá te mira
            var mainCam = r.player.cam.GetComponent<Camera>();
            if (mainCam) UIManager.I.SetWorldTexture(mainCam.targetTexture);
            e.Despawn();
            a.SetAmbience(0.05f);
            yield return new WaitForSeconds(1.2f);
            a.PlayVoice(Voice.Mom, r.carStop.position + Vector3.up * 1.5f, 0.75f);
            h.Say(T("Mamá", "Mom"), T("¿Estás bien, cariño?", "Are you okay, sweetie?"), 3f);
            yield return VoiceBank.Wait(3.2f);
            h.Say(You, T("Sí, mamá.", "Yes, mom."), 2.2f);
            yield return VoiceBank.Wait(2.6f);
            // la sonrisa demasiado rígida: un instante de imagen "rota"
            if (PostFX.I) PostFX.I.Shock(0.35f);
            a.UI(a.staticBurst, 0.25f);
            h.Say(You, T("Ahora sí.", "Now I am."), 2.5f);
            yield return VoiceBank.Wait(2.8f);
            a.PlayVoice(Voice.Dad, null, 0.6f);
            h.Say(T("Papá (grabación)", "Dad (recording)"), T("El problema nunca fue que las copias olvidaran quiénes eran. El problema fue que aprendieron a recordar demasiado bien.", "The problem was never that the copies forgot who they were. The problem was that they learned to remember too well."), 6f);
            yield return VoiceBank.Wait(6.2f);
            yield return Final(T("<i>Encerraste al que llegó a las 22:10.</i>", "<i>You locked up the one who came home at 22:10.</i>"), T("FINAL A · BUENOS DÍAS", "ENDING A · GOOD MORNING"));
        }

        IEnumerator TrappedVoice(GameRefs r, HUDView h, AudioManager a)
        {
            yield return new WaitForSeconds(0.8f);
            for (int i = 0; i < 4; i++) { a.UI(a.knock, 0.6f); yield return new WaitForSeconds(0.3f); }
            a.PlayVoice(Voice.You, null, 0.8f);
            h.Say(T("Una voz igual a la tuya", "A voice just like yours"), T("¡Mamá! ¡No soy yo el que salió!", "Mom! I'm not the one who got out!"), 3f);
            yield return VoiceBank.Wait(3.3f);
            a.PlayVoice(Voice.Guardian, null, 0.8f);
            h.Say("GUARDIÁN", T("Protocolo completado. Sujeto original contenido. Copia liberada.", "Protocol complete. Original subject contained. Copy released."), 3.5f);
        }

        // ------------------------------------------------------------------ FINAL B
        public IEnumerator PlayB()
        {
            if (playing) yield break;
            playing = true;
            Prepare();
            var r = GameRefs.I; var h = HUDView.I; var a = AudioManager.I; var e = r.enemy;
            a.PlayVoice(Voice.Guardian, null, 0.8f);
            h.Say("GUARDIÁN", T("Autoaislamiento solicitado. Habitación 06: cámara de contención. ¿Confirma?", "Self-isolation requested. Room 06: containment chamber. Confirm?"), 4f);
            yield return VoiceBank.Wait(4.2f);
            h.Say(You, T("No puedo arriesgarme a soltar una copia. Confirmo.", "I can't risk letting a copy out. Confirmed."), 3f);
            yield return VoiceBank.Wait(1.5f);
            r.doorMetal.locked = false; r.doorMetal.speed = 1.2f;
            r.doorMetal.SetOpen(false);
            yield return new WaitForSeconds(0.9f);
            a.At(a.metalBang, r.doorMetal.transform.position, 1f);
            foreach (var s in r.shuttersSecret) s.SetDown(true);
            CameraFX.Shake(0.4f);
            r.doorMetal.locked = true;
            LightingManager.I.SetEmergency(false);
            yield return new WaitForSeconds(2f);
            // los monitores: la cocina se abre; sale la otra identidad
            var kc = SecurityCameraSystem.I.ByZone("cocina");
            foreach (var m in r.secretMonitors) { m.source = kc; m.showStatic = false; m.message = null; m.SetOn(true); }
            r.bolt.SetGuardianLock(false); r.bolt.SetKeyLocked(false); r.bolt.SetClosed(false);
            foreach (var s in r.shuttersKitchen) s.SetDown(false);
            a.At(a.shutter, r.doorKitchen.transform.position, 0.6f);
            r.doorKitchen.locked = false; r.doorKitchen.SetOpen(true);
            e.ShowAt(r.spawnKitchen.position, r.spawnKitchen.eulerAngles.y, "Idle", true);
            PlayerController.CutsceneLockLook = true;
            if (r.secretMonitors.Length > 0)
                for (float t = 0; t < 2f; t += Time.deltaTime) { r.player.LookAtPoint(r.secretMonitors[0].transform.position, Time.deltaTime * 3f); yield return null; }
            yield return new WaitForSeconds(1f);
            var an = e.Anim; if (an) an.Play("Walk");
            Vector3 from = r.spawnKitchen.position, to = r.doorKitchen.transform.position; to.y = from.y;
            for (float t = 0; t < 1f; t += Time.deltaTime / 4f)
            {
                e.transform.position = Vector3.Lerp(from, to, t);
                var d = to - from; d.y = 0; if (d.sqrMagnitude > 0.01f) e.transform.rotation = Quaternion.LookRotation(d);
                yield return null;
            }
            e.Despawn();
            yield return new WaitForSeconds(1.5f);
            a.PlayVoice(Voice.Guardian, null, 0.7f);
            h.Say("GUARDIÁN", T("Duplicado aislado. Ocupantes: uno. Gracias.", "Duplicate isolated. Occupants: one. Thank you."), 4f);
            yield return VoiceBank.Wait(4.5f);
            GameClock.Set(6, 0);
            LightingManager.I.SetMood(Mood.Dawn);
            yield return CarArrives();
            a.PlayVoice(Voice.Mom, null, 0.35f);
            h.Say(T("Mamá (afuera)", "Mom (outside)"), T("¡Cariño! ¿Qué haces despierto tan temprano?", "Honey! What are you doing up so early?"), 4f);
            yield return VoiceBank.Wait(4.5f);
            // el último mensaje
            PhoneSystem.I.Outgoing(PhoneSystem.Mama, GameClock.Text, T("Cuídalo, ma.", "Take care of him, mom."));
            a.UI(a.messagePop, 0.6f);
            yield return new WaitForSeconds(2.5f);
            // el análisis que llega tarde
            a.SetAmbience(0f);
            yield return new WaitForSeconds(1f);
            a.UI(a.sting, 0.55f);
            foreach (var m in r.secretMonitors) { m.overrideTexture = null; m.source = null; m.showStatic = false; m.message = T("ANÁLISIS COMPLETADO\n<size=16>IDENTIDAD ORIGINAL: NO CONCLUYENTE\nAMBOS SUJETOS PRESENTAN\nRECUERDOS AUTÉNTICOS</size>", "ANALYSIS COMPLETE\n<size=16>ORIGINAL IDENTITY: INCONCLUSIVE\nBOTH SUBJECTS SHOW\nAUTHENTIC MEMORIES</size>"); m.SetOn(true); }
            a.PlayVoice(Voice.Guardian, null, 0.8f);
            h.Say("GUARDIÁN", T("Análisis completado. Identidad original: no concluyente. Ambos sujetos presentan recuerdos auténticos.", "Analysis complete. Original identity: inconclusive. Both subjects show authentic memories."), 5f);
            yield return VoiceBank.Wait(5.3f);
            a.PlayVoice(Voice.Dad, null, 0.6f);
            h.Say(T("Papá (grabación)", "Dad (recording)"), T("Si ambos recuerdan haber vivido la misma vida... ¿cuál de los dos tiene derecho a conservarla?", "If both remember living the same life... which of them has the right to keep it?"), 5.5f);
            yield return VoiceBank.Wait(5.8f);
            a.PlayVoice(Voice.You, r.doorMetal.transform.position + Vector3.up * 1.5f, 0.5f);
            h.Say(T("Tu propia voz, del otro lado de la puerta", "Your own voice, behind the door"), T("Buenas noches.", "Good night."), 3f);
            yield return VoiceBank.Wait(3f);
            foreach (var m in r.secretMonitors) m.SetOn(false);
            yield return Final(T("<i>Buenas noches.</i>", "<i>Good night.</i>"), T("FINAL B · BUENAS NOCHES", "ENDING B · GOOD NIGHT"));
        }

        static string You => T("Tú", "You");

        IEnumerator Walk(Vector3 from, Vector3 to, float seconds)
        {
            var r = GameRefs.I;
            for (float t = 0; t < 1f; t += Time.deltaTime / seconds)
            {
                r.player.transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0, 1, t));
                r.player.LookAtPoint(to + (to - from).normalized * 10f + Vector3.up * 1.4f, Time.deltaTime * 2f);
                yield return null;
            }
        }

        IEnumerator Final(string lastLine, string endingName)
        {
            var h = HUDView.I;
            yield return h.FadeTo(1f, 2.5f);
            yield return new WaitForSeconds(1f);
            h.Say("", lastLine, 4f);
            yield return VoiceBank.Wait(4.5f);
            yield return h.ShowTitle("GOOD NIGHT", 3f);
            int truths = StoryManager.I != null ? StoryManager.I.TruthCount : 0;
            yield return h.ShowTitle(endingName + "\n<size=60%>" + T("Verdades descubiertas: ", "Truths discovered: ") + truths + "/4</size>", 4f);
            Checkpoint.ClearSave();
            PlayerPrefs.DeleteKey("gn_flags");
            UIManager.I.Get<CreditsPanel>().Roll();
        }
    }
}
