using UnityEngine;

namespace GNight
{
    /// <summary>Bus de eventos simple: los objetos del mundo avisan, la historia escucha.</summary>
    public static class GameEvents
    {
        public static event System.Action<string> OnEvent;
        public static void Raise(string id) { if (OnEvent != null) OnEvent(id); }
        public static void Clear() { OnEvent = null; }
    }
}
