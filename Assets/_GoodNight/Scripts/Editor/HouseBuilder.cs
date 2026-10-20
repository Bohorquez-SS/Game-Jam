using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static GNight.EditorTools.BuildKit;

namespace GNight.EditorTools
{
    /// <summary>
    /// Construye la casa completa (una planta). Coordenadas en metros, Y arriba.
    ///   Dormitorio x[-4,0]   z[0.6,5.1]      Baño x[0,2.5] z[0.6,3.6]     Secreta(06) x[2.5,4.6] z[0.6,3.6]
    ///   Pasillo    x[-4,4.6] z[-0.6,0.6]
    ///   Sala       x[-4,1]   z[-5.1,-0.6]    Cocina x[1,4.6] z[-5.1,-0.6]
    /// Puerta principal en la pared sur de la sala. Ventanas: dormitorio (N), sala (O), cocina (S).
    /// </summary>
    public class HouseBuilder
    {
        // ---------------------------------------------------------------- resultado (para GameRefs)
        public Transform root, exterior;
        public DoorController doorBedroom, doorBathroom, doorKitchen, doorFront, doorMetal;
        public DeadBolt bolt;
        public WindowController[] windows = new WindowController[3];
        public AudioSource[] windowOutside = new AudioSource[3];
        public AudioSource[] windowRain = new AudioSource[3];
        public Computer computer;
        public RoomLight bedroomLamp;
        public SlidingWardrobe wardrobe;
        public Shutter[] shuttersKitchen = new Shutter[2], shuttersBedroom = new Shutter[1];
        public List<RoomZone> zones = new List<RoomZone>();
        public List<RoomLight> roomLights = new List<RoomLight>();
        public List<Light> emergency = new List<Light>();
        public Light moon, sun;
        public List<Renderer> skyRenderers = new List<Renderer>();
        public Material skyNight, skyDawn;
        public PanelStation monitorStation, alarmStation, powerStation, isolationStation;
        public StoryInteractable recorder, auxLever;
        public FlashlightPickup flashlightPickup;
        public MonitorScreen[] secretMonitors = new MonitorScreen[4];
        public MonitorScreen salaMonitor;
        public SecurityCam[] cams = new SecurityCam[4];
        public TextMesh alarmScreen, metalSign;
        public Drawer receiptDrawer, planDrawer;
        public DocumentData docReceipt, docPlan, docFatherNote, docIsolation, docPhoto;
        public Transform shadowFigure, shadowStart, shadowEnd, ghost, ghostFar, ghostNear;
        public List<Renderer> ghostRenderers = new List<Renderer>();
        public Transform doppelInBed, doppelTorso, doppelHead;
        public AudioSource frontDoorOutside, hallSteps, bathroomInside, kitchenVoice, secretWall, thunder;
        public Transform[] hallStepPath;
        public Lightning lightning;

        // materiales
        Material mWallBed, mWallHall, mWallLiving, mWallKitchen, mWallBath, mConcrete, mSiding, mWoodFloor, mKitchenFloor, mBathFloor, mCeiling,
                 mPaint, mWoodDark, mWoodLight, mDoor, mCarpet, mBlanket, mSheet, mCurtain, mBeige, mDarkPlastic, mMetal, mSofa, mCounter,
                 mFridge, mGrass, mFence, mMetalDoor, mCork, mCable, mPoster, mPhoto, mPhotoFather, mScreenDesktop, mMonitorOff, mStatic,
                 mGlass, mRainGlass, mRainFar, mBookRed, mBookBlue, mBookGreen, mBookCream, mBulbOn, mBulbOff, mShadeOn, mShadeOff,
                 mEntity, mEyes, mHoodie, mJeans, mSkin, mHair, mMirror, mTrunk, mLeaves, mRoof, mRed;

        public void Build()
        {
            root = Group("Casa", null);
            Materials();
            Shell();
            Doors();
            Windows();
            Bedroom();
            Hallway();
            Living();
            Kitchen();
            Bathroom();
            Secret();
            Exterior();
            Zones();
            Cameras();
            Ghost();
            DoppelInBed();
        }

        // ================================================================= materiales
        void Materials()
        {
            mWallBed = Lit("M_WallBedroom", "wallpaper", Color.white);
            mWallHall = Lit("M_WallHall", "wall_hall", Color.white);
            mWallLiving = Lit("M_WallLiving", "wall_living", Color.white);
            mWallKitchen = Lit("M_WallKitchen", "tile_kitchen", Color.white, 0.3f);
            mWallBath = Lit("M_WallBath", "tile_bath", Color.white, 0.4f);
            mConcrete = Lit("M_Concrete", "concrete", Color.white);
            mSiding = Lit("M_Siding", "siding", Color.white);
            mWoodFloor = Lit("M_WoodFloor", "wood_floor", Color.white, 0.25f);
            mKitchenFloor = Lit("M_KitchenFloor", "floor_kitchen", Color.white, 0.3f);
            mBathFloor = Lit("M_BathFloor", "floor_bath", Color.white, 0.3f);
            mCeiling = Lit("M_Ceiling", "ceiling", Color.white);
            mPaint = Lit("M_Paint", "paint_white", Color.white);
            mWoodDark = Lit("M_WoodDark", "wood_dark", Color.white);
            mWoodLight = Lit("M_WoodLight", "wood_light", Color.white);
            mDoor = Lit("M_Door", "door", Color.white);
            mCarpet = Lit("M_Carpet", "carpet", Color.white);
            mBlanket = Lit("M_Blanket", "blanket", Color.white);
            mSheet = Lit("M_Sheet", "sheet", Color.white);
            mCurtain = Lit("M_Curtain", "curtain", Color.white);
            mBeige = Lit("M_PlasticBeige", "plastic_beige", Color.white);
            mDarkPlastic = Lit("M_PlasticDark", "plastic_dark", Color.white);
            mMetal = Lit("M_Metal", "metal", Color.white, 0.4f);
            mSofa = Lit("M_Sofa", "fabric_sofa", Color.white);
            mCounter = Lit("M_Counter", "counter", Color.white, 0.3f);
            mFridge = Lit("M_Fridge", "fridge", Color.white, 0.4f);
            mGrass = Lit("M_Grass", "grass", Color.white);
            mFence = Lit("M_Fence", "fence", Color.white);
            mMetalDoor = Lit("M_MetalDoor", "metal_door", Color.white, 0.3f);
            mCork = Lit("M_Cork", "cork", Color.white);
            mCable = Lit("M_Cable", "cable", Color.white);
            mPoster = Lit("M_Poster", "poster", Color.white);
            mPhoto = Lit("M_Photo", "photo_family", Color.white);
            mPhotoFather = Lit("M_PhotoFather", "photo_father", Color.white);
            mMonitorOff = Lit("M_MonitorOff", "monitor_off", Color.white, 0.7f);
            mBookRed = Lit("M_BookRed", "noise_light", new Color(0.45f, 0.12f, 0.1f));
            mBookBlue = Lit("M_BookBlue", "noise_light", new Color(0.12f, 0.18f, 0.4f));
            mBookGreen = Lit("M_BookGreen", "noise_light", new Color(0.15f, 0.3f, 0.18f));
            mBookCream = Lit("M_BookCream", "noise_light", new Color(0.65f, 0.6f, 0.5f));
            mBulbOff = Lit("M_BulbOff", "noise_light", new Color(0.5f, 0.48f, 0.42f));
            mShadeOff = Lit("M_ShadeOff", "noise_light", new Color(0.55f, 0.48f, 0.38f));
            mEntity = Lit("M_Entity", "plastic_dark", new Color(0.04f, 0.04f, 0.04f));
            mHoodie = Lit("M_Hoodie", "noise_light", new Color(0.18f, 0.24f, 0.36f));
            mJeans = Lit("M_Jeans", "noise_light", new Color(0.12f, 0.16f, 0.28f));
            mSkin = Lit("M_Skin", "noise_light", new Color(0.78f, 0.6f, 0.48f));
            mHair = Lit("M_Hair", "noise_light", new Color(0.08f, 0.06f, 0.05f));
            mMirror = Lit("M_Mirror", "noise_light", new Color(0.18f, 0.22f, 0.26f), 0.95f, 0.6f);
            mTrunk = Lit("M_Trunk", "wood_dark", new Color(0.5f, 0.45f, 0.4f));
            mLeaves = Lit("M_Leaves", "grass", new Color(0.55f, 0.7f, 0.55f));
            mRoof = Lit("M_Roof", "concrete", new Color(0.25f, 0.22f, 0.22f));
            mRed = Lit("M_RedPlastic", "noise_light", new Color(0.5f, 0.05f, 0.04f));

            mBulbOn = UnlitColor("M_BulbOn", new Color(1f, 0.9f, 0.7f));
            mShadeOn = UnlitColor("M_ShadeOn", new Color(1f, 0.82f, 0.55f));
            mEyes = UnlitColor("M_EntityEyes", new Color(0.85f, 0.85f, 0.8f));
            mScreenDesktop = Unlit("M_ScreenDesktop", "screen_desktop", false);
            mStatic = Unlit("M_Static", "crt_static", false);
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
                new RoomDef { id = "dormitorio", x0 = -4, x1 = 0, z0 = 0.6f, z1 = 5.1f, wall = mWallBed, floor = mWoodFloor, ceil = mCeiling,
                    n = new[] { Win(-1.4f) }, s = new[] { Door(-2.6f) }, w = none, e = none },
                new RoomDef { id = "bano", x0 = 0, x1 = 2.5f, z0 = 0.6f, z1 = 3.6f, wall = mWallBath, floor = mBathFloor, ceil = mCeiling,
                    n = new[] { Win(1.8f, 0.6f, 1.5f, 2.0f) }, s = new[] { Door(1.0f, 0.85f) }, w = none, e = none },
                new RoomDef { id = "secreta", x0 = 2.5f, x1 = 4.6f, z0 = 0.6f, z1 = 3.6f, wall = mConcrete, floor = mConcrete, ceil = mConcrete,
                    n = none, s = new[] { Door(3.5f) }, w = none, e = none },
                new RoomDef { id = "pasillo", x0 = -4, x1 = 4.6f, z0 = -0.6f, z1 = 0.6f, wall = mWallHall, floor = mWoodFloor, ceil = mCeiling,
                    n = new[] { Door(-2.6f), Door(1.0f, 0.85f), Door(3.5f) }, s = new[] { Door(-1.0f, 1.3f, 2.2f), Door(2.05f) }, w = none, e = none },
                new RoomDef { id = "sala", x0 = -4, x1 = 1.0f, z0 = -5.1f, z1 = -0.6f, wall = mWallLiving, floor = mWoodFloor, ceil = mCeiling,
                    n = new[] { Door(-1.0f, 1.3f, 2.2f) }, s = new[] { Door(-2.15f, 0.95f, 2.1f) }, w = new[] { Win(-2.8f) }, e = new[] { Door(-2.05f) } },
                new RoomDef { id = "cocina", x0 = 1.0f, x1 = 4.6f, z0 = -5.1f, z1 = -0.6f, wall = mWallKitchen, floor = mKitchenFloor, ceil = mCeiling,
                    n = new[] { Door(2.05f) }, s = new[] { Win(3.2f) }, w = new[] { Door(-2.05f) }, e = none },
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
                // zócalo
                var trim = mWoodDark;
                BoxMM("Zocalo", g, new Vector3(r.x0 + T, 0, r.z1 - T - 0.015f), new Vector3(r.x1 - T, 0.09f, r.z1 - T), trim, 1f, false);
                BoxMM("Zocalo", g, new Vector3(r.x0 + T, 0, r.z0 + T), new Vector3(r.x1 - T, 0.09f, r.z0 + T + 0.015f), trim, 1f, false);
            }
            // capa exterior (revestimiento)
            var ext = Group("Fachada", shell);
            float o = T * 0.5f;
            WallX(ext, "Fach_S", -5.1f - o, -4 - T, 4.6f + T, mSiding, 1f, Door(-2.15f, 0.95f, 2.1f), Win(3.2f));
            WallZ(ext, "Fach_E", 4.6f + o, -5.1f - T, 3.6f + T, mSiding, 1f);
            WallX(ext, "Fach_NE", 3.6f + o, 0f, 4.6f + T, mSiding, 1f, Win(1.8f, 0.6f, 1.5f, 2.0f));
            WallZ(ext, "Fach_Escalon", 0f + o, 3.6f, 5.1f + T, mSiding, 1f);
            WallX(ext, "Fach_N", 5.1f + o, -4 - T, 0f + T, mSiding, 1f, Win(-1.4f));
            WallZ(ext, "Fach_O", -4f - o, -5.1f - T, 5.1f + T, mSiding, 1f, Win(-2.8f));
            BoxMM("Techo_Ext_A", ext, new Vector3(-4.3f, H + 0.1f, -5.4f), new Vector3(4.9f, H + 0.25f, 3.9f), mRoof, 2f, false);
            BoxMM("Techo_Ext_B", ext, new Vector3(-4.3f, H + 0.1f, 3.9f), new Vector3(0.3f, H + 0.25f, 5.4f), mRoof, 2f, false);
        }

