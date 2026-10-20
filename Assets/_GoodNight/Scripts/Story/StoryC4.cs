using System.Collections;
using UnityEngine;

namespace GNight
{
    /// <summary>CAPÍTULO 4 — EL PROTOCOLO GUARDIÁN (01:40).
    /// Llave en el abrigo → cerradura del armario → puerta metálica (circuito 06) → habitación 06 →
    /// cinta del padre → energía auxiliar → el doble se incorpora en tu cama → GUARDIÁN bloquea la casa.</summary>
    public partial class StoryManager
    {
        IEnumerator C4_Start(bool fromCheckpoint)
        {
            if (fromCheckpoint)
            {
                PlayerController.CutsceneLock = true;
                H.SetFade(1f);
                R.player.SetPose(R.poseHall != null ? R.poseHall : R.poseSalaMonitor, -1f);
                yield return H.FadeTo(0f, 1.5f);
                PlayerController.CutsceneLock = false;
            }
            GameClock.Set(1, 40);
            yield return H.ShowTitle(ChapterLabel, 2.5f);
            R.doorMetal.locked = true;
            R.doorMetal.lockedEs = "Puerta metálica. No tiene manija... solo un panel apagado: «CIRCUITO 06».";
            R.doorMetal.lockedEn = "A metal door. No handle... just a dead panel: \"CIRCUIT 06\".";
            if (R.coatPocket)
            {
                R.coatPocket.available = true;
                if (R.coatPocket.hidden == null) R.coatPocket.hidden = R.key;   // red de seguridad: la llave SIEMPRE está en el bolsillo
            }
            if (R.key) { R.key.gameObject.SetActive(true); R.key.collect = true; }
            O.Set(T("El cuarto que no existe", "The room that doesn't exist"),
                  T("El abrigo de papá cuelga junto a la entrada. El armario del pasillo tiene una cerradura pequeña.", "Dad's coat hangs by the entrance. The hallway wardrobe has a small lock."),
                  T("Busca la llave en el abrigo de papá", "Find the key in dad's coat"),
                  T("Abre el armario del pasillo", "Open the hallway wardrobe"),
                  T("Entra en la habitación 06", "Enter room 06"));
            StartCoroutine(C4_Ambient());
            StartCoroutine(C4_StuckHints());
        }

        /// <summary>Pistas progresivas (voz interior) si el jugador se atasca en el camino a la habitación 06.</summary>
        IEnumerator C4_StuckHints()
        {
            float t = 0f; int step = 0; string last = "";
            while (stage == Stage.C4_Protocolo && !Has("in06"))
            {
                yield return null;
                if (PlayerController.CutsceneLock || FocusSystem.Active || InspectSystem.Active) continue;
                string phase = !Has("gotKey") ? "key" : !Has("wardrobeOpen") ? "wardrobe" : !Has("metalOpen") ? "power" : "enter";
                if (phase != last) { last = phase; t = 0f; step = 0; }
                t += Time.deltaTime;
                if (t < (step == 0 ? 55f : 45f)) continue;
                t = 0f; step++;
                switch (phase)
                {
                    case "key":
                        H.Say(You, step == 1 ? T("¿Una llave...? Papá nunca sacaba nada de los bolsillos de su abrigo.", "A key...? Dad never emptied his coat pockets.")
                                             : T("El abrigo de papá sigue en el perchero, junto a la puerta principal.", "Dad's coat is still on the rack, by the front door."), 4f); break;
                    case "wardrobe":
                        H.Say(You, step == 1 ? T("Una llave tan pequeña... es de un mueble, no de una puerta.", "Such a tiny key... it's for furniture, not a door.")
                                             : T("El armario del pasillo tiene una cerradura pequeña. ¿Por qué está fijado a la pared?", "The hallway wardrobe has a small lock. Why is it fixed to the wall?"), 4f); break;
                    case "power":
                        H.Say(You, step == 1 ? T("La puerta 06 no tiene manija. Solo un panel apagado... necesita energía.", "Door 06 has no handle. Just a dead panel... it needs power.")
                                             : T("En el cuadro de la cocina, el interruptor 5 dice 06. Tengo que apagar otra cosa para no pasar de 7.", "At the kitchen panel, switch 5 says 06. I have to turn something else off to stay within 7."), 4.5f); break;
                    default:
                        H.Say(You, T("La puerta metálica se destrabó. Está en el pasillo, donde estaba el armario.", "The metal door unlocked. It's in the hallway, where the wardrobe was."), 4f); break;
                }
            }
        }

