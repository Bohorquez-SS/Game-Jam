using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static GNight.EditorTools.BuildKit;

namespace GNight.EditorTools
{
    /// <summary>
    /// Tools > GOOD NIGHT > Build Complete Game (2.0)
    /// Configura URP y las capas, construye la casa con los modelos FBX, el jugador (cámara del mundo + capa en primera persona
    /// con manos y teléfono), personajes, UI y sistemas, conecta todas las referencias y guarda Assets/_GoodNight/Scenes/GoodNight_Main.unity.
    /// Reproducible: se puede volver a ejecutar (regenera esa escena desde cero).
    /// </summary>
    public static class GameBuilder
    {
        public const string ScenePath = Root + "/Scenes/GoodNight_Main.unity";

        [MenuItem("Tools/GOOD NIGHT/Build Complete Game", false, 0)]
        public static void BuildMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (System.IO.File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("GOOD NIGHT", "La escena GoodNight_Main se va a regenerar desde cero (los cambios hechos a mano en ESA escena se pierden; las demás escenas no se tocan).\n\n¿Continuar?", "Construir", "Cancelar"))
                return;
            Build();
        }

        [MenuItem("Tools/GOOD NIGHT/Borrar partida guardada", false, 40)]
        public static void ClearSave() { PlayerPrefs.DeleteKey("gn_stage"); PlayerPrefs.DeleteKey("gn_flags"); PlayerPrefs.Save(); Debug.Log("[GOOD NIGHT] Partida guardada borrada."); }

