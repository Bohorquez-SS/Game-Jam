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
        /// <summary>Corrección global de orientación de los FBX (el frente del modelo debe mirar a +Z con yaw 0).</summary>
        public static float ModelYawFix = 0f;

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
            { "Label", P("tape_label_blank", 1f, 1f, 1f) }, { "Phone", P("plastic_dark", 0.4f, 0.4f, 0.45f) }, { "PhoneScreen", P("monitor_off", 1f, 1f, 1f) },
            { "Coat", P("cloth_coat", 1f, 1f, 1f) }, { "Carpet", P("carpet", 1f, 1f, 1f) },
        };
        static KeyValuePair<string, Color> P(string t, float r, float g, float b) => new KeyValuePair<string, Color>(t, new Color(r, g, b));

        public static Material PaletteMat(string key)
        {
            if (key == "Glass") return Unlit("GN_Glass", "glass", true);
            KeyValuePair<string, Color> e;
            if (!Palette.TryGetValue(key, out e)) e = P("noise_light", 0.5f, 0.5f, 0.5f);
            float smooth = key == "Metal" || key == "Chrome" || key == "Ceramic" || key == "Glass" || key == "Fridge" ? 0.55f : (key == "Screen" ? 0.8f : 0.12f);
            float metal = key == "Chrome" || key == "Metal" || key == "Brass" ? 0.6f : 0f;
            return Lit("GN_" + key, e.Key, e.Value, smooth, metal);
        }

        /// <summary>Instancia un modelo FBX. Si no existe, devuelve una caja gris (para que el juego siga funcionando).</summary>
        public static GameObject Model(string file, Transform parent, Vector3 pos, float yaw, string rename = null, bool colliders = true)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelDir + file + ".fbx");
            GameObject g;
            if (prefab != null)
            {
                g = (GameObject)Object.Instantiate(prefab);
                g.name = rename ?? file;
                g.transform.SetParent(parent, false);
                g.transform.localPosition = pos;
                g.transform.localRotation = Quaternion.Euler(0, yaw + ModelYawFix, 0);
                RemapMaterials(g);
                if (colliders) AddColliders(g);
            }
            else
            {
                Debug.LogWarning("[GOOD NIGHT] Falta el modelo " + file + ".fbx — se usa una caja.");
                g = Box(rename ?? file, parent, pos + Vector3.up * 0.4f, new Vector3(0.6f, 0.8f, 0.6f), PaletteMat("Paint"), 1f, colliders);
                g.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            }
            return g;
        }

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
