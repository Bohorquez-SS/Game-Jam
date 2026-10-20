using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static GNight.EditorTools.BuildKit;

namespace GNight.EditorTools
{
    /// <summary>
    /// GOOD NIGHT 2.0 — construye la casa (una planta) con muebles FBX de Blender y la conecta con los sistemas.
    /// Coordenadas en metros, Y arriba, frente de los muebles hacia +Z con yaw 0.
    ///   Dormitorio x[-4,0]  z[0.8,5.3]     Baño x[0,2.5] z[0.8,3.8]     Habitación 06 x[2.5,4.6] z[0.8,3.8]
    ///   Pasillo    x[-4,4.6] z[-0.8,0.8]   (el armario del pasillo tapa la puerta metálica en x=3.6)
    ///   Sala       x[-4,1]  z[-5.3,-0.8]   Cocina x[1,4.6] z[-5.3,-0.8]   (cocina: puerta al pasillo x=2.05 y paso a la sala z=-2.0)
    /// Puerta principal en la pared sur de la sala (x=-2.15). Ventanas: dormitorio (N), sala (O), cocina (S).
    /// </summary>
    public partial class HouseBuilder
    {
        // ------------------------------------------------------------------ límites
        public const float HallZ = 0.8f, NorthZ = 3.8f, BedN = 5.3f, SouthZ = -5.3f, WestX = -4f, EastX = 4.6f;
        public const float BathX0 = 0f, SecX0 = 2.5f, KitX0 = 1f;
        /// <summary>Límite norte de la habitación 06 (ampliada el 18/10, alineada con el dormitorio).</summary>
        public const float Sec06N = BedN;
        public const float DoorBedX = -2.6f, DoorBathX = 0.9f, DoorMetalX = 3.6f, DoorKitX = 2.05f, ArchX = -1.0f, FrontX = -2.15f, KitPassZ = -2.0f;

        // ------------------------------------------------------------------ resultado (para GameRefs / sistemas)
        public Transform root, exterior, poses;
        public DoorController doorBedroom, doorBathroom, doorKitchen, doorFront, doorMetal;
        public DeadBolt bolt;
        public WindowController[] windows = new WindowController[3];
        public AudioSource[] windowOutside = new AudioSource[3];
        public Computer computer;
        public RoomLight bedroomLamp;
        public SlidingWardrobe wardrobe;
        public Shutter[] shuttersKitchen = new Shutter[2], shuttersBedroom = new Shutter[1], shuttersSecret = new Shutter[1];
        public List<RoomZone> zones = new List<RoomZone>();
        public List<RoomLight> roomLights = new List<RoomLight>();
        public List<Light> emergency = new List<Light>();
        public Light moon, sun, streetLamp;
        public List<Renderer> skyRenderers = new List<Renderer>();
        public Material skyNight, skyDawn;
        public HidingSpot wardrobeHide, pantryHide, lockerHide;
        public Drawer kitchenDrawerSlam;
        public CameraDeskStation cameraDesk; public AlarmKeypadStation alarmKeypad; public BreakerStation breaker; public VCRStation vcr; public IsolationStation isolation;
        public MirrorReflection mirror;
        public FlashlightPickup flashlightPickup;
        public Inspectable receipt, fuse, tapeA, tapeB, tapeC, key, photoFamily, photoHall, reel, plan, fatherNotes, isolationDoc, fuseNote;
        public SearchSpot coatPocket, underBed;
        public ItemTarget wardrobeKeyhole, recorderSlot;
        public StoryInteractable recorder, auxLever;
        public MonitorScreen[] secretMonitors = new MonitorScreen[3];
        public Renderer photoFamilyPicture;
        public SecurityCam[] cams = new SecurityCam[4];
        public AudioSource frontDoorOutside, kitchenVoice, secretWall, bathroomInside, platesSource;
        public Transform[] hallStepPath;
        public Transform momCar, carStart, carStop; public Light[] carLights;
        public Lightning lightning;
        public RenderTexture tapeRT;

        // materiales
        Material mWallBed, mWallHall, mWallLiving, mWallKitchen, mWallBath, mConcrete, mSiding, mWoodFloor, mKitchenFloor, mBathFloor, mCeiling,
                 mPaint, mWoodDark, mDoor, mCurtain, mBeige, mMetal, mGrass, mFence, mMetalDoor, mRoof, mCarpet,
                 mGlass, mRainGlass, mRainFar, mBulbOn, mBulbOff, mStatic, mMonitorOff, mScreenDesktop, mLedOn, mLedOff, mLedFault, mBlack;

        public void Build()
        {
            root = Group("Casa", null);
            poses = Group("Poses", null);
            Materials();
            Shell();
            Doors();
            Windows();
            Lights();
            Bedroom();
            Bathroom();
            Hallway();
            Secret();
            Living();
            Kitchen();
            Exterior();
            Zones();
            Cameras();
            Physics.SyncTransforms();
        }

