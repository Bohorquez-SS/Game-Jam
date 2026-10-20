using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GNight.EditorTools
{
    /// <summary>
    /// Crea y asigna un URP Asset (por reflexión, para compilar aunque el paquete URP no esté instalado).
    /// Si URP no está disponible, el juego funciona igual en Built-in.
    /// </summary>
    public static class URPSetup
    {
        const string Dir = BuildKit.Root + "/Settings";

        static Type FindType(string fullName)
        {
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = a.GetType(fullName, false);
                if (t != null) return t;
            }
            return null;
        }

        [MenuItem("Tools/GOOD NIGHT/Configurar URP", false, 20)]
        public static void Menu() { string m; TryConfigure(out m); Debug.Log("[GOOD NIGHT] " + m); }

        public static bool TryConfigure(out string msg)
        {
            try
            {
                if (GraphicsSettings.defaultRenderPipeline != null) { msg = "URP ya estaba configurado."; return true; }
                var assetType = FindType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset");
                var rendererType = FindType("UnityEngine.Rendering.Universal.UniversalRendererData");
                if (assetType == null || rendererType == null) { msg = "Paquete URP no instalado: se usa Built-in."; return false; }

                BuildKit.EnsureFolder(Dir);
                var rd = ScriptableObject.CreateInstance(rendererType);
                var ppType = FindType("UnityEngine.Rendering.Universal.PostProcessData");
                var ppField = rendererType.GetField("postProcessData", BindingFlags.Public | BindingFlags.Instance);
                var getDefault = ppType != null ? ppType.GetMethod("GetDefaultPostProcessData", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) : null;
                if (ppField != null && getDefault != null) ppField.SetValue(rd, getDefault.Invoke(null, null));
                AssetDatabase.DeleteAsset(Dir + "/GN_URP_Renderer.asset");
                AssetDatabase.CreateAsset(rd, Dir + "/GN_URP_Renderer.asset");

                var create = assetType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m => m.Name == "Create" && m.GetParameters().Length == 1);
                if (create == null) { msg = "No se encontró UniversalRenderPipelineAsset.Create."; return false; }
                var asset = create.Invoke(null, new object[] { rd }) as RenderPipelineAsset;
                if (asset == null) { msg = "No se pudo crear el URP Asset."; return false; }
                AssetDatabase.DeleteAsset(Dir + "/GN_URP.asset");
                AssetDatabase.CreateAsset(asset, Dir + "/GN_URP.asset");

                var so = new SerializedObject(asset);
                SetInt(so, "m_AdditionalLightsRenderingMode", 1);
                SetInt(so, "m_AdditionalLightsPerObjectLimit", 8);
                SetBool(so, "m_AdditionalLightShadowsSupported", true);
                SetBool(so, "m_MainLightShadowsSupported", true);
                SetBool(so, "m_SoftShadowsSupported", true);
                SetFloat(so, "m_ShadowDistance", 30f);
                SetBool(so, "m_SupportsHDR", false);
                so.ApplyModifiedPropertiesWithoutUndo();

                GraphicsSettings.defaultRenderPipeline = asset;
                int current = QualitySettings.GetQualityLevel();
                for (int i = 0; i < QualitySettings.names.Length; i++) { QualitySettings.SetQualityLevel(i, false); QualitySettings.renderPipeline = asset; }
                QualitySettings.SetQualityLevel(current, false);
                AssetDatabase.SaveAssets();
                msg = "URP configurado (Assets/_GoodNight/Settings/GN_URP.asset).";
                return true;
            }
            catch (Exception e)
            {
                msg = "No se pudo configurar URP automáticamente (" + e.Message + "). Se usa el pipeline actual.";
                return false;
            }
        }

        /// <summary>
        /// Calidad de imagen (se aplica en cada construcción, aunque URP ya existiera): resolución interna 100 %, sin MSAA
        /// (la cámara usa SMAA), sombras 4096 con 2 cascadas y suaves en calidad alta, sin reescalado.
        /// </summary>
        [MenuItem("Tools/GOOD NIGHT/Ajustar calidad URP", false, 21)]
        public static void Tune()
        {
            var asset = GraphicsSettings.defaultRenderPipeline;
            if (asset == null) return;
            var so = new SerializedObject(asset);
            SetFloat(so, "m_RenderScale", 1f);
            SetInt(so, "m_MSAA", 1);
            SetInt(so, "m_UpscalingFilter", 0);
            SetInt(so, "m_MainLightShadowmapResolution", 4096);
            SetInt(so, "m_AdditionalLightsShadowmapResolution", 4096);
            SetInt(so, "m_AdditionalLightsShadowResolutionTierHigh", 1024);
            SetInt(so, "m_AdditionalLightsShadowResolutionTierMedium", 512);
            SetInt(so, "m_ShadowCascadeCount", 2);
            SetFloat(so, "m_ShadowDistance", 25f);
            SetBool(so, "m_SoftShadowsSupported", true);
            SetInt(so, "m_SoftShadowQuality", 3);
            SetInt(so, "m_AdditionalLightsPerObjectLimit", 8);
            SetBool(so, "m_SupportsHDR", false);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            QualitySettings.globalTextureMipmapLimit = 0;
            Debug.Log("[GOOD NIGHT] Calidad URP ajustada (render 100 %, SMAA, sombras 4096).");
        }

        static void SetInt(SerializedObject so, string n, int v) { var p = so.FindProperty(n); if (p != null) p.intValue = v; }
        static void SetBool(SerializedObject so, string n, bool v) { var p = so.FindProperty(n); if (p != null) p.boolValue = v; }
        static void SetFloat(SerializedObject so, string n, float v) { var p = so.FindProperty(n); if (p != null) p.floatValue = v; }
    }

    /// <summary>
    /// Texturas de GOOD NIGHT (única regla de importación). v3: filtro trilineal + mipmaps + anisotrópico (antes Point sin mipmaps,
    /// que producía el aspecto pixelado/"Minecraft" y el parpadeo a distancia). Los *_n se importan como mapas normales.
    /// </summary>
    public class GoodNightTexturePostprocessor : AssetPostprocessor
    {
        public override uint GetVersion() => 3;

        void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').Contains("_GoodNight/Art/Textures")) return;
            var ti = (TextureImporter)assetImporter;
            string f = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            bool normal = f.EndsWith("_n");
            bool hud = f == "grain" || f == "scanlines" || f == "crt_static";
            bool doc = f == "vignette" || f.StartsWith("note_") || f.StartsWith("receipt") || f.StartsWith("photo") || f.StartsWith("tape_label")
                       || f.StartsWith("face_") || f == "phone_wallpaper" || f == "blueprint" || f == "kid_drawing" || f == "calendar" || f == "poster"
                       || f == "clock_face" || f == "screen_desktop" || f == "monitor_off" || f.StartsWith("entity_face");
            ti.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            ti.filterMode = FilterMode.Trilinear;
            ti.mipmapEnabled = !hud && f != "vignette";
            ti.anisoLevel = hud ? 0 : 8;
            ti.npotScale = TextureImporterNPOTScale.ToNearest;
            ti.maxTextureSize = 2048;
            ti.textureCompression = doc ? TextureImporterCompression.Uncompressed : TextureImporterCompression.CompressedHQ;
            ti.alphaIsTransparency = !normal;
            ti.wrapMode = doc ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
        }
    }
}
