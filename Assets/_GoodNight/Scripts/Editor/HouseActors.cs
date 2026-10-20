using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static GNight.EditorTools.BuildKit;

namespace GNight.EditorTools
{
    /// <summary>Exterior, cámaras de seguridad, personajes (entidad, doble, reflejo, cintas), grafo de IA y poses.</summary>
    public partial class HouseBuilder
    {
        // personajes y puntos
        public EnemyAI enemy; public WaypointGraph graph;
        public int nodeBathroom, nodeBedroom, nodeHallEnd; public int[] routeChase1, routeChase2, routeFinal;
        public Transform camGhost, ghostFar, ghostNear, doppelBedRoot, hallFigure, windowPeek;
        public CharacterAnim doppelBed;
        public Transform spawnBathroom, spawnHallEnd, spawnBedroom, spawnBedroomCam, spawnKitchen;
        public Transform poseMenu, poseDeskSeated, poseDeskStand, poseBedLie, poseBedSit, poseBedSide, poseSalaDoor, poseSalaMonitor, poseSecret, poseExitStart, poseExitEnd, poseKitchen, poseHall;

        // ================================================================= exterior
        void Exterior()
        {
            exterior = Group("Exterior", null);
            Box("Suelo", exterior, new Vector3(0, -0.16f, 0), new Vector3(60f, 0.1f, 60f), mGrass, 2f);
            BoxMM("Porche", exterior, new Vector3(-3.3f, -0.1f, -7.1f), new Vector3(-1.0f, 0.0f, SouthZ - 0.06f), mWoodDark, 1f);
            BoxMM("Camino", exterior, new Vector3(-2.6f, -0.12f, -12.5f), new Vector3(-1.7f, -0.1f, -7.1f), mConcrete, 1f);
            BoxMM("Calle", exterior, new Vector3(-30f, -0.13f, -15.5f), new Vector3(30f, -0.11f, -13f), Lit("M_Asfalto", "concrete", new Color(0.35f, 0.35f, 0.38f)), 1.5f);
            for (float x = -10f; x < 10f; x += 2.02f) { Put("fence_segment", x + 1.01f, 10f, 180f, 0f, "Cerca_N", exterior); }
            for (float x = -10f; x < -3f; x += 2.02f) { Put("fence_segment", x + 1.01f, -12.5f, 0f, 0f, "Cerca_S", exterior); }
            for (float x = -1.2f; x < 10f; x += 2.02f) { Put("fence_segment", x + 1.01f, -12.5f, 0f, 0f, "Cerca_S", exterior); }
            for (float z = -12.5f; z < 10f; z += 2.02f) { Put("fence_segment", -10f, z + 1.01f, 90f, 0f, "Cerca_O", exterior); Put("fence_segment", 10f, z + 1.01f, -90f, 0f, "Cerca_E", exterior); }
            // límites invisibles del porche (no se sale hasta el final)
            foreach (var b in new[] { new[] { -3.4f, -7.2f, -3.3f, SouthZ }, new[] { -1.0f, -7.2f, -0.9f, SouthZ }, new[] { -3.4f, -7.3f, -0.9f, -7.2f } })
            {
                var inv = new GameObject("Limite"); inv.transform.SetParent(exterior, false);
                var bc = inv.AddComponent<BoxCollider>();
                inv.transform.localPosition = new Vector3((b[0] + b[2]) * 0.5f, 1.5f, (b[1] + b[3]) * 0.5f);
                bc.size = new Vector3(b[2] - b[0], 3f, b[3] - b[1]);
            }
            Vector2[] trees = { new Vector2(-7, -8), new Vector2(6, -9), new Vector2(-8, 4), new Vector2(7, 6), new Vector2(-6, 8), new Vector2(3, 8.5f), new Vector2(8, -3), new Vector2(-8.5f, -2) };
            int k = 0;
            foreach (var t in trees) { var tr = Put("tree", t.x, t.y, (k++ * 67) % 360, 0f, "Arbol", exterior); NoShadows(tr); }
            // farola
            var lamp = Put("street_lamp", -5f, -13.4f, 0f, 0f, "Farola", exterior);
            var lb = WorldBounds(lamp);
            streetLamp = MakeLight("Luz_Farola", exterior, LightType.Spot, new Vector3(-5f, lb.max.y - 0.15f, -12.9f), new Color(1f, 0.72f, 0.42f), 4f, 14f, LightShadows.Soft);
            streetLamp.transform.rotation = Quaternion.Euler(80f, 0f, 0f); streetLamp.spotAngle = 95f;
            // casas vecinas lejanas
            var mSid = Lit("M_SidingVecino", "siding", new Color(0.7f, 0.75f, 0.8f));
            for (int i = 0; i < 5; i++)
            {
                var hg = Group("Casa_Vecina", exterior, new Vector3(-24f + i * 12f, 0, -24f));
                Box("Cuerpo", hg, new Vector3(0, 1.8f, 0), new Vector3(6f, 3.6f, 5f), mSid, 2f, false);
                Box("Techo", hg, new Vector3(0, 3.75f, 0), new Vector3(6.4f, 0.3f, 5.4f), mRoof, 2f, false);
                Box("Ventana", hg, new Vector3(1.2f, 1.8f, 2.51f), new Vector3(0.9f, 0.8f, 0.02f), i % 2 == 0 ? mBulbOn : mBlack, 1f, false);
            }
            // auto de mamá (aparece al amanecer)
            var car = Put("car", -14f, -14.2f, 90f, 0f, "Auto_Mama", exterior);
            momCar = car.transform;
            carStart = Group("Auto_Inicio", exterior, new Vector3(-14f, 0, -14.2f), 90f);
            carStop = Group("Auto_Fin", exterior, new Vector3(-3.6f, 0, -14.2f), 90f);
            var cb = WorldBounds(car);
            var l1 = MakeLight("Faro_I", car.transform, LightType.Spot, Vector3.zero, new Color(1f, 0.95f, 0.8f), 6f, 20f, LightShadows.None);
            var l2 = MakeLight("Faro_D", car.transform, LightType.Spot, Vector3.zero, new Color(1f, 0.95f, 0.8f), 6f, 20f, LightShadows.None);
            l1.transform.position = new Vector3(cb.max.x, 0.7f, cb.center.z - 0.6f); l2.transform.position = new Vector3(cb.max.x, 0.7f, cb.center.z + 0.6f);
            l1.transform.rotation = l2.transform.rotation = Quaternion.Euler(8f, 90f, 0); l1.spotAngle = l2.spotAngle = 60f;
            l1.enabled = l2.enabled = false;
            carLights = new[] { l1, l2 };
            car.SetActive(false);
            // cielo (4 paneles)
            float D = 30f, Hs = 18f;
            var sky = Group("Cielo", exterior);
            skyRenderers.Add(Quad("Cielo_N", sky, new Vector3(0, Hs * 0.35f, D), new Vector2(2 * D, Hs), 180f, skyNight).GetComponent<Renderer>());
            skyRenderers.Add(Quad("Cielo_S", sky, new Vector3(0, Hs * 0.35f, -D), new Vector2(2 * D, Hs), 0f, skyNight).GetComponent<Renderer>());
            skyRenderers.Add(Quad("Cielo_E", sky, new Vector3(D, Hs * 0.35f, 0), new Vector2(2 * D, Hs), -90f, skyNight).GetComponent<Renderer>());
            skyRenderers.Add(Quad("Cielo_O", sky, new Vector3(-D, Hs * 0.35f, 0), new Vector2(2 * D, Hs), 90f, skyNight).GetComponent<Renderer>());
            // luna, relámpagos y sol
            var mg = new GameObject("Luz_Luna"); mg.transform.SetParent(exterior, false);
            mg.transform.rotation = Quaternion.Euler(32f, 160f, 0);
            moon = mg.AddComponent<Light>(); moon.type = LightType.Directional; moon.color = new Color(0.45f, 0.55f, 0.9f); moon.intensity = 0.45f; moon.shadows = LightShadows.Soft; moon.shadowStrength = 0.8f;
            lightning = mg.AddComponent<Lightning>(); lightning.flashLight = moon; lightning.flashIntensity = 3.5f;
            lightning.thunder = Src("Trueno", exterior, new Vector3(0, 10, 0), Clip("thunder"), 0.8f, false, 0f);
            var sg = new GameObject("Luz_Amanecer"); sg.transform.SetParent(exterior, false);
            sg.transform.rotation = Quaternion.Euler(12f, 20f, 0);
            sun = sg.AddComponent<Light>(); sun.type = LightType.Directional; sun.color = new Color(1f, 0.72f, 0.5f); sun.intensity = 1.2f; sun.shadows = LightShadows.Soft; sun.enabled = false;
        }