        IEnumerator C4_Ambient()
        {
            // la casa respira: crujidos lejos del jugador mientras busca
            while (stage == Stage.C4_Protocolo && !Has("in06"))
            {
                yield return Wait(Random.Range(18f, 30f));
                if (stage != Stage.C4_Protocolo || PlayerController.CutsceneLock) continue;
                Vector3 p = R.player.transform.position - R.player.transform.forward * 4f + Random.insideUnitSphere * 1.5f; p.y = 1.2f;
                A.At(Random.value < 0.5f ? A.woodCreak : A.steps1, p, 0.5f);
            }
        }

        void C4_Event(string id)
        {
            if (id == "search:key" && Set("coatSmell"))
                H.Say(You, T("Huele a la colonia de papá... Hay algo dentro del bolsillo.", "It smells like dad's cologne... There's something in the pocket."), 3.5f);
            else if (id == "collected:key" && Set("gotKey"))
            {
                O.Check(0);
                O.SetItem(1, T("Usa la llave para abrir el armario del pasillo", "Use the key to open the hallway wardrobe"));
                H.Hint(T("Llave del armario obtenida", "Wardrobe key obtained"), 3.5f);
                StartCoroutine(SayLater(1f, You, T("Una llave pequeña... con un 06 grabado. Es de un mueble.", "A tiny key... with 06 engraved on it. It's for furniture."), 3.5f));
            }
            else if (id == "wardrobe_unlocked" && Set("wardrobeUnlocked"))
            {
                R.wardrobe.Open(false);
                CameraFX.Shake(0.15f);
            }
            else if (id == "wardrobe_opened" && Set("wardrobeOpen"))
            {
                O.Check(1);
                H.Say(You, T("Siempre hubo una puerta aquí. Detrás del armario. Toda mi vida.", "There was always a door here. Behind the wardrobe. My whole life."), 4.5f);
                PowerPanel.I.c06Known = true;
                O.Set(T("El cuarto que no existe", "The room that doesn't exist"),
                      T("El cuadro solo aguanta 7 de carga. Para dar energía al 06 tendrás que apagar otra cosa.", "The panel only holds a load of 7. To power 06 you'll have to switch something else off."),
                      T("Da energía al circuito 06 en el cuadro eléctrico", "Power circuit 06 at the breaker panel"),
                      T("Entra en la habitación 06", "Enter room 06"));
                if (PowerPanel.I.On(Circuit.C06)) C4_Event("power_changed");
                StartCoroutine(SayLater(5f, "", T("(En el cuadro eléctrico, el interruptor sin nombre ahora dice: 06.)", "(At the breaker panel, the unnamed switch now reads: 06.)"), 4f));
            }
            else if (id == "power_changed" && Has("wardrobeOpen") && !Has("metalOpen") && PowerPanel.I.On(Circuit.C06))
            {
                Set("metalOpen");
                O.Check(0);
                R.doorMetal.locked = false;
                A.At(A.mechanism, R.doorMetal.transform.position, 1f);
                A.At(A.lockClunk, R.doorMetal.transform.position, 1f);
                H.Say("", T("(Algo se destrabó en el pasillo.)", "(Something unlocked in the hallway.)"), 3f);
            }
            else if (id == "zone:secreta" && Has("metalOpen") && Set("in06"))
                StartCoroutine(C4_Enter06());
            else if (id == "collected:reel" && Set("gotReel"))
            {
                O.Check(1);
                H.Hint(T("Cinta de carrete obtenida. Colócala en la grabadora ([E] sobre la grabadora).", "Reel tape obtained. Load it into the recorder ([E] on the recorder)."), 4f);
            }
            else if (id == "reel_loaded" && Set("reelLoaded"))
                StartCoroutine(C4_DadTape());
            else if (id == "aux_lever" && Has("dadTapeDone") && Set("auxOn"))
                StartCoroutine(C4_Monitors());
            else if (id == "inspected:fatherNotes" && Set("readNotes"))
                JournalManager.I.AddClue("notes", T("Notas de papá: «La casa no deja entrar a nadie. Los hace. Una copia no recuerda haber llegado; recuerda haber estado siempre.»", "Dad's notes: \"The house doesn't let anyone in. It makes them. A copy doesn't remember arriving; it remembers having always been there.\""));
            else if (id == "inspected:isolationDoc" && Set("readIso"))
                JournalManager.I.AddClue("iso", T("Manual GUARDIÁN: un sensor que parpadea a ritmo fijo es un ECO. Una cámara con REC y hora vieja está CONGELADA. Aísla solo lo que confirmen un sensor estable y una cámara en DIRECTO.", "GUARDIAN manual: a sensor blinking at a steady rhythm is an ECHO. A camera showing REC and an old time is FROZEN. Only isolate what a steady sensor AND a LIVE camera confirm."));
        }

