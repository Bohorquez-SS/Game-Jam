using UnityEngine;

namespace GNight
{
    /// <summary>Hora de la ficción.</summary>
    public static class GameClock
    {
        public static int Hour = 23, Minute = 47;
        public static void Set(int h, int m) { Hour = h; Minute = m; }
        public static void Add(int minutes)
        {
            int t = (Hour * 60 + Minute + minutes) % (24 * 60);
            Hour = t / 60; Minute = t % 60;
        }
        public static string Text => Hour.ToString("00") + ":" + Minute.ToString("00");
    }
}
