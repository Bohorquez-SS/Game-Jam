using UnityEngine;

namespace GNight
{
    /// <summary>Opciones del jugador (persisten en PlayerPrefs).</summary>
    public static class Settings
    {
        public static float Master = 0.9f, Sfx = 1f, Sensitivity = 2f, Brightness = 0.5f;
        public static bool Fullscreen = true;
        /// <summary>0 = bajo, 1 = medio, 2 = alto.</summary>
        public static int Quality = 2;   // Alto por defecto (resolución interna completa)
        /// <summary>0 = sin VHS, 1 = suave, 2 = intenso.</summary>
        public static int Vhs = 1;
        public static Lang Language = Lang.ES;
        static bool loaded;
        public static event System.Action Changed;

        public static void Load()
        {
            if (loaded) return; loaded = true;
            Master = PlayerPrefs.GetFloat("gn_master", 0.9f);
            Sfx = PlayerPrefs.GetFloat("gn_sfx", 1f);
            Sensitivity = PlayerPrefs.GetFloat("gn_sens", 2f);
            Brightness = PlayerPrefs.GetFloat("gn_bright", 0.5f);
            Fullscreen = PlayerPrefs.GetInt("gn_full", 1) == 1;
            Quality = 2;   // (18/10) siempre ALTA: ya no hay selector y se ignora cualquier preferencia vieja
            Vhs = PlayerPrefs.GetInt("gn_vhs", 1);
            Language = (Lang)PlayerPrefs.GetInt("gn_lang", 0);
            Apply();
        }

        public static void Save()
        {
            PlayerPrefs.SetFloat("gn_master", Master);
            PlayerPrefs.SetFloat("gn_sfx", Sfx);
            PlayerPrefs.SetFloat("gn_sens", Sensitivity);
            PlayerPrefs.SetFloat("gn_bright", Brightness);
            PlayerPrefs.SetInt("gn_full", Fullscreen ? 1 : 0);
            PlayerPrefs.SetInt("gn_quality_v3", Quality);
            PlayerPrefs.SetInt("gn_vhs", Vhs);
            PlayerPrefs.SetInt("gn_lang", (int)Language);
            PlayerPrefs.Save();
            Apply();
        }

        public static void Apply()
        {
            AudioListener.volume = Master;
            if (!Application.isEditor) Screen.fullScreen = Fullscreen;
            if (Changed != null) Changed();
        }

        /// <summary>Altura en píxeles de la imagen interna según calidad (estética retro).</summary>
        /// <summary>Altura interna del render: proporcional a la pantalla (antes 300/420/600 fijos → demasiado pixelado).</summary>
        public static int RenderHeight => UnityEngine.Mathf.RoundToInt(UnityEngine.Screen.height * (Quality == 0 ? 0.75f : 1f));
        public static string QualityName => Quality == 0 ? T("Bajo", "Low") : (Quality == 1 ? T("Medio", "Medium") : T("Alto", "High"));
        public static string VhsName => Vhs == 0 ? T("Desactivado", "Off") : (Vhs == 1 ? T("Suave", "Soft") : T("Intenso", "Strong"));

        /// <summary>Texto localizado: español por defecto, inglés si está elegido y existe.</summary>
        public static string T(string es, string en) => Language == Lang.EN && !string.IsNullOrEmpty(en) ? en : es;
    }
}
