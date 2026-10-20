using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>
    /// (18/10) Arreglos visuales de personajes, al cargar la escena (no hace falta reexportar los FBX):
    ///  · Doble: la "capucha" (esfera con material Hoodie pegada al hueso de la cabeza) sobresalía en la espalda
    ///    como una joroba al girar la cabeza → se quita esa sub-malla de la cara.
    ///  · Sujeto 0 (entidad): fuera los ojos/boca de caricatura; en su lugar, el rostro pálido del espejo pegado a la cabeza.
    /// </summary>
    public static class CharacterFix
    {
        static readonly Dictionary<Mesh, Mesh> cache = new Dictionary<Mesh, Mesh>();

        public static void Apply(Material faceMat)
        {
            foreach (var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (mf == null || mf.sharedMesh == null || !mf.name.StartsWith("Face")) continue;
                var mr = mf.GetComponent<MeshRenderer>(); if (mr == null) continue;
                var mats = mr.sharedMaterials;
                bool isEntity = false;
                for (var t = mf.transform; t != null; t = t.parent) if (t.name.StartsWith("entity_fbx")) { isEntity = true; break; }
                var drop = new List<int>();
                for (int i = 0; i < mats.Length && i < mf.sharedMesh.subMeshCount; i++)
                {
                    string n = mats[i] != null ? mats[i].name : "";
                    if (n.Contains("Hoodie")) drop.Add(i);
                }
                if (isEntity)
                    for (int i = 0; i < mats.Length && i < mf.sharedMesh.subMeshCount; i++)
                    {
                        string n = mats[i] != null ? mats[i].name : "";
                        if (n.Contains("Eye") || n.Contains("Mouth")) drop.Add(i);
                    }
                if (drop.Count == 0) continue;
                // centro de lo que se quita (ojos/boca): ahí va el rostro nuevo
                Vector3 c = Vector3.zero; int cnt = 0;
                var src = mf.sharedMesh;
                if (isEntity)
                {
                    var verts = src.vertices;
                    foreach (int sm in drop) foreach (int vi in src.GetTriangles(sm)) { c += verts[vi]; cnt++; }
                    if (cnt > 0) c /= cnt;
                }
                if (!cache.TryGetValue(src, out var m))
                {
                    m = Object.Instantiate(src); m.name = src.name + "_fix";
                    foreach (int sm in drop) m.SetTriangles(new int[0], sm);
                    cache[src] = m;
                }
                mf.sharedMesh = m;
                if (isEntity && cnt > 0 && faceMat != null && mf.transform.Find("Rostro_Sujeto0") == null)
                {
                    Transform root = mf.transform; while (root.parent != null && !root.name.StartsWith("entity_fbx")) root = root.parent;
                    if (root.parent != null) root = root.parent;   // el contenedor del modelo (orientación del personaje)
                    var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    q.name = "Rostro_Sujeto0";
                    Object.Destroy(q.GetComponent<Collider>());
                    var r = q.GetComponent<Renderer>(); r.sharedMaterial = faceMat; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    q.layer = mf.gameObject.layer;
                    Vector3 fwd = root.forward; fwd.y = 0; fwd.Normalize();
                    q.transform.SetParent(mf.transform, true);
                    q.transform.position = mf.transform.TransformPoint(c) + fwd * 0.035f + Vector3.up * 0.03f;
                    q.transform.rotation = Quaternion.LookRotation(-fwd, Vector3.up);
                    float s = Mathf.Max(0.5f, root.lossyScale.y);
                    q.transform.localScale = Vector3.one;   // reset y escalar en mundo
                    var ls = q.transform.lossyScale;
                    q.transform.localScale = new Vector3(0.27f * s / Mathf.Max(0.0001f, ls.x), 0.34f * s / Mathf.Max(0.0001f, ls.y), 1f / Mathf.Max(0.0001f, ls.z));
                }
            }
        }
    }
}
