namespace GNight
{
    /// <summary>Capas especiales: primera persona (manos/teléfono), solo-cámaras, solo-cintas, solo-espejo.</summary>
    public static class GNLayers
    {
        static int fp = -2, cam = -2, tape = -2, mirror = -2;
        static int Get(ref int cache, string name, int fallback)
        {
            if (cache == -2) { cache = UnityEngine.LayerMask.NameToLayer(name); if (cache < 0) cache = fallback; }
            return cache;
        }
        public static int FP => Get(ref fp, "GN_FirstPerson", 31);
        public static int CamOnly => Get(ref cam, "GN_CamOnly", 30);
        public static int Tape => Get(ref tape, "GN_Tape", 29);
        public static int Mirror => Get(ref mirror, "GN_Mirror", 28);
        public static int Mask(int layer) => 1 << layer;
        public static void SetLayerRecursive(UnityEngine.GameObject g, int layer)
        {
            g.layer = layer;
            foreach (UnityEngine.Transform c in g.transform) SetLayerRecursive(c.gameObject, layer);
        }
    }
}
