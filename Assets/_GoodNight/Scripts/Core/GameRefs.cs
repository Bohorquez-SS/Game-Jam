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
        public Transform poseMenu, poseDesk, poseBed, poseBedSide, poseSalaMonitor, poseSalaDoor, poseSecret, poseExitStart, poseExitEnd;

        [Header("Puertas y ventanas")]
        public DoorController doorBedroom, doorBathroom, doorKitchen, doorFront, doorMetal;
        public DeadBolt bolt;
        public WindowController[] windows;
        public Computer computer;
        public RoomLight bedroomLamp;
        public SlidingWardrobe wardrobe;
        public Shutter[] shuttersKitchen, shuttersBedroom;
        public RoomZone[] zones;

        [Header("Entidad")]
        public EnemyAI enemy;
        public WaypointGraph graph;
        public int nodeBathroom, nodeBedroom;
        public int[] routeAct2, routeFinal;
        public Transform spawnBathroom, spawnBed;
        public Transform doppelInBed, doppelTorso, doppelHead;
        public Transform ghostFar, ghostNear;

        [Header("Fuentes de sonido en el mundo")]
        public AudioSource[] windowOutside;
        public AudioSource frontDoorOutside, hallSteps, bathroomInside, kitchenVoice, secretWall;
        public Transform shadowFigure, shadowStart, shadowEnd;
        public Transform[] hallStepPath;

        [Header("Estaciones")]
        public PanelStation monitorStation, alarmStation, powerStation, isolationStation;
        public StoryInteractable recorder, auxLever;
        public FlashlightPickup flashlightPickup;
        public MonitorScreen[] secretMonitors;
        public TextMesh metalDoorSign;

        [Header("Documentos (para restaurar puntos de control)")]
        public DocumentData docReceipt, docPlan, docFatherNote, docIsolation;
        public Drawer receiptDrawer, planDrawer;

        void Awake()
        {
            I = this;
            RoomZone.All = zones;
        }
    }
}
