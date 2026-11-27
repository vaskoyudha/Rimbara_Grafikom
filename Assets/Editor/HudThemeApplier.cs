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
        [MenuItem("RIMBARA/UI/Apply Fantastic theme to open scene")]
        public static void ApplyToOpenScene()
        {
            UiKit kit = AssetDatabase.LoadAssetAtPath<UiKit>(FantasticThemeBuilder.AssetPath);
            if (kit == null)
                throw new InvalidOperationException("Run RIMBARA/UI/Build Fantastic theme first.");

            Scene scene = SceneManager.GetActiveScene();
            Canvas canvas = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
                .FirstOrDefault(c => c.name == "UI - RIMBARA");
            if (canvas == null)
                throw new InvalidOperationException("Open a scene that contains the UI - RIMBARA canvas.");

            HudTheme.Apply(canvas, kit);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Fantastic theme applied to {scene.name}.");
        }

        [MenuItem("RIMBARA/UI/Preview Coordinator Dialogue")]
        public static void PreviewCoordinatorDialogue()
        {
            if (!Application.isPlaying) return;
            var flow = UnityEngine.Object.FindFirstObjectByType<Rimbara.Mission.GameFlow>();
            var hud = UnityEngine.Object.FindFirstObjectByType<DisasterHud>();
            var opening = UnityEngine.Object.FindFirstObjectByType<OpeningDirector>();
            if (opening != null) opening.MarkCompleted();
            if (flow != null)
            {
                flow.Begin();
                flow.EndOpeningCinematic();
            }
            if (hud != null)
            {
                hud.OpenCoordinatorDialogue();
            }
        }

        [MenuItem("RIMBARA/UI/Preview Gameplay HUD")]
        public static void PreviewGameplayHud()
        {
            if (!Application.isPlaying) return;
            var flow = UnityEngine.Object.FindFirstObjectByType<Rimbara.Mission.GameFlow>();
            var opening = UnityEngine.Object.FindFirstObjectByType<OpeningDirector>();
            if (opening != null) opening.MarkCompleted();
            if (flow != null)
            {
                flow.Begin();
                flow.EndOpeningCinematic();
            }
        }

        [MenuItem("RIMBARA/UI/Clear Saved Game")]
        public static void ClearSavedGame()
        {
            Rimbara.Mission.GameSaveSystem.Clear();
            Debug.Log("Cleared saved RIMBARA progress.");
        }
    }
}
