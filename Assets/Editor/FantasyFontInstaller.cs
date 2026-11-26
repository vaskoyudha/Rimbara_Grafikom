using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Rimbara.Editor
{
    public static class FantasyFontInstaller
    {
        private const string OutDir = "Assets/External/Fonts/TMP";

        [MenuItem("RIMBARA/UI/Install fantasy fonts")]
        public static void Install()
        {
            Create("Assets/External/Fonts/Cinzel-Bold.ttf", OutDir + "/Cinzel-Bold SDF.asset");
            Create("Assets/External/Fonts/Cinzel-Regular.ttf", OutDir + "/Cinzel-Regular SDF.asset");
            Create("Assets/External/Fonts/EBGaramond-Regular.ttf", OutDir + "/EBGaramond-Regular SDF.asset");
            Create("Assets/External/Fonts/EBGaramond-SemiBold.ttf", OutDir + "/EBGaramond-SemiBold SDF.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void Create(string ttfPath, string assetPath)
        {
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath) != null)
            {
                Debug.Log("Fantasy font exists: " + assetPath);
                return;
            }

            Font font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (font == null)
            {
                Debug.LogError("Fantasy font source missing: " + ttfPath);
                return;
            }

            TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(
                font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
            if (asset == null)
            {
                Debug.LogError("Could not create TMP font asset from " + ttfPath);
                return;
            }

            asset.name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.CreateAsset(asset, assetPath);

            if (asset.atlasTextures != null && asset.atlasTextures.Length > 0 && asset.atlasTextures[0] != null)
            {
                asset.atlasTextures[0].name = asset.name + " Atlas";
                AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
            }
            if (asset.material != null)
            {
                asset.material.name = asset.name + " Material";
                AssetDatabase.AddObjectToAsset(asset.material, asset);
            }

            EditorUtility.SetDirty(asset);
            Debug.Log("Fantasy font created: " + assetPath);
        }
    }
}