        // ================================================================= puertas
        /// <summary>Puerta con bisagra. yaw: 0 abre hacia -Z, 180 hacia +Z, -90 hacia +X, 90 hacia -X.</summary>
        DoorController MakeDoor(string name, string id, Vector3 center, float yaw, float width, float height, Material m, string es, string en, out Transform leaf)
        {
            var frame = Group(name, root, center, yaw);
            var hinge = Group("Bisagra", frame, new Vector3(-width * 0.5f, 0, 0));
            leaf = Box("Hoja", hinge, new Vector3(width * 0.5f, height * 0.5f, 0), new Vector3(width - 0.02f, height - 0.02f, 0.045f), m, height).transform;
            Prim(PrimitiveType.Sphere, "Perilla_A", leaf, new Vector3(width * 0.5f - 0.1f, -0.05f, 0.04f), Vector3.one * 0.06f, mMetal);
            Prim(PrimitiveType.Sphere, "Perilla_B", leaf, new Vector3(width * 0.5f - 0.1f, -0.05f, -0.04f), Vector3.one * 0.06f, mMetal);
            // marcos en ambas caras
            foreach (float side in new[] { -1f, 1f })
            {
                Box("Marco_I", frame, new Vector3(-width * 0.5f - 0.04f, height * 0.5f, side * 0.075f), new Vector3(0.08f, height + 0.04f, 0.03f), mPaint, 1f, false);
                Box("Marco_D", frame, new Vector3(width * 0.5f + 0.04f, height * 0.5f, side * 0.075f), new Vector3(0.08f, height + 0.04f, 0.03f), mPaint, 1f, false);
                Box("Marco_S", frame, new Vector3(0, height + 0.04f, side * 0.075f), new Vector3(width + 0.16f, 0.08f, 0.03f), mPaint, 1f, false);
            }
            var d = hinge.gameObject.AddComponent<DoorController>();
            d.id = id; d.displayEs = es; d.displayEn = en; d.openAngle = 90f;
            d.openClip = Clip("door_creak"); d.closeClip = Clip("door_close"); d.lockedClip = Clip("lock_clunk");
            return d;
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
            Transform leaf;
            doorBedroom = MakeDoor("Puerta_Dormitorio", "bedroom", new Vector3(-2.6f, 0, 0.6f), 180f, 0.9f, 2.05f, mDoor, "puerta del dormitorio", "bedroom door", out leaf);
            doorBedroom.isOpen = true;
            doorBathroom = MakeDoor("Puerta_Bano", "bathroom", new Vector3(1.0f, 0, 0.6f), 180f, 0.85f, 2.05f, mDoor, "puerta del baño", "bathroom door", out leaf);
            doorKitchen = MakeDoor("Puerta_Cocina", "kitchen", new Vector3(2.05f, 0, -0.6f), 0f, 0.9f, 2.05f, mDoor, "puerta de la cocina", "kitchen door", out leaf);
            doorKitchen.isOpen = true;
            doorFront = MakeDoor("Puerta_Principal", "front", new Vector3(-2.15f, 0, -5.1f), 180f, 0.95f, 2.1f, mDoor, "puerta principal", "front door", out leaf);
            doorFront.isOpen = true;
            // cerrojo interior (lado sala, cara +Z de la hoja)
            var boltBase = Box("Cerrojo", leaf, new Vector3(0.3f, 0.15f, -0.035f), new Vector3(0.12f, 0.05f, 0.03f), mMetal, 1f);
            var boltBar = Box("Barra", boltBase.transform, new Vector3(0.0f, 0, -0.015f), new Vector3(0.1f, 0.025f, 0.02f), mMetal, 1f, false);
            bolt = boltBase.AddComponent<DeadBolt>();
            bolt.door = doorFront; bolt.boltVisual = boltBar.transform; bolt.clip = Clip("lock_clunk");

            doorMetal = MakeDoor("Puerta_Metalica_06", "metal", new Vector3(3.5f, 0, 0.6f), 180f, 0.9f, 2.05f, mMetalDoor, "puerta metálica", "metal door", out leaf);
            doorMetal.locked = true;
            doorMetal.lockedEs = "Está sellada. No tiene manija de este lado."; doorMetal.lockedEn = "It's sealed. There's no handle on this side.";
            metalSign = Text3D("Inscripcion", leaf, new Vector3(0f, 0.35f, 0.03f), 180f,
                "NO ABRIR SI TODAVÍA\nESCUCHAS SU VOZ.", 0.006f, new Color(0.85f, 0.15f, 0.1f));

            // persianas de aislamiento (escondidas sobre el techo)
            shuttersBedroom[0] = MakeShutter("Persiana_Dormitorio", new Vector3(-2.6f, 0, 0.6f - 0.075f), 0.9f, 2.05f, true);
            shuttersKitchen[0] = MakeShutter("Persiana_Cocina_Pasillo", new Vector3(2.05f, 0, -0.6f + 0.075f), 0.9f, 2.05f, true);
            shuttersKitchen[1] = MakeShutter("Persiana_Cocina_Sala", new Vector3(1.0f, 0, -2.05f), 0.9f, 2.05f, false);
        }

        // ================================================================= ventanas
        /// <summary>Ventana de guillotina. yaw: 0 = pared norte (interior hacia -Z), 180 = sur, -90 = oeste, 90 = este.</summary>
        WindowController MakeWindow(string name, string id, Vector3 center, float yaw, int index)
        {
            float w = 1.2f, b = 0.9f, t = 2.0f, h = (t - b) * 0.5f;
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
            // cortinas
            Box("Barra", g, new Vector3(0, t + 0.47f - 0.0f, -0.14f), new Vector3(w + 0.75f, 0.025f, 0.025f), mMetal, 1f, false);
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
            windows[index] = wc; windowOutside[index] = knock; windowRain[index] = rain;
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
            MakeWindow("Ventana_Dormitorio", "bedroom", new Vector3(-1.4f, 0, 5.1f), 0f, 0);
            MakeWindow("Ventana_Sala", "living", new Vector3(-4f, 0, -2.8f), -90f, 1);
            MakeWindow("Ventana_Cocina", "kitchen", new Vector3(3.2f, 0, -5.1f), 180f, 2);
            // ventanita del baño (fija)
            var bw = Group("Ventana_Bano", root, new Vector3(1.8f, 0, 3.6f), 0f);
            Quad("Vidrio", bw, new Vector3(0, 1.75f, 0.0f), new Vector2(0.6f, 0.5f), 0f, mGlass);
            Quad("Lluvia", bw, new Vector3(0, 1.75f, 0.04f), new Vector2(0.6f, 0.5f), 0f, mRainGlass).AddComponent<TextureScroll>().speed = new Vector2(0, 0.1f);
        }

        // ================================================================= luces de habitación
        RoomLight CeilingLight(string zone, Transform parent, Vector3 pos, Color c, float intensity, float range, bool shadows = false)
        {
            Box("Plafon", parent, pos + new Vector3(0, 0.05f, 0), new Vector3(0.36f, 0.04f, 0.36f), mPaint, 1f, false);
            var bulb = Prim(PrimitiveType.Sphere, "Foco", parent, pos, new Vector3(0.18f, 0.08f, 0.18f), mBulbOn);
            NoShadow(bulb);
            var l = MakeLight("Luz_" + zone, parent, LightType.Point, pos - new Vector3(0, 0.15f, 0), c, intensity, range, shadows ? LightShadows.Soft : LightShadows.None);
            var rl = l.gameObject.AddComponent<RoomLight>();
            rl.zone = zone; rl.lightSource = l; rl.bulb = bulb.GetComponent<Renderer>(); rl.onMat = mBulbOn; rl.offMat = mBulbOff;
            roomLights.Add(rl);
            return rl;
        }

        void Switch(string zone, Transform parent, Vector3 pos, float yaw)
        {
            var g = Group("Interruptor_" + zone, parent, pos, yaw);
            Box("Placa", g, Vector3.zero, new Vector3(0.08f, 0.12f, 0.015f), mPaint, 1f);
            var tg = Box("Tecla", g, new Vector3(0, 0, -0.012f), new Vector3(0.025f, 0.04f, 0.015f), mBeige, 1f, false);
            var s = g.gameObject.AddComponent<LightSwitch>(); s.zone = zone; s.toggle = tg.transform;
        }

