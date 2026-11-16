using System;
using System.IO;
using System.Linq;
using Rimbara.Interaction;
using Rimbara.Mission;
using Rimbara.Player;
using Rimbara.UI;
using Rimbara.World;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Rimbara.Editor
{
    public static class RestoredMapBuilder
    {
        public const string RootName = "Storyline - Restored zone and ending (editable)";
        private const string Folder = "Assets/Settings/RestoredMap";
        private static TMP_FontAsset font;
        [MenuItem("RIMBARA/Storyline/Add Restored zone and ending")]
        public static void AddRestored()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.OpenScene(StorylineBuilder.ScenePath, OpenSceneMode.Single);
            if (scene.GetRootGameObjects().Any(r => r.name == RootName)) throw new InvalidOperationException("Restored already exists. Edit authored objects; do not regenerate.");
            Directory.CreateDirectory("Backups");
            EditorSceneManager.SaveScene(scene, "Backups/FromAshes-before-zone3-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".unity", true);
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Settings", "RestoredMap");
            RecoveryChapter recovery = UnityEngine.Object.FindFirstObjectByType<RecoveryChapter>();
            if (recovery == null) throw new InvalidOperationException("Complete authored Recovery first.");
            RestoredChapter chapter = recovery.gameObject.AddComponent<RestoredChapter>();
            Transform root = Group(RootName, null);
            RestoredEnvironment environment = root.gameObject.AddComponent<RestoredEnvironment>();
            EndingDirector director = root.gameObject.AddComponent<EndingDirector>();
            Transform visuals = Group("01 - Reborn forest and active village", root);
            Transform disaster = scene.GetRootGameObjects().First(r => r.name == DisasterMapBuilder.EnvironmentName).transform;
            // Renderers alone change; original collision and object identities stay stable.
            Renderer[] charred = disaster.GetComponentsInChildren<Transform>(true)
                .Where(t => t.name.StartsWith("Scorched standing tree ") || t.name.StartsWith("Fallen charred tree ") || t.name.StartsWith("Damaged forest edge ") || t.name.StartsWith("Burnt branches "))
                .SelectMany(t => t.GetComponentsInChildren<Renderer>(true)).Distinct().ToArray();
            Transform[] trunks = disaster.GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith("Scorched standing tree ") || t.name.StartsWith("Damaged forest edge ")).ToArray();
            foreach (Transform trunk in trunks)
            {
                Renderer[] renderers = trunk.GetComponentsInChildren<Renderer>(true);
                Bounds b = renderers[0].bounds; foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
                PlaceKit("Prefabs/Nature/Tree/EA03_Nature_Tree_01b_PRE.prefab", "Mature green tree - " + trunk.name, visuals, Ground(b.center.x, b.center.z), Mathf.Clamp(b.size.y, 4.5f, 8f));
            }
            RecoveryPlantingPoint[] beds = UnityEngine.Object.FindObjectsByType<RecoveryPlantingPoint>(FindObjectsSortMode.None);
            foreach (RecoveryPlantingPoint recoveryBed in beds)
                PlaceKit("Prefabs/Nature/Tree/EA03_Nature_Tree_02b_PRE.prefab", "Mature tree from Recovery - " + recoveryBed.ObjectId, visuals, recoveryBed.transform.position, 5f);
            var random = new System.Random(16032026);
            for (int i = 0; i < 180; i++)
            {
                float x = -34f + (float)random.NextDouble() * 68f, z = -38f + (float)random.NextDouble() * 82f;
                if (Mathf.Abs(x) < 3.2f || (z < -12f && Mathf.Abs(x) < 9f) || Mathf.Abs(x - RecoveryRiverLayout.CenterX(z)) < 3f && z > 17f) continue;
                if (beds.Any(b => Vector2.Distance(new Vector2(x, z), new Vector2(b.transform.position.x, b.transform.position.z)) < 2f)) continue;
                PlaceKit(i % 3 == 0 ? "Prefabs/Nature/Bushes/EA03_Nature_Bush_01a_PRE.prefab" : "Prefabs/Nature/Grass/EA03_Plant_Grass_01c_PRE.prefab",
                    "Restored understory " + i, visuals, Ground(x, z), i % 3 == 0 ? 0.75f : 0.35f);
            }
            Material flower = Mat("Restored flowers", new Color(0.88f, 0.62f, 0.22f));
            for (int i = 0; i < 28; i++)
            {
                Vector3 p = Ground(i % 2 == 0 ? -7f - i % 3 : 7f + i % 3, -34f + i * 2.5f);
                Shape(PrimitiveType.Sphere, "Wildflower " + i, visuals, p + Vector3.up * 0.22f, new Vector3(0.13f, 0.12f, 0.13f), flower);
            }
            Transform model = recovery.Disaster.Coordinator.Find("Coordinator visual - Ch42");
            for (int i = 0; i < 5; i++)
            {
                GameObject npc = UnityEngine.Object.Instantiate(model.gameObject, visuals);
                npc.name = "Restored villager ambient " + i;
                npc.transform.position = Ground(i % 2 == 0 ? -9f : 9f, -35f + i * 3f);
                npc.AddComponent<RestoredAmbientMotion>().Configure(new Vector3(0f, 0f, 1.5f), 0.35f + i * 0.06f);
            }
            Material bird = Mat("Restored birds", new Color(0.38f, 0.24f, 0.12f));
            for (int i = 0; i < 6; i++)
            {
                Transform animal = Group("Restored bird ambient " + i, visuals);
                animal.position = Ground(i % 2 == 0 ? -4f : 4f, 22f + i * 3f) + Vector3.up * 2.8f;
                Shape(PrimitiveType.Sphere, "Bird body", animal, animal.position, new Vector3(0.18f, 0.14f, 0.26f), bird);
                Shape(PrimitiveType.Cube, "Bird wings", animal, animal.position, new Vector3(0.5f, 0.03f, 0.12f), bird);
                animal.gameObject.AddComponent<RestoredAmbientMotion>().Configure(new Vector3(1.8f, 0.3f, 0f), 0.7f);
            }

            RimbaraHud hud = UnityEngine.Object.FindFirstObjectByType<RimbaraHud>(); font = hud.View.MissionDescription.font;
            Transform objectives = Group("02 - Final promise and viewpoint", root);
            GameObject elder = Target("Village elder - restored chapter", objectives, Ground(-4f, -29f));
            GameObject elderModel = UnityEngine.Object.Instantiate(model.gameObject, elder.transform);
            elderModel.name = "Elder visual - existing character placeholder"; elderModel.transform.localPosition = Vector3.zero;
            elderModel.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            // Distinct clothing overlay and grey cap without touching the shared model.
            Shape(PrimitiveType.Capsule, "Elder outer robe", elder.transform, elder.transform.position + Vector3.up * 0.65f, new Vector3(0.5f, 0.43f, 0.34f), Mat("Elder robe", new Color(0.28f, 0.4f, 0.24f)));
            Shape(PrimitiveType.Sphere, "Elder grey cap", elder.transform, elder.transform.position + Vector3.up * 1.8f, new Vector3(0.27f, 0.13f, 0.27f), Mat("Elder cap", new Color(0.65f, 0.63f, 0.56f)));
            elder.AddComponent<ElderDialogue>().Configure(chapter, UnityEngine.Object.FindFirstObjectByType<DisasterHud>(), Beacon("TETUA DESA", elder.transform, 2.35f), Ring(elder.transform));
            GameObject bed = Target("Final tree - village restoration bed", objectives, Ground(-4f, -22f));
            Shape(PrimitiveType.Cylinder, "Prepared final planting soil", bed.transform, bed.transform.position + Vector3.up * 0.02f, new Vector3(1.6f, 0.025f, 1.6f), Mat("Final soil", new Color(0.26f, 0.19f, 0.1f)));
            GameObject wet = Shape(PrimitiveType.Cylinder, "Watered final soil", bed.transform, bed.transform.position + Vector3.up * 0.04f, new Vector3(1.6f, 0.02f, 1.6f), Mat("Final moist soil", new Color(0.14f, 0.2f, 0.09f)));
            GameObject young = PlaceKit("Prefabs/Nature/Tree/EA03_Nature_Tree_01b_PRE.prefab", "Final young sapling", bed.transform, bed.transform.position, 1.25f);
            Transform sprayTarget = Group("Final sapling water target", bed.transform); sprayTarget.gameObject.layer = 11;
            SphereCollider waterTarget = sprayTarget.gameObject.AddComponent<SphereCollider>(); waterTarget.center = Vector3.up * 0.55f; waterTarget.radius = 0.55f; waterTarget.isTrigger = true; waterTarget.enabled = false;
            FinalPlantingPoint finalBed = bed.AddComponent<FinalPlantingPoint>();
            finalBed.Configure(chapter, young, wet, Beacon("BIBIT TERAKHIR", bed.transform, 1.8f), waterTarget, Ring(bed.transform));
            young.SetActive(false); wet.SetActive(false);
            GameObject viewpoint = Target("Restored forest viewpoint", objectives, Ground(0f, -16f));
            RestoredViewpoint panorama = viewpoint.AddComponent<RestoredViewpoint>();
            panorama.Configure(chapter, Beacon("TITIK PANDANG HUTAN", viewpoint.transform, 1.8f), Ring(viewpoint.transform));
            GameObject plaque = Shape(PrimitiveType.Cube, "Viewpoint physical plaque", objectives, Ground(2.4f, -16f) + Vector3.up * 1f, new Vector3(1.8f, 0.5f, 0.12f), Mat("Viewpoint timber", new Color(0.27f, 0.16f, 0.07f)));
            Inscription(plaque.transform, "HUTAN ARUNIKA / TITIK PANDANG");
            Shape(PrimitiveType.Cube, "Viewpoint plaque support", objectives, Ground(2.4f, -16f) + Vector3.up * 0.5f, new Vector3(0.12f, 1f, 0.12f), Mat("Viewpoint timber", new Color(0.27f, 0.16f, 0.07f)));
            Material sky = new Material(Shader.Find("Skybox/Procedural")); sky.SetColor("_SkyTint", new Color(0.55f, 0.71f, 0.81f)); sky.SetFloat("_AtmosphereThickness", 1f); sky.SetFloat("_Exposure", 1.05f);
            AssetDatabase.CreateAsset(sky, Folder + "/RestoredSky.mat");
            Texture2D earth = new Texture2D(32, 32);
            for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++) earth.SetPixel(x, y, Color.Lerp(new Color(0.23f, 0.32f, 0.13f), new Color(0.4f, 0.47f, 0.21f), Mathf.PerlinNoise(x * 0.3f, y * 0.3f)));
            earth.Apply(); File.WriteAllBytes(Folder + "/RestoredSoil.png", earth.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(earth); AssetDatabase.ImportAsset(Folder + "/RestoredSoil.png");
            var layer = new TerrainLayer { diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "/RestoredSoil.png"), tileSize = new Vector2(5f, 5f) };
            AssetDatabase.CreateAsset(layer, Folder + "/RestoredSoil.terrainlayer");
            RecoveryRiver river = UnityEngine.Object.FindFirstObjectByType<RecoveryRiver>();
            Material riverMaterial = new Material(Shader.Find("Rimbara/Recovery River")); riverMaterial.SetColor("_BaseColor", new Color(0.16f, 0.43f, 0.46f)); riverMaterial.SetColor("_RecoveredTint", new Color(0.2f, 0.48f, 0.49f)); riverMaterial.SetFloat("_Recovery", 1f); riverMaterial.SetFloat("_FlowSpeed", 0.22f);
            AssetDatabase.CreateAsset(riverMaterial, Folder + "/RestoredRiver.mat");
            ParticleSystem smoke = UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None).First(p => p.name == "Low wildfire smoke drifting over forest");
            environment.Configure(chapter, visuals.gameObject, charred, beds.Select(b => b.transform.Find("Young planted tree").gameObject).ToArray(), Terrain.activeTerrain, layer,
                river.transform.Find("River water surface").GetComponent<Renderer>(), riverMaterial, sky, RenderSettings.sun, smoke);
            Transform cameras = Group("03 - Ending camera poses", root);
            Vector3[] cameraPoints = { new Vector3(-1f, 4f, -25f), new Vector3(5f, 9f, -7f), new Vector3(0f, 18f, -33f), new Vector3(25f, 27f, -49f) };
            Transform[] poses = new Transform[4];
            for (int i = 0; i < poses.Length; i++) { poses[i] = Group("Ending shot " + (i + 1), cameras); poses[i].position = cameraPoints[i]; poses[i].LookAt(i == 0 ? bed.transform.position : new Vector3(0f, 2f, 4f)); }
            GameObject panel = new GameObject("Story cinematic overlay", typeof(RectTransform), typeof(Image));
            RectTransform rect = panel.GetComponent<RectTransform>(); rect.SetParent(hud.View.Canvas.transform, false); Stretch(rect);
            Image fade = panel.GetComponent<Image>(); fade.color = Color.clear; fade.raycastTarget = true;
            TMP_Text subtitle = new GameObject("Ending subtitle", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
            subtitle.rectTransform.SetParent(rect, false); subtitle.rectTransform.anchorMin = new Vector2(0.12f, 0.05f); subtitle.rectTransform.anchorMax = new Vector2(0.88f, 0.24f); subtitle.rectTransform.offsetMin = subtitle.rectTransform.offsetMax = Vector2.zero;
            subtitle.font = font; subtitle.fontSize = 25f; subtitle.fontSizeMin = 18f; subtitle.fontSizeMax = 25f; subtitle.enableAutoSizing = true; subtitle.alignment = TextAlignmentOptions.Center; subtitle.color = Color.white;
            Button skip = UnityEngine.Object.Instantiate(hud.View.Resume, hud.View.Canvas.transform); skip.name = "Skip ending cinematic";
            skip.GetComponent<RectTransform>().anchorMin = skip.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.1f); skip.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            TMP_Text caption = skip.GetComponentInChildren<TMP_Text>(); caption.name = "Skip cinematic label"; caption.text = "LEWATI SINEMATIK"; skip.onClick = new Button.ButtonClickedEvent();
            AudioSource music = root.gameObject.AddComponent<AudioSource>(); music.clip = AudioFactory.EnsureEndingMusic(); music.playOnAwake = false; music.loop = false; music.volume = 0.55f;
            director.Configure(chapter, environment, UnityEngine.Object.FindFirstObjectByType<GameFlow>(), UnityEngine.Object.FindFirstObjectByType<VolunteerRig>().ViewCamera, panel, fade, subtitle, skip, music, poses);
            chapter.Configure(recovery, elder.transform, finalBed, panorama, director);
            objectives.SetParent(visuals, true);
            visuals.gameObject.SetActive(false); panel.SetActive(false); skip.gameObject.SetActive(false);
            foreach (Transform t in objectives.GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith("Restored beacon - "))) t.gameObject.SetActive(false);
            elder.SetActive(true);
            // Objective visuals are activated by their runtime gates; elder/model
            // must not appear in the Disaster state.
            elderModel.SetActive(false);
            foreach (Renderer r in elder.GetComponentsInChildren<Renderer>(true)) if (!r.transform.IsChildOf(elderModel.transform)) r.enabled = false;
            elder.AddComponent<RestoredRoleVisibility>();
            IndonesianText.ApplyStaticLabels(hud.View.Canvas.transform);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets(); VerifyRestored();
        }
        public static void VerifyRestored()
        {
            RestoredChapter chapter = UnityEngine.Object.FindFirstObjectByType<RestoredChapter>();
            GameObject root = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(r => r.name == RootName);
            if (chapter == null || root == null || root.GetComponent<EndingDirector>() == null || root.GetComponent<RestoredEnvironment>() == null)
                throw new InvalidOperationException("Missing Restored scene extension.");
            var data = new SerializedObject(chapter);
            foreach (string field in new[] { "recovery", "elder", "finalBed", "viewpoint", "director" }) if (data.FindProperty(field).objectReferenceValue == null) throw new InvalidOperationException("Missing Restored reference " + field);
            if (root.GetComponentInChildren<FinalPlantingPoint>(true) == null || root.GetComponentInChildren<RestoredViewpoint>(true) == null || root.GetComponentInChildren<ElderDialogue>(true) == null)
                throw new InvalidOperationException("Restored needs its three authored interactions.");
            Transform visuals = root.transform.Find("01 - Reborn forest and active village");
            if (visuals == null || visuals.gameObject.activeSelf || visuals.GetComponentInChildren<FinalPlantingPoint>(true) == null)
                throw new InvalidOperationException("Restored visual/objective state must remain hidden before transition.");
            foreach (InteractableBehaviour target in visuals.GetComponentsInChildren<InteractableBehaviour>(true))
                if (target.gameObject.layer != 10 || target.GetComponent<Collider>() == null || !target.GetComponent<Collider>().isTrigger)
                    throw new InvalidOperationException("Final interactions require layer 10 triggers.");
            var ending = new SerializedObject(root.GetComponent<EndingDirector>());
            foreach (string field in new[] { "chapter", "environment", "flow", "cameraView", "overlay", "fade", "subtitle", "skip", "music" })
                if (ending.FindProperty(field).objectReferenceValue == null) throw new InvalidOperationException("Missing cinematic reference " + field);
            if (ending.FindProperty("cameraPoses").arraySize != 4) throw new InvalidOperationException("Ending needs four camera poses.");
            ProjectBuilder.WriteArtifact("restored-verification.json", "{\"scene\":\"" + StorylineBuilder.ScenePath + "\",\"finalTrees\":1,\"viewpoints\":1,\"endingSeconds\":36,\"result\":\"passed\"}");
        }
        public static void PolishRestored()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.OpenScene(StorylineBuilder.ScenePath, OpenSceneMode.Single);
            Transform root = scene.GetRootGameObjects().First(r => r.name == RootName).transform;
            Transform visuals = root.Find("01 - Reborn forest and active village");
            Transform finalObjectives = root.Find("02 - Final promise and viewpoint");
            if (finalObjectives != null) finalObjectives.SetParent(visuals, true);
            if (visuals.Find("Restored canopy additions") == null)
            {
                Transform canopy = Group("Restored canopy additions", visuals);
                var random = new System.Random(345);
                for (int i = 0; i < 65; i++)
                {
                    float x = -34f + (float)random.NextDouble() * 68f;
                    float z = -1f + (float)random.NextDouble() * 46f;
                    if (Mathf.Abs(x) < 9f) continue;
                    PlaceKit(i % 2 == 0 ? "Prefabs/Nature/Tree/EA03_Nature_Tree_02c_PRE.prefab" : "Prefabs/Nature/Tree/EA03_Nature_Tree_03c_PRE.prefab",
                        "Restored leafy canopy " + i, canopy, Ground(x, z), 5f + i % 3);
                }
                for (int i = 0; i < 36; i++)
                {
                    float x = i % 2 == 0 ? -5.5f : 5.5f;
                    float z = -36f + i * 2f;
                    PlaceKit("Prefabs/Nature/Bushes/EA03_Nature_Bush_01a_PRE.prefab", "Village and forest planted cover " + i,
                        canopy, Ground(x, z), 0.65f);
                }
            }
            // Fallen timber remains visible where its original collision remains.
            var environment = new SerializedObject(root.GetComponent<RestoredEnvironment>());
            SerializedProperty charred = environment.FindProperty("charredRenderers");
            Renderer[] kept = Enumerable.Range(0, charred.arraySize).Select(i => charred.GetArrayElementAtIndex(i).objectReferenceValue as Renderer)
                .Where(r => r != null && !r.GetComponentsInParent<Transform>(true).Any(t => t.name.StartsWith("Fallen charred tree "))).ToArray();
            charred.arraySize = kept.Length; for (int i = 0; i < kept.Length; i++) charred.GetArrayElementAtIndex(i).objectReferenceValue = kept[i];
            environment.ApplyModifiedPropertiesWithoutUndo();
            font = UnityEngine.Object.FindFirstObjectByType<RimbaraHud>().View.MissionDescription.font;
            Transform plaque = visuals.Find("02 - Final promise and viewpoint/Viewpoint physical plaque");
            TextMeshPro plaqueText = plaque.GetComponentInChildren<TextMeshPro>();
            plaqueText.text = "HUTAN ARUNIKA\nTITIK PANDANG"; plaqueText.fontSize = 1.3f; plaqueText.textWrappingMode = TextWrappingModes.Normal;
            plaqueText.rectTransform.sizeDelta = new Vector2(1.7f, 0.42f);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets(); VerifyRestored();
        }
        private static Transform Group(string name, Transform parent) { var go = new GameObject(name); go.transform.SetParent(parent, false); return go.transform; }
        private static Vector3 Ground(float x, float z) => new Vector3(x, Terrain.activeTerrain.SampleHeight(new Vector3(x, 0f, z)) + Terrain.activeTerrain.transform.position.y + 0.06f, z);
        private static GameObject Target(string name, Transform parent, Vector3 p) { GameObject go = Group(name, parent).gameObject; go.transform.position = p; go.layer = 10; SphereCollider c = go.AddComponent<SphereCollider>(); c.isTrigger = true; c.radius = 0.65f; c.center = Vector3.up * 0.55f; return go; }
        private static Material Mat(string name, Color color) { string path = Folder + "/" + name + ".mat"; Material m = AssetDatabase.LoadAssetAtPath<Material>(path); if (m != null) return m; m = new Material(Shader.Find("Universal Render Pipeline/Lit")); m.SetColor("_BaseColor", color); AssetDatabase.CreateAsset(m, path); return m; }
        private static GameObject Shape(PrimitiveType type, string name, Transform parent, Vector3 p, Vector3 scale, Material mat) { GameObject go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent, true); go.transform.position = p; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = mat; UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); go.layer = 2; return go; }
        private static GameObject PlaceKit(string path, string name, Transform parent, Vector3 p, float height)
        {
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(DisasterMapBuilder.KitRoot + path), parent); go.name = name;
            Renderer[] rs = go.GetComponentsInChildren<Renderer>(true); Bounds b = rs[0].bounds; foreach (Renderer r in rs) b.Encapsulate(r.bounds);
            go.transform.localScale *= height / Mathf.Max(b.size.y, 0.1f); b = rs[0].bounds; foreach (Renderer r in rs) b.Encapsulate(r.bounds);
            go.transform.position += p - new Vector3(b.center.x, b.min.y, b.center.z);
            foreach (Collider c in go.GetComponentsInChildren<Collider>(true)) c.enabled = false; foreach (Transform t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = 2; return go;
        }
        private static GameObject Ring(Transform parent) => EnvironmentFactory.Ring("Restored focus ring", parent, 0.7f, AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Amber marker.mat"));
        private static GameObject Beacon(string text, Transform parent, float y) { Transform root = Group("Restored beacon - " + text, parent); TMP_Text t = EnvironmentFactory.WorldText(text, root, Vector3.up * y, EnvironmentFactory.Amber, 1.8f, 4f); t.font = font; return root.gameObject; }
        private static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
        private static void Inscription(Transform surface, string text)
        {
            GameObject go = new GameObject("Physical inscription - " + text, typeof(RectTransform), typeof(TextMeshPro)); go.transform.SetParent(surface, false); go.transform.localPosition = new Vector3(0f, 0f, -0.6f);
            go.transform.localScale = new Vector3(1f / surface.lossyScale.x, 1f / surface.lossyScale.y, 1f / surface.lossyScale.z);
            TextMeshPro t = go.GetComponent<TextMeshPro>(); t.font = font; t.text = text; t.fontSize = 1.4f; t.alignment = TextAlignmentOptions.Center; t.rectTransform.sizeDelta = new Vector2(1.7f, 0.4f);
        }
    }
}