        // ================================================================= cámaras de vigilancia (visión nocturna IR)
        void Cameras()
        {
            var cr = Group("Camaras_Vigilancia", root);
            cams[0] = SecCam(cr, "CAM 01", "sala", new Vector3(0.75f, 2.38f, -1.05f), new Vector3(-2.3f, 0.8f, -4.6f));
            cams[1] = SecCam(cr, "CAM 02", "pasillo", new Vector3(-3.8f, 2.35f, 0.45f), new Vector3(4.2f, 0.9f, -0.1f));
            cams[2] = SecCam(cr, "CAM 03", "cocina", new Vector3(4.38f, 2.38f, -1.05f), new Vector3(1.9f, 0.7f, -4.2f));
            cams[3] = SecCam(cr, "CAM 04", "dormitorio", new Vector3(-0.25f, 2.38f, 1.05f), new Vector3(-3.3f, 0.6f, 4.4f));
        }

        SecurityCam SecCam(Transform p, string name, string zone, Vector3 pos, Vector3 lookAt)
        {
            var g = Group(name, p, pos);
            g.LookAt(lookAt);
            Box("Carcasa", g, new Vector3(0, 0, -0.08f), new Vector3(0.1f, 0.08f, 0.18f), mPaint, 1f, false);
            Box("Lente", g, new Vector3(0, 0, 0.02f), new Vector3(0.05f, 0.05f, 0.03f), mBlack, 1f, false);
            var led = Prim(PrimitiveType.Sphere, "LED", g, new Vector3(0.035f, 0.03f, 0.01f), Vector3.one * 0.015f, UnlitColor("M_LedRojo", new Color(1f, 0.1f, 0.05f)));
            NoShadow(led);
            var cg = Group("Camera", g, new Vector3(0, 0, 0.05f));
            var cam = cg.gameObject.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.02f, 0.02f, 0.03f);
            cam.nearClipPlane = 0.05f; cam.farClipPlane = 18f; cam.fieldOfView = 80f; cam.depth = -10; cam.enabled = false;
            // iluminador infrarrojo: solo se enciende durante el render de esta cámara
            var ir = MakeLight("IR", cg, LightType.Spot, Vector3.zero, new Color(0.85f, 1f, 0.85f), 3.2f, 12f, LightShadows.None);
            ir.spotAngle = 110f; ir.enabled = false;
            var sc = g.gameObject.AddComponent<SecurityCam>();
            sc.camName = name + " — " + zone.ToUpperInvariant(); sc.zoneId = zone; sc.cam = cam; sc.irLight = ir;
            return sc;
        }

