using System;
using System.IO;
using System.Linq;
using Rimbara.Interaction;
using Rimbara.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Rimbara.Editor
{
    /// <summary>
    /// Captures a studio bust portrait of Bima Pratama (Coordinator) from the authored 3D model
    /// and installs the portrait frame inside the Coordinator dialogue panel.
    /// </summary>
    public static class CoordinatorPortraitBuilder
    {
        public const string PortraitPath = "Assets/Art/UI/CoordinatorPortrait.png";

        [MenuItem("RIMBARA/Storyline/Install coordinator portrait")]
        public static void Install()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Scene scene = EditorSceneManager.OpenScene(StorylineBuilder.ScenePath, OpenSceneMode.Single);
            CoordinatorDialogue coordinator = UnityEngine.Object.FindFirstObjectByType<CoordinatorDialogue>(FindObjectsInactive.Include);
            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            GameObject dialoguePanel = canvas == null ? null : canvas.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => t.name == "Coordinator dialogue")?.gameObject;
            UiKit kit = AssetDatabase.LoadAssetAtPath<UiKit>(FantasticThemeBuilder.AssetPath);
            if (coordinator == null || dialoguePanel == null || kit == null)
                throw new InvalidOperationException("Missing CoordinatorDialogue, Coordinator dialogue panel, or UiKit.");

            Directory.CreateDirectory("Assets/Art/UI");
            CaptureStudioPortrait(coordinator.transform, PortraitPath);
            AssetDatabase.ImportAsset(PortraitPath, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(PortraitPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            Sprite portraitSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PortraitPath);
            if (portraitSprite == null)
                throw new InvalidOperationException("Failed to load imported Coordinator portrait sprite.");

            Transform existing = dialoguePanel.transform.Find("Dialogue portrait frame");
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);

            var frameGo = new GameObject("Dialogue portrait frame", typeof(RectTransform), typeof(Image));
            frameGo.layer = 5;
            frameGo.transform.SetParent(dialoguePanel.transform, false);
            frameGo.transform.SetAsFirstSibling();

            RectTransform frameRect = frameGo.GetComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0f, 0.5f);
            frameRect.anchorMax = new Vector2(0f, 0.5f);
            frameRect.pivot = new Vector2(0f, 0.5f);
            frameRect.anchoredPosition = new Vector2(36f, 0f);
            frameRect.sizeDelta = new Vector2(216f, 216f);

            Image frameImage = frameGo.GetComponent<Image>();
            HudTheme.ApplyPanel(frameImage, kit, HudSurface.Inset);
            frameImage.raycastTarget = false;

            var portraitGo = new GameObject("Dialogue portrait", typeof(RectTransform), typeof(Image));
            portraitGo.layer = 5;
            portraitGo.transform.SetParent(frameGo.transform, false);
            RectTransform portraitRect = portraitGo.GetComponent<RectTransform>();
            portraitRect.anchorMin = Vector2.zero;
            portraitRect.anchorMax = Vector2.one;
            portraitRect.offsetMin = new Vector2(10f, 10f);
            portraitRect.offsetMax = new Vector2(-10f, -10f);

            Image portraitImage = portraitGo.GetComponent<Image>();
            portraitImage.sprite = portraitSprite;
            portraitImage.type = Image.Type.Simple;
            portraitImage.preserveAspect = true;
            portraitImage.color = Color.white;
            portraitImage.raycastTarget = false;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("RIMBARA: Installed Coordinator portrait frame and sprite at " + PortraitPath);
        }

        private static void CaptureStudioPortrait(Transform coordinatorRoot, string outputPath)
        {
            const int size = 512;
            Rimbara.World.HumanNpcVisual visual = coordinatorRoot.GetComponentInChildren<Rimbara.World.HumanNpcVisual>(true);
            Transform subject = visual != null ? visual.transform : coordinatorRoot;
            SkinnedMeshRenderer[] skins = subject.GetComponentsInChildren<SkinnedMeshRenderer>(false);
            Bounds bounds = new Bounds(subject.position + Vector3.up * 1.35f, Vector3.one * 0.6f);
            if (skins.Length > 0)
            {
                bounds = skins[0].bounds;
                for (int i = 1; i < skins.Length; i++) bounds.Encapsulate(skins[i].bounds);
            }

            Vector3 headFocus = new Vector3(bounds.center.x, bounds.min.y + bounds.size.y * 0.86f, bounds.center.z);
            Vector3 forward = subject.forward.sqrMagnitude > 0.01f ? subject.forward.normalized : Vector3.forward;

            var camGo = new GameObject("TempPortraitCamera");
            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.14f, 0.09f, 0.06f, 1f);
            cam.cullingMask = 1;
            cam.fieldOfView = 24f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 20f;
            camGo.transform.position = headFocus + forward * 0.92f + subject.right * 0.10f + Vector3.up * 0.03f;
            camGo.transform.LookAt(headFocus + Vector3.up * 0.01f);

            var keyGo = new GameObject("TempPortraitKeyLight");
            Light key = keyGo.AddComponent<Light>();
            key.type = LightType.Directional;
            key.intensity = 1.65f;
            key.color = new Color(1f, 0.89f, 0.74f);
            keyGo.transform.rotation = Quaternion.LookRotation((headFocus - (camGo.transform.position + subject.right * 0.7f + Vector3.up * 0.6f)).normalized);

            var rimGo = new GameObject("TempPortraitRimLight");
            Light rim = rimGo.AddComponent<Light>();
            rim.type = LightType.Directional;
            rim.intensity = 1.1f;
            rim.color = new Color(0.95f, 0.62f, 0.30f);
            rimGo.transform.rotation = Quaternion.LookRotation(forward);

            RenderTexture rt = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;

            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            tex.Apply();
            File.WriteAllBytes(outputPath, tex.EncodeToPNG());

            cam.targetTexture = null;
            RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(camGo);
            UnityEngine.Object.DestroyImmediate(keyGo);
            UnityEngine.Object.DestroyImmediate(rimGo);
        }
    }
}
