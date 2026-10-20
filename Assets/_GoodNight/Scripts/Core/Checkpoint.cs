using UnityEngine;
using UnityEngine.SceneManagement;

namespace GNight
{
    public static class Checkpoint
    {
        /// <summary>Etapa con la que debe empezar la escena tras recargar (None = menú principal).</summary>
        public static Stage Pending = Stage.None;

        public static Stage Saved => (Stage)PlayerPrefs.GetInt("gn_stage", -1);
        public static bool HasSave => Saved != Stage.None;

        public static void Save(Stage s) { PlayerPrefs.SetInt("gn_stage", (int)s); PlayerPrefs.Save(); }
        public static void ClearSave() { PlayerPrefs.DeleteKey("gn_stage"); PlayerPrefs.Save(); }

        public static void ReloadAt(Stage s)
        {
            Pending = s;
            Time.timeScale = 1f;
            GameEvents.Clear();
            var scene = SceneManager.GetActiveScene();
            if (scene.buildIndex >= 0) SceneManager.LoadScene(scene.buildIndex);
            else SceneManager.LoadScene(scene.name);
        }
    }
}