        // ================================================================= personajes
        GameObject Character(string file, Transform parent, Vector3 pos, float yaw, string name, bool twitchy, out CharacterAnim anim)
        {
            var g = Model(file, parent, pos, yaw, name, false);
            anim = g.AddComponent<CharacterAnim>();
            anim.twitchy = twitchy;
            foreach (var r in g.GetComponentsInChildren<Renderer>(true)) { r.shadowCastingMode = ShadowCastingMode.On; }
            return g;
        }

        public void BuildCharacters()
        {
            var chars = Group("Personajes", null);
            // --- entidad / doble controlados por la IA
            var e = new GameObject("Entidad");
            e.transform.SetParent(chars, false);
            var cc = e.AddComponent<CharacterController>();
            // cápsula más baja y delgada que el modelo: con 2.0 m + piel chocaba con el dintel (2.05 m) y no pasaba por las puertas
            cc.height = 1.7f; cc.radius = 0.22f; cc.center = new Vector3(0, 0.85f, 0); cc.stepOffset = 0.25f; cc.skinWidth = 0.04f;
            enemy = e.AddComponent<EnemyAI>();
            CharacterAnim ca, da;
            var cv = Character("entity", e.transform, Vector3.zero, 0f, "Visual_Entidad", true, out ca);
            cv.transform.localScale *= 0.85f;  // ~1,89 m (18/10): algo más alta que una persona, cabe holgada por las puertas (2.05 m)
            var dv = Character("double", e.transform, Vector3.zero, 0f, "Visual_Doble", false, out da);
            dv.SetActive(false);
            enemy.creatureVisual = cv; enemy.doubleVisual = dv; enemy.creatureAnim = ca; enemy.doubleAnim = da;
            enemy.head = Find(cv.transform, "Head") ?? Group("Cabeza", e.transform, new Vector3(0, 1.82f, 0.05f));
            enemy.breath = Src("Respiracion", e.transform, new Vector3(0, 1.8f, 0), Clip("breath"), 0.6f, true, 1f, 1f, 9f);
            enemy.breath.playOnAwake = false;
            e.SetActive(false);

            // --- figura que solo existe en las cámaras
            CharacterAnim ga;
            var gh = Character("entity", chars, new Vector3(-2f, 0, 0), 90f, "Figura_Solo_Camara", true, out ga);
            SetLayer(gh, GNLayers.CamOnly);
            camGhost = gh.transform; gh.SetActive(false);
            ghostFar = Group("Figura_Lejos", chars, new Vector3(3.2f, 0, -0.1f), -90f);
            ghostNear = Group("Figura_Cerca", chars, new Vector3(-1.6f, 0, 0.1f), -90f);
            ghostFar.rotation = Quaternion.Euler(0, YawTo(ghostFar.position, cams[1].transform.position), 0);
            ghostNear.rotation = Quaternion.Euler(0, YawTo(ghostNear.position, cams[1].transform.position), 0);

            // --- el doble acostado en tu cama (revelación del cap. 4 y cámara congelada del cap. 5)
            doppelBedRoot = Group("Doble_En_Cama", chars, new Vector3(-3.3f, 0.52f, 4.05f), 0f);
            Character("double", doppelBedRoot, Vector3.zero, 0f, "Visual", false, out doppelBed);
            doppelBedRoot.gameObject.SetActive(false);

            // --- reflejo del espejo (solo lo ve la cámara del espejo)
            CharacterAnim ma;
            var mb = Character("double", chars, Vector3.zero, 0f, "Reflejo_Cuerpo", false, out ma);
            mirror.body = mb.transform; mirror.bodyAnim = ma;
            SetLayer(mb, GNLayers.Mirror);   // solo lo ve la cámara del espejo (también en el editor)
            // figura del susto del lavabo: también solo en el reflejo
            CharacterAnim sa;
            var sg = Character("entity", chars, new Vector3(0.9f, 0, 2.0f), -90f, "Susto_Espejo", true, out sa);
            sg.transform.localScale = Vector3.one * 0.85f;   // igual que la entidad (~1,89 m)
            SetLayer(sg, GNLayers.Mirror);
            mirror.scare = sg.transform; mirror.scareAnim = sa;
            sg.SetActive(false);

            // --- puntos de escena
            hallFigure = Group("Figura_Pasillo", chars, new Vector3(4.15f, 0, 0f), -90f);
            windowPeek = Group("Asomo_Ventana", chars, new Vector3(-1.6f, 0, BedN + 0.7f), 180f);
            spawnBathroom = Group("Spawn_Bano", chars, new Vector3(1.4f, 0, 2.2f), 180f);
            spawnHallEnd = Group("Spawn_FinPasillo", chars, new Vector3(4.15f, 0, 0f), -90f);
            spawnBedroom = Group("Spawn_Dormitorio", chars, new Vector3(-2.0f, 0, 3.2f), 180f);
            var bcam = cams[3].transform.position;
            var sbc = new Vector3(-1.9f, 0, 3.7f);
            spawnBedroomCam = Group("Spawn_Dormitorio_Camara", chars, sbc, YawTo(sbc, bcam));
            var skc = new Vector3(1.75f, 0, -3.9f);
            spawnKitchen = Group("Spawn_Cocina", chars, skc, YawTo(skc, cams[2].transform.position));

            BuildTapeScenes(chars);
        }