        // ================================================================= dormitorio (prototipo)
        void Bedroom()
        {
            var r = Group("Dormitorio", root, new Vector3(-2f, 0, 2.85f)); // origen = centro de la habitación (coordenadas del prototipo)
            // cama
            var bed = Group("Cama", r);
            Box("Base", bed, new Vector3(-1.48f, 0.2f, 1.05f), new Vector3(1.0f, 0.3f, 2.0f), mWoodDark, 1f);
            Box("Colchon", bed, new Vector3(-1.48f, 0.45f, 1.05f), new Vector3(0.96f, 0.2f, 1.94f), mSheet, 1f);
            Box("Frazada", bed, new Vector3(-1.48f, 0.575f, 0.7f), new Vector3(1.0f, 0.06f, 1.35f), mBlanket, 1f);
            Box("Frazada_Lado", bed, new Vector3(-0.975f, 0.42f, 0.7f), new Vector3(0.02f, 0.3f, 1.35f), mBlanket, 1f, false);
            Box("Almohada", bed, new Vector3(-1.48f, 0.61f, 1.78f), new Vector3(0.7f, 0.12f, 0.38f), mSheet, 1f);
            Box("Cabecera", bed, new Vector3(-1.48f, 0.65f, 2.1f), new Vector3(1.04f, 1.0f, 0.06f), mWoodDark, 1f);
            bed.gameObject.AddComponent<Bed>();
            // mesa de noche, lámpara, reloj
            var ns = Group("Mesa_De_Noche", r);
            Box("Mueble", ns, new Vector3(-0.705f, 0.27f, 1.825f), new Vector3(0.45f, 0.54f, 0.45f), mWoodDark, 1f);
            Box("Cajon", ns, new Vector3(-0.705f, 0.36f, 1.595f), new Vector3(0.38f, 0.14f, 0.01f), mWoodLight, 1f, false);
            var lampG = Group("Lampara", ns);
            Prim(PrimitiveType.Cylinder, "Base", lampG, new Vector3(-0.78f, 0.57f, 1.9f), new Vector3(0.12f, 0.03f, 0.12f), mMetal, true);
            NoShadow(Prim(PrimitiveType.Cylinder, "Tubo", lampG, new Vector3(-0.78f, 0.72f, 1.9f), new Vector3(0.025f, 0.13f, 0.025f), mMetal, true));
            var shade = Prim(PrimitiveType.Cylinder, "Pantalla", lampG, new Vector3(-0.78f, 0.9f, 1.9f), new Vector3(0.28f, 0.1f, 0.28f), mShadeOn, true);
            NoShadow(shade);
            var ll = MakeLight("Luz_Lampara", lampG, LightType.Point, new Vector3(-0.78f, 0.86f, 1.85f), new Color(1f, 0.72f, 0.45f), 1.3f, 4.2f, LightShadows.Soft);
            bedroomLamp = ll.gameObject.AddComponent<RoomLight>();
            bedroomLamp.zone = "dormitorio"; bedroomLamp.lightSource = ll; bedroomLamp.bulb = shade.GetComponent<Renderer>(); bedroomLamp.onMat = mShadeOn; bedroomLamp.offMat = mShadeOff;
            roomLights.Add(bedroomLamp);
            Box("Reloj", ns, new Vector3(-0.58f, 0.575f, 1.72f), new Vector3(0.15f, 0.07f, 0.06f), mDarkPlastic, 1f);
            Text3D("Reloj_Display", ns, new Vector3(-0.58f, 0.575f, 1.688f), 0f, "23:47", 0.0045f, new Color(1f, 0.15f, 0.1f)).gameObject.AddComponent<ClockDisplay>();
            Box("Celular", ns, new Vector3(-0.86f, 0.548f, 1.72f), new Vector3(0.07f, 0.012f, 0.14f), mDarkPlastic, 1f);
            // escritorio
            var desk = Group("Escritorio", r);
            Box("Tablero", desk, new Vector3(1.64f, 0.74f, -0.2f), new Vector3(0.6f, 0.04f, 1.3f), mWoodLight, 1f);
            Box("Lateral_A", desk, new Vector3(1.64f, 0.36f, -0.83f), new Vector3(0.56f, 0.72f, 0.03f), mWoodLight, 1f);
            Box("Lateral_B", desk, new Vector3(1.64f, 0.36f, 0.43f), new Vector3(0.56f, 0.72f, 0.03f), mWoodLight, 1f);
            var chair = Group("Silla", r);
            Box("Asiento", chair, new Vector3(1.02f, 0.45f, -0.2f), new Vector3(0.42f, 0.05f, 0.42f), mWoodDark, 1f, false);
            Box("Respaldo", chair, new Vector3(0.82f, 0.75f, -0.2f), new Vector3(0.04f, 0.55f, 0.42f), mWoodDark, 1f, false);
            var pc = Group("Computadora", desk);
            Box("Monitor", pc, new Vector3(1.74f, 0.97f, -0.25f), new Vector3(0.42f, 0.38f, 0.44f), mBeige, 1f);
            var screen = Quad("Pantalla", pc, new Vector3(1.525f, 0.98f, -0.25f), new Vector2(0.34f, 0.27f), 90f, mScreenDesktop);
            Box("Teclado", pc, new Vector3(1.47f, 0.77f, -0.25f), new Vector3(0.16f, 0.02f, 0.44f), mBeige, 1f);
            Box("Torre", pc, new Vector3(1.66f, 0.24f, 0.15f), new Vector3(0.42f, 0.44f, 0.18f), mBeige, 1f);
            var glow = MakeLight("Luz_Monitor", pc, LightType.Point, new Vector3(1.34f, 1.0f, -0.25f), new Color(0.55f, 0.75f, 1f), 0.6f, 2.3f, LightShadows.None);
            glow.gameObject.AddComponent<LightFlicker>().amount = 0.08f;
            computer = pc.gameObject.AddComponent<Computer>();
            computer.screen = screen.GetComponent<Renderer>(); computer.onMat = mScreenDesktop; computer.offMat = mMonitorOff; computer.glow = glow;
            // ventilador
            var fan = Group("Ventilador", desk);
            Prim(PrimitiveType.Cylinder, "Base", fan, new Vector3(1.76f, 0.775f, 0.28f), new Vector3(0.14f, 0.015f, 0.14f), mDarkPlastic, true);
            Box("Motor", fan, new Vector3(1.78f, 0.95f, 0.28f), new Vector3(0.1f, 0.09f, 0.09f), mDarkPlastic, 1f);
            var blades = Group("Aspas", fan, new Vector3(1.715f, 0.95f, 0.28f));
            Box("Aspa1", blades, Vector3.zero, new Vector3(0.008f, 0.22f, 0.05f), mBeige, 1f, false);
            Box("Aspa2", blades, Vector3.zero, new Vector3(0.008f, 0.05f, 0.22f), mBeige, 1f, false);
            blades.gameObject.AddComponent<Spinner>().speed = 1100f;
            Src("Ventilador_Sonido", fan, new Vector3(1.75f, 0.95f, 0.28f), Clip("fan_hum"), 0.3f, true, 1f, 0.5f, 6f);
            // repisa con libros
            var shelf = Group("Repisa", r);
            Box("Tabla", shelf, new Vector3(1.82f, 1.5f, -0.2f), new Vector3(0.26f, 0.03f, 1.1f), mWoodDark, 1f);
            Books(shelf, new Vector3(1.84f, 1.515f, -0.7f), 0.95f, true);
            // póster, alfombra, foto
            var poster = Quad("Poster", r, new Vector3(0.8f, 1.6f, -2.18f), new Vector2(0.6f, 0.8f), 180f, mPoster, true);
            var px = poster.AddComponent<Examinable>(); px.nameEs = "póster"; px.nameEn = "poster";
            px.textEs = "Un póster viejo. Ya ni recuerdas por qué lo compraste."; px.textEn = "An old poster. You don't even remember buying it.";
            Box("Alfombra", r, new Vector3(0.1f, 0.006f, -0.4f), new Vector3(1.6f, 0.012f, 1.2f), mCarpet, 1.6f, false);
            // armario con rendijas (escondite)
            var wd = Group("Armario", r);
            HidingCloset(wd, new Vector3(-1.98f, 0, -2.15f), new Vector3(-1.43f, 2.0f, -1.1f), true, "armario", "wardrobe",
                new Vector3(-1.72f, 0f, -1.62f), 100f, new Vector3(-1.0f, 0f, -1.62f), 90f);
            CeilingLight("dormitorio", r, new Vector3(0, H - 0.08f, 0), new Color(1f, 0.85f, 0.65f), 1.0f, 5f);
            Switch("dormitorio", r, new Vector3(-1.3f, 1.25f, -2.18f), 180f);
        }

        void Books(Transform parent, Vector3 start, float length, bool alongZ)
        {
            var mats = new[] { mBookRed, mBookBlue, mBookGreen, mBookCream };
            var rnd = new System.Random(start.GetHashCode());
            float d = 0; int i = 0;
            while (d < length)
            {
                float w = 0.03f + (float)rnd.NextDouble() * 0.03f, h = 0.18f + (float)rnd.NextDouble() * 0.08f;
                Vector3 p = start + (alongZ ? new Vector3(0, h * 0.5f, d + w * 0.5f) : new Vector3(d + w * 0.5f, h * 0.5f, 0));
                Box("Libro", parent, p, alongZ ? new Vector3(0.17f, h, w) : new Vector3(w, h, 0.17f), mats[i++ % 4], 1f, false);
                d += w + 0.004f;
            }
        }

        /// <summary>Armario/despensa/casillero con puertas de rendijas (se ve hacia afuera).</summary>
        HidingSpot HidingCloset(Transform parent, Vector3 min, Vector3 max, bool doorsOnMaxX, string es, string en,
                                Vector3 inPos, float inYaw, Vector3 outPos, float outYaw, bool doorsOnMaxZ = false)
        {
            float wd = max.x - min.x, hh = max.y, dz = max.z - min.z;
            Vector3 c = (min + max) * 0.5f;
            var m = mWoodDark;
            // laterales completos excepto el frente
            if (!doorsOnMaxZ)
            {
                Box("Fondo", parent, new Vector3(doorsOnMaxX ? min.x + 0.015f : max.x - 0.015f, hh * 0.5f, c.z), new Vector3(0.03f, hh, dz), m, 1f);
                Box("Lado_A", parent, new Vector3(c.x, hh * 0.5f, min.z + 0.015f), new Vector3(wd, hh, 0.03f), m, 1f);
                Box("Lado_B", parent, new Vector3(c.x, hh * 0.5f, max.z - 0.015f), new Vector3(wd, hh, 0.03f), m, 1f);
            }
            else
            {
                Box("Fondo", parent, new Vector3(c.x, hh * 0.5f, min.z + 0.015f), new Vector3(wd, hh, 0.03f), m, 1f);
                Box("Lado_A", parent, new Vector3(min.x + 0.015f, hh * 0.5f, c.z), new Vector3(0.03f, hh, dz), m, 1f);
                Box("Lado_B", parent, new Vector3(max.x - 0.015f, hh * 0.5f, c.z), new Vector3(0.03f, hh, dz), m, 1f);
            }
            Box("Techo", parent, new Vector3(c.x, hh - 0.015f, c.z), new Vector3(wd, 0.03f, dz), m, 1f);
            Box("Piso", parent, new Vector3(c.x, 0.05f, c.z), new Vector3(wd, 0.1f, dz), m, 1f);
            // frente de rendijas
            if (!doorsOnMaxZ)
            {
                float x = doorsOnMaxX ? max.x - 0.015f : min.x + 0.015f;
                Box("Larguero_A", parent, new Vector3(x, hh * 0.5f, min.z + 0.05f), new Vector3(0.03f, hh, 0.05f), m, 1f);
                Box("Larguero_B", parent, new Vector3(x, hh * 0.5f, max.z - 0.05f), new Vector3(0.03f, hh, 0.05f), m, 1f);
                Box("Larguero_C", parent, new Vector3(x, hh * 0.5f, c.z), new Vector3(0.03f, hh, 0.04f), m, 1f);
                Box("Trav_Inf", parent, new Vector3(x, 0.15f, c.z), new Vector3(0.03f, 0.1f, dz), m, 1f);
                Box("Trav_Sup", parent, new Vector3(x, hh - 0.1f, c.z), new Vector3(0.03f, 0.1f, dz), m, 1f);
                for (float y = 0.26f; y < hh - 0.16f; y += 0.1f)
                    Box("Rendija", parent, new Vector3(x, y, c.z), new Vector3(0.012f, 0.075f, dz - 0.08f), m, 1f, true, new Vector3(0, 0, doorsOnMaxX ? 40f : -40f));
            }
            else
            {
                float z = max.z - 0.015f;
                Box("Larguero_A", parent, new Vector3(min.x + 0.05f, hh * 0.5f, z), new Vector3(0.05f, hh, 0.03f), m, 1f);
                Box("Larguero_B", parent, new Vector3(max.x - 0.05f, hh * 0.5f, z), new Vector3(0.05f, hh, 0.03f), m, 1f);
                Box("Trav_Inf", parent, new Vector3(c.x, 0.15f, z), new Vector3(wd, 0.1f, 0.03f), m, 1f);
                Box("Trav_Sup", parent, new Vector3(c.x, hh - 0.1f, z), new Vector3(wd, 0.1f, 0.03f), m, 1f);
                for (float y = 0.26f; y < hh - 0.16f; y += 0.1f)
                    Box("Rendija", parent, new Vector3(c.x, y, z), new Vector3(wd - 0.08f, 0.075f, 0.012f), m, 1f, true, new Vector3(-40f, 0, 0));
            }
            var ins = Group("Pose_Dentro", parent, inPos, inYaw);
            var outs = Group("Pose_Fuera", parent, outPos, outYaw);
            var hs = parent.gameObject.AddComponent<HidingSpot>();
            hs.nameEs = es; hs.nameEn = en; hs.inside = ins; hs.outside = outs; hs.insideEye = Mathf.Min(1.5f, hh - 0.3f); hs.lookLimit = 50f;
            return hs;
        }

