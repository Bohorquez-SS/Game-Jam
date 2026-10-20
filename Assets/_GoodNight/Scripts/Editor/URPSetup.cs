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

        static void SetInt(SerializedObject so, string n, int v) { var p = so.FindProperty(n); if (p != null) p.intValue = v; }
        static void SetBool(SerializedObject so, string n, bool v) { var p = so.FindProperty(n); if (p != null) p.boolValue = v; }
        static void SetFloat(SerializedObject so, string n, float v) { var p = so.FindProperty(n); if (p != null) p.floatValue = v; }
    }

    /// <summary>Texturas de GOOD NIGHT con filtro point (PS1) y sin mipmaps.</summary>
    public class GoodNightTexturePostprocessor : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').Contains("_GoodNight/Art/Textures")) return;
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Default;
            ti.filterMode = FilterMode.Point;
            ti.mipmapEnabled = false;
            ti.wrapMode = TextureWrapMode.Repeat;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.npotScale = TextureImporterNPOTScale.None;
            ti.alphaIsTransparency = true;
            ti.maxTextureSize = 1024;
        }
    }
}
