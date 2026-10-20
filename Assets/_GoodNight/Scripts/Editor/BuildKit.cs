using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GNight.EditorTools
{
    /// <summary>Hueco en una pared (puerta, ventana o arco).</summary>
    public struct Opening
    {
        public float center, width, bottom, top;
        public Opening(float c, float w, float b, float t) { center = c; width = w; bottom = b; top = t; }
    }

    /// <summary>Herramientas de construcción procedural: mallas con UV en metros, materiales, luces, audio y paredes con huecos.</summary>
    public static partial class BuildKit
    {
        public const string Root = "Assets/_GoodNight";
        public const string MatDir = Root + "/Materials";
        public const string MeshPath = Root + "/Materials/HouseMeshes.asset";
        public const float H = 2.6f;    // altura de paredes
        public const float T = 0.06f;   // espesor de cada capa de pared

        static Dictionary<string, Mesh> meshCache;
        static Dictionary<string, Material> matCache;
        static bool meshAssetCreated;
        public static bool URP => GraphicsSettings.currentRenderPipeline != null;

        public static void Reset()
        {
            EnsureFolder(MatDir);
            AssetDatabase.DeleteAsset(MeshPath);
            meshCache = new Dictionary<string, Mesh>();
            matCache = new Dictionary<string, Material>();
            meshAssetCreated = false;
        }

        // ------------------------------------------------------------------ assets
        public static Texture2D Tex(string n) => AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "/Art/Textures/" + n + ".png");
        public static AudioClip Clip(string n) => AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Audio/" + n + ".wav");

        static Material Save(Material m, string name)
        {
            if (matCache == null) matCache = new Dictionary<string, Material>();
            string p = MatDir + "/" + name + ".mat";
            AssetDatabase.DeleteAsset(p);
            AssetDatabase.CreateAsset(m, p);
            matCache[name] = m;
            return m;
        }

        /// <summary>Material iluminado (URP/Lit o Standard según el pipeline activo).</summary>
        public static Material Lit(string name, string tex, Color color, float smooth = 0.05f, float metallic = 0f)
        {
            if (matCache == null) matCache = new Dictionary<string, Material>();
            if (matCache.TryGetValue(name, out var cached)) return cached;
            var sh = URP ? Shader.Find("Universal Render Pipeline/Lit") : Shader.Find("Standard");
            var m = new Material(sh);
            var t = string.IsNullOrEmpty(tex) ? null : Tex(tex);
            if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", t);
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", t);
            // relieve: si existe <tex>_n.png (generado por Tools/Textures/gen_textures_hd.py) se usa como mapa normal
            var n = string.IsNullOrEmpty(tex) ? null : Tex(tex + "_n");
            if (n != null && m.HasProperty("_BumpMap"))
            {
                m.SetTexture("_BumpMap", n); m.EnableKeyword("_NORMALMAP");
                if (m.HasProperty("_BumpScale")) m.SetFloat("_BumpScale", 0.8f);
            }
            if (m.HasProperty("_Color")) m.SetColor("_Color", color);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smooth);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            if (m.HasProperty("_GlossyReflections")) { m.SetFloat("_GlossyReflections", 0f); m.EnableKeyword("_GLOSSYREFLECTIONS_OFF"); }
            if (m.HasProperty("_EnvironmentReflections")) { m.SetFloat("_EnvironmentReflections", 0f); m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF"); }
            return Save(m, name);
        }

        public static Material Unlit(string name, string tex, bool transparent)
        {
            if (matCache == null) matCache = new Dictionary<string, Material>();
            if (matCache.TryGetValue(name, out var cached)) return cached;
            var m = new Material(Shader.Find(transparent ? "Unlit/Transparent" : "Unlit/Texture"));
            m.mainTexture = Tex(tex);
            return Save(m, name);
        }

        public static Material UnlitColor(string name, Color c)
        {
            if (matCache == null) matCache = new Dictionary<string, Material>();
            if (matCache.TryGetValue(name, out var cached)) return cached;
            var m = new Material(Shader.Find("Unlit/Color")) { color = c };
            return Save(m, name);
        }

        // ------------------------------------------------------------------ geometría
        public static Transform Group(string name, Transform parent, Vector3 pos = default(Vector3), float yaw = 0f)
        {
            var g = new GameObject(name);
            if (parent) g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            g.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            return g.transform;
        }

        /// <summary>Caja con UV en metros (densidad de textura uniforme estilo PS1).</summary>
        public static GameObject Box(string name, Transform parent, Vector3 center, Vector3 size, Material m, float tile = 1f, bool collider = true, Vector3? euler = null)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.transform.localPosition = center;
            if (euler.HasValue) g.transform.localEulerAngles = euler.Value;
            g.AddComponent<MeshFilter>().sharedMesh = BoxMesh(size, tile);
            g.AddComponent<MeshRenderer>().sharedMaterial = m;
            if (collider) g.AddComponent<BoxCollider>().size = size;
            return g;
        }

        /// <summary>Caja definida por sus esquinas mínima/máxima.</summary>
        public static GameObject BoxMM(string name, Transform parent, Vector3 min, Vector3 max, Material m, float tile = 1f, bool collider = true)
        {
            return Box(name, parent, (min + max) * 0.5f, max - min, m, tile, collider);
        }

        static Mesh BoxMesh(Vector3 s, float tile)
        {
            string key = s.x.ToString("F3") + "_" + s.y.ToString("F3") + "_" + s.z.ToString("F3") + "_" + tile.ToString("F2");
            if (meshCache.TryGetValue(key, out var cached)) return cached;
            var v = new List<Vector3>(); var n = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();
            Vector3 h = s * 0.5f;
            Face(v, n, uv, tri, h, tile, Vector3.right, Vector3.back, Vector3.up, s.z, s.y);
            Face(v, n, uv, tri, h, tile, Vector3.left, Vector3.forward, Vector3.up, s.z, s.y);
            Face(v, n, uv, tri, h, tile, Vector3.up, Vector3.right, Vector3.back, s.x, s.z);
            Face(v, n, uv, tri, h, tile, Vector3.down, Vector3.right, Vector3.forward, s.x, s.z);
            Face(v, n, uv, tri, h, tile, Vector3.forward, Vector3.right, Vector3.up, s.x, s.y);
            Face(v, n, uv, tri, h, tile, Vector3.back, Vector3.left, Vector3.up, s.x, s.y);
            var mesh = new Mesh { name = "Box_" + key };
            mesh.SetVertices(v); mesh.SetNormals(n); mesh.SetUVs(0, uv); mesh.SetTriangles(tri, 0);
            mesh.RecalculateBounds(); mesh.RecalculateTangents();
            if (!meshAssetCreated) { AssetDatabase.CreateAsset(mesh, MeshPath); meshAssetCreated = true; }
            else AssetDatabase.AddObjectToAsset(mesh, MeshPath);
            meshCache[key] = mesh;
            return mesh;
        }

        static void Face(List<Vector3> v, List<Vector3> n, List<Vector2> uv, List<int> tri, Vector3 h, float tile,
                         Vector3 normal, Vector3 u, Vector3 vv, float uLen, float vLen)
        {
            Vector3 c = Vector3.Scale(normal, h);
            Vector3 du = u * uLen * 0.5f, dv = vv * vLen * 0.5f;
            int i = v.Count;
            v.Add(c - du - dv); v.Add(c + du - dv); v.Add(c + du + dv); v.Add(c - du + dv);
            for (int k = 0; k < 4; k++) n.Add(normal);
            float U = uLen / tile, V = vLen / tile;
            uv.Add(new Vector2(0, 0)); uv.Add(new Vector2(U, 0)); uv.Add(new Vector2(U, V)); uv.Add(new Vector2(0, V));
            tri.Add(i); tri.Add(i + 1); tri.Add(i + 2);
            tri.Add(i); tri.Add(i + 2); tri.Add(i + 3);
        }

        public static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Material m, bool collider = false, Vector3? euler = null)
        {
            var g = GameObject.CreatePrimitive(type);
            g.name = name;
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            g.transform.localScale = scale;
            if (euler.HasValue) g.transform.localEulerAngles = euler.Value;
            g.GetComponent<Renderer>().sharedMaterial = m;
            if (!collider) Object.DestroyImmediate(g.GetComponent<Collider>());
            return g;
        }

        /// <summary>Quad: yaw 0 = visible desde -Z (mira hacia -Z); 180 → +Z; 90 → -X; -90 → +X.</summary>
        public static GameObject Quad(string name, Transform parent, Vector3 pos, Vector2 size, float yaw, Material m, bool collider = false)
        {
            var q = Prim(PrimitiveType.Quad, name, parent, pos, new Vector3(size.x, size.y, 1f), m, collider, new Vector3(0, yaw, 0));
            q.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            return q;
        }

        public static void NoShadow(GameObject g) { var r = g.GetComponent<Renderer>(); if (r) r.shadowCastingMode = ShadowCastingMode.Off; }

        public static TextMesh Text3D(string name, Transform parent, Vector3 pos, float yaw, string text, float charSize, Color color, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            g.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            var tm = g.AddComponent<TextMesh>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tm.font = font; tm.fontSize = 64; tm.characterSize = charSize; tm.anchor = anchor;
            tm.alignment = TextAlignment.Center; tm.color = color; tm.text = text;
            g.GetComponent<MeshRenderer>().sharedMaterial = TextMaterial(font);
            g.AddComponent<Text3DDepth>();
            return tm;
        }

        /// <summary>Material de texto con prueba de profundidad (los rótulos ya no se ven a través de las paredes).</summary>
        static Material TextMaterial(Font font)
        {
            if (matCache == null) matCache = new Dictionary<string, Material>();
            if (matCache.TryGetValue("M_Text3D", out var cached)) return cached;
            var sh = Shader.Find("GNight/Text3D");
            if (sh == null) return font.material;
            var m = new Material(sh) { mainTexture = font.material != null ? font.material.mainTexture : null };
            return Save(m, "M_Text3D");
        }

        public static Light MakeLight(string name, Transform parent, LightType type, Vector3 pos, Color c, float intensity, float range, LightShadows shadows)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            var l = g.AddComponent<Light>();
            l.type = type; l.color = c; l.intensity = intensity; l.range = range; l.shadows = shadows; l.shadowStrength = 0.9f;
            return l;
        }

        public static AudioSource Src(string name, Transform parent, Vector3 pos, AudioClip clip, float volume, bool loop, float spatial = 1f, float minD = 1f, float maxD = 12f)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            var a = g.AddComponent<AudioSource>();
            a.clip = clip; a.volume = volume; a.loop = loop; a.playOnAwake = loop;
            a.spatialBlend = spatial; a.rolloffMode = AudioRolloffMode.Linear; a.minDistance = minD; a.maxDistance = maxD; a.dopplerLevel = 0f;
            return a;
        }

        // ------------------------------------------------------------------ paredes
        /// <summary>Pared paralela al eje X (z constante), entre x0 y x1, con huecos.</summary>
        public static void WallX(Transform p, string name, float z, float x0, float x1, Material m, float tile, params Opening[] ops)
        {
            var list = new List<Opening>(ops); list.Sort((a, b) => a.center.CompareTo(b.center));
            float cur = x0; int k = 0;
            foreach (var o in list)
            {
                float a = o.center - o.width * 0.5f, b = o.center + o.width * 0.5f;
                if (a > cur) BoxMM(name + "_" + k++, p, new Vector3(cur, 0, z - T * 0.5f), new Vector3(a, H, z + T * 0.5f), m, tile);
                if (o.bottom > 0.001f) BoxMM(name + "_bajo" + k++, p, new Vector3(a, 0, z - T * 0.5f), new Vector3(b, o.bottom, z + T * 0.5f), m, tile);
                if (o.top < H - 0.001f) BoxMM(name + "_alto" + k++, p, new Vector3(a, o.top, z - T * 0.5f), new Vector3(b, H, z + T * 0.5f), m, tile);
                cur = b;
            }
            if (x1 > cur) BoxMM(name + "_" + k, p, new Vector3(cur, 0, z - T * 0.5f), new Vector3(x1, H, z + T * 0.5f), m, tile);
        }

        /// <summary>Pared paralela al eje Z (x constante), entre z0 y z1, con huecos.</summary>
        public static void WallZ(Transform p, string name, float x, float z0, float z1, Material m, float tile, params Opening[] ops)
        {
            var list = new List<Opening>(ops); list.Sort((a, b) => a.center.CompareTo(b.center));
            float cur = z0; int k = 0;
            foreach (var o in list)
            {
                float a = o.center - o.width * 0.5f, b = o.center + o.width * 0.5f;
                if (a > cur) BoxMM(name + "_" + k++, p, new Vector3(x - T * 0.5f, 0, cur), new Vector3(x + T * 0.5f, H, a), m, tile);
                if (o.bottom > 0.001f) BoxMM(name + "_bajo" + k++, p, new Vector3(x - T * 0.5f, 0, a), new Vector3(x + T * 0.5f, o.bottom, b), m, tile);
                if (o.top < H - 0.001f) BoxMM(name + "_alto" + k++, p, new Vector3(x - T * 0.5f, o.top, a), new Vector3(x + T * 0.5f, H, b), m, tile);
                cur = b;
            }
            if (z1 > cur) BoxMM(name + "_" + k, p, new Vector3(x - T * 0.5f, 0, cur), new Vector3(x + T * 0.5f, H, z1), m, tile);
        }

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