        // ================================================================= cintas VHS: cámaras del pasado + actores (capa GN_Tape)
        void BuildTapeScenes(Transform chars)
        {
            tapeRT = new RenderTexture(320, 240, 16) { name = "RT_Cintas" };
            AssetDatabase.DeleteAsset(Root + "/Materials/RT_Cintas.renderTexture");
            AssetDatabase.CreateAsset(tapeRT, Root + "/Materials/RT_Cintas.renderTexture");
            vcr.tapeRT = tapeRT;
            // A — cocina 22:47
            TapeScene(chars, vcr.tapes[0], new Vector3(1.25f, 2.3f, -1.0f), new Vector3(3.4f, 0.9f, -2.4f),
                new[] { new Vector3(1.3f, 0, -2.0f), new Vector3(3.75f, 0, -1.55f), new Vector3(3.0f, 0, -2.2f) }, new[] { 3.5f, 1.5f });
            // B — pasillo 01:24 (todavía no ocurrió)
            TapeScene(chars, vcr.tapes[1], new Vector3(4.35f, 2.3f, -0.4f), new Vector3(-2.6f, 0.9f, 0.4f),
                new[] { new Vector3(DoorBedX, 0, 1.3f), new Vector3(DoorBedX, 0, 0f), new Vector3(2.9f, 0, 0f) }, new[] { 1.5f, 0f });
            // C — porche 22:10
            TapeScene(chars, vcr.tapes[2], new Vector3(-0.9f, 2.45f, SouthZ - 0.4f), new Vector3(-2.5f, 0.8f, -9.5f),
                new[] { new Vector3(-2.2f, 0, -11.8f), new Vector3(-2.15f, 0, -7.4f), new Vector3(FrontX, 0, SouthZ - 0.45f) }, new[] { 0f, 1.2f });
        }

