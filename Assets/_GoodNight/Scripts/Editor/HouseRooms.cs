using UnityEngine;
using static GNight.EditorTools.BuildKit;

namespace GNight.EditorTools
{
    /// <summary>Habitaciones: muebles FBX + objetos interactivos de la historia.</summary>
    public partial class HouseBuilder
    {
        // Caras interiores de las paredes
        const float In = 0.06f;

        // ================================================================= DORMITORIO
        void Bedroom()
        {
            var g = Group("Dormitorio", root);
            float eastIn = 0f - In, westIn = WestX + In, northIn = BedN - In;

            // cama (cabecera al norte)
            var bed = Put("bed", -3.3f, northIn - 1.06f, 180f, 0f, "Cama", g);
            var bedComp = bed.AddComponent<Bed>();
            // revisar debajo de la cama (cinta B)
            var ub = new GameObject("Debajo_Cama"); ub.transform.SetParent(g, false);
            ub.transform.position = new Vector3(-2.68f, 0.18f, 3.9f);
            var ubc = ub.AddComponent<BoxCollider>(); ubc.size = new Vector3(0.12f, 0.3f, 1.6f);
            underBed = ub.AddComponent<SearchSpot>(); underBed.promptEs = "Revisar debajo de la cama"; underBed.promptEn = "Look under the bed";
            var tb = Model("vhs_tape", g, new Vector3(-3.2f, 0.01f, 3.7f), 90f, "Cinta_B");
            tb.transform.rotation = Quaternion.Euler(90f, 70f, 0f);
            SingleBox(tb);
            tapeB = Item(tb, "tape_b", "Cinta VHS «B»", "VHS tape \"B\"", "Etiqueta escrita por papá: «B — pasillo». La caja está tibia, como si alguien la hubiera tenido en la mano hace un momento.",
                         "Dad's handwriting on the label: \"B — hallway\". The case is warm, as if someone was holding it a moment ago.", true, "Cinta B", "Tape B");
            tapeB.available = false;
            underBed.hidden = tapeB;

            // mesita de luz + reloj despertador
            var ns = Put("nightstand", -2.45f, northIn - 0.25f, 180f, 0f, "Mesita", g);
            var clock = Put("alarm_clock", -2.45f, northIn - 0.25f, 180f, TopY(ns), "Despertador", g);
            var ct = Text3D("Hora", clock.transform, Vector3.zero, 0f, "23:47", 0.006f, new Color(1f, 0.25f, 0.15f));
            ct.transform.position = WorldBounds(clock).center + FrontDir(180f) * (WorldBounds(clock).extents.z + 0.003f);
            ct.transform.rotation = Quaternion.Euler(0, 0f, 0);
            ct.gameObject.AddComponent<ClockDisplay>(); NoShadow(ct.gameObject);
            var lampN = MakeLight("Luz_Despertador", clock.transform, LightType.Point, Vector3.zero, new Color(1f, 0.3f, 0.2f), 0.25f, 1.2f, LightShadows.None);
            lampN.transform.position = ct.transform.position + Vector3.forward * -0.05f;

            // escritorio contra la pared este, mirando al oeste
            float deskZ = 2.75f;
            var desk = AgainstWall("desk", g, new Vector3(eastIn, 0, deskZ), -90f, 0f, "Escritorio");
            // altura REAL del tablero (rayo hacia abajo): el límite superior del modelo puede incluir molduras
            float deskTop = SurfaceY(new Vector3(-0.5f, 1.2f, deskZ), TopY(desk));
            var deskB = WorldBounds(desk);
            float deskBack = deskB.max.x, deskFront = deskB.min.x;      // el escritorio mira al oeste (-X)
            // monitor CRT más compacto (menos fondo) y pegado atrás, para que el teclado quepa delante
            var mon = Model("crt_monitor", g, Vector3.zero, -90f, "Monitor_PC");
            mon.transform.localScale = new Vector3(1f, 1f, 0.62f);
            var mb0 = WorldBounds(mon);
            PlaceByBounds(mon, new Vector3(deskBack - 0.03f - mb0.extents.x, 0, deskZ), deskTop);
            var scr = ScreenQuad(mon, -90f, new Vector2(0.29f, 0.22f), 0.55f);
            scr.transform.SetParent(g, true);                               // fuera del monitor escalado (no deformar la pantalla)
            scr.transform.localScale = new Vector3(0.29f, 0.22f, 1f);
            computer = scr.AddComponent<Computer>();
            computer.screen = scr.GetComponent<Renderer>(); computer.onMat = mScreenDesktop; computer.offMat = mMonitorOff;
            computer.glow = MakeLight("Brillo_Pantalla", g, LightType.Point, scr.transform.position + FrontDir(-90f) * 0.35f, new Color(0.55f, 0.7f, 1f), 0.9f, 2.6f, LightShadows.None);
            var monCol = mon.GetComponentsInChildren<Collider>(); foreach (var c in monCol) Object.DestroyImmediate(c);
            scr.AddComponent<BoxCollider>().size = new Vector3(1.1f, 1.1f, 0.2f);
            // teclado apoyado en el tablero, delante del monitor y dentro del borde
            float monFront = WorldBounds(mon).min.x;
            var kb = Model("keyboard", g, Vector3.zero, -90f, "Teclado");
            var kbB = WorldBounds(kb);
            float kbX = Mathf.Max(deskFront + 0.06f + kbB.extents.x, Mathf.Min(monFront - 0.05f - kbB.extents.x, (monFront + deskFront) * 0.5f));
            PlaceByBounds(kb, new Vector3(kbX, 0, deskZ + 0.02f), deskTop);
            // mouse + alfombrilla a la derecha del teclado (el jugador mira al este: su derecha es -Z)
            float mz = deskZ - 0.36f;
            var pad = Box("Alfombrilla_Mouse", g, new Vector3(kbX, deskTop + 0.002f, mz), new Vector3(0.21f, 0.004f, 0.25f), Lit("M_MousePad", null, new Color(0.06f, 0.07f, 0.1f)), 1f, false);
            NoShadow(pad);
            var mouse = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mouse.name = "Mouse"; mouse.transform.SetParent(g, false);
            Object.DestroyImmediate(mouse.GetComponent<Collider>());
            mouse.transform.position = new Vector3(kbX + 0.01f, deskTop + 0.006f, mz);
            mouse.transform.localScale = new Vector3(0.105f, 0.042f, 0.062f);      // largo hacia el jugador (X), bajo, angosto
            mouse.GetComponent<Renderer>().sharedMaterial = mBeige;
            var wheel = Box("Rueda", mouse.transform, new Vector3(0.3f, 0.45f, 0f), new Vector3(0.12f, 0.18f, 0.12f), mBlack, 1f, false);
            var btn = Box("Separacion", mouse.transform, new Vector3(0.28f, 0.42f, 0f), new Vector3(0.36f, 0.12f, 0.025f), mBlack, 1f, false);
            NoShadow(wheel); NoShadow(btn);
            // cable hacia atrás
            Box("Cable_Mouse", g, new Vector3((kbX + 0.06f + deskBack - 0.05f) * 0.5f, deskTop + 0.004f, mz + 0.01f),
                new Vector3(Mathf.Max(0.05f, deskBack - 0.05f - (kbX + 0.06f)), 0.006f, 0.006f), mBlack, 1f, false);
            Put("pc_tower", -0.3f, deskZ + 0.42f, -90f, 0f, "Torre", g);
            var fan = Put("desk_fan", -0.3f, deskZ - 0.48f, -60f, deskTop, "Ventilador", g);
            var blades = Part(fan, "Blades"); Spinner fanSp = null;
            if (blades) { fanSp = blades.gameObject.AddComponent<Spinner>(); fanSp.axis = Vector3.forward; fanSp.speed = 900f; }
            var fanHum = Src("Zumbido_Ventilador", fan.transform, new Vector3(0, 0.2f, 0), Clip("fan_hum"), 0.18f, true, 1f, 0.5f, 4f);
            var fanT = fan.AddComponent<Toggleable>(); fanT.nounEs = "ventilador"; fanT.nounEn = "fan";
            fanT.spinners = fanSp != null ? new[] { fanSp } : new Spinner[0]; fanT.hum = fanHum;
            if (fan.GetComponentInChildren<Collider>() == null) fan.AddComponent<BoxCollider>();
            var dl = Put("desk_lamp", -0.28f, deskZ + 0.45f, -110f, deskTop, "Lampara_Escritorio", g);
            var dlBulb = Part(dl, "Bulb");
            var dlLight = MakeLight("Luz_Lampara", dl.transform, LightType.Point, Vector3.zero, new Color(1f, 0.8f, 0.55f), 1.6f, 4.5f, LightShadows.Soft);
            dlLight.transform.position = (dlBulb ? dlBulb.position : dl.transform.position + Vector3.up * 0.4f) + Vector3.down * 0.05f;
            bedroomLamp = dlLight.gameObject.AddComponent<RoomLight>();
            bedroomLamp.zone = "lampara"; bedroomLamp.lightSource = dlLight; bedroomLamp.alwaysPowered = true;
            if (dlBulb) { bedroomLamp.bulb = dlBulb.GetComponentInChildren<Renderer>(); bedroomLamp.onMat = mBulbOn; bedroomLamp.offMat = mBulbOff; }
            var dlT = dl.AddComponent<Toggleable>(); dlT.nounEs = "lámpara"; dlT.nounEn = "lamp"; dlT.lamp = bedroomLamp;
            roomLights.Add(bedroomLamp);
            var chair = Put("chair", -1.02f, deskZ, 90f, 0f, "Silla", g);
            var shelf = Model("bookshelf_wall", g, Vector3.zero, -90f, "Repisa");
            PlaceByBounds(shelf, new Vector3(eastIn - 0.13f, 0, deskZ), 1.55f);
            var cork = Model("cork_board", g, Vector3.zero, -90f, "Corcho");
            PlaceByBounds(cork, new Vector3(eastIn - 0.025f, 0, 1.55f), 1.15f);
            Quad("Dibujo", cork.transform, Vector3.zero, new Vector2(0.28f, 0.2f), 0f, Lit("M_KidDrawing", "kid_drawing", Color.white)).transform
                .SetPositionAndRotation(new Vector3(eastIn - 0.05f, 1.55f, 1.4f), Quaternion.Euler(0, 90f, 0));
            Quad("Calendario", cork.transform, Vector3.zero, new Vector2(0.22f, 0.3f), 0f, Lit("M_Calendar", "calendar", Color.white)).transform
                .SetPositionAndRotation(new Vector3(eastIn - 0.05f, 1.52f, 1.78f), Quaternion.Euler(0, 90f, 0));

            // armario de listones (escondite) contra la pared oeste
            var wd = AgainstWall("wardrobe_slatted", g, new Vector3(westIn, 0, 1.75f), 90f, 0f, "Armario_Dormitorio");
            wardrobeHide = MakeHide(wd, "armario", "wardrobe", 90f, new[] { "Door_L", "Door_R" }, 1.45f);

            // planta y póster
            Put("plant", -0.35f, northIn - 0.25f, 200f, 0f, "Planta", g);
            Quad("Poster", g, new Vector3(-1.3f, 1.6f, HallZ + In + 0.005f), new Vector2(0.6f, 0.85f), 180f, Lit("M_Poster", "poster", Color.white));
        }

