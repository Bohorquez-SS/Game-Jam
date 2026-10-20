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

        void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').Contains("_GoodNight/Art/Textures/")) return;
            var ti = assetImporter as TextureImporter;
            if (ti == null) return;
            ti.maxTextureSize = 1024;
            ti.anisoLevel = 2;
            string f = Path.GetFileNameWithoutExtension(assetPath);
            if (f == "grain" || f == "scanlines" || f == "crt_static")
            {
                ti.mipmapEnabled = false; ti.wrapMode = UnityEngine.TextureWrapMode.Repeat; // el HUD los desplaza: deben repetirse
            }
            else if (f == "vignette" || f.StartsWith("note_") || f.StartsWith("receipt") || f.StartsWith("photo") || f.StartsWith("tape_label") || f == "phone_wallpaper" || f == "blueprint" || f == "kid_drawing" || f == "calendar")
            {
                ti.mipmapEnabled = f != "vignette"; ti.alphaIsTransparency = true;
                ti.wrapMode = UnityEngine.TextureWrapMode.Clamp;
            }
        }
    }
}
