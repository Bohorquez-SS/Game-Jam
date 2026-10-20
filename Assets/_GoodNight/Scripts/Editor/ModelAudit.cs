using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static GNight.EditorTools.BuildKit;

namespace GNight.EditorTools
{
    /// <summary>
    /// Auditoría visual y numérica (Tools > GOOD NIGHT > Auditoría).
    /// 1) Modelos: instancia cada FBX tal como lo hace el constructor (incluida la corrección ModelYawFix), mide sus límites
    ///    (ancho X, alto Y, fondo Z) y saca una foto desde +Z: el FRENTE del mueble (cajones, puertas, pantalla) debe verse de cara.
    /// 2) Casa: planta cenital sin techos + 2 fotos por habitación con luz de trabajo.
    /// Todo se guarda en &lt;proyecto&gt;/Docs/Audit (fuera de Assets).
    /// </summary>
    public static class ModelAudit
    {
        static string OutDir => Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/Audit"));

        [MenuItem("Tools/GOOD NIGHT/Auditoría/1 · Modelos (medidas + fotos)", false, 60)]
        public static void AuditModels()
        {
            Directory.CreateDirectory(OutDir);
            var sb = new StringBuilder("modelo;ancho_x;alto_y;fondo_z;min_y;centro_x;centro_z;partes\n");
            BuildKit.SkipRemap = false;
            var guids = AssetDatabase.FindAssets("t:Model", new[] { ModelDir.TrimEnd('/') });
            var holder = new GameObject("__Audit");
            var light = new GameObject("__AuditLight").AddComponent<Light>();
            light.type = LightType.Directional; light.intensity = 1.3f; light.transform.rotation = Quaternion.Euler(35f, 160f, 0);
            var amb = RenderSettings.ambientLight; var ambMode = RenderSettings.ambientMode; var fog = RenderSettings.fog;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.6f); RenderSettings.fog = false;
            try
            {
                foreach (var g in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(g);
                    string name = Path.GetFileNameWithoutExtension(path);
                    if (name == "fp_hands") continue;
                    var go = Model(name, holder.transform, new Vector3(500, 0, 0), 0f, name, false);
                    var b = WorldBounds(go);
                    var parts = new List<string>();
                    foreach (Transform c in go.transform) parts.Add(c.name + "(" + Round(c.localEulerAngles) + ")");
                    sb.Append(name).Append(';').Append(b.size.x.ToString("F2")).Append(';').Append(b.size.y.ToString("F2")).Append(';').Append(b.size.z.ToString("F2"))
                      .Append(';').Append(b.min.y.ToString("F2")).Append(';').Append((b.center.x - 500).ToString("F2")).Append(';').Append(b.center.z.ToString("F2"))
                      .Append(';').Append(string.Join(" ", parts)).Append('\n');
                    // foto frontal 3/4
                    float r = Mathf.Max(0.25f, b.extents.magnitude);
                    Vector3 eye = b.center + new Vector3(r * 0.9f, r * 0.7f, r * 2.2f);
                    Shot(eye, b.center, 256, 256, Path.Combine(OutDir, "model_" + name + ".png"));
                    Object.DestroyImmediate(go);
                }
            }
            finally
            {
                Object.DestroyImmediate(holder); Object.DestroyImmediate(light.gameObject);
                RenderSettings.ambientLight = amb; RenderSettings.ambientMode = ambMode; RenderSettings.fog = fog;
            }
            File.WriteAllText(Path.Combine(OutDir, "models.csv"), sb.ToString());
            Debug.Log("[GOOD NIGHT] Auditoría de modelos lista: " + OutDir);
        }