        /// <summary>Escondite en un mueble con puertas (armario, alacena, locker).</summary>
        HidingSpot MakeHide(GameObject furniture, string es, string en, float facing, string[] doorParts, float eye)
        {
            var b = WorldBounds(furniture);
            var hs = furniture.AddComponent<HidingSpot>();
            hs.nameEs = es; hs.nameEn = en; hs.insideEye = eye; hs.lookLimit = 40f;
            Vector3 f = FrontDir(facing);
            var inside = Group("Dentro", furniture.transform); inside.position = new Vector3(b.center.x, 0, b.center.z) - f * 0.05f; inside.rotation = Quaternion.Euler(0, facing, 0);
            var outside = Group("Fuera", furniture.transform); outside.position = new Vector3(b.center.x, 0, b.center.z) + f * (Mathf.Abs(f.x) > 0.5f ? b.extents.x : b.extents.z) + f * 0.55f; outside.rotation = Quaternion.Euler(0, facing + 180f, 0);
            hs.inside = inside; hs.outside = outside;
            var doors = new System.Collections.Generic.List<Transform>(); var angles = new System.Collections.Generic.List<float>();
            foreach (var n in doorParts) { var p = Part(furniture, n); if (p) { doors.Add(p); angles.Add(HingeSign(p, f, Vector3.up) * 100f); } }
            hs.doors = doors.ToArray(); hs.doorOpenAngles = angles.ToArray();
            // el interior no debe bloquear el rayo de "Salir": la raíz tiene un collider propio delante
            var bc = furniture.AddComponent<BoxCollider>();
            bc.center = furniture.transform.InverseTransformPoint(b.center);
            var sz = Quaternion.Inverse(furniture.transform.rotation) * b.size; bc.size = new Vector3(Mathf.Abs(sz.x), Mathf.Abs(sz.y), Mathf.Abs(sz.z)) * 0.98f;
            bc.isTrigger = false;
            hs.insideBreath = Src("Respiracion", inside, new Vector3(0, eye, 0), Clip("breath"), 0.3f, false, 0f);
            return hs;
        }

        // ================================================================= BAÑO
        void Bathroom()
        {
            var g = Group("Bano", root);
            float westIn = BathX0 + In, eastIn = SecX0 - In, northIn = NorthZ - In;
            // (18/10) sin bañera: ducha pequeña en la esquina NE, el baño queda caminable
            Shower(g, eastIn, northIn);
            AgainstWall("toilet", g, new Vector3(0.6f, 0, northIn), 180f, 0f, "Inodoro");
            var sink = AgainstWall("bath_sink", g, new Vector3(westIn, 0, 2.0f), 90f, 0f, "Lavabo");
            AgainstWall("towel_rack", g, new Vector3(westIn, 0, 3.5f), 90f, 1.25f, "Toallero");
            // botiquín hecho a medida (18/10): mueble con volumen, estante, interior claro y puerta con bisagra.
            // La cinta C va de pie en el estante de abajo, con la etiqueta hacia afuera.
            var cabT = Group("Botiquin", g, new Vector3(westIn, 1.15f, 2.72f), 0f);
            const float cW = 0.5f, cH = 0.6f, cD = 0.17f, th = 0.015f;
            var mOut = Lit("M_Botiquin", null, new Color(0.88f, 0.88f, 0.85f), 0.35f);
            var mIn = Lit("M_BotiquinInterior", null, new Color(0.93f, 0.92f, 0.88f), 0.2f);
            Box("Fondo", cabT, new Vector3(th * 0.5f, cH * 0.5f, 0), new Vector3(th, cH, cW), mIn, 1f, true);
            Box("Lado_S", cabT, new Vector3(cD * 0.5f, cH * 0.5f, -cW * 0.5f + th * 0.5f), new Vector3(cD, cH, th), mOut, 1f, true);
            Box("Lado_N", cabT, new Vector3(cD * 0.5f, cH * 0.5f, cW * 0.5f - th * 0.5f), new Vector3(cD, cH, th), mOut, 1f, true);
            Box("Base", cabT, new Vector3(cD * 0.5f, th * 0.5f, 0), new Vector3(cD, th, cW), mIn, 1f, true);
            Box("Techo", cabT, new Vector3(cD * 0.5f, cH - th * 0.5f, 0), new Vector3(cD, th, cW), mOut, 1f, true);
            Box("Estante", cabT, new Vector3(cD * 0.5f, 0.31f, 0), new Vector3(cD - th, 0.012f, cW - 2 * th), mIn, 1f, true);
            Prim(PrimitiveType.Cylinder, "Frasco_1", cabT, new Vector3(0.07f, 0.316f + 0.045f, 0.12f), new Vector3(0.045f, 0.045f, 0.045f), Lit("M_Frasco", null, new Color(0.55f, 0.32f, 0.12f), 0.6f), false);
            Prim(PrimitiveType.Cylinder, "Frasco_2", cabT, new Vector3(0.07f, 0.316f + 0.035f, 0.05f), new Vector3(0.04f, 0.035f, 0.04f), Lit("M_Frasco2", null, new Color(0.9f, 0.9f, 0.95f), 0.4f), false);
            var cabLight = MakeLight("Luz_Botiquin", cabT, LightType.Point, new Vector3(0.1f, 0.25f, 0), new Color(1f, 0.92f, 0.8f), 0.35f, 0.6f, LightShadows.None);
            // puerta: pivote en la bisagra (borde sur, al frente)
            var hinge = new GameObject("Puerta_Botiquin").transform; hinge.SetParent(cabT, false); hinge.localPosition = new Vector3(cD, 0, -cW * 0.5f);
            Box("Hoja", hinge, new Vector3(0.01f, cH * 0.5f, cW * 0.5f), new Vector3(0.02f, cH, cW), mOut, 1f, true);
            var cq = Quad("Vidrio_Botiquin", hinge, new Vector3(0.0215f, cH * 0.5f, cW * 0.5f), new Vector2(cW - 0.06f, cH - 0.06f), -90f, mGlass);
            Box("Tirador", hinge, new Vector3(0.03f, cH * 0.5f, cW - 0.04f), new Vector3(0.02f, 0.08f, 0.015f), PaletteMat("Metal"), 1f, false);
            var cab = cabT.gameObject;
            var cb = WorldBounds(cab);
            var tc = VhsVisual(cabT, "Cinta_C", "C", "porche");
            tc.transform.localPosition = new Vector3(0.06f, th + 0.0515f + 0.002f, -0.05f);
            tc.transform.localRotation = Quaternion.Euler(0, -90f, 0);    // etiqueta mirando a +X (hacia el jugador)
            GrabBox(tc, new Vector3(0.24f, 0.15f, 0.05f));
            tapeC = Item(tc, "tape_c", "Cinta VHS «C»", "VHS tape \"C\"", "«C — porche». Detrás del espejo, envuelta en una toalla húmeda. Papá no escondía cosas en el baño.",
                         "\"C — porch\". Behind the mirror, wrapped in a damp towel. Dad never hid things in the bathroom.", true, "Cinta C", "Tape C");
            var dr = MakeDrawer(hinge, "botiquin", "botiquín", "cabinet", FrontDir(90f), true, 105f, tapeC);
            if (dr) tapeC.available = false;
            // espejo sobre el lavabo: marco + superficie que refleja (quad mirando a +X)
            var sb = WorldBounds(sink);
            float mirY = Mathf.Max(sb.max.y + 0.42f, 1.45f);
            Box("Marco_Espejo", g, new Vector3(westIn + 0.012f, mirY, 2.0f), new Vector3(0.024f, 0.74f, 0.58f), mWoodDark, 1f, true);
            var mq = Quad("Espejo", g, new Vector3(westIn + 0.026f, mirY, 2.0f), new Vector2(0.5f, 0.66f), -90f, mGlass);
            // rostro del susto: aparición controlada DELANTE del espejo (siempre visible, sin depender de luces ni capas)
            var face = Quad("Cara_Susto", g, new Vector3(westIn + 0.034f, mirY + 0.02f, 2.0f), new Vector2(0.46f, 0.575f), -90f, Unlit("M_ScareFace", "scare_face", true));
            face.SetActive(false);
            // lavabo interactivo: [E] Lavarse la cara
            var basin = sink.AddComponent<WashBasin>();
            var wp = new GameObject("Pose_Lavabo"); wp.transform.SetParent(g, false);
            wp.transform.SetPositionAndRotation(new Vector3(westIn + Mathf.Max(0.55f, sb.size.x + 0.3f), 0f, 2.0f), Quaternion.Euler(0, -90f, 0));
            basin.standPose = wp.transform;
            var tap = new GameObject("Grifo"); tap.transform.SetParent(g, false); tap.transform.position = new Vector3(westIn + 0.12f, sb.max.y + 0.05f, 2.0f);
            basin.tap = tap.transform;
            var mg = new GameObject("Reflejo"); mg.transform.SetParent(g, false);
            mirror = mg.AddComponent<MirrorReflection>();
            mirror.surface = mq.GetComponent<Renderer>();
            mirror.basin = basin; mirror.scareFace = face.transform;
            var mc = new GameObject("Camara_Espejo"); mc.transform.SetParent(mg.transform, false);
            mirror.mirrorCam = mc.AddComponent<Camera>(); mirror.mirrorCam.fieldOfView = 60f; mirror.mirrorCam.farClipPlane = 12f;
            mirror.mirrorCam.clearFlags = CameraClearFlags.SolidColor; mirror.mirrorCam.backgroundColor = Color.black;
            mirror.zone = "bano";
            bathroomInside = Src("Sonido_Bano", g, new Vector3(1.4f, 1.2f, 2.4f), null, 1f, false, 1f, 1f, 10f);
        }

