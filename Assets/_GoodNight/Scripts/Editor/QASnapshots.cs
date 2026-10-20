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

        /// <summary>Abre (temporalmente) cada cajón deslizante y lo fotografía de cerca; también el escritorio y el panel de alarma.</summary>
        [MenuItem("Tools/GOOD NIGHT/QA/Fotos de cajones y detalles")]
        public static void Details()
        {
            string dir = Path.Combine(Path.GetDirectoryName(Application.dataPath), "QA_Snapshots");
            Directory.CreateDirectory(dir);
            var go = new GameObject("QA_Cam") { hideFlags = HideFlags.HideAndDontSave };
            var cam = go.AddComponent<Camera>();
            cam.cullingMask = ~((1 << 31) | (1 << 30) | (1 << 29) | (1 << 28) | (1 << 5));
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
            cam.fieldOfView = 60f; cam.nearClipPlane = 0.02f; cam.farClipPlane = 30f;
            var rt = new RenderTexture(960, 720, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            cam.targetTexture = rt;
            int n = 0;
            try
            {
                foreach (var d in Object.FindObjectsByType<Drawer>(FindObjectsSortMode.None))
                {
                    if (d.slide == null) continue;
                    if (d.rotate)
                    {
                        if (d.id != "toolbox") continue;
                        var r0 = d.slide.localRotation;
                        d.slide.localRotation = r0 * Quaternion.Euler(d.openEuler);
                        var bb = d.GetComponentInParent<Transform>().parent.GetComponentInChildren<Renderer>().bounds;
                        go.transform.position = bb.center + new Vector3(-0.55f, 0.75f, 0f); go.transform.LookAt(bb.center);
                        Shot(cam, rt, Path.Combine(dir, "cajon_toolbox.jpg")); n++;
                        d.slide.localRotation = r0;
                        continue;
                    }
                    var p0 = d.slide.localPosition;
                    d.slide.localPosition = p0 + d.openOffset;
                    Vector3 w = d.slide.parent != null ? d.slide.parent.TransformVector(d.openOffset).normalized : d.openOffset.normalized;
                    var r = d.slide.GetComponentInChildren<Renderer>();
                    Vector3 c = r != null ? r.bounds.center : d.slide.position;
                    go.transform.position = c + w * 0.75f + Vector3.up * 0.55f;
                    go.transform.LookAt(c - w * 0.12f);
                    Shot(cam, rt, Path.Combine(dir, "cajon_" + d.id + ".jpg")); n++;
                    d.slide.localPosition = p0;
                }
                var desk = GameObject.Find("Escritorio");
                if (desk != null)
                {
                    var b = desk.GetComponentInChildren<Renderer>().bounds;
                    go.transform.position = new Vector3(b.center.x - 1.1f, 1.45f, b.center.z - 0.2f); go.transform.LookAt(new Vector3(b.center.x, 0.85f, b.center.z));
                    Shot(cam, rt, Path.Combine(dir, "detalle_escritorio.jpg")); n++;
                }
                var ap = GameObject.Find("Panel_Alarma");
                if (ap != null)
                {
                    var b = ap.GetComponentInChildren<Renderer>().bounds;
                    go.transform.position = b.center + new Vector3(0, 0.05f, 0.55f); go.transform.LookAt(b.center);
                    Shot(cam, rt, Path.Combine(dir, "detalle_alarma.jpg")); n++;
                }
            }
            finally
            {
                cam.targetTexture = null; rt.Release(); Object.DestroyImmediate(rt); Object.DestroyImmediate(go);
            }
            Debug.Log("[GOOD NIGHT QA] " + n + " fotos de detalle en " + dir);
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
