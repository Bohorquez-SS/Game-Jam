using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>Grafo de puntos de navegación de la casa. Las aristas pueden pasar por puertas o persianas.</summary>
    public class WaypointGraph : MonoBehaviour
    {
        public Transform[] nodes;
        public int[] edgeA, edgeB;
        public DoorController[] edgeDoor;
        public Shutter[] edgeShutter;
        string[] zones;
        List<int>[] adj;

        void Start() { Init(); }

        public void Init()
        {
            if (adj != null) return;
            adj = new List<int>[nodes.Length];
            for (int i = 0; i < nodes.Length; i++) adj[i] = new List<int>();
            for (int e = 0; e < edgeA.Length; e++) { adj[edgeA[e]].Add(e); adj[edgeB[e]].Add(e); }
            zones = new string[nodes.Length];
            for (int i = 0; i < nodes.Length; i++) zones[i] = RoomZone.At(nodes[i].position);
        }

        public Vector3 Pos(int n) => nodes[n].position;
        public string ZoneOf(int n) { Init(); return zones[n]; }
        int Other(int e, int n) => edgeA[e] == n ? edgeB[e] : edgeA[e];
        public bool Blocked(int e)
        {
            if (edgeShutter != null && e < edgeShutter.Length && edgeShutter[e] != null && edgeShutter[e].Blocks) return true;
            if (edgeDoor != null && e < edgeDoor.Length && edgeDoor[e] != null && edgeDoor[e].locked && !edgeDoor[e].isOpen) return true;
            return false;
        }

        public int Edge(int a, int b)
        {
            Init();
            foreach (var e in adj[a]) if (Other(e, a) == b) return e;
            return -1;
        }

        public DoorController DoorBetween(int a, int b)
        {
            int e = Edge(a, b);
            return e >= 0 && edgeDoor != null && e < edgeDoor.Length ? edgeDoor[e] : null;
        }

        /// <summary>Camino BFS (sin incluir el nodo de inicio). Null si no hay camino.</summary>
        public List<int> Path(int from, int to)
        {
            Init();
            if (from < 0 || to < 0) return null;
            var prev = new int[nodes.Length];
            for (int i = 0; i < prev.Length; i++) prev[i] = -2;
            var q = new Queue<int>();
            q.Enqueue(from); prev[from] = -1;
            while (q.Count > 0)
            {
                int n = q.Dequeue();
                if (n == to) break;
                foreach (var e in adj[n])
                {
                    if (Blocked(e)) continue;
                    int m = Other(e, n);
                    if (prev[m] != -2) continue;
                    prev[m] = n; q.Enqueue(m);
                }
            }
            if (prev[to] == -2) return null;
            var path = new List<int>();
            for (int c = to; c != from && c >= 0; c = prev[c]) path.Add(c);
            path.Reverse();
            return path;
        }

        /// <summary>Nodo más cercano, preferentemente de la misma habitación.</summary>
        public int Nearest(Vector3 p)
        {
            Init();
            string z = RoomZone.At(p);
            int best = -1; float bd = float.MaxValue;
            for (int pass = 0; pass < 2 && best < 0; pass++)
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (pass == 0 && z != null && zones[i] != z) continue;
                    float d = (nodes[i].position - p).sqrMagnitude;
                    if (d < bd) { bd = d; best = i; }
                }
            return best;
        }
    }
}