        /// <summary>Ducha pequeña en la esquina noreste del baño: plato, mamparas, desagüe, grifo y cabezal.</summary>
        void Shower(Transform g, float eastIn, float northIn)
        {
            const float S = 0.85f;
            var root = Group("Ducha", g, new Vector3(eastIn - S * 0.5f, 0, northIn - S * 0.5f), 0f);
            var white = Lit("M_Ceramica", null, new Color(0.93f, 0.93f, 0.92f), 0.6f);
            var chrome = PaletteMat("Metal");
            Box("Plato", root, new Vector3(0, 0.03f, 0), new Vector3(S, 0.06f, S), white, 1f, true);
            Prim(PrimitiveType.Cylinder, "Desague", root, new Vector3(0.1f, 0.062f, 0.1f), new Vector3(0.07f, 0.002f, 0.07f), chrome, false);
            // mampara oeste (entera) y sur (mitad): la entrada queda en la esquina SO
            Box("Mampara_O", root, new Vector3(-S * 0.5f + 0.005f, 1.0f, 0), new Vector3(0.01f, 1.9f, S), mGlass, 1f, true);
            Box("Mampara_S", root, new Vector3(0.18f, 1.0f, -S * 0.5f + 0.005f), new Vector3(S * 0.5f, 1.9f, 0.01f), mGlass, 1f, true);
            Box("Perfil_O", root, new Vector3(-S * 0.5f + 0.005f, 1.96f, 0), new Vector3(0.025f, 0.025f, S), chrome, 1f, false);
            Box("Perfil_S", root, new Vector3(0.18f, 1.96f, -S * 0.5f + 0.005f), new Vector3(S * 0.5f, 0.025f, 0.025f), chrome, 1f, false);
            // grifería en la pared este y cabezal
            Box("Barra", root, new Vector3(S * 0.5f - 0.03f, 1.45f, 0.1f), new Vector3(0.02f, 0.9f, 0.02f), chrome, 1f, false);
            Prim(PrimitiveType.Cylinder, "Cabezal", root, new Vector3(S * 0.5f - 0.12f, 1.92f, 0.1f), new Vector3(0.16f, 0.012f, 0.16f), chrome, false);
            Box("Brazo", root, new Vector3(S * 0.5f - 0.07f, 1.93f, 0.1f), new Vector3(0.1f, 0.02f, 0.02f), chrome, 1f, false);
            Prim(PrimitiveType.Cylinder, "Grifo", root, new Vector3(S * 0.5f - 0.03f, 1.05f, 0.1f), new Vector3(0.06f, 0.02f, 0.06f), chrome, false, new Vector3(0, 0, 90f));
        }

        // ================================================================= PASILLO
        void Hallway()
        {
            var g = Group("Pasillo", root);
            float northIn = HallZ - In, southIn = -HallZ + In;
            // armario del pasillo que tapa la puerta 06 (se desliza hacia -X)
            var hw = Model("hall_wardrobe", g, Vector3.zero, 180f, "Armario_Pasillo");
            var hb = WorldBounds(hw);
            float sx = Mathf.Min(1f, 1.35f / Mathf.Max(0.1f, hb.size.x));
            hw.transform.localScale = new Vector3(sx, 1f, 1f);
            PlaceByBounds(hw, new Vector3(DoorMetalX, 0, northIn - WorldBounds(hw).extents.z - 0.005f), 0f);
            var holder = Group("Armario_Deslizante", g);
            holder.position = Vector3.zero;
            hw.transform.SetParent(holder, true);
            wardrobe = holder.gameObject.AddComponent<SlidingWardrobe>();
            hb = WorldBounds(hw);
            wardrobe.slideOffset = new Vector3(-(hb.size.x + 0.05f), 0, 0);
            // cerradura pequeña en el frente
            var kh = Box("Cerradura", hw.transform, Vector3.zero, new Vector3(0.05f, 0.07f, 0.02f), PaletteMat("Brass"), 1f);
            kh.transform.position = new Vector3(hb.center.x + 0.1f, 1.12f, hb.min.z - 0.008f);
            kh.transform.rotation = Quaternion.identity;
            kh.transform.localScale = new Vector3(1f / sx, 1f, 1f);
            wardrobeKeyhole = kh.AddComponent<ItemTarget>();
            wardrobeKeyhole.itemId = "key"; wardrobeKeyhole.eventId = "wardrobe_unlocked";
            wardrobeKeyhole.useEs = "Usar la llave 06"; wardrobeKeyhole.useEn = "Use the 06 key"; wardrobeKeyhole.lookEs = "Examinar cerradura"; wardrobeKeyhole.lookEn = "Examine lock";
            wardrobeKeyhole.noItemEs = "Una cerradura diminuta en un armario que nunca se abrió. Necesito una llave."; wardrobeKeyhole.noItemEn = "A tiny lock on a wardrobe that was never opened. I need a key.";

            // foto del pasillo
            var ph = Model("photo_frame_wall", g, Vector3.zero, 180f, "Foto_Pasillo");
            PlaceByBounds(ph, new Vector3(-0.9f, 0, northIn - 0.012f), 1.35f);
            var pic = Part(ph, "Picture");
            if (pic) foreach (var r in pic.GetComponentsInChildren<Renderer>()) r.sharedMaterial = Lit("M_PhotoHall", "photo_hall_2014", Color.white, 0.2f);
            SingleBox(ph);
            photoHall = Item(ph, "photo_hall", "Foto del pasillo", "Hallway photo", "Papá, mamá y yo en este mismo pasillo, en 2014. Al fondo hay una puerta con un «06» pintado... donde hoy está el armario.",
                             "Dad, mom and me in this same hallway, 2014. In the back there's a door with a painted \"06\"... where the wardrobe stands today.", false, null, null, 0.3f);
            photoHall.clueId = "photo2014"; photoHall.clueEs = "En una foto de 2014 se ve una puerta «06» donde hoy está el armario del pasillo."; photoHall.clueEn = "A 2014 photo shows a door marked \"06\" where the hallway wardrobe stands today.";

            Model("wall_clock", g, Vector3.zero, -90f, "Reloj_Pasillo").transform.position = new Vector3(EastX - In - 0.03f, 1.9f, 0f);
            Put("side_table", -3.3f, southIn + 0.19f, 0f, 0f, "Consola_Pasillo", g);
            Put("plant", -3.75f, northIn - 0.2f, 0f, 0f, "Planta_Pasillo", g);

            hallStepPath = new[] { Group("Paso_0", g, new Vector3(-3.4f, 0, 0)), Group("Paso_1", g, new Vector3(4.2f, 0, 0)) };
            secretWall = Src("Muro_06", g, new Vector3(DoorMetalX, 1.3f, HallZ + 0.4f), null, 1f, false, 1f, 1f, 10f);
        }