        /// <summary>Red de seguridad: entrar a la 06 se detecta también por posición (no solo por el evento de zona).</summary>
        void C4_Tick()
        {
            if (!Has("metalOpen") || Has("in06")) return;
            var p = R.player.transform.position;
            if (p.x > HouseZone06.X0 + 0.1f && p.x < HouseZone06.X1 && p.z > HouseZone06.Z0 + 0.35f && p.z < HouseZone06.Z1 && Set("in06"))
                StartCoroutine(C4_Enter06());
        }

        IEnumerator C4_Enter06()
        {
            A.SetAmbience(0.3f);
            yield return Wait(0.8f);
            H.Say(You, T("Monitores, papeles... una grabadora de carrete. Papá trabajaba aquí.", "Monitors, papers... a reel-to-reel recorder. Dad worked here."), 4f);
            O.Set(T("Habitación 06", "Room 06"),
                  T("A la izquierda, bajo la lámpara, está la mesa de GRABACIONES: la grabadora de papá y una cinta de carrete. Los papeles del escritorio del fondo explican el resto.", "On the left, under the lamp, is the RECORDINGS table: dad's recorder and a reel tape. The papers on the back desk explain the rest."),
                  T("Entra en la habitación 06", "Enter room 06"),
                  T("Encuentra la cinta de carrete", "Find the reel tape"),
                  T("Escucha la grabación de papá", "Listen to dad's recording"));
            O.Check(0);
            if (R.reel) R.reel.available = true;
            if (R.recorder) R.recorder.available = false;
        }

        IEnumerator C4_DadTape()
        {
            O.Check(2);
            A.At(A.tapeHiss, R.recorderSlot.transform.position, 0.5f);
            yield return Wait(1.5f);
            string dad = T("Papá (grabación)", "Dad (recording)");
            string[] es =
            {
                "Si escuchas esto, GUARDIÁN contó dos.",
                "No es un intruso. Nadie entra en esta casa sin que la alarma lo vea.",
                "La casa... hace copias. Las hace dentro. Aparecen en una habitación, sin cruzar ninguna puerta.",
                "La primera vez no salió bien. La casa intentó copiar a tu madre... y se quedó a medias. Sin cara. Sin nombre.",
                "GUARDIÁN la registró como SUJETO 0. Imita voces, sobre todo la de ella. Busca a alguien con quien terminar de copiarse.",
                "Las copias siguientes salieron mejor. Demasiado bien. La copia no sabe que lo es. Recuerda la cena. Recuerda a su madre. Te recuerda a ti.",
                "Si tú eres mi hijo, aísla al otro. Si no lo eres...",
                "...entonces ten el valor de quedarte. Te quiero. A los dos. Lo siento."
            };
            string[] en =
            {
                "If you're hearing this, GUARDIAN counted two.",
                "It's not an intruder. Nobody gets into this house without the alarm seeing it.",
                "The house... makes copies. It makes them inside. They appear in a room without crossing any door.",
                "The first time it went wrong. The house tried to copy your mother... and it stopped halfway. No face. No name.",
                "GUARDIAN logged it as SUBJECT 0. It imitates voices, hers most of all. It's looking for someone to finish copying itself from.",
                "The next copies came out better. Too well. The copy doesn't know it is one. It remembers dinner. It remembers its mother. It remembers you.",
                "If you are my son, isolate the other one. If you aren't...",
                "...then have the courage to stay. I love you. Both of you. I'm sorry."
            };
            for (int i = 0; i < es.Length; i++)
            {
                A.PlayVoice(Voice.Dad, R.recorderSlot.transform.position, 0.8f);
                H.Say(dad, T(es[i], en[i]), 4.6f);
                yield return Wait(5f);
            }
            Set("dadTapeDone");
            JournalManager.I.AddClue("dad", T("Papá: las copias aparecen dentro de una habitación sin cruzar puertas, y no saben que son copias.", "Dad: copies appear inside a room without crossing doors, and don't know they are copies."));
            JournalManager.I.AddClue("subject0", T("SUJETO 0: la primera copia, fallida. Intentó ser mamá y se quedó sin cara. Imita voces (¿la «mamá» de la puerta?) y busca a quién copiar.", "SUBJECT 0: the first, failed copy. It tried to be mom and was left without a face. It imitates voices (the \"mom\" at the door?) and looks for someone to copy."));
            if (TruthCount >= 3) StartCoroutine(SayLater(1f, You, T("23:46. El dormitorio. Ninguna puerta... Yo aparecí en mi escritorio a las 23:47.", "23:46. The bedroom. No door... I was at my desk at 23:47."), 5f));
            if (R.auxLever) R.auxLever.available = true;
            O.Set(T("Habitación 06", "Room 06"),
                  T("La palanca roja junto a los monitores dice ENERGÍA AUXILIAR.", "The red lever next to the monitors reads AUX POWER."),
                  T("Activa la energía auxiliar de los monitores", "Turn on the monitors' auxiliary power"));
        }