        void TapeScene(Transform chars, TapeData td, Vector3 camPos, Vector3 look, Vector3[] path, float[] waits)
        {
            var cg = Group("Cinta_" + td.letter + "_Camara", chars, camPos);
            cg.LookAt(look);
            var cam = cg.gameObject.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.02f, 0.02f, 0.03f);
            cam.fieldOfView = 72f; cam.nearClipPlane = 0.05f; cam.farClipPlane = 30f; cam.enabled = false; cam.depth = -11;
            var ir = MakeLight("IR", cg, LightType.Spot, Vector3.zero, new Color(0.9f, 1f, 0.9f), 2.6f, 16f, LightShadows.None);
            ir.spotAngle = 100f;
            td.cam = cam;
            var actors = Group("Cinta_" + td.letter + "_Actores", chars);
            CharacterAnim an;
            var who = Character("double", actors, path[0], YawTo(path[0], path[1]), "Actor", false, out an);
            var ta = who.AddComponent<TapeActor>();
            ta.anim = an; ta.cameraToLook = cg; ta.waits = waits;
            var pts = new Transform[path.Length];
            for (int i = 0; i < path.Length; i++)
            {
                float yaw = i < path.Length - 1 ? YawTo(path[i], path[i + 1]) : YawTo(path[i], camPos);
                pts[i] = Group("P" + i, actors, path[i], yaw);
            }
            ta.points = pts;
            td.actors = actors.gameObject; td.actor = ta;
            ir.enabled = false; // VCRStation lo enciende solo durante el render de esta cámara
            SetLayer(actors.gameObject, GNLayers.Tape);
            actors.gameObject.SetActive(false);
        }

        // ================================================================= grafo de navegación
        public void BuildGraph()
        {
            var g = new GameObject("Grafo_Navegacion");
            Vector3[] p =
            {
                new Vector3(-2.2f,0,3.0f),   // 0 dormitorio
                new Vector3(-2.6f,0,0f),     // 1 pasillo O (puerta dormitorio)
                new Vector3(-1.0f,0,0f),     // 2 pasillo medio (arco)
                new Vector3(0.9f,0,0f),      // 3 pasillo baño
                new Vector3(2.05f,0,0f),     // 4 pasillo cocina
                new Vector3(4.15f,0,0f),     // 5 fin del pasillo
                new Vector3(1.0f,0,2.4f),    // 6 baño (lejos de la hoja abierta de la puerta)
                new Vector3(-1.0f,0,-1.7f),  // 7 sala N
                new Vector3(-1.25f,0,-4.45f), // 8 sala S (puerta), libre del sofá
                new Vector3(0.3f,0,-2.0f),   // 9 sala E (paso a cocina)
                new Vector3(1.6f,0,-2.0f),   // 10 cocina O
                new Vector3(2.05f,0,-1.55f), // 11 cocina N
                new Vector3(3.3f,0,-4.0f),   // 12 cocina S
                new Vector3(3.55f,0,2.1f),   // 13 habitación 06
                new Vector3(3.6f,0,0f),      // 14 pasillo frente al armario/06
                new Vector3(DoorBedX,0,HallZ),   // 15 vano de la puerta del dormitorio (cruza por el centro)
                new Vector3(DoorBathX,0,HallZ),  // 16 vano de la puerta del baño
                new Vector3(DoorMetalX,0,HallZ), // 17 vano de la puerta 06
            };
            var nodes = new Transform[p.Length];
            for (int i = 0; i < p.Length; i++) nodes[i] = Group("N" + i, g.transform, p[i]);
            int[,] e = { { 0, 15 }, { 15, 1 }, { 1, 2 }, { 2, 3 }, { 3, 4 }, { 4, 14 }, { 14, 5 }, { 3, 16 }, { 16, 6 }, { 2, 7 }, { 7, 8 }, { 7, 9 }, { 8, 9 }, { 9, 10 }, { 4, 11 }, { 10, 11 }, { 11, 12 }, { 10, 12 }, { 14, 17 }, { 17, 13 } };
            int n = e.GetLength(0);
            graph = g.AddComponent<WaypointGraph>();
            graph.nodes = nodes; graph.edgeA = new int[n]; graph.edgeB = new int[n]; graph.edgeDoor = new DoorController[n]; graph.edgeShutter = new Shutter[n];
            for (int i = 0; i < n; i++)
            {
                int a = e[i, 0], b = e[i, 1];
                graph.edgeA[i] = a; graph.edgeB[i] = b;
                // la puerta se abre al llegar al vano (desde cualquiera de los dos lados)
                if ((a == 0 && b == 15) || (a == 15 && b == 1)) { graph.edgeDoor[i] = doorBedroom; graph.edgeShutter[i] = shuttersBedroom[0]; }
                if ((a == 3 && b == 16) || (a == 16 && b == 6)) graph.edgeDoor[i] = doorBathroom;
                if (a == 4 && b == 11) { graph.edgeDoor[i] = doorKitchen; graph.edgeShutter[i] = shuttersKitchen[0]; }
                if (a == 9 && b == 10) graph.edgeShutter[i] = shuttersKitchen[1];
                if ((a == 14 && b == 17) || (a == 17 && b == 13)) { graph.edgeDoor[i] = doorMetal; graph.edgeShutter[i] = shuttersSecret[0]; }
            }
            nodeBathroom = 6; nodeBedroom = 0; nodeHallEnd = 5;
            routeChase1 = new[] { 5, 14, 4, 3, 2, 7, 8, 9, 7, 2, 1, 0, 1, 2, 3, 6, 3, 4, 14 };
            routeChase2 = new[] { 0, 1, 2, 7, 8, 9, 10, 12, 11, 4, 3, 6, 3, 2, 1 };
            routeFinal = new[] { 5, 14, 4, 11, 12, 10, 9, 8, 7, 2, 1, 0, 1, 2, 3, 6, 3, 4 };
            enemy.graph = graph;
        }

        // ================================================================= poses de cámara del jugador
        public void BuildPoses()
        {
            poseMenu = Pose("Pose_Menu", new Vector3(-1.5f, 0, 3.4f), 340f, -4f);
            poseDeskSeated = Pose("Pose_Escritorio", new Vector3(-1.08f, 0, 2.75f), 90f, 8f);
            poseDeskStand = Pose("Pose_Escritorio_DePie", new Vector3(-1.45f, 0, 2.75f), 90f, 0f);
            poseBedLie = Pose("Pose_Acostado", new Vector3(-3.3f, 0, 4.55f), 180f, -12f);
            poseBedSit = Pose("Pose_Sentado_Cama", new Vector3(-2.85f, 0, 3.9f), 90f, 5f);
            poseBedSide = Pose("Pose_JuntoCama", new Vector3(-2.3f, 0, 3.6f), 180f, 0f);
            poseSalaDoor = Pose("Pose_Puerta", new Vector3(FrontX, 0, -4.5f), 180f, 0f);
            poseSalaMonitor = Pose("Pose_Monitor", new Vector3(-0.15f, 0, -3.65f), 90f, 4f);
            poseSecret = Pose("Pose_Secreta", new Vector3(3.5f, 0, 1.6f), 0f, 5f);
            poseExitStart = Pose("Pose_Salida_A", new Vector3(FrontX, 0, -4.7f), 180f, 0f);
            poseExitEnd = Pose("Pose_Salida_B", new Vector3(FrontX, 0, -7.0f), 180f, 0f);
            poseKitchen = Pose("Pose_Cocina", new Vector3(2.4f, 0, -1.6f), 180f, 0f);
            poseHall = Pose("Pose_Pasillo", new Vector3(0.2f, 0, 0f), 90f, 0f);
        }
    }
}