        // ================================================================= HABITACIÓN 06
        void Secret()
        {
            // (18/10) Habitación 06 AMPLIADA: x 2.5 → 6.6, z 0.8 → 5.3 (4,1 × 4,5 m). Centro libre y estaciones con cartel:
            //   entrada (sur): casillero a la izquierda, archivador a la derecha · oeste: MESA DE GRABACIONES (grabadora + cinta, lámpara)
            //   norte: escritorio con 3 monitores, plano y notas + silla · noreste: PALANCA AUXILIAR · este: CONSOLA DE AISLAMIENTO
            var g = Group("Habitacion_06", root);
            float westIn = SecX0 + In, eastIn = Sec06E - In, northIn = Sec06N - In;
            const float deskX = 4.4f;
            var desk = AgainstWall("secret_desk", g, new Vector3(deskX, 0, northIn), 180f, 0f, "Escritorio_06");
            float top = SurfaceY(new Vector3(deskX, 2.5f, northIn - 0.3f), 0.76f);
            float[] xs = { deskX - 0.58f, deskX, deskX + 0.58f };
            for (int i = 0; i < 3; i++)
            {
                var m = Put("crt_monitor", xs[i], northIn - 0.32f, 180f, top, "Monitor_06_" + i, g);
                secretMonitors[i] = Monitor(m, 180f, new Vector2(0.29f, 0.22f), 0.55f, "secreta");
            }
            Text3D("Cartel", g, new Vector3(deskX, 2.2f, northIn - 0.01f), 0f, "GUARDIÁN · NODO 06", 0.01f, new Color(0.8f, 0.2f, 0.15f));
            Put("chair", deskX, northIn - 1.05f, 0f, 0f, "Silla_06", g);
            Put("cable_bundle", deskX + 0.9f, northIn - 0.9f, 20f, 0f, "Cables", g);
            // documentos sobre el escritorio
            plan = Doc("plan", "blueprint", null, new Vector3(deskX + 0.05f, top + 0.005f, northIn - 0.6f), 190f, new Vector2(0.42f, 0.3f),
                "Plano de la casa", "House blueprint", "Plano de obra de 1998. La casa tiene SEIS habitaciones. En la versión que conozco solo hay cinco. La sexta está marcada en rojo: «06 — GUARDIÁN».",
                "1998 building plan. The house has SIX rooms. The one I know only has five. The sixth is marked in red: \"06 — GUARDIAN\".", g);
            plan.clueId = "plan06"; plan.clueEs = "El plano original muestra una sexta habitación: la 06, sede de GUARDIÁN."; plan.clueEn = "The original plan shows a sixth room: 06, home of GUARDIAN.";
            fatherNotes = Doc("fatherNotes", "note_father31", null, new Vector3(deskX + 0.6f, top + 0.005f, northIn - 0.6f), 170f, new Vector2(0.21f, 0.28f),
                "Notas de papá", "Dad's notes", "«Día 31. La casa no deja entrar a nadie: los HACE. Aparecen en una habitación sin cruzar ninguna puerta. Una copia no recuerda haber llegado; recuerda haber estado siempre. Si GUARDIÁN cuenta dos... uno de ellos no nació. (Día 4: el primero salió mal. S-0. Sin cara. Imita la voz de M. No le abras.)»",
                "\"Day 31. The house doesn't let anyone in: it MAKES them. They appear in a room without crossing any door. A copy doesn't remember arriving; it remembers having always been there. If GUARDIAN counts two... one of them was never born. (Day 4: the first one went wrong. S-0. No face. It imitates M's voice. Don't open the door to it.)\"", g);

            // ---- MESA DE GRABACIONES (pared oeste): grabadora + cinta de carrete bien visible, con lámpara
            const float tz = 3.0f, tD = 0.6f, tW = 1.3f, tH = 0.78f;
            var mesa = Group("Mesa_Grabaciones", g, new Vector3(westIn + tD * 0.5f, 0, tz), 0f);
            Box("Tablero", mesa, new Vector3(0, tH - 0.02f, 0), new Vector3(tD, 0.04f, tW), mWoodDark, 1f, true);
            foreach (var lp in new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(1, 1) })
                Box("Pata", mesa, new Vector3(lp.x * (tD * 0.5f - 0.04f), (tH - 0.04f) * 0.5f, lp.y * (tW * 0.5f - 0.04f)), new Vector3(0.04f, tH - 0.04f, 0.04f), mWoodDark, 1f, true);
            var rr = Put("reel_recorder", westIn + 0.27f, tz + 0.2f, 90f, tH, "Grabadora", g);
            SingleBox(rr);
            recorderSlot = rr.AddComponent<ItemTarget>();
            recorderSlot.itemId = "reel"; recorderSlot.eventId = "reel_loaded";
            recorderSlot.useEs = "Colocar la cinta de carrete"; recorderSlot.useEn = "Load the reel tape"; recorderSlot.lookEs = "Examinar grabadora"; recorderSlot.lookEn = "Examine recorder";
            recorderSlot.noItemEs = "La grabadora de papá. Le falta la cinta: debería estar aquí cerca, en la mesa."; recorderSlot.noItemEn = "Dad's recorder. The tape is missing: it should be close by, on the table.";
            var recBtn = Box("Boton_Play", rr.transform, Vector3.zero, new Vector3(0.03f, 0.015f, 0.03f), mBeige, 1f);
            recBtn.transform.position = WorldBounds(rr).center + new Vector3(0.1f, 0.08f, 0.12f);
            recorder = recBtn.AddComponent<StoryInteractable>(); recorder.eventId = "recorder_play"; recorder.available = false;
            recorder.promptEs = "Reproducir"; recorder.promptEn = "Play";
            // cinta de carrete: de pie (se ve el disco entero), 1,7× más grande, con collider amplio
            var reelRoot = new GameObject("Cinta_Carrete"); reelRoot.transform.SetParent(g, false);
            reelRoot.transform.position = new Vector3(westIn + 0.2f, tH + 0.14f, tz - 0.3f);
            var rl = Model("tape_reel", reelRoot.transform, Vector3.zero, 0f, "Disco", false);
            rl.transform.localScale *= 1.7f;
            rl.transform.localRotation = Quaternion.Euler(0, 0, 80f);   // el disco mira hacia la habitación (+X), apenas inclinado
            var rlb = WorldBounds(rl); rl.transform.position += new Vector3(0, (tH + 0.003f) - rlb.min.y, 0) + (reelRoot.transform.position - rlb.center).x * Vector3.right;
            var rbc = reelRoot.AddComponent<BoxCollider>(); rbc.center = reelRoot.transform.InverseTransformPoint(WorldBounds(rl).center); rbc.size = new Vector3(0.14f, 0.34f, 0.34f);
            reel = Item(reelRoot, "reel", "Cinta de carrete", "Reel tape", "Etiqueta: «PARA MI HIJO. SI GUARDIÁN CUENTA DOS».", "Label: \"FOR MY SON. IF GUARDIAN COUNTS TWO\".", true, "Cinta de carrete", "Reel tape");
            reel.available = false;
            var tag = Text3D("Etiqueta_Carrete", g, new Vector3(westIn + 0.03f, tH + 0.42f, tz - 0.3f), -90f, "PARA MI HIJO", 0.0035f, new Color(0.95f, 0.9f, 0.75f));
            NoShadow(tag.gameObject);
            Text3D("Cartel_Grabaciones", g, new Vector3(westIn + 0.012f, 1.75f, tz), -90f, "GRABACIONES", 0.007f, new Color(0.95f, 0.85f, 0.55f));
            var lampL = MakeLight("Lampara_Mesa", g, LightType.Point, new Vector3(westIn + 0.35f, tH + 0.55f, tz - 0.1f), new Color(1f, 0.8f, 0.55f), 1.1f, 1.8f, LightShadows.None);
            var shade = Prim(PrimitiveType.Cylinder, "Pantalla_Lampara", g, new Vector3(westIn + 0.35f, tH + 0.62f, tz - 0.1f), new Vector3(0.18f, 0.06f, 0.18f), Lit("M_LampShade06", null, new Color(0.25f, 0.3f, 0.25f), 0.3f), false);
            Box("Brazo_Lampara", g, new Vector3(westIn + 0.35f, tH + 0.3f, tz - 0.1f), new Vector3(0.02f, 0.6f, 0.02f), mMetal, 1f, false);

            // ---- archivador y corcho (decorado), cerca de la entrada
            AgainstWall("filing_cabinet", g, new Vector3(5.0f, 0, HallZ + In), 0f, 0f, "Archivador");
            AgainstWall("filing_cabinet", g, new Vector3(5.55f, 0, HallZ + In), 0f, 0f, "Archivador_2");
            var cork = Model("cork_board", g, Vector3.zero, 90f, "Corcho_06");
            PlaceByBounds(cork, new Vector3(westIn + 0.025f, 0, 4.4f), 1.2f);

