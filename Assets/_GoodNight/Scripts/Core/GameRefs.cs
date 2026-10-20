using UnityEngine;

namespace GNight
{
    /// <summary>Referencias de escena (las asigna el constructor). Evita búsquedas por nombre.</summary>
    public class GameRefs : MonoBehaviour
    {
        public static GameRefs I;

        [Header("Jugador")]
        public PlayerController player;
        public FlashlightController flashlight;

        [Header("Poses")]
        public Transform poseMenu, poseDeskSeated, poseDeskStand, poseBedLie, poseBedSit, poseBedSide, poseSalaDoor, poseSalaMonitor,
                         poseSecret, poseExitStart, poseExitEnd, poseKitchen, poseHall;

        [Header("Puertas, ventanas, muebles")]
        public DoorController doorBedroom, doorBathroom, doorKitchen, doorFront, doorMetal;
        public DeadBolt bolt;
        public WindowController[] windows;
        public Computer computer;
        public RoomLight bedroomLamp;
        public SlidingWardrobe wardrobe;
        public Shutter[] shuttersKitchen, shuttersBedroom, shuttersSecret;
        public RoomZone[] zones;
        public HidingSpot wardrobeHide, pantryHide, lockerHide;
        public Drawer kitchenDrawerSlam;

        [Header("Entidad")]
        public EnemyAI enemy;
        public WaypointGraph graph;
        public int nodeBathroom, nodeBedroom, nodeHallEnd;
        public int[] routeChase1, routeChase2, routeFinal;
        public Transform spawnBathroom, spawnHallEnd, spawnBedroom, spawnBedroomCam, spawnKitchen, hallFigure, windowPeek;
        public Transform camGhost, ghostFar, ghostNear;
        public CharacterAnim doppelBed;        // el doble acostado (revelación)
        public Transform doppelBedRoot;

        [Header("Sonidos en el mundo")]
        public AudioSource[] windowOutside;
        public AudioSource frontDoorOutside, kitchenVoice, secretWall, bathroomInside, platesSource;
        public Transform[] hallStepPath;

        [Header("Estaciones y objetos")]
        public CameraDeskStation cameraDesk;
        public AlarmKeypadStation alarmKeypad;
        public BreakerStation breaker;
        public VCRStation vcr;
        public IsolationStation isolation;
        public MirrorReflection mirror;
        public FlashlightPickup flashlightPickup;
        public Inspectable receipt, fuse, tapeA, tapeB, tapeC, key, photoFamily, photoHall, reel, plan, fatherNotes, isolationDoc, fuseNote;
        public SearchSpot coatPocket, underBed;
        public ItemTarget wardrobeKeyhole, recorderSlot;
        public StoryInteractable recorder, auxLever;
        public MonitorScreen[] secretMonitors;
        public Texture2D photoFamilyMissing;
        public Renderer photoFamilyPicture;

        [Header("Final")]
        public Transform momCar, carStart, carStop;
        public Light[] carLights;

        void Awake()
        {
            I = this;
            RoomZone.All = zones;
            // (18/10) doble sin "joroba" y Sujeto 0 con el rostro del espejo
            Material faceMat = null;
            if (mirror != null && mirror.scareFace != null) { var fr = mirror.scareFace.GetComponent<Renderer>(); if (fr) faceMat = fr.sharedMaterial; }
            try { CharacterFix.Apply(faceMat); } catch (System.Exception ex) { Debug.LogWarning("[GOOD NIGHT] CharacterFix: " + ex.Message); }
        }
    }
}
