using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rimbara.Editor
{
    public static class FantasticImportReport
    {
        private static readonly string[] Roots =
        {
            "Assets/ThirdParty/FantasticUI",
            "Assets/MyIndie.Games/Fantastic UI",
            "Assets/MyIndieGames/FantasticUI",
            "Assets/Fantastic UI Starter Pack",
            "Assets/FantasticUI"
        };

        public static string DetectRoot()
        {
            foreach (string root in Roots)
                if (AssetDatabase.IsValidFolder(root)) return root;

            // The pack may import under a publisher folder we did not predict.
            // Pick the shallowest folder whose path names the kit.
            string found = AssetDatabase.GetAllAssetPaths()
                .Where(p => p.StartsWith("Assets/") && AssetDatabase.IsValidFolder(p))
                .Where(p => p.IndexOf("fantastic", StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(p => p.Count(c => c == '/'))
                .ThenBy(p => p.Length)
                .FirstOrDefault();
            if (found != null) return found;

            throw new InvalidOperationException(
                "Fantastic UI kit not found. Import the .unitypackage, then rerun RIMBARA/UI/Report Fantastic Import.");
        }

        [MenuItem("RIMBARA/UI/Report Fantastic Import")]
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
            ProjectBuilder.WriteArtifact("fantastic-import.json", json);
            Debug.Log($"Fantastic root={root} prefabs={prefabs.Length} sprites={sprites.Length} fonts={fonts.Length}");
        }

        private static string Join(string[] values) =>
            string.Join(",", values.Select(v => "\"" + v.Replace("\\", "/") + "\""));
    }
}