            // ---- PALANCA DE ENERGÍA AUXILIAR (pared este, al fondo, junto a los monitores) con piloto rojo y cartel
            var lev = AgainstWall("generator_lever", g, new Vector3(eastIn, 0, 4.5f), -90f, 1.05f, "Palanca_Auxiliar");
            SingleBox(lev);
            auxLever = lev.AddComponent<StoryInteractable>(); auxLever.eventId = "aux_lever"; auxLever.available = false; auxLever.once = true;
            auxLever.promptEs = "Bajar la palanca: ENERGÍA AUXILIAR"; auxLever.promptEn = "Pull the lever: AUX POWER";
            Text3D("Cartel_Auxiliar", g, new Vector3(eastIn - 0.012f, 1.85f, 4.5f), 90f, "ENERGÍA AUXILIAR", 0.006f, new Color(1f, 0.35f, 0.25f));
            Box("Piloto", g, new Vector3(eastIn - 0.01f, 1.68f, 4.5f), new Vector3(0.02f, 0.04f, 0.04f), mLedFault, 1f, false);

            // ---- CONSOLA DE AISLAMIENTO (pared este, cerca de la entrada)
            var con = AgainstWall("isolation_console", g, new Vector3(eastIn, 0, 2.6f), -90f, 0f, "Consola_Aislamiento");
            isolation = con.AddComponent<IsolationStation>();
            isolation.stationId = "isolation"; isolation.promptEs = "Usar consola de aislamiento"; isolation.promptEn = "Use isolation console";
            isolation.view = View("Vista_Aislamiento", new Vector3(eastIn - 1.25f, 1.5f, 2.6f), WorldBounds(con).center + Vector3.up * 0.25f);
            string[] btn = { "Btn_0", "Btn_1", "Btn_2" };
            string[] lab = { "AISLAR\nCOCINA", "AISLAR\nDORMITORIO", "AISLAR\n06" };
            PushButton[] pbs = new PushButton[3];
            for (int i = 0; i < 3; i++)
            {
                var p = Part(con, btn[i]);
                if (p == null) { p = Box("Boton_" + i, con.transform, Vector3.zero, new Vector3(0.08f, 0.03f, 0.08f), PaletteMat("Red"), 1f).transform; p.position = WorldBounds(con).center + new Vector3(-0.1f, 0.48f, -0.25f + i * 0.25f); }
                pbs[i] = p.gameObject.AddComponent<PushButton>();
                pbs[i].labelEs = lab[i].Replace("\n", " "); pbs[i].labelEn = new[] { "ISOLATE KITCHEN", "ISOLATE BEDROOM", "ISOLATE 06" }[i];
                pbs[i].clip = Clip("click");
                var t = Text3D("Etiqueta_" + i, con.transform, Vector3.zero, 0f, lab[i], 0.0045f, new Color(0.95f, 0.9f, 0.8f));
                t.transform.position = PartCenter(p) + new Vector3(-0.12f, 0.02f, 0f);
                t.transform.rotation = Quaternion.Euler(70f, -90f, 0);
            }
            isolation.isoKitchen = pbs[0]; isolation.isoBedroom = pbs[1]; isolation.isoSelf = pbs[2];
            var scrT = Text3D("Pantalla_Sensores", con.transform, Vector3.zero, 0f, "", 0.0042f, new Color(0.4f, 1f, 0.5f), TextAnchor.UpperLeft);
            var cbb = WorldBounds(con);
            scrT.transform.position = new Vector3(cbb.center.x - 0.05f, cbb.max.y + 0.02f, cbb.center.z - 0.38f);
            scrT.transform.rotation = Quaternion.Euler(60f, -90f, 0);
            isolation.screen = scrT;
            Text3D("Cartel_Aislamiento", g, new Vector3(eastIn - 0.012f, 1.85f, 2.6f), 90f, "AISLAMIENTO", 0.006f, new Color(0.55f, 1f, 0.6f));
            isolationDoc = Doc("isolationDoc", "note_isolation", null, new Vector3(cbb.center.x - 0.1f, cbb.max.y + 0.004f, cbb.center.z + 0.3f), -80f, new Vector2(0.21f, 0.28f),
                "Manual GUARDIÁN", "GUARDIAN manual", "«Protocolo GUARDIÁN\n1. Restaure ALARMA + CÁMARAS + CERRADURAS (7), con LUCES y 06 apagados.\n2. Regrese a la habitación 06 para preparar el aislamiento.\n3. La consola mostrará dos identidades coincidentes. Solo una puede abandonar la casa.\n4. Elija AISLAR AL DOBLE o AISLARME A MÍ MISMO. La decisión es irreversible.»",
                "\"GUARDIAN protocol\n1. Restore ALARM + CAMERAS + LOCKS (7), with LIGHTS and 06 off.\n2. Return to room 06 to prepare isolation.\n3. The console will show two matching identities. Only one may leave the house.\n4. Choose ISOLATE THE DOUBLE or ISOLATE MYSELF. The decision is irreversible.\"", g);
            // ---- casillero (escondite) en la esquina suroeste, junto a la entrada
            var lk = AgainstWall("locker", g, new Vector3(2.86f, 0, HallZ + In), 0f, 0f, "Locker");
            lockerHide = MakeHide(lk, "casillero", "locker", 0f, new[] { "Door" }, 1.5f);
        }

        /// <summary>Rótulo impreso sobre la cara frontal (+Z del mundo, mirando a la sala) de una tecla; se hunde con ella.</summary>
        void KeyLabel(Transform key, string text, float size)
        {
            var b = WorldBounds(key.gameObject);
            var t = Text3D("Rotulo_" + text, key, Vector3.zero, 0f, text, size, new Color(0.96f, 0.96f, 0.9f));   // teclas negras: rótulo claro
            t.fontStyle = FontStyle.Bold;
            t.transform.SetPositionAndRotation(new Vector3(b.center.x, b.center.y, b.max.z + 0.0015f), Quaternion.Euler(0, 180f, 0));
            NoShadow(t.gameObject);
        }

        /// <summary>Altura de la superficie bajo un punto (raycast hacia abajo); 'fallback' si no hay nada.</summary>
        static float SurfaceY(Vector3 from, float fallback)
        {
            Physics.SyncTransforms();
            RaycastHit h;
            if (Physics.Raycast(from, Vector3.down, out h, 5f, ~0, QueryTriggerInteraction.Ignore) && h.point.y > 0.1f) return h.point.y;
            return fallback;
        }

        /// <summary>Punto de vista de una estación (cámara de enfoque).</summary>
        Transform View(string name, Vector3 eye, Vector3 lookAt)
        {
            var t = Group(name, root, eye);
            t.rotation = Quaternion.LookRotation(lookAt - eye, Vector3.up);
            return t;
        }