        // Evento grande: el doble se incorpora en tu cama → GUARDIÁN bloquea la casa (susto)
        IEnumerator C4_Monitors()
        {
            O.Check(0);
            A.UI(A.leverClick, 1f);
            A.At(A.breaker, R.auxLever.transform.position, 0.9f);
            PowerPanel.I.SetSilently(true, true, true, false, true);
            var bedCam = SecurityCameraSystem.I.ByZone("dormitorio");
            var kitchenCam = SecurityCameraSystem.I.ByZone("cocina");
            var hallCam = SecurityCameraSystem.I.ByZone("pasillo");
            var feeds = new[] { bedCam, kitchenCam, hallCam };
            for (int i = 0; i < R.secretMonitors.Length; i++)
            {
                var m = R.secretMonitors[i];
                m.source = feeds[i % feeds.Length]; m.overrideTexture = null; m.message = null; m.SetOn(true);
                A.At(A.staticBurst, m.transform.position, 0.3f);
                yield return Wait(0.35f);
            }
            // tu cama ya no está vacía
            R.doppelBedRoot.gameObject.SetActive(true);
            R.doppelBed.Play("LieIdle", 0f);
            yield return Wait(2f);
            H.Say(You, T("Hay alguien... en mi cama.", "There's someone... in my bed."), 3f);
            yield return WaitLookAt(R.secretMonitors[0].transform.position, 25f, 8f);
            yield return Wait(1.5f);
            R.doppelBed.Play("SitUp", 0.3f);
            A.SetTension(0.7f);
            yield return Wait(Mathf.Max(1.5f, R.doppelBed.Length("SitUp") * 0.9f));
            R.doppelBed.lookTarget = bedCam.transform.position; R.doppelBed.lookWeight = 1f;
            yield return Wait(1.2f);
            H.Say(You, T("Tiene mi cara. Me está mirando... a través de la cámara.", "It has my face. It's looking at me... through the camera."), 3.5f);
            yield return Wait(3.5f);

            // GUARDIÁN
            foreach (var m in R.secretMonitors) { m.showStatic = true; m.message = "GUARDIÁN"; }
            A.UI(A.staticBurst, 0.8f);
            yield return Wait(0.8f);
            PowerPanel.I.Trip();
            R.bolt.SetGuardianLock(true);
            LightingManager.I.SetMood(Mood.Emergency);
            LightingManager.I.SetEmergency(true);
            R.flashlight.Flicker(2f);
            A.SetAmbience(0f);
            yield return Wait(1.6f);

            // susto: algo en el umbral de la puerta metálica
            Vector3 door = R.doorMetal.transform.position; door.y = R.player.transform.position.y;
            Vector3 dir = (R.player.transform.position - door); dir.y = 0; dir.Normalize();
            E.ShowAt(door + dir * 0.6f, Quaternion.LookRotation(dir).eulerAngles.y, "Stare", false);
            E.LookAtPlayer(1f);
            yield return WaitLookAt(door, 35f, 6f);
            A.UI(A.entityScream, 1f);
            CameraFX.Shake(0.6f); CameraFX.FovKick(-8f);
            if (PostFX.I) PostFX.I.Shock(0.7f);
            yield return Wait(0.45f);
            E.Despawn();
            R.flashlight.Flicker(0.6f);
            yield return Wait(1.5f);
            A.PlayVoice(Voice.Guardian, null, 0.8f);
            H.Say("GUARDIÁN", T("Protocolo activado. Ocupantes: dos. Autorizados: uno. Aísle al duplicado antes del amanecer.", "Protocol active. Occupants: two. Authorized: one. Isolate the duplicate before dawn."), 6f);
            P.Receive(PhoneSystem.Guardian, T("PROTOCOLO GUARDIÁN\nOCUPANTES: 2 · AUTORIZADOS: 1\nPUERTAS EXTERIORES BLOQUEADAS\nRESTAURE ALARMA + CÁMARAS + CERRADURAS PARA AISLAR", "GUARDIAN PROTOCOL\nOCCUPANTS: 2 · AUTHORIZED: 1\nEXTERIOR DOORS LOCKED\nRESTORE ALARM + CAMERAS + LOCKS TO ISOLATE"), true, -1f, false);
            R.doppelBedRoot.gameObject.SetActive(false);
            A.SetTension(0.3f);
            SaveStage(Stage.C5_Final);
            yield return Wait(5f);
            yield return C5_Start(false);
        }
    }
}
