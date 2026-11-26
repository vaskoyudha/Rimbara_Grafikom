using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rimbara.Editor
{
    public static class KingsforgeImportReport
    {
        private static readonly string[] Roots =
        {
            "Assets/ThirdParty/Kingsforge",
            "Assets/Kingsforge",
            "Assets/Kingsforge Fantasy RPG UI Kit"
        };

        public static string DetectRoot()
        {
            foreach (string root in Roots)
                if (AssetDatabase.IsValidFolder(root)) return root;
            throw new InvalidOperationException(
                "Kingsforge kit not found. Import the .unitypackage, then rerun RIMBARA/UI/Report Kingsforge Import.");
        }

        [MenuItem("RIMBARA/UI/Report Kingsforge Import")]
        public static void Report()
        {
            string root = DetectRoot();
            string[] prefabs = AssetDatabase.FindAssets("t:Prefab", new[] { root })
                .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).ToArray();
            string[] sprites = AssetDatabase.FindAssets("t:Sprite", new[] { root })
                .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).ToArray();
            string[] fonts = AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { root })
                .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).ToArray();
            string json = "{\"root\":\"" + root + "\",\"prefabs\":[" + Join(prefabs)
                + "],\"sprites\":[" + Join(sprites) + "],\"fonts\":[" + Join(fonts) + "]}";
            ProjectBuilder.WriteArtifact("kingsforge-import.json", json);
            Debug.Log($"Kingsforge root={root} prefabs={prefabs.Length} sprites={sprites.Length} fonts={fonts.Length}");
        }

        private static string Join(string[] values) =>
            string.Join(",", values.Select(v => "\"" + v.Replace("\\", "/") + "\""));
    }
}