        // ================================================================= materiales
        void Materials()
        {
            mWallBed = Lit("M_WallBedroom", "wallpaper", Color.white);
            mWallHall = Lit("M_WallHall", "wall_hall", Color.white);
            mWallLiving = Lit("M_WallLiving", "wall_living", Color.white);
            mWallKitchen = Lit("M_WallKitchen", "tile_kitchen", Color.white, 0.35f);
            mWallBath = Lit("M_WallBath", "tile_bath", Color.white, 0.45f);
            mConcrete = Lit("M_Concrete", "concrete", Color.white);
            mSiding = Lit("M_Siding", "siding", Color.white);
            mWoodFloor = Lit("M_WoodFloor", "wood_floor", Color.white, 0.3f);
            mKitchenFloor = Lit("M_KitchenFloor", "floor_kitchen", Color.white, 0.3f);
            mBathFloor = Lit("M_BathFloor", "floor_bath", Color.white, 0.35f);
            mCeiling = Lit("M_Ceiling", "ceiling", Color.white);
            mPaint = PaletteMat("Paint");
            mWoodDark = PaletteMat("WoodDark");
            mDoor = Lit("M_Door", "door", Color.white);
            mCurtain = Lit("M_Curtain", "curtain", Color.white);
            mBeige = PaletteMat("PlasticBeige");
            mMetal = PaletteMat("Metal");
            mGrass = Lit("M_Grass", "grass", Color.white);
            mFence = Lit("M_Fence", "fence", Color.white);
            mMetalDoor = Lit("M_MetalDoor", "metal_door", Color.white, 0.35f, 0.3f);
            mRoof = Lit("M_Roof", "concrete", new Color(0.25f, 0.22f, 0.22f));
            mCarpet = Lit("M_Carpet", "carpet", Color.white);
            mBlack = Lit("M_Black", null, new Color(0.02f, 0.02f, 0.02f));
            mBulbOn = UnlitColor("M_BulbOn", new Color(1f, 0.9f, 0.72f));
            mBulbOff = Lit("M_BulbOff", "noise_light", new Color(0.5f, 0.48f, 0.42f));
            mLedOn = UnlitColor("M_LedOn", new Color(0.3f, 1f, 0.4f));
            mLedOff = UnlitColor("M_LedOff", new Color(0.12f, 0.14f, 0.12f));
            mLedFault = UnlitColor("M_LedFault", new Color(1f, 0.2f, 0.1f));
            mScreenDesktop = Unlit("M_ScreenDesktop", "screen_desktop", false);
            mStatic = Unlit("M_Static", "crt_static", false);
            mMonitorOff = Lit("M_MonitorOff", "monitor_off", Color.white, 0.8f);
            mGlass = Unlit("M_Glass", "glass", true);
            mRainGlass = Unlit("M_RainGlass", "rain_glass", true);
            mRainFar = Unlit("M_RainFar", "rain_far", true);
            skyNight = Unlit("M_SkyNight", "sky_night", false);
            skyDawn = Unlit("M_SkyDawn", "sky_dawn", false);
        }

        // ================================================================= estructura
        struct RoomDef { public string id; public float x0, x1, z0, z1; public Material wall, floor, ceil; public Opening[] n, s, w, e; }
        static Opening Door(float c, float w = 0.9f, float top = 2.05f) => new Opening(c, w, 0f, top);
        static Opening Win(float c, float w = 1.2f, float b = 0.9f, float t = 2.0f) => new Opening(c, w, b, t);

        void Shell()
        {
            var shell = Group("Estructura", root);
            var none = new Opening[0];
            var rooms = new[]
            {
                new RoomDef { id = "dormitorio", x0 = WestX, x1 = 0, z0 = HallZ, z1 = BedN, wall = mWallBed, floor = mWoodFloor, ceil = mCeiling,
                    n = new[] { Win(-1.6f) }, s = new[] { Door(DoorBedX) }, w = none, e = none },
                new RoomDef { id = "bano", x0 = BathX0, x1 = SecX0, z0 = HallZ, z1 = NorthZ, wall = mWallBath, floor = mBathFloor, ceil = mCeiling,
                    n = new[] { Win(1.8f, 0.6f, 1.5f, 2.0f) }, s = new[] { Door(DoorBathX, 0.85f) }, w = none, e = none },
                new RoomDef { id = "secreta", x0 = SecX0, x1 = EastX, z0 = HallZ, z1 = Sec06N, wall = mConcrete, floor = mConcrete, ceil = mConcrete,
                    n = none, s = new[] { Door(DoorMetalX, 0.85f) }, w = none, e = none },
                new RoomDef { id = "pasillo", x0 = WestX, x1 = EastX, z0 = -HallZ, z1 = HallZ, wall = mWallHall, floor = mWoodFloor, ceil = mCeiling,
                    n = new[] { Door(DoorBedX), Door(DoorBathX, 0.85f), Door(DoorMetalX, 0.85f) }, s = new[] { Door(ArchX, 1.3f, 2.2f), Door(DoorKitX) }, w = none, e = none },
                new RoomDef { id = "sala", x0 = WestX, x1 = KitX0, z0 = SouthZ, z1 = -HallZ, wall = mWallLiving, floor = mWoodFloor, ceil = mCeiling,
                    n = new[] { Door(ArchX, 1.3f, 2.2f) }, s = new[] { Door(FrontX, 0.95f, 2.1f) }, w = new[] { Win(-3.0f) }, e = new[] { Door(KitPassZ) } },
                new RoomDef { id = "cocina", x0 = KitX0, x1 = EastX, z0 = SouthZ, z1 = -HallZ, wall = mWallKitchen, floor = mKitchenFloor, ceil = mCeiling,
                    n = new[] { Door(DoorKitX) }, s = new[] { Win(3.2f, 1.2f, 1.2f, 2.1f) }, w = new[] { Door(KitPassZ) }, e = none },
            };
            foreach (var r in rooms)
            {
                var g = Group("Hab_" + r.id, shell);
                BoxMM("Piso", g, new Vector3(r.x0, -0.1f, r.z0), new Vector3(r.x1, 0f, r.z1), r.floor, r.floor == mWoodFloor ? 1.0f : 0.8f);
                BoxMM("Techo", g, new Vector3(r.x0, H, r.z0), new Vector3(r.x1, H + 0.1f, r.z1), r.ceil, 1.5f);
                WallX(g, "Pared_N", r.z1 - T * 0.5f, r.x0, r.x1, r.wall, 0.8f, r.n);
                WallX(g, "Pared_S", r.z0 + T * 0.5f, r.x0, r.x1, r.wall, 0.8f, r.s);
                WallZ(g, "Pared_O", r.x0 + T * 0.5f, r.z0, r.z1, r.wall, 0.8f, r.w);
                WallZ(g, "Pared_E", r.x1 - T * 0.5f, r.z0, r.z1, r.wall, 0.8f, r.e);
                if (r.id != "secreta")
                {
                    BoxMM("Zocalo", g, new Vector3(r.x0 + T, 0, r.z1 - T - 0.015f), new Vector3(r.x1 - T, 0.09f, r.z1 - T), mWoodDark, 1f, false);
                    BoxMM("Zocalo", g, new Vector3(r.x0 + T, 0, r.z0 + T), new Vector3(r.x1 - T, 0.09f, r.z0 + T + 0.015f), mWoodDark, 1f, false);
                }
            }
            // alfombras
            BoxMM("Alfombra_Dormitorio", shell, new Vector3(-3.0f, 0f, 1.6f), new Vector3(-1.4f, 0.008f, 2.9f), mCarpet, 1.6f, false);
            BoxMM("Alfombra_Sala", shell, new Vector3(-3.6f, 0f, -3.9f), new Vector3(-1.6f, 0.008f, -2.1f), mCarpet, 2f, false);
            // fachada
            var ext = Group("Fachada", shell);
            float o = T * 0.5f;
            WallX(ext, "Fach_S", SouthZ - o, WestX - T, EastX + T, mSiding, 1f, Door(FrontX, 0.95f, 2.1f), Win(3.2f, 1.2f, 1.2f, 2.1f));
            WallZ(ext, "Fach_E", EastX + o, SouthZ - T, Sec06N + T, mSiding, 1f);
            WallX(ext, "Fach_NE", NorthZ + o, 0f, SecX0 + T, mSiding, 1f, Win(1.8f, 0.6f, 1.5f, 2.0f));
            WallZ(ext, "Fach_06_O", SecX0 - o, NorthZ, Sec06N + T, mSiding, 1f);
            WallX(ext, "Fach_06_N", Sec06N + o, SecX0 - T, EastX + T, mSiding, 1f);
            WallZ(ext, "Fach_Escalon", 0f + o, NorthZ, BedN + T, mSiding, 1f);
            WallX(ext, "Fach_N", BedN + o, WestX - T, 0f + T, mSiding, 1f, Win(-1.6f));
            WallZ(ext, "Fach_O", WestX - o, SouthZ - T, BedN + T, mSiding, 1f, Win(-3.0f));
            BoxMM("Techo_Ext_A", ext, new Vector3(WestX - 0.3f, H + 0.1f, SouthZ - 0.3f), new Vector3(EastX + 0.3f, H + 0.25f, NorthZ + 0.3f), mRoof, 2f, false);
            BoxMM("Techo_Ext_B", ext, new Vector3(WestX - 0.3f, H + 0.1f, NorthZ + 0.3f), new Vector3(0.3f, H + 0.25f, BedN + 0.3f), mRoof, 2f, false);
            BoxMM("Techo_Ext_C", ext, new Vector3(SecX0 - 0.3f, H + 0.1f, NorthZ + 0.3f), new Vector3(EastX + 0.3f, H + 0.25f, Sec06N + 0.3f), mRoof, 2f, false);
        }

