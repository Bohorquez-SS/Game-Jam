using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using static GNight.EditorTools.BuildKit;

namespace GNight.EditorTools
{
    /// <summary>
    /// Tools > GOOD NIGHT > Build Complete Game
    /// Configura URP (si está instalado), construye la casa, el jugador, la entidad, la UI y todos los sistemas,
    /// conecta las referencias y guarda la escena principal en Assets/_GoodNight/Scenes/GoodNight_Main.unity.
    /// Es reproducible: se puede ejecutar de nuevo y regenera todo desde cero (no toca otras escenas).
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
        public static void ClearSave() { PlayerPrefs.DeleteKey("gn_stage"); PlayerPrefs.Save(); Debug.Log("[GOOD NIGHT] Partida guardada borrada."); }

        public static void Build()
        {
            try
            {
                EditorUtility.DisplayProgressBar("GOOD NIGHT", "Configurando render pipeline...", 0.05f);
                string urpMsg; URPSetup.TryConfigure(out urpMsg);
                Debug.Log("[GOOD NIGHT] " + urpMsg);
                PlayerSettings.productName = "GOOD NIGHT";

                EnsureFolder(Root + "/Scenes"); EnsureFolder(Root + "/Prefabs");
                Reset();
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                RenderSettings.skybox = null;
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.07f, 0.065f, 0.065f);
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogDensity = 0.07f;
                RenderSettings.fogColor = new Color(0.02f, 0.02f, 0.03f);
                RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
                RenderSettings.reflectionIntensity = 0f;

                EditorUtility.DisplayProgressBar("GOOD NIGHT", "Construyendo la casa...", 0.2f);
                var house = new HouseBuilder();
                house.Build();
                var enemy = house.BuildEnemy();
                int nodeBath, nodeBed; int[] routeA, routeF;
                var graph = house.BuildGraph(out nodeBath, out nodeBed, out routeA, out routeF);
                enemy.graph = graph;

                EditorUtility.DisplayProgressBar("GOOD NIGHT", "Jugador, interfaz y sistemas...", 0.6f);
                // ---------------- jugador
                var player = new GameObject("Jugador");
                player.tag = "Player";
                player.transform.position = new Vector3(-1.05f, 0, 2.65f);
                var cc = player.AddComponent<CharacterController>();
                cc.height = 1.7f; cc.radius = 0.28f; cc.center = new Vector3(0, 0.85f, 0); cc.stepOffset = 0.25f; cc.skinWidth = 0.02f;
                var camGO = new GameObject("Camara");
                camGO.tag = "MainCamera";
                camGO.transform.SetParent(player.transform, false);
                camGO.transform.localPosition = new Vector3(0, 1.6f, 0);
                var cam = camGO.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
                cam.nearClipPlane = 0.03f; cam.farClipPlane = 60f; cam.fieldOfView = 70f;
                camGO.AddComponent<AudioListener>();
                camGO.AddComponent<PS1Display>().targetHeight = 270;
                var pc = player.AddComponent<PlayerController>();
                pc.cam = camGO.transform;
                var foot = player.AddComponent<AudioSource>(); foot.playOnAwake = false; foot.spatialBlend = 0f; foot.volume = 0.7f;
                pc.footSource = foot; pc.footsteps = new[] { Clip("step_1"), Clip("step_2"), Clip("step_3") };
                player.AddComponent<InteractionSystem>().cam = camGO.transform;
                var beam = MakeLight("Linterna", camGO.transform, LightType.Spot, new Vector3(0.12f, -0.12f, 0.1f), new Color(1f, 0.95f, 0.85f), 2.2f, 14f, LightShadows.Hard);
                beam.spotAngle = 58f;
                var fl = player.AddComponent<FlashlightController>(); fl.beam = beam; fl.click = Clip("click");

                var clearCam = new GameObject("Camara_Fondo").AddComponent<Camera>();
                clearCam.clearFlags = CameraClearFlags.SolidColor; clearCam.backgroundColor = Color.black;
                clearCam.cullingMask = 0; clearCam.depth = -100;

                // ---------------- poses
                var poses = Group("Poses", null);
                var poseMenu = Pose(poses, "Pose_Menu", new Vector3(-1.4f, 0, 3.4f), 0f, -4f);
                var poseDesk = Pose(poses, "Pose_Escritorio", new Vector3(-1.05f, 0, 2.65f), 90f, 8f);
                var poseBed = Pose(poses, "Pose_Acostado", new Vector3(-3.48f, 0, 4.4f), 182f, 2f);
                var poseBedSide = Pose(poses, "Pose_JuntoCama", new Vector3(-2.55f, 0, 3.65f), 175f, 0f);
                var poseSalaMon = Pose(poses, "Pose_Monitor", new Vector3(-0.15f, 0, -3.4f), 90f, 4f);
                var poseSalaDoor = Pose(poses, "Pose_Puerta", new Vector3(-2.15f, 0, -4.2f), 10f, 0f);
                var poseSecret = Pose(poses, "Pose_Secreta", new Vector3(3.5f, 0, 1.9f), 0f, 5f);
                var poseExitA = Pose(poses, "Pose_Salida_A", new Vector3(-2.15f, 0, -4.5f), 180f, 0f);
                var poseExitB = Pose(poses, "Pose_Salida_B", new Vector3(-2.15f, 0, -6.6f), 180f, 0f);
                var spawnBath = Pose(poses, "Spawn_Bano", new Vector3(1.1f, 0, 2.3f), 180f, 0f);
                var spawnBed = Pose(poses, "Spawn_Cama", new Vector3(-2.6f, 0, 3.6f), 180f, 0f);

                // ---------------- UI
                var ui = new GameObject("UI");
                ui.AddComponent<UIManager>();
                var hud = ui.AddComponent<HUDView>(); hud.vignetteTex = Tex("vignette"); hud.grainTex = Tex("grain");
                ui.AddComponent<PhonePanel>();
                ui.AddComponent<DocumentPanel>();
                ui.AddComponent<KeypadPanel>();
                ui.AddComponent<PowerPanelUI>();
                ui.AddComponent<CameraMonitorPanel>().noiseTex = Tex("crt_static");
                ui.AddComponent<IsolationPanel>();
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
                var alarm = sys.AddComponent<AlarmSystem>(); alarm.screen = house.alarmScreen;
                sys.AddComponent<PowerPanel>();
                var secSys = sys.AddComponent<SecurityCameraSystem>();
                secSys.cams = house.cams; secSys.hallCam = house.cams[1];
                secSys.recordingDoor = house.doorBathroom;
                secSys.ghost = house.ghost; secSys.ghostRenderers = house.ghostRenderers.ToArray();
                var lm = sys.AddComponent<LightingManager>();
                lm.lights = house.roomLights.ToArray(); lm.moon = house.moon; lm.emergency = house.emergency.ToArray(); lm.sun = house.sun;
                lm.skyRenderers = house.skyRenderers.ToArray(); lm.skyNight = house.skyNight; lm.skyDawn = house.skyDawn;
                var am = sys.AddComponent<AudioManager>();
                am.vibrate = Clip("vibrate"); am.beep = Clip("guardian_beep"); am.click = Clip("click"); am.error = Clip("error"); am.keyBeep = Clip("key_beep"); am.pageFlip = Clip("page_flip");
                am.sting = Clip("sting"); am.breaker = Clip("breaker"); am.mechanism = Clip("mechanism"); am.lockClunk = Clip("lock_clunk"); am.drawer = Clip("drawer");
                am.knock = Clip("knock"); am.windowKnock = Clip("window_knock"); am.metalBang = Clip("metal_bang"); am.shutter = Clip("shutter"); am.spark = Clip("breaker");
                am.whisper = Clip("breath"); am.heartbeat = Clip("heartbeat"); am.breath = Clip("breath"); am.staticBurst = Clip("static_burst");
                am.steps1 = Clip("step_1"); am.steps2 = Clip("step_2"); am.steps3 = Clip("step_3");
                am.voiceMom = Clip("voice_mom"); am.voiceDad = Clip("voice_dad"); am.voiceGuardian = Clip("voice_guardian"); am.voiceYou = Clip("voice_you"); am.voiceEntity = Clip("voice_entity");
                var amb = Group("Ambiente_2D", sys.transform);
                var rain2d = Src("Lluvia_General", amb, Vector3.zero, Clip("rain_loop"), 0.25f, true, 0f);
                var wind2d = Src("Viento", amb, Vector3.zero, Clip("wind_loop"), 0.18f, true, 0f);
                am.ambience = new[] { rain2d, wind2d };
                am.music = Src("Musica_Drone", amb, Vector3.zero, Clip("music_drone"), 0.12f, true, 0f);

                // ---------------- monitores
                house.salaMonitor.source = house.cams[1];
                for (int i = 0; i < 4; i++) house.secretMonitors[i].source = house.cams[i];

                // ---------------- referencias
                refs.player = pc; refs.flashlight = fl;
                refs.poseMenu = poseMenu; refs.poseDesk = poseDesk; refs.poseBed = poseBed; refs.poseBedSide = poseBedSide;
                refs.poseSalaMonitor = poseSalaMon; refs.poseSalaDoor = poseSalaDoor; refs.poseSecret = poseSecret;
                refs.poseExitStart = poseExitA; refs.poseExitEnd = poseExitB;
                refs.doorBedroom = house.doorBedroom; refs.doorBathroom = house.doorBathroom; refs.doorKitchen = house.doorKitchen;
                refs.doorFront = house.doorFront; refs.doorMetal = house.doorMetal; refs.bolt = house.bolt;
                refs.windows = house.windows; refs.computer = house.computer; refs.bedroomLamp = house.bedroomLamp;
                refs.wardrobe = house.wardrobe; refs.shuttersKitchen = house.shuttersKitchen; refs.shuttersBedroom = house.shuttersBedroom;
                refs.zones = house.zones.ToArray();
                refs.enemy = enemy; refs.graph = graph; refs.nodeBathroom = nodeBath; refs.nodeBedroom = nodeBed;
                refs.routeAct2 = routeA; refs.routeFinal = routeF; refs.spawnBathroom = spawnBath; refs.spawnBed = spawnBed;
                refs.doppelInBed = house.doppelInBed; refs.doppelTorso = house.doppelTorso; refs.doppelHead = house.doppelHead;
                refs.ghostFar = house.ghostFar; refs.ghostNear = house.ghostNear;
                refs.windowOutside = house.windowOutside;
                refs.frontDoorOutside = house.frontDoorOutside; refs.hallSteps = house.hallSteps; refs.bathroomInside = house.bathroomInside;
                refs.kitchenVoice = house.kitchenVoice; refs.secretWall = house.secretWall;
                refs.shadowFigure = house.shadowFigure; refs.shadowStart = house.shadowStart; refs.shadowEnd = house.shadowEnd;
                refs.hallStepPath = house.hallStepPath;
                refs.monitorStation = house.monitorStation; refs.alarmStation = house.alarmStation; refs.powerStation = house.powerStation; refs.isolationStation = house.isolationStation;
                refs.recorder = house.recorder; refs.auxLever = house.auxLever; refs.flashlightPickup = house.flashlightPickup;
                refs.secretMonitors = house.secretMonitors; refs.metalDoorSign = house.metalSign;
                refs.docReceipt = house.docReceipt; refs.docPlan = house.docPlan; refs.docFatherNote = house.docFatherNote; refs.docIsolation = house.docIsolation;
                refs.receiptDrawer = house.receiptDrawer; refs.planDrawer = house.planDrawer;

                // ---------------- prefab de la entidad (para reutilizar)
                EditorUtility.DisplayProgressBar("GOOD NIGHT", "Guardando...", 0.9f);
                enemy.gameObject.SetActive(true);
                PrefabUtility.SaveAsPrefabAssetAndConnect(enemy.gameObject, Root + "/Prefabs/Entidad.prefab", InteractionMode.AutomatedAction);
                enemy.gameObject.SetActive(false);

                AssetDatabase.SaveAssets();
                EditorSceneManager.SaveScene(scene, ScenePath);
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
                Debug.Log("[GOOD NIGHT] Juego construido: " + ScenePath + ". Pulsa Play.");
            }
            finally { EditorUtility.ClearProgressBar(); }
        }

        static Transform Pose(Transform parent, string name, Vector3 pos, float yaw, float pitch)
        {
            var t = Group(name, parent, pos);
            t.rotation = Quaternion.Euler(pitch, yaw, 0);
            return t;
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
                string key = "GN_FullBuilt_" + Application.dataPath;
                if (EditorPrefs.GetBool(key, false)) return;
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                EditorPrefs.SetBool(key, true);
                GameBuilder.Build();
                EditorSceneManager.OpenScene(GameBuilder.ScenePath);
            };
        }
    }
}
