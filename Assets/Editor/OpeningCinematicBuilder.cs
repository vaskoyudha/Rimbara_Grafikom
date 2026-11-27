using System;
using System.IO;
using System.Linq;
using Rimbara.Interaction;
using Rimbara.Mission;
using Rimbara.Player;
using Rimbara.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Rimbara.Editor
{
    /// <summary>
    /// Installs and verifies the prologue opening cinematic in FromAshes.unity without regenerating the map.
    /// </summary>
    public static class OpeningCinematicBuilder
    {
        private const string RootName = "Opening Cinematic";
        private const string OverlayName = "Opening cinematic overlay";
        private const string SkipName = "Skip opening cinematic";

        [MenuItem("RIMBARA/Storyline/Install opening cinematic")]
        public static void Install()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Scene scene = EditorSceneManager.OpenScene(StorylineBuilder.ScenePath, OpenSceneMode.Single);
            Directory.CreateDirectory("Backups");
            EditorSceneManager.SaveScene(scene, "Backups/FromAshes-before-opening-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".unity", true);

            GameFlow flow = UnityEngine.Object.FindFirstObjectByType<GameFlow>();
            DisasterChapter chapter = UnityEngine.Object.FindFirstObjectByType<DisasterChapter>();
            VolunteerRig rig = UnityEngine.Object.FindFirstObjectByType<VolunteerRig>();
            RimbaraHud hud = UnityEngine.Object.FindFirstObjectByType<RimbaraHud>();
            CoordinatorDialogue coordinator = UnityEngine.Object.FindFirstObjectByType<CoordinatorDialogue>();
            GameObject villager = GameObject.Find("Evacuated villager at relief post");

            if (flow == null || chapter == null || rig == null || hud == null || coordinator == null)
                throw new InvalidOperationException("Missing required FromAshes storyline components.");

            DestroyExisting(scene, RootName);
            DestroyChild(hud.View.Canvas.transform, OverlayName);
            DestroyChild(hud.View.Canvas.transform, SkipName);

            GameObject root = new GameObject(RootName);
            OpeningDirector director = root.AddComponent<OpeningDirector>();
            Transform cameras = new GameObject("01 - Opening camera poses").transform;
            cameras.SetParent(root.transform, false);

            Vector3 villagerPos = villager != null ? villager.transform.position : coordinator.transform.position + new Vector3(9f, 0f, 5f);
            Vector3 coordPos = coordinator.transform.position;
            Vector3 rigPos = rig.transform.position;

            Transform[] poses =
            {
                CreatePose(cameras, "Opening shot 1", new Vector3(-14f, 15f, -4f), new Vector3(4f, 2f, 22f)),
                CreatePose(cameras, "Opening shot 2", villagerPos + new Vector3(-4.2f, 2.3f, -4.5f), villagerPos + Vector3.up * 1.35f),
                CreatePose(cameras, "Opening shot 3", coordPos + new Vector3(-4.5f, 2.5f, -6.5f), coordPos + Vector3.up * 1.3f),
                CreatePose(cameras, "Opening shot 4", rigPos + new Vector3(0f, 3.2f, -5.5f), rigPos + new Vector3(0f, 1.5f, 10f))
            };

            GameObject panel = new GameObject(OverlayName, typeof(RectTransform), typeof(Image));
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.SetParent(hud.View.Canvas.transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image fade = panel.GetComponent<Image>();
            fade.color = Color.clear;
            fade.raycastTarget = true;

            TMP_Text subtitle = new GameObject("Opening subtitle", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
            subtitle.rectTransform.SetParent(rect, false);
            subtitle.rectTransform.anchorMin = new Vector2(0.12f, 0.05f);
            subtitle.rectTransform.anchorMax = new Vector2(0.88f, 0.25f);
            subtitle.rectTransform.offsetMin = Vector2.zero;
            subtitle.rectTransform.offsetMax = Vector2.zero;
            subtitle.font = hud.View.MissionDescription.font;
            subtitle.fontSize = 25f;
            subtitle.fontSizeMin = 18f;
            subtitle.fontSizeMax = 25f;
            subtitle.enableAutoSizing = true;
            subtitle.alignment = TextAlignmentOptions.Center;
            subtitle.color = Color.white;
            subtitle.richText = true;

            Button skip = UnityEngine.Object.Instantiate(hud.View.Resume, hud.View.Canvas.transform);
            skip.name = SkipName;
            RectTransform skipRect = skip.GetComponent<RectTransform>();
            skipRect.anchorMin = skipRect.anchorMax = new Vector2(0.5f, 0.1f);
            skipRect.anchoredPosition = Vector2.zero;
            TMP_Text caption = skip.GetComponentInChildren<TMP_Text>();
            caption.name = "Skip opening cinematic label";
            caption.text = IndonesianText.Labels["Skip opening cinematic label"];
            skip.onClick = new Button.ButtonClickedEvent();

            panel.SetActive(false);
            skip.gameObject.SetActive(false);

            director.Configure(chapter, flow, rig.ViewCamera, panel, fade, subtitle, skip, poses);
            flow.ConfigureOpening(director);
            EditorUtility.SetDirty(director);
            EditorUtility.SetDirty(flow);

            IndonesianText.ApplyStaticLabels(hud.View.Canvas.transform);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            VerifyOpeningCinematic();
            Debug.Log("RIMBARA: Opening cinematic installed and verified in " + StorylineBuilder.ScenePath);
        }

        [MenuItem("RIMBARA/Storyline/Verify opening cinematic")]
        public static void VerifyOpeningCinematic()
        {
            OpeningDirector director = UnityEngine.Object.FindFirstObjectByType<OpeningDirector>(FindObjectsInactive.Include);
            GameFlow flow = UnityEngine.Object.FindFirstObjectByType<GameFlow>(FindObjectsInactive.Include);
            if (director == null) throw new InvalidOperationException("Missing OpeningDirector in scene.");
            if (flow == null || flow.Opening != director) throw new InvalidOperationException("GameFlow.Opening is not wired to OpeningDirector.");

            SerializedObject so = new SerializedObject(director);
            foreach (string field in new[] { "chapter", "flow", "cameraView", "overlay", "fade", "subtitle", "skip" })
            {
                if (so.FindProperty(field).objectReferenceValue == null)
                    throw new InvalidOperationException("Missing OpeningDirector reference: " + field);
            }
            if (so.FindProperty("cameraPoses").arraySize != 4)
                throw new InvalidOperationException("OpeningDirector requires four camera poses.");
        }

        private static Transform CreatePose(Transform parent, string name, Vector3 position, Vector3 lookTarget)
        {
            Transform pose = new GameObject(name).transform;
            pose.SetParent(parent, false);
            pose.position = position;
            pose.LookAt(lookTarget);
            return pose;
        }

        private static void DestroyExisting(Scene scene, string rootName)
        {
            foreach (GameObject existing in scene.GetRootGameObjects().Where(g => g.name == rootName).ToArray())
                UnityEngine.Object.DestroyImmediate(existing);
        }

        private static void DestroyChild(Transform parent, string childName)
        {
            Transform existing = parent.Find(childName);
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);
        }
    }
}