        public static void Build()
        {
            try
            {
                EditorUtility.DisplayProgressBar("GOOD NIGHT", "Render pipeline y capas...", 0.05f);
                string urpMsg; URPSetup.TryConfigure(out urpMsg); URPSetup.Tune();
                Debug.Log("[GOOD NIGHT] " + urpMsg);
                SetupLayers();
                PlayerSettings.productName = "GOOD NIGHT";
                PlayerSettings.colorSpace = ColorSpace.Linear;

                EnsureFolder(Root + "/Scenes"); EnsureFolder(Root + "/Prefabs");
                Reset();
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                RenderSettings.skybox = null;
                RenderSettings.ambientMode = AmbientMode.Trilight;
                RenderSettings.ambientSkyColor = new Color(0.16f, 0.17f, 0.22f);
                RenderSettings.ambientEquatorColor = new Color(0.12f, 0.11f, 0.11f);
                RenderSettings.ambientGroundColor = new Color(0.06f, 0.05f, 0.05f);
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogDensity = 0.045f;
                RenderSettings.fogColor = new Color(0.03f, 0.035f, 0.05f);
                RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
                RenderSettings.reflectionIntensity = 0.15f;

                EditorUtility.DisplayProgressBar("GOOD NIGHT", "Construyendo la casa (modelos FBX)...", 0.2f);
                var house = new HouseBuilder();
                house.Build();
                EditorUtility.DisplayProgressBar("GOOD NIGHT", "Personajes, cintas y navegación...", 0.45f);
                house.BuildCharacters();
                house.BuildGraph();
                house.BuildPoses();

                EditorUtility.DisplayProgressBar("GOOD NIGHT", "Jugador, interfaz y sistemas...", 0.6f);
                // ---------------- jugador
                var player = new GameObject("Jugador");
                player.tag = "Player";
                player.transform.position = house.poseDeskSeated.position;
                var cc = player.AddComponent<CharacterController>();
                cc.height = 1.7f; cc.radius = 0.28f; cc.center = new Vector3(0, 0.85f, 0); cc.stepOffset = 0.25f; cc.skinWidth = 0.02f;
                var camGO = new GameObject("Camara");
                camGO.tag = "MainCamera";
                camGO.transform.SetParent(player.transform, false);
                camGO.transform.localPosition = new Vector3(0, 1.6f, 0);
                var cam = camGO.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.02f, 0.025f, 0.04f);
                cam.nearClipPlane = 0.03f; cam.farClipPlane = 70f; cam.fieldOfView = 68f;
                cam.cullingMask = ~(GNLayers.Mask(GNLayers.FP) | GNLayers.Mask(GNLayers.CamOnly) | GNLayers.Mask(GNLayers.Tape) | GNLayers.Mask(GNLayers.Mirror));
                camGO.AddComponent<AudioListener>();
                camGO.AddComponent<PS1Display>();
                Urp(cam, true);
                var pc = player.AddComponent<PlayerController>();
                pc.cam = camGO.transform; pc.mainCam = cam;
                var foot = player.AddComponent<AudioSource>(); foot.playOnAwake = false; foot.spatialBlend = 0f; foot.volume = 0.7f;
                pc.footSource = foot; pc.footsteps = new[] { Clip("step_1"), Clip("step_2"), Clip("step_3") };
                player.AddComponent<InteractionSystem>().cam = camGO.transform;
                var beam = MakeLight("Linterna", camGO.transform, LightType.Spot, new Vector3(0.14f, -0.14f, 0.15f), new Color(1f, 0.95f, 0.85f), 3.2f, 16f, LightShadows.Soft);
                beam.spotAngle = 60f; beam.innerSpotAngle = 25f; beam.enabled = false;
                var fl = player.AddComponent<FlashlightController>(); fl.beam = beam; fl.click = Clip("click");

                // ---------------- capa en primera persona (manos, linterna, teléfono, objetos inspeccionados)
                var fpGO = new GameObject("Camara_PrimeraPersona");
                fpGO.transform.SetParent(camGO.transform, false);
                var fpCam = fpGO.AddComponent<Camera>();
                fpCam.clearFlags = CameraClearFlags.SolidColor; fpCam.backgroundColor = new Color(0, 0, 0, 0);
                fpCam.cullingMask = GNLayers.Mask(GNLayers.FP); fpCam.nearClipPlane = 0.01f; fpCam.farClipPlane = 3f; fpCam.fieldOfView = 60f; fpCam.depth = 5;
                Urp(fpCam, false);
                var fpLight = MakeLight("Luz_Manos", fpGO.transform, LightType.Point, new Vector3(0.1f, 0.25f, -0.1f), new Color(0.8f, 0.82f, 0.9f), 0.35f, 2f, LightShadows.None);
                fpLight.cullingMask = GNLayers.Mask(GNLayers.FP);
                var rig = fpGO.AddComponent<FPRig>();
                rig.fpCam = fpCam; rig.flashlightLight = beam;
                // mano derecha con la linterna
                var rh = Group("Mano_Derecha", fpGO.transform);
                var hands = Model("fp_hands", rh, Vector3.zero, 0f, "Manos_Linterna", false);
                foreach (Transform c in Inner(hands)) c.gameObject.SetActive(c.name == "FlashHand" || c.name == "Flashlight");
                // la linterna debe apuntar hacia delante (+Z de la cámara): se mide en lugar de suponer un giro fijo
                var flm = Part(hands, "Flashlight"); var fhm = Part(hands, "FlashHand");
                if (flm && fhm && rh.InverseTransformPoint(PartCenterOf(flm)).z < rh.InverseTransformPoint(PartCenterOf(fhm)).z)
                    hands.transform.localRotation = Quaternion.Euler(0, 180f, 0);
                var fh = Part(hands, "FlashHand");
                if (fh) hands.transform.localPosition -= rh.InverseTransformPoint(fh.position);
                rig.rightHand = rh; rig.flashlightModel = Part(hands, "Flashlight");
                SetLayer(rh.gameObject, GNLayers.FP); NoShadows(rh.gameObject);
                // teléfono: smartphone + mano izquierda que lo sostiene (fp_phone.fbx, Tools/Blender/phone_model.py).
                // La orientación se calcula midiendo las piezas (pantalla, cuerpo, altavoz), así no depende de los ejes del FBX.
                var phoneRoot = Group("Telefono", fpGO.transform);
                float screenW;
                var anchor = BuildPhoneModel(phoneRoot, out screenW);
                SetLayer(phoneRoot.gameObject, GNLayers.FP); NoShadows(phoneRoot.gameObject);
                var phone3d = fpGO.AddComponent<Phone3D>();
                phone3d.phoneRoot = phoneRoot; phone3d.screenAnchor = anchor; phone3d.fpCam = fpCam; phone3d.wallpaper = Tex("phone_wallpaper"); phone3d.screenWidth = screenW;

                // ---------------- UI
                var ui = new GameObject("UI");
                ui.AddComponent<UIManager>();
                var hud = ui.AddComponent<HUDView>(); hud.vignetteTex = Tex("vignette"); hud.grainTex = Tex("grain"); hud.scanTex = Tex("scanlines");
                var opaque = Shader.Find("GNight/UIOpaque");
                if (opaque) { var wm = new Material(opaque) { name = "M_WorldView" }; AssetDatabase.DeleteAsset(MatDir + "/M_WorldView.mat"); AssetDatabase.CreateAsset(wm, MatDir + "/M_WorldView.mat"); hud.worldMaterial = wm; }
                // cámara de fondo: limpia la pantalla (todas las demás renderizan a texturas)
                var clearCam = new GameObject("Camara_Fondo").AddComponent<Camera>();
                clearCam.clearFlags = CameraClearFlags.SolidColor; clearCam.backgroundColor = Color.black;
                clearCam.cullingMask = 0; clearCam.depth = -100; clearCam.farClipPlane = 1f; clearCam.nearClipPlane = 0.5f;
                Urp(clearCam, false);
                ui.AddComponent<ConfirmPanel>();
                ui.AddComponent<MenuPanel>();
                ui.AddComponent<PausePanel>();
                ui.AddComponent<OptionsPanel>();
                ui.AddComponent<DeathPanel>();
                ui.AddComponent<CreditsPanel>();

                // ---------------- sistemas
                var sys = new GameObject("Sistemas");
                var refs = sys.AddComponent<GameRefs>();
                sys.AddComponent<GameManager>();
                sys.AddComponent<StoryManager>();
                sys.AddComponent<EndingManager>();
                sys.AddComponent<ObjectiveManager>();
                sys.AddComponent<PhoneSystem>();
                sys.AddComponent<JournalManager>();
                sys.AddComponent<HorrorEventManager>();
                sys.AddComponent<DebugJump>();
                sys.AddComponent<FocusSystem>().mainCam = cam;
                sys.AddComponent<InspectSystem>().fpCam = fpCam;
                sys.AddComponent<AlarmSystem>();
                sys.AddComponent<PowerPanel>();
                var secSys = sys.AddComponent<SecurityCameraSystem>();
                secSys.cams = house.cams; secSys.hallCam = house.cams[1]; secSys.recordingDoor = house.doorBathroom;
                foreach (var c in house.cams) Urp(c.cam, false);
                foreach (var t in house.vcr.tapes) if (t.cam) Urp(t.cam, false);
                Urp(house.mirror.mirrorCam, false);
                var lm = sys.AddComponent<LightingManager>();
                lm.lights = house.roomLights.ToArray(); lm.moon = house.moon; lm.sun = house.sun; lm.streetLamp = house.streetLamp; lm.emergency = house.emergency.ToArray();
                lm.skyRenderers = house.skyRenderers.ToArray(); lm.skyNight = house.skyNight; lm.skyDawn = house.skyDawn;
                AudioSetup(sys.AddComponent<AudioManager>(), sys.transform);
                new GameObject("PostFX").AddComponent<PostFX>();

                // ---------------- referencias
                refs.player = pc; refs.flashlight = fl;
                refs.poseMenu = house.poseMenu; refs.poseDeskSeated = house.poseDeskSeated; refs.poseDeskStand = house.poseDeskStand; refs.poseBedLie = house.poseBedLie;
                refs.poseBedSit = house.poseBedSit; refs.poseBedSide = house.poseBedSide; refs.poseSalaDoor = house.poseSalaDoor; refs.poseSalaMonitor = house.poseSalaMonitor;
                refs.poseSecret = house.poseSecret; refs.poseExitStart = house.poseExitStart; refs.poseExitEnd = house.poseExitEnd; refs.poseKitchen = house.poseKitchen; refs.poseHall = house.poseHall;
                refs.doorBedroom = house.doorBedroom; refs.doorBathroom = house.doorBathroom; refs.doorKitchen = house.doorKitchen; refs.doorFront = house.doorFront; refs.doorMetal = house.doorMetal;
                refs.bolt = house.bolt; refs.windows = house.windows; refs.computer = house.computer; refs.bedroomLamp = house.bedroomLamp; refs.wardrobe = house.wardrobe;
                refs.shuttersKitchen = house.shuttersKitchen; refs.shuttersBedroom = house.shuttersBedroom; refs.shuttersSecret = house.shuttersSecret;
                refs.zones = house.zones.ToArray();
                refs.wardrobeHide = house.wardrobeHide; refs.pantryHide = house.pantryHide; refs.lockerHide = house.lockerHide; refs.kitchenDrawerSlam = house.kitchenDrawerSlam;
                refs.enemy = house.enemy; refs.graph = house.graph; refs.nodeBathroom = house.nodeBathroom; refs.nodeBedroom = house.nodeBedroom; refs.nodeHallEnd = house.nodeHallEnd;
                refs.routeChase1 = house.routeChase1; refs.routeChase2 = house.routeChase2; refs.routeFinal = house.routeFinal;
                refs.spawnBathroom = house.spawnBathroom; refs.spawnHallEnd = house.spawnHallEnd; refs.spawnBedroom = house.spawnBedroom; refs.spawnBedroomCam = house.spawnBedroomCam;
                refs.spawnKitchen = house.spawnKitchen; refs.hallFigure = house.hallFigure; refs.windowPeek = house.windowPeek;
                refs.camGhost = house.camGhost; refs.ghostFar = house.ghostFar; refs.ghostNear = house.ghostNear; refs.doppelBed = house.doppelBed; refs.doppelBedRoot = house.doppelBedRoot;
                refs.windowOutside = house.windowOutside; refs.frontDoorOutside = house.frontDoorOutside; refs.kitchenVoice = house.kitchenVoice; refs.secretWall = house.secretWall;
                refs.bathroomInside = house.bathroomInside; refs.platesSource = house.platesSource; refs.hallStepPath = house.hallStepPath;
                refs.cameraDesk = house.cameraDesk; refs.alarmKeypad = house.alarmKeypad; refs.breaker = house.breaker; refs.vcr = house.vcr; refs.isolation = house.isolation;
                refs.mirror = house.mirror; refs.flashlightPickup = house.flashlightPickup;
                refs.receipt = house.receipt; refs.fuse = house.fuse; refs.tapeA = house.tapeA; refs.tapeB = house.tapeB; refs.tapeC = house.tapeC; refs.key = house.key;
                refs.photoFamily = house.photoFamily; refs.photoHall = house.photoHall; refs.reel = house.reel; refs.plan = house.plan; refs.fatherNotes = house.fatherNotes;
                refs.isolationDoc = house.isolationDoc; refs.fuseNote = house.fuseNote;
                refs.coatPocket = house.coatPocket; refs.underBed = house.underBed; refs.wardrobeKeyhole = house.wardrobeKeyhole; refs.recorderSlot = house.recorderSlot;
                refs.recorder = house.recorder; refs.auxLever = house.auxLever; refs.secretMonitors = house.secretMonitors;
                refs.photoFamilyMissing = Tex("photo_family_missing"); refs.photoFamilyPicture = house.photoFamilyPicture;
                refs.momCar = house.momCar; refs.carStart = house.carStart; refs.carStop = house.carStop; refs.carLights = house.carLights;

                // ---------------- prefab de la entidad
                EditorUtility.DisplayProgressBar("GOOD NIGHT", "Guardando...", 0.9f);
                house.enemy.gameObject.SetActive(true);
                PrefabUtility.SaveAsPrefabAssetAndConnect(house.enemy.gameObject, Root + "/Prefabs/Entidad.prefab", InteractionMode.AutomatedAction);
                house.enemy.gameObject.SetActive(false);

                AssetDatabase.SaveAssets();
                EditorSceneManager.SaveScene(scene, ScenePath);
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
                Debug.Log("[GOOD NIGHT] Juego construido: " + ScenePath + ". Pulsa Play.");
            }
            finally { EditorUtility.ClearProgressBar(); }
        }

        /// <summary>
        /// Instancia fp_phone y lo orienta: pantalla hacia la cámara (-Z del raíz), parte superior hacia +Y, centro de la
        /// pantalla en el origen. Devuelve el ancla de la UI (justo delante del cristal). Si falta el modelo usa fp_hands.
        /// </summary>
        static Transform BuildPhoneModel(Transform root, out float screenWidth)
        {
            screenWidth = 0.0665f;
            var ph = Model("fp_phone", root, Vector3.zero, 0f, "Telefono_Modelo", false);
            var scr = Part(ph, "PhoneScreen"); var body = Part(ph, "PhoneBody"); var spk = Part(ph, "Speaker");
            if (scr == null || body == null || spk == null)
            {
                Debug.LogWarning("[GOOD NIGHT] fp_phone.fbx incompleto: se usa el teléfono antiguo de fp_hands.");
                Object.DestroyImmediate(ph);
                var old = Model("fp_hands", root, Vector3.zero, 0f, "Manos_Telefono", false);
                foreach (Transform c in Inner(old)) c.gameObject.SetActive(c.name == "PhoneHand" || c.name == "Phone" || c.name == "PhoneScreen");
                old.transform.localRotation = Quaternion.Euler(0, 0, 180f) * Quaternion.Euler(90f, 180f, 0);
                var os = Part(old, "PhoneScreen");
                if (os) old.transform.localPosition -= root.InverseTransformPoint(PartCenterOf(os));
                screenWidth = 0.066f;
                return Group("Pantalla_Anchor", root, new Vector3(0, 0, -0.003f));
            }
            System.Func<Transform, Vector3> C = t => root.InverseTransformPoint(PartCenterOf(t));
            Vector3 n = (C(scr) - C(body));                    // normal de la pantalla (hacia el usuario)
            Vector3 up = C(spk) - C(scr); up -= Vector3.Project(up, n);
            if (n.sqrMagnitude < 1e-8f || up.sqrMagnitude < 1e-8f) Debug.LogWarning("[GOOD NIGHT] No se pudo medir la orientación del teléfono.");
            var q = Quaternion.LookRotation(Vector3.back, Vector3.up) * Quaternion.Inverse(Quaternion.LookRotation(n.normalized, up.normalized));
            ph.transform.localRotation = q * ph.transform.localRotation;
            ph.transform.localPosition = q * ph.transform.localPosition;
            ph.transform.localPosition -= C(scr);
            // ancho real de la pantalla en el espacio del raíz (para escalar el Canvas)
            var mf = scr.GetComponentInChildren<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                float minX = 1e9f, maxX = -1e9f;
                foreach (var v in mf.sharedMesh.vertices) { float x = root.InverseTransformPoint(mf.transform.TransformPoint(v)).x; minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x); }
                if (maxX - minX > 0.02f) screenWidth = maxX - minX;
            }
            // la pantalla del modelo queda negra debajo de la UI
            var sr = scr.GetComponentInChildren<Renderer>(); if (sr) sr.sharedMaterial = UnlitColor("M_PhoneGlass", new Color(0.01f, 0.012f, 0.016f));
            Debug.Log("[GOOD NIGHT] Teléfono orientado. Ancho de pantalla " + screenWidth.ToString("F4") + " m.");
            return Group("Pantalla_Anchor", root, new Vector3(0, 0, -0.0006f));
        }

        static Vector3 PartCenterOf(Transform t)
        {
            var mf = t.GetComponentInChildren<MeshFilter>();
            return mf != null && mf.sharedMesh != null ? mf.transform.TransformPoint(mf.sharedMesh.bounds.center) : t.position;
        }

        static void Urp(Camera c, bool post)
        {
            if (c == null) return;
            var d = c.GetUniversalAdditionalCameraData();
            if (d == null) return;
            d.renderPostProcessing = post;
            d.antialiasing = post ? AntialiasingMode.SubpixelMorphologicalAntiAliasing : AntialiasingMode.None;   // SMAA: bordes limpios sin desenfocar
            if (post) d.antialiasingQuality = AntialiasingQuality.High;
            d.renderShadows = true;
        }

        /// <summary>Nombra las capas especiales en ProjectSettings/TagManager (28-31).</summary>
        static void SetupLayers()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0) return;
            var so = new SerializedObject(assets[0]);
            var layers = so.FindProperty("layers");
            if (layers == null) return;
            string[] names = { "GN_Mirror", "GN_Tape", "GN_CamOnly", "GN_FirstPerson" };
            for (int i = 0; i < 4; i++)
            {
                var el = layers.GetArrayElementAtIndex(28 + i);
                if (el != null && string.IsNullOrEmpty(el.stringValue)) el.stringValue = names[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AudioSetup(AudioManager am, Transform sys)
        {
            am.vibrate = Clip("vibrate"); am.beep = Clip("guardian_beep"); am.click = Clip("click"); am.error = Clip("error"); am.keyBeep = Clip("key_beep");
            am.pageFlip = Clip("page_flip"); am.pocket = Clip("pocket"); am.phoneTap = Clip("phone_tap"); am.phoneRing = Clip("phone_ring"); am.callEnd = Clip("call_end");
            am.messagePop = Clip("message_pop"); am.check = Clip("message_pop"); am.pickup = Clip("pocket"); am.leverClick = Clip("click");
            am.sting = Clip("sting"); am.breaker = Clip("breaker"); am.mechanism = Clip("mechanism"); am.lockClunk = Clip("lock_clunk"); am.drawer = Clip("drawer");
            am.knock = Clip("knock"); am.windowKnock = Clip("window_knock"); am.metalBang = Clip("metal_bang"); am.shutter = Clip("shutter"); am.spark = Clip("breaker");
            am.whisper = Clip("whisper"); am.heartbeat = Clip("heartbeat"); am.breath = Clip("breath"); am.breathClose = Clip("breath_close"); am.staticBurst = Clip("static_burst");
            am.steps1 = Clip("step_1"); am.steps2 = Clip("step_2"); am.steps3 = Clip("step_3"); am.heavyStep1 = Clip("heavy_step_1"); am.heavyStep2 = Clip("heavy_step_2");
            am.crack = Clip("crack"); am.entityScream = Clip("entity_scream"); am.hitLow = Clip("hit_low"); am.tensionRise = Clip("tension_rise"); am.handleRattle = Clip("handle_rattle");
            am.woodCreak = Clip("wood_creak"); am.plates = Clip("plates"); am.birds = Clip("birds"); am.carArrive = Clip("car_arrive"); am.carDoor = Clip("car_door");
            am.vhsInsert = Clip("vhs_insert"); am.tapeHiss = Clip("tape_hiss"); am.tvStatic = Clip("tv_static"); am.doorCreak = Clip("door_creak"); am.doorClose = Clip("door_close"); am.thunder = Clip("thunder");
            am.water = Clip("water_run"); am.scareHit = Clip("scare_hit"); am.scareHit2 = Clip("scare_hit_2"); am.doorSlow = Clip("door_slow_open");
            am.voiceMom = Clip("voice_mom"); am.voiceDad = Clip("voice_dad"); am.voiceGuardian = Clip("voice_guardian"); am.voiceYou = Clip("voice_you"); am.voiceEntity = Clip("voice_entity");
            var amb = Group("Ambiente_2D", sys);
            var rain2d = Src("Lluvia_General", amb, Vector3.zero, Clip("rain_loop"), 0.22f, true, 0f);
            var wind2d = Src("Viento", amb, Vector3.zero, Clip("wind_loop"), 0.14f, true, 0f);
            var house2d = Src("Casa", amb, Vector3.zero, Clip("house_ambience"), 0.2f, true, 0f);
            am.ambience = new[] { rain2d, wind2d, house2d };
            am.music = Src("Musica_Drone", amb, Vector3.zero, Clip("music_drone"), 0.12f, true, 0f);
            am.tension = Src("Tension", amb, Vector3.zero, Clip("heartbeat"), 0f, true, 0f);
        }
    }

    /// <summary>La primera vez que se abre el proyecto, construye el juego automáticamente.</summary>
    [InitializeOnLoad]
    static class GoodNightFirstRun
    {
        static GoodNightFirstRun()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
                if (System.IO.File.Exists(GameBuilder.ScenePath)) return;
                string key = "GN_V2Built_" + Application.dataPath;
                if (EditorPrefs.GetBool(key, false)) return;
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                EditorPrefs.SetBool(key, true);
                GameBuilder.Build();
                EditorSceneManager.OpenScene(GameBuilder.ScenePath);
            };
        }
    }
}
