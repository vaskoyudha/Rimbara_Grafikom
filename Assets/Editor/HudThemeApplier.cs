using System;
using System.Linq;
using Rimbara.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rimbara.Editor
{
    public static class HudThemeApplier
    {
        [MenuItem("RIMBARA/UI/Apply Kingsforge theme to open scene")]
        public static void ApplyToOpenScene()
        {
            UiKit kit = AssetDatabase.LoadAssetAtPath<UiKit>(KingsforgeThemeBuilder.AssetPath);
            if (kit == null)
                throw new InvalidOperationException("Run RIMBARA/UI/Build Kingsforge theme first.");

            Scene scene = SceneManager.GetActiveScene();
            Canvas canvas = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
                .FirstOrDefault(c => c.name == "UI - RIMBARA");
            if (canvas == null)
                throw new InvalidOperationException("Open a scene that contains the UI - RIMBARA canvas.");

            HudTheme.Apply(canvas, kit);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Kingsforge theme applied to {scene.name}.");
        }
    }
}
