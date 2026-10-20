using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GNight.EditorTools
{
    /// <summary>Modelos FBX hechos en Blender (Tools/Blender): carga, materiales GN_*, colocación por límites y colisiones.</summary>
    public static partial class BuildKit
    {
        public const string ModelDir = Root + "/Art/Models/";
        /// <summary>
        /// Corrección global de orientación de los FBX. Los modelos de Blender tienen el frente en -Y y, con la conversión de
        /// ejes del exportador + importador, llegan a Unity con el frente en -Z. El constructor coloca todo suponiendo frente en
        /// +Z (FrontDir = forward), así que se giran 180°. Comprobado con la auditoría de modelos (respaldos, cajones y puertas
        /// de los muebles miraban a la pared) y con el teléfono (pantalla hacia -Z antes de la corrección).
        /// </summary>
        public static float ModelYawFix = 180f;

        // Paleta de Blender (gnlib.COLORS) → textura del proyecto + tinte.
        static readonly Dictionary<string, KeyValuePair<string, Color>> Palette = new Dictionary<string, KeyValuePair<string, Color>>
        {
            { "WoodDark", P("wood_dark", 1f, 1f, 1f) }, { "WoodLight", P("wood_light", 1f, 1f, 1f) }, { "WoodMid", P("wood_light", 0.62f, 0.5f, 0.42f) },
            { "Paint", P("paint_white", 1f, 1f, 1f) }, { "Metal", P("metal", 1f, 1f, 1f) }, { "Chrome", P("metal", 1.3f, 1.3f, 1.35f) },
            { "PlasticDark", P("plastic_dark", 1f, 1f, 1f) }, { "PlasticBeige", P("plastic_beige", 1f, 1f, 1f) }, { "Sheet", P("sheet", 1f, 1f, 1f) },
            { "Blanket", P("blanket", 1f, 1f, 1f) }, { "Pillow", P("sheet", 1.1f, 1.1f, 1.05f) }, { "Fabric", P("fabric_sofa", 1f, 1f, 1f) },
            { "FabricGreen", P("fabric_sofa", 0.45f, 0.75f, 0.55f) }, { "Ceramic", P("paint_white", 1.15f, 1.15f, 1.15f) }, { "Glass", P("glass", 1f, 1f, 1f) },
            { "Screen", P("monitor_off", 1f, 1f, 1f) }, { "LampShade", P("noise_light", 0.95f, 0.85f, 0.65f) }, { "Bulb", P("noise_light", 1f, 0.95f, 0.8f) },
            { "Paper", P("noise_light", 0.9f, 0.88f, 0.8f) }, { "BookRed", P("noise_light", 0.45f, 0.1f, 0.08f) }, { "BookBlue", P("noise_light", 0.1f, 0.16f, 0.4f) },
            { "BookGreen", P("noise_light", 0.12f, 0.3f, 0.16f) }, { "BookCream", P("noise_light", 0.7f, 0.62f, 0.5f) }, { "Rubber", P("plastic_dark", 0.6f, 0.6f, 0.6f) },
            { "Red", P("noise_light", 0.6f, 0.05f, 0.04f) }, { "Counter", P("counter", 1f, 1f, 1f) }, { "Fridge", P("fridge", 1f, 1f, 1f) },
            { "Skin", P("skin_hand", 1f, 1f, 1f) }, { "Hoodie", P("cloth_hoodie", 1f, 1f, 1f) }, { "Jeans", P("cloth_jeans", 1f, 1f, 1f) },
            { "Hair", P("hair_wet", 0.5f, 0.45f, 0.4f) }, { "EntitySkin", P("entity_skin", 1f, 1f, 1f) }, { "EntityCloth", P("entity_cloth", 1f, 1f, 1f) },
            { "Eye", P("noise_light", 0.9f, 0.9f, 0.85f) }, { "EyeDark", P(null, 0f, 0f, 0f) }, { "Mouth", P(null, 0.12f, 0.02f, 0.02f) },
            { "Cardboard", P("noise_light", 0.55f, 0.4f, 0.25f) }, { "Brass", P("metal", 0.9f, 0.7f, 0.3f) }, { "Plant", P("grass", 0.6f, 0.9f, 0.6f) },
            { "Soil", P("noise_light", 0.15f, 0.1f, 0.06f) }, { "Towel", P("sheet", 0.6f, 0.75f, 0.8f) }, { "Tape", P("vhs_plastic", 1f, 1f, 1f) },
            { "Label", P("tape_label_blank", 1f, 1f, 1f) }, { "Phone", P("plastic_dark", 0.4f, 0.4f, 0.45f) }, { "PhoneScreen", P(null, 0.015f, 0.017f, 0.02f) },
            { "Coat", P("cloth_coat", 1f, 1f, 1f) }, { "Carpet", P("carpet", 1f, 1f, 1f) },
        };
        static KeyValuePair<string, Color> P(string t, float r, float g, float b) => new KeyValuePair<string, Color>(t, new Color(r, g, b));

        public static Material PaletteMat(string key)
        {
            if (key == "Glass") return Unlit("GN_Glass", "glass", true);
            KeyValuePair<string, Color> e;
            if (!Palette.TryGetValue(key, out e)) e = P("noise_light", 0.5f, 0.5f, 0.5f);
            float smooth = key == "Metal" || key == "Chrome" || key == "Ceramic" || key == "Glass" || key == "Fridge" ? 0.55f : (key == "Screen" || key == "PhoneScreen" || key == "Phone" ? 0.8f : 0.12f);
            float metal = key == "Chrome" || key == "Metal" || key == "Brass" ? 0.6f : 0f;
            return Lit("GN_" + key, e.Key, e.Value, smooth, metal);
        }

        /// <summary>Medidas reales de cada modelo en Blender (ancho X, alto Y, fondo Z), para validar la orientación al importar.</summary>
        static readonly Dictionary<string, Vector3> Expected = new Dictionary<string, Vector3>
        {
            { "alarm_clock", new Vector3(0.15f, 0.07f, 0.07f) },
            { "alarm_panel", new Vector3(0.30f, 0.42f, 0.06f) },
            { "armchair", new Vector3(0.90f, 0.90f, 0.84f) },
            { "bath_sink", new Vector3(0.56f, 1.00f, 0.44f) },
            { "bathtub", new Vector3(0.79f, 2.01f, 1.70f) },
            { "bed", new Vector3(1.14f, 1.08f, 2.07f) },
            { "bookshelf_wall", new Vector3(1.10f, 0.39f, 0.24f) },
            { "breaker_box", new Vector3(0.60f, 0.80f, 0.20f) },
            { "cable_bundle", new Vector3(0.21f, 0.04f, 2.01f) },
            { "camera_switcher", new Vector3(0.34f, 0.07f, 0.12f) },
            { "car", new Vector3(1.95f, 1.45f, 4.25f) },
            { "chair", new Vector3(0.42f, 0.97f, 0.42f) },
            { "coat_rack", new Vector3(0.49f, 1.77f, 0.42f) },
            { "coffee_table", new Vector3(1.10f, 0.51f, 0.60f) },
            { "cork_board", new Vector3(1.10f, 0.76f, 0.04f) },
            { "crt_monitor", new Vector3(0.42f, 0.36f, 0.49f) },
            { "desk", new Vector3(1.30f, 0.76f, 0.66f) },
            { "desk_fan", new Vector3(0.20f, 0.31f, 0.20f) },
            { "desk_lamp", new Vector3(0.30f, 0.49f, 0.30f) },
            { "door_frame", new Vector3(1.04f, 2.12f, 0.17f) },
            { "double", new Vector3(0.46f, 1.79f, 0.29f) },
            { "dresser", new Vector3(1.04f, 0.88f, 0.48f) },
            { "entity", new Vector3(0.56f, 2.22f, 0.58f) },
            { "fence_segment", new Vector3(2.02f, 1.21f, 0.07f) },
            { "filing_cabinet", new Vector3(0.48f, 1.30f, 0.63f) },
            { "floor_lamp", new Vector3(0.44f, 1.69f, 0.44f) },
            { "fp_hands", new Vector3(0.68f, 0.11f, 0.44f) },
            { "fridge", new Vector3(0.66f, 1.80f, 0.68f) },
            { "front_door", new Vector3(0.95f, 2.10f, 0.16f) },
            { "fuse", new Vector3(0.08f, 0.03f, 0.03f) },
            { "generator_lever", new Vector3(0.36f, 0.51f, 0.19f) },
            { "hall_wardrobe", new Vector3(1.66f, 2.18f, 0.55f) },
            { "interior_door", new Vector3(0.88f, 2.03f, 0.12f) },
            { "isolation_console", new Vector3(1.00f, 0.96f, 0.50f) },
            { "keyboard", new Vector3(0.44f, 0.04f, 0.16f) },
            { "kitchen_base", new Vector3(2.44f, 1.17f, 0.62f) },
            { "kitchen_table", new Vector3(1.10f, 1.01f, 0.80f) },
            { "kitchen_wall", new Vector3(2.40f, 0.71f, 0.38f) },
            { "locker", new Vector3(0.55f, 1.95f, 0.60f) },
            { "metal_door", new Vector3(0.90f, 2.03f, 0.11f) },
            { "mirror_cabinet", new Vector3(0.86f, 0.70f, 0.15f) },
            { "nightstand", new Vector3(0.48f, 0.58f, 0.46f) },
            { "pantry", new Vector3(0.70f, 2.05f, 0.64f) },
            { "pc_tower", new Vector3(0.18f, 0.42f, 0.43f) },
            { "phone_on_table", new Vector3(0.07f, 0.01f, 0.15f) },
            { "photo_frame_stand", new Vector3(0.20f, 0.16f, 0.12f) },
            { "photo_frame_wall", new Vector3(0.44f, 0.34f, 0.02f) },
            { "plant", new Vector3(0.32f, 0.72f, 0.35f) },
            { "reel_recorder", new Vector3(0.40f, 0.14f, 0.30f) },
            { "sash_window", new Vector3(1.40f, 1.22f, 0.25f) },
            { "secret_desk", new Vector3(1.90f, 1.24f, 0.71f) },
            { "security_desk", new Vector3(1.40f, 0.84f, 0.60f) },
            { "side_table", new Vector3(0.60f, 0.82f, 0.36f) },
            { "sofa", new Vector3(1.90f, 0.87f, 0.84f) },
            { "stove", new Vector3(0.60f, 1.05f, 0.65f) },
            { "street_lamp", new Vector3(0.28f, 4.40f, 1.02f) },
            { "tall_bookcase", new Vector3(1.00f, 1.90f, 0.32f) },
            { "tape_reel", new Vector3(0.15f, 0.02f, 0.15f) },
            { "toilet", new Vector3(0.41f, 0.80f, 0.58f) },
            { "toolbox", new Vector3(0.44f, 0.25f, 0.23f) },
            { "towel_rack", new Vector3(0.62f, 0.55f, 0.11f) },
            { "tree", new Vector3(2.28f, 5.01f, 2.37f) },
            { "tv_stand", new Vector3(1.20f, 1.26f, 0.60f) },
            { "vcr_deck", new Vector3(0.42f, 0.09f, 0.34f) },
            { "vhs_tape", new Vector3(0.19f, 0.19f, 0.03f) },
            { "wall_clock", new Vector3(0.34f, 0.34f, 0.06f) },
            { "wardrobe_slatted", new Vector3(1.14f, 2.05f, 0.61f) },
        };
        public static bool SkipRemap;
        public static readonly List<string> AutoFixLog = new List<string>();

        /// <summary>
        /// Instancia un modelo FBX dentro de un contenedor con el yaw pedido. Si el FBX llega volcado o girado (sus medidas
        /// no coinciden con las de Blender), se prueba cada rotación de 90° del modelo interior y se elige la que coincide.
        /// Si no existe, devuelve una caja gris (para que el juego siga funcionando).
        /// </summary>
        public static GameObject Model(string file, Transform parent, Vector3 pos, float yaw, string rename = null, bool colliders = true)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelDir + file + ".fbx");
            if (prefab == null)
            {
                Debug.LogWarning("[GOOD NIGHT] Falta el modelo " + file + ".fbx — se usa una caja.");
                var bx = Box(rename ?? file, parent, pos + Vector3.up * 0.4f, new Vector3(0.6f, 0.8f, 0.6f), PaletteMat("Paint"), 1f, colliders);
                bx.transform.localRotation = Quaternion.Euler(0, yaw, 0);
                return bx;
            }
            var wrap = new GameObject(rename ?? file);
            wrap.transform.SetParent(parent, false);
            wrap.transform.localPosition = pos;
            wrap.transform.localRotation = Quaternion.identity;
            var inner = (GameObject)Object.Instantiate(prefab);
            inner.name = file + "_fbx";
            inner.transform.SetParent(wrap.transform, false);
            inner.transform.localPosition = Vector3.zero;
            inner.transform.localRotation = Quaternion.Euler(0, ModelYawFix, 0);
            FixOrientation(file, wrap.transform, inner.transform);
            wrap.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            if (!SkipRemap) RemapMaterials(wrap);
            if (colliders) AddColliders(wrap);
            return wrap;
        }

        /// <summary>Compara el tamaño local con el esperado y corrige vuelcos/giros de 90° del modelo interior.</summary>
        static void FixOrientation(string file, Transform wrap, Transform inner)
        {
            Vector3 exp;
            if (!Expected.TryGetValue(file, out exp)) return;
            if (file == "entity" || file == "double") return; // personajes: el rig decide (se validan aparte)
            var baseRot = inner.localRotation;
            var candidates = new[]
            {
                Vector3.zero, new Vector3(-90, 0, 0), new Vector3(90, 0, 0), new Vector3(0, 0, 90), new Vector3(0, 0, -90), new Vector3(180, 0, 0),
                new Vector3(0, 90, 0), new Vector3(-90, 90, 0), new Vector3(90, 90, 0), new Vector3(0, 90, 90), new Vector3(0, 90, -90)
            };
            float best = float.MaxValue; Vector3 bestE = Vector3.zero; Vector3 firstSize = Vector3.zero;
            for (int i = 0; i < candidates.Length; i++)
            {
                inner.localRotation = Quaternion.Euler(candidates[i]) * baseRot;
                var sz = LocalSize(wrap, inner.gameObject);
                if (i == 0) firstSize = sz;
                float err = Mathf.Abs(sz.y - exp.y) * 2f + Mathf.Abs(sz.x - exp.x) + Mathf.Abs(sz.z - exp.z);
                if (err < best - 0.02f) { best = err; bestE = candidates[i]; }
            }
            inner.localRotation = Quaternion.Euler(bestE) * baseRot;
            if (bestE != Vector3.zero)
            {
                string msg = file + ": llegó con medidas " + firstSize.ToString("F2") + " (esperado " + exp.ToString("F2") + ") → corregido con rotación " + bestE;
                AutoFixLog.Add(msg);
                Debug.LogWarning("[GOOD NIGHT · orientación] " + msg);
            }
        }

        static Vector3 LocalSize(Transform space, GameObject g)
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
            return bb.size;
        }

        /// <summary>El FBX dentro del contenedor creado por Model().</summary>
        public static Transform Inner(GameObject wrap) => wrap.transform.childCount > 0 ? wrap.transform.GetChild(0) : wrap.transform;

        public static void RemapMaterials(GameObject g)
        {
            foreach (var r in g.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    string n = mats[i] != null ? mats[i].name : "Paint";
                    n = n.Replace(" (Instance)", "");
                    int dot = n.IndexOf('.'); if (dot > 0) n = n.Substring(0, dot);
                    if (n.StartsWith("GN_")) n = n.Substring(3);
                    mats[i] = PaletteMat(n);
                }
                r.sharedMaterials = mats;
                r.lightProbeUsage = LightProbeUsage.Off;
            }
        }

        /// <summary>MeshCollider exacto por pieza: los muebles huecos dejan alcanzar lo que hay dentro.</summary>
        public static void AddColliders(GameObject g)
        {
            foreach (var mf in g.GetComponentsInChildren<MeshFilter>(true))
                if (mf.GetComponent<Collider>() == null && mf.sharedMesh != null) mf.gameObject.AddComponent<MeshCollider>().sharedMesh = mf.sharedMesh;
            foreach (var sm in g.GetComponentsInChildren<SkinnedMeshRenderer>(true)) sm.updateWhenOffscreen = true;
        }

        public static void RemoveColliders(GameObject g)
        {
            foreach (var c in g.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
        }

        public static Transform Find(Transform t, string name)
        {
            if (t.name == name) return t;
            foreach (Transform c in t) { var r = Find(c, name); if (r != null) return r; }
            return null;
        }

        public static Transform Part(GameObject g, string name) => g != null ? Find(g.transform, name) : null;

        public static Bounds WorldBounds(GameObject g)
        {
            var rs = g.GetComponentsInChildren<Renderer>(true);
            if (rs.Length == 0) return new Bounds(g.transform.position, Vector3.zero);
            var b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            return b;
        }

        /// <summary>Mueve el modelo para que el centro de sus límites (en XZ) quede en 'worldXZ' y su base en 'floorY'.</summary>
        public static void PlaceByBounds(GameObject g, Vector3 worldXZ, float floorY = float.NaN)
        {
            var b = WorldBounds(g);
            Vector3 d = new Vector3(worldXZ.x - b.center.x, float.IsNaN(floorY) ? 0f : floorY - b.min.y, worldXZ.z - b.center.z);
            g.transform.position += d;
        }

        /// <summary>Coloca un modelo contra una pared: 'wallPoint' es un punto de la cara de la pared, 'facing' el yaw hacia la habitación.</summary>
        public static GameObject AgainstWall(string file, Transform parent, Vector3 wallPoint, float facing, float y = 0f, string rename = null, float gap = 0.01f)
        {
            var g = Model(file, parent, Vector3.zero, facing, rename);
            g.transform.position = wallPoint;
            var b = WorldBounds(g);
            Vector3 n = Quaternion.Euler(0, facing, 0) * Vector3.forward;
            float depth = Mathf.Abs(n.x) > 0.5f ? b.size.x : b.size.z;
            Vector3 target = wallPoint + n * (depth * 0.5f + gap);
            PlaceByBounds(g, target, y);
            return g;
        }

        public static void SetLayer(GameObject g, int layer) { GNLayers.SetLayerRecursive(g, layer); }

        public static void NoShadows(GameObject g)
        {
            foreach (var r in g.GetComponentsInChildren<Renderer>(true)) r.shadowCastingMode = ShadowCastingMode.Off;
        }
    }
}