        // ================================================================= pasillo
        void Hallway()
        {
            var r = Group("Pasillo", root);
            CeilingLight("pasillo", r, new Vector3(-1.8f, H - 0.08f, 0), new Color(1f, 0.82f, 0.6f), 0.8f, 4.5f);
            CeilingLight("pasillo", r, new Vector3(2.2f, H - 0.08f, 0), new Color(1f, 0.82f, 0.6f), 0.8f, 4.5f);
            Switch("pasillo", r, new Vector3(-3.4f, 1.25f, 0.53f), 0f);
            // panel de la alarma (pared sur, mira al norte)
            var ap = Group("Panel_Alarma", r, new Vector3(0.3f, 1.45f, -0.53f), 180f);
            Box("Caja", ap, Vector3.zero, new Vector3(0.3f, 0.42f, 0.04f), mBeige, 1f);
            Box("Pantalla", ap, new Vector3(0, 0.1f, -0.022f), new Vector3(0.22f, 0.1f, 0.005f), mDarkPlastic, 1f, false);
            for (int i = 0; i < 12; i++) Box("Tecla", ap, new Vector3(-0.07f + (i % 3) * 0.07f, -0.02f - (i / 3) * 0.045f, -0.022f), new Vector3(0.05f, 0.03f, 0.008f), mDarkPlastic, 1f, false);
            alarmScreen = Text3D("Display", ap, new Vector3(0, 0.1f, -0.026f), 0f, "GUARDIAN", 0.0028f, new Color(1f, 0.35f, 0.25f));
            alarmStation = ap.gameObject.AddComponent<PanelStation>();
            alarmStation.kind = PanelStation.Kind.Alarm; alarmStation.promptEs = "Introducir código"; alarmStation.promptEn = "Enter code";
            // armario del pasillo (oculta la puerta 06) — se desliza hacia -X
            var w = Group("Armario_Pasillo", r, new Vector3(3.5f, 0, 0.32f));
            Box("Cuerpo", w, new Vector3(0, 1.05f, 0), new Vector3(1.1f, 2.1f, 0.48f), mWoodDark, 1f);
            Box("Puerta_I", w, new Vector3(-0.275f, 1.05f, -0.245f), new Vector3(0.53f, 1.95f, 0.02f), mWoodLight, 1f, false);
            Box("Puerta_D", w, new Vector3(0.275f, 1.05f, -0.245f), new Vector3(0.53f, 1.95f, 0.02f), mWoodLight, 1f, false);
            wardrobe = w.gameObject.AddComponent<SlidingWardrobe>();
            wardrobe.slideOffset = new Vector3(-1.15f, 0, 0);
            var wx = w.gameObject.AddComponent<Examinable>();
            wx.nameEs = "armario"; wx.nameEn = "wardrobe";
            wx.textEs = "Un armario viejo, pesado. Está atornillado a algo detrás."; wx.textEn = "A heavy old wardrobe. It's bolted to something behind it.";
            // cuadros
            Quad("Cuadro", r, new Vector3(-0.5f, 1.6f, 0.535f), new Vector2(0.4f, 0.3f), 0f, mPhoto);
            Quad("Cuadro2", r, new Vector3(0.9f, 1.55f, -0.535f), new Vector2(0.3f, 0.4f), 180f, mPoster);
            // emergencia
            emergency.Add(MakeLight("Emergencia_Pasillo_O", r, LightType.Point, new Vector3(-3.5f, 2.3f, 0f), new Color(1f, 0.08f, 0.05f), 1.6f, 6f, LightShadows.None));
            emergency.Add(MakeLight("Emergencia_Pasillo_E", r, LightType.Point, new Vector3(3.0f, 2.3f, 0f), new Color(1f, 0.08f, 0.05f), 1.6f, 6f, LightShadows.None));
            foreach (var e in emergency) { Box("Caja_Emergencia", e.transform, new Vector3(0, 0.2f, 0), new Vector3(0.25f, 0.08f, 0.1f), mRed, 1f, false); e.enabled = false; }
            hallSteps = Src("Pasos_Pasillo", r, new Vector3(3f, 0.1f, 0), null, 1f, false);
            hallStepPath = new[] { Group("Pasos_A", r, new Vector3(3.6f, 0.1f, 0)), Group("Pasos_B", r, new Vector3(-2.4f, 0.1f, 0)) };
            secretWall = Src("Golpe_Pared", r, new Vector3(3.5f, 1.3f, 0.7f), null, 1f, false, 1f, 1f, 10f);
        }

        // ================================================================= sala
        void Living()
        {
            var r = Group("Sala", root);
            // sofá (respaldo hacia el este: esconderse agachado al oeste)
            var sofa = Group("Sofa", r);
            BoxMM("Asiento", sofa, new Vector3(-2.72f, 0.0f, -3.35f), new Vector3(-1.87f, 0.45f, -1.45f), mSofa, 0.8f);
            BoxMM("Respaldo", sofa, new Vector3(-2.05f, 0.45f, -3.35f), new Vector3(-1.87f, 1.0f, -1.45f), mSofa, 0.8f);
            BoxMM("Brazo_S", sofa, new Vector3(-2.72f, 0.45f, -3.35f), new Vector3(-1.87f, 0.68f, -3.17f), mSofa, 0.8f);
            BoxMM("Brazo_N", sofa, new Vector3(-2.72f, 0.45f, -1.63f), new Vector3(-1.87f, 0.68f, -1.45f), mSofa, 0.8f);
            Box("Cojin", sofa, new Vector3(-2.3f, 0.52f, -2.0f), new Vector3(0.4f, 0.12f, 0.45f), mBlanket, 1f, false);
            Box("Alfombra", r, new Vector3(-2.9f, 0.006f, -2.4f), new Vector3(1.6f, 0.012f, 2.2f), mCarpet, 1.6f, false);
            // TV vieja
            var tv = Group("TV", r);
            BoxMM("Mueble", tv, new Vector3(-3.94f, 0, -4.5f), new Vector3(-3.4f, 0.5f, -3.6f), mWoodDark, 1f);
            Box("Televisor", tv, new Vector3(-3.66f, 0.75f, -4.05f), new Vector3(0.45f, 0.45f, 0.55f), mDarkPlastic, 1f);
            Quad("Pantalla", tv, new Vector3(-3.43f, 0.76f, -4.05f), new Vector2(0.42f, 0.33f), -90f, mMonitorOff);
            // cómoda con cajón del plano
            var com = Group("Comoda", r);
            BoxMM("Cuerpo", com, new Vector3(-3.92f, 0, -1.08f), new Vector3(-2.9f, 0.85f, -0.66f), mWoodDark, 1f);
            for (int i = 0; i < 3; i++)
            {
                var slide = Group("Cajon_" + i, com, new Vector3(-3.41f, 0.68f - i * 0.25f, -1.09f));
                Box("Frente", slide, Vector3.zero, new Vector3(0.95f, 0.2f, 0.02f), mWoodLight, 1f);
                Box("Tirador", slide, new Vector3(0, 0, -0.02f), new Vector3(0.12f, 0.02f, 0.02f), mMetal, 1f, false);
                var dr = slide.gameObject.AddComponent<Drawer>();
                dr.id = "comoda" + i; dr.slide = slide; dr.openOffset = new Vector3(0, 0, -0.28f);
                if (i == 1)
                {
                    planDrawer = dr;
                    docPlan = new DocumentData
                    {
                        id = "plan", titleEs = "Plano original de la casa (2014)", titleEn = "Original house floor plan (2014)",
                        bodyEs = "Seis espacios numerados: 01 dormitorio, 02 baño, 03 pasillo, 04 sala, 05 cocina... y un sexto recuadro, \"06\", dibujado detrás del armario del pasillo.\n\nNota de papá al margen: \"El 06 tiene circuito propio. El mecanismo del armario necesita 2 unidades.\"",
                        bodyEn = "Six numbered spaces: 01 bedroom, 02 bathroom, 03 hallway, 04 living room, 05 kitchen... and a sixth box, \"06\", drawn behind the hallway wardrobe.\n\nDad's note in the margin: \"06 has its own circuit. The wardrobe mechanism needs 2 units.\"",
                        image = Tex("blueprint")
                    };
                    dr.content = docPlan;
                }
            }
            // fotos familiares (pista falsa: 14/05)
            var ph = Group("Foto_Familiar", r);
            Box("Marco", ph, new Vector3(-3.6f, 0.95f, -0.8f), new Vector3(0.22f, 0.18f, 0.02f), mWoodDark, 1f);
            Quad("Foto", ph, new Vector3(-3.6f, 0.95f, -0.812f), new Vector2(0.18f, 0.14f), 0f, mPhoto);
            docPhoto = new DocumentData
            {
                id = "photo", titleEs = "Fotografía familiar", titleEn = "Family photo",
                bodyEs = "Papá, mamá y tú, de niño. Papá sostiene una placa de circuitos.\n\nDetrás, escrito a mano: \"Cumpleaños de Ana — 14/05\".",
                bodyEn = "Dad, Mom and you as a kid. Dad is holding a circuit board.\n\nOn the back, handwritten: \"Ana's birthday — 05/14\".",
                image = Tex("photo_family")
            };
            var pdi = ph.gameObject.AddComponent<DocumentItem>(); pdi.doc = docPhoto; pdi.verbEs = "Examinar fotografía"; pdi.verbEn = "Examine photo";
            Quad("Cuadro_Pared", r, new Vector3(-2.3f, 1.65f, -0.665f), new Vector2(0.5f, 0.4f), 0f, mPhoto);
            // escritorio de vigilancia (pared este, mira al oeste)
            var sec = Group("Escritorio_Vigilancia", r);
            BoxMM("Tablero", sec, new Vector3(0.34f, 0.72f, -4.0f), new Vector3(0.94f, 0.76f, -2.8f), mWoodLight, 1f);
            BoxMM("Pata_A", sec, new Vector3(0.34f, 0, -4.0f), new Vector3(0.94f, 0.72f, -3.96f), mWoodLight, 1f);
            BoxMM("Pata_B", sec, new Vector3(0.34f, 0, -2.84f), new Vector3(0.94f, 0.72f, -2.8f), mWoodLight, 1f);
            var mon = Group("Monitor_Vigilancia", sec);
            Box("CRT", mon, new Vector3(0.68f, 0.98f, -3.4f), new Vector3(0.4f, 0.4f, 0.44f), mBeige, 1f);
            var ms = Quad("Pantalla", mon, new Vector3(0.475f, 0.99f, -3.4f), new Vector2(0.34f, 0.27f), 90f, mStatic);
            salaMonitor = mon.gameObject.AddComponent<MonitorScreen>();
            salaMonitor.screen = ms.GetComponent<Renderer>(); salaMonitor.offMat = mStatic; salaMonitor.on = true; salaMonitor.requireZone = "sala";
            Box("VCR", mon, new Vector3(0.62f, 0.8f, -3.4f), new Vector3(0.3f, 0.06f, 0.4f), mDarkPlastic, 1f, false);
            monitorStation = mon.gameObject.AddComponent<PanelStation>();
            monitorStation.kind = PanelStation.Kind.Cameras; monitorStation.promptEs = "Revisar cámaras"; monitorStation.promptEn = "Check cameras";
            BoxMM("Silla", r, new Vector3(-0.2f, 0.42f, -3.6f), new Vector3(0.2f, 0.47f, -3.2f), mWoodDark, 1f, false);
            // estantería
            var bs = Group("Estanteria", r);
            BoxMM("Cuerpo", bs, new Vector3(-1.1f, 0, -5.04f), new Vector3(0.1f, 1.8f, -4.74f), mWoodDark, 1f);
            Books(bs, new Vector3(-1.05f, 1.22f, -4.92f), 1.1f, false);
            Books(bs, new Vector3(-1.05f, 0.62f, -4.92f), 1.1f, false);
            // reloj de pared + tic tac
            Prim(PrimitiveType.Cylinder, "Reloj_Pared", r, new Vector3(-0.2f, 1.9f, -0.67f), new Vector3(0.3f, 0.015f, 0.3f), mPaint, false, new Vector3(90, 0, 0));
            Src("TicTac", r, new Vector3(-0.2f, 1.9f, -0.7f), Clip("clock_tick"), 0.35f, true, 1f, 0.5f, 5f);
            // lámpara de pie
            Prim(PrimitiveType.Cylinder, "Lampara_Pie", r, new Vector3(-3.7f, 0.75f, -1.5f), new Vector3(0.04f, 0.75f, 0.04f), mMetal, true);
            var fl = Prim(PrimitiveType.Cylinder, "Pantalla", r, new Vector3(-3.7f, 1.55f, -1.5f), new Vector3(0.35f, 0.12f, 0.35f), mShadeOn, false);
            NoShadow(fl);
            var fll = MakeLight("Luz_LamparaPie", r, LightType.Point, new Vector3(-3.7f, 1.45f, -1.5f), new Color(1f, 0.75f, 0.5f), 0.9f, 4f, LightShadows.None);
            var frl = fll.gameObject.AddComponent<RoomLight>(); frl.zone = "sala"; frl.lightSource = fll; frl.bulb = fl.GetComponent<Renderer>(); frl.onMat = mShadeOn; frl.offMat = mShadeOff;
            roomLights.Add(frl);
            CeilingLight("sala", r, new Vector3(-1.5f, H - 0.08f, -2.85f), new Color(1f, 0.85f, 0.65f), 1.0f, 6f);
            Switch("sala", r, new Vector3(-0.2f, 1.25f, -0.675f), 0f);
            // perchero, alfombrilla de la puerta
            Box("Felpudo", r, new Vector3(-2.15f, 0.006f, -4.7f), new Vector3(0.9f, 0.012f, 0.5f), mCarpet, 1f, false);
            Prim(PrimitiveType.Cylinder, "Perchero", r, new Vector3(-3.2f, 0.9f, -4.85f), new Vector3(0.05f, 0.9f, 0.05f), mWoodDark, true);
            Box("Abrigo", r, new Vector3(-3.2f, 1.45f, -4.82f), new Vector3(0.35f, 0.6f, 0.12f), mJeans, 1f, false);
            frontDoorOutside = Src("Exterior_Puerta", r, new Vector3(-2.15f, 1.3f, -5.7f), null, 1f, false, 1f, 1.5f, 14f);
        }

