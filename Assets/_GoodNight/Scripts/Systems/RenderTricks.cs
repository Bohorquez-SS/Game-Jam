using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GNight
{
    /// <summary>
    /// Permite cambiar el mundo SOLO mientras una cámara concreta renderiza (Built-in y URP):
    /// cosas que solo existen en un monitor, en una cinta VHS o en un espejo.
    /// </summary>
    public static class RenderTricks
    {
        class Hook { public System.Action begin, end; }
        static readonly Dictionary<Camera, List<Hook>> hooks = new Dictionary<Camera, List<Hook>>();
        static bool installed;
        static readonly List<Camera> active = new List<Camera>();

        static void Install()
        {
            if (installed) return; installed = true;
            Camera.onPreCull += c => Begin(c);
            Camera.onPostRender += c => End(c);
            RenderPipelineManager.beginCameraRendering += (ctx, c) => Begin(c);
            RenderPipelineManager.endCameraRendering += (ctx, c) => End(c);
        }

        public static void Add(Camera cam, System.Action begin, System.Action end)
        {
            if (cam == null) return;
            Install();
            List<Hook> l;
            if (!hooks.TryGetValue(cam, out l)) { l = new List<Hook>(); hooks[cam] = l; }
            l.Add(new Hook { begin = begin, end = end });
        }

        public static void ClearAll() { hooks.Clear(); active.Clear(); }

        static void Begin(Camera c)
        {
            List<Hook> l;
            if (c == null || !hooks.TryGetValue(c, out l) || active.Contains(c)) return;
            active.Add(c);
            foreach (var h in l) if (h.begin != null) h.begin();
        }

        static void End(Camera c)
        {
            List<Hook> l;
            if (c == null || !active.Contains(c) || !hooks.TryGetValue(c, out l)) return;
            active.Remove(c);
            for (int i = l.Count - 1; i >= 0; i--) if (l[i].end != null) l[i].end();
        }
    }
}