        // ================================================================= SALA
        void Living()
        {
            var g = Group("Sala", root);
            float northIn = -HallZ - In, southIn = SouthZ + In, westIn = WestX + In, eastIn = KitX0 - In;
            // DISTRIBUCIÓN (corregida): zona de estar al oeste y un pasillo libre de 1.5 m entre el arco (x=-1),
            // la puerta principal (x=-2.15) y el paso a la cocina. La ventana (pared oeste, z=-3) queda accesible.
            const float tvX = -2.5f;
            // TV + videograbadora contra la pared norte (oeste del arco)
            var tv = AgainstWall("tv_stand", g, new Vector3(tvX, 0, northIn), 180f, 0f, "TV");
            var tvB = WorldBounds(tv);
            var tvScreen = Monitor(tv, 180f, new Vector2(0.46f, 0.34f), 0.571f, "sala");
            // la pantalla encaja DENTRO del marco del CRT (antes flotaba más arriba y más grande, tapando la TV y la videograbadora)
            var tvPart = Part(tv, "Screen");
            if (tvPart != null) tvScreen.transform.position = PartCenter(tvPart) + Quaternion.Euler(0, 180f, 0) * Vector3.forward * 0.006f;
            var tvCol = tvScreen.GetComponent<Collider>(); if (tvCol) Object.DestroyImmediate(tvCol);   // no bloquea clics ni la mira
            float standTop = SurfaceY(new Vector3(tvX, 2.0f, tvB.min.z + 0.15f), 0.5f);
            if (standTop > 1.0f) standTop = 0.5f;
            var vcrM = Put("vcr_deck", tvX, tvB.min.z + 0.2f, 180f, standTop, "Videograbadora", g);
            BuildVCR(g, vcrM, tvScreen);
            // sofá frente a la TV, mesa ratona y sillón
            Put("sofa", tvX, -3.6f, 0f, 0f, "Sofa", g);                       // mira a la TV, separado de la ventana
            Put("coffee_table", tvX, -2.45f, 0f, 0f, "Mesa_Ratona", g);
            Put("armchair", westIn + 0.55f, southIn + 0.5f, 45f, 0f, "Sillon", g);  // rincón suroeste, fuera del paso
            var fl = Put("floor_lamp", westIn + 0.25f, -3.95f, 45f, 0f, "Lampara_Pie", g);
            var flL = MakeLight("Luz_Lampara_Pie", fl.transform, LightType.Point, Vector3.zero, new Color(1f, 0.78f, 0.5f), 1.4f, 5f, LightShadows.None);
            flL.transform.position = WorldBounds(fl).center + Vector3.up * 0.6f;
            var flr = flL.gameObject.AddComponent<RoomLight>(); flr.zone = "sala"; flr.lightSource = flL; roomLights.Add(flr);
            var flBulb = Part(fl, "Bulb"); if (flBulb) { flr.bulb = flBulb.GetComponentInChildren<Renderer>(); flr.onMat = mBulbOn; flr.offMat = mBulbOff; }
            var flT = fl.AddComponent<Toggleable>(); flT.nounEs = "lámpara de pie"; flT.nounEn = "floor lamp"; flT.lamp = flr;
            AgainstWall("tall_bookcase", g, new Vector3(westIn, 0, -1.55f), 90f, 0f, "Biblioteca");
            Put("plant", eastIn - 0.25f, southIn + 0.25f, 0f, 0f, "Planta_Sala", g);
            var wc = Model("wall_clock", g, Vector3.zero, 180f, "Reloj_Sala"); PlaceByBounds(wc, new Vector3(tvX, 0, northIn - 0.03f), 1.95f);

            // cómoda (recibo en el cajón 0, cinta A en el cajón 1) + foto familiar
            var dr = AgainstWall("dresser", g, new Vector3(0.3f, 0, northIn), 180f, 0f, "Comoda");
            var drB = WorldBounds(dr);
            receipt = Doc("receipt", "receipt_front", "receipt_back", Vector3.zero, 180f, new Vector2(0.09f, 0.18f), "Recibo del cerrajero", "Locksmith receipt",
                "«Cerrajería Ramírez — Cambio de cerradura y cerrojo interior. Fecha: 23/08.» Hay algo escrito atrás.",
                "\"Ramírez Locksmiths — Lock and inner deadbolt replacement. Date: 23/08.\" There's something written on the back.", g);
            receipt.backTextEs = "Con la letra de mamá: «Nuevo PIN de la alarma = día y mes del cambio de cerradura. No lo anotes en ningún otro lado.»";
            receipt.backTextEn = "In mom's handwriting: \"New alarm PIN = day and month of the lock change. Don't write it anywhere else.\"";
            receipt.collect = true; receipt.invNameEs = "Recibo"; receipt.invNameEn = "Receipt";
            receipt.clueId = "pin"; receipt.clueEs = "PIN de la alarma: 2308 (día en que cambiaron la cerradura)."; receipt.clueEn = "Alarm PIN: 2308 (the day the lock was changed)."; receipt.clueOnBack = true;
            var ta = Model("vhs_tape", g, Vector3.zero, 180f, "Cinta_A");
            SingleBox(ta);
            tapeA = Item(ta, "tape_a", "Cinta VHS «A»", "VHS tape \"A\"", "«A — cocina». Debajo de los manteles de la abuela. Alguien la puso aquí hace poco: no tiene polvo.",
                         "\"A — kitchen\". Under grandma's tablecloths. Someone put it here recently: there's no dust on it.", true, "Cinta A", "Tape A");
            var d0 = Part(dr, "Drawer_2"); var d1 = Part(dr, "Drawer_1");   // Drawer_2 = el de ARRIBA (recibo), Drawer_1 = el del medio (cinta A)
            if (d0) { receipt.transform.SetParent(d0, true); receipt.transform.position = PartCenter(d0) + Vector3.up * 0.02f; }
            if (d1) { ta.transform.SetParent(d1, true); ta.transform.position = PartCenter(d1) + Vector3.up * 0.01f; ta.transform.rotation = Quaternion.Euler(90f, 20f, 0); }
            MakeDrawer(d0, "comoda_0", "cajón de arriba", "top drawer", FrontDir(180f), false, 0.3f, receipt);
            MakeDrawer(d1, "comoda_1", "cajón del medio", "middle drawer", FrontDir(180f), false, 0.3f, tapeA);
            MakeDrawer(Part(dr, "Drawer_0"), "comoda_2", "cajón de abajo", "bottom drawer", FrontDir(180f), false, 0.3f);
            receipt.available = false; tapeA.available = false;
            var pf = Put("photo_frame_stand", drB.center.x - 0.25f, drB.center.z, 200f, drB.max.y, "Foto_Familia", g);
            var pic = Part(pf, "Picture");
            if (pic) { photoFamilyPicture = pic.GetComponentInChildren<Renderer>(); photoFamilyPicture.sharedMaterial = Lit("M_PhotoFamily", "photo_family", Color.white, 0.2f); }
            SingleBox(pf);
            photoFamily = Item(pf, "photo_family", "Foto familiar", "Family photo", "Mamá, papá y yo frente a la casa. Papá tiene la mano en mi hombro.", "Mom, dad and me in front of the house. Dad has his hand on my shoulder.", false, null, null, 0.26f);

            // escritorio de vigilancia contra la pared este (sur del paso a la cocina)
            var sd = AgainstWall("security_desk", g, new Vector3(eastIn, 0, -3.65f), -90f, 0f, "Escritorio_Camaras");
            float sdTop = TopY(sd);
            var cm = Put("crt_monitor", WorldBounds(sd).center.x + 0.06f, -3.65f, -90f, sdTop, "Monitor_Camaras", g);
            var camMon = Monitor(cm, -90f, new Vector2(0.29f, 0.22f), 0.55f, "sala");
            var sw = Put("camera_switcher", WorldBounds(sd).center.x - 0.18f, -3.65f, -90f, sdTop, "Selector_Camaras", g);
            cameraDesk = sd.AddComponent<CameraDeskStation>();
            cameraDesk.stationId = "cameras"; cameraDesk.promptEs = "Usar monitor de cámaras"; cameraDesk.promptEn = "Use camera monitor";
            cameraDesk.monitor = camMon; cameraDesk.index = 1;
            cameraDesk.view = View("Vista_Camaras", new Vector3(-0.05f, 1.32f, -3.65f), PartCenter(cm.transform) + new Vector3(0, 0.02f, 0));
            for (int i = 0; i < 4; i++)
            {
                var p = Part(sw, "Btn_" + i);
                if (p == null) continue;
                var pb = p.gameObject.AddComponent<PushButton>(); pb.labelEs = "CAM 0" + (i + 1); pb.labelEn = "CAM 0" + (i + 1); pb.clip = Clip("click");
                cameraDesk.camButtons[i] = pb;
            }
            var mb = Part(sw, "Btn_4");
            if (mb) { cameraDesk.modeButton = mb.gameObject.AddComponent<PushButton>(); cameraDesk.modeButton.labelEs = "ARCHIVO / DIRECTO"; cameraDesk.modeButton.labelEn = "ARCHIVE / LIVE"; cameraDesk.modeButton.clip = Clip("click"); }
            Text3D("Rotulo", sw.transform, Vector3.zero, 0f, "1   2   3   4   A/D", 0.0035f, Color.white).transform
                .SetPositionAndRotation(WorldBounds(sw).center + new Vector3(0.07f, 0.045f, 0), Quaternion.Euler(70f, -90f, 0));

            // perchero con el abrigo de papá (llave en el bolsillo)
            var cr = Put("coat_rack", -0.45f, southIn + 0.3f, 0f, 0f, "Perchero", g);   // junto a la puerta, sin tapar el panel de alarma
            var coat = Part(cr, "Coat");
            var cp = new GameObject("Bolsillo_Abrigo"); cp.transform.SetParent(cr.transform, false);
            cp.transform.position = coat != null ? PartCenter(coat) : WorldBounds(cr).center;
            var cpc = cp.AddComponent<BoxCollider>(); cpc.size = new Vector3(0.45f, 0.6f, 0.45f);
            coatPocket = cp.AddComponent<SearchSpot>(); coatPocket.promptEs = "Revisar el abrigo de papá"; coatPocket.promptEn = "Search dad's coat"; coatPocket.available = false;
            coatPocket.emptyEs = "Huele a su colonia. No hay nada más."; coatPocket.emptyEn = "It smells like his cologne. Nothing else.";
            var kg = Box("Llave_06", cp.transform, Vector3.zero, new Vector3(0.05f, 0.012f, 0.02f), PaletteMat("Brass"), 1f);
            kg.transform.position = cp.transform.position; kg.SetActive(true);
            key = Item(kg, "key", "Llave pequeña", "Small key", "Una llave diminuta de bronce. Tiene grabado «06».", "A tiny brass key. It has \"06\" engraved on it.", true, "Llave 06", "06 key", 0.12f);
            key.available = false;
            foreach (var r in kg.GetComponentsInChildren<Renderer>()) r.enabled = false;
            coatPocket.hidden = key;   // (faltaba: el bolsillo no tenía la llave asignada y decía "no hay nada")
            coatPocket.emptyEs = "Huele a su colonia. Ya no hay nada más en los bolsillos."; coatPocket.emptyEn = "It smells like his cologne. Nothing else in the pockets.";

            // panel de alarma junto a la puerta
            var ap = Model("alarm_panel", g, Vector3.zero, 0f, "Panel_Alarma");
            PlaceByBounds(ap, new Vector3(-1.25f, 0, southIn + 0.04f), 1.25f);
            alarmKeypad = ap.AddComponent<AlarmKeypadStation>();
            alarmKeypad.stationId = "alarm"; alarmKeypad.promptEs = "Usar panel de alarma"; alarmKeypad.promptEn = "Use alarm panel";
            var apB = WorldBounds(ap);
            alarmKeypad.view = View("Vista_Alarma", new Vector3(apB.center.x, apB.center.y + 0.05f, apB.center.z + 0.5f), apB.center);
            for (int i = 0; i < 10; i++)
            {
                var p = Part(ap, "Btn_" + i); if (p == null) continue;
                var pb = p.gameObject.AddComponent<PushButton>(); pb.key = i.ToString(); pb.labelEs = pb.labelEn = i.ToString(); pb.clip = Clip("key_beep");
                alarmKeypad.digits[i] = pb;
            }
            var bs = Part(ap, "Btn_s"); if (bs) { alarmKeypad.clearBtn = bs.gameObject.AddComponent<PushButton>(); alarmKeypad.clearBtn.labelEs = "* BORRAR"; alarmKeypad.clearBtn.labelEn = "* CLEAR"; alarmKeypad.clearBtn.clip = Clip("key_beep"); }
            var bh = Part(ap, "Btn_h"); if (bh) { alarmKeypad.okBtn = bh.gameObject.AddComponent<PushButton>(); alarmKeypad.okBtn.labelEs = "# ACTIVAR"; alarmKeypad.okBtn.labelEn = "# ARM"; alarmKeypad.okBtn.clip = Clip("key_beep"); }
            if (alarmKeypad.clearBtn) { alarmKeypad.clearBtn.labelEs = "C · BORRAR"; alarmKeypad.clearBtn.labelEn = "C · CLEAR"; }
            if (alarmKeypad.okBtn) { alarmKeypad.okBtn.labelEs = "OK · ACTIVAR"; alarmKeypad.okBtn.labelEn = "OK · ARM"; }
            // números impresos en cada tecla (antes las teclas estaban en blanco)
            for (int i = 0; i < 10; i++) if (alarmKeypad.digits[i]) KeyLabel(alarmKeypad.digits[i].transform, i.ToString(), 0.0052f);
            if (alarmKeypad.clearBtn) KeyLabel(alarmKeypad.clearBtn.transform, "C", 0.0052f);
            if (alarmKeypad.okBtn) KeyLabel(alarmKeypad.okBtn.transform, "OK", 0.0036f);
            var instr = Text3D("Instrucciones_Alarma", ap.transform, Vector3.zero, 0f, "GUARDIÁN\nPIN de 4 cifras + OK", 0.0022f, new Color(0.85f, 0.85f, 0.8f));
            instr.transform.SetPositionAndRotation(new Vector3(apB.center.x, apB.max.y + 0.035f, apB.max.z - 0.01f), Quaternion.Euler(0, 180f, 0));
            // texto centrado DENTRO de la pantalla negra del panel (en el modelo: 22 x 10 cm, 11 cm sobre el centro)
            var aps = Text3D("Pantalla_Alarma", ap.transform, Vector3.zero, 0f, "", 0.0023f, new Color(0.35f, 1f, 0.45f));
            aps.transform.SetPositionAndRotation(new Vector3(apB.center.x, apB.center.y + 0.11f, apB.max.z + 0.004f), Quaternion.Euler(0, 180f, 0));
            alarmKeypad.screen = aps;

            frontDoorOutside = Src("Exterior_Puerta", g, new Vector3(FrontX, 1.2f, SouthZ - 0.7f), null, 1f, false, 1f, 1.5f, 16f);
        }