        // ================================================================= cocina
        void Kitchen()
        {
            var r = Group("Cocina", root);
            // encimera este con cajones
            BoxMM("Encimera", r, new Vector3(3.98f, 0.86f, -3.3f), new Vector3(4.54f, 0.92f, -0.9f), mCounter, 1f);
            BoxMM("Mueble_Bajo", r, new Vector3(4.02f, 0, -3.3f), new Vector3(4.54f, 0.86f, -0.9f), mWoodLight, 1f);
            for (int i = 0; i < 3; i++)
            {
                float z = -1.25f - i * 0.7f;
                var slide = Group("Cajon_Cocina_" + i, r, new Vector3(4.0f, 0.74f, z));
                Box("Frente", slide, Vector3.zero, new Vector3(0.02f, 0.16f, 0.6f), mWoodDark, 1f);
                Box("Tirador", slide, new Vector3(-0.02f, 0, 0), new Vector3(0.02f, 0.02f, 0.14f), mMetal, 1f, false);
                var dr = slide.gameObject.AddComponent<Drawer>();
                dr.id = "cocina" + i; dr.slide = slide; dr.openOffset = new Vector3(-0.3f, 0, 0);
                if (i == 1)
                {
                    receiptDrawer = dr;
                    docReceipt = new DocumentData
                    {
                        id = "receipt", titleEs = "Recibo de cerrajería", titleEn = "Locksmith receipt",
                        bodyEs = "CERRAJERÍA SAN LUIS\nCambio de cerradura — puerta principal\nFecha del trabajo: 23 de agosto\nTotal: Bs 180\n\nAl pie, con la letra de mamá: \"código nuevo = día del cambio\".",
                        bodyEn = "SAN LUIS LOCKSMITHS\nLock replacement — front door\nDate of work: August 23\nTotal: Bs 180\n\nAt the bottom, in Mom's handwriting: \"new code = day of the change\".",
                        image = Tex("receipt")
                    };
                    dr.content = docReceipt;
                    dr.clueId = "pin"; dr.clueEs = "Recibo del cerrajero: cerradura cambiada el 23 de agosto → PIN 2308 (DDMM)."; dr.clueEn = "Locksmith receipt: lock changed on August 23 → PIN 2308 (DDMM).";
                }
            }
            BoxMM("Alacena", r, new Vector3(4.2f, 1.5f, -3.3f), new Vector3(4.54f, 2.2f, -0.9f), mWoodLight, 1f);
            // heladera con zumbido
            BoxMM("Heladera", r, new Vector3(3.9f, 0, -4.25f), new Vector3(4.54f, 1.8f, -3.4f), mFridge, 1f);
            Src("Zumbido_Heladera", r, new Vector3(4.2f, 1.0f, -3.8f), Clip("electric_hum"), 0.25f, true, 1f, 0.5f, 5f);
            // fregadero bajo la ventana
            BoxMM("Mesada_Sur", r, new Vector3(2.4f, 0, -5.04f), new Vector3(3.9f, 0.9f, -4.5f), mWoodLight, 1f);
            BoxMM("Mesada_Sur_Top", r, new Vector3(2.4f, 0.9f, -5.04f), new Vector3(3.9f, 0.95f, -4.5f), mCounter, 1f);
            Box("Pileta", r, new Vector3(3.2f, 0.955f, -4.77f), new Vector3(0.5f, 0.01f, 0.35f), mMetal, 1f, false);
            // mesa y sillas
            BoxMM("Mesa", r, new Vector3(1.9f, 0.72f, -3.0f), new Vector3(2.9f, 0.77f, -2.2f), mWoodLight, 1f);
            foreach (var p in new[] { new Vector2(1.95f, -2.95f), new Vector2(2.85f, -2.95f), new Vector2(1.95f, -2.25f), new Vector2(2.85f, -2.25f) })
                Box("Pata", r, new Vector3(p.x, 0.36f, p.y), new Vector3(0.05f, 0.72f, 0.05f), mWoodLight, 1f);
            BoxMM("Silla_A", r, new Vector3(2.2f, 0.42f, -3.5f), new Vector3(2.6f, 0.47f, -3.1f), mWoodDark, 1f, false);
            Box("Plato", r, new Vector3(2.3f, 0.78f, -2.6f), new Vector3(0.22f, 0.01f, 0.22f), mPaint, 1f, false);
            Box("Taza", r, new Vector3(2.6f, 0.82f, -2.5f), new Vector3(0.07f, 0.09f, 0.07f), mRed, 1f, false);
            // cuadro eléctrico (pared norte, mira al sur)
            var pp = Group("Cuadro_Electrico", r, new Vector3(3.55f, 1.5f, -0.69f), 0f);
            Box("Caja", pp, Vector3.zero, new Vector3(0.5f, 0.65f, 0.1f), mMetal, 1f);
            for (int i = 0; i < 5; i++) Box("Llave", pp, new Vector3(-0.18f + i * 0.09f, 0.05f, -0.055f), new Vector3(0.04f, 0.1f, 0.02f), i == 4 ? mRed : mDarkPlastic, 1f, false);
            Text3D("Etiqueta", pp, new Vector3(0, 0.25f, -0.056f), 0f, "A  C  L  I  06", 0.0026f, Color.white);
            powerStation = pp.gameObject.AddComponent<PanelStation>();
            powerStation.kind = PanelStation.Kind.Power; powerStation.promptEs = "Abrir cuadro eléctrico"; powerStation.promptEn = "Open breaker panel";
            // linterna
            var fl = Group("Linterna", r, new Vector3(4.25f, 0.95f, -1.0f));
            Prim(PrimitiveType.Cylinder, "Cuerpo", fl, Vector3.zero, new Vector3(0.05f, 0.09f, 0.05f), mDarkPlastic, true, new Vector3(0, 0, 90));
            flashlightPickup = fl.gameObject.AddComponent<FlashlightPickup>();
            // despensa (escondite)
            var pantry = Group("Despensa", r);
            HidingCloset(pantry, new Vector3(1.06f, 0, -5.04f), new Vector3(1.66f, 2.0f, -4.4f), false, "despensa", "pantry",
                new Vector3(1.36f, 0, -4.75f), 0f, new Vector3(1.36f, 0, -3.9f), 0f, true);
            CeilingLight("cocina", r, new Vector3(2.8f, H - 0.08f, -2.8f), new Color(0.95f, 0.95f, 0.85f), 1.0f, 6f);
            Switch("cocina", r, new Vector3(1.45f, 1.25f, -0.675f), 0f);
            kitchenVoice = Src("Voz_Cocina", r, new Vector3(3.0f, 1.4f, -3.0f), null, 1f, false);
        }

