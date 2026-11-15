using System;
using System.Linq;
using Rimbara.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rimbara.Editor
{
    public static class IndonesianSceneLocalization
    {
        [MenuItem("RIMBARA/Storyline/Terapkan Bahasa Indonesia")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode before localizing the authored scene.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.OpenScene(StorylineBuilder.ScenePath, OpenSceneMode.Single);
            RimbaraHud hud = UnityEngine.Object.FindFirstObjectByType<RimbaraHud>();
            IndonesianText.ApplyStaticLabels(hud.View.Canvas.transform);
            foreach (TextMeshPro text in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<TextMeshPro>(true)))
            {
                text.text = IndonesianText.WorldLabel(text.text);
                EditorUtility.SetDirty(text);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Verify();
        }

        public static void Verify()
        {
            RimbaraHud hud = UnityEngine.Object.FindFirstObjectByType<RimbaraHud>();
            TMP_Text[] labels = hud.View.Canvas.GetComponentsInChildren<TMP_Text>(true);
            string[] mismatches = labels.Where(t => IndonesianText.Labels.TryGetValue(t.name, out string expected) && t.text != expected)
                .Select(t => t.name).ToArray();
            if (mismatches.Length > 0) throw new InvalidOperationException("Untranslated scene UI: " + string.Join(", ", mismatches));
            if (labels.Any(t => !IndonesianText.Labels.ContainsKey(t.name) && !string.IsNullOrWhiteSpace(t.text)))
                throw new InvalidOperationException("A nonempty UI label has no Indonesian translation entry.");
            var world = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<TextMeshPro>(true)).ToArray();
            if (world.Any(t => t.text != IndonesianText.WorldLabel(t.text)))
                throw new InvalidOperationException("Untranslated world marker in authored scene.");
            ProjectBuilder.WriteArtifact("indonesian-verification.json", "{\"scene\":\"" + StorylineBuilder.ScenePath
                + "\",\"language\":\"id-ID\",\"uiLabels\":" + labels.Length + ",\"worldLabels\":" + world.Length
                + ",\"untranslated\":0,\"result\":\"passed\"}");
        }
    }
}
