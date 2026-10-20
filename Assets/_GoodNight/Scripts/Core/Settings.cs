using UnityEngine;

namespace GNight
{
    /// <summary>Opciones del jugador (persisten en PlayerPrefs).</summary>
    public static class Settings
    {
        public static float Master = 0.9f, Sfx = 1f, Sensitivity = 2f;
        public static bool Fullscreen = true;
        public static Lang Language = Lang.ES;
        static bool loaded;

        public static void Load()
        {
            if (loaded) return; loaded = true;
            Master = PlayerPrefs.GetFloat("gn_master", 0.9f);
            Sfx = PlayerPrefs.GetFloat("gn_sfx", 1f);
            Sensitivity = PlayerPrefs.GetFloat("gn_sens", 2f);
            Fullscreen = PlayerPrefs.GetInt("gn_full", 1) == 1;
            Language = (Lang)PlayerPrefs.GetInt("gn_lang", 0);
            Apply();
        }

        public static void Save()
        {
            PlayerPrefs.SetFloat("gn_master", Master);
            PlayerPrefs.SetFloat("gn_sfx", Sfx);
            PlayerPrefs.SetFloat("gn_sens", Sensitivity);
            PlayerPrefs.SetInt("gn_full", Fullscreen ? 1 : 0);
            PlayerPrefs.SetInt("gn_lang", (int)Language);
            PlayerPrefs.Save();
            Apply();
        }

        public static void Apply()
        {
            AudioListener.volume = Master;
            if (!Application.isEditor) Screen.fullScreen = Fullscreen;
        }

        /// <summary>Texto localizado: español por defecto, inglés si está elegido y existe.</summary>
        public static string T(string es, string en) => Language == Lang.EN && !string.IsNullOrEmpty(en) ? en : es;
    }
}