        // ================================================================= baño
        void Bathroom()
        {
            var r = Group("Bano", root);
            BoxMM("Inodoro_Base", r, new Vector3(0.1f, 0, 2.75f), new Vector3(0.55f, 0.42f, 3.15f), mPaint, 1f);
            BoxMM("Inodoro_Tanque", r, new Vector3(0.06f, 0.42f, 2.8f), new Vector3(0.25f, 0.8f, 3.1f), mPaint, 1f);
            BoxMM("Lavabo", r, new Vector3(0.06f, 0.78f, 1.45f), new Vector3(0.5f, 0.88f, 1.95f), mPaint, 1f);
            BoxMM("Pie_Lavabo", r, new Vector3(0.15f, 0, 1.6f), new Vector3(0.3f, 0.78f, 1.8f), mPaint, 1f);
            Quad("Espejo", r, new Vector3(0.065f, 1.5f, 1.7f), new Vector2(0.5f, 0.6f), -90f, mMirror);
            // ducha
            BoxMM("Plato_Ducha", r, new Vector3(1.6f, 0, 2.7f), new Vector3(2.44f, 0.06f, 3.54f), mPaint, 1f);
            var cur = Group("Cortina_Ducha", r, new Vector3(2.0f, 2.1f, 2.7f));
            Box("Tela", cur, new Vector3(0, -0.9f, 0), new Vector3(0.84f, 1.8f, 0.02f), mCurtain, 1f, false);
            Prim(PrimitiveType.Cylinder, "Barra", r, new Vector3(2.0f, 2.12f, 2.7f), new Vector3(0.02f, 0.42f, 0.02f), mMetal, false, new Vector3(0, 0, 90));
            var bl = CeilingLight("bano", r, new Vector3(1.25f, H - 0.08f, 2.1f), new Color(0.75f, 0.88f, 1f), 0.9f, 4.5f);
            bl.lightSource.gameObject.AddComponent<LightFlicker>().amount = 0.25f;
            Switch("bano", r, new Vector3(0.3f, 1.25f, 0.665f), 180f);
            bathroomInside = Src("Interior_Bano", r, new Vector3(1.2f, 0.2f, 2.2f), null, 1f, false);
        }

        // ================================================================= habitación secreta (06)
        void Secret()
        {
            var r = Group("Habitacion_06", root);
            BoxMM("Escritorio", r, new Vector3(2.62f, 0.72f, 2.95f), new Vector3(4.48f, 0.77f, 3.54f), mWoodDark, 1f);
            BoxMM("Pata_A", r, new Vector3(2.62f, 0, 2.95f), new Vector3(2.68f, 0.72f, 3.54f), mWoodDark, 1f);
            BoxMM("Pata_B", r, new Vector3(4.42f, 0, 2.95f), new Vector3(4.48f, 0.72f, 3.54f), mWoodDark, 1f);
            BoxMM("Estante", r, new Vector3(2.62f, 1.22f, 3.15f), new Vector3(4.48f, 1.26f, 3.54f), mWoodDark, 1f);
            // 4 monitores: [0] sala, [1] cocina (arriba) / [2] pasillo, [3] dormitorio (abajo)
            Vector3[] pos = { new Vector3(3.1f, 1.5f, 3.33f), new Vector3(3.95f, 1.5f, 3.33f), new Vector3(3.1f, 0.98f, 3.25f), new Vector3(3.95f, 0.98f, 3.25f) };
            for (int i = 0; i < 4; i++)
            {
                var m = Group("CRT_" + i, r);
                Box("Caja", m, pos[i] + new Vector3(0, 0, 0.05f), new Vector3(0.48f, 0.42f, 0.4f), mBeige, 1f);
                var q = Quad("Pantalla", m, pos[i] + new Vector3(0, 0.01f, -0.152f), new Vector2(0.38f, 0.3f), 0f, mMonitorOff);
                var ms = m.gameObject.AddComponent<MonitorScreen>();
                ms.screen = q.GetComponent<Renderer>(); ms.offMat = mMonitorOff; ms.requireZone = "secreta";
                secretMonitors[i] = ms;
            }
            // monitores decorativos y cables
            Box("CRT_Suelo", r, new Vector3(2.85f, 0.2f, 2.6f), new Vector3(0.42f, 0.4f, 0.4f), mBeige, 1f);
            Quad("Estatica", r, new Vector3(2.85f, 0.21f, 2.39f), new Vector2(0.32f, 0.26f), 0f, mStatic);
            Box("CRT_Suelo2", r, new Vector3(4.2f, 0.2f, 2.4f), new Vector3(0.42f, 0.4f, 0.4f), mBeige, 1f);
            for (int i = 0; i < 6; i++) Box("Cable", r, new Vector3(2.8f + i * 0.28f, 0.01f, 2.0f + (i % 3) * 0.25f), new Vector3(0.03f, 0.02f, 1.2f), mCable, 1f, false, new Vector3(0, i * 23f, 0));
            // grabadora
            var rec = Group("Grabadora", r, new Vector3(3.5f, 0.82f, 3.05f));
            Box("Cuerpo", rec, Vector3.zero, new Vector3(0.3f, 0.09f, 0.18f), mDarkPlastic, 1f);
            Prim(PrimitiveType.Cylinder, "Carrete_A", rec, new Vector3(-0.07f, 0.05f, 0), new Vector3(0.08f, 0.01f, 0.08f), mBeige);
            Prim(PrimitiveType.Cylinder, "Carrete_B", rec, new Vector3(0.07f, 0.05f, 0), new Vector3(0.08f, 0.01f, 0.08f), mBeige);
            recorder = rec.gameObject.AddComponent<StoryInteractable>();
            recorder.promptEs = "Reproducir grabación"; recorder.promptEn = "Play recording"; recorder.eventId = "recorder_play"; recorder.once = true;
            // procedimiento de aislamiento (documento sobre el escritorio)
            var doc = Group("Procedimiento", r, new Vector3(4.15f, 0.775f, 3.05f));
            Box("Hoja", doc, Vector3.zero, new Vector3(0.21f, 0.005f, 0.28f), mPaint, 1f);
            docIsolation = new DocumentData
            {
                id = "isolation", titleEs = "Procedimiento de aislamiento — GUARDIÁN", titleEn = "Isolation procedure — GUARDIAN",
                bodyEs = "1. Seguridad completa en el cuadro: ALARMA + CÁMARAS + CERRADURAS.\n2. La alarma marca movimiento, pero también registra ECOS: lugares donde no hay nadie.\n3. Confirma en la cámara EN DIRECTO dónde está realmente.\n4. Aísla esa zona. Solo la cocina y el dormitorio tienen persianas.\n5. Si te equivocas, te escuchará. Escóndete.\n\nPuede imitarte, pero no puede estar en dos lugares al mismo tiempo.",
                bodyEn = "1. Full security at the panel: ALARM + CAMERAS + LOCKS.\n2. The alarm shows motion, but it also logs ECHOES: places where nobody is.\n3. Confirm on the LIVE camera where it really is.\n4. Isolate that zone. Only the kitchen and the bedroom have shutters.\n5. If you get it wrong, it will hear you. Hide.\n\nIt can imitate you, but it can't be in two places at once."
            };
            var ddi = doc.gameObject.AddComponent<DocumentItem>(); ddi.doc = docIsolation; ddi.verbEs = "Leer procedimiento"; ddi.verbEn = "Read procedure";
            // corcho con notas del padre
            var cork = Group("Corcho", r, new Vector3(2.565f, 1.5f, 2.2f), -90f);
            Box("Tablero", cork, Vector3.zero, new Vector3(1.0f, 0.7f, 0.02f), mCork, 1f);
            Quad("Foto", cork, new Vector3(-0.25f, 0.1f, -0.012f), new Vector2(0.2f, 0.15f), 0f, mPhotoFather);
            for (int i = 0; i < 4; i++) Box("Nota", cork, new Vector3(0.05f + (i % 2) * 0.22f, 0.15f - (i / 2) * 0.25f, -0.012f), new Vector3(0.16f, 0.12f, 0.003f), mPaint, 1f, false);
            docFatherNote = new DocumentData
            {
                id = "father_note", titleEs = "Notas de papá", titleEn = "Dad's notes",
                bodyEs = "Día 12 — Me llamó con la voz de Ana desde la cocina. Ana estaba conmigo en el auto.\nDía 19 — GUARDIÁN registra dos de mí. Ninguno se movía.\nDía 23 — Cambié la cerradura. No sirve. No entra desde afuera.\nDía 31 — Si alguien lee esto: no le abras a ninguna voz. Ni a la mía.",
                bodyEn = "Day 12 — It called me with Ana's voice from the kitchen. Ana was with me in the car.\nDay 19 — GUARDIAN logs two of me. Neither was moving.\nDay 23 — I changed the lock. It doesn't matter. It doesn't come in from outside.\nDay 31 — If anyone reads this: don't open the door to any voice. Not even mine.",
                image = Tex("photo_father")
            };
            var cdi = cork.gameObject.AddComponent<DocumentItem>(); cdi.doc = docFatherNote; cdi.verbEs = "Leer notas"; cdi.verbEn = "Read notes";
            // palanca de energía auxiliar (pared oeste)
            var lev = Group("Generador_Auxiliar", r, new Vector3(2.58f, 1.1f, 1.3f), -90f);
            Box("Caja", lev, Vector3.zero, new Vector3(0.35f, 0.5f, 0.12f), mMetal, 1f);
            Box("Palanca", lev, new Vector3(0, 0.05f, -0.1f), new Vector3(0.04f, 0.22f, 0.04f), mRed, 1f, false, new Vector3(30, 0, 0));
            auxLever = lev.gameObject.AddComponent<StoryInteractable>();
            auxLever.promptEs = "Activar generador auxiliar"; auxLever.promptEn = "Turn on auxiliary generator"; auxLever.eventId = "aux_on"; auxLever.once = true; auxLever.available = false;
            // panel de aislamiento (pared este)
            var iso = Group("Panel_Aislamiento", r, new Vector3(4.53f, 1.35f, 2.1f), 90f);
            Box("Caja", iso, Vector3.zero, new Vector3(0.6f, 0.5f, 0.08f), mMetal, 1f);
            Box("Pantalla", iso, new Vector3(0, 0.05f, -0.045f), new Vector3(0.45f, 0.25f, 0.01f), mDarkPlastic, 1f, false);
            Text3D("Rotulo", iso, new Vector3(0, 0.05f, -0.052f), 0f, "AISLAMIENTO\nGUARDIAN", 0.0035f, new Color(0.3f, 1f, 0.4f));
            isolationStation = iso.gameObject.AddComponent<PanelStation>();
            isolationStation.kind = PanelStation.Kind.Isolation; isolationStation.promptEs = "Usar panel de aislamiento"; isolationStation.promptEn = "Use isolation panel";
            // casillero (escondite)
            var locker = Group("Casillero", r);
            HidingCloset(locker, new Vector3(4.0f, 0, 0.7f), new Vector3(4.54f, 1.95f, 1.35f), false, "casillero", "locker",
                new Vector3(4.27f, 0, 1.02f), -90f, new Vector3(3.6f, 0, 1.02f), -90f);
            // luces de emergencia
            var e1 = MakeLight("Emergencia_06", r, LightType.Point, new Vector3(3.5f, 2.4f, 2.0f), new Color(1f, 0.08f, 0.05f), 1.6f, 6f, LightShadows.None);
            Box("Caja_Emergencia", e1.transform, new Vector3(0, 0.12f, 0), new Vector3(0.25f, 0.08f, 0.1f), mRed, 1f, false);
            e1.enabled = false; emergency.Add(e1);
            var glow = MakeLight("Brillo_Monitores", r, LightType.Point, new Vector3(3.5f, 1.2f, 2.6f), new Color(0.5f, 0.7f, 1f), 0.5f, 3f, LightShadows.None);
            glow.gameObject.AddComponent<LightFlicker>().amount = 0.3f;
        }