        // ================================================================= puertas
        /// <summary>Puerta con bisagra (hoja FBX). yaw: 0 abre hacia -Z, 180 hacia +Z, -90 hacia +X, 90 hacia -X.</summary>
        DoorController MakeDoor(string name, string id, string modelFile, Vector3 center, float yaw, float width, float height, string es, string en, out GameObject leafModel)
        {
            var frame = Group(name, root, center, yaw);
            var hinge = Group("Bisagra", frame, new Vector3(-width * 0.5f, 0, 0));
            leafModel = Model(modelFile, hinge, Vector3.zero, 0f, "Hoja");
            FitLeaf(hinge, leafModel, width - 0.02f, height - 0.01f);
            // marco (modelo) en ambas caras
            var fr = Model("door_frame", frame, Vector3.zero, 0f, "Marco", false);
            FitFrame(frame, fr, width, height);
            var d = hinge.gameObject.AddComponent<DoorController>();
            d.id = id; d.displayEs = es; d.displayEn = en; d.openAngle = 90f;
            d.openClip = Clip("door_creak"); d.closeClip = Clip("door_close"); d.lockedClip = Clip("handle_rattle");
            return d;
        }

        /// <summary>Ajusta la hoja para que vaya de x=0 (bisagra) a x=width, centrada en z, apoyada en y=0.</summary>
        static void FitLeaf(Transform hinge, GameObject leaf, float width, float height)
        {
            var b = LocalBounds(hinge, leaf);
            if (Mathf.Abs(b.min.x) > Mathf.Abs(b.max.x)) { leaf.transform.localRotation *= Quaternion.Euler(0, 180f, 0); b = LocalBounds(hinge, leaf); }
            var s = leaf.transform.localScale;
            leaf.transform.localScale = new Vector3(s.x * width / Mathf.Max(0.1f, b.size.x), s.y * height / Mathf.Max(0.1f, b.size.y), s.z);
            b = LocalBounds(hinge, leaf);
            leaf.transform.localPosition += new Vector3(-b.min.x, -b.min.y, -b.center.z);
        }

        static void FitFrame(Transform frame, GameObject fr, float width, float height)
        {
            var b = LocalBounds(frame, fr);
            var s = fr.transform.localScale;
            fr.transform.localScale = new Vector3(s.x * (width + 0.16f) / Mathf.Max(0.1f, b.size.x), s.y * (height + 0.08f) / Mathf.Max(0.1f, b.size.y), s.z);
            b = LocalBounds(frame, fr);
            fr.transform.localPosition += new Vector3(-b.center.x, -b.min.y, -b.center.z);
            NoShadows(fr);
        }

