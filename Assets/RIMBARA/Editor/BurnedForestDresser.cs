using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rimbara.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rimbara.Editor
{
    // Hand-authored dressing pass over the already authored Disaster map. It is a
    // tool for the designer, not a generator: positions are chosen by hand and the
    // seed only varies rotations, sizes and the surface noise.
    public static class BurnedForestDresser
    {
        private const string EnvironmentName = DisasterMapBuilder.EnvironmentName;
        private const string KitRoot = DisasterMapBuilder.KitRoot;
        private const string MapFolder = "Assets/Settings/DisasterMap";
        private const string CharcoalPath = MapFolder + "/Charred timber.mat";
        private const string BaseHeightPath = "Assets/RIMBARA/Authoring/FromAshes-terrain-base.bytes";
        private const int Seed = 20261117;

        private static readonly string[] GeneratedGroups = { "BurnedTrees_Generated", "Houses_Generated", "Debris_Generated" };
        private static readonly string[] LayerPaths =
        {
            MapFolder + "/VillageSoil.terrainlayer",
            MapFolder + "/BurntSoil.terrainlayer",
            "Assets/Art/Terrain/Dirt.terrainlayer",
            "Assets/Art/Environment/DryEarth.terrainlayer",
            "Assets/Art/Environment/BurntEarth.terrainlayer"
        };
        private static readonly string[] TreePrefabs =
        {
            KitRoot + "Prefabs/Nature/Tree/EA03_Nature_Tree_01b_PRE.prefab",
            KitRoot + "Prefabs/Nature/Tree/EA03_Nature_Tree_02b_PRE.prefab",
            KitRoot + "Prefabs/Nature/Tree/EA03_Nature_Tree_03b_PRE.prefab"
        };

        [MenuItem("RIMBARA/Environment/Dress abandoned burned forest")]
        public static void Dress()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Scene scene = EditorSceneManager.OpenScene(StorylineBuilder.ScenePath, OpenSceneMode.Single);
            Terrain terrain = Terrain.activeTerrain;
            if (terrain == null) throw new InvalidOperationException("The authored RIMBARA terrain is missing.");
            TerrainData data = terrain.terrainData;
            GameObject environment = scene.GetRootGameObjects().FirstOrDefault(r => r.name == EnvironmentName);
            if (environment == null) throw new InvalidOperationException("An original RIMBARA Disaster map is required.");
            Material charcoal = AssetDatabase.LoadAssetAtPath<Material>(CharcoalPath);
            if (charcoal == null) throw new InvalidOperationException("Missing charred timber material: " + CharcoalPath);

            RemoveGenerated(scene);
            Physics.SyncTransforms();
            Directory.CreateDirectory("Backups");
            File.Copy(AssetDatabase.GetAssetPath(data),
                "Backups/DisasterTerrain-before-forest-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".asset", true);

            List<Rect> keepRects = CollectKeepBounds(scene);
            var rng = new System.Random(Seed);
            float[] offset = Enumerable.Range(0, 8).Select(_ => (float)rng.NextDouble() * 500f).ToArray();

            var features = new (float x, float z, float r, float amp)[]
            {
                (46f, 20f, 13f, 1.4f),
                (-45f, 28f, 15f, 1.9f),
                (-20f, 50f, 10f, -0.8f),
                (16f, 56f, 12f, 1.1f),
                (38f, -18f, 9f, -0.7f),
                (-46f, -8f, 11f, 1.3f),
                (-6f, 6f, 7f, -0.45f),
                (30f, 34f, 13f, 0.9f),
                (-32f, -2f, 8f, -0.5f)
            };
            Vector2[] fires = { new Vector2(-12f, 16f), new Vector2(13f, 29f) };

            int n = data.heightmapResolution;
            float sx = data.size.x, sy = data.size.y, sz = data.size.z;
            Vector3 origin = terrain.transform.position;
            float[,] heights = BaseHeights(data, n);
            var result = new float[n, n];
            for (int zi = 0; zi < n; zi++)
            {
                for (int xi = 0; xi < n; xi++)
                {
                    float x = origin.x + xi * sx / (n - 1);
                    float z = origin.z + zi * sz / (n - 1);
                    float minDistance = float.MaxValue;
                    foreach (Rect rect in keepRects) minDistance = Mathf.Min(minDistance, DistanceToRect(rect, x, z));

                    float keep = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 3.2f, minDistance));
                    keep *= Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(9f, 13f, Vector2.Distance(new Vector2(x, z), new Vector2(0f, -44f))));
                    float road = (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(7.5f, 11f, Mathf.Abs(x))))
                        * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-3f, -9f, z));
                    keep *= 1f - road;
                    float riverBand = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(10f, 14f, z))
                        * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(52f, 48f, z));
                    float riverEdge = Mathf.SmoothStep(0f, 1f,
                        Mathf.InverseLerp(4.5f, 8.5f, Mathf.Abs(x - RecoveryRiverLayout.CenterX(z))));
                    keep *= 1f - riverBand * (1f - riverEdge);
                    foreach (Vector2 fire in fires)
                        keep *= Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(6f, 9f, Vector2.Distance(new Vector2(x, z), fire)));

                    float baseGround = heights[zi, xi] * sy + origin.y;
                    float delta = 0f;
                    for (int o = 0; o < 3; o++)
                    {
                        float d = Mathf.PerlinNoise(x * (o == 0 ? 0.035f : o == 1 ? 0.085f : 0.21f) + offset[o * 2],
                            z * (o == 0 ? 0.035f : o == 1 ? 0.085f : 0.21f) + offset[o * 2 + 1]) - 0.5f;
                        delta += d * 2f * (o == 0 ? 0.42f : o == 1 ? 0.18f : 0.06f);
                    }
                    foreach (var f in features)
                    {
                        float dx = x - f.x, dz = z - f.z;
                        float q = (dx * dx + dz * dz) / (f.r * f.r);
                        if (q < 1f) delta += f.amp * Mathf.SmoothStep(0f, 1f, 1f - q);
                    }
                    if (minDistance < 7f || (z < -4f && Mathf.Abs(x) < 30f)) delta = Mathf.Max(0f, delta);
                    delta *= 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(4.5f, 7f, baseGround));
                    result[zi, xi] = baseGround + delta * keep;
                }
            }

            var houseSites = new (float x, float z)[] { (42f, -46f), (-38f, -48f) };
            foreach (var site in houseSites)
            {
                foreach (Rect rect in keepRects)
                    if (DistanceToRect(rect, site.x, site.z) < 11.5f)
                        throw new InvalidOperationException("A chosen house pad overlaps existing content near " + site.x + "," + site.z + ".");
                float target = Sample(result, origin, sx, sz, n, site.x, site.z);
                float pad = 11.5f, core = 7.5f;
                for (int zi = 0; zi < n; zi++)
                {
                    for (int xi = 0; xi < n; xi++)
                    {
                        float x = origin.x + xi * sx / (n - 1);
                        float z = origin.z + zi * sz / (n - 1);
                        float d = Vector2.Distance(new Vector2(x, z), new Vector2(site.x, site.z));
                        if (d > pad) continue;
                        float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(pad, core, d));
                        result[zi, xi] = Mathf.Lerp(result[zi, xi], target, t);
                    }
                }
            }

            var painted = new float[n, n];
            for (int zi = 0; zi < n; zi++)
                for (int xi = 0; xi < n; xi++)
                {
                    float world = result[zi, xi];
                    painted[zi, xi] = Mathf.Clamp01((world - origin.y) / sy);
                }
            data.SetHeights(0, 0, painted);

            TerrainLayer[] layers = LayerPaths.Select(p => AssetDatabase.LoadAssetAtPath<TerrainLayer>(p)).ToArray();
            int missing = Array.FindIndex(layers, l => l == null);
            if (missing >= 0) throw new InvalidOperationException("Missing terrain layer: " + LayerPaths[missing]);
            data.terrainLayers = layers;
            if (data.alphamapResolution != 256) data.alphamapResolution = 256;
            data.baseMapResolution = 256;
            int a = data.alphamapResolution;
            var map = new float[a, a, layers.Length];
            for (int zi = 0; zi < a; zi++)
            {
                for (int xi = 0; xi < a; xi++)
                {
                    float x = origin.x + xi * sx / (a - 1);
                    float z = origin.z + zi * sz / (a - 1);
                    float h = Sample(result, origin, sx, sz, n, x, z);
                    float gx = (Sample(result, origin, sx, sz, n, x + 0.75f, z) - Sample(result, origin, sx, sz, n, x - 0.75f, z)) / 1.5f;
                    float gz = (Sample(result, origin, sx, sz, n, x, z + 0.75f) - Sample(result, origin, sx, sz, n, x, z - 0.75f)) / 1.5f;
                    float slope = Mathf.Sqrt(gx * gx + gz * gz);
                    float village = (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-16f, -2f, z)))
                        * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(24f, 36f, Mathf.Abs(x))));
                    float forest = 1f - village;
                    float p1 = Mathf.PerlinNoise(x * 0.045f + offset[0], z * 0.045f + offset[1]);
                    float p2 = Mathf.PerlinNoise(x * 0.11f + offset[2], z * 0.11f + offset[3]);
                    float p3 = Mathf.PerlinNoise(x * 0.023f + offset[4], z * 0.023f + offset[5]);
                    float steep = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.18f, 0.55f, slope));
                    float high = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(2.4f, 5.2f, h));
                    float ash = (village * 0.65f + forest * (0.55f + 0.45f * (1f - p1))) * (1f - 0.55f * steep) * (1f - 0.65f * high);
                    ash += village * Mathf.Max(0f, p2 - 0.62f) * 1.5f;
                    float dirt = (0.15f + 0.6f * p2) * (0.35f + 0.65f * steep) * (1f - 0.6f * village) + village * 0.25f * (1f - p2);
                    float dry = (0.10f + 0.55f * p3 + high) * (0.30f + 0.70f * steep) * (0.25f + 0.75f * forest);
                    float charred = forest * Mathf.Max(0f, p1 - 0.5f) * 1.6f * (1f - 0.5f * steep) * (1f - 0.7f * high);
                    float[] weights = { village * (0.75f + 0.25f * (1f - p1)), ash + 0.001f, dirt + 0.001f, dry + 0.001f, charred + 0.001f };
                    float total = weights.Sum();
                    for (int l = 0; l < weights.Length; l++) map[zi, xi, l] = weights[l] / total;
                }
            }
            data.SetAlphamaps(0, 0, map);
            EditorUtility.SetDirty(data);

            Transform trees = Group("BurnedTrees_Generated", environment.transform);
            Transform houses = Group("Houses_Generated", environment.transform);
            Transform debris = Group("Debris_Generated", environment.transform);
            Func<float, float, float> ground = (x, z) => Sample(result, origin, sx, sz, n, x, z);

            PlaceHouse(KitRoot + "Prefabs/Town/Building/EA03_Town_House_Comp_01a_PRE.prefab", houses,
                "Abandoned farmhouse at the east bluff", houseSites[0].x, houseSites[0].z, 12f, 207.4f, ground, keepRects);
            PlaceHouse(KitRoot + "Prefabs/Town/Building/EA03_Town_House_Comp_02a_PRE.prefab", houses,
                "Abandoned longhouse on the western rise", houseSites[1].x, houseSites[1].z, 11.5f, 18.4f, ground, keepRects);

            var houseRects = new List<Rect>();
            foreach (Transform house in houses)
            {
                Bounds houseBounds = BoundsOf(house.gameObject);
                houseRects.Add(Rect.MinMaxRect(houseBounds.min.x - 1f, houseBounds.min.z - 1f,
                    houseBounds.max.x + 1f, houseBounds.max.z + 1f));
            }
            var treeKeep = new List<Rect>(keepRects);
            treeKeep.AddRange(houseRects);
            var placedTrees = new List<Vector2>();

            var standing = new (float x, float z, float width, int prefab, float tiltX, float tiltZ)[]
            {
                (40f, -38f, 4.1f, 0, 9.5f, -3f),
                (30f, -38f, 3.4f, 1, 1.5f, 2f),
                (-30f, 50f, 3.9f, 2, -2f, 1.5f),
                (-42f, 42f, 4.4f, 0, 1f, -2f),
                (-39f, 47f, 3.2f, 1, 2.5f, 1f),
                (-26f, -6f, 3.8f, 2, -1f, 2f),
                (25f, -2f, 3.6f, 0, 1.5f, -1.5f),
                (-16f, 44f, 4.6f, 1, -2f, -2.5f),
                (-14f, 34f, 3.3f, 2, 1f, 1f),
                (36f, 14f, 3.7f, 0, -1.5f, 2f),

                (32f, -45f, 4.3f, 0, 2f, -1.5f),
                (32f, -53f, 4.6f, 2, 1.5f, -2f),
                (41f, -36f, 3.5f, 1, -2f, 1f),
                (56f, -51f, 4.0f, 1, 7f, 1f),

                (-50f, -48f, 4.8f, 0, 1.5f, -1f),
                (-47f, -40f, 4.2f, 1, -1f, 2f),
                (-42f, -34f, 5.0f, 2, 2f, 1.5f),
                (-34f, -34f, 3.9f, 0, -1.5f, -2f),
                (-27f, -42f, 4.6f, 1, 1f, 2f),
                (-26f, -50f, 3.6f, 2, -2f, 1f),
                (-27f, -54f, 4.4f, 0, 1.5f, -1.5f),
                (-49f, -54f, 3.8f, 1, -1f, 1f),
                (-52f, -54f, 4.0f, 2, 2f, -2f),
                (-54f, -47f, 4.5f, 0, -1.5f, 1.5f),
                (-53f, -40f, 3.7f, 1, 1f, -1f),
                (-48f, -33f, 4.1f, 2, -2f, 2f),
                (-40f, -28f, 4.9f, 0, 1.5f, 1f),
                (-30f, -31f, 3.8f, 1, -1f, -2f),
                (-25f, -37f, 4.3f, 2, 2f, 1.5f),
                (-22f, -47f, 3.5f, 0, -1.5f, -1f),
                (-49f, -44f, 4.0f, 1, 1f, 2f),
                (-57f, -50f, 4.2f, 2, -2f, 1f),
                (-58f, -43f, 3.6f, 0, 1.5f, -2f),
                (-55f, -34f, 4.4f, 1, -1f, 1f),
                (-44f, -27f, 3.9f, 2, 2f, -1.5f)
            };
            for (int i = 0; i < standing.Length; i++)
            {
                var s = standing[i];
                float yaw = (float)rng.NextDouble() * 360f;
                float sink = s.tiltX > 6f ? 0.18f : 0.07f;
                PlaceStanding(TreePrefabs[s.prefab], trees, "Burned standing tree " + (i + 1), s.x, s.z, s.width, yaw, s.tiltX, s.tiltZ, sink, ground, charcoal, treeKeep, placedTrees, 2.8f);
            }

            var fallen = new (float x, float z, float width, int prefab)[]
            {
                (30f, -30f, 4.2f, 1),
                (-20f, 40f, 3.8f, 2),
                (54f, -50f, 4.5f, 1),
                (-56f, -53f, 4.6f, 2)
            };
            for (int i = 0; i < fallen.Length; i++)
            {
                var s = fallen[i];
                PlaceFallen(TreePrefabs[s.prefab], trees, "Burned fallen tree " + (i + 1), s.x, s.z, s.width,
                    (float)rng.NextDouble() * 360f, 78f + (float)rng.NextDouble() * 8f, ground, charcoal, treeKeep, placedTrees, 2f);
            }

            var debrisSites = new (string path, float x, float z, float width, bool tint)[]
            {
                (KitRoot + "Prefabs/Prop/Carpentry/EA03_Prop_Tool_Stump_02a_PRE.prefab", 23f, 1f, 1.1f, true),
                (KitRoot + "Prefabs/Prop/wooden/EA03_Prop_Village_Plank_01c_PRE.prefab", -26f, -2f, 1.6f, true),
                (KitRoot + "Prefabs/Prop/Container/EA03_Prop_Container_Barrel_01d_PRE.prefab", 34f, -41f, 0.9f, true),
                (KitRoot + "Prefabs/Prop/Carpentry/EA03_Prop_Brushwood_03a_PRE.prefab", 12f, 42f, 1.8f, true),
                (KitRoot + "Prefabs/Environment/Rock/EA03_Environment_Rock_Mini_CatHead_01a_PRE.prefab", 35f, -34f, 1.4f, false),
                (KitRoot + "Prefabs/Environment/Rock/EA03_EnvRock_01a_PRE.prefab", -36f, 22f, 2.0f, false)
            };
            for (int i = 0; i < debrisSites.Length; i++)
            {
                var s = debrisSites[i];
                PlaceFootprint(s.path, debris, "Burned forest debris " + (i + 1), s.x, s.z, s.width,
                    (float)rng.NextDouble() * 360f, 0f, 0f, ground, s.tint ? charcoal : null, treeKeep, 2.5f, placedTrees, 1.4f);
            }

            Physics.SyncTransforms();
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Abandoned burned forest dressed: " + standing.Length + " standing trees, " + fallen.Length
                + " fallen trees, 2 houses, " + debrisSites.Length + " debris pieces; terrain " + n + "x" + n
                + " reshaped and " + a + "x" + a + " repainted with " + layers.Length + " layers (seed " + Seed + ").");
        }

        private static void RemoveGenerated(Scene scene)
        {
            foreach (GameObject go in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (go == null || !GeneratedGroups.Contains(go.name)) continue;
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        private static List<Rect> CollectKeepBounds(Scene scene)
        {
            var rects = new List<Rect>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer is ParticleSystemRenderer || renderer is LineRenderer || renderer is TrailRenderer) continue;
                    if (renderer is SpriteRenderer) continue;
                    Bounds bounds = renderer.bounds;
                    if (bounds.size.x < 0.02f && bounds.size.z < 0.02f) continue;
                    if (bounds.size.x > 28f || bounds.size.z > 28f) continue;
                    rects.Add(Rect.MinMaxRect(bounds.min.x - 0.6f, bounds.min.z - 0.6f,
                        bounds.max.x + 0.6f, bounds.max.z + 0.6f));
                }
            }
            return rects;
        }

        private static float DistanceToRect(Rect rect, float x, float z)
        {
            float dx = Mathf.Max(rect.xMin - x, 0f, x - rect.xMax);
            float dz = Mathf.Max(rect.yMin - z, 0f, z - rect.yMax);
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        // The dresser is re-runnable: it always builds on the terrain as it was
        // before the first dress, so running the menu again cannot stack bumps.
        private static float[,] BaseHeights(TerrainData data, int resolution)
        {
            if (File.Exists(BaseHeightPath))
            {
                byte[] bytes = File.ReadAllBytes(BaseHeightPath);
                if (bytes.Length != resolution * resolution * sizeof(float))
                    throw new InvalidOperationException("Base heightmap " + BaseHeightPath
                        + " does not match the terrain heightmap resolution " + resolution + ".");
                var saved = new float[resolution, resolution];
                Buffer.BlockCopy(bytes, 0, saved, 0, bytes.Length);
                return saved;
            }

            float[,] heights = data.GetHeights(0, 0, resolution, resolution);
            Directory.CreateDirectory(Path.GetDirectoryName(BaseHeightPath));
            byte[] raw = new byte[resolution * resolution * sizeof(float)];
            Buffer.BlockCopy(heights, 0, raw, 0, raw.Length);
            File.WriteAllBytes(BaseHeightPath, raw);
            AssetDatabase.ImportAsset(BaseHeightPath);
            return heights;
        }

        private static float Sample(float[,] heights, Vector3 origin, float sizeX, float sizeZ, int resolution, float x, float z)
        {
            float fx = Mathf.Clamp((x - origin.x) / sizeX * (resolution - 1), 0f, resolution - 1f);
            float fz = Mathf.Clamp((z - origin.z) / sizeZ * (resolution - 1), 0f, resolution - 1f);
            int x0 = Mathf.Min(Mathf.FloorToInt(fx), resolution - 2);
            int z0 = Mathf.Min(Mathf.FloorToInt(fz), resolution - 2);
            float tx = fx - x0, tz = fz - z0;
            float a = Mathf.Lerp(heights[z0, x0], heights[z0, x0 + 1], tx);
            float b = Mathf.Lerp(heights[z0 + 1, x0], heights[z0 + 1, x0 + 1], tx);
            return Mathf.Lerp(a, b, tz);
        }

        private static void PlaceStanding(string path, Transform parent, string name, float x, float z, float width,
            float yaw, float tiltX, float tiltZ, float sink, Func<float, float, float> ground, Material tint, List<Rect> keep,
            List<Vector2> avoid, float separation)
        {
            float[] spot = ClearSpot(x, z, 2.5f, keep, avoid, separation);
            GameObject go = Instantiate(path, parent, name, width, yaw, new Vector3(spot[0], 0f, spot[1]));
            go.transform.rotation = Quaternion.Euler(tiltX, yaw, tiltZ);
            go.transform.position = new Vector3(spot[0], ground(spot[0], spot[1]) - sink, spot[1]);
            Tint(go, tint);
            avoid.Add(new Vector2(spot[0], spot[1]));
        }

        private static void PlaceFallen(string path, Transform parent, string name, float x, float z, float width,
            float yaw, float roll, Func<float, float, float> ground, Material tint, List<Rect> keep,
            List<Vector2> avoid, float separation)
        {
            float[] spot = ClearSpot(x, z, 2.5f, keep, avoid, separation);
            GameObject go = Instantiate(path, parent, name, width, yaw, new Vector3(spot[0], 0f, spot[1]));
            go.transform.rotation = Quaternion.Euler(0f, yaw, roll);
            go.transform.position = new Vector3(spot[0], ground(spot[0], spot[1]) + 0.15f, spot[1]);
            Tint(go, tint);
            avoid.Add(new Vector2(spot[0], spot[1]));
        }

        private static void PlaceHouse(string path, Transform parent, string name, float x, float z, float width,
            float yaw, Func<float, float, float> ground, List<Rect> keep)
        {
            PlaceFootprint(path, parent, name, x, z, width, yaw, 0f, 0f, ground, null, keep, 10f);
        }

        private static void PlaceFootprint(string path, Transform parent, string name, float x, float z, float width,
            float yaw, float tiltX, float tiltZ, Func<float, float, float> ground, Material tint, List<Rect> keep, float clearance,
            List<Vector2> avoid = null, float separation = 0f)
        {
            float[] spot = ClearSpot(x, z, clearance, keep, avoid, separation);
            GameObject go = Instantiate(path, parent, name, width, yaw, new Vector3(spot[0], 0f, spot[1]));
            go.transform.rotation = Quaternion.Euler(tiltX, yaw, tiltZ);
            Bounds bounds = BoundsOf(go);
            go.transform.position += new Vector3(spot[0] - bounds.center.x, 0f, spot[1] - bounds.center.z);
            bounds = BoundsOf(go);
            go.transform.position += Vector3.up * (ground(spot[0], spot[1]) - bounds.min.y);
            if (tint != null) Tint(go, tint);
        }

        private static float[] ClearSpot(float x, float z, float clearance, List<Rect> keep,
            List<Vector2> avoid = null, float separation = 0f)
        {
            if (IsClear(x, z, clearance, keep) && IsSeparated(x, z, avoid, separation)) return new[] { x, z };
            for (int step = 1; step <= 16; step++)
            {
                float radius = step * 0.8f;
                for (int angle = 0; angle < 8; angle++)
                {
                    float a = angle * Mathf.PI * 0.25f + step * 0.37f;
                    float px = x + Mathf.Cos(a) * radius, pz = z + Mathf.Sin(a) * radius;
                    if (IsClear(px, pz, clearance, keep) && IsSeparated(px, pz, avoid, separation)) return new[] { px, pz };
                }
            }
            return new[] { x, z };
        }

        private static bool IsSeparated(float x, float z, List<Vector2> avoid, float separation)
        {
            if (avoid == null || separation <= 0f) return true;
            foreach (Vector2 point in avoid)
            {
                float dx = point.x - x, dz = point.y - z;
                if (dx * dx + dz * dz < separation * separation) return false;
            }
            return true;
        }

        private static bool IsClear(float x, float z, float clearance, List<Rect> keep)
        {
            foreach (Rect rect in keep)
                if (DistanceToRect(rect, x, z) < clearance) return false;
            return true;
        }

        private static GameObject Instantiate(string path, Transform parent, string name, float width, float yaw, Vector3 site)
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (source == null) throw new InvalidOperationException("Missing kit resource: " + path);
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(source, parent);
            go.name = name;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            Bounds bounds = BoundsOf(go);
            float factor = width / Mathf.Max(bounds.size.x, bounds.size.z, 0.1f);
            go.transform.localScale = Vector3.one * factor;
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            go.transform.position = new Vector3(site.x, 0f, site.z);
            foreach (Transform child in go.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = 8;
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(child.gameObject);
            }
            return go;
        }

        private static Bounds BoundsOf(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new InvalidOperationException("Kit resource has no renderer: " + go.name);
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
            return bounds;
        }

        private static void Tint(GameObject go, Material material)
        {
            foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterials = Enumerable.Repeat(material, renderer.sharedMaterials.Length).ToArray();
        }

        private static Transform Group(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }
    }
}