        // ================================================================= exterior
        void Exterior()
        {
            exterior = Group("Exterior", null);
            Box("Suelo", exterior, new Vector3(0, -0.16f, 0), new Vector3(60f, 0.1f, 60f), mGrass, 2f);
            BoxMM("Porche", exterior, new Vector3(-3.3f, -0.1f, -6.9f), new Vector3(-1.0f, 0.0f, -5.16f), mWoodDark, 1f);
            BoxMM("Camino", exterior, new Vector3(-2.6f, -0.12f, -12.5f), new Vector3(-1.7f, -0.1f, -6.9f), mConcrete, 1f);
            BoxMM("Calle", exterior, new Vector3(-30f, -0.13f, -15f), new Vector3(30f, -0.11f, -13f), mConcrete, 1.5f);
            // cerca
            float fx0 = -10f, fx1 = 10f, fz0 = -12.5f, fz1 = 10f;
            BoxMM("Cerca_N", exterior, new Vector3(fx0, 0, fz1), new Vector3(fx1, 1.2f, fz1 + 0.06f), mFence, 1f);
            BoxMM("Cerca_O", exterior, new Vector3(fx0 - 0.06f, 0, fz0), new Vector3(fx0, 1.2f, fz1), mFence, 1f);
            BoxMM("Cerca_E", exterior, new Vector3(fx1, 0, fz0), new Vector3(fx1 + 0.06f, 1.2f, fz1), mFence, 1f);
            BoxMM("Cerca_S_A", exterior, new Vector3(fx0, 0, fz0 - 0.06f), new Vector3(-2.8f, 1.2f, fz0), mFence, 1f);
            BoxMM("Cerca_S_B", exterior, new Vector3(-1.5f, 0, fz0 - 0.06f), new Vector3(fx1, 1.2f, fz0), mFence, 1f);
            // límites invisibles alrededor del porche (no se puede salir hasta el final)
            foreach (var b in new[] { new[] { -3.4f, -7.0f, -3.3f, -5.1f }, new[] { -1.0f, -7.0f, -0.9f, -5.1f }, new[] { -3.4f, -7.1f, -0.9f, -7.0f } })
            {
                var inv = new GameObject("Limite"); inv.transform.SetParent(exterior, false);
                var bc = inv.AddComponent<BoxCollider>();
                inv.transform.localPosition = new Vector3((b[0] + b[2]) * 0.5f, 1.5f, (b[1] + b[3]) * 0.5f);
                bc.size = new Vector3(b[2] - b[0], 3f, b[3] - b[1]);
            }
            // árboles
            var rnd = new System.Random(11);
            Vector2[] trees = { new Vector2(-7, -8), new Vector2(6, -9), new Vector2(-8, 4), new Vector2(7, 6), new Vector2(-6, 8), new Vector2(3, 8.5f), new Vector2(8, -3), new Vector2(-8.5f, -2) };
            foreach (var t in trees)
            {
                var tg = Group("Arbol", exterior, new Vector3(t.x, 0, t.y));
                Prim(PrimitiveType.Cylinder, "Tronco", tg, new Vector3(0, 1.2f, 0), new Vector3(0.3f, 1.2f, 0.3f), mTrunk, true);
                for (int i = 0; i < 3; i++)
                    Prim(PrimitiveType.Sphere, "Copa", tg, new Vector3((float)rnd.NextDouble() - 0.5f, 2.8f + i * 0.9f, (float)rnd.NextDouble() - 0.5f), Vector3.one * (2.4f - i * 0.6f), mLeaves);
            }
            // farola
            var lamp = Group("Farola", exterior, new Vector3(-5f, 0, -13.6f));
            Prim(PrimitiveType.Cylinder, "Poste", lamp, new Vector3(0, 2.2f, 0), new Vector3(0.12f, 2.2f, 0.12f), mMetal, true);
            Box("Cabeza", lamp, new Vector3(0, 4.45f, 0.3f), new Vector3(0.3f, 0.12f, 0.7f), mMetal, 1f, false);
            Box("Foco", lamp, new Vector3(0, 4.38f, 0.45f), new Vector3(0.2f, 0.02f, 0.3f), mBulbOn, 1f, false);
            MakeLight("Luz_Farola", lamp, LightType.Point, new Vector3(0, 4.1f, 0.45f), new Color(1f, 0.75f, 0.45f), 2.2f, 11f, LightShadows.None);
            // casas lejanas
            for (int i = 0; i < 5; i++)
            {
                var hg = Group("Casa_Vecina", exterior, new Vector3(-24f + i * 12f, 0, -24f));
                Box("Cuerpo", hg, new Vector3(0, 1.8f, 0), new Vector3(6f, 3.6f, 5f), mSiding, 2f, false);
                Box("Ventana", hg, new Vector3(1.2f, 1.8f, 2.51f), new Vector3(0.9f, 0.8f, 0.02f), i % 2 == 0 ? mBulbOn : mDarkPlastic, 1f, false);
            }
            // cielo (4 paneles)
            float D = 30f, Hs = 18f;
            var sky = Group("Cielo", exterior);
            skyRenderers.Add(Quad("Cielo_N", sky, new Vector3(0, Hs * 0.35f, D), new Vector2(2 * D, Hs), 180f, skyNight).GetComponent<Renderer>());
            skyRenderers.Add(Quad("Cielo_S", sky, new Vector3(0, Hs * 0.35f, -D), new Vector2(2 * D, Hs), 0f, skyNight).GetComponent<Renderer>());
            skyRenderers.Add(Quad("Cielo_E", sky, new Vector3(D, Hs * 0.35f, 0), new Vector2(2 * D, Hs), -90f, skyNight).GetComponent<Renderer>());
            skyRenderers.Add(Quad("Cielo_O", sky, new Vector3(-D, Hs * 0.35f, 0), new Vector2(2 * D, Hs), 90f, skyNight).GetComponent<Renderer>());
            // luna y sol
            var mg = new GameObject("Luz_Luna"); mg.transform.SetParent(exterior, false);
            mg.transform.rotation = Quaternion.Euler(32f, 160f, 0);
            moon = mg.AddComponent<Light>(); moon.type = LightType.Directional; moon.color = new Color(0.42f, 0.52f, 0.9f); moon.intensity = 0.35f; moon.shadows = LightShadows.Soft; moon.shadowStrength = 0.85f;
            lightning = mg.AddComponent<Lightning>(); lightning.flashLight = moon; lightning.flashIntensity = 3.5f;
            thunder = Src("Trueno", exterior, new Vector3(0, 10, 0), Clip("thunder"), 0.8f, false, 0f);
            lightning.thunder = thunder;
            var sg = new GameObject("Luz_Amanecer"); sg.transform.SetParent(exterior, false);
            sg.transform.rotation = Quaternion.Euler(12f, 20f, 0);
            sun = sg.AddComponent<Light>(); sun.type = LightType.Directional; sun.color = new Color(1f, 0.72f, 0.5f); sun.intensity = 1.1f; sun.shadows = LightShadows.Soft; sun.enabled = false;
            // sombra que pasa por la ventana de la sala
            shadowFigure = Group("Sombra_Ventana", exterior, new Vector3(-4.9f, 0, -4.5f));
            Prim(PrimitiveType.Capsule, "Cuerpo", shadowFigure, new Vector3(0, 1.1f, 0), new Vector3(0.45f, 1.1f, 0.35f), mEntity);
            Prim(PrimitiveType.Sphere, "Cabeza", shadowFigure, new Vector3(0, 2.25f, 0), Vector3.one * 0.32f, mEntity);
            shadowStart = Group("Sombra_Inicio", exterior, new Vector3(-4.9f, 0, -4.6f));
            shadowEnd = Group("Sombra_Fin", exterior, new Vector3(-4.9f, 0, -1.0f));
            shadowFigure.gameObject.SetActive(false);
        }

        // ================================================================= zonas
        void Zones()
        {
            var zr = Group("Zonas", root);
            Zone(zr, "dormitorio", "Dormitorio", "Bedroom", -4, 0, 0.6f, 5.1f);
            Zone(zr, "bano", "Baño", "Bathroom", 0, 2.5f, 0.6f, 3.6f);
            Zone(zr, "secreta", "Habitación 06", "Room 06", 2.5f, 4.6f, 0.6f, 3.6f);
            Zone(zr, "pasillo", "Pasillo", "Hallway", -4, 4.6f, -0.6f, 0.6f);
            Zone(zr, "sala", "Sala", "Living room", -4, 1.0f, -5.1f, -0.6f);
            Zone(zr, "cocina", "Cocina", "Kitchen", 1.0f, 4.6f, -5.1f, -0.6f);
        }

        void Zone(Transform p, string id, string es, string en, float x0, float x1, float z0, float z1)
        {
            var g = Group("Zona_" + id, p, new Vector3((x0 + x1) * 0.5f, 0, (z0 + z1) * 0.5f));
            var z = g.gameObject.AddComponent<RoomZone>();
            z.id = id; z.nameEs = es; z.nameEn = en; z.size = new Vector3(x1 - x0, 3f, z1 - z0);
            zones.Add(z);
        }

        // ================================================================= cámaras de vigilancia
        void Cameras()
        {
            var cr = Group("Camaras_Vigilancia", root);
            cams[0] = SecCam(cr, "CAM 01", "sala", new Vector3(0.75f, 2.4f, -0.85f), new Vector3(-2.2f, 0.6f, -3.2f));
            cams[1] = SecCam(cr, "CAM 02", "pasillo", new Vector3(4.35f, 2.35f, -0.4f), new Vector3(-3f, 1.0f, 0.15f));
            cams[2] = SecCam(cr, "CAM 03", "cocina", new Vector3(4.4f, 2.4f, -0.85f), new Vector3(2.2f, 0.6f, -3.6f));
            cams[3] = SecCam(cr, "CAM 04", "dormitorio", new Vector3(-0.3f, 2.4f, 0.85f), new Vector3(-3.3f, 0.6f, 3.7f));
        }

