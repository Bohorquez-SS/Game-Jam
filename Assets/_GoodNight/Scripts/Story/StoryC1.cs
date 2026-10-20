using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>CAPÍTULO 1 — UNA NOCHE NORMAL (23:47).
    /// Apagar la PC · cerrar la puerta principal · cerrar 3 ventanas (golpes en el vidrio) · alarma con PIN del recibo
    /// (OCUPANTES: 2) · espejo del baño con retraso (opcional) · figura al fondo del pasillo · dormir.</summary>
    public partial class StoryManager
    {
        int mirrorPhase; float mirrorT;
        /// <summary>El lavabo solo funciona una vez, en el cap. 1, después de activar la alarma.</summary>
        public bool WashAvailable => stage == Stage.C1_Noche && Has("sleepReady") && !Has("washed") && !PlayerController.CutsceneLock;

        IEnumerator C1_Intro()
        {
            PlayerController.CutsceneLock = true;
            H.SetFade(1f);
            R.player.SetPose(R.poseDeskSeated, 1.12f);
            GameClock.Set(23, 47);
            R.computer.SetOn(true);
            A.SetAmbience(1f);
            yield return Wait(0.6f);
            yield return H.FadeTo(0f, 2.2f);
            yield return H.ShowTitle(ChapterLabel, 2.5f);
            yield return Wait(0.8f);
            P.Receive(PhoneSystem.Mama, MomIntro);
            yield return Wait(2.5f);
            H.Say(You, T("Turno de noche otra vez... Bueno. Cerrar todo y a dormir.", "Night shift again... Fine. Lock everything up and go to sleep."), 4f);
            yield return Wait(1.2f);
            // levantarse de la silla
            yield return R.player.MoveCamera(R.poseDeskStand.position, R.poseDeskStand.eulerAngles.y, 0f, R.player.standEye, 0f, 1.0f);
            R.player.ReleaseEyeOverride();
            PlayerController.CutsceneLock = false;
            H.Hint(T("[Tab] Teléfono · [E] Usar · [Shift] Correr · [Ctrl] Agacharse", "[Tab] Phone · [E] Use · [Shift] Run · [Ctrl] Crouch"), 7f);
            O.Set(T("Asegura la casa antes de dormir", "Lock up the house before bed"),
                  T("La puerta principal está en la sala. Hay ventanas en tu cuarto, la sala y la cocina.", "The front door is in the living room. There are windows in your room, the living room and the kitchen."),
                  T("Apaga la computadora", "Turn off the computer"), T("Cierra la puerta principal", "Close the front door"), WindowsText());
            Set("c1Objective");
            C1_Refresh();
        }

        int WindowsClosed() { int n = 0; foreach (var w in R.windows) if (!w.isOpen) n++; return n; }
        string WindowsText() => T("Cierra las ventanas", "Close the windows") + " (" + WindowsClosed() + "/" + R.windows.Length + ")";

        void C1_Event(string id)
        {
            if (Has("houseSecured")) { C1_LateEvent(id); return; }
            if (id == "door_closed:front" && Set("frontKey")) { R.bolt.SetKeyLocked(true); A.At(A.lockClunk, R.doorFront.transform.position, 0.8f); }
            if (id.StartsWith("window_closed:") && WindowsClosed() == R.windows.Length && Set("windowKnock"))
            {
                int last = 0;
                for (int i = 0; i < R.windows.Length; i++) if (id == "window_closed:" + R.windows[i].id) last = i;
                StartCoroutine(C1_WindowKnock(last));
            }
            if (id == "pc_off" || id == "pc_on" || id.StartsWith("door_") || id.StartsWith("window_")) C1_Refresh();
        }

        /// <summary>El objetivo refleja SIEMPRE el estado real de la casa (no depende del orden de los eventos).</summary>
        void C1_Refresh()
        {
            if (!Has("c1Objective") || Has("houseSecured")) return;
            bool pc = !R.computer.on, door = !R.doorFront.isOpen, wins = WindowsClosed() == R.windows.Length;
            O.Check(0, pc); O.Check(1, door);
            O.SetItem(2, WindowsText()); O.Check(2, wins);
            if (pc && door && wins) { Set("houseSecured"); StartCoroutine(C1_AlarmObjective()); }
        }

        void C1_LateEvent(string id)
        {
            if (id == "focus:alarm" && Has("alarmMsg") && Set("alarmHint") && !Inventory.Has("receipt") && !JournalManager.I.HasClue("pin"))
                H.Say(You, T("¿El día en que cambiamos la cerradura...? Mamá guarda todos los recibos en la cómoda de la sala.", "The day we changed the lock...? Mom keeps every receipt in the living room dresser."), 4.5f);
            else if (id == "inspected:receipt" || id == "inspect_back:receipt")
            {
                if (id == "inspect_back:receipt" && Set("pinRead")) O.SetItem(0, T("Activa la alarma (PIN: 2308)", "Arm the alarm (PIN: 2308)"));
            }
            else if (id == "alarm_wrong" && Set("wrongOnce"))
                H.Say(You, T("No es ese. «El día en que cambiamos la cerradura»...", "That's not it. \"The day we changed the lock\"..."), 3f);
            else if (id == "alarm_armed" && Set("armed")) StartCoroutine(C1_Armed());
            else if (id == "bed_sleep" && CanSleep && Set("slept")) StartCoroutine(C1_Sleep());
            else if (id == "wash_face" && Has("sleepReady") && Set("washed")) StartCoroutine(C1_WashFace());
            else if (id == "door_opened:front" && Has("armed") && !Has("slept"))
                H.Say(You, T("Mejor no. La alarma ya está activada.", "Better not. The alarm is already armed."), 2.5f);
        }

        IEnumerator C1_WindowKnock(int lastIndex)
        {
            yield return Wait(1.2f);
            var src = R.windowOutside[lastIndex];
            src.PlayOneShot(A.windowKnock, Settings.Sfx);
            CameraFX.Shake(0.05f);
            yield return Wait(1.4f);
            H.Say(You, T("¿Hay alguien...? Es el viento. Tiene que ser el viento.", "Is someone...? It's the wind. It has to be the wind."), 3.5f);
            var win = R.windows[lastIndex].transform;
            // un instante: algo pálido detrás del vidrio del dormitorio
            if (lastIndex == 0 && R.windowPeek != null && R.player.AngleTo(win.position + Vector3.up * 1.4f) < 30f)
            {
                E.ShowAt(R.windowPeek.position, R.windowPeek.eulerAngles.y, "Peek");
                E.LookAtPlayer(1f);
                yield return Wait(0.5f);
                LightingManager.I.Flicker("dormitorio", 0.4f);
                yield return Wait(0.3f);
                E.Despawn();
            }
            yield return Wait(3.5f);
            int other = (lastIndex + 1) % R.windowOutside.Length;
            R.windowOutside[other].PlayOneShot(A.windowKnock, Settings.Sfx * 0.9f);
        }

        IEnumerator C1_AlarmObjective()
        {
            yield return Wait(1.5f);
            P.Receive(PhoneSystem.Mama, MomAlarm);
            Set("alarmMsg");
            O.Set(T("Activa la alarma", "Arm the alarm"),
                  T("El panel está junto a la puerta principal. El código es el día y el mes en que cambiaron la cerradura: busca el recibo.", "The panel is next to the front door. The code is the day and month the lock was changed: look for the receipt."),
                  T("Activa la alarma", "Arm the alarm"));
        }

        IEnumerator C1_Armed()
        {
            O.Check(0);
            A.UI(A.beep, 0.8f);
            yield return Wait(1.6f);
            H.Say(You, T("«Ocupantes: 2»... ¿dos? Estoy solo.", "\"Occupants: 2\"... two? I'm alone."), 4f);
            Set("truth_alarm");
            JournalManager.I.AddClue("occupants", T("La alarma contó 2 ocupantes. Estoy solo.", "The alarm counted 2 occupants. I'm alone."));
            yield return Wait(4f);
            H.Say(You, T("Seguro está mal calibrada. Papá decía que se equivocaba con los gatos.", "It must be miscalibrated. Dad used to say it got confused by cats."), 4f);
            yield return Wait(3f);
            P.Receive(PhoneSystem.Mama, MomGoodnight);
            GameClock.Set(0, 4);
            Set("sleepReady");
            O.Set(T("Prepárate para dormir", "Get ready for bed"), T("El baño está en el pasillo. Tu cama, en el dormitorio.", "The bathroom is in the hallway. Your bed is in the bedroom."),
                  T("Lávate la cara en el lavabo del baño (opcional)", "Wash your face at the bathroom sink (optional)"), T("Acuéstate", "Lie down"));
            HorrorEventManager.I.OnEnterZone("hallFigure", "pasillo", () => stage == Stage.C1_Noche && Has("sleepReady"), () => StartCoroutine(C1_HallFigure()));
            if (HorrorEventManager.I.PlayerZone == "pasillo") StartCoroutine(C1_HallFigure());
        }

        IEnumerator C1_HallFigure()
        {
            if (!Set("hallFigure")) yield break;
            // la figura aparece al fondo del pasillo, la luz parpadea y desaparece
            var p = R.hallFigure;
            yield return WaitLookAt(p.position + Vector3.up * 1.4f, 40f, 12f);
            if (R.player.AngleTo(p.position + Vector3.up * 1.4f) > 40f) { yield return C1_VanishMessage(); yield break; }
            E.ShowAt(p.position, p.eulerAngles.y, "Stare");
            E.LookAtPlayer(1f);
            A.SetTension(0.6f);
            yield return Wait(1.2f);
            LightingManager.I.Flicker("pasillo", 1.4f);
            A.At(A.staticBurst, p.position, 0.5f);
            yield return Wait(0.9f);
            E.Despawn();
            A.SetTension(0f);
            yield return Wait(1f);
            yield return C1_VanishMessage();
        }

        IEnumerator C1_VanishMessage()
        {
            yield return Wait(2f);
            P.Receive(PhoneSystem.Unknown, T("No era nada. Ve a dormir.", "It was nothing. Go to sleep."), true, 7f);
            yield return Wait(3f);
            H.Say(You, T("¿Quién...? El mensaje se borró solo.", "Who...? The message deleted itself."), 3.5f);
        }

        void C1_Tick()
        {
            // espejo del baño: el reflejo se mueve con retraso... y luego te sigue mirando
            if (R.mirror == null || Has("mirrorDone")) return;
            var pc = R.player;
            if (RoomZone.At(pc.transform.position) != "bano") { if (mirrorPhase == 1) { mirrorPhase = 0; R.mirror.lag = 0f; } return; }
            Vector3 mp = R.mirror.surface.transform.position;
            bool looking = pc.AngleTo(mp) < 22f && Vector3.Distance(pc.EyePosition, mp) < 2.2f;
            mirrorT += Time.deltaTime;
            switch (mirrorPhase)
            {
                case 0: if (looking) { mirrorPhase = 1; mirrorT = 0f; R.mirror.lag = 0.45f; } break;
                case 1: if (mirrorT > 5f) { mirrorPhase = 2; mirrorT = 0f; } break;
                case 2:
                    if (pc.AngleTo(mp) > 110f) { R.mirror.stare = true; R.mirror.lag = 3f; mirrorPhase = 3; mirrorT = 0f; A.At(A.whisper, mp, 0.4f); }
                    break;
                case 3:
                    if (looking && mirrorT > 0.6f)
                    {
                        Set("truth_mirror"); Set("mirrorDone");
                        StartCoroutine(C1_MirrorEnd());
                    }
                    break;
            }
        }

        /// <summary>
        /// Lavarse la cara: el grifo, agua en la cara (cámara baja), silencio... al levantar la vista, en el espejo hay
        /// alguien detrás de ti. Golpe de sonido, sacudida, se abalanza. Te das vuelta: nadie.
        /// </summary>
        IEnumerator C1_WashFace()
        {
            var basin = R.mirror != null ? R.mirror.basin : null;
            if (basin == null || basin.standPose == null) yield break;
            PlayerController.CutsceneLock = true;
            if (Phone3D.I) Phone3D.I.Close();
            var pose = basin.standPose; float yaw = pose.eulerAngles.y; float eye = R.player.standEye;
            Vector3 tapPos = basin.tap != null ? basin.tap.position : pose.position + Vector3.up;
            R.mirror.lag = 0f; R.mirror.stare = false;
            yield return R.player.MoveCamera(pose.position, yaw, 12f, eye, 0f, 0.8f);
            // grifo
            AudioSource water = A.At(A.water != null ? A.water : A.tapeHiss, tapPos, 0.55f, 8f, 1f);
            yield return Wait(0.5f);
            // inclinarse sobre el lavabo
            yield return R.player.MoveCamera(pose.position + pose.forward * 0.12f, yaw, 62f, eye - 0.32f, 0f, 0.7f);
            H.Say("", T("(Agua helada en la cara...)", "(Ice-cold water on your face...)"), 2.2f);
            yield return H.Blink(1);
            yield return Wait(1.4f);
            // el agua se corta sola. Silencio.
            if (water) water.Stop();
            A.SetAmbience(0.1f);
            yield return Wait(1.1f);
            // la figura se coloca DETRÁS del jugador (solo visible en el reflejo)
            var sc = R.mirror.scare;
            Vector3 back = -pose.forward; back.y = 0; back.Normalize();
            Vector3 from = pose.position + back * 1.05f, to = pose.position + back * 0.42f;
            if (sc)
            {
                sc.position = from; sc.rotation = Quaternion.LookRotation(-back, Vector3.up);
                sc.gameObject.SetActive(true);
                if (R.mirror.scareAnim) { R.mirror.scareAnim.Play("Stare", 0f); R.mirror.scareAnim.lookTarget = R.player.EyePosition; R.mirror.scareAnim.lookWeight = 1f; }
            }
            // la luz del baño se enciende para que el espejo se vea normal al levantar la vista
            LightingManager.I.SetZone("bano", true);
            // levantar la vista despacio hacia el espejo: un instante, todo normal...
            yield return R.player.MoveCamera(pose.position, yaw, 4f, eye, 0f, 1.3f);
            yield return Wait(0.7f);
            // GOLPE: el rostro aparece pegado al espejo, se abalanza hacia ti
            var face = R.mirror.scareFace;
            A.UI(A.scareMirror != null ? A.scareMirror : (A.scareHit != null ? A.scareHit : A.sting), 0.9f);
            CameraFX.Shake(0.5f); CameraFX.FovKick(-10f);
            if (PostFX.I) PostFX.I.Shock(0.6f);
            if (R.mirror.scareAnim) R.mirror.scareAnim.Play("Attack", 0.05f, 1.3f, true);
            Vector3 fBase = face ? face.localPosition : Vector3.zero, fScale = face ? face.localScale : Vector3.one;
            if (face) face.gameObject.SetActive(true);
            for (float t = 0; t < 0.9f; t += Time.deltaTime)
            {
                float k = Ease.Out(Mathf.Clamp01(t / 0.35f));
                if (face)
                {
                    face.localScale = fScale * Mathf.Lerp(0.8f, 1.45f, k);
                    face.position = face.parent.TransformPoint(fBase) + pose.forward * (-0.25f * k) + Random.insideUnitSphere * 0.012f;
                }
                if (sc) sc.position = Vector3.Lerp(from, to, k);
                yield return null;
            }
            if (face) { face.gameObject.SetActive(false); face.localScale = fScale; face.localPosition = fBase; }
            LightingManager.I.Flicker("bano", 1.2f);
            if (sc) sc.gameObject.SetActive(false);
            // girarse de golpe: no hay nadie
            yield return R.player.MoveCamera(pose.position, yaw + 180f, 0f, eye, 0f, 0.4f);
            A.SetAmbience(1f);
            yield return Wait(0.5f);
            H.Say(You, T("¿Qué demonios fue eso?", "What the hell was that?"), 3.5f);
            R.player.ReleaseEyeOverride();
            PlayerController.CutsceneLock = false;
            O.Check(0);
            // el reflejo sigue "raro": al volver a mirarlo, se queda mirándote (verdad del espejo)
            if (!Has("mirrorDone")) { mirrorPhase = 2; mirrorT = 0f; R.mirror.lag = 0.45f; }
            JournalManager.I.AddClue("wash", T("Mientras me lavaba la cara vi a alguien detrás de mí en el espejo. Al girarme no había nadie.", "While washing my face I saw someone behind me in the mirror. When I turned around there was no one."));
        }

        IEnumerator C1_MirrorEnd()
        {
            A.UI(A.sting, 0.7f);
            CameraFX.Shake(0.15f);
            LightingManager.I.Flicker("bano", 0.6f);
            yield return Wait(0.6f);
            R.mirror.stare = false; R.mirror.lag = 0f;
            H.Say(You, T("Mi reflejo... tardó en moverse. Y me seguía mirando.", "My reflection... moved late. And it kept staring at me."), 4f);
            JournalManager.I.AddClue("mirror", T("Mi reflejo en el espejo del baño se movió tarde... y siguió mirándome.", "My reflection in the bathroom mirror moved late... and kept staring."));
        }

        IEnumerator C1_Sleep()
        {
            PlayerController.CutsceneLock = true;
            O.Clear();
            if (Phone3D.I) Phone3D.I.Close();
            // sentarse en el borde y acostarse
            yield return R.player.MoveCamera(R.poseBedSit.position, R.poseBedSit.eulerAngles.y, 10f, 0.95f, 0f, 1.1f);
            yield return Wait(0.4f);
            yield return R.player.MoveCamera(R.poseBedLie.position, R.poseBedLie.eulerAngles.y, -55f, 0.72f, -12f, 1.5f);
            LightingManager.I.SetZone("dormitorio", false);
            A.At(A.click, R.bedroomLamp.transform.position, 0.5f);
            R.bedroomLamp.forcedOff = true; LightingManager.I.ApplyAll();
            yield return Wait(1.5f);
            H.Say(You, T("Buenas noches.", "Good night."), 2.5f);
            yield return H.Blink(2);
            yield return H.FadeTo(1f, 2.5f);
            A.SetAmbience(0.4f);
            yield return Wait(1.5f);
            yield return H.ShowTitle("00:18", 1.6f);
            R.bedroomLamp.forcedOff = false;
            SaveStage(Stage.C2_Despertar);
            // lo mismo que ApplyWorld hace al cargar este punto de control (sin duplicar el historial del teléfono)
            var lm = LightingManager.I;
            lm.SetZone("sala", false); lm.SetZone("cocina", false); lm.SetZone("bano", false); lm.SetZone("pasillo", false);
            lm.SetMood(Mood.Night);
            GameClock.Set(0, 18);
            R.bolt.SetClosed(false);
            yield return C2_Wake();
        }
    }
}