        /// <summary>Límites de un modelo expresados en el espacio local de 'space'.</summary>
        static Bounds LocalBounds(Transform space, GameObject g)
        {
            bool first = true; var bb = new Bounds();
            foreach (var mf in g.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh == null) continue;
                var mb = mf.sharedMesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    var c = new Vector3((i & 1) == 0 ? mb.min.x : mb.max.x, (i & 2) == 0 ? mb.min.y : mb.max.y, (i & 4) == 0 ? mb.min.z : mb.max.z);
                    var p = space.InverseTransformPoint(mf.transform.TransformPoint(c));
                    if (first) { bb = new Bounds(p, Vector3.zero); first = false; } else bb.Encapsulate(p);
                }
            }
            return bb;
        }

        Shutter MakeShutter(string name, Vector3 lineCenter, float width, float top, bool alongX)
        {
            float h = top + 0.12f, drop = H + 0.12f;
            Vector3 size = alongX ? new Vector3(width + 0.1f, h, 0.035f) : new Vector3(0.035f, h, width + 0.1f);
            var s = Box(name, root, lineCenter + new Vector3(0, drop + h * 0.5f, 0), size, mMetalDoor, 0.6f);
            var sh = s.AddComponent<Shutter>(); sh.drop = drop; sh.speed = 1.8f;
            return sh;
        }

        void Doors()
        {
            GameObject leaf;
            doorBedroom = MakeDoor("Puerta_Dormitorio", "bedroom", "interior_door", new Vector3(DoorBedX, 0, HallZ), 180f, 0.9f, 2.05f, "puerta del dormitorio", "bedroom door", out leaf);
            doorBedroom.isOpen = true;
            doorBathroom = MakeDoor("Puerta_Bano", "bathroom", "interior_door", new Vector3(DoorBathX, 0, HallZ), 180f, 0.85f, 2.05f, "puerta del baño", "bathroom door", out leaf);
            doorKitchen = MakeDoor("Puerta_Cocina", "kitchen", "interior_door", new Vector3(DoorKitX, 0, -HallZ), 0f, 0.9f, 2.05f, "puerta de la cocina", "kitchen door", out leaf);
            doorKitchen.isOpen = true;
            doorFront = MakeDoor("Puerta_Principal", "front", "front_door", new Vector3(FrontX, 0, SouthZ), 180f, 0.95f, 2.1f, "puerta principal", "front door", out leaf);
            doorFront.isOpen = true;
            // cerrojo interior: piezas BoltBase/BoltBar del modelo (si faltan, se crea uno)
            var bb = Part(leaf, "BoltBase"); var bar = Part(leaf, "BoltBar");
            if (bb == null)
            {
                var hinge = doorFront.transform;
                var b0 = Box("Cerrojo", hinge, new Vector3(0.78f, 1.15f, 0.04f), new Vector3(0.12f, 0.05f, 0.03f), mMetal, 1f);
                bb = b0.transform;
                bar = Box("Barra", bb, new Vector3(0f, 0, 0.015f), new Vector3(0.1f, 0.025f, 0.02f), mMetal, 1f, false).transform;
            }
            bolt = bb.gameObject.AddComponent<DeadBolt>();
            bolt.door = doorFront; bolt.boltVisual = bar; bolt.clip = Clip("lock_clunk");

            doorMetal = MakeDoor("Puerta_Metalica_06", "metal", "metal_door", new Vector3(DoorMetalX, 0, HallZ), 180f, 0.85f, 2.05f, "puerta metálica", "metal door", out leaf);
            doorMetal.locked = true;
            doorMetal.openClip = Clip("mechanism"); doorMetal.closeClip = Clip("metal_bang");
            var sign = Text3D("Inscripcion", doorMetal.transform, new Vector3(0.42f, 1.55f, -0.06f), 180f, "06", 0.02f, new Color(0.85f, 0.15f, 0.1f));
            NoShadow(sign.gameObject);

            shuttersBedroom[0] = MakeShutter("Persiana_Dormitorio", new Vector3(DoorBedX, 0, HallZ - 0.075f), 0.9f, 2.05f, true);
            shuttersKitchen[0] = MakeShutter("Persiana_Cocina_Pasillo", new Vector3(DoorKitX, 0, -HallZ + 0.075f), 0.9f, 2.05f, true);
            shuttersKitchen[1] = MakeShutter("Persiana_Cocina_Sala", new Vector3(KitX0 - 0.075f, 0, KitPassZ), 0.9f, 2.05f, false);
            shuttersSecret[0] = MakeShutter("Persiana_06", new Vector3(DoorMetalX, 0, HallZ + 0.075f), 0.85f, 2.05f, true);
        }

        // ================================================================= ventanas (guillotina procedural, probada en v1)
        /// <summary>yaw: 0 = pared norte (interior hacia -Z), 180 = sur, -90 = oeste, 90 = este.</summary>
        WindowController MakeWindow(string name, string id, Vector3 center, float yaw, int index, float b = 0.9f, float t = 2.0f)
        {
            float w = 1.2f, h = (t - b) * 0.5f;
            var g = Group(name, root, center, yaw);
            var fixedSash = Group("Hoja_Fija", g);
            Sash(fixedSash, b + h * 1.5f, 0.025f, w, h);
            var sash = Group("Hoja_Movil", g);
            Sash(sash, b + h * 0.5f, -0.025f, w, h);
            Box("Alfeizar", g, new Vector3(0, b - 0.02f, -0.12f), new Vector3(w + 0.2f, 0.04f, 0.2f), mPaint, 1f);
            Box("Moldura_I", g, new Vector3(-w * 0.5f - 0.04f, (b + t) * 0.5f, -0.075f), new Vector3(0.08f, t - b + 0.08f, 0.03f), mPaint, 1f, false);
            Box("Moldura_D", g, new Vector3(w * 0.5f + 0.04f, (b + t) * 0.5f, -0.075f), new Vector3(0.08f, t - b + 0.08f, 0.03f), mPaint, 1f, false);
            Box("Moldura_S", g, new Vector3(0, t + 0.04f, -0.075f), new Vector3(w + 0.16f, 0.08f, 0.03f), mPaint, 1f, false);
            var rn = Quad("Lluvia_Vidrio", g, new Vector3(0, (b + t) * 0.5f, 0.07f), new Vector2(w, t - b), 0f, mRainGlass);
            rn.AddComponent<TextureScroll>().speed = new Vector2(0f, 0.12f);
            var rf = Quad("Lluvia_Exterior", g, new Vector3(0, 1.4f, 1.0f), new Vector2(3.2f, 2.8f), 0f, mRainFar);
            rf.AddComponent<TextureScroll>().speed = new Vector2(0.05f, 1.6f);
            Box("Barra", g, new Vector3(0, t + 0.47f, -0.14f), new Vector3(w + 0.75f, 0.025f, 0.025f), mMetal, 1f, false);
            var cl = Group("Cortina_I", g, new Vector3(-w * 0.5f - 0.18f, t + 0.45f, -0.16f));
            Box("Tela", cl, new Vector3(0, -0.875f, 0), new Vector3(0.36f, 1.75f, 0.04f), mCurtain, 1.75f, false);
            var cr = Group("Cortina_D", g, new Vector3(w * 0.5f + 0.18f, t + 0.45f, -0.16f));
            Box("Tela", cr, new Vector3(0, -0.875f, 0), new Vector3(0.36f, 1.75f, 0.04f), mCurtain, 1.75f, false);
            var rain = Src("Lluvia", g, new Vector3(0, 1.4f, 0.2f), Clip("rain_loop"), 0.6f, true, 0.85f, 1.2f, 9f);
            var knock = Src("Golpes_Exterior", g, new Vector3(0, 1.4f, 0.6f), null, 1f, false, 1f, 1.5f, 14f);
            var wc = g.gameObject.AddComponent<WindowController>();
            wc.id = id; wc.sash = sash; wc.openOffset = h - 0.02f; wc.isOpen = true; wc.rain = rain;
            wc.slideClip = Clip("window_slide"); wc.latchClip = Clip("lock_clunk");
            wc.curtains = new[] { cl.gameObject.AddComponent<CurtainSway>(), cr.gameObject.AddComponent<CurtainSway>() };
            windows[index] = wc; windowOutside[index] = knock;
            return wc;
        }

        void Sash(Transform p, float cy, float z, float w, float h)
        {
            Box("Riel_Inf", p, new Vector3(0, cy - h * 0.5f + 0.025f, z), new Vector3(w, 0.05f, 0.04f), mPaint, 1f);
            Box("Riel_Sup", p, new Vector3(0, cy + h * 0.5f - 0.025f, z), new Vector3(w, 0.05f, 0.04f), mPaint, 1f);
            Box("Lado_I", p, new Vector3(-w * 0.5f + 0.025f, cy, z), new Vector3(0.05f, h, 0.04f), mPaint, 1f);
            Box("Lado_D", p, new Vector3(w * 0.5f - 0.025f, cy, z), new Vector3(0.05f, h, 0.04f), mPaint, 1f);
            Box("Parteluz", p, new Vector3(0, cy, z), new Vector3(0.03f, h, 0.03f), mPaint, 1f);
            Quad("Vidrio", p, new Vector3(0, cy, z), new Vector2(w - 0.1f, h - 0.1f), 0f, mGlass, true);
        }

        void Windows()
        {
            MakeWindow("Ventana_Dormitorio", "bedroom", new Vector3(-1.6f, 0, BedN), 0f, 0);
            MakeWindow("Ventana_Sala", "living", new Vector3(WestX, 0, -3.0f), -90f, 1);
            MakeWindow("Ventana_Cocina", "kitchen", new Vector3(3.2f, 0, SouthZ), 180f, 2, 1.2f, 2.1f);
            var bw = Group("Ventana_Bano", root, new Vector3(1.8f, 0, NorthZ), 0f);
            Quad("Vidrio", bw, new Vector3(0, 1.75f, 0.0f), new Vector2(0.6f, 0.5f), 0f, mGlass);
            Quad("Lluvia", bw, new Vector3(0, 1.75f, 0.04f), new Vector2(0.6f, 0.5f), 0f, mRainGlass).AddComponent<TextureScroll>().speed = new Vector2(0, 0.1f);
        }

        // ================================================================= luces
        RoomLight CeilingLight(string zone, Vector3 pos, float intensity = 2.6f, float range = 7.5f, bool shadows = true)
        {
            var parent = Group("Plafon_" + zone, root, pos);
            Box("Base", parent, new Vector3(0, 0.05f, 0), new Vector3(0.36f, 0.04f, 0.36f), mPaint, 1f, false);
            var bulb = Prim(PrimitiveType.Sphere, "Foco", parent, Vector3.zero, new Vector3(0.3f, 0.1f, 0.3f), mBulbOn);
            NoShadow(bulb);
            var l = MakeLight("Luz_" + zone, parent, LightType.Point, new Vector3(0, -0.18f, 0), new Color(1f, 0.86f, 0.68f), intensity, range, shadows ? LightShadows.Soft : LightShadows.None);
            l.shadowStrength = 0.75f; l.shadowBias = 0.03f; l.shadowNormalBias = 0.3f;
            var rl = l.gameObject.AddComponent<RoomLight>();
            rl.zone = zone; rl.lightSource = l; rl.bulb = bulb.GetComponent<Renderer>(); rl.onMat = mBulbOn; rl.offMat = mBulbOff;
            roomLights.Add(rl);
            return rl;
        }

        void Switch(string zone, Vector3 pos, float yaw)
        {
            var g = Group("Interruptor_" + zone, root, pos, yaw);
            Box("Placa", g, Vector3.zero, new Vector3(0.08f, 0.12f, 0.015f), mPaint, 1f);
            var tg = Box("Tecla", g, new Vector3(0, 0, -0.012f), new Vector3(0.025f, 0.04f, 0.015f), mBeige, 1f, false);
            var s = g.gameObject.AddComponent<LightSwitch>(); s.zone = zone; s.toggle = tg.transform;
        }

        Light Emergency(Vector3 pos)
        {
            var g = Group("Luz_Emergencia", root, pos);
            Box("Caja", g, new Vector3(0, 0.05f, 0), new Vector3(0.22f, 0.06f, 0.08f), mMetal, 1f, false);
            var l = MakeLight("Luz", g, LightType.Point, Vector3.zero, new Color(1f, 0.12f, 0.08f), 1.6f, 6f, LightShadows.None);
            l.enabled = false;
            emergency.Add(l);
            return l;
        }

        void Lights()
        {
            CeilingLight("dormitorio", new Vector3(-2.0f, H - 0.1f, 3.0f), 2.4f);
            CeilingLight("bano", new Vector3(1.25f, H - 0.1f, 2.3f), 2.2f, 6f, false);
            CeilingLight("pasillo", new Vector3(-2.2f, H - 0.1f, 0f), 1.8f, 6f, false);
            CeilingLight("pasillo", new Vector3(2.3f, H - 0.1f, 0f), 1.8f, 6f, false);
            CeilingLight("sala", new Vector3(-1.6f, H - 0.1f, -3.0f), 2.8f, 8.5f);
            CeilingLight("cocina", new Vector3(2.8f, H - 0.1f, -3.0f), 2.8f, 8f);
            var sec = CeilingLight("secreta", new Vector3(3.55f, H - 0.1f, 3.05f), 1.9f, 6.5f, false);
            sec.lightSource.color = new Color(0.85f, 0.95f, 1f);
            Switch("dormitorio", new Vector3(-2.0f, 1.25f, HallZ + T + 0.01f), 180f);
            Switch("bano", new Vector3(0.3f, 1.25f, HallZ + T + 0.01f), 180f);
            Switch("pasillo", new Vector3(-3.2f, 1.25f, -HallZ + T + 0.01f), 180f);   // antes quedaba dentro de la pared
            Switch("sala", new Vector3(-0.2f, 1.25f, -HallZ - T - 0.01f), 0f);
            Switch("cocina", new Vector3(2.75f, 1.25f, -HallZ - T - 0.01f), 0f);
            Emergency(new Vector3(-0.4f, H - 0.1f, 0f));
            Emergency(new Vector3(3.6f, H - 0.1f, 0.4f));
            Emergency(new Vector3(3.55f, H - 0.1f, 2.4f));
            Emergency(new Vector3(3.55f, H - 0.1f, 4.6f));
            Emergency(new Vector3(-1.5f, H - 0.1f, -1.2f));
        }

        // ================================================================= zonas
        void Zones()
        {
            var zr = Group("Zonas", root);
            Zone(zr, "dormitorio", "Dormitorio", "Bedroom", WestX, 0, HallZ, BedN);
            Zone(zr, "bano", "Baño", "Bathroom", BathX0, SecX0, HallZ, NorthZ);
            Zone(zr, "secreta", "Habitación 06", "Room 06", SecX0, EastX, HallZ, Sec06N);
            Zone(zr, "pasillo", "Pasillo", "Hallway", WestX, EastX, -HallZ, HallZ);
            Zone(zr, "sala", "Sala", "Living room", WestX, KitX0, SouthZ, -HallZ);
            Zone(zr, "cocina", "Cocina", "Kitchen", KitX0, EastX, SouthZ, -HallZ);
        }

        void Zone(Transform p, string id, string es, string en, float x0, float x1, float z0, float z1)
        {
            var g = Group("Zona_" + id, p, new Vector3((x0 + x1) * 0.5f, 1.5f, (z0 + z1) * 0.5f));
            var z = g.gameObject.AddComponent<RoomZone>();
            z.id = id; z.nameEs = es; z.nameEn = en; z.size = new Vector3(x1 - x0, 3.2f, z1 - z0);
            zones.Add(z);
        }

        // ================================================================= utilidades de colocación
        /// <summary>Pone un modelo con el centro de sus límites en (x,z) y la base en 'y'.</summary>
        GameObject Put(string file, float x, float z, float yaw, float y = 0f, string name = null, Transform parent = null)
        {
            var g = Model(file, parent ?? root, new Vector3(x, y, z), yaw, name);
            PlaceByBounds(g, new Vector3(x, 0, z), y);
            return g;
        }

        static float TopY(GameObject g) => WorldBounds(g).max.y;

        Transform Pose(string name, Vector3 pos, float yaw, float pitch = 0f)
        {
            var t = Group(name, poses, pos);
            t.rotation = Quaternion.Euler(pitch, yaw, 0);
            return t;
        }

        static float YawTo(Vector3 from, Vector3 to) { var d = to - from; d.y = 0; return Quaternion.LookRotation(d.sqrMagnitude > 0.0001f ? d : Vector3.forward).eulerAngles.y; }

        /// <summary>Pantalla (quad) delante de la cara frontal de un monitor/TV. 'facingYaw' = hacia dónde mira la pantalla.</summary>
        GameObject ScreenQuad(GameObject monitor, float facingYaw, Vector2 size, float heightFrac, float inset = 0.004f)
        {
            var b = WorldBounds(monitor);
            Vector3 n = Quaternion.Euler(0, facingYaw, 0) * Vector3.forward;
            float half = Mathf.Abs(n.x) > 0.5f ? b.extents.x : b.extents.z;
            Vector3 p = new Vector3(b.center.x, b.min.y + b.size.y * heightFrac, b.center.z) + n * (half + inset);
            // Quad de BuildKit: yaw 0 se ve desde -Z → para mirar hacia 'n' se usa facingYaw + 180
            var q = Quad("Pantalla", monitor.transform, Vector3.zero, size, 0f, mMonitorOff, true);
            q.transform.position = p;
            q.transform.rotation = Quaternion.Euler(0, facingYaw + 180f, 0);
            q.transform.localScale = new Vector3(size.x / Mathf.Max(0.0001f, monitor.transform.lossyScale.x), size.y / Mathf.Max(0.0001f, monitor.transform.lossyScale.y), 1f);
            return q;
        }

        MonitorScreen Monitor(GameObject monitorModel, float facingYaw, Vector2 size, float heightFrac, string requireZone)
        {
            var q = ScreenQuad(monitorModel, facingYaw, size, heightFrac);
            var ms = q.AddComponent<MonitorScreen>();
            ms.screen = q.GetComponent<Renderer>(); ms.offMat = mMonitorOff; ms.staticMat = mStatic; ms.requireZone = requireZone;
            return ms;
        }

        /// <summary>Documento: caja fina con la textura arriba (cara visible) y opcionalmente detrás.</summary>
        Inspectable Doc(string id, string tex, string backTex, Vector3 pos, float yaw, Vector2 size, string titleEs, string titleEn, string textEs, string textEn, Transform parent = null)
        {
            var g = new GameObject("Doc_" + id);
            g.transform.SetParent(parent ?? root, false);
            g.transform.position = pos;
            g.transform.rotation = Quaternion.Euler(90f, yaw, 0);
            var bc = g.AddComponent<BoxCollider>(); bc.size = new Vector3(size.x, size.y, 0.01f);
            var front = Quad("Frente", g.transform, new Vector3(0, 0, -0.002f), size, 0f, Lit("M_Doc_" + tex, tex, Color.white, 0.1f));
            NoShadow(front);
            var back = Quad("Dorso", g.transform, new Vector3(0, 0, 0.002f), size, 180f, Lit("M_Doc_" + (backTex ?? "paper"), backTex ?? "noise_light", backTex != null ? Color.white : new Color(0.88f, 0.86f, 0.8f), 0.1f));
            NoShadow(back);
            var it = g.AddComponent<Inspectable>();
            it.id = id; it.titleEs = titleEs; it.titleEn = titleEn; it.textEs = textEs; it.textEn = textEn;
            it.isDocument = true; it.verbEs = "Leer"; it.verbEn = "Read"; it.targetSize = 0.34f;
            return it;
        }

        Inspectable Item(GameObject model, string id, string titleEs, string titleEn, string textEs, string textEn, bool collect, string invEs = null, string invEn = null, float size = 0.2f)
        {
            var it = model.AddComponent<Inspectable>();
            it.id = id; it.titleEs = titleEs; it.titleEn = titleEn; it.textEs = textEs; it.textEn = textEn;
            it.collect = collect; it.invNameEs = invEs ?? titleEs; it.invNameEn = invEn ?? titleEn; it.targetSize = size;
            it.verbEs = collect ? "Recoger" : "Examinar"; it.verbEn = collect ? "Take" : "Examine";
            if (model.GetComponent<Collider>() == null)
            {
                var b = WorldBounds(model);
                var bc = model.AddComponent<BoxCollider>();
                bc.center = model.transform.InverseTransformPoint(b.center);
                var ls = model.transform.lossyScale;
                bc.size = new Vector3(Mathf.Max(0.04f, b.size.x / Mathf.Max(0.001f, ls.x)), Mathf.Max(0.04f, b.size.y / Mathf.Max(0.001f, ls.y)), Mathf.Max(0.04f, b.size.z / Mathf.Max(0.001f, ls.z)));
            }
            return it;
        }

        /// <summary>
        /// Fusible de cartucho reconocible (18/10): cuerpo cerámico, casquillos de latón y banda roja "F2".
        /// Eje largo en X local. Pivote en el centro de la base (apoyado).
        /// </summary>
        GameObject FuseVisual(Transform parent, string name, bool burnt)
        {
            var g = new GameObject(name); g.transform.SetParent(parent, false);
            const float L = 0.11f, D = 0.034f;
            var ceramic = Lit(burnt ? "M_FuseBurnt" : "M_FuseCeramic", null, burnt ? new Color(0.16f, 0.13f, 0.11f) : new Color(0.93f, 0.9f, 0.82f), 0.35f);
            var brass = PaletteMat("Brass");
            var band = burnt ? Lit("M_FuseBandBurnt", null, new Color(0.25f, 0.05f, 0.03f)) : Lit("M_FuseBand", null, new Color(0.95f, 0.22f, 0.08f), 0.3f);
            Prim(PrimitiveType.Cylinder, "Cuerpo", g.transform, new Vector3(0, D * 0.5f, 0), new Vector3(D, L * 0.36f, D), ceramic, false, new Vector3(0, 0, 90f));
            Prim(PrimitiveType.Cylinder, "Casquillo_A", g.transform, new Vector3(-L * 0.41f, D * 0.5f, 0), new Vector3(D * 1.12f, L * 0.09f, D * 1.12f), brass, false, new Vector3(0, 0, 90f));
            Prim(PrimitiveType.Cylinder, "Casquillo_B", g.transform, new Vector3(L * 0.41f, D * 0.5f, 0), new Vector3(D * 1.12f, L * 0.09f, D * 1.12f), brass, false, new Vector3(0, 0, 90f));
            Prim(PrimitiveType.Cylinder, "Banda", g.transform, new Vector3(0, D * 0.5f, 0), new Vector3(D * 1.04f, L * 0.1f, D * 1.04f), band, false, new Vector3(0, 0, 90f));
            if (burnt) Prim(PrimitiveType.Sphere, "Hollin", g.transform, new Vector3(0.012f, D * 0.85f, 0), new Vector3(0.03f, 0.012f, 0.025f), Lit("M_Soot", null, new Color(0.03f, 0.03f, 0.03f)), false);
            foreach (var r in g.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.Off;
            return g;
        }

        /// <summary>
        /// Cinta VHS reconocible (18/10): carcasa negra 18,7 × 10,3 × 2,5 cm, ventanas de las bobinas y etiqueta grande
        /// con la letra escrita a mano. La etiqueta mira hacia -Z local. Pivote en el centro.
        /// </summary>
        GameObject VhsVisual(Transform parent, string name, string letter, string labelText)
        {
            var g = new GameObject(name); g.transform.SetParent(parent, false);
            var shell = Lit("M_VhsShell", null, new Color(0.035f, 0.035f, 0.04f), 0.25f);
            var label = Lit("M_VhsLabel", null, new Color(0.93f, 0.9f, 0.78f), 0.05f);
            var window = Lit("M_VhsWindow", null, new Color(0.18f, 0.12f, 0.08f), 0.6f);
            Box("Carcasa", g.transform, Vector3.zero, new Vector3(0.187f, 0.103f, 0.025f), shell, 1f, false);
            Box("Etiqueta", g.transform, new Vector3(0, 0.022f, -0.0128f), new Vector3(0.15f, 0.042f, 0.001f), label, 1f, false);
            Box("Ventana_I", g.transform, new Vector3(-0.045f, -0.022f, -0.0128f), new Vector3(0.05f, 0.03f, 0.001f), window, 1f, false);
            Box("Ventana_D", g.transform, new Vector3(0.045f, -0.022f, -0.0128f), new Vector3(0.05f, 0.03f, 0.001f), window, 1f, false);
            var t = Text3D("Texto", g.transform, new Vector3(0, 0.022f, -0.0136f), 0f, letter + "  —  " + labelText, 0.0022f, new Color(0.08f, 0.08f, 0.25f));
            NoShadow(t.gameObject);
            foreach (var r in g.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.Off;
            return g;
        }

        /// <summary>Collider de interacción cómodo (más grande que el objeto) para cosas pequeñas.</summary>
        static void GrabBox(GameObject g, Vector3 localSize, Vector3 localCenter = default)
        {
            RemoveColliders(g);
            var bc = g.AddComponent<BoxCollider>(); bc.size = localSize; bc.center = localCenter;
        }

        /// <summary>Un único collider de caja (en la raíz) para objetos pequeños: evita que las piezas internas capten el rayo.</summary>
        static void SingleBox(GameObject g)
        {
            RemoveColliders(g);
            var b = WorldBounds(g);
            var bc = g.AddComponent<BoxCollider>();
            bc.center = g.transform.InverseTransformPoint(b.center);
            var ls = g.transform.lossyScale;
            var r = Quaternion.Inverse(g.transform.rotation);
            Vector3 sz = r * b.size; sz = new Vector3(Mathf.Abs(sz.x), Mathf.Abs(sz.y), Mathf.Abs(sz.z));
            bc.size = new Vector3(Mathf.Max(0.03f, sz.x / Mathf.Max(0.001f, ls.x)), Mathf.Max(0.03f, sz.y / Mathf.Max(0.001f, ls.y)), Mathf.Max(0.03f, sz.z / Mathf.Max(0.001f, ls.z)));
        }

        /// <summary>Cajón (desliza hacia 'front' en el mundo) o puerta con bisagra (gira hacia afuera, signo automático).</summary>
        Drawer MakeDrawer(Transform part, string id, string nounEs, string nounEn, Vector3 front, bool rotate, float amount, params Inspectable[] contents)
        {
            if (part == null) { Debug.LogWarning("[GOOD NIGHT] Falta la pieza para " + id); return null; }
            var d = part.gameObject.AddComponent<Drawer>();
            d.id = id; d.slide = part; d.nounEs = nounEs; d.nounEn = nounEn; d.rotate = rotate;
            if (rotate) d.openEuler = new Vector3(0, HingeSign(part, front, Vector3.up) * amount, 0);
            else d.openOffset = part.parent != null ? part.parent.InverseTransformVector(front.normalized * amount) : front.normalized * amount;
            d.contents = contents;
            if (!rotate) DrawerTray(part, front.normalized, amount, contents);
            FrontGrab(part);
            return d;
        }

        /// <summary>Collider de caja que cubre TODO el frente (y el tirador) con 1 cm de margen: se abre apuntando a cualquier parte.</summary>
        static void FrontGrab(Transform part)
        {
            var mfs = part.GetComponentsInChildren<MeshFilter>();
            if (mfs.Length == 0) return;
            bool has = false; Bounds lb = new Bounds();
            foreach (var mf in mfs)
            {
                if (mf.sharedMesh == null) continue;
                // solo el frente del FBX: ni la caja interior ni lo que hay DENTRO del cajón (cinta, recibo...).
                // (si el collider envolvía la cinta, el rayo daba siempre en el cajón y no se podía recoger)
                if (mf.GetComponentInParent<Inspectable>() != null) continue;
                bool inTray = false; for (var p = mf.transform; p != null && p != part; p = p.parent) if (p.name == "Caja_Cajon") inTray = true;
                if (inTray) continue;
                var mb = mf.sharedMesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 c = mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    Vector3 lp = part.InverseTransformPoint(mf.transform.TransformPoint(c));
                    if (!has) { lb = new Bounds(lp, Vector3.zero); has = true; } else lb.Encapsulate(lp);
                }
            }
            if (!has) return;
            var ls = part.lossyScale;
            var bc = part.gameObject.AddComponent<BoxCollider>();
            bc.center = lb.center;
            bc.size = lb.size + new Vector3(0.02f / Mathf.Max(0.001f, Mathf.Abs(ls.x)), 0.02f / Mathf.Max(0.001f, Mathf.Abs(ls.y)), 0.02f / Mathf.Max(0.001f, Mathf.Abs(ls.z)));
        }

        /// <summary>
        /// Los FBX solo traen el frente del cajón: se le agrega una caja real (fondo, laterales y trasera) que se desliza
        /// con él, y su contenido se coloca dentro. Cerrado queda oculto dentro del mueble.
        /// </summary>
        void DrawerTray(Transform part, Vector3 f, float amount, Inspectable[] contents)
        {
            var rends = part.GetComponentsInChildren<Renderer>();
            if (rends.Length == 0) return;
            Bounds pb = rends[0].bounds; foreach (var r in rends) pb.Encapsulate(r.bounds);
            Vector3 side = Vector3.Cross(Vector3.up, f).normalized;
            float width = Mathf.Abs(Vector3.Dot(pb.size, new Vector3(Mathf.Abs(side.x), 0, Mathf.Abs(side.z)))) - 0.035f;
            float panelT = Mathf.Abs(Vector3.Dot(pb.size, new Vector3(Mathf.Abs(f.x), 0, Mathf.Abs(f.z))));
            float h = Mathf.Max(0.05f, pb.size.y * 0.72f);
            float depth = amount + 0.05f, t = 0.012f;
            if (width < 0.05f) return;
            var mat = rends[0].sharedMaterial != null ? rends[0].sharedMaterial : mWoodDark;
            // ancla con rotación alineada al frente (Z local = hacia atrás del cajón)
            var tray = new GameObject("Caja_Cajon").transform;
            tray.position = new Vector3(pb.center.x, pb.min.y + 0.015f, pb.center.z) - f * (panelT * 0.5f);
            tray.rotation = Quaternion.LookRotation(-f, Vector3.up);
            Box("Fondo", tray, new Vector3(0, t * 0.5f, depth * 0.5f), new Vector3(width, t, depth), mat, 1f, false);
            Box("Lateral_I", tray, new Vector3(-width * 0.5f + t * 0.5f, h * 0.5f, depth * 0.5f), new Vector3(t, h, depth), mat, 1f, false);
            Box("Lateral_D", tray, new Vector3(width * 0.5f - t * 0.5f, h * 0.5f, depth * 0.5f), new Vector3(t, h, depth), mat, 1f, false);
            Box("Trasera", tray, new Vector3(0, h * 0.5f, depth - t * 0.5f), new Vector3(width, h, t), mat, 1f, false);
            foreach (var r in tray.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.Off;
            tray.SetParent(part, true);
            // contenido apoyado en el fondo, en la mitad delantera (la que queda a la vista al abrir)
            if (contents != null)
                foreach (var c in contents)
                {
                    if (c == null) continue;
                    var cb = WorldBounds(c.gameObject);
                    Vector3 target = tray.position + tray.forward * Mathf.Min(depth * 0.45f, amount * 0.55f) + Vector3.up * (t + 0.002f);
                    c.transform.position += new Vector3(target.x - cb.center.x, target.y - cb.min.y, target.z - cb.center.z);
                }
        }

        /// <summary>+1 o -1: el sentido de giro (sobre 'axis' local) que mueve la pieza hacia 'front'.</summary>
        static float HingeSign(Transform part, Vector3 front, Vector3 axis)
        {
            var r0 = part.localRotation;
            Vector3 c0 = PartCenter(part);
            part.localRotation = r0 * Quaternion.AngleAxis(60f, axis);
            Vector3 c1 = PartCenter(part);
            part.localRotation = r0;
            return Vector3.Dot(c1 - c0, front) >= 0f ? 1f : -1f;
        }

        static Vector3 PartCenter(Transform part)
        {
            var r = part.GetComponentInChildren<Renderer>();
            if (r == null) return part.position;
            var mf = r.GetComponent<MeshFilter>();
            return mf != null && mf.sharedMesh != null ? r.transform.TransformPoint(mf.sharedMesh.bounds.center) : r.bounds.center;
        }

        static Vector3 FrontDir(float yaw) => Quaternion.Euler(0, yaw, 0) * Vector3.forward;
    }
}
