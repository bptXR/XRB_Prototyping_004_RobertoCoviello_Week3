#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEditor.Rendering.MaterialUpgrader;
 
namespace BearingPoint.Utility.Helpers.Tools.Editor
{
    public static class URPMaterialUpgrader
    {
        [MenuItem("Tools/Universal Render Pipeline/Upgrade Project Materials to UniversalRP Materials")]
        private static void UpgradeProjectMaterials()
        {
            List<MaterialUpgrader> upgraders = new List<MaterialUpgrader>();
            GetUpgraders(ref upgraders);
 
            HashSet<string> shaderNamesToIgnore = new HashSet<string>();
            GetShaderNamesToIgnore(ref shaderNamesToIgnore);
 
            MaterialUpgrader.UpgradeProjectFolder(upgraders, shaderNamesToIgnore, "Upgrade to URP Materials", MaterialUpgrader.UpgradeFlags.LogMessageWhenNoUpgraderFound);
        }
 
        [MenuItem("Tools/Universal Render Pipeline/Upgrade Selected Materials to UniversalRP Materials")]
        private static void UpgradeSelectedMaterials()
        {
            List<MaterialUpgrader> upgraders = new List<MaterialUpgrader>();
            GetUpgraders(ref upgraders);
 
            HashSet<string> shaderNamesToIgnore = new HashSet<string>();
            GetShaderNamesToIgnore(ref shaderNamesToIgnore);
         
            MaterialUpgrader.UpgradeSelection(upgraders, shaderNamesToIgnore, "Upgrade to URP Materials", MaterialUpgrader.UpgradeFlags.LogMessageWhenNoUpgraderFound);
        }
 
        [MenuItem("Tools/Universal Render Pipeline/Upgrade Materials From Selected Folder to UniversalRP Materials")]
        private static void UpgradeSelectedMaterialsFromFolder()
        {
            List<MaterialUpgrader> upgraders = new List<MaterialUpgrader>();
            GetUpgraders(ref upgraders);
 
            HashSet<string> shaderNamesToIgnore = new HashSet<string>();
            GetShaderNamesToIgnore(ref shaderNamesToIgnore);
         
            if(TryGetSelectedDirectory(out string directoryPath))
                UpgradeMaterialFromFolder(upgraders, shaderNamesToIgnore, "Upgrade to URP Materials", directoryPath, MaterialUpgrader.UpgradeFlags.LogMessageWhenNoUpgraderFound);
            else EditorUtility.DisplayDialog(DialogText.title, "Folder is not selected.", DialogText.ok);
        }
 
#region Custom Material Upgraders
        private class StandardSimpleLightingUpgrader : MaterialUpgrader
        {
            public StandardSimpleLightingUpgrader(string oldShaderName, UpgradeParams upgradeParams)
            {
                if (oldShaderName == null)
                    throw new ArgumentNullException("oldShaderName");
 
                RenameShader(oldShaderName, ShaderUtils.GetShaderPath(ShaderPathID.SimpleLit), UpdateMaterialKeywords);
 
                SetFloat("_Surface", (float)upgradeParams.surfaceType);
                SetFloat("_Blend", (float)upgradeParams.blendMode);
                SetFloat("_AlphaClip", upgradeParams.alphaClip ? 1 : 0);
                SetFloat("_SpecularHighlights", (float)upgradeParams.specularSource);
                SetFloat("_SmoothnessSource", (float)upgradeParams.smoothnessSource);
 
                RenameTexture("_MainTex", "_BaseMap");
                RenameColor("_Color", "_BaseColor");
                RenameFloat("_Shininess", "_Smoothness");
 
                if (oldShaderName.Contains("Legacy Shaders/Self-Illumin"))
                {
                    RenameTexture("_Illum", "_EmissionMap");
                    RemoveTexture("_Illum");
                    SetColor("_EmissionColor", Color.white);
                }
            }
 
