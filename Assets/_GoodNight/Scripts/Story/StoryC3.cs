using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>CAPÍTULO 3 — LA NOCHE QUE YA OCURRIÓ (01:05).</summary>
    public partial class StoryManager
    {
        IEnumerator C3_Start(bool fromCheckpoint)
        {
            if (fromCheckpoint)
            {
                PlayerController.CutsceneLock = true;
                H.SetFade(1f);
                R.player.SetPose(R.poseSalaMonitor, -1f);
                yield return H.FadeTo(0f, 1.5f);
                PlayerController.CutsceneLock = false;
            }
            GameClock.Set(1, 5);
            tapeSearchT = 0f; tapeHintStep = 0;
            yield return Wait(2f);
            // el teléfono escribe solo
            P.Outgoing(PhoneSystem.Unknown, GameClock.Text, T("¿Quién eres?", "Who are you?"));
            A.UI(A.messagePop, 0.6f);
            H.Hint(T("Tu teléfono envió un mensaje... sin que lo escribieras.", "Your phone sent a message... you didn't type it."), 4f);
            yield return Wait(4f);
            P.Receive(PhoneSystem.Unknown, T("Soy el que llegó a las 22:10.", "I'm the one who got home at 22:10."));
            yield return Wait(4.5f);
            P.Receive(PhoneSystem.Unknown, T("Papá grabó todo. Hay tres cintas en la casa. Míralas en la tele de la sala.", "Dad recorded everything. There are three tapes in the house. Watch them on the living room TV."), true, -1f, false);
            O.Set(T("Encuentra las cintas de papá", "Find dad's tapes"),
                  T("Revisa los muebles: la cómoda de la sala, debajo de tu cama, el botiquín del baño. La videograbadora está bajo la tele.", "Search the furniture: the living room dresser, under your bed, the bathroom cabinet. The VCR is under the TV."),
                  TapeCountText(), T("Mira las cintas en la videograbadora (bajo la TV)", "Watch the tapes on the VCR (under the TV)"), T("Ordénalas y pulsa REPRODUCIR SECUENCIA", "Order them and press PLAY SEQUENCE"));
        }

        int TapesFound() { int n = 0; foreach (var id in new[] { "tape_a", "tape_b", "tape_c" }) if (Inventory.Has(id) || Has("got_" + id)) n++; return n; }
        string TapeCountText() => T("Cintas encontradas: ", "Tapes found: ") + TapesFound() + "/3";
        float tapeSearchT; int tapeHintStep;

        void C3_Event(string id)
        {
            if (id.StartsWith("collected:tape_"))
            {
                Set("got_" + id.Substring(10));
                tapeSearchT = 0f;
                O.SetItem(0, TapeCountText());
                if (TapesFound() >= 3) O.Check(0);
                if (TapesFound() == 2 && Set("kitchenEvent")) StartCoroutine(C3_KitchenEvent());
            }
            else if (id.StartsWith("tape_watched:"))
            {
                if (R.vcr.WatchedCount >= 3) O.Check(1);
                if (id.EndsWith("tape_c") && Set("sawEntry"))
                    StartCoroutine(SayLater(1.5f, You, T("Ese soy yo... entrando a las 22:10. Pero yo estaba aquí desde la tarde... ¿o no?", "That's me... coming in at 22:10. But I've been here since the afternoon... haven't I?"), 5f));
                if (id.EndsWith("tape_b") && Set("sawFuture"))
                    StartCoroutine(SayLater(1.5f, You, T("01:24... todavía no son las 01:24.", "01:24... it isn't 01:24 yet."), 4f));
            }
            else if (id == "tapes_reconstructed" && Set("reconstructed"))
            {
                O.Check(2);
                Set("truth_tapes");
                JournalManager.I.AddClue("subject2", T("GUARDIÁN: a las 23:46 apareció un SUJETO 2 en el dormitorio, sin entrar por ninguna puerta. A esa hora yo estaba en el escritorio.", "GUARDIAN: at 23:46 a SUBJECT 2 appeared in the bedroom without entering through any door. At that time I was at the desk."));
                StartCoroutine(C3_AfterReconstruction());
            }
            else if (id == "inspected:photo_family" && Has("reconstructed") && Set("truth_photo"))
                JournalManager.I.AddClue("photo", T("En la foto familiar ya no aparezco. Solo mamá y papá... y un espacio vacío entre ellos.", "I'm no longer in the family photo. Just mom and dad... and an empty space between them."));
        }

        IEnumerator SayLater(float d, string who, string text, float dur) { yield return Wait(d); H.Say(who, text, dur); }

        // Evento 6: platos en la cocina, todo quieto, un cajón se cierra solo detrás
        IEnumerator C3_KitchenEvent()
        {
            yield return Wait(3f);
            R.platesSource.PlayOneShot(A.plates, Settings.Sfx);
            H.Say("", T("(Platos moviéndose... en la cocina.)", "(Plates moving... in the kitchen.)"), 3f);
            float t = 0f;
            while (RoomZone.At(R.player.transform.position) != "cocina" && t < 90f) { t += Time.deltaTime; yield return null; }
            if (t >= 90f) yield break;
            yield return Wait(2.5f);
            // detrás del jugador
            var d = R.kitchenDrawerSlam;
            if (d != null && R.player.AngleTo(d.transform.position) > 100f) { d.SetOpen(true, true); yield return Wait(0.1f); d.SetOpen(false); A.At(A.hitLow, d.transform.position, 0.5f); CameraFX.Shake(0.1f); }
        }

        void C3_Tick()
        {
            TapeHints();
            // la foto familiar cambia después de la reconstrucción
            if (Has("reconstructed") && Set("photoSwapped") && R.photoFamilyPicture != null && R.photoFamilyMissing != null)
            {
                var m = new Material(R.photoFamilyPicture.sharedMaterial) { mainTexture = R.photoFamilyMissing };
                if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", R.photoFamilyMissing);
                R.photoFamilyPicture.sharedMaterial = m;
                if (R.photoFamily)
                {
                    R.photoFamily.textEs = "Mamá y papá, sonriendo frente a la casa. Entre los dos hay un espacio vacío... donde antes estaba yo.";
                    R.photoFamily.textEn = "Mom and dad, smiling in front of the house. Between them there's an empty space... where I used to be.";
                    R.photoFamily.seen = false;
                }
            }
        }

        /// <summary>Pistas progresivas (voz interior) solo si el jugador lleva un rato sin encontrar las cintas.</summary>
        void TapeHints()
        {
            if (FocusSystem.Active || InspectSystem.Active || TapesFound() >= 3) return;
            tapeSearchT += Time.deltaTime;
            bool needB = !Inventory.Has("tape_b") && !Has("got_tape_b");
            bool needC = !Inventory.Has("tape_c") && !Has("got_tape_c");
            bool needA = !Inventory.Has("tape_a") && !Has("got_tape_a");
            // la cama: más escondida a propósito. Solo después de un buen rato.
            if (needB && TapesFound() >= 2)
            {
                if (tapeHintStep == 0 && tapeSearchT > 50f) { tapeHintStep = 1; tapeSearchT = 0f; H.Say(You, T("Estoy seguro de que todavía falta una grabación.", "I'm sure there's still one recording missing."), 4f); }
                else if (tapeHintStep == 1 && tapeSearchT > 45f) { tapeHintStep = 2; tapeSearchT = 0f; H.Say(You, T("Quizás haya algo escondido en mi habitación.", "Maybe there's something hidden in my room."), 4f); }
                else if (tapeHintStep == 2 && (tapeSearchT > 45f || (tapeSearchT > 10f && RoomZone.At(R.player.transform.position) == "dormitorio")))
                { tapeHintStep = 3; tapeSearchT = 0f; H.Say(You, T("¿Y si reviso debajo de la cama?", "What if I check under the bed?"), 4f); }
                return;
            }
            if (tapeSearchT > 120f)
            {
                tapeSearchT = 0f;
                if (needC) H.Say(You, T("Papá guardaba cosas raras en el baño... ¿detrás del espejo?", "Dad kept odd things in the bathroom... behind the mirror?"), 4f);
                else if (needA) H.Say(You, T("La cómoda de la sala tiene más cajones que el del recibo.", "The living room dresser has more drawers than the receipt one."), 4f);
            }
        }

        // Evento 7: la tele muestra "tu" imagen en otra habitación
        IEnumerator C3_AfterReconstruction()
        {
            yield return Wait(6f);
            var tv = R.vcr.tv;
            var bedCam = SecurityCameraSystem.I.ByZone("dormitorio");
            tv.SetOn(true); tv.overrideTexture = null; tv.source = bedCam; tv.message = null; tv.recLabel = false; tv.stampOverride = null;
            A.UI(A.staticBurst, 0.5f);
            // el doble, de pie en tu cuarto, mirando la cámara
            E.ShowAt(R.spawnBedroomCam.position, R.spawnBedroomCam.eulerAngles.y, "Stare", true);
            var bedCamPos = bedCam.transform.position;
            var a = E.Anim; if (a) { a.lookTarget = bedCamPos; a.lookWeight = 1f; }
            A.SetTension(0.5f);
            yield return Wait(2f);
            H.Say(You, T("Ese... soy yo. En mi cuarto.", "That's... me. In my room."), 3f);
            yield return Wait(4f);
            A.PlayVoice(Voice.You, R.spawnBedroomCam.position, 0.9f);
            H.Say(T("Tu propia voz", "Your own voice"), T("¿Hay alguien ahí afuera?", "Is someone out there?"), 3f);
            yield return Wait(3.2f);
            tv.showStatic = true; tv.message = T("SEÑAL PERDIDA", "SIGNAL LOST");
            if (FocusSystem.Active) FocusSystem.I.Exit();
            SaveStage(Stage.C3_Persecucion);
            yield return Wait(1.2f);
            tv.showStatic = false; tv.SetOn(false); tv.message = null;
            yield return C3_Chase(false);
        }

        IEnumerator C3_Chase(bool fromCheckpoint)
        {
            if (fromCheckpoint)
            {
                PlayerController.CutsceneLock = true;
                H.SetFade(1f);
                R.player.SetPose(R.poseSalaMonitor, -1f);
                yield return H.FadeTo(0f, 1.5f);
                PlayerController.CutsceneLock = false;
            }
            O.Set(T("Que no te encuentre", "Don't let it find you"),
                  T("Usa tu voz para engañarte. No respondas. Escóndete o mantén distancia.", "It uses your voice. Don't answer. Hide or keep your distance."),
                  T("Sobrevive", "Survive"));
            E.telegraph = true;
            E.OnCaught = () => GameManager.I.Die(T("\"Tenía tu cara. Tenía tu voz.\"", "\"It had your face. It had your voice.\""));
            E.Spawn(R.spawnBedroom.position, R.spawnBedroom.eulerAngles.y, true, R.routeChase2, AIState.Patrol);
            E.patrolSpeed = 1.15f;
            A.SetTension(0.8f);
            float time = 0f, voiceT = 8f;
            while (time < 75f || E.State == AIState.Chase || E.State == AIState.Notice || E.State == AIState.InspectHide)
            {
                if (stage != Stage.C3_Persecucion) yield break;
                time += Time.deltaTime; voiceT -= Time.deltaTime;
                if (voiceT <= 0f && E.State == AIState.Patrol)
                {
                    voiceT = Random.Range(10f, 16f);
                    A.PlayVoice(Voice.You, E.transform.position + Vector3.up * 1.6f, 0.9f);
                    string[] es = { "¿Mamá? ¿Eres tú?", "Sal. Solo quiero hablar.", "Sé dónde te escondes.", "No es tu casa." };
                    string[] en = { "Mom? Is that you?", "Come out. I just want to talk.", "I know where you hide.", "It's not your house." };
                    int k = Random.Range(0, es.Length);
                    H.Say(T("Tu propia voz", "Your own voice"), T(es[k], en[k]), 3f);
                }
                yield return null;
            }
            bool returned = false;
            E.OnReturned = () => returned = true;
            E.ReturnTo(R.nodeBedroom);
            for (float rt = 0f; !returned && rt < 30f; rt += Time.deltaTime) yield return null;
            E.Despawn();
            A.SetTension(0f);
            yield return Wait(2.5f);
            P.Receive(PhoneSystem.Unknown, T("Ya viste. La llave está en el abrigo de papá. El cuarto que no existe está detrás del armario del pasillo.", "Now you've seen. The key is in dad's coat. The room that doesn't exist is behind the hallway wardrobe."));
            SaveStage(Stage.C4_Protocolo);
            yield return Wait(4f);
            yield return C4_Start(false);
        }
    }
}
