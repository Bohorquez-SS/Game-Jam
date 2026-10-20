using System.IO;
using UnityEditor;
using UnityEngine;

namespace GNight.EditorTools
{
    /// <summary>
    /// QA visual: saca fotos de cada habitación (4 esquinas a la altura de los ojos + planta cenital) y las guarda en
    /// &lt;proyecto&gt;/QA_Snapshots. Sirve para revisar muebles volcados, flotando o atravesando paredes sin recorrer la casa.
    /// Menú: Tools/GOOD NIGHT/QA/Fotos de habitaciones.
    /// </summary>
    public static class QASnapshots
    {
        struct Room { public string id; public float x0, x1, z0, z1; public Room(string i, float a, float b, float c, float d) { id = i; x0 = a; x1 = b; z0 = c; z1 = d; } }

        [MenuItem("Tools/GOOD NIGHT/QA/Fotos de habitaciones")]
        public static void Run()
        {
            string dir = Path.Combine(Path.GetDirectoryName(Application.dataPath), "QA_Snapshots");
            Directory.CreateDirectory(dir);
            var rooms = new[]
            {
                new Room("dormitorio", HouseBuilder.WestX, 0f, HouseBuilder.HallZ, HouseBuilder.BedN),
                new Room("bano", HouseBuilder.BathX0, HouseBuilder.SecX0, HouseBuilder.HallZ, HouseBuilder.NorthZ),
                new Room("secreta", HouseBuilder.SecX0, HouseBuilder.EastX, HouseBuilder.HallZ, HouseBuilder.NorthZ),
                new Room("pasilloO", HouseBuilder.WestX, 0.3f, -HouseBuilder.HallZ, HouseBuilder.HallZ),
                new Room("pasilloE", 0.3f, HouseBuilder.EastX, -HouseBuilder.HallZ, HouseBuilder.HallZ),
                new Room("sala", HouseBuilder.WestX, HouseBuilder.KitX0, HouseBuilder.SouthZ, -HouseBuilder.HallZ),
                new Room("cocina", HouseBuilder.KitX0, HouseBuilder.EastX, HouseBuilder.SouthZ, -HouseBuilder.HallZ),
            };
            var go = new GameObject("QA_Cam") { hideFlags = HideFlags.HideAndDontSave };
            var cam = go.AddComponent<Camera>();
            cam.cullingMask = ~((1 << 31) | (1 << 30) | (1 << 29) | (1 << 5));
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
            cam.fieldOfView = 78f; cam.nearClipPlane = 0.05f; cam.farClipPlane = 60f;
            var rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            cam.targetTexture = rt;
            int n = 0;
            try
            {
                foreach (var r in rooms)
                {
                    Vector3 c = new Vector3((r.x0 + r.x1) * 0.5f, 1.2f, (r.z0 + r.z1) * 0.5f);
                    float m = 0.35f;
                    var corners = new[] { new Vector3(r.x0 + m, 1.62f, r.z0 + m), new Vector3(r.x1 - m, 1.62f, r.z0 + m), new Vector3(r.x1 - m, 1.62f, r.z1 - m), new Vector3(r.x0 + m, 1.62f, r.z1 - m) };
                    for (int i = 0; i < 4; i++)
                    {
                        cam.orthographic = false;
                        go.transform.position = corners[i];
                        go.transform.LookAt(c);
                        Shot(cam, rt, Path.Combine(dir, r.id + "_" + i + ".jpg")); n++;
                    }
                }
                // planta cenital (por debajo del techo)
                cam.orthographic = true; cam.orthographicSize = 5.6f; cam.nearClipPlane = 0.01f;
                go.transform.position = new Vector3(0.3f, 2.45f, 0f); go.transform.rotation = Quaternion.Euler(90, 0, 0);
                Shot(cam, rt, Path.Combine(dir, "planta.jpg")); n++;
                // exterior frontal
                cam.orthographic = false; cam.nearClipPlane = 0.05f;
                go.transform.position = new Vector3(-1f, 2.2f, -12f); go.transform.LookAt(new Vector3(0, 1.4f, -4f));
                Shot(cam, rt, Path.Combine(dir, "exterior.jpg")); n++;
            }
            finally
            {
                cam.targetTexture = null; rt.Release(); Object.DestroyImmediate(rt); Object.DestroyImmediate(go);
            }
            Debug.Log("[GOOD NIGHT QA] " + n + " fotos en " + dir);
        }

        static void Shot(Camera cam, RenderTexture rt, string path)
        {
            cam.Render();
            var prev = RenderTexture.active; RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); tex.Apply();
            RenderTexture.active = prev;
            File.WriteAllBytes(path, tex.EncodeToJPG(85));
            Object.DestroyImmediate(tex);
        }
    }
}