        void BuildVCR(Transform g, GameObject vcrM, MonitorScreen tv)
        {
            SingleBox(vcrM);
            vcr = vcrM.AddComponent<VCRStation>();
            vcr.stationId = "vcr"; vcr.promptEs = "Usar videograbadora"; vcr.promptEn = "Use VCR";
            vcr.tv = tv;
            var vb = WorldBounds(vcrM);
            vcr.view = View("Vista_VCR", new Vector3(vb.center.x, 1.05f, vb.center.z - 1.25f), new Vector3(vb.center.x, 0.85f, vb.center.z + 0.1f));
            // ranura y EJECT
            var slot = Box("Ranura", vcrM.transform, Vector3.zero, new Vector3(0.2f, 0.025f, 0.02f), mBlack, 1f);
            slot.transform.SetPositionAndRotation(new Vector3(vb.center.x - 0.05f, vb.center.y + 0.01f, vb.min.z - 0.008f), Quaternion.identity);
            vcr.slot = slot.AddComponent<VcrSlot>(); vcr.slot.labelEs = "Insertar cinta"; vcr.slot.labelEn = "Insert tape";
            var ej = Box("Eject", vcrM.transform, Vector3.zero, new Vector3(0.03f, 0.015f, 0.015f), mBeige, 1f);
            ej.transform.SetPositionAndRotation(new Vector3(vb.center.x + 0.14f, vb.center.y + 0.01f, vb.min.z - 0.008f), Quaternion.identity);
            vcr.ejectBtn = ej.AddComponent<PushButton>(); vcr.ejectBtn.labelEs = "EJECT"; vcr.ejectBtn.labelEn = "EJECT"; vcr.ejectBtn.clip = Clip("click");
            // cajas de cintas para ordenar (aparecen al verlas)
            string[] ids = { "tape_a", "tape_b", "tape_c" }; string[] letters = { "A", "B", "C" };
            string[] stamps = { "22:47", "01:24", "22:10" };
            string[] clueEs = { "Cinta A (22:47): el chico que llegó a las 22:10 está en la cocina. Se sirve agua. Mira hacia el pasillo, como si oyera algo.",
                                "Cinta B (01:24): el pasillo. Una figura con mi ropa camina hacia la cámara y se detiene. Pero la cinta marca una hora que todavía no pasó.",
                                "Cinta C (22:10): alguien igual a mí llega por el camino, abre la puerta principal con su llave y entra." };
            string[] clueEn = { "Tape A (22:47): the boy who arrived at 22:10 is in the kitchen. He pours some water. He looks toward the hallway, as if he heard something.",
                                "Tape B (01:24): the hallway. A figure in my clothes walks toward the camera and stops. But the tape shows a time that hasn't happened yet.",
                                "Tape C (22:10): someone identical to me walks up the path, opens the front door with his key and comes in." };
            vcr.tapes = new TapeData[3];
            for (int i = 0; i < 3; i++)
            {
                var box = Model("vhs_tape", g, Vector3.zero, 180f, "Caja_" + letters[i]);
                PlaceByBounds(box, new Vector3(vb.center.x - 0.42f + i * 0.13f, 0, vb.center.z), vb.min.y);
                SingleBox(box);
                var tbx = box.AddComponent<TapeBox>(); tbx.labelEs = "Cinta " + letters[i] + " (" + stamps[i] + ")"; tbx.labelEn = "Tape " + letters[i] + " (" + stamps[i] + ")";
                var lt = Text3D("Letra", box.transform, Vector3.zero, 0f, letters[i], 0.01f, Color.white);
                lt.transform.SetPositionAndRotation(WorldBounds(box).center + Vector3.forward * -0.02f, Quaternion.identity);
                vcr.tapes[i] = new TapeData { id = ids[i], letter = letters[i], stamp = stamps[i], clueEs = clueEs[i], clueEn = clueEn[i], box = box, duration = 14f };
            }
        }