        SecurityCam SecCam(Transform p, string name, string zone, Vector3 pos, Vector3 lookAt)
        {
            var g = Group(name, p, pos);
            g.LookAt(lookAt);
            Box("Carcasa", g, new Vector3(0, 0, -0.08f), new Vector3(0.1f, 0.08f, 0.18f), mPaint, 1f, false);
            Box("Lente", g, new Vector3(0, 0, 0.02f), new Vector3(0.05f, 0.05f, 0.03f), mDarkPlastic, 1f, false);
            var led = Prim(PrimitiveType.Sphere, "LED", g, new Vector3(0.035f, 0.03f, 0.01f), Vector3.one * 0.015f, UnlitColor("M_LedRojo", new Color(1f, 0.1f, 0.05f)));
            NoShadow(led);
            var cg = Group("Camera", g, new Vector3(0, 0, 0.05f));
            var cam = cg.gameObject.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.02f, 0.02f, 0.03f);
            cam.nearClipPlane = 0.05f; cam.farClipPlane = 16f; cam.fieldOfView = 78f; cam.depth = -10; cam.enabled = false;
            var sc = g.gameObject.AddComponent<SecurityCam>();
            sc.camName = name + " — " + zone.ToUpperInvariant(); sc.zoneId = zone; sc.cam = cam;
            return sc;
        }

        // ================================================================= figura que solo aparece en cámara
        void Ghost()
        {
            ghost = Group("Figura_Camara", root, new Vector3(-2f, 0, 0), 90f);
            ghostRenderers.Add(Prim(PrimitiveType.Capsule, "Cuerpo", ghost, new Vector3(0, 1.05f, 0), new Vector3(0.38f, 1.05f, 0.28f), mEntity).GetComponent<Renderer>());
            ghostRenderers.Add(Prim(PrimitiveType.Sphere, "Cabeza", ghost, new Vector3(0, 2.1f, 0), new Vector3(0.24f, 0.3f, 0.24f), mEntity).GetComponent<Renderer>());
            ghostRenderers.Add(Prim(PrimitiveType.Capsule, "Brazo_I", ghost, new Vector3(-0.25f, 0.95f, 0), new Vector3(0.09f, 0.65f, 0.09f), mEntity).GetComponent<Renderer>());
            ghostRenderers.Add(Prim(PrimitiveType.Capsule, "Brazo_D", ghost, new Vector3(0.25f, 0.95f, 0), new Vector3(0.09f, 0.65f, 0.09f), mEntity).GetComponent<Renderer>());
            ghostRenderers.Add(Prim(PrimitiveType.Sphere, "Ojo_I", ghost, new Vector3(-0.05f, 2.12f, 0.11f), Vector3.one * 0.03f, mEyes).GetComponent<Renderer>());
            ghostRenderers.Add(Prim(PrimitiveType.Sphere, "Ojo_D", ghost, new Vector3(0.05f, 2.12f, 0.11f), Vector3.one * 0.03f, mEyes).GetComponent<Renderer>());
            foreach (var r in ghostRenderers) r.shadowCastingMode = ShadowCastingMode.Off;
            ghostFar = Group("Figura_Lejos", root, new Vector3(-2.2f, 0, 0.05f), 90f);
            ghostNear = Group("Figura_Cerca", root, new Vector3(2.0f, 0, -0.05f), 90f);
        }

        // ================================================================= el doble en la cama
        void DoppelInBed()
        {
            // cadera sobre el colchón; acostado = torso rotado 90° hacia la almohada (+Z)
            doppelInBed = Group("Doble_En_Cama", root, new Vector3(-3.48f, 0.6f, 3.95f));
            Box("Pierna_I", doppelInBed, new Vector3(-0.1f, 0.05f, -0.45f), new Vector3(0.14f, 0.14f, 0.85f), mJeans, 1f, false);
            Box("Pierna_D", doppelInBed, new Vector3(0.1f, 0.05f, -0.45f), new Vector3(0.14f, 0.14f, 0.85f), mJeans, 1f, false);
            doppelTorso = Group("Torso", doppelInBed, Vector3.zero);
            doppelTorso.localRotation = Quaternion.Euler(90f, 0, 0);
            Box("Pecho", doppelTorso, new Vector3(0, 0.33f, 0), new Vector3(0.4f, 0.6f, 0.22f), mHoodie, 1f, false);
            Box("Brazo_I", doppelTorso, new Vector3(-0.25f, 0.3f, 0), new Vector3(0.1f, 0.55f, 0.12f), mHoodie, 1f, false);
            Box("Brazo_D", doppelTorso, new Vector3(0.25f, 0.3f, 0), new Vector3(0.1f, 0.55f, 0.12f), mHoodie, 1f, false);
            doppelHead = Group("Cabeza", doppelTorso, new Vector3(0, 0.75f, 0));
            Box("Cara", doppelHead, Vector3.zero, new Vector3(0.2f, 0.25f, 0.22f), mSkin, 1f, false);
            Box("Pelo", doppelHead, new Vector3(0, 0.1f, -0.02f), new Vector3(0.22f, 0.08f, 0.24f), mHair, 1f, false);
            Box("Ojo_I", doppelHead, new Vector3(-0.05f, 0.02f, 0.111f), new Vector3(0.03f, 0.02f, 0.005f), mHair, 1f, false);
            Box("Ojo_D", doppelHead, new Vector3(0.05f, 0.02f, 0.111f), new Vector3(0.03f, 0.02f, 0.005f), mHair, 1f, false);
        }

        // ================================================================= entidad (IA)
        public EnemyAI BuildEnemy()
        {
            var e = new GameObject("Entidad");
            var cc = e.AddComponent<CharacterController>();
            cc.height = 1.9f; cc.radius = 0.25f; cc.center = new Vector3(0, 0.95f, 0); cc.stepOffset = 0.25f;
            var ai = e.AddComponent<EnemyAI>();
            var head = Group("Cabeza_Ojos", e.transform, new Vector3(0, 1.75f, 0.05f));
            ai.head = head;
            // silueta alta y delgada
            var sil = Group("Silueta", e.transform);
            var body = Group("Cuerpo", sil); body.localRotation = Quaternion.Euler(8f, 0, 0);
            Prim(PrimitiveType.Capsule, "Torso", body, new Vector3(0, 1.25f, 0), new Vector3(0.36f, 0.62f, 0.26f), mEntity);
            Prim(PrimitiveType.Capsule, "Pierna_I", body, new Vector3(-0.09f, 0.45f, 0), new Vector3(0.11f, 0.48f, 0.11f), mEntity);
            Prim(PrimitiveType.Capsule, "Pierna_D", body, new Vector3(0.09f, 0.45f, 0), new Vector3(0.11f, 0.48f, 0.11f), mEntity);
            Prim(PrimitiveType.Capsule, "Brazo_I", body, new Vector3(-0.24f, 1.0f, 0.02f), new Vector3(0.08f, 0.65f, 0.08f), mEntity, false, new Vector3(0, 0, -6f));
            Prim(PrimitiveType.Capsule, "Brazo_D", body, new Vector3(0.24f, 1.0f, 0.02f), new Vector3(0.08f, 0.65f, 0.08f), mEntity, false, new Vector3(0, 0, 6f));
            var hd = Group("Cabeza", body, new Vector3(0, 1.98f, 0.05f)); hd.localRotation = Quaternion.Euler(0, 0, 18f);
            Prim(PrimitiveType.Sphere, "Craneo", hd, Vector3.zero, new Vector3(0.22f, 0.27f, 0.22f), mEntity);
            Prim(PrimitiveType.Sphere, "Ojo_I", hd, new Vector3(-0.045f, 0.02f, 0.1f), Vector3.one * 0.025f, mEyes);
            Prim(PrimitiveType.Sphere, "Ojo_D", hd, new Vector3(0.045f, 0.02f, 0.1f), Vector3.one * 0.025f, mEyes);
            // el doble (misma ropa que el protagonista)
            var dop = Group("Doble", e.transform);
            Box("Pierna_I", dop, new Vector3(-0.1f, 0.45f, 0), new Vector3(0.14f, 0.9f, 0.16f), mJeans, 1f, false);
            Box("Pierna_D", dop, new Vector3(0.1f, 0.45f, 0), new Vector3(0.14f, 0.9f, 0.16f), mJeans, 1f, false);
            Box("Pecho", dop, new Vector3(0, 1.22f, 0), new Vector3(0.42f, 0.62f, 0.24f), mHoodie, 1f, false);
            Box("Brazo_I", dop, new Vector3(-0.27f, 1.2f, 0), new Vector3(0.1f, 0.6f, 0.12f), mHoodie, 1f, false);
            Box("Brazo_D", dop, new Vector3(0.27f, 1.2f, 0), new Vector3(0.1f, 0.6f, 0.12f), mHoodie, 1f, false);
            var dh = Group("Cabeza", dop, new Vector3(0, 1.67f, 0)); dh.localRotation = Quaternion.Euler(10f, 0, -8f);
            Box("Cara", dh, Vector3.zero, new Vector3(0.2f, 0.25f, 0.22f), mSkin, 1f, false);
            Box("Pelo", dh, new Vector3(0, 0.1f, -0.02f), new Vector3(0.22f, 0.08f, 0.24f), mHair, 1f, false);
            Box("Ojo_I", dh, new Vector3(-0.05f, 0.02f, 0.111f), new Vector3(0.03f, 0.02f, 0.005f), mEyes, 1f, false);
            Box("Ojo_D", dh, new Vector3(0.05f, 0.02f, 0.111f), new Vector3(0.03f, 0.02f, 0.005f), mEyes, 1f, false);
            dop.gameObject.SetActive(false);
            ai.silhouetteVisual = sil.gameObject; ai.doppelVisual = dop.gameObject;
            ai.breath = Src("Respiracion", e.transform, new Vector3(0, 1.7f, 0), Clip("breath"), 0.6f, true, 1f, 1f, 9f);
            ai.breath.playOnAwake = false;
            e.SetActive(false);
            return ai;
        }

        // ================================================================= grafo de navegación
        public WaypointGraph BuildGraph(out int nodeBathroom, out int nodeBedroom, out int[] routeAct2, out int[] routeFinal)
        {
            var g = new GameObject("Grafo_Navegacion");
            Vector3[] p =
            {
                new Vector3(-3.4f,0,0), new Vector3(-2.6f,0,0), new Vector3(-1.0f,0,0), new Vector3(1.0f,0,0), new Vector3(2.05f,0,0), new Vector3(3.3f,0,-0.25f), // 0-5 pasillo
                new Vector3(-2.6f,0,1.3f), new Vector3(-1.6f,0,2.6f), new Vector3(-2.6f,0,3.6f),                                                              // 6-8 dormitorio
                new Vector3(-1.0f,0,-1.3f), new Vector3(-0.9f,0,-3.0f), new Vector3(-1.4f,0,-4.4f), new Vector3(0.4f,0,-2.05f),                               // 9-12 sala
                new Vector3(1.6f,0,-2.05f), new Vector3(2.05f,0,-1.3f), new Vector3(3.4f,0,-2.0f), new Vector3(3.2f,0,-4.0f),                                // 13-16 cocina
                new Vector3(1.0f,0,1.3f), new Vector3(1.1f,0,2.3f),                                                                                            // 17-18 baño
                new Vector3(3.5f,0,1.25f)                                                                                                                       // 19 secreta
            };
            var nodes = new Transform[p.Length];
            for (int i = 0; i < p.Length; i++) nodes[i] = Group("N" + i, g.transform, p[i]);
            int[,] e =
            {
                {0,1},{1,2},{2,3},{3,4},{4,5},
                {1,6},{6,7},{7,8},{6,8},
                {2,9},{9,10},{10,11},{10,12},{9,12},{12,13},
                {13,15},{13,14},{4,14},{14,15},{15,16},{13,16},
                {3,17},{17,18},
                {5,19}
            };
            int n = e.GetLength(0);
            var wg = g.AddComponent<WaypointGraph>();
            wg.nodes = nodes; wg.edgeA = new int[n]; wg.edgeB = new int[n]; wg.edgeDoor = new DoorController[n]; wg.edgeShutter = new Shutter[n];
            for (int i = 0; i < n; i++)
            {
                int a = e[i, 0], b = e[i, 1];
                wg.edgeA[i] = a; wg.edgeB[i] = b;
                if ((a == 1 && b == 6)) { wg.edgeDoor[i] = doorBedroom; wg.edgeShutter[i] = shuttersBedroom[0]; }
                if ((a == 4 && b == 14)) { wg.edgeDoor[i] = doorKitchen; wg.edgeShutter[i] = shuttersKitchen[0]; }
                if ((a == 12 && b == 13)) { wg.edgeShutter[i] = shuttersKitchen[1]; }
                if ((a == 3 && b == 17)) wg.edgeDoor[i] = doorBathroom;
                if ((a == 5 && b == 19)) wg.edgeDoor[i] = doorMetal;
            }
            nodeBathroom = 18; nodeBedroom = 8;
            routeAct2 = new[] { 3, 2, 9, 10, 11, 12, 13, 15, 16, 14, 4, 3, 1, 0, 6, 7, 8, 1, 2, 10 };
            routeFinal = new[] { 8, 7, 10, 11, 15, 16, 13, 7 };
            return wg;
        }
    }
}
