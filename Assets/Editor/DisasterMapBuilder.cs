using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rimbara.Interaction;
using Rimbara.Mission;
using Rimbara.Player;
using Rimbara.UI;
using TMPro;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Rimbara.Editor
{
    // Builds RIMBARA's own level from individual kit resources. Never opens or
    // copies a vendor demo scene, village hierarchy, terrain or prefab placement.
    public static class DisasterMapBuilder
    {
        public const string EnvironmentName = "Environment - RIMBARA Disaster Zone (EmaceArt resources)";
        public const string KitRoot = "Assets/EmaceArt/Slavic World Free/";
        private const string MapFolder = "Assets/Settings/DisasterMap";
        private static readonly List<Bounds> occupied = new List<Bounds>();
        private static readonly List<string> resources = new List<string>();
        private static Material charcoal;

        [MenuItem("RIMBARA/Storyline/Author original Disaster map from kit resources")]
        public static void BuildMap()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.OpenScene(StorylineBuilder.ScenePath, OpenSceneMode.Single);
            if (scene.GetRootGameObjects().Any(r => r.name == EnvironmentName))
                throw new InvalidOperationException("The original Disaster map already exists. Edit it directly; this authoring operation does not regenerate it.");
            Directory.CreateDirectory("Backups");
            EditorSceneManager.SaveScene(scene, "Backups/FromAshes-before-original-map-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".unity", true);
            var objectives = new GameObject("Preserved storyline objectives");
            foreach (MonoBehaviour component in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<MonoBehaviour>(true)).ToArray())
                if (component is FirePoint || component is SeedlingPickup || component is PlantingPoint || component is MissionBoard || component is MissionZone)
                    component.transform.SetParent(objectives.transform, true);
            // Only environments in the derived scene. Never modify source scenes.
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == "Environment - EmaceArt Slavic Village" || root.name == "Arunika - authored restoration map"
                    || root.name.StartsWith("RIMBARA_") || root.name == "Sun - static late morning" || root.name == "Atmosphere - static grading")
                    UnityEngine.Object.DestroyImmediate(root);
            EnsureFolder(MapFolder);
            charcoal = CreateMaterial("Charred timber", new Color(0.075f, 0.065f, 0.055f));
            occupied.Clear(); resources.Clear();
            var environment = new GameObject(EnvironmentName);
            Transform village = Group("01 - Quiet emergency village", environment.transform);
            Transform post = Group("02 - Emergency mission post", environment.transform);
            Transform forest = Group("03 - Burnt forest and cleanup clearings", environment.transform);
            Transform border = Group("04 - Distant smoke and forest boundary", environment.transform);
            CreateTerrain(environment.transform);
            BuildVillage(village);
            BuildPost(post);
            BuildForest(forest, border);
            WireObjectives(forest, post);
            ConfigurePresentation(environment.transform);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            IndonesianSceneLocalization.Apply();
            VerifyMap();
            StorylineBuilder.VerifyChapter();
        }

        private static void CreateTerrain(Transform parent)
        {
            const string path = MapFolder + "/DisasterTerrain.asset";
            var data = new TerrainData { heightmapResolution = 129, size = new Vector3(120f, 8f, 120f) };
            float[,] heights = new float[129, 129];
            for (int z = 0; z < 129; z++)
                for (int x = 0; x < 129; x++)
                {
                    float wx = -60f + x * 120f / 128f, wz = -55f + z * 120f / 128f;
                    float edge = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((Mathf.Max(Mathf.Abs(wx) / 50f, Mathf.Abs(wz - 5f) / 52f) - 0.65f) / 0.35f));
                    if (Mathf.Abs(wx) < 8f && wz < 0f) edge = 0f;
                    heights[z, x] = edge * (0.08f + Mathf.PerlinNoise(wx * 0.04f + 4f, wz * 0.04f + 2f) * 0.2f);
                }
            data.SetHeights(0, 0, heights);
            // The kit supplies a palette atlas, not a repeating ground texture.
            // Derive coherent soil/ash from its earth swatch instead of tiling
            // the whole multicolour atlas across terrain.
            Texture2D source = KitGroundTexture();
            Texture2D texture = DerivedSoil(source, "Village-soil", false);
            Texture2D burntTexture = DerivedSoil(source, "Burnt-soil", true);
            var villageLayer = new TerrainLayer { diffuseTexture = texture, tileSize = new Vector2(8f, 8f),
                diffuseRemapMin = Vector4.zero, diffuseRemapMax = Vector4.one };
            var burntLayer = new TerrainLayer { diffuseTexture = burntTexture, tileSize = new Vector2(6f, 6f),
                diffuseRemapMin = Vector4.zero, diffuseRemapMax = Vector4.one };
            AssetDatabase.CreateAsset(villageLayer, MapFolder + "/VillageSoil.terrainlayer");
            AssetDatabase.CreateAsset(burntLayer, MapFolder + "/BurntSoil.terrainlayer");
            data.terrainLayers = new[] { villageLayer, burntLayer };
            data.alphamapResolution = 128;
            float[,,] map = new float[128, 128, 2];
            for (int z = 0; z < 128; z++)
                for (int x = 0; x < 128; x++)
                {
                    float worldZ = -55f + z * 120f / 127f;
                    float burnt = Mathf.SmoothStep(0.15f, 1f, Mathf.Clamp01((worldZ + 12f) / 16f));
                    map[z, x, 0] = 1f - burnt; map[z, x, 1] = burnt;
                }
            data.SetAlphamaps(0, 0, map);
            AssetDatabase.CreateAsset(data, path);
            GameObject ground = Terrain.CreateTerrainGameObject(data);
            ground.name = "RIMBARA authored terrain - ash and emergency village soil";
            ground.transform.SetParent(parent); ground.transform.position = new Vector3(-60f, 0f, -55f); ground.layer = 8;
            Terrain terrain = ground.GetComponent<Terrain>(); terrain.drawInstanced = true;
            terrain.materialTemplate = new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));
            AssetDatabase.CreateAsset(terrain.materialTemplate, MapFolder + "/DisasterTerrain.mat");
            resources.Add(AssetDatabase.GetAssetPath(source));
        }

        private static Texture2D DerivedSoil(Texture2D atlas, string name, bool burnt)
        {
            RenderTexture temporary = RenderTexture.GetTemporary(atlas.width, atlas.height, 0, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            Graphics.Blit(atlas, temporary); RenderTexture.active = temporary;
            var readable = new Texture2D(atlas.width, atlas.height, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0f, 0f, atlas.width, atlas.height), 0, 0); readable.Apply();
            // Select an earth colour, excluding blue/green/bright plaster swatches.
            Color earth = readable.GetPixels().Where(c => c.r > c.g * 1.05f && c.g > c.b * 1.08f && c.r < 0.6f && c.r > 0.12f)
                .OrderBy(c => Mathf.Abs(c.r - 0.28f)).First();
            RenderTexture.active = previous; RenderTexture.ReleaseTemporary(temporary); UnityEngine.Object.DestroyImmediate(readable);
            var image = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            for (int z = 0; z < 128; z++) for (int x = 0; x < 128; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.17f + 2f, z * 0.17f + 6f);
                Color color = burnt ? Color.Lerp(new Color(0.065f, 0.06f, 0.05f), new Color(0.17f, 0.15f, 0.12f), n) : earth * Mathf.Lerp(0.8f, 1.2f, n);
                color.a = 1f; image.SetPixel(x, z, color);
            }
            image.Apply(); string path = MapFolder + "/" + name + ".png"; File.WriteAllBytes(path, image.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(image); AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        public static void RepairTerrainPalette()
        {
            Scene scene = EditorSceneManager.OpenScene(StorylineBuilder.ScenePath, OpenSceneMode.Single);
            Texture2D source = KitGroundTexture();
            var village = AssetDatabase.LoadAssetAtPath<TerrainLayer>(MapFolder + "/VillageSoil.terrainlayer");
            var burnt = AssetDatabase.LoadAssetAtPath<TerrainLayer>(MapFolder + "/BurntSoil.terrainlayer");
            village.diffuseTexture = DerivedSoil(source, "Village-soil", false);
            burnt.diffuseTexture = DerivedSoil(source, "Burnt-soil", true);
            village.diffuseRemapMax = burnt.diffuseRemapMax = Vector4.one;
            EditorUtility.SetDirty(village); EditorUtility.SetDirty(burnt); AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene);
        }

        private static Texture2D KitGroundTexture()
        {
            GameObject field = AssetDatabase.LoadAssetAtPath<GameObject>(KitRoot + "Prefabs/Terrain/Field/EA03_Environment_Field_Comp_01a.prefab");
            foreach (Renderer renderer in field.GetComponentsInChildren<Renderer>(true))
                foreach (Material material in renderer.sharedMaterials)
                    if (material != null)
                    {
                        Texture2D texture = material.HasProperty("_BaseMap") ? material.GetTexture("_BaseMap") as Texture2D : material.mainTexture as Texture2D;
                        if (texture != null) return texture;
                    }
            throw new InvalidOperationException("The kit's field resource needs its original ground texture.");
        }

        private static void BuildVillage(Transform parent)
        {
            string[] houses = { "Prefabs/Town/Building/EA03_Town_House_Comp_01a_PRE.prefab",
                "Prefabs/Town/Building/EA03_Town_House_Comp_02a_PRE.prefab", "Prefabs/Town/Building/EA03_Town_House_Comp_03a_PRE.prefab" };
            Vector3[] positions = { new Vector3(-17f, 0f, -36f), new Vector3(17f, 0f, -33f), new Vector3(-18f, 0f, -21f), new Vector3(18f, 0f, -19f) };
            for (int i = 0; i < positions.Length; i++)
                Place(houses[i % houses.Length], parent, "Quiet village house " + (i + 1), positions[i], i % 2 == 0 ? 90f : -90f, 11f, true);
            Place("Prefabs/Village/OutBuilding/EA03_Village_OutBuilding_Shed_01a_PRE.prefab", parent, "Evacuation storage shed", new Vector3(-29f, 0f, -10f), 90f, 7f, true);
            for (int i = 0; i < 11; i++)
                Place("Prefabs/Environment/Road/EA03_Environment_Road_Cobble_01a_PRE.prefab", parent, "Arrival road " + i,
                    new Vector3(0f, 0.02f, -46f + i * 3.5f), 0f, 5f, false);
            for (int i = 0; i < 6; i++)
                Place("Prefabs/Fence/Plank2/EA03_Fence_Plank_02a_PRE.prefab", parent, "Village garden fence " + i,
                    new Vector3(i < 3 ? -9f : 9f, 0f, -37f + (i % 3) * 4f), 0f, 3.8f, true);
            Place("Prefabs/Prop/Container/EA03_Prop_Container_Barrel_01d_PRE.prefab", parent, "Water reserve barrel", new Vector3(10f, 0f, -28f), 0f, 1.1f, true);
            Place("Prefabs/Prop/Container/EA03_Prop_Container_Crate_01a_PRE.prefab", parent, "Abandoned village supply crate", new Vector3(-10f, 0f, -29f), 20f, 1.2f, true);
            Place("Prefabs/Prop/Furniture/EA03_Prop_Town_Bench_01a_PRE.prefab", parent, "Empty village bench", new Vector3(-10f, 0f, -17f), 90f, 2.3f, true);
        }

        private static void BuildPost(Transform parent)
        {
            Place("Prefabs/Village/OutBuilding/EA03_Village_OutBuilding_WoodRoof_01b_PRE.prefab", parent,
                "Emergency shelter", new Vector3(13f, 0f, -8f), -90f, 9f, true);
            Place("Prefabs/Prop/Furniture/EA03_Prop_Tabble_01a_PRE.prefab", parent, "Coordinator briefing table", new Vector3(7f, 0f, -11f), 0f, 2.4f, true);
            Place("Prefabs/Prop/Container/EA03_Prop_Container_Crate_02a_PRE.prefab", parent, "Relief supplies", new Vector3(17f, 0f, -4f), 0f, 1.8f, true);
            Place("Prefabs/Prop/Container/EA03_Prop_Container_Barrel_01d_PRE.prefab", parent, "Firefighting water reserves", new Vector3(18f, 0f, -9f), 0f, 1.2f, true);
            // Original mission map/board, made from the kit's timber material.
            Material timber = KitMaterial("Prefabs/Prop/wooden/EA03_Prop_Village_Plank_01c_PRE.prefab");
            GameObject board = Primitive(parent, "Mission map board", new Vector3(4f, 1.5f, -6f), new Vector3(2.8f, 1.5f, 0.14f), timber, true);
            Primitive(parent, "Mission board left support", new Vector3(2.8f, 0.8f, -6f), new Vector3(0.16f, 1.6f, 0.16f), timber, true);
            Primitive(parent, "Mission board right support", new Vector3(5.2f, 0.8f, -6f), new Vector3(0.16f, 1.6f, 0.16f), timber, true);
            var face = Primitive(parent, "Fire area map", new Vector3(4f, 1.5f, -6.09f), new Vector3(2.5f, 1.2f, 0.02f), CreateMaterial("Emergency map parchment", new Color(0.65f, 0.55f, 0.37f)), false);
            for (int i = 0; i < 2; i++) Primitive(face.transform, "Marked fire location " + i, new Vector3(3.5f + i, 1.5f, -6.12f), new Vector3(0.14f, 0.14f, 0.025f), CreateMaterial("Map fire marks", new Color(0.7f, 0.14f, 0.05f)), false);
            TMP_Text sign = EnvironmentFactory.WorldText("EMERGENCY MISSION POST", parent, Vector3.zero, Color.black, 2.5f, 2.2f);
            AttachPostSign(sign, board.transform);
        }

        private static void AttachPostSign(TMP_Text sign, Transform board)
        {
            // A physical inscription, not a camera-facing objective beacon.
            // Keep world scale at one despite the board's nonuniform cube scale.
            foreach (Rimbara.World.Billboard billboard in sign.GetComponents<Rimbara.World.Billboard>())
                UnityEngine.Object.DestroyImmediate(billboard);
            sign.transform.SetParent(board, true);
            sign.transform.localScale = new Vector3(1f / board.lossyScale.x, 1f / board.lossyScale.y, 1f / board.lossyScale.z);
            sign.transform.localRotation = Quaternion.identity;
            // Front (-Z) of the parchment, with a small gap to prevent z-fighting.
            sign.transform.position = board.TransformPoint(new Vector3(0f, 0.29f, -0.5f)) - board.forward * 0.055f;
            sign.rectTransform.sizeDelta = new Vector2(2.2f, 0.28f);
            sign.color = new Color(0.14f, 0.09f, 0.045f);
            sign.text = IndonesianText.WorldLabel("EMERGENCY MISSION POST");
            sign.fontSize = sign.fontSizeMax = 2.5f;
            sign.fontSizeMin = 1.2f;
            sign.enableAutoSizing = true;
            sign.alignment = TextAlignmentOptions.Center;
            sign.textWrappingMode = TextWrappingModes.NoWrap;
            sign.ForceMeshUpdate();
        }

        [MenuItem("RIMBARA/Storyline/Attach emergency post sign to board")]
        public static void RepairPostSign()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.OpenScene(StorylineBuilder.ScenePath, OpenSceneMode.Single);
            GameObject environment = scene.GetRootGameObjects().First(r => r.name == EnvironmentName);
            Transform post = environment.transform.Find("02 - Emergency mission post");
            Transform board = post.Find("Mission map board");
            TMP_Text sign = post.GetComponentsInChildren<TextMeshPro>(true).Single(t => t.name == "Label - EMERGENCY MISSION POST");
            AttachPostSign(sign, board);
            EditorUtility.SetDirty(sign);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            VerifyMap();
            IndonesianSceneLocalization.Verify();
        }

        private static void BuildForest(Transform forest, Transform border)
        {
            var random = new System.Random(167010);
            string[] trees = { "Prefabs/Nature/Tree/EA03_Nature_Tree_01b_PRE.prefab", "Prefabs/Nature/Tree/EA03_Nature_Tree_02b_PRE.prefab", "Prefabs/Nature/Tree/EA03_Nature_Tree_03b_PRE.prefab" };
            // Intentional clear corridors: central walking route, fire approaches,
            // and cleanup patches. Scenery placement never blocks these reserves.
            for (int i = 0; i < 62; i++)
            {
                float x = -37f + (float)random.NextDouble() * 74f, z = -2f + (float)random.NextDouble() * 52f;
                if (Mathf.Abs(x) < 5.5f || NearObjective(x, z) || occupied.Any(b => b.Contains(new Vector3(x, b.center.y, z)))) continue;
                GameObject tree = Place(trees[i % trees.Length], forest, "Scorched standing tree " + i, new Vector3(x, 0f, z), i * 137.5f, 4f + i % 4, true);
                Tint(tree, charcoal);
            }
            for (int i = 0; i < 14; i++)
            {
                float x = i % 2 == 0 ? -24f - i % 5 : 23f + i % 6;
                float z = 2f + i * 3f;
                GameObject fallen = Place(trees[i % trees.Length], forest, "Fallen charred tree " + i,
                    new Vector3(x, 0.15f, z), 40f + i * 31f, 5f, false);
                fallen.transform.rotation *= Quaternion.Euler(0f, 0f, 82f);
                Tint(fallen, charcoal);
                AddTrunkCollider(fallen);
            }
            for (int i = 0; i < 18; i++)
            {
                float x = i % 2 == 0 ? -33f : 32f, z = -4f + i * 2.6f;
                GameObject brush = Place("Prefabs/Prop/Carpentry/EA03_Prop_Brushwood_03a_PRE.prefab", forest, "Burnt branches " + i,
                    new Vector3(x, 0f, z), i * 73f, 2f, false);
                Tint(brush, charcoal);
            }
            for (int i = 0; i < 24; i++)
            {
                float angle = i * Mathf.PI * 2f / 24f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 48f, 0f, 12f + Mathf.Sin(angle) * 44f);
                if (Mathf.Abs(pos.x) < 6f && pos.z < 0f) continue;
                GameObject tree = Place(trees[i % trees.Length], border, "Damaged forest edge " + i, pos, i * 67f, 7f, true);
                Tint(tree, i % 4 == 0 ? CreateMaterial("Smoke-dulled foliage", new Color(0.14f, 0.16f, 0.1f)) : charcoal);
            }
            // Kit rocks/planks are resources, not its showcase cliff composition.
            for (int i = 0; i < 10; i++)
                Place("Prefabs/Environment/Rock/EA03_EnvRock_01a_PRE.prefab", border, "Forest boundary rock " + i,
                    new Vector3(i % 2 == 0 ? -43f : 43f, 0f, -28f + i * 8f), i * 37f, 4f, true);
        }

        private static bool NearObjective(float x, float z)
        {
            Vector2[] reserve = { new Vector2(-12f, 16f), new Vector2(13f, 29f), new Vector2(-7f, 6f), new Vector2(8f, 10f), new Vector2(-8f, 27f), new Vector2(7f, 38f) };
            return reserve.Any(p => Vector2.Distance(p, new Vector2(x, z)) < 6f);
        }

        private static void WireObjectives(Transform forest, Transform post)
        {
            DisasterChapter chapter = UnityEngine.Object.FindFirstObjectByType<DisasterChapter>();
            chapter.Coordinator.position = new Vector3(3f, 0.06f, -11f);
            EquipmentPickup equipment = UnityEngine.Object.FindFirstObjectByType<EquipmentPickup>();
            equipment.transform.position = new Vector3(10f, 0.06f, -14f);
            var equipmentData = new SerializedObject(equipment);
            if (equipment.transform.Find("Equipment chest - EmaceArt") == null)
            {
                Transform crate = equipment.transform.Find("Equipment crate");
                if (crate != null) UnityEngine.Object.DestroyImmediate(crate.gameObject);
                GameObject chest = Place("Prefabs/Prop/Container/EA03_Prop_Container_Chest_02a_PRE.prefab", equipment.transform,
                    "Equipment chest - EmaceArt", equipment.transform.position, 0f, 1.2f, false);
                equipmentData.FindProperty("pickupVisual").objectReferenceValue = chest;
                equipmentData.ApplyModifiedPropertiesWithoutUndo();
            }
            foreach (SeedlingPickup seed in UnityEngine.Object.FindObjectsByType<SeedlingPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None)) seed.gameObject.SetActive(false);
            foreach (PlantingPoint bed in UnityEngine.Object.FindObjectsByType<PlantingPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None)) bed.gameObject.SetActive(false);
            foreach (MissionZone zone in UnityEngine.Object.FindObjectsByType<MissionZone>(FindObjectsInactive.Include, FindObjectsSortMode.None)) zone.gameObject.SetActive(false);
            foreach (MissionBoard board in UnityEngine.Object.FindObjectsByType<MissionBoard>(FindObjectsInactive.Include, FindObjectsSortMode.None)) board.gameObject.SetActive(false);
            FirePoint[] fires = UnityEngine.Object.FindObjectsByType<FirePoint>(FindObjectsSortMode.None).OrderBy(f => f.Slot).ToArray();
            Vector3[] firePoints = { new Vector3(-12f, 0.05f, 16f), new Vector3(13f, 0.05f, 29f) };
            for (int i = 0; i < fires.Length; i++) { fires[i].transform.position = firePoints[i]; fires[i].transform.SetParent(forest, true); }
            DebrisPickup[] piles = UnityEngine.Object.FindObjectsByType<DebrisPickup>(FindObjectsSortMode.None).OrderBy(d => d.ObjectId).ToArray();
            Vector3[] debrisPoints = { new Vector3(-7f, 0.06f, 6f), new Vector3(8f, 0.06f, 10f), new Vector3(-8f, 0.06f, 27f), new Vector3(7f, 0.06f, 38f) };
            for (int i = 0; i < piles.Length; i++) { piles[i].transform.position = debrisPoints[i]; piles[i].transform.SetParent(forest, true); }
            VolunteerRig rig = UnityEngine.Object.FindFirstObjectByType<VolunteerRig>();
            Vector3 spawn = new Vector3(0f, 0f, -44f);
            Terrain terrain = Terrain.activeTerrain;
            spawn.y = terrain.SampleHeight(spawn) + terrain.transform.position.y + 0.12f;
            rig.transform.position = spawn;
            var data = new SerializedObject(rig);
            data.FindProperty("spawnPosition").vector3Value = rig.transform.position;
            data.FindProperty("recoveryRadius").floatValue = 100f;
            data.FindProperty("recoveryFloor").floatValue = -10f;
            data.ApplyModifiedPropertiesWithoutUndo();
            UnityEngine.Object.FindFirstObjectByType<DisasterHud>().ConfigureMap(new Vector2(0f, 3f), new Vector2(60f, 60f));
            // Other volunteers gather at the post; the village itself stays quiet.
            Transform npc = chapter.Coordinator.Find("Coordinator visual - Ch42");
            if (npc != null)
            {
                GameObject support = UnityEngine.Object.Instantiate(npc.gameObject, post);
                support.name = "Volunteer awaiting orders"; support.transform.position = new Vector3(17f, 0f, -12f);
                support.transform.rotation = Quaternion.Euler(0f, -80f, 0f);
                GameObject resident = UnityEngine.Object.Instantiate(npc.gameObject, post);
                resident.name = "Evacuated villager at relief post"; resident.transform.position = new Vector3(18f, 0f, -5f);
                resident.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }

        private static void ConfigurePresentation(Transform environment)
        {
            GameObject lightObject = new GameObject("Sun through wildfire smoke", typeof(Light));
            lightObject.transform.SetParent(environment); lightObject.transform.rotation = Quaternion.Euler(35f, -30f, 0f);
            Light sun = lightObject.GetComponent<Light>(); sun.type = LightType.Directional; sun.intensity = 1.2f;
            sun.color = new Color(0.75f, 0.67f, 0.52f); sun.shadows = LightShadows.Soft;
            RenderSettings.sun = sun; RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.28f, 0.27f, 0.24f);
            RenderSettings.ambientEquatorColor = new Color(0.19f, 0.17f, 0.14f);
            RenderSettings.ambientGroundColor = new Color(0.10f, 0.09f, 0.07f); RenderSettings.ambientIntensity = 0.9f;
            var sky = new Material(Shader.Find("Skybox/Procedural")); sky.SetColor("_SkyTint", new Color(0.28f, 0.24f, 0.2f));
            sky.SetColor("_GroundColor", new Color(0.1f, 0.09f, 0.08f)); sky.SetFloat("_AtmosphereThickness", 2.5f); sky.SetFloat("_Exposure", 0.65f);
            AssetDatabase.CreateAsset(sky, MapFolder + "/SmokeSky.mat"); RenderSettings.skybox = sky;
            DisasterChapter chapter = UnityEngine.Object.FindFirstObjectByType<DisasterChapter>();
            chapter.GetComponent<DisasterAtmosphere>().Configure(chapter, sun);
            // Windblown overhead smoke uses existing smoke FX material, but is
            // authored in our map, not imported with vendor demonstration weather.
            var smokeObject = new GameObject("Low wildfire smoke drifting over forest", typeof(ParticleSystem));
            smokeObject.transform.SetParent(environment); smokeObject.transform.position = new Vector3(0f, 7f, 19f);
            var particles = smokeObject.GetComponent<ParticleSystem>(); var main = particles.main;
            main.startLifetime = 22f; main.startSpeed = 0.25f; main.startSize = new ParticleSystem.MinMaxCurve(12f, 20f);
            main.startColor = new Color(0.18f, 0.17f, 0.16f, 0.22f); main.maxParticles = 70; main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = particles.emission; emission.rateOverTime = 2f;
            var shape = particles.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(75f, 2f, 55f);
            var velocity = particles.velocityOverLifetime; velocity.enabled = true; velocity.x = 0.6f;
            particles.GetComponent<ParticleSystemRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Soft particles.mat");
            chapter.GetComponent<DisasterAtmosphere>().ConfigureSmoke(particles);
            CinemachineCamera overview = UnityEngine.Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None).First(c => c.name.Contains("overview"));
            overview.transform.position = new Vector3(30f, 24f, -44f); overview.transform.LookAt(new Vector3(0f, 2f, 2f));
            foreach (TMP_Text label in UnityEngine.Object.FindFirstObjectByType<RimbaraHud>().View.Canvas.GetComponentsInChildren<TMP_Text>(true))
                if (label.name == "Map title") label.text = "ARUNIKA / DISASTER";
            Physics.SyncTransforms();
        }

        private static GameObject Place(string relative, Transform parent, string name, Vector3 position, float yaw, float width, bool solid)
        {
            string path = KitRoot + relative;
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (source == null) throw new InvalidOperationException("Missing kit resource: " + path);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(source, parent);
            instance.name = name; instance.transform.localPosition = Vector3.zero; instance.transform.localRotation = Quaternion.identity;
            Bounds bounds = BoundsOf(instance);
            float factor = width / Mathf.Max(bounds.size.x, bounds.size.z, 0.1f);
            instance.transform.localScale *= factor;
            instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            bounds = BoundsOf(instance);
            instance.transform.position += position - new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            foreach (Transform t in instance.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer = 8;
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            }
            foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true)) collider.enabled = solid;
            if (solid && !instance.GetComponentsInChildren<Collider>(true).Any(c => c.enabled))
            {
                Bounds world = BoundsOf(instance);
                GameObject proxy = new GameObject("Solid footprint"); proxy.transform.SetParent(parent); proxy.layer = 8;
                proxy.transform.position = world.center; proxy.AddComponent<BoxCollider>().size = world.size;
                proxy.transform.SetParent(instance.transform, true);
            }
            occupied.Add(BoundsOf(instance)); resources.Add(path);
            return instance;
        }

        private static void AddTrunkCollider(GameObject tree)
        {
            var collider = tree.AddComponent<BoxCollider>();
            Bounds bounds = BoundsOf(tree);
            collider.center = tree.transform.InverseTransformPoint(bounds.center);
            collider.size = new Vector3(0.3f, 3f, 0.3f);
        }
        private static Bounds BoundsOf(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new InvalidOperationException("Kit resource has no renderer: " + go.name);
            Bounds b = renderers[0].bounds; foreach (Renderer r in renderers) b.Encapsulate(r.bounds); return b;
        }
        private static void Tint(GameObject instance, Material material)
        {
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>()) renderer.sharedMaterials = Enumerable.Repeat(material, renderer.sharedMaterials.Length).ToArray();
        }
        private static Material KitMaterial(string relative)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(KitRoot + relative);
            Material material = go.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials).First(m => m != null);
            resources.Add(AssetDatabase.GetAssetPath(material)); return material;
        }
        private static GameObject Primitive(Transform parent, string name, Vector3 position, Vector3 scale, Material material, bool solid)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.transform.SetParent(parent, true);
            go.transform.position = position; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = material;
            go.layer = 8; if (!solid) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
        }
        private static Material CreateMaterial(string name, Color color)
        {
            string path = MapFolder + "/" + name + ".mat"; Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null) return m;
            m = new Material(Shader.Find("Universal Render Pipeline/Lit")); m.SetColor("_BaseColor", color); m.SetFloat("_Smoothness", 0f);
            AssetDatabase.CreateAsset(m, path); return m;
        }
        private static Transform Group(string name, Transform parent) { GameObject go = new GameObject(name); go.transform.SetParent(parent, false); return go.transform; }
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/'); EnsureFolder(parent); AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        public static void VerifyMap()
        {
            GameObject environment = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(r => r.name == EnvironmentName);
            if (environment == null || environment.transform.Find("01 - Quiet emergency village") == null
                || environment.transform.Find("03 - Burnt forest and cleanup clearings") == null)
                throw new InvalidOperationException("An original RIMBARA Disaster map is required, not the Slavic demo layout.");
            if (SceneManager.GetActiveScene().GetRootGameObjects().Any(r => r.name == "Environment - EmaceArt Slavic Village"))
                throw new InvalidOperationException("Copied demo village must not remain in the authored map.");
            int count = environment.GetComponentsInChildren<MeshFilter>(true).Count(m => m.sharedMesh != null && AssetDatabase.GetAssetPath(m.sharedMesh).StartsWith(KitRoot));
            if (count < 40 || Terrain.activeTerrain == null || environment.GetComponentInChildren<ParticleSystem>() == null)
                throw new InvalidOperationException("Disaster map requires kit resources, original terrain and smoke.");
            Transform board = environment.transform.Find("02 - Emergency mission post/Mission map board");
            TextMeshPro sign = board == null ? null : board.GetComponentInChildren<TextMeshPro>();
            if (sign == null || sign.text != "POSKO DARURAT" || sign.GetComponent<Rimbara.World.Billboard>() != null
                || Quaternion.Angle(sign.transform.rotation, board.rotation) > 0.1f)
                throw new InvalidOperationException("The emergency post sign must be attached to the board, not a floating billboard.");
            sign.ForceMeshUpdate();
            Vector3 signCenter = board.InverseTransformPoint(sign.transform.position);
            if (Mathf.Abs(signCenter.x) > 0.01f || signCenter.y < 0.2f || signCenter.y > 0.4f
                || signCenter.z >= -0.5f || signCenter.z < -1f || sign.isTextOverflowing)
                throw new InvalidOperationException("The emergency post inscription must fit on the front header of the board.");
            ProjectBuilder.WriteArtifact("post-sign-verification.json", "{\"scene\":\"" + StorylineBuilder.ScenePath
                + "\",\"text\":\"POSKO DARURAT\",\"attachedToBoard\":true,\"billboard\":false,\"overflow\":false,\"result\":\"passed\"}");
            ProjectBuilder.WriteArtifact("slavic-base-verification.json", "{\"scene\":\"" + StorylineBuilder.ScenePath
                + "\",\"base\":\"EmaceArt resources / original RIMBARA Disaster layout\",\"demoLayoutCopied\":false,\"kitMeshInstances\":" + count + ",\"result\":\"passed\"}");
            if (resources.Count > 0) ProjectBuilder.WriteArtifact("disaster-map-resources.json", JsonUtility.ToJson(new ResourceReport { Resources = resources.Distinct().ToArray() }, true));
        }
        [Serializable] private sealed class ResourceReport { public string[] Resources; }

        public static void ClearArrivalCorridor()
        {
            Scene scene = EditorSceneManager.OpenScene(StorylineBuilder.ScenePath, OpenSceneMode.Single);
            GameObject environment = scene.GetRootGameObjects().First(r => r.name == EnvironmentName);
            Terrain terrain = Terrain.activeTerrain;
            TerrainData data = terrain.terrainData;
            int resolution = data.heightmapResolution;
            float[,] heights = data.GetHeights(0, 0, resolution, resolution);
            for (int z = 0; z < resolution; z++) for (int x = 0; x < resolution; x++)
            {
                float wx = terrain.transform.position.x + x * data.size.x / (resolution - 1);
                float wz = terrain.transform.position.z + z * data.size.z / (resolution - 1);
                if (Mathf.Abs(wx) < 8f && wz < 0f) heights[z, x] = 0f;
            }
            data.SetHeights(0, 0, heights); EditorUtility.SetDirty(data);
            VolunteerRig rig = UnityEngine.Object.FindFirstObjectByType<VolunteerRig>();
            rig.transform.position = new Vector3(0f, 0.12f, -44f);
            var rigData = new SerializedObject(rig); rigData.FindProperty("spawnPosition").vector3Value = rig.transform.position;
            rigData.ApplyModifiedPropertiesWithoutUndo();
            foreach (Transform t in environment.GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith("Damaged forest edge ")).ToArray())
                if (Mathf.Abs(t.position.x) < 6f && t.position.z < 0f) UnityEngine.Object.DestroyImmediate(t.gameObject);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        }
    }
}