        [MenuItem("Tools/GOOD NIGHT/Auditoría/2 · Casa (planta + habitaciones)", false, 61)]
        public static void AuditHouse()
        {
            Directory.CreateDirectory(OutDir);
            var hidden = new List<Renderer>();
            foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (r.name.StartsWith("Techo") || r.name.StartsWith("Cielo") || r.name == "Base" || r.name == "Foco") { if (r.enabled) { r.enabled = false; hidden.Add(r); } }
            var work = new List<Light>();
            var fog = RenderSettings.fog; RenderSettings.fog = false;
            var amb = RenderSettings.ambientLight; var ambMode = RenderSettings.ambientMode;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.52f);
            try
            {
                var sun = new GameObject("__AuditSun").AddComponent<Light>(); sun.type = LightType.Directional; sun.intensity = 1.1f; sun.transform.rotation = Quaternion.Euler(70f, 30f, 0); sun.shadows = LightShadows.Soft;
                work.Add(sun);
                // planta cenital
                Shot(new Vector3(0.3f, 13f, -0.4f), new Vector3(0.3f, 0f, 0f), 1400, 1400, Path.Combine(OutDir, "house_plan.png"), false, 5f, 52f);
                foreach (var t in work) t.transform.rotation = Quaternion.Euler(50f, 30f, 0);
                // habitaciones: dos esquinas opuestas a 1.6 m
                Room("dormitorio", new Vector3(-0.4f, 1.7f, 1.1f), new Vector3(-3.2f, 0.7f, 4.6f), new Vector3(-3.7f, 1.7f, 5.0f), new Vector3(-0.8f, 0.7f, 1.4f));
                Room("bano", new Vector3(2.2f, 1.7f, 1.0f), new Vector3(0.4f, 0.8f, 3.3f), new Vector3(0.3f, 1.7f, 3.5f), new Vector3(2.0f, 0.8f, 1.2f));
                Room("pasillo", new Vector3(-3.8f, 1.7f, -0.6f), new Vector3(4.2f, 0.8f, 0.5f), new Vector3(4.3f, 1.7f, -0.45f), new Vector3(-3.0f, 0.8f, 0.2f));
                Room("secreta", new Vector3(2.7f, 1.7f, 1.0f), new Vector3(4.3f, 0.8f, 3.4f), new Vector3(4.4f, 1.7f, 3.6f), new Vector3(2.8f, 0.8f, 1.1f));
                Room("sala", new Vector3(0.8f, 1.7f, -1.0f), new Vector3(-3.6f, 0.7f, -5.0f), new Vector3(-2.3f, 1.75f, -5.05f), new Vector3(0.6f, 0.7f, -1.1f));
                Room("cocina", new Vector3(1.2f, 1.7f, -1.0f), new Vector3(4.3f, 0.8f, -5.0f), new Vector3(4.4f, 1.7f, -5.1f), new Vector3(1.3f, 0.8f, -1.1f));
            }
            finally
            {
                foreach (var r in hidden) if (r) r.enabled = true;
                foreach (var l in work) if (l) Object.DestroyImmediate(l.gameObject);
                RenderSettings.fog = fog; RenderSettings.ambientLight = amb; RenderSettings.ambientMode = ambMode;
            }
            Debug.Log("[GOOD NIGHT] Auditoría de la casa lista: " + OutDir);
        }

        static void Room(string id, Vector3 eyeA, Vector3 lookA, Vector3 eyeB, Vector3 lookB)
        {
            Shot(eyeA, lookA, 960, 600, Path.Combine(OutDir, "room_" + id + "_A.png"));
            Shot(eyeB, lookB, 960, 600, Path.Combine(OutDir, "room_" + id + "_B.png"));
        }

        static string Round(Vector3 e) => Mathf.RoundToInt(e.x) + "," + Mathf.RoundToInt(e.y) + "," + Mathf.RoundToInt(e.z);

        static void Shot(Vector3 eye, Vector3 look, int w, int h, string file, bool ortho = false, float orthoSize = 5f, float fov = 70f)
        {
            var cg = new GameObject("__AuditCam");
            var cam = cg.AddComponent<Camera>();
            cam.transform.position = eye;
            var dir = (look - eye).normalized;
            cam.transform.rotation = Quaternion.LookRotation(dir, Mathf.Abs(dir.y) > 0.9f ? Vector3.forward : Vector3.up);
            cam.fieldOfView = fov; cam.nearClipPlane = 0.02f; cam.farClipPlane = 60f;
            cam.orthographic = ortho; cam.orthographicSize = orthoSize;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.18f, 0.2f, 0.24f);
            cam.cullingMask = ~(GNLayers.Mask(GNLayers.FP) | GNLayers.Mask(GNLayers.CamOnly) | GNLayers.Mask(GNLayers.Tape) | GNLayers.Mask(GNLayers.Mirror));
            var d = cam.GetUniversalAdditionalCameraData(); if (d != null) d.renderPostProcessing = false;
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            cam.targetTexture = rt;
            cam.Render();
            var prev = RenderTexture.active; RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply();
            RenderTexture.active = prev;
            File.WriteAllBytes(file, tex.EncodeToPNG());
            cam.targetTexture = null; Object.DestroyImmediate(rt); Object.DestroyImmediate(tex); Object.DestroyImmediate(cg);
        }
    }
}
