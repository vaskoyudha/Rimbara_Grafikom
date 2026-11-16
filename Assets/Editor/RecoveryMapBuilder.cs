using System;
using System.IO;
using System.Linq;
using Rimbara.Interaction;
using Rimbara.Mission;
using Rimbara.UI;
using Rimbara.World;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rimbara.Editor
{
    // One-time additive authoring. Never regenerates Disaster or opens kit demos.
    public static class RecoveryMapBuilder
    {
        public const string RootName = "Storyline - Recovery zone (editable)";
        private const string Folder = "Assets/Settings/RecoveryMap";
        private static Material wood;
        private static Material soil;
        private static Material moist;
        private static TMP_FontAsset font;

        [MenuItem("RIMBARA/Storyline/Add Recovery zone to authored map")]
        public static void AddRecovery()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.OpenScene(StorylineBuilder.ScenePath, OpenSceneMode.Single);
            if (scene.GetRootGameObjects().Any(r => r.name == RootName) || UnityEngine.Object.FindFirstObjectByType<RecoveryChapter>() != null)
                throw new InvalidOperationException("Recovery already exists. Edit its objects; do not regenerate.");
            Directory.CreateDirectory("Backups");
            EditorSceneManager.SaveScene(scene, "Backups/FromAshes-before-zone2-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".unity", true);
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Settings", "RecoveryMap");
            DisasterChapter disaster = UnityEngine.Object.FindFirstObjectByType<DisasterChapter>();
            var chapter = disaster.gameObject.AddComponent<RecoveryChapter>();
            var root = new GameObject(RootName);
            font = UnityEngine.Object.FindFirstObjectByType<RimbaraHud>().View.MissionDescription.font;
            // A kit palette atlas cannot be mapped coherently onto a Unity cube.
            wood = Material("Recovery timber", new Color(0.28f, 0.17f, 0.08f));
            soil = Material("Prepared planting soil", new Color(0.29f, 0.20f, 0.12f));
            moist = Material("Watered planting soil", new Color(0.14f, 0.17f, 0.08f));
            Material ash = AssetDatabase.LoadAssetAtPath<Material>("Assets/Settings/DisasterMap/Charred timber.mat");
            Material dullWater = Material("Ash-affected recovery water", new Color(0.22f, 0.25f, 0.18f));
            Material cleanWater = Material("Cleaner recovery water", new Color(0.25f, 0.46f, 0.40f));
            Transform nursery = Group("01 - Nursery and forest ranger", root.transform);
            Shape(PrimitiveType.Cube, "Nursery work table", nursery, Ground(-11f, -7f) + Vector3.up * 0.62f, new Vector3(4.4f, 0.18f, 1.3f), wood);
            for (int i = 0; i < 2; i++) Shape(PrimitiveType.Cube, "Nursery support " + i, nursery, Ground(-12.5f + i * 3f, -7f) + Vector3.up * 0.3f, new Vector3(0.18f, 0.6f, 0.8f), wood);
            GameObject board = Shape(PrimitiveType.Cube, "Nursery physical sign", nursery, Ground(-11f, -6f) + Vector3.up * 1.45f, new Vector3(3.7f, 0.55f, 0.12f), soil);
            Inscription("PERSEMAIAN / PEMULIHAN ARUNIKA", board.transform, 3.4f);
            AddSignSupports(nursery);
            GameObject ranger = Target("Forest ranger - Recovery guide", nursery, Ground(-6f, -9f));
            Transform coordinatorModel = disaster.Coordinator.Find("Coordinator visual - Ch42");
            GameObject guideModel = UnityEngine.Object.Instantiate(coordinatorModel.gameObject, ranger.transform);
            guideModel.name = "Ranger visual - Ch42 placeholder";
            guideModel.transform.localPosition = Vector3.zero;
            guideModel.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            Shape(PrimitiveType.Cube, "Ranger green badge", ranger.transform, ranger.transform.position + new Vector3(0f, 1.35f, -0.22f), new Vector3(0.25f, 0.14f, 0.025f), Material("Ranger badge", new Color(0.2f, 0.5f, 0.18f)));
            ranger.AddComponent<RangerDialogue>().Configure(chapter, UnityEngine.Object.FindFirstObjectByType<DisasterHud>(), Beacon("PETUGAS HUTAN", ranger.transform, 2.25f), Ring(ranger.transform));
            var seeds = new RecoverySeedling[4];
            for (int i = 0; i < seeds.Length; i++)
            {
                GameObject seed = Target("Recovery seedling " + (i + 1), nursery, Ground(-15f + i * 2.2f, -10.5f));
                Transform model = Group("Seedling pot visual", seed.transform);
                Shape(PrimitiveType.Cylinder, "Nursery pot", model, seed.transform.position + Vector3.up * 0.15f, new Vector3(0.38f, 0.15f, 0.38f), soil);
                PlaceKit("Prefabs/Nature/Tree/EA03_Nature_Tree_01b_PRE.prefab", "Young nursery tree", model, seed.transform.position + Vector3.up * 0.3f, 0.7f);
                seeds[i] = seed.AddComponent<RecoverySeedling>();
                seeds[i].Configure("recovery-seed-" + i, chapter, model.gameObject, Beacon("AMBIL BIBIT", seed.transform, 1.25f), Ring(seed.transform));
            }
            Transform planting = Group("02 - Prepared reforestation clearings", root.transform);
            Vector2[] positions = { new Vector2(-7f, 6f), new Vector2(8f, 10f), new Vector2(-8f, 27f), new Vector2(7f, 38f) };
            var beds = new RecoveryPlantingPoint[positions.Length];
            for (int i = 0; i < beds.Length; i++)
            {
                GameObject bed = Target("Recovery planting bed " + (i + 1), planting, Ground(positions[i].x, positions[i].y));
                GameObject dug = Shape(PrimitiveType.Cylinder, "Dug planting hole", bed.transform, bed.transform.position + Vector3.up * 0.025f, new Vector3(1.6f, 0.025f, 1.6f), soil);
                GameObject tended = Shape(PrimitiveType.Cylinder, "Watered soil and roots", bed.transform, bed.transform.position + Vector3.up * 0.035f, new Vector3(1.65f, 0.025f, 1.65f), moist);
                GameObject tree = PlaceKit("Prefabs/Nature/Tree/EA03_Nature_Tree_01b_PRE.prefab", "Young planted tree", bed.transform, bed.transform.position, 1.25f);
                beds[i] = bed.AddComponent<RecoveryPlantingPoint>();
                beds[i].Configure("recovery-bed-" + i, chapter, i, dug, tree, tended, Beacon("TANAM DAN SIRAM", bed.transform, 1.8f), Ring(bed.transform));
                AddWaterTarget(bed.transform);
                dug.SetActive(false); tended.SetActive(false); tree.SetActive(false);
            }
            Transform ecosystem = Group("03 - Water source and small wildlife habitats", root.transform);
            Vector3 poolCenter = Ground(0f, 29f);
            GameObject pool = Shape(PrimitiveType.Cylinder, "Recovery water source", ecosystem, poolCenter + Vector3.up * 0.035f, new Vector3(5f, 0.03f, 4f), dullWater);
            for (int i = 0; i < 7; i++)
            {
                float angle = (i + 1) * Mathf.PI / 8f;
                PlaceKit("Prefabs/Environment/Rock/EA03_EnvRock_01a_PRE.prefab", "Water edge rock " + i, ecosystem,
                    Ground(Mathf.Cos(angle) * 2.8f, 29f + Mathf.Sin(angle) * 2.3f), 0.5f);
            }
            var tasks = new RecoveryTask[5];
            for (int i = 0; i < 3; i++)
            {
                GameObject task = Target("Water source waste " + (i + 1), ecosystem, Ground(-3.2f + i * 3.2f, 25.5f));
                Transform trash = Group("Remaining ash and litter", task.transform);
                PlaceKit("Prefabs/Prop/Carpentry/EA03_Prop_Brushwood_03a_PRE.prefab", "Burnt waste branches", trash, task.transform.position, 0.7f, ash);
                Shape(PrimitiveType.Cube, "Discarded litter", trash, task.transform.position + new Vector3(0.4f, 0.13f, 0f), new Vector3(0.25f, 0.15f, 0.32f), Material("Recovery litter", new Color(0.42f, 0.38f, 0.29f)));
                tasks[i] = task.AddComponent<RecoveryTask>();
                tasks[i].Configure("recovery-water-waste-" + i, chapter, RecoveryTaskKind.Waste, trash.gameObject, null, Beacon("BERSIHKAN SUMBER AIR", task.transform, 1.4f), Ring(task.transform));
            }
            Vector2[] habitats = { new Vector2(0f, 10f), new Vector2(0f, 39f) };
            for (int i = 0; i < habitats.Length; i++)
            {
                GameObject habitat = Target("Small wildlife habitat " + (i + 1), ecosystem, Ground(habitats[i].x, habitats[i].y));
                GameObject before = PlaceKit("Prefabs/Prop/Carpentry/EA03_Prop_Brushwood_03a_PRE.prefab", "Damaged shelter branches", habitat.transform, habitat.transform.position, 1.2f, ash);
                Transform after = Group("Rebuilt brush shelter and native cover", habitat.transform);
                PlaceKit("Prefabs/Prop/Carpentry/EA03_Prop_Brushwood_03a_PRE.prefab", "Reassembled brush shelter", after, habitat.transform.position, 1.2f);
                for (int j = 0; j < 3; j++) PlaceKit("Prefabs/Nature/Bushes/EA03_Nature_Bush_01a_PRE.prefab", "Restored habitat cover " + j, after,
                    habitat.transform.position + new Vector3((j - 1) * 0.65f, 0f, 0.55f), 0.65f);
                tasks[i + 3] = habitat.AddComponent<RecoveryTask>();
                tasks[i + 3].Configure("recovery-habitat-" + i, chapter, RecoveryTaskKind.Habitat, before, after.gameObject, Beacon("PULIHKAN HABITAT", habitat.transform, 1.8f), Ring(habitat.transform));
                RefineHabitat(habitat.transform);
                after.gameObject.SetActive(false);
            }
            Transform wildlife = Group("Returning wildlife signs - static birds", ecosystem);
            Material birdMaterial = Material("Returning bird silhouettes", new Color(0.3f, 0.23f, 0.13f));
            for (int i = 0; i < 2; i++)
            {
                Vector3 p = Ground(i == 0 ? 0.9f : -0.9f, 39f) + Vector3.up * 0.35f;
                Shape(PrimitiveType.Sphere, "Small bird body " + i, wildlife, p, new Vector3(0.18f, 0.13f, 0.25f), birdMaterial);
                Shape(PrimitiveType.Sphere, "Small bird head " + i, wildlife, p + new Vector3(0f, 0.08f, -0.1f), Vector3.one * 0.12f, birdMaterial);
            }
            wildlife.gameObject.SetActive(false);
            chapter.Configure(disaster, ranger.transform, seeds, beds, tasks);
            ParticleSystem smoke = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<ParticleSystem>(true)).First(p => p.name == "Low wildfire smoke drifting over forest");
            root.AddComponent<RecoveryEnvironment>().Configure(chapter, pool.GetComponent<Renderer>(), dullWater, cleanWater, wildlife.gameObject, smoke, RenderSettings.sun);
            foreach (Transform marker in root.GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith("Recovery beacon - "))) marker.gameObject.SetActive(false);
            IndonesianText.ApplyStaticLabels(UnityEngine.Object.FindFirstObjectByType<RimbaraHud>().View.Canvas.transform);
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            VerifyRecovery();
        }

        public static void VerifyRecovery()
        {
            GameObject root = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(r => r.name == RootName);
            RecoveryChapter chapter = UnityEngine.Object.FindFirstObjectByType<RecoveryChapter>();
            if (root == null || chapter == null) throw new InvalidOperationException("Missing authored Recovery extension.");
            RecoverySeedling[] seeds = root.GetComponentsInChildren<RecoverySeedling>(true);
            RecoveryPlantingPoint[] beds = root.GetComponentsInChildren<RecoveryPlantingPoint>(true);
            RecoveryTask[] tasks = root.GetComponentsInChildren<RecoveryTask>(true);
            bool river = root.GetComponentInChildren<RecoveryRiver>(true) != null;
            int wasteGoal = river ? RecoveryRiverLayout.TrashCount : 3;
            if (seeds.Length != 4 || beds.Length != 4 || tasks.Count(t => t.Kind == RecoveryTaskKind.Waste) != wasteGoal || tasks.Count(t => t.Kind == RecoveryTaskKind.Habitat) != 2
                || root.GetComponentInChildren<RangerDialogue>(true) == null || root.GetComponent<RecoveryEnvironment>() == null) throw new InvalidOperationException("Recovery target supply/wiring invalid.");
            InteractableBehaviour[] targets = root.GetComponentsInChildren<InteractableBehaviour>(true);
            if (targets.Any(t => string.IsNullOrWhiteSpace(t.ObjectId) || t.gameObject.layer != 10 || !t.GetComponent<Collider>().isTrigger)
                || targets.Select(t => t.ObjectId).Distinct().Count() != targets.Length) throw new InvalidOperationException("Recovery requires unique IDs and interaction triggers.");
            var serialized = new SerializedObject(chapter);
            if (serialized.FindProperty("disaster").objectReferenceValue == null || serialized.FindProperty("ranger").objectReferenceValue == null
                || serialized.FindProperty("seedlings").arraySize != 4 || serialized.FindProperty("beds").arraySize != 4 || serialized.FindProperty("tasks").arraySize != wasteGoal + 2)
                throw new InvalidOperationException("Missing Recovery serialized references.");
            if (beds.Any(b => b.transform.Find("Young planted tree").gameObject.activeSelf)) throw new InvalidOperationException("Saplings must be hidden before planting.");
            if (beds.Any(b => b.transform.Find("Sapling water target") == null || b.transform.Find("Sapling water target").gameObject.layer != 11))
                throw new InvalidOperationException("Every sapling requires a WaterTarget-layer trigger.");
            foreach (InteractableBehaviour target in targets)
            {
                var data = new SerializedObject(target);
                if (data.FindProperty("chapter").objectReferenceValue != chapter)
                    throw new InvalidOperationException("Recovery interaction is bound to another chapter: " + target.name);
                string[] required = target is RecoverySeedling ? new[] { "visual", "beacon" }
                    : target is RecoveryPlantingPoint ? new[] { "preparedSoil", "youngTree", "tendedSoil", "beacon" }
                    : target is RecoveryTask ? new[] { "damaged", "beacon" } : new[] { "hud", "beacon" };
                if (required.Any(field => data.FindProperty(field).objectReferenceValue == null))
                    throw new InvalidOperationException("Recovery interaction visual/UI missing: " + target.name);
            }
            ProjectBuilder.WriteArtifact("recovery-verification.json", "{\"scene\":\"" + StorylineBuilder.ScenePath
                + "\",\"seedlings\":4,\"plantingBeds\":4,\"waste\":" + wasteGoal + ",\"habitats\":2,\"language\":\"id-ID\",\"result\":\"passed\"}");
            if (river) RecoveryRiverBuilder.VerifyRiver();
        }

        public static void RepairRecoveryPresentation()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.OpenScene(StorylineBuilder.ScenePath, OpenSceneMode.Single);
            GameObject root = scene.GetRootGameObjects().First(r => r.name == RootName);
            wood = Material("Recovery timber", new Color(0.28f, 0.17f, 0.08f));
            Transform nursery = root.transform.Find("01 - Nursery and forest ranger");
            foreach (Transform t in nursery.GetComponentsInChildren<Transform>(true))
                if (t.name == "Nursery work table" || t.name.StartsWith("Nursery support ")) t.GetComponent<Renderer>().sharedMaterial = wood;
            AddSignSupports(nursery);
            foreach (RecoveryPlantingPoint bed in root.GetComponentsInChildren<RecoveryPlantingPoint>(true)) AddWaterTarget(bed.transform);
            foreach (RecoveryTask habitat in root.GetComponentsInChildren<RecoveryTask>(true).Where(t => t.Kind == RecoveryTaskKind.Habitat)) RefineHabitat(habitat.transform);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets(); VerifyRecovery();
        }

        private static void AddSignSupports(Transform nursery)
        {
            if (nursery.Find("Nursery sign support 0") != null) return;
            for (int i = 0; i < 2; i++) Shape(PrimitiveType.Cube, "Nursery sign support " + i, nursery,
                Ground(-12.5f + i * 3f, -6f) + Vector3.up * 0.75f, new Vector3(0.14f, 1.5f, 0.14f), wood);
        }

        private static void AddWaterTarget(Transform bed)
        {
            if (bed.Find("Sapling water target") != null) return;
            GameObject target = Group("Sapling water target", bed).gameObject; target.layer = 11;
            SphereCollider trigger = target.AddComponent<SphereCollider>(); trigger.isTrigger = true; trigger.center = Vector3.up * 0.55f; trigger.radius = 0.55f; trigger.enabled = false;
        }

        private static void RefineHabitat(Transform habitat)
        {
            foreach (Transform shelter in habitat.GetComponentsInChildren<Transform>(true)
                .Where(t => t.name == "Damaged shelter branches" || t.name == "Reassembled brush shelter").ToArray())
            {
                // Kit bundles have a very low height/width ratio; height-only
                // scaling makes them wall-sized. Fit a small animal shelter.
                Bounds bounds = Bounds(shelter.gameObject);
                float width = Mathf.Max(bounds.size.x, bounds.size.z);
                if (width > 1.8f) shelter.localScale *= 1.8f / width;
                bounds = Bounds(shelter.gameObject);
                shelter.position += habitat.position - new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            }
            Transform restored = habitat.Find("Rebuilt brush shelter and native cover");
            if (restored.Find("Habitat ground cover 0") != null) return;
            for (int i = 0; i < 4; i++)
            {
                PlaceKit("Prefabs/Nature/Grass/EA03_Plant_Grass_01c_PRE.prefab", "Habitat ground cover " + i, restored,
                    habitat.position + new Vector3((i - 1.5f) * 0.45f, 0f, -0.65f), 0.3f);
            }
        }

        private static Transform Group(string name, Transform parent) { var go = new GameObject(name); go.transform.SetParent(parent, false); return go.transform; }
        private static Vector3 Ground(float x, float z) => new Vector3(x, Terrain.activeTerrain.SampleHeight(new Vector3(x, 0f, z)) + Terrain.activeTerrain.transform.position.y + 0.06f, z);
        private static GameObject Target(string name, Transform parent, Vector3 position)
        {
            GameObject go = Group(name, parent).gameObject; go.layer = 10; go.transform.position = position;
            SphereCollider trigger = go.AddComponent<SphereCollider>(); trigger.isTrigger = true; trigger.center = Vector3.up * 0.55f; trigger.radius = 0.65f; return go;
        }
        private static GameObject Shape(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent, true);
            go.transform.position = position; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); go.layer = 2; return go;
        }
        private static GameObject PlaceKit(string path, string name, Transform parent, Vector3 position, float height, Material material = null)
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(DisasterMapBuilder.KitRoot + path);
            if (source == null) throw new InvalidOperationException("Missing kit resource " + path);
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(source, parent); go.name = name;
            Bounds bounds = Bounds(go); go.transform.localScale *= height / Mathf.Max(0.1f, bounds.size.y);
            bounds = Bounds(go); go.transform.position += position - new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            foreach (Collider collider in go.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            foreach (Transform t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = 2;
            if (material != null) foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>(true)) renderer.sharedMaterials = Enumerable.Repeat(material, renderer.sharedMaterials.Length).ToArray();
            return go;
        }
        private static Bounds Bounds(GameObject go) { Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true); var b = renderers[0].bounds; foreach (Renderer r in renderers) b.Encapsulate(r.bounds); return b; }
        private static Material Material(string name, Color color)
        {
            string path = Folder + "/" + name + ".mat"; Material existing = AssetDatabase.LoadAssetAtPath<Material>(path); if (existing != null) return existing;
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")); material.SetColor("_BaseColor", color); material.SetFloat("_Smoothness", 0.05f); AssetDatabase.CreateAsset(material, path); return material;
        }
        private static GameObject Ring(Transform parent) => EnvironmentFactory.Ring("Recovery focus ring", parent, 0.7f, AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Amber marker.mat"));
        private static GameObject Beacon(string text, Transform parent, float height)
        {
            GameObject go = Group("Recovery beacon - " + text, parent).gameObject;
            TMP_Text label = EnvironmentFactory.WorldText(text, go.transform, Vector3.up * height, EnvironmentFactory.Amber, 1.6f, 4.5f); label.font = font; return go;
        }
        private static void Inscription(string text, Transform surface, float width)
        {
            GameObject go = new GameObject("Physical inscription - " + text, typeof(RectTransform), typeof(TextMeshPro)); go.transform.SetParent(surface, false);
            go.transform.localPosition = new Vector3(0f, 0f, -0.6f); go.transform.localScale = new Vector3(1f / surface.lossyScale.x, 1f / surface.lossyScale.y, 1f / surface.lossyScale.z);
            TextMeshPro label = go.GetComponent<TextMeshPro>(); label.font = font; label.text = text; label.color = new Color(0.93f, 0.9f, 0.68f);
            label.fontSize = label.fontSizeMax = 2f; label.fontSizeMin = 1f; label.enableAutoSizing = true;
            label.alignment = TextAlignmentOptions.Center; label.rectTransform.sizeDelta = new Vector2(width, surface.lossyScale.y * 0.8f); label.textWrappingMode = TextWrappingModes.NoWrap;
        }
    }
}