            public static void UpdateMaterialKeywords(Material material)
            {
                if (material == null)
                    throw new ArgumentNullException("material");
 
                material.shaderKeywords = null;
                BaseShaderGUI.SetupMaterialBlendMode(material);
                UpdateMaterialSpecularSource(material);
                CoreUtils.SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap"));
 
                // A material's GI flag internally keeps track of whether emission is enabled at all, it's enabled but has no effect
                // or is enabled and may be modified at runtime. This state depends on the values of the current flag and emissive color.
                // The fixup routine makes sure that the material is in the correct state if/when changes are made to the mode or color.
                MaterialEditor.FixupEmissiveFlag(material);
                bool shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
                CoreUtils.SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);
            }
 
            private static void UpdateMaterialSpecularSource(Material material)
            {
                SpecularSource specSource = (SpecularSource)material.GetFloat("_SpecSource");
                if (specSource == SpecularSource.NoSpecular)
                {
                    CoreUtils.SetKeyword(material, "_SPECGLOSSMAP", false);
                    CoreUtils.SetKeyword(material, "_SPECULAR_COLOR", false);
                    CoreUtils.SetKeyword(material, "_GLOSSINESS_FROM_BASE_ALPHA", false);
                }
                else
                {
                    SmoothnessSource glossSource = (SmoothnessSource)material.GetFloat("_SmoothnessSource");
                    bool hasGlossMap = material.GetTexture("_SpecGlossMap");
                    CoreUtils.SetKeyword(material, "_SPECGLOSSMAP", hasGlossMap);
                    CoreUtils.SetKeyword(material, "_SPECULAR_COLOR", !hasGlossMap);
                    CoreUtils.SetKeyword(material, "_GLOSSINESS_FROM_BASE_ALPHA", glossSource == SmoothnessSource.BaseAlpha);
                }
            }
        }
 
        private class SpeedTreeUpgrader : MaterialUpgrader
        {
            internal SpeedTreeUpgrader(string oldShaderName) =>
                RenameShader(oldShaderName, ShaderUtils.GetShaderPath(ShaderPathID.SpeedTree7));
        }
 
        private class SpeedTreeBillboardUpgrader : MaterialUpgrader
        {
            internal SpeedTreeBillboardUpgrader(string oldShaderName) =>
                RenameShader(oldShaderName, ShaderUtils.GetShaderPath(ShaderPathID.SpeedTree7Billboard));
        }
 
        private class SpeedTree8Upgrader : MaterialUpgrader
        {
            internal SpeedTree8Upgrader(string oldShaderName) =>
                RenameShader(oldShaderName, ShaderUtils.GetShaderPath(ShaderPathID.SpeedTree8));
        }
#endregion
     
#region Implementation
        private static void GetShaderNamesToIgnore(ref HashSet<string> shadersToIgnore)
        {
            shadersToIgnore.Add("Universal Render Pipeline/Baked Lit");
            shadersToIgnore.Add("Universal Render Pipeline/Lit");
            shadersToIgnore.Add("Universal Render Pipeline/Particles/Lit");
            shadersToIgnore.Add("Universal Render Pipeline/Particles/Simple Lit");
            shadersToIgnore.Add("Universal Render Pipeline/Particles/Unlit");
            shadersToIgnore.Add("Universal Render Pipeline/Simple Lit");
            shadersToIgnore.Add("Universal Render Pipeline/Nature/SpeedTree7");
            shadersToIgnore.Add("Universal Render Pipeline/Nature/SpeedTree7 Billboard");
            shadersToIgnore.Add("Universal Render Pipeline/Nature/SpeedTree8");
            shadersToIgnore.Add("Universal Render Pipeline/2D/Sprite-Lit-Default");
            shadersToIgnore.Add("Universal Render Pipeline/Terrain/Lit");
            shadersToIgnore.Add("Universal Render Pipeline/Unlit");
            shadersToIgnore.Add("Sprites/Default");
        }
 
