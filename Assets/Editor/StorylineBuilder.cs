using System;
using System.IO;
using System.Linq;
using Rimbara.Interaction;
using Rimbara.Mission;
using Rimbara.Player;
using Rimbara.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Rimbara.Editor
{
    // Copy once, then edit normally. Never regenerate or overwrite the team maps.
    public static class StorylineBuilder
    {
        public const string ScenePath = "Assets/Scenes/FromAshes.unity";

        [MenuItem("RIMBARA/Storyline/Create Disaster chapter copy")]
        public static void CreateChapter()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (File.Exists(ScenePath)) throw new InvalidOperationException("FromAshes already exists. Edit it directly; creation never overwrites your scene.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!AssetDatabase.CopyAsset(ProjectBuilder.ScenePath, ScenePath))
                throw new InvalidOperationException("Cannot copy the team's authored Main scene.");
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameState state = Find<GameState>();
            MissionSystem missions = Find<MissionSystem>();
            GameFlow flow = Find<GameFlow>();
            VolunteerRig rig = Find<VolunteerRig>();
            RimbaraHud hud = Find<RimbaraHud>();
            var chapter = state.gameObject.AddComponent<DisasterChapter>();
            missions.ConfigureDisaster(chapter);
            // The CLI week1 profile would hide one of the authored chapter fires.
            ProfileSelector selector = Find<ProfileSelector>();
            UnityEngine.Object.DestroyImmediate(selector);
            foreach (Terrain terrain in UnityEngine.Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None)) terrain.gameObject.layer = 8;

            var root = new GameObject("Storyline - Disaster chapter (editable)");
            Material wood = MaterialAsset("Assets/Art/Materials/Charred bark.mat");
            Material ground = MaterialAsset("Assets/Art/Materials/Planting soil.mat", "Assets/Art/Materials/Dry earth.mat");
            var coordinator = new GameObject("Mission coordinator - emergency post");
            coordinator.transform.SetParent(root.transform);
            coordinator.transform.position = ClearPosition(new Vector2(-3.5f, -25f));
            ColliderTrigger(coordinator);
            EnvironmentFactory.Beacon("MISSION COORDINATOR", coordinator.transform, EnvironmentFactory.Amber, 2.1f);
            GameObject ring = EnvironmentFactory.Ring("Focus ring", coordinator.transform, 0.65f,
                AssetCatalog.Material("Amber marker", EnvironmentFactory.Amber, true));
            // Reuse the team's Humanoid visual as a stationary coordinator placeholder.
            GameObject volunteerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/Volunteer_Ch42.prefab");
            if (volunteerPrefab != null)
            {
                GameObject npc = (GameObject)PrefabUtility.InstantiatePrefab(volunteerPrefab, coordinator.transform);
                PrefabUtility.UnpackPrefabInstance(npc, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                npc.name = "Coordinator visual - Ch42";
                npc.transform.localPosition = Vector3.zero;
                npc.tag = "Untagged";
                foreach (MonoBehaviour behaviour in npc.GetComponentsInChildren<MonoBehaviour>(true).Reverse()) UnityEngine.Object.DestroyImmediate(behaviour);
                foreach (Collider collider in npc.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(collider);
                foreach (ParticleSystem particles in npc.GetComponentsInChildren<ParticleSystem>(true)) UnityEngine.Object.DestroyImmediate(particles.gameObject);
                foreach (Transform t in npc.GetComponentsInChildren<Transform>(true))
                    if (t != null) t.gameObject.layer = 0;
                Transform nozzle = npc.transform.Find("Water nozzle");
                if (nozzle != null) nozzle.gameObject.SetActive(false);
            }

            GameObject kit = new GameObject("Firefighting equipment");
            kit.transform.SetParent(root.transform);
            kit.transform.position = ClearPosition(new Vector2(3f, -25f));
            ColliderTrigger(kit);
            GameObject kitVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            kitVisual.name = "Equipment crate"; kitVisual.transform.SetParent(kit.transform, false);
            kitVisual.transform.localPosition = Vector3.up * 0.4f;
            kitVisual.transform.localScale = new Vector3(0.8f, 0.8f, 0.6f);
            UnityEngine.Object.DestroyImmediate(kitVisual.GetComponent<Collider>());
            kitVisual.GetComponent<Renderer>().sharedMaterial = AssetCatalog.Material("Equipment crate", new Color(0.75f, 0.37f, 0.12f));
            GameObject kitRing = EnvironmentFactory.Ring("Focus ring", kit.transform, 0.75f, AssetCatalog.Material("Amber marker", EnvironmentFactory.Amber, true));
            GameObject kitBeacon = EnvironmentFactory.Beacon("COLLECT EQUIPMENT", kit.transform, EnvironmentFactory.Amber, 1.4f);
            kit.AddComponent<EquipmentPickup>().Configure(chapter, kitVisual, kitRing, kitBeacon);

            Vector2[] positions = { new Vector2(-4f, -12f), new Vector2(4f, -10f), new Vector2(-5f, -17f), new Vector2(5f, -19f) };
            var debris = new DebrisPickup[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject pile = new GameObject("Designated debris " + (i + 1).ToString("00"));
                pile.transform.SetParent(root.transform); pile.transform.position = ClearPosition(positions[i]);
                ColliderTrigger(pile);
                GameObject burnt = new GameObject("Burnt debris visual"); burnt.transform.SetParent(pile.transform, false);
                GameObject log = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Environment/KenneyNature/log.fbx");
                for (int j = 0; j < 3; j++)
                {
                    GameObject piece = UnityEngine.Object.Instantiate(log, burnt.transform);
                    piece.transform.localPosition = new Vector3((j - 1) * 0.28f, 0.03f, 0f);
                    piece.transform.localScale = Vector3.one * 1.6f;
                    piece.transform.localRotation = Quaternion.Euler(0f, j * 43f, 0f);
                    foreach (Renderer renderer in piece.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = wood;
                }
                GameObject ready = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ready.name = "Prepared patch"; ready.transform.SetParent(pile.transform.parent, false);
                ready.transform.position = pile.transform.position + Vector3.up * 0.015f; ready.transform.localScale = new Vector3(1.8f, 0.015f, 1.8f);
                UnityEngine.Object.DestroyImmediate(ready.GetComponent<Collider>());
                ready.GetComponent<Renderer>().sharedMaterial = ground; ready.SetActive(false);
                var focus = EnvironmentFactory.Ring("Focus ring", pile.transform, 1f, AssetCatalog.Material("Amber marker", EnvironmentFactory.Amber, true));
                EnvironmentFactory.Beacon("DESIGNATED DEBRIS", burnt.transform, EnvironmentFactory.Amber, 1.3f);
                GameObject dust = (GameObject)PrefabUtility.InstantiatePrefab(ParticleFactory.ClearDust(), root.transform);
                dust.name = "Clear dust " + (i + 1).ToString("00");
                dust.transform.position = pile.transform.position;
                dust.SetActive(false);
                debris[i] = pile.AddComponent<DebrisPickup>();
                debris[i].Configure("disaster-debris-" + i, chapter, burnt, ready, focus, dust);
            }
            chapter.Configure(state, coordinator.transform, kit.transform, debris,
                UnityEngine.Object.FindObjectsByType<FirePoint>(FindObjectsSortMode.None).OrderBy(f => f.Slot).ToArray());
            chapter.ConfigureEquipmentVisuals(rig.GetComponentsInChildren<Renderer>(true)
                .Where(r => r.GetComponent<SkinnedMeshRenderer>() == null && (r.transform.IsChildOf(rig.transform.Find("Water nozzle"))
                    || r.name.Contains("water pack") || r.name.Contains("pack band"))).ToArray());
            chapter.gameObject.AddComponent<DisasterAtmosphere>().Configure(chapter,
                UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).FirstOrDefault(l => l.type == LightType.Directional));
            DisasterHud storyHud = hud.gameObject.AddComponent<DisasterHud>();
            Transform panel = DialoguePanel(hud.View, out TMP_Text text, out Button next);
            storyHud.Configure(chapter, flow, rig, Find<WaterSprayer>(), hud.View, panel.gameObject, text, next);
            coordinator.AddComponent<CoordinatorDialogue>().Configure(chapter, storyHud, ring);
            RewriteText(hud.View);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            // Main supplies gameplay wiring only. Individual EmaceArt resources
            // are assembled into an original Disaster level, not a demo scene.
            DisasterMapBuilder.BuildMap();
        }

        private static T Find<T>() where T : UnityEngine.Object => UnityEngine.Object.FindFirstObjectByType<T>(FindObjectsInactive.Include)
            ?? throw new InvalidOperationException("Missing team scene dependency: " + typeof(T).Name);

        private static Material MaterialAsset(params string[] paths)
        {
            foreach (string path in paths) { Material m = AssetDatabase.LoadAssetAtPath<Material>(path); if (m != null) return m; }
            return AssetCatalog.Material("Storyline earth", new Color(0.22f, 0.17f, 0.12f));
        }

        private static void ColliderTrigger(GameObject target)
        {
            target.layer = 10;
            SphereCollider collider = target.AddComponent<SphereCollider>();
            collider.isTrigger = true; collider.center = Vector3.up * 0.55f; collider.radius = 0.7f;
        }

        private static Vector3 ClearPosition(Vector2 desired)
        {
            Physics.SyncTransforms();
            for (int i = 0; i < 49; i++)
            {
                float x = desired.x + (i % 7 - 3) * 0.75f;
                float z = desired.y + (i / 7 - 3) * 0.75f;
                if (i == 0) { x = desired.x; z = desired.y; }
                Terrain terrain = Terrain.activeTerrain;
                Vector3 point = new Vector3(x, terrain.SampleHeight(new Vector3(x, 0f, z)) + terrain.transform.position.y + 0.03f, z);
                Collider[] blockers = Physics.OverlapCapsule(point + Vector3.up * 0.8f, point + Vector3.up * 1.5f, 1.1f,
                    ~0, QueryTriggerInteraction.Collide);
                if (blockers.All(c => c is TerrainCollider)) return point;
            }
            throw new InvalidOperationException("No clear interaction location near " + desired + ". Place it manually.");
        }

        private static Transform DialoguePanel(HudReferences view, out TMP_Text text, out Button next)
        {
            GameObject panel = new GameObject("Coordinator dialogue", typeof(RectTransform), typeof(Image));
            var rect = panel.GetComponent<RectTransform>();
            Transform parent = view.SafeArea != null ? view.SafeArea.transform : view.Canvas.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f); rect.sizeDelta = new Vector2(860f, 440f);
            Image background = panel.GetComponent<Image>(); background.color = new Color(0.08f, 0.12f, 0.09f, 0.98f);
            background.raycastTarget = true;
            UiKit kit = AssetDatabase.LoadAssetAtPath<UiKit>(FantasticThemeBuilder.AssetPath);
            if (kit != null)
            {
                HudTheme.ApplyPanel(background, kit, HudSurface.Window);
                background.raycastTarget = true;
            }
            text = new GameObject("Dialogue text", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
            text.rectTransform.SetParent(rect, false); text.rectTransform.anchorMin = new Vector2(0f, 0.25f); text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(35f, 10f); text.rectTransform.offsetMax = new Vector2(-35f, -30f);
            text.font = kit != null ? kit.BodyFont : view.MissionDescription.font;
            text.fontSize = 23f; text.color = kit != null ? kit.Parchment : Color.white; text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal; text.enableAutoSizing = true; text.fontSizeMin = 18f; text.fontSizeMax = 23f;
            next = UnityEngine.Object.Instantiate(view.Begin, rect); next.name = "Dialogue next";
            if (kit != null)
            {
                Image nextImage = next.GetComponent<Image>();
                if (nextImage != null) HudTheme.ApplyButton(next, nextImage, kit, true);
            }
            RectTransform button = next.GetComponent<RectTransform>(); button.anchorMin = button.anchorMax = new Vector2(0.5f, 0f);
            button.pivot = new Vector2(0.5f, 0f); button.anchoredPosition = new Vector2(0f, 25f); button.sizeDelta = new Vector2(260f, 58f);
            next.onClick = new Button.ButtonClickedEvent();
            TMP_Text caption = next.GetComponentInChildren<TMP_Text>();
            caption.rectTransform.anchorMin = Vector2.zero; caption.rectTransform.anchorMax = Vector2.one;
            caption.rectTransform.offsetMin = new Vector2(12f, 8f); caption.rectTransform.offsetMax = new Vector2(-12f, -8f);
            caption.alignment = TextAlignmentOptions.Center;
            panel.SetActive(false);
            return rect;
        }

        private static void RewriteText(HudReferences view)
        {
            IndonesianText.ApplyStaticLabels(view.Canvas.transform);
        }

        [MenuItem("RIMBARA/Storyline/Verify Disaster chapter")]
        public static void VerifyChapter()
        {
            UiKit kit = AssetDatabase.LoadAssetAtPath<UiKit>(FantasticThemeBuilder.AssetPath);
            if (kit == null || !kit.IsComplete)
                throw new InvalidOperationException("Fantastic UiKit is missing or incomplete. Import the kit and run RIMBARA/UI/Build Fantastic theme.");
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform[] transforms = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            Transform[] missing = transforms.Where(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) > 0).ToArray();
            if (missing.Length > 0)
                throw new InvalidOperationException("Missing script in storyline scene: " + string.Join(", ", missing.Select(t => t.name)));
            DisasterChapter chapter = Find<DisasterChapter>();
            FirePoint[] authoredFires = UnityEngine.Object.FindObjectsByType<FirePoint>(FindObjectsSortMode.None);
            DisasterMapBuilder.VerifyMap();
            if (Find<DisasterChapter>().Recovery != null) RecoveryMapBuilder.VerifyRecovery();
            if (UnityEngine.Object.FindFirstObjectByType<RestoredChapter>() != null) RestoredMapBuilder.VerifyRestored();
            if (Find<MissionSystem>().DisasterChapter != chapter || Find<GameState>().Profile.Fires != 10
                || UnityEngine.Object.FindObjectsByType<DebrisPickup>(FindObjectsSortMode.None).Length != 6
                || authoredFires.Length != 10)
                throw new InvalidOperationException("Storyline chapter references or object supply are invalid.");
            Terrain terrain = UnityEngine.Object.FindFirstObjectByType<Terrain>();
            if (terrain == null) throw new InvalidOperationException("Storyline chapter is missing its terrain.");
            Bounds bounds = terrain.terrainData.bounds;
            Vector3 min = terrain.transform.position + bounds.min;
            Vector3 max = terrain.transform.position + bounds.max;
            if (authoredFires.Any(f => f.transform.position.x < min.x + 8f || f.transform.position.x > max.x - 8f
                || f.transform.position.z < min.z + 8f || f.transform.position.z > max.z - 8f))
                throw new InvalidOperationException("Fire targets must stay inside the terrain with an eight metre margin.");
            if (authoredFires.Any(a => authoredFires.Any(b => a != b && Vector3.Distance(a.transform.position, b.transform.position) < 10f)))
                throw new InvalidOperationException("Fire targets must be spread at least ten metres apart.");
            if (Find<DisasterHud>() == null || Find<CoordinatorDialogue>() == null || Find<EquipmentPickup>() == null)
                throw new InvalidOperationException("Missing chapter UI or interactions.");
            string[] ids = UnityEngine.Object.FindObjectsByType<DebrisPickup>(FindObjectsSortMode.None).Select(d => d.ObjectId).ToArray();
            if (ids.Any(string.IsNullOrWhiteSpace) || ids.Distinct().Count() != ids.Length)
                throw new InvalidOperationException("Debris requires distinct, nonempty IDs.");
            foreach (InteractableBehaviour interactable in UnityEngine.Object.FindObjectsByType<InteractableBehaviour>(FindObjectsSortMode.None)
                .Where(i => i is DebrisPickup || i is EquipmentPickup || i is CoordinatorDialogue))
            {
                if (!interactable.GetComponents<Collider>().Any(c => c.isTrigger) || interactable.gameObject.layer != 10)
                    throw new InvalidOperationException("Chapter interaction must have an Interaction-layer trigger: " + interactable.name);
            }
            ProjectBuilder.WriteArtifact("storyline-verification.json", "{\"scene\":\"" + ScenePath + "\",\"utc\":\"" + DateTime.UtcNow.ToString("O")
                + "\",\"fires\":10,\"debris\":6,\"missingScripts\":0,\"result\":\"passed\"}");
        }

        [MenuItem("RIMBARA/Storyline/Update chapter presentation")]
        public static void UpdatePresentation()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            DisasterChapter chapter = Find<DisasterChapter>();
            if (chapter.GetComponent<DisasterAtmosphere>() == null)
                chapter.gameObject.AddComponent<DisasterAtmosphere>().Configure(chapter,
                    UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).FirstOrDefault(l => l.type == LightType.Directional));
            foreach (Transform transform in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)))
            {
                if (transform.name == "Coordinator visual - Ch42")
                {
                    transform.gameObject.tag = "Untagged";
                    Transform nozzle = transform.Find("Water nozzle");
                    if (nozzle != null) nozzle.gameObject.SetActive(false);
                }
                if (transform.name != "Dialogue next") continue;
                TMP_Text caption = transform.GetComponentInChildren<TMP_Text>(true);
                caption.rectTransform.anchorMin = Vector2.zero; caption.rectTransform.anchorMax = Vector2.one;
                caption.rectTransform.offsetMin = new Vector2(12f, 8f); caption.rectTransform.offsetMax = new Vector2(-12f, -8f);
                caption.alignment = TextAlignmentOptions.Center;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("RIMBARA/Storyline/Build Disaster Windows x64")]
        public static void BuildChapter()
        {
            VerifyChapter();
            DisasterMapBuilder.VerifyMap();
            IndonesianSceneLocalization.Verify();
            HumanNpcBuilder.Verify();
            ConfigureCompatibleRendering();
            string output = Path.GetFullPath("Builds/Storyline-Windows/RIMBARA-FromAshes.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath }, locationPathName = output, target = BuildTarget.StandaloneWindows64
            });
            ProjectBuilder.WriteArtifact("storyline-build.json", "{\"result\":\"" + report.summary.result + "\",\"errors\":"
                + report.summary.totalErrors + ",\"warnings\":" + report.summary.totalWarnings + "}");
            if (report.summary.result != BuildResult.Succeeded) throw new BuildFailedException("Storyline build failed.");
            string directory = Path.GetDirectoryName(output);
            File.Copy("THIRD_PARTY_NOTICES.md", Path.Combine(directory, "THIRD_PARTY_NOTICES.md"), true);
            string licences = Path.Combine(directory, "Licenses");
            Directory.CreateDirectory(licences);
            File.Copy("Docs/Licenses/Unity-Companion-License.txt", Path.Combine(licences, "Unity-Companion-License.txt"), true);
            File.Copy("Assets/Art/Environment/KenneyNature/License.txt", Path.Combine(licences, "Kenney-Nature-CC0.txt"), true);
            foreach (string notice in Directory.GetFiles("Assets/RIMBARA/Licenses", "*.txt"))
                File.Copy(notice, Path.Combine(licences, Path.GetFileName(notice)), true);
            foreach (string notice in Directory.GetFiles("Assets/TextMesh Pro/Fonts", "*.txt"))
                File.Copy(notice, Path.Combine(licences, Path.GetFileName(notice)), true);
            File.Copy("Docs/Slavic-Base.md", Path.Combine(licences, "EmaceArt-Slavic-Base.md"), true);
            string npcLicences = Path.Combine(licences, "HumanNPCs");
            Directory.CreateDirectory(npcLicences);
            foreach (string notice in Directory.GetFiles("Docs/Licenses/HumanNPCs"))
                File.Copy(notice, Path.Combine(npcLicences, Path.GetFileName(notice)), true);
        }

        private static void ConfigureCompatibleRendering()
        {
            const string path = "Assets/Settings/StorylineURPGlobalSettings.asset";
            Type type = typeof(UniversalRenderPipeline).Assembly.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineGlobalSettings", true);
            var settings = AssetDatabase.LoadAssetAtPath(path, type) as RenderPipelineGlobalSettings;
            if (settings == null)
                settings = RenderPipelineGlobalSettingsUtils.Create(type, path);
            // Populate through the installed URP API. Do not downgrade the version
            // number of the team's newer global settings or overwrite that asset.
            EditorGraphicsSettings.SetRenderPipelineGlobalSettingsAsset<UniversalRenderPipeline>(settings);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }
    }
}
