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
    public static class RecoveryRiverBuilder
    {
        private const string Folder = "Assets/Settings/RecoveryMap";
        private const string RiverName = "Recovery river - original channel";

        [MenuItem("RIMBARA/Storyline/Replace Recovery pool with river")]
        public static void ReplacePool()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode before editing the river.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.OpenScene(StorylineBuilder.ScenePath, OpenSceneMode.Single);
            GameObject recoveryRoot = scene.GetRootGameObjects().First(r => r.name == RecoveryMapBuilder.RootName);
            if (recoveryRoot.GetComponentInChildren<RecoveryRiver>(true) != null)
                throw new InvalidOperationException("The river already exists. Edit it normally; do not replace it again.");
            Directory.CreateDirectory("Backups");
            string stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            EditorSceneManager.SaveScene(scene, "Backups/FromAshes-before-river-" + stamp + ".unity", true);
            Terrain terrain = Terrain.activeTerrain;
            File.Copy(AssetDatabase.GetAssetPath(terrain.terrainData), "Backups/DisasterTerrain-before-river-" + stamp + ".asset");
            RecoveryChapter chapter = UnityEngine.Object.FindFirstObjectByType<RecoveryChapter>();
            Transform ecosystem = recoveryRoot.transform.Find("03 - Water source and small wildlife habitats");
            GameObject pool = ecosystem.Find("Recovery water source").gameObject;
            // Remove only the obsolete Recovery pool and its authored edge rocks.
            UnityEngine.Object.DestroyImmediate(pool);
            foreach (Transform rock in ecosystem.Cast<Transform>().Where(t => t.name.StartsWith("Water edge rock ")).ToArray())
                UnityEngine.Object.DestroyImmediate(rock.gameObject);

            CarveChannel(terrain);
            Transform riverRoot = Group(RiverName, ecosystem);
            riverRoot.position = new Vector3(0f, 0f, 31.5f);
            Material dirty = WaterMaterial("Ash-affected river water", new Color(0.24f, 0.24f, 0.14f));
            Material clean = WaterMaterial("Cleaner recovery water", new Color(0.16f, 0.40f, 0.42f));
            Renderer water = MeshSurface("River water surface", riverRoot, MakeRibbon(false), dirty).GetComponent<Renderer>();
            MeshSurface("Exposed river banks", riverRoot, MakeRibbon(true), LitMaterial("Riverbank soil", new Color(0.28f, 0.24f, 0.15f)));
            Transform vegetation = Group("Recovering riverbank vegetation", riverRoot);
            for (int i = 0; i < 12; i++)
            {
                float z = 19f + i * 2.1f;
                Vector3 point = RecoveryRiverLayout.BankPoint(z, i % 2 == 0 ? -1 : 1, 2.55f);
                PlaceKit("Prefabs/Nature/Grass/EA03_Plant_Grass_01c_PRE.prefab", "Young riverbank grass " + i, vegetation, Ground(point), 0.32f);
            }
            vegetation.gameObject.SetActive(false);
            // Small kit resources decorate the bank, never form a copied demo layout.
            for (int i = 0; i < 14; i++)
            {
                Vector3 point = RecoveryRiverLayout.BankPoint(18.5f + i * 1.95f, i % 2 == 0 ? -1 : 1, 2.2f);
                PlaceKit("Prefabs/Environment/Rock/EA03_EnvRock_01a_PRE.prefab", "Riverbank stone " + i, riverRoot, Ground(point), 0.55f);
            }
            BuildBridge(riverRoot, 18f);
            BuildBridge(riverRoot, RecoveryRiverLayout.BridgeZ);
            AudioClip clip = AudioFactory.EnsureRiverFlow();
            var source = Group("River flow ambience", riverRoot).gameObject.AddComponent<AudioSource>();
            source.transform.position = new Vector3(0f, 0.3f, 31.5f);
            source.clip = clip; source.loop = true; source.playOnAwake = false; source.spatialBlend = 1f;
            source.minDistance = 6f; source.maxDistance = 45f; source.volume = 0.22f;
            riverRoot.gameObject.AddComponent<RecoveryRiver>().Configure(chapter, water, vegetation.gameObject, source);
            recoveryRoot.GetComponent<RecoveryEnvironment>().ConfigureWater(water, dirty, clean);

            RecoveryTask[] previous = ecosystem.GetComponentsInChildren<RecoveryTask>(true).Where(t => t.Kind == RecoveryTaskKind.Waste).OrderBy(t => t.ObjectId).ToArray();
            for (int i = 0; i < RecoveryRiverLayout.TrashCount; i++)
            {
                GameObject target = i < previous.Length ? previous[i].gameObject : Group("River trash " + (i + 1).ToString("00"), ecosystem).gameObject;
                // Keep the original 3 task IDs, rebuild only their litter visuals.
                if (i < previous.Length)
                    foreach (Transform child in target.transform.Cast<Transform>().ToArray()) UnityEngine.Object.DestroyImmediate(child.gameObject);
                target.name = "River trash " + (i + 1).ToString("00"); target.layer = 10;
                target.transform.position = Ground(RecoveryRiverLayout.TrashPosition(i));
                SphereCollider trigger = target.GetComponent<SphereCollider>();
                if (trigger == null) trigger = target.AddComponent<SphereCollider>();
                trigger.center = Vector3.up * 0.55f; trigger.radius = 0.65f; trigger.isTrigger = true;
                Transform litter = Group("River litter cluster", target.transform);
                BuildLitter(litter, i);
                Transform beacon = Group("Recovery beacon - SAMPAH SUNGAI", target.transform);
                GameObject label = new GameObject("Label - SAMPAH SUNGAI", typeof(RectTransform), typeof(TextMeshPro));
                label.transform.SetParent(beacon, false); label.transform.localPosition = Vector3.up * 1.4f;
                TextMeshPro text = label.GetComponent<TextMeshPro>(); text.font = UnityEngine.Object.FindFirstObjectByType<RimbaraHud>().View.MissionDescription.font;
                text.text = "SAMPAH SUNGAI " + (i + 1).ToString("00"); text.fontSize = 1.5f; text.color = EnvironmentFactory.Amber;
                text.alignment = TextAlignmentOptions.Center; text.rectTransform.sizeDelta = new Vector2(4f, 0.5f); label.AddComponent<Billboard>();
                label.layer = 2; beacon.gameObject.SetActive(false);
                GameObject ring = EnvironmentFactory.Ring("Recovery focus ring", target.transform, 0.7f,
                    AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Amber marker.mat"));
                RecoveryTask task = target.GetComponent<RecoveryTask>();
                if (task == null) task = target.AddComponent<RecoveryTask>();
                task.Configure("recovery-water-waste-" + i, chapter, RecoveryTaskKind.Waste, litter.gameObject, null, beacon.gameObject, ring);
            }

            // Relocate only Recovery habitats off the new channel onto its banks.
            RecoveryTask[] habitats = ecosystem.GetComponentsInChildren<RecoveryTask>(true).Where(t => t.Kind == RecoveryTaskKind.Habitat).OrderBy(t => t.ObjectId).ToArray();
            for (int i = 0; i < habitats.Length; i++) habitats[i].transform.position = Ground(RecoveryRiverLayout.BankPoint(i == 0 ? 17f : 41f, -1, 5f));
            Transform wildlife = ecosystem.Find("Returning wildlife signs - static birds");
            Vector3 birdShift = habitats[1].transform.position - new Vector3(0f, 0.06f, 39f);
            wildlife.position += birdShift;
            var data = new SerializedObject(chapter);
            RecoveryTask[] tasks = ecosystem.GetComponentsInChildren<RecoveryTask>(true).OrderBy(t => t.ObjectId).ToArray();
            data.FindProperty("tasks").arraySize = tasks.Length;
            for (int i = 0; i < tasks.Length; i++) data.FindProperty("tasks").GetArrayElementAtIndex(i).objectReferenceValue = tasks[i];
            data.ApplyModifiedPropertiesWithoutUndo();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            RecoveryMapBuilder.VerifyRecovery();
            IndonesianSceneLocalization.Verify();
        }

        private static void CarveChannel(Terrain terrain)
        {
            TerrainData data = terrain.terrainData;
            int resolution = data.heightmapResolution;
            float[,] heights = data.GetHeights(0, 0, resolution, resolution);
            for (int z = 0; z < resolution; z++) for (int x = 0; x < resolution; x++)
            {
                float wz = terrain.transform.position.z + z * data.size.z / (resolution - 1);
                if (wz < RecoveryRiverLayout.StartZ - 0.8f || wz > RecoveryRiverLayout.EndZ + 0.8f) continue;
                float wx = terrain.transform.position.x + x * data.size.x / (resolution - 1);
                float distance = Mathf.Abs(wx - RecoveryRiverLayout.CenterX(wz));
                if (distance >= 2.35f) continue;
                float weight = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(1.1f, 2.35f, distance));
                float original = heights[z, x] * data.size.y + terrain.transform.position.y;
                float lowered = Mathf.Lerp(original, -0.32f, weight);
                heights[z, x] = (lowered - terrain.transform.position.y) / data.size.y;
            }
            // Unity terrain cannot encode negative heights relative to its origin.
            // Lower the terrain origin by 0.5m while raising every height equally;
            // world elevations outside the channel remain identical.
            float shift = terrain.transform.position.y > -0.4f ? 0.5f : 0f;
            for (int z = 0; z < resolution; z++) for (int x = 0; x < resolution; x++) heights[z, x] += shift / data.size.y;
            terrain.transform.position -= Vector3.up * shift;
            data.SetHeights(0, 0, heights); EditorUtility.SetDirty(data);
        }

        public static void RepairChannel()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.OpenScene(StorylineBuilder.ScenePath, OpenSceneMode.Single);
            if (UnityEngine.Object.FindFirstObjectByType<RecoveryRiver>() == null)
                throw new InvalidOperationException("Add the authored river before repairing its channel.");
            Terrain terrain = Terrain.activeTerrain;
            // Capture world heights before sculpting. Do not deserialize the
            // backup TerrainData into the editor: it has the same asset identity.
            int resolution = terrain.terrainData.heightmapResolution;
            float[,] before = terrain.terrainData.GetHeights(0, 0, resolution, resolution);
            float oldOrigin = terrain.transform.position.y;
            CarveChannel(terrain);
            float[,] after = terrain.terrainData.GetHeights(0, 0, resolution, resolution);
            float maxOutside = 0f;
            int changed = 0;
            for (int z = 0; z < resolution; z++) for (int x = 0; x < resolution; x++)
            {
                float wz = terrain.transform.position.z + z * terrain.terrainData.size.z / (resolution - 1);
                float wx = terrain.transform.position.x + x * terrain.terrainData.size.x / (resolution - 1);
                float delta = Mathf.Abs(before[z, x] * terrain.terrainData.size.y + oldOrigin - (after[z, x] * terrain.terrainData.size.y + terrain.transform.position.y));
                bool channel = wz >= RecoveryRiverLayout.StartZ - 0.8f && wz <= RecoveryRiverLayout.EndZ + 0.8f
                    && Mathf.Abs(wx - RecoveryRiverLayout.CenterX(wz)) < 2.35f;
                if (channel) { if (delta > 0.01f) changed++; }
                else maxOutside = Mathf.Max(maxOutside, delta);
            }
            if (maxOutside > 0.003f) throw new InvalidOperationException("River repair changed terrain outside its channel.");
            ProjectBuilder.WriteArtifact("recovery-preservation.json", "{\"baseline\":\"f4ec160\",\"scene\":\"" + StorylineBuilder.ScenePath
                + "\",\"terrainOutsideChannelMaxDeltaMeters\":" + maxOutside.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture)
                + ",\"changedChannelPoints\":" + changed + ",\"terrainOriginCompensated\":true,\"passed\":true}");
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            VerifyRiver();
        }

        private static Mesh MakeRibbon(bool bank)
        {
            const int rows = 109;
            int strips = bank ? 2 : 1;
            var vertices = new Vector3[rows * 2 * strips]; var uvs = new Vector2[vertices.Length];
            var indices = new int[(rows - 1) * 6 * strips];
            for (int s = 0; s < strips; s++) for (int i = 0; i < rows; i++)
            {
                float z = Mathf.Lerp(RecoveryRiverLayout.StartZ, RecoveryRiverLayout.EndZ, i / (float)(rows - 1));
                float center = RecoveryRiverLayout.CenterX(z);
                int at = s * rows * 2 + i * 2;
                for (int edge = 0; edge < 2; edge++)
                {
                    float x = bank ? center + (s == 0 ? -1 : 1) * (edge == 0 ? RecoveryRiverLayout.HalfWidth : 2.35f)
                        : center + (edge == 0 ? -1 : 1) * RecoveryRiverLayout.HalfWidth;
                    vertices[at + edge] = new Vector3(x, bank ? edge == 0 ? -0.07f : 0.035f : RecoveryRiverLayout.WaterY, z - 31.5f);
                    uvs[at + edge] = new Vector2(edge, (z - RecoveryRiverLayout.StartZ) * 0.5f);
                }
                if (i == rows - 1) continue;
                int idx = (s * (rows - 1) + i) * 6;
                bool flip = bank && s == 0;
                indices[idx] = at; indices[idx + 1] = flip ? at + 1 : at + 2; indices[idx + 2] = flip ? at + 2 : at + 1;
                indices[idx + 3] = at + 1; indices[idx + 4] = flip ? at + 3 : at + 2; indices[idx + 5] = flip ? at + 2 : at + 3;
            }
            var mesh = new Mesh { name = bank ? "Original winding river banks" : "Original winding river surface" };
            mesh.vertices = vertices; mesh.uv = uvs; mesh.triangles = indices; mesh.RecalculateNormals(); mesh.RecalculateBounds();
            string path = Folder + (bank ? "/RiverBanks.asset" : "/RiverWater.asset");
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null) AssetDatabase.CreateAsset(mesh, path);
            else { EditorUtility.CopySerialized(mesh, existing); UnityEngine.Object.DestroyImmediate(mesh); mesh = existing; EditorUtility.SetDirty(mesh); }
            return mesh;
        }

        private static void BuildBridge(Transform river, float z)
        {
            Transform bridge = Group("River footbridge " + z.ToString("0"), river);
            bridge.position = new Vector3(RecoveryRiverLayout.CenterX(z), 0f, z);
            Material timber = LitMaterial("River bridge timber", new Color(0.3f, 0.19f, 0.09f));
            GameObject deck = Shape(PrimitiveType.Cube, "Solid bridge deck", bridge, new Vector3(0f, 0.11f, 0f), new Vector3(5.6f, 0.18f, 2.5f), timber);
            deck.layer = 8; deck.AddComponent<BoxCollider>();
            for (int i = 0; i < 13; i++) Shape(PrimitiveType.Cube, "Crossing plank " + i, bridge, new Vector3(-2.55f + i * 0.425f, 0.215f, 0f), new Vector3(0.38f, 0.05f, 2.5f), timber);
            for (int side = -1; side <= 1; side += 2)
            {
                // Leave the north bridge's end and walking deck clear.
                Shape(PrimitiveType.Cube, "Bridge railing " + side, bridge, new Vector3(0f, 0.9f, side * 1.2f), new Vector3(5.5f, 0.08f, 0.08f), timber);
                for (int i = 0; i < 3; i++) Shape(PrimitiveType.Cube, "Bridge post " + side + " " + i, bridge, new Vector3(-2.5f + i * 2.5f, 0.53f, side * 1.2f), new Vector3(0.09f, 1.05f, 0.09f), timber);
            }
        }

        private static void BuildLitter(Transform parent, int index)
        {
            Material plastic = LitMaterial("River discarded plastic", new Color(0.7f, 0.75f, 0.63f));
            Material cap = LitMaterial("River bottle caps", new Color(0.15f, 0.32f, 0.48f));
            Material bag = LitMaterial("River discarded bags", new Color(0.55f, 0.49f, 0.32f));
            Material metal = LitMaterial("River discarded cans", new Color(0.44f, 0.48f, 0.47f));
            // Each of the 10 interactable clusters contains several visible pieces.
            GameObject bottle = Shape(PrimitiveType.Cylinder, "Discarded plastic bottle", parent, new Vector3(-0.25f, 0.16f, 0f), new Vector3(0.18f, 0.24f, 0.18f), plastic);
            bottle.transform.localRotation = Quaternion.Euler(0f, index * 37f, 75f);
            Shape(PrimitiveType.Cylinder, "Bottle cap", bottle.transform, new Vector3(0f, 1.06f, 0f), new Vector3(0.7f, 0.1f, 0.7f), cap);
            GameObject sack = Shape(PrimitiveType.Sphere, "Crumpled plastic bag", parent, new Vector3(0.24f, 0.12f, 0.12f), new Vector3(0.44f, 0.18f, 0.34f), bag);
            sack.transform.localRotation = Quaternion.Euler(0f, index * 51f, 0f);
            Shape(PrimitiveType.Cylinder, "Discarded metal can", parent, new Vector3(0.07f, 0.11f, -0.27f), new Vector3(0.19f, 0.11f, 0.19f), metal);
        }

        public static void VerifyRiver()
        {
            RecoveryRiver river = UnityEngine.Object.FindFirstObjectByType<RecoveryRiver>();
            RecoveryTask[] trash = UnityEngine.Object.FindObjectsByType<RecoveryTask>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(t => t.Kind == RecoveryTaskKind.Waste).ToArray();
            if (river == null || trash.Length != RecoveryRiverLayout.TrashCount || trash.Select(t => t.ObjectId).Distinct().Count() != 10)
                throw new InvalidOperationException("Original river requires 10 unique trash targets.");
            MeshFilter water = river.transform.Find("River water surface").GetComponent<MeshFilter>();
            if (water.sharedMesh == null || water.sharedMesh.vertexCount < 100 || river.GetComponentsInChildren<BoxCollider>().Length != 2)
                throw new InvalidOperationException("River needs continuous authored water and two solid footbridges.");
            foreach (RecoveryTask task in trash)
            {
                Vector3 p = task.transform.position;
                if (Mathf.Abs(p.x - RecoveryRiverLayout.CenterX(p.z)) < 2.7f || p.z < 18f || p.z > 45f)
                    throw new InvalidOperationException("Trash must be reachable from a dry riverbank: " + task.name);
            }
            Terrain terrain = Terrain.activeTerrain;
            for (float z = RecoveryRiverLayout.StartZ + 1f; z < RecoveryRiverLayout.EndZ - 1f; z += 0.5f)
            {
                Vector3 p = new Vector3(RecoveryRiverLayout.CenterX(z), 0f, z);
                if (terrain.SampleHeight(p) + terrain.transform.position.y >= RecoveryRiverLayout.WaterY - 0.02f)
                    throw new InvalidOperationException("Terrain clips the river surface at z=" + z);
            }
            ProjectBuilder.WriteArtifact("river-verification.json", "{\"scene\":\"" + StorylineBuilder.ScenePath
                + "\",\"trash\":10,\"riverLengthMeters\":27,\"solidBridges\":2,\"demoLayoutCopied\":false,\"result\":\"passed\"}");
        }

        private static Transform Group(string name, Transform parent) { var go = new GameObject(name); go.transform.SetParent(parent, false); return go.transform; }
        private static Vector3 Ground(Vector3 p) { p.y = Terrain.activeTerrain.SampleHeight(p) + Terrain.activeTerrain.transform.position.y + 0.06f; return p; }
        private static GameObject Shape(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent, false); go.layer = 2;
            go.transform.localPosition = position; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
        }
        private static GameObject MeshSurface(string name, Transform parent, Mesh mesh, Material material)
        {
            GameObject go = Group(name, parent).gameObject; go.layer = 2; go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material; return go;
        }
        private static Material LitMaterial(string name, Color color)
        {
            string path = Folder + "/" + name + ".mat"; Material m = AssetDatabase.LoadAssetAtPath<Material>(path); if (m != null) return m;
            m = new Material(Shader.Find("Universal Render Pipeline/Lit")); m.SetColor("_BaseColor", color); m.SetFloat("_Smoothness", 0.1f); AssetDatabase.CreateAsset(m, path); return m;
        }
        private static Material WaterMaterial(string name, Color tint)
        {
            string path = Folder + "/" + name + ".mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(Shader.Find("Rimbara/Recovery River")); AssetDatabase.CreateAsset(m, path); }
            m.shader = Shader.Find("Rimbara/Recovery River"); m.SetColor("_BaseColor", tint); m.SetColor("_RecoveredTint", new Color(0.14f, 0.4f, 0.44f));
            EditorUtility.SetDirty(m); return m;
        }
        private static void PlaceKit(string path, string name, Transform parent, Vector3 p, float width)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(DisasterMapBuilder.KitRoot + path);
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent); go.name = name;
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true); Bounds b = renderers[0].bounds; foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
            go.transform.localScale *= width / Mathf.Max(b.size.x, b.size.z, 0.1f);
            b = renderers[0].bounds; foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
            go.transform.position += p - new Vector3(b.center.x, b.min.y, b.center.z);
            foreach (Collider c in go.GetComponentsInChildren<Collider>(true)) c.enabled = false;
            foreach (Transform t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = 2;
        }
    }
}
