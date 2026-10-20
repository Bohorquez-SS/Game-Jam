using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GNight.EditorTools
{
    /// <summary>Build de Windows para jugadores (sin herramientas de depuración): &lt;proyecto&gt;/Build/GOOD_NIGHT.exe</summary>
    public static class WindowsBuild
    {
        [MenuItem("Tools/GOOD NIGHT/Build Windows (jugadores)")]
        public static void Build()
        {
            string dir = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Build");
            Directory.CreateDirectory(dir);
            var opts = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/_GoodNight/Scenes/GoodNight_Main.unity" },
                locationPathName = Path.Combine(dir, "GOOD_NIGHT.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(opts);
            var s = report.summary;
            Debug.Log("[GOOD NIGHT] Build Windows: " + s.result + " · " + (s.totalSize / (1024 * 1024)) + " MB · errores " + s.totalErrors + " · " + opts.locationPathName);
        }
    }
}