        private static void GetUpgraders(ref List<MaterialUpgrader> upgraders)
        {
            /////////////////////////////////////
            //     Unity Standard Upgraders    //
            /////////////////////////////////////
            upgraders.Add(new StandardUpgrader("Standard"));
            upgraders.Add(new StandardUpgrader("Standard (Specular setup)"));
 
            /////////////////////////////////////
            // Legacy Shaders upgraders         /
            /////////////////////////////////////
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Diffuse", SupportedUpgradeParams.diffuseOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Diffuse Detail", SupportedUpgradeParams.diffuseOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Diffuse Fast", SupportedUpgradeParams.diffuseOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Specular", SupportedUpgradeParams.specularOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Bumped Diffuse", SupportedUpgradeParams.diffuseOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Bumped Specular", SupportedUpgradeParams.specularOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Parallax Diffuse", SupportedUpgradeParams.diffuseOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Parallax Specular", SupportedUpgradeParams.specularOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/VertexLit", SupportedUpgradeParams.specularOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Transparent/Cutout/VertexLit", SupportedUpgradeParams.specularAlphaCutout));
 
            // Reflective
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Reflective/Bumped Diffuse", SupportedUpgradeParams.diffuseCubemap));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Reflective/Bumped Specular", SupportedUpgradeParams.specularCubemap));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Reflective/Bumped Unlit", SupportedUpgradeParams.diffuseCubemap));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Reflective/Bumped VertexLit", SupportedUpgradeParams.diffuseCubemap));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Reflective/Diffuse", SupportedUpgradeParams.diffuseCubemap));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Reflective/Specular", SupportedUpgradeParams.specularCubemap));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Reflective/VertexLit", SupportedUpgradeParams.diffuseCubemap));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Reflective/Parallax Diffuse", SupportedUpgradeParams.diffuseCubemap));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Reflective/Parallax Specular", SupportedUpgradeParams.specularCubemap));
 
            // Self-Illum upgrader
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Self-Illumin/Diffuse", SupportedUpgradeParams.diffuseOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Self-Illumin/Bumped Diffuse", SupportedUpgradeParams.diffuseOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Self-Illumin/Parallax Diffuse", SupportedUpgradeParams.diffuseOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Self-Illumin/Specular", SupportedUpgradeParams.specularOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Self-Illumin/Bumped Specular", SupportedUpgradeParams.specularOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Self-Illumin/Parallax Specular", SupportedUpgradeParams.specularOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Self-Illumin/VertexLit", SupportedUpgradeParams.specularOpaque));
 
            // Alpha Blended
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Transparent/Diffuse", SupportedUpgradeParams.diffuseAlpha));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Transparent/Specular", SupportedUpgradeParams.specularAlpha));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Transparent/Bumped Diffuse", SupportedUpgradeParams.diffuseAlpha));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Transparent/Bumped Specular", SupportedUpgradeParams.specularAlpha));
 
            // Cutout
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Transparent/Cutout/Diffuse", SupportedUpgradeParams.diffuseAlphaCutout));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Transparent/Cutout/Specular", SupportedUpgradeParams.specularAlphaCutout));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Transparent/Cutout/Bumped Diffuse", SupportedUpgradeParams.diffuseAlphaCutout));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Transparent/Cutout/Bumped Specular", SupportedUpgradeParams.specularAlphaCutout));
 
            // Lightmapped
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Lightmapped/Diffuse", SupportedUpgradeParams.diffuseOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Lightmapped/Specular", SupportedUpgradeParams.specularOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Lightmapped/VertexLit", SupportedUpgradeParams.specularOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Lightmapped/Bumped Diffuse", SupportedUpgradeParams.diffuseOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Legacy Shaders/Lightmapped/Bumped Specular", SupportedUpgradeParams.specularOpaque));
 
            /////////////////////////////////////
            // Sprites Upgraders
            /////////////////////////////////////
            upgraders.Add(new StandardSimpleLightingUpgrader("Sprites/Diffuse", SupportedUpgradeParams.diffuseAlpha));
 
            /////////////////////////////////////
            // UI Upgraders
            /////////////////////////////////////
            upgraders.Add(new StandardSimpleLightingUpgrader("UI/Lit/Bumped", SupportedUpgradeParams.diffuseAlphaCutout));
            upgraders.Add(new StandardSimpleLightingUpgrader("UI/Lit/Detail", SupportedUpgradeParams.diffuseAlphaCutout));
            upgraders.Add(new StandardSimpleLightingUpgrader("UI/Lit/Refraction", SupportedUpgradeParams.diffuseAlphaCutout));
            upgraders.Add(new StandardSimpleLightingUpgrader("UI/Lit/Refraction Detail", SupportedUpgradeParams.diffuseAlphaCutout));
            upgraders.Add(new StandardSimpleLightingUpgrader("UI/Lit/Transparent", SupportedUpgradeParams.diffuseAlpha));
 
 
            /////////////////////////////////////
            // Mobile Upgraders                 /
            /////////////////////////////////////
            upgraders.Add(new StandardSimpleLightingUpgrader("Mobile/Diffuse", SupportedUpgradeParams.diffuseOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Mobile/Bumped Specular", SupportedUpgradeParams.specularOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Mobile/Bumped Specular (1 Directional Light)", SupportedUpgradeParams.specularOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Mobile/Bumped Diffuse", SupportedUpgradeParams.diffuseOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Mobile/Unlit (Supports Lightmap)", SupportedUpgradeParams.diffuseOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Mobile/VertexLit", SupportedUpgradeParams.specularOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Mobile/VertexLit (Only Directional Lights)", SupportedUpgradeParams.specularOpaque));
            upgraders.Add(new StandardSimpleLightingUpgrader("Mobile/Particles/VertexLit Blended", SupportedUpgradeParams.specularOpaque));
 
            ////////////////////////////////////
            // Terrain Upgraders              //
            ////////////////////////////////////
            upgraders.Add(new TerrainUpgrader("Nature/Terrain/Standard"));
            upgraders.Add(new SpeedTreeUpgrader("Nature/SpeedTree"));
            upgraders.Add(new SpeedTreeBillboardUpgrader("Nature/SpeedTree Billboard"));
            upgraders.Add(new SpeedTree8Upgrader("Nature/SpeedTree8"));
 
            ////////////////////////////////////
            // Particle Upgraders             //
            ////////////////////////////////////
            upgraders.Add(new ParticleUpgrader("Particles/Standard Surface"));
            upgraders.Add(new ParticleUpgrader("Particles/Standard Unlit"));
            upgraders.Add(new ParticleUpgrader("Particles/VertexLit Blended"));
 
            ////////////////////////////////////
            // Autodesk Interactive           //
            ////////////////////////////////////
            upgraders.Add(new AutodeskInteractiveUpgrader("Autodesk Interactive"));
        }
 
        private static void UpgradeMaterialFromFolder(List<MaterialUpgrader> upgraders, HashSet<string> shaderNamesToIgnore, string progressBarName, string directoryPath, UpgradeFlags flags = UpgradeFlags.None)
        {
            string[] materialPaths = AssetDatabase.GetAllAssetPaths()
                .Where(path => path.Contains(directoryPath))
                .Where(path => path.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
                .Where(path => File.Exists(path))
                .ToArray();
             
            int totalMaterialCount = materialPaths.Length;
 
            if(totalMaterialCount == 0)
            {
                EditorUtility.DisplayDialog(DialogText.title, "No Materials in selected folder to update.", DialogText.ok);
                return;
            }
         
            if ((!Application.isBatchMode) && (!EditorUtility.DisplayDialog(DialogText.title, "The upgrade will overwrite materials in your project. " + DialogText.projectBackMessage, DialogText.proceed, DialogText.cancel)))
                return;
         
            int materialIndex = 0;
            foreach (string path in materialPaths)
            {
                materialIndex++;
                if (EditorUtility.DisplayCancelableProgressBar(progressBarName, string.Format("({0} of {1}) {2}", materialIndex, totalMaterialCount, path), (float)materialIndex / (float)totalMaterialCount))
                    break;
 
                Material material = AssetDatabase.LoadMainAssetAtPath(path) as Material;
 
                if (!shaderNamesToIgnore.Contains(material?.shader?.name))
                    Upgrade(material, upgraders, flags);
            }
 
            EditorUtility.ClearProgressBar();
        }
 
        private static bool TryGetSelectedDirectory(out string directoryPath)
        {
            directoryPath = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
            if (string.IsNullOrEmpty(directoryPath)) return false;
 
            if (Directory.Exists(directoryPath)) return true;
         
            directoryPath = null;
            return false;
        }
#endregion
    }
}
#endif