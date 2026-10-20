namespace GNight
{
    /// <summary>
    /// Pistas opcionales de 3 niveles para cada objetivo: 1 general → 2 habitación → 3 concreta.
    /// Se desbloquean con el tiempo atascado en el mismo objetivo y el jugador las revela en el teléfono (app Objetivos).
    /// La búsqueda es por título (+ primer punto cuando un título se repite), en español o en inglés.
    /// </summary>
    public static class HintBook
    {
        public class Entry
        {
            public string titleEs, titleEn, firstEs, firstEn;
            public bool investigate;           // investigación (buscar/deducir) o directo (hacer X)
            public string[] es, en;            // 3 niveles
        }

        static Entry E(string tEs, string tEn, string fEs, string fEn, bool inv, string es1, string es2, string es3, string en1, string en2, string en3)
            => new Entry { titleEs = tEs, titleEn = tEn, firstEs = fEs, firstEn = fEn, investigate = inv, es = new[] { es1, es2, es3 }, en = new[] { en1, en2, en3 } };

        public static readonly Entry[] All =
        {
            // ---------------- CAPÍTULO 1
            E("Asegura la casa antes de dormir", "Lock up the house before bed", null, null, false,
              "Antes de dormir, todo tiene que quedar apagado y cerrado.",
              "La computadora está en tu escritorio. La puerta principal está en la sala. Hay ventanas en tu cuarto, la sala y la cocina.",
              "Apunta y pulsa [E]: la computadora para apagarla, la puerta principal para cerrarla y cada ventana abierta para bajarla. La lista cuenta las ventanas que faltan.",
              "Before bed, everything has to be off and shut.",
              "The computer is on your desk. The front door is in the living room. There are windows in your room, the living room and the kitchen.",
              "Aim and press [E]: the computer to turn it off, the front door to close it and every open window to shut it. The list counts the windows left."),
            E("Activa la alarma", "Arm the alarm", null, null, true,
              "El código no es al azar: alguien de la casa lo dejó escrito en un papel.",
              "Busca en los cajones de la cómoda de la sala.",
              "En el cajón de ARRIBA de la cómoda de la sala hay un recibo del cerrajero. Examínalo y gíralo con el ratón: el PIN es el día y el mes de la fecha (23/08 → 2308). Escríbelo en el panel junto a la puerta principal.",
              "The code isn't random: someone in the house wrote it down.",
              "Search the drawers of the living room dresser.",
              "The TOP drawer of the living room dresser holds a locksmith receipt. Examine it and turn it with the mouse: the PIN is the day and month of the date (23/08 → 2308). Enter it on the panel by the front door."),
            E("Prepárate para dormir", "Get ready for bed", null, null, false,
              "Ya está todo cerrado. Solo queda acostarte.",
              "Tu cama está en el dormitorio. El baño (opcional) está en el pasillo.",
              "Ve a tu cama y pulsa [E] para acostarte. Lavarte la cara en el baño es opcional.",
              "Everything is locked. All that's left is going to bed.",
              "Your bed is in the bedroom. The bathroom (optional) is in the hallway.",
              "Go to your bed and press [E] to lie down. Washing your face in the bathroom is optional."),
            // ---------------- CAPÍTULO 2
            E("Revisa el cerrojo sin hacer ruido", "Check the deadbolt without making noise", null, null, false,
              "Algo te despertó. Comprueba que la entrada siga cerrada.",
              "La puerta principal está en la sala. Agáchate con [Ctrl] para no hacer ruido.",
              "Acércate agachado a la puerta principal y pulsa [E] sobre el cerrojo interior (la pieza metálica a la altura de la manija).",
              "Something woke you up. Make sure the entrance is still locked.",
              "The front door is in the living room. Crouch with [Ctrl] to stay quiet.",
              "Crouch up to the front door and press [E] on the inside deadbolt (the metal piece next to the handle)."),
            E("¿Mamá?", "Mom?", null, null, false,
              "Mamá dijo que estaba en el trabajo. Piensa antes de abrir.",
              "No hace falta tocar nada: mira el teléfono cuando vibre ([Tab]).",
              "NO abras el cerrojo ni la puerta. Quédate quieto unos segundos: la escena sigue sola.",
              "Mom said she was at work. Think before you open.",
              "You don't need to touch anything: check the phone when it buzzes ([Tab]).",
              "DON'T open the deadbolt or the door. Stay still for a few seconds: the scene continues by itself."),
            E("Revisa las cámaras de seguridad", "Check the security cameras", null, null, false,
              "Papá dejó cámaras por toda la casa.",
              "El monitor está en el escritorio de la sala, contra la pared.",
              "Ve al escritorio de la sala y pulsa [E] frente al monitor. Cambia de cámara con [1]-[4] o haciendo clic en los botones del selector.",
              "Dad put cameras all over the house.",
              "The monitor is on the living room desk, against the wall.",
              "Go to the living room desk and press [E] in front of the monitor. Switch cameras with [1]-[4] or by clicking the selector buttons."),
            E("Descubre por qué la cámara muestra algo distinto", "Find out why the camera shows something different", null, null, true,
              "Compara lo que ves en la cámara con lo que ves con tus propios ojos.",
              "Mira la puerta del baño desde el pasillo. Después vuelve al monitor de la sala.",
              "Ve al pasillo y mira la puerta del baño. Luego, en el monitor, pulsa el botón ARCHIVO/DIRECTO: la imagen era una grabación.",
              "Compare what the camera shows with what you see with your own eyes.",
              "Look at the bathroom door from the hallway. Then go back to the living room monitor.",
              "Go to the hallway and look at the bathroom door. Then press the monitor's ARCHIVE/LIVE button: the image was a recording."),
            E("Restaura la energía de las cámaras", "Restore power to the cameras", null, null, true,
              "El directo falla porque las cámaras no tienen energía.",
              "El cuadro eléctrico está en la cocina. Papá dejó una nota al lado y hay una caja de herramientas en el suelo.",
              "Lee la nota del cuadro (aguanta 7: Alarma 3 · Cámaras 2 · Cerraduras 2 · Luces 3). Abre la caja de herramientas, toma el fusible F2, colócalo en el zócalo del cuadro y deja encendidos ALARMA + CÁMARAS + CERRADURAS (= 7), con LUCES apagadas.",
              "The live feed fails because the cameras have no power.",
              "The breaker panel is in the kitchen. Dad left a note next to it, and there's a toolbox on the floor.",
              "Read the note by the panel (it holds 7: Alarm 3 · Cameras 2 · Locks 2 · Lights 3). Open the toolbox, take fuse F2, put it in the panel's socket and leave ALARM + CAMERAS + LOCKS on (= 7) with LIGHTS off."),
            E("Revisa la transmisión en directo", "Check the live feed", null, null, false,
              "Las cámaras ya tienen energía.",
              "Vuelve al monitor de la sala.",
              "Pulsa [E] en el monitor de la sala y luego el botón ARCHIVO/DIRECTO.",
              "The cameras have power now.",
              "Go back to the living room monitor.",
              "Press [E] on the living room monitor, then the ARCHIVE/LIVE button."),
            E("Escóndete", "Hide", null, null, false,
              "No puedes pelear. Rompe la línea de visión y escóndete.",
              "El armario del dormitorio y la alacena de la cocina sirven de escondite.",
              "Entra en un escondite con [E] y no salgas hasta que los pasos se alejen. Si te ve, corre y dobla una esquina antes de esconderte.",
              "You can't fight. Break line of sight and hide.",
              "The bedroom wardrobe and the kitchen pantry are hiding spots.",
              "Get into a hiding spot with [E] and stay until the footsteps fade. If it sees you, run and turn a corner before hiding."),
            // ---------------- CAPÍTULO 3
            E("Encuentra las cintas de papá", "Find dad's tapes", null, null, true,
              "Hay tres cintas VHS escondidas en distintos muebles. La videograbadora está bajo la tele.",
              "Una en la cómoda de la sala, otra debajo de tu cama y otra en el baño.",
              "A: segundo cajón de la cómoda de la sala. B: «Revisar debajo de la cama» en el dormitorio. C: abre el botiquín con espejo del baño. Luego usa la videograbadora ([E]): clic en cada cinta para verla. Cuando veas las tres, selecciona una cinta y colócala en 1º/2º/3º por la hora de cada grabación: C (22:10) → A (22:47) → B (01:24, domingo). Pulsa REPRODUCIR SECUENCIA.",
              "Three VHS tapes are hidden in different pieces of furniture. The VCR is under the TV.",
              "One is in the living room dresser, one under your bed and one in the bathroom.",
              "A: second drawer of the living room dresser. B: \"Look under the bed\" in the bedroom. C: open the bathroom mirror cabinet. Then use the VCR ([E]): click each tape to watch it. Once you've seen all three, select a tape and place it in 1st/2nd/3rd by each recording's time: C (22:10) → A (22:47) → B (01:24, Sunday). Press PLAY SEQUENCE."),
            E("Que no te encuentre", "Don't let it find you", null, null, false,
              "Usa tu voz. No respondas.",
              "Mantén distancia y usa los escondites (armario del dormitorio, alacena de la cocina).",
              "Escóndete con [E] y espera a que se aleje; si te persigue, corre por el pasillo y rompe la línea de visión.",
              "It uses your voice. Don't answer.",
              "Keep your distance and use the hiding spots (bedroom wardrobe, kitchen pantry).",
              "Hide with [E] and wait for it to leave; if it chases you, run down the hallway and break line of sight."),
            // ---------------- CAPÍTULO 4
            E("El cuarto que no existe", "The room that doesn't exist", "Busca la llave en el abrigo de papá", "Find the key in dad's coat", true,
              "Papá guardaba una llave pequeña encima.",
              "Su abrigo cuelga del perchero junto a la entrada de la sala. El armario del pasillo tiene una cerradura.",
              "Examina el abrigo del perchero (sala, junto a la puerta principal) y toma la llave «06». Úsala con [E] en la cerradura del armario corredizo del pasillo.",
              "Dad kept a small key on him.",
              "His coat hangs on the rack by the living room entrance. The hallway wardrobe has a lock.",
              "Examine the coat on the rack (living room, by the front door) and take the \"06\" key. Use it with [E] on the hallway sliding wardrobe's lock."),
            E("El cuarto que no existe", "The room that doesn't exist", "Da energía al circuito 06", "Power circuit 06", true,
              "La puerta 06 necesita energía, pero el cuadro tiene un límite.",
              "Ve al cuadro eléctrico de la cocina. El interruptor 5 (???) es el circuito 06 y gasta 2.",
              "Con un máximo de 7: deja ALARMA (3) y CERRADURAS (2), apaga CÁMARAS y LUCES y enciende el 5 (06) → 3+2+2 = 7. La puerta metálica del pasillo se abrirá.",
              "Door 06 needs power, but the panel has a limit.",
              "Go to the kitchen breaker panel. Switch 5 (???) is circuit 06 and uses 2.",
              "With a maximum of 7: keep ALARM (3) and LOCKS (2), turn CAMERAS and LIGHTS off and turn on 5 (06) → 3+2+2 = 7. The hallway metal door will open."),
            E("Habitación 06", "Room 06", "Entra en la habitación 06", "Enter room 06", true,
              "Papá dejó todo preparado aquí dentro.",
              "Lee los papeles del escritorio; la cinta de carrete está junto a la grabadora, bajo la lámpara.",
              "Examina las notas de papá y el manual sobre la consola. La cinta de carrete está de pie sobre la mesa de GRABACIONES (pared izquierda al entrar, bajo la lámpara), junto a la grabadora. Colócala en la grabadora con [E] y escucha.",
              "Dad left everything ready in here.",
              "Read the papers on the desk; the reel tape is next to the recorder, under the lamp.",
              "Examine dad's notes and the manual on the console. The reel tape stands on the RECORDINGS table (left wall as you enter, under the lamp), next to the recorder. Load it into the recorder with [E] and listen."),
            E("Habitación 06", "Room 06", "Activa la energía auxiliar", "Turn on the monitors", false,
              "Los monitores de la habitación 06 están apagados.",
              "Hay una palanca roja en la pared, junto a los monitores.",
              "Pulsa [E] en la palanca roja de ENERGÍA AUXILIAR (pared oeste de la habitación 06).",
              "Room 06's monitors are off.",
              "There's a red lever on the wall next to the monitors.",
              "Press [E] on the red AUX POWER lever (west wall of room 06)."),
            // ---------------- CAPÍTULO 5
            E("Aísla al duplicado", "Isolate the duplicate", null, null, true,
              "GUARDIÁN necesita la seguridad completa para aislar una zona. Después, compara sensores y cámaras.",
              "Cuadro de la cocina: ALARMA + CÁMARAS + CERRADURAS (3+2+2 = 7). Luego vuelve a la consola de la habitación 06.",
              "En la consola: un sensor que parpadea a ritmo fijo es un ECO y una cámara con REC y hora vieja está CONGELADA. Aísla solo la zona con sensor estable Y cámara en DIRECTO. Si reuniste suficientes verdades aparece una tercera opción.",
              "GUARDIAN needs full security to isolate a zone. Then compare sensors and cameras.",
              "Kitchen panel: ALARM + CAMERAS + LOCKS (3+2+2 = 7). Then go back to the room 06 console.",
              "On the console: a sensor blinking at a steady rhythm is an ECHO and a camera showing REC and an old time is FROZEN. Only isolate the zone with a steady sensor AND a LIVE camera. If you gathered enough truths, a third option appears."),
        };

        static bool Eq(string a, string b) => !string.IsNullOrEmpty(a) && !string.IsNullOrEmpty(b) && (a == b || b.StartsWith(a));

        public static Entry Find(string title, string first)
        {
            Entry byTitle = null;
            foreach (var e in All)
            {
                if (!(Eq(e.titleEs, title) || Eq(e.titleEn, title))) continue;
                if (e.firstEs == null) { if (byTitle == null) byTitle = e; continue; }
                if (Eq(e.firstEs, first) || Eq(e.firstEn, first)) return e;
                if (byTitle == null) byTitle = e;
            }
            return byTitle;
        }
    }
}
