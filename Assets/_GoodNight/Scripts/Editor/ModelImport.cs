using System.IO;
using UnityEditor;

namespace GNight.EditorTools
{
    /// <summary>Ajustes de importación para los FBX de Blender: escala real, ejes horneados, animación Legacy solo en los personajes.</summary>
    public class ModelImport : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            if (!assetPath.Replace('\\', '/').Contains("_GoodNight/Art/Models/")) return;
            var mi = assetImporter as ModelImporter;
            if (mi == null) return;
            string f = Path.GetFileNameWithoutExtension(assetPath);
            bool rig = f == "entity" || f == "double";
            mi.globalScale = 1f;
            mi.useFileScale = true;
            mi.bakeAxisConversion = true;
            mi.importCameras = false;
            mi.importLights = false;
            mi.importBlendShapes = false;
            mi.importVisibility = false;
            mi.isReadable = true;   // MeshCollider exactos (muebles huecos: cajones, botiquín, caja de herramientas)
            mi.addCollider = false;
            mi.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            mi.animationType = rig ? ModelImporterAnimationType.Legacy : ModelImporterAnimationType.None;
            mi.importAnimation = rig;
            mi.generateAnimations = ModelImporterGenerateAnimations.GenerateAnimations;
        }
        // Las texturas las gestiona GoodNightTexturePostprocessor (URPSetup.cs).
    }
}