        // ================================================================= COCINA
        void Kitchen()
        {
            var g = Group("Cocina", root);
            float southIn = SouthZ + In, eastIn = EastX - In, northIn = -HallZ - In;
            var kb = AgainstWall("kitchen_base", g, new Vector3(3.2f, 0, southIn), 0f, 0f, "Mesada");
            var kbB = WorldBounds(kb);
            float counter = SurfaceY(new Vector3(2.4f, 2.0f, kbB.center.z), 0.92f);
            if (counter > 1.05f) counter = 0.92f;
            kitchenDrawerSlam = MakeDrawer(Part(kb, "Drawer_0"), "cocina_0", "cajón", "drawer", FrontDir(0f), false, 0.28f);
            MakeDrawer(Part(kb, "Drawer_1"), "cocina_1", "cajón", "drawer", FrontDir(0f), false, 0.28f);
            MakeDrawer(Part(kb, "Drawer_2"), "cocina_2", "cajón", "drawer", FrontDir(0f), false, 0.28f);
            for (int i = 0; i < 3; i++) MakeDrawer(Part(kb, "Door_" + i), "alacena_" + i, "puerta", "cabinet door", FrontDir(0f), true, 95f);
            AgainstWall("stove", g, new Vector3(1.7f, 0, southIn), 0f, 0f, "Cocina_Estufa");
            AgainstWall("fridge", g, new Vector3(eastIn, 0, -1.45f), -90f, 0f, "Heladera");
            var pan = AgainstWall("pantry", g, new Vector3(eastIn, 0, -2.45f), -90f, 0f, "Alacena");
            pantryHide = MakeHide(pan, "alacena", "pantry", -90f, new[] { "Door" }, 1.45f);
            // mesa y sillas un poco al norte: queda un paso libre de ~0,75 m hasta el fregadero (la entidad se trababa con la silla)
            Put("kitchen_table", 2.55f, -2.95f, 0f, 0f, "Mesa_Cocina", g);
            Put("chair", 2.55f, -2.25f, 180f, 0f, "Silla_1", g);
            Put("chair", 2.55f, -3.62f, 0f, 0f, "Silla_2", g);
            platesSource = Src("Platos", g, new Vector3(2.4f, counter + 0.2f, kbB.center.z), Clip("plates"), 1f, false, 1f, 1.5f, 16f);
            kitchenVoice = Src("Voz_Cocina", g, new Vector3(2.2f, 1.5f, -3.4f), null, 1f, false, 1f, 1.5f, 14f);
            Src("Heladera_Zumbido", g, new Vector3(eastIn - 0.3f, 1f, -1.45f), Clip("electric_hum"), 0.12f, true, 1f, 0.5f, 5f);

            // linterna sobre la mesada
            var fl = Model("fp_hands", g, Vector3.zero, 0f, "Linterna_Mesada", false);
            foreach (Transform c in Inner(fl)) c.gameObject.SetActive(c.name == "Flashlight");
            var flPart = Part(fl, "Flashlight");
            if (flPart != null) { flPart.SetParent(g, true); Object.DestroyImmediate(fl); fl = flPart.gameObject; }
            fl.name = "Linterna_Mesada";
            PlaceByBounds(fl, new Vector3(3.9f, 0, kbB.center.z + 0.05f), counter);
            fl.transform.rotation *= Quaternion.Euler(0, 35f, 0);
            SingleBox(fl);
            flashlightPickup = fl.AddComponent<FlashlightPickup>();

            // cuadro eléctrico en la pared este + nota + caja de herramientas con el fusible
            var bx = Model("breaker_box", g, Vector3.zero, -90f, "Cuadro_Electrico");
            PlaceByBounds(bx, new Vector3(eastIn - 0.09f, 0, -3.75f), 1.05f);
            breaker = bx.AddComponent<BreakerStation>();
            breaker.stationId = "breaker"; breaker.promptEs = "Abrir cuadro eléctrico"; breaker.promptEn = "Open breaker panel";
            var bxB = WorldBounds(bx);
            breaker.view = View("Vista_Cuadro", new Vector3(bxB.center.x - 0.62f, bxB.center.y + 0.1f, bxB.center.z), bxB.center);
            breaker.door = Part(bx, "Door");
            if (breaker.door) breaker.doorAngle = HingeSign(breaker.door, FrontDir(-90f), Vector3.up) * 110f;
            string[] labels = { "1 · ALARMA (3)", "2 · CÁMARAS (2)", "3 · CERRADURAS (2)", "4 · LUCES (3)", "5 · ??? (2)" };
            for (int i = 0; i < 5; i++)
            {
                var sp = Part(bx, "Switch_" + i);
                if (sp == null) continue;
                var lv = sp.gameObject.AddComponent<Lever>();
                lv.handle = sp; lv.angle = 28f; lv.labelEs = labels[i]; lv.labelEn = new[] { "1 · ALARM (3)", "2 · CAMERAS (2)", "3 · LOCKS (2)", "4 · LIGHTS (3)", "5 · ??? (2)" }[i];
                var led = Box("LED_" + i, bx.transform, Vector3.zero, new Vector3(0.015f, 0.015f, 0.015f), mLedOff, 1f, false);
                led.transform.position = PartCenter(sp) + new Vector3(-0.03f, 0.07f, 0f);
                lv.led = led.GetComponent<Renderer>(); lv.ledOn = mLedOn; lv.ledOff = mLedOff; lv.ledFault = mLedFault;
                breaker.switches[i] = lv;
                // rótulo corto en dos líneas (antes se pisaban entre sí): nombre + consumo
                string[] shortEs = { "ALARMA", "CÁMARAS", "CERRADURAS", "LUCES", "???" };
                int[] cost = { 3, 2, 2, 3, 2 };
                var lt = Text3D("Rotulo_" + i, bx.transform, Vector3.zero, 0f, shortEs[i] + "\n" + cost[i] + " u.", 0.0019f, Color.white);
                lt.transform.SetPositionAndRotation(PartCenter(sp) + new Vector3(-0.03f, -0.07f, 0f), Quaternion.Euler(0, 90f, 0));
                if (i == 4) breaker.label06 = lt;
            }
            var disp = Text3D("Display_Carga", bx.transform, Vector3.zero, 0f, "", 0.0045f, new Color(0.4f, 1f, 0.5f));
            disp.transform.SetPositionAndRotation(new Vector3((Part(bx, "Switch_0") ? PartCenter(Part(bx, "Switch_0")).x - 0.03f : bxB.min.x + 0.03f), bxB.max.y - 0.08f, bxB.center.z), Quaternion.Euler(0, 90f, 0));   // dentro del cuadro, a la vista
            breaker.display = disp;
            var fs = Box("Zocalo_Fusible", bx.transform, Vector3.zero, new Vector3(0.03f, 0.05f, 0.05f), mBlack, 1f);
            fs.transform.position = new Vector3(bxB.max.x - 0.12f, bxB.min.y + 0.12f, bxB.center.z + 0.18f);   // medido desde la pared (la puerta ya no cuenta)
            breaker.fuseSlot = fs.AddComponent<SocketSlot>();
            breaker.fuseSlot.itemId = "fuse"; breaker.fuseSlot.missingEs = "F2 · fusible fundido"; breaker.fuseSlot.missingEn = "F2 · blown fuse";
            breaker.fuseSlot.installEs = "Colocar fusible"; breaker.fuseSlot.installEn = "Install fuse";
            // zócalo más grande y visible + fusible instalado (bueno) y fusible quemado (estado inicial del cap. 2)
            fs.transform.localScale = new Vector3(1f, 2.2f, 2.6f);
            var fsC = fs.GetComponent<BoxCollider>(); if (fsC) fsC.size = new Vector3(0.06f, 0.07f, 0.07f);
            var fv = FuseVisual(bx.transform, "Fusible_Instalado", false);
            fv.transform.SetPositionAndRotation(fs.transform.position + new Vector3(-0.012f, 0, 0), Quaternion.Euler(0, 0, 90f));
            var fb = FuseVisual(bx.transform, "Fusible_Quemado", true);
            fb.transform.SetPositionAndRotation(fs.transform.position + new Vector3(-0.012f, 0, 0), Quaternion.Euler(0, 0, 90f));
            breaker.fuseSlot.installedVisual = fv;
            breaker.burntFuse = fb;
            var f2l = Text3D("Rotulo_F2", bx.transform, Vector3.zero, 0f, "F2 · FUSIBLE", 0.0024f, new Color(1f, 0.85f, 0.3f));
            f2l.transform.SetPositionAndRotation(fs.transform.position + new Vector3(-0.035f, 0.085f, 0), Quaternion.Euler(0, 90f, 0));
            breaker.fuseLabel = f2l;
            fuseNote = Doc("fuseNote", "note_fuse", null, new Vector3(eastIn - 0.004f, 1.45f, -3.2f), 0f, new Vector2(0.15f, 0.2f),
                "Nota de papá", "Dad's note", "«El cuadro aguanta 7. Alarma 3 · Cámaras 2 · Cerraduras 2 · Luces 3. Si se funde F2, hay repuestos en la caja de herramientas. NUNCA dejes la alarma sin corriente.»",
                "\"The panel holds 7. Alarm 3 · Cameras 2 · Locks 2 · Lights 3. If F2 blows, there are spares in the toolbox. NEVER leave the alarm without power.\"", g);
            fuseNote.transform.rotation = Quaternion.Euler(0, 90f, 0); // pegada a la pared
            var tbx = Put("toolbox", eastIn - 0.3f, -4.55f, -90f, 0f, "Caja_Herramientas", g);
            // fusible reconocible (cerámica + latón + banda roja), 11 cm, sobre la bandeja y bajo la tapa
            var fz = FuseVisual(g, "Fusible_Repuesto", false);
            fz.transform.rotation = Quaternion.Euler(0, 20f, 0);
            var tbB = WorldBounds(tbx);
            float tray = SurfaceY(new Vector3(tbB.center.x, tbB.max.y + 0.5f, tbB.center.z), tbB.min.y + 0.16f);
            if (tray > tbB.min.y + 0.17f) tray = tbB.min.y + 0.16f;   // si el rayo dio en la tapa cerrada, usar la medida del modelo
            PlaceByBounds(fz, new Vector3(tbB.center.x, 0, tbB.center.z), tray + 0.002f);
            GrabBox(fz, new Vector3(0.17f, 0.09f, 0.11f), new Vector3(0, 0.03f, 0));   // zona de interacción cómoda
            fuse = Item(fz, "fuse", "Fusible de repuesto", "Spare fuse", "Un fusible de cerámica. En la etiqueta, con la letra de papá: «F2 · cámaras».", "A ceramic fuse. Dad's handwriting on the label: \"F2 · cameras\".", true, "Fusible F2", "Fuse F2", 0.12f);
            var lid = Part(tbx, "Lid");
            if (lid)
            {
                var d = lid.gameObject.AddComponent<Drawer>();
                d.id = "toolbox"; d.slide = lid; d.rotate = true; d.nounEs = "caja de herramientas"; d.nounEn = "toolbox";
                d.openEuler = new Vector3(HingeSign(lid, Vector3.up, Vector3.right) * 105f, 0, 0);
                d.contents = new[] { fuse };
                fuse.available = false;
            }
        }
    }
}
