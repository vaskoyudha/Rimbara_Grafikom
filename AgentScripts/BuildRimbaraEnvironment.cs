using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class BuildRimbaraEnvironment
{
    const string FantasyTown = "Assets/RIMBARA/Environment/Village/Models/FantasyTown/";
    const string Survival = "Assets/RIMBARA/Props/Models/Survival/";
    const string Furniture = "Assets/RIMBARA/Props/Models/Furniture/";
    const string Nature = "Assets/RIMBARA/Vegetation/Models/Nature/";
    const string OldNature = "Assets/Art/Environment/KenneyNature/";
    const string VillagePrefabs = "Assets/RIMBARA/Environment/Village/Prefabs/";
    const string AshMaterialPath = "Assets/RIMBARA/Materials/BurnedForest/M_Ash.mat";
    const string CharredBarkPath = "Assets/Art/Materials/Charred bark.mat";
    const string DarkStonePath = "Assets/RIMBARA/Materials/Palette/Palette_stoneDark.mat";
    const string DirtDarkPath = "Assets/RIMBARA/Materials/Palette/Palette_dirtDark.mat";

    enum ColliderKind { None, Box, Capsule }

    static Terrain terrain;
    static System.Random rng;
    static readonly List<Bounds> occupied = new List<Bounds>();
    static Material ashMaterial;
    static Material charredBark;
    static Material darkStone;
    static Material dirtDark;

    public static void BuildAll()
    {
        if (EditorApplication.isPlaying)
            throw new System.Exception("Exit play mode before building the environment.");

        var scene = SceneManager.GetActiveScene();
        terrain = Terrain.activeTerrain;
        if (terrain == null) throw new System.Exception("No active terrain found.");
        rng = new System.Random(20261111);

        var created = new List<GameObject>();
        try
        {
            ClearPrevious();
            LoadMaterials();
            CollectExistingObstacles();

            var village = new GameObject("RIMBARA_Village");
            created.Add(village);
            BuildVillage(village.transform);

            var burned = new GameObject("RIMBARA_BurnedForest");
            created.Add(burned);
            BuildBurnedForest(burned.transform);

            var restoration = new GameObject("RIMBARA_RestorationCamp");
            created.Add(restoration);
            BuildRestoration(restoration.transform);

            var recovered = new GameObject("RIMBARA_RecoveredForest");
            created.Add(recovered);
            BuildRecovered(recovered.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("RIMBARA_ENV_DONE villageRenderers=" + CountRenderers(village)
                + " burnedRenderers=" + CountRenderers(burned)
                + " restorationRenderers=" + CountRenderers(restoration)
                + " recoveredRenderers=" + CountRenderers(recovered)
                + " totalRenderers=" + (CountRenderers(village) + CountRenderers(burned) + CountRenderers(restoration) + CountRenderers(recovered)));
        }
        catch (System.Exception e)
        {
            foreach (var go in created)
                if (go != null) Object.DestroyImmediate(go);
            Debug.LogError("RIMBARA_ENV_FAILED " + e);
            throw;
        }
    }

    static void ClearPrevious()
    {
        foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
            if (go.name.StartsWith("RIMBARA_"))
                Object.DestroyImmediate(go);
    }

    static void LoadMaterials()
    {
        ashMaterial = EnsureMaterial(AshMaterialPath, new Color(0.16f, 0.14f, 0.12f, 1f), 0.05f);
        charredBark = AssetDatabase.LoadAssetAtPath<Material>(CharredBarkPath);
        darkStone = AssetDatabase.LoadAssetAtPath<Material>(DarkStonePath);
        dirtDark = AssetDatabase.LoadAssetAtPath<Material>(DirtDarkPath);
        if (charredBark == null) Debug.LogWarning("Missing " + CharredBarkPath);
        if (darkStone == null) Debug.LogWarning("Missing " + DarkStonePath);
        if (dirtDark == null) Debug.LogWarning("Missing " + DirtDarkPath);
    }

    static void CollectExistingObstacles()
    {
        occupied.Clear();
        foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (c == null || c.isTrigger || c is TerrainCollider) continue;
            if (c.transform.root.name.StartsWith("RIMBARA_")) continue;
            if (c.bounds.size.sqrMagnitude < 0.0001f) continue;
            occupied.Add(c.bounds);
        }

        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (r == null || r.transform.root.name.StartsWith("RIMBARA_")) continue;
            string rootName = r.transform.root.name;
            if (rootName != "Arunika - authored restoration map" && rootName != "Volunteer") continue;
            if (r.GetComponentInParent<Collider>() != null) continue;
            if (r.bounds.size.sqrMagnitude < 0.0001f) continue;
            if (r.bounds.size.x * r.bounds.size.y * r.bounds.size.z < 0.75f) continue;
            occupied.Add(r.bounds);
        }
    }

    static void BuildVillage(Transform root)
    {
        var houses = Child(root, "Houses");
        PlaceHouse(houses, "House_A", new Vector3(34.5f, 0f, 4f), 0f);
        PlaceHouse(houses, "House_B", new Vector3(27f, 0f, 4f), 0f);
        PlaceHouse(houses, "House_C", new Vector3(42.5f, 0f, -9f), 90f);
        PlaceHouse(houses, "House_D", new Vector3(41f, 0f, -14.5f), 90f);
        PlaceHouse(houses, "House_E", new Vector3(30.5f, 0f, -10.5f), 180f);
        PlaceHouse(houses, "House_F", new Vector3(23f, 0f, -19f), 180f);
        PlaceHouse(houses, "Shed_1", new Vector3(31.5f, 0f, -3.5f), -90f);
        PlaceHouse(houses, "Shed_2", new Vector3(35f, 0f, 11f), 0f);

        BuildVillagePlaza(root);
        BuildVillageTrail(root);
        BuildVillageProps(root);
        BuildVillageFences(root);
        BuildVillageVegetation(root);
    }

    static void PlaceHouse(Transform parent, string name, Vector3 pos, float yaw)
    {
        string path = VillagePrefabs + name + ".prefab";
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null)
        {
            Debug.LogWarning("HOUSE_MISSING " + path);
            return;
        }

        var inst = PrefabUtility.InstantiatePrefab(asset, parent) as GameObject;
        if (inst == null)
        {
            Debug.LogWarning("HOUSE_INSTANTIATE_FAILED " + name);
            return;
        }

        inst.name = name;
        inst.transform.position = new Vector3(pos.x, Ground(pos.x, pos.z) + 0.01f, pos.z);
        inst.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        var b = ComputeWorldBounds(inst);
        bool overlap = Intersects(b, 0.05f);
        if (overlap)
            Debug.LogWarning("HOUSE_OVERLAP " + name + " pos=" + inst.transform.position.ToString("F2") + " size=" + b.size.ToString("F2"));
        AddOccupied(b, 0.05f);
        Debug.Log("HOUSE_PLACED " + name + " pos=" + inst.transform.position.ToString("F2") + " bounds=" + b.size.ToString("F2"));
    }

    static void BuildVillagePlaza(Transform root)
    {
        var plaza = Child(root, "Plaza");
        for (int ix = 0; ix < 3; ix++)
            for (int iz = 0; iz < 5; iz++)
            {
                float x = 35.25f + ix * 1.5f;
                float z = -6.75f + iz * 1.5f;
                PlaceModel(plaza, FantasyTown, "road", new Vector3(x, 0f, z), 0f, 1.5f, null, ColliderKind.None, false, 0f, true, false);
            }

        PlaceModel(plaza, FantasyTown, "fountain-round", new Vector3(36.75f, 0f, -3.75f), 0f, 2f, null, ColliderKind.Box, false, 0f, true, true);
        PlaceModel(plaza, FantasyTown, "fountain-center", new Vector3(36.75f, Ground(36.75f, -3.75f) + 0.28f, -3.75f), 0f, 2f, null, ColliderKind.None, false, 0f, false, false);

        PlaceModel(plaza, FantasyTown, "pillar-wood", new Vector3(35.25f, 0f, -5.25f), 0f, 1.5f, null, ColliderKind.Box, false, 0f, true, true);
        PlaceModel(plaza, FantasyTown, "pillar-wood", new Vector3(38.25f, 0f, -5.25f), 0f, 1.5f, null, ColliderKind.Box, false, 0f, true, true);
        PlaceModel(plaza, FantasyTown, "pillar-wood", new Vector3(35.25f, 0f, -2.25f), 0f, 1.5f, null, ColliderKind.Box, false, 0f, true, true);
        PlaceModel(plaza, FantasyTown, "pillar-wood", new Vector3(38.25f, 0f, -2.25f), 0f, 1.5f, null, ColliderKind.Box, false, 0f, true, true);
        PlaceModel(plaza, FantasyTown, "roof-gable", new Vector3(36.75f, Ground(36.75f, -3.75f) + 1.092f, -3.75f), 0f, 4f, null, ColliderKind.None, false, 0f, false, false);

        PlaceModel(plaza, FantasyTown, "lantern", new Vector3(34.85f, 0f, -7.05f), 0f, 1.5f, null, ColliderKind.Box, true, 0.05f, true, true);
        PlaceModel(plaza, FantasyTown, "lantern", new Vector3(38.65f, 0f, -7.05f), 0f, 1.5f, null, ColliderKind.Box, true, 0.05f, true, true);
        PlaceModel(plaza, FantasyTown, "lantern", new Vector3(34.85f, 0f, -0.45f), 0f, 1.5f, null, ColliderKind.Box, true, 0.05f, true, true);
        PlaceModel(plaza, FantasyTown, "lantern", new Vector3(38.65f, 0f, -0.45f), 0f, 1.5f, null, ColliderKind.Box, true, 0.05f, true, true);

        PlaceModel(plaza, FantasyTown, "banner-red", new Vector3(34.55f, 0f, -6.95f), 0f, 2f, null, ColliderKind.None, false, 0f, true, false);
        PlaceModel(plaza, FantasyTown, "banner-green", new Vector3(38.95f, 0f, -6.95f), 0f, 2f, null, ColliderKind.None, false, 0f, true, false);

        AddOccupied(new Bounds(new Vector3(36.75f, Ground(36.75f, -3.75f) + 0.2f, -3.75f), new Vector3(4.5f, 0.4f, 7.5f)), 0.1f);
    }

    static void BuildVillageTrail(Transform root)
    {
        var trail = Child(root, "Trail");
        Vector3[] points =
        {
            new Vector3(36.75f, 0f, -9f),
            new Vector3(35f, 0f, -11f),
            new Vector3(35f, 0f, -13f),
            new Vector3(33f, 0f, -15f),
            new Vector3(30f, 0f, -17f),
            new Vector3(27f, 0f, -19f),
            new Vector3(27f, 0f, -21.5f),
            new Vector3(18f, 0f, -21f),
            new Vector3(15f, 0f, -21f),
            new Vector3(12f, 0f, -21.5f),
            new Vector3(9f, 0f, -21.5f),
            new Vector3(6f, 0f, -21.5f)
        };

        foreach (var p in points)
        {
            var go = PlaceModel(trail, FantasyTown, "road", p, 0f, 2f, null, ColliderKind.None, true, 0.05f, true, false);
            if (go != null)
            {
                var b = ComputeWorldBounds(go);
                AddOccupied(b, 0.5f);
            }
        }

        PlaceModel(trail, FantasyTown, "lantern", new Vector3(36.2f, 0f, -10.5f), 0f, 1.5f, null, ColliderKind.Box, true, 0.05f, true, true);
        PlaceModel(trail, FantasyTown, "lantern", new Vector3(34.2f, 0f, -14.2f), 0f, 1.5f, null, ColliderKind.Box, true, 0.05f, true, true);
        PlaceModel(trail, FantasyTown, "lantern", new Vector3(31.2f, 0f, -18.2f), 0f, 1.5f, null, ColliderKind.Box, true, 0.05f, true, true);
        PlaceModel(trail, FantasyTown, "lantern", new Vector3(28.5f, 0f, -22.5f), 0f, 1.5f, null, ColliderKind.Box, true, 0.05f, true, true);
        PlaceModel(trail, FantasyTown, "lantern", new Vector3(16.2f, 0f, -20.2f), 0f, 1.5f, null, ColliderKind.Box, true, 0.05f, true, true);
        PlaceModel(trail, Survival, "signpost", new Vector3(36.75f, 0f, -11.5f), 90f, 4f, null, ColliderKind.Box, true, 0.05f, true, true);
    }

    static void BuildVillageProps(Transform root)
    {
        var props = Child(root, "Props");
        PlaceModel(props, FantasyTown, "cart", new Vector3(33.8f, 0f, 0.6f), 0f, 2f, null, ColliderKind.Box, true, 0.05f, true, true);
        PlaceModel(props, FantasyTown, "cart-high", new Vector3(33.6f, 0f, -6.8f), 0f, 2f, null, ColliderKind.Box, true, 0.05f, true, true);

        PlaceModel(props, Survival, "barrel", new Vector3(34.2f, 0f, 0.8f), 0f, 4f, null, ColliderKind.Box, true, 0.05f, true, true);
        PlaceModel(props, Survival, "barrel", new Vector3(39.0f, 0f, 0.8f), 0f, 4f, null, ColliderKind.Box, true, 0.05f, true, true);
        PlaceModel(props, Survival, "barrel", new Vector3(33.0f, 0f, -7.2f), 0f, 4f, null, ColliderKind.Box, true, 0.05f, true, true);
        PlaceModel(props, Survival, "box", new Vector3(34.0f, 0f, 0.2f), 0f, 4f, null, ColliderKind.Box, true, 0.05f, true, true);
        PlaceModel(props, Survival, "box-large", new Vector3(39.2f, 0f, 0.2f), 0f, 4f, null, ColliderKind.Box, true, 0.05f, true, true);
        PlaceModel(props, Survival, "box-large", new Vector3(33.2f, 0f, -7.8f), 0f, 4f, null, ColliderKind.Box, true, 0.05f, true, true);
        PlaceModel(props, Survival, "chest", new Vector3(34.5f, 0f, -0.2f), 0f, 4f, null, ColliderKind.Box, true, 0.05f, true, true);
        PlaceModel(props, Survival, "bucket", new Vector3(35.0f, 0f, 0.1f), 0f, 4f, null, ColliderKind.Box, true, 0.05f, true, true);

        PlaceModel(props, Furniture, "pottedPlant", new Vector3(34.7f, 0f, -0.1f), 0f, 2f, null, ColliderKind.Box, true, 0.05f, true, true);
        PlaceModel(props, Furniture, "pottedPlant", new Vector3(38.8f, 0f, -0.1f), 0f, 2f, null, ColliderKind.Box, true, 0.05f, true, true);
        PlaceModel(props, Furniture, "bench", new Vector3(36.0f, 0f, 0.2f), 0f, 2f, null, ColliderKind.Box, true, 0.05f, true, true);
        PlaceModel(props, Furniture, "bench", new Vector3(37.5f, 0f, 0.2f), 0f, 2f, null, ColliderKind.Box, true, 0.05f, true, true);
    }

    static void BuildVillageFences(Transform root)
    {
        var fences = Child(root, "Fences");
        for (int i = 0; i < 4; i++)
            PlaceModel(fences, FantasyTown, "fence", new Vector3(24f + i * 2f, 0f, 8f), 90f, 2f, null, ColliderKind.Box, true, 0.05f, true, true);
    }

    static void BuildVillageVegetation(Transform root)
    {
        var veg = Child(root, "Vegetation");
        PlaceModel(veg, Nature, "tree_palmDetailedTall", new Vector3(30.5f, 0f, 7f), 20f, 4.5f, null, ColliderKind.Capsule, true, 0.15f, true, true);
        PlaceModel(veg, Nature, "tree_palmBend", new Vector3(43.2f, 0f, -4.5f), 120f, 4.5f, null, ColliderKind.Capsule, true, 0.15f, true, true);
        PlaceModel(veg, Nature, "tree_palmDetailedTall", new Vector3(46.5f, 0f, -13.5f), 250f, 4.5f, null, ColliderKind.Capsule, true, 0.15f, true, true);
        PlaceModel(veg, Nature, "tree_palmBend", new Vector3(26f, 0f, -14f), 300f, 4.5f, null, ColliderKind.Capsule, true, 0.15f, true, true);
        PlaceModel(veg, Nature, "tree_palmDetailedTall", new Vector3(41f, 0f, 13.5f), 80f, 4.5f, null, ColliderKind.Capsule, true, 0.15f, true, true);
        PlaceModel(veg, Nature, "tree_palmBend", new Vector3(31.5f, 0f, -18f), 190f, 4.5f, null, ColliderKind.Capsule, true, 0.15f, true, true);

        PlaceModel(veg, Nature, "tree_detailed_dark", new Vector3(35f, 0f, -8f), 45f, 4.5f, null, ColliderKind.Capsule, true, 0.2f, true, true);
        PlaceModel(veg, Nature, "tree_detailed_dark", new Vector3(43.5f, 0f, 2f), 200f, 4.5f, null, ColliderKind.Capsule, true, 0.2f, true, true);
        PlaceModel(veg, Nature, "tree_detailed_dark", new Vector3(45.5f, 0f, -17f), 310f, 4.5f, null, ColliderKind.Capsule, true, 0.2f, true, true);
        PlaceModel(veg, Nature, "tree_detailed_dark", new Vector3(31f, 0f, 2f), 150f, 4.5f, null, ColliderKind.Capsule, true, 0.2f, true, true);

        Scatter(veg, Nature, new[] { "plant_bushLarge", "plant_bush", "plant_bushTriangle" }, 22, 27f, 43f, -20f, 12f, 2.5f, 3.5f, null, ColliderKind.None, 0.15f, true);
        Scatter(veg, Nature, new[] { "flower_redA", "flower_redB", "flower_redC", "flower_yellowB", "flower_yellowC", "flower_purpleB", "flower_purpleC" }, 36, 27f, 43f, -20f, 12f, 4f, 6f, null, ColliderKind.None, 0.08f, true);
    }

    static void BuildBurnedForest(Transform root)
    {
        var logs = Child(root, "CharredLogs");
        var rocks = Child(root, "DarkRocks");
        var ash = Child(root, "AshPatches");
        var trees = Child(root, "DeadTrees");

        Scatter(logs, Survival, new[] { "tree-log", "tree-log-small", "tree-trunk" }, 18, -27f, 27f, -32f, 2f, 3.2f, 4.5f, charredBark, ColliderKind.Box, 0.25f, true);
        Scatter(logs, OldNature, new[] { "stump_roundDetailed", "stump_old" }, 12, -27f, 27f, -32f, 2f, 2.5f, 3.8f, charredBark, ColliderKind.Box, 0.2f, true);
        Scatter(rocks, Nature, new[] { "rock_largeC", "rock_largeD", "rock_largeE", "rock_smallC", "rock_smallD" }, 14, -27f, 27f, -32f, 2f, 2.5f, 5f, darkStone, ColliderKind.Box, 0.25f, true);
        Scatter(ash, Survival, new[] { "patch-grass", "patch-grass-large" }, 34, -27f, 27f, -32f, 2f, 4f, 8f, ashMaterial, ColliderKind.None, 0f, false);
        Scatter(trees, Nature, new[] { "tree_detailed_dark", "tree_small_dark", "tree_cone_dark", "tree_default_dark" }, 10, -27f, 27f, -32f, 2f, 3.5f, 5f, charredBark, ColliderKind.Capsule, 0.3f, true);
    }

    static void BuildRestoration(Transform root)
    {
        var camp = Child(root, "Camp");
        var nursery = Child(root, "Nursery");
        var veg = Child(root, "Vegetation");

        PlaceModel(camp, Survival, "tent", new Vector3(12f, 0f, 14f), 0f, 4f, null, ColliderKind.Box, true, 0.2f, true, true);
        PlaceModel(camp, Survival, "tent-canvas", new Vector3(16f, 0f, 14f), 180f, 4f, null, ColliderKind.Box, true, 0.2f, true, true);
        PlaceModel(camp, Survival, "workbench", new Vector3(14f, 0f, 18f), 0f, 4f, null, ColliderKind.Box, true, 0.1f, true, true);
        PlaceModel(camp, Survival, "campfire-pit", new Vector3(14f, 0f, 16f), 0f, 4f, null, ColliderKind.Box, true, 0.1f, true, true);
        PlaceModel(camp, Survival, "campfire-stand", new Vector3(14f, 0f, 16f), 0f, 4f, null, ColliderKind.Box, true, 0.1f, true, true);
        PlaceModel(camp, Survival, "barrel", new Vector3(16f, 0f, 18f), 0f, 4f, null, ColliderKind.Box, true, 0.1f, true, true);
        PlaceModel(camp, Survival, "box", new Vector3(12f, 0f, 18f), 0f, 4f, null, ColliderKind.Box, true, 0.1f, true, true);
        PlaceModel(camp, Survival, "box-large", new Vector3(12.8f, 0f, 18.2f), 0f, 4f, null, ColliderKind.Box, true, 0.1f, true, true);
        PlaceModel(camp, Survival, "chest", new Vector3(15f, 0f, 19f), 0f, 4f, null, ColliderKind.Box, true, 0.1f, true, true);
        PlaceModel(camp, Survival, "bedroll", new Vector3(11.2f, 0f, 13.2f), 90f, 4f, null, ColliderKind.Box, true, 0.1f, true, true);
        PlaceModel(camp, Survival, "bedroll", new Vector3(16.8f, 0f, 13.2f), 270f, 4f, null, ColliderKind.Box, true, 0.1f, true, true);
        PlaceModel(camp, Survival, "signpost", new Vector3(10f, 0f, 14f), 90f, 4f, null, ColliderKind.Box, true, 0.1f, true, true);

        for (int i = 0; i < 6; i++)
            PlaceModel(nursery, FantasyTown, "fence", new Vector3(-4f + i * 2f, 0f, 12.5f), 90f, 2f, null, ColliderKind.Box, true, 0.1f, true, true);

        Scatter(veg, OldNature, new[] { "tree_small" }, 26, -22f, 16f, 2f, 38f, 2.2f, 3f, null, ColliderKind.Capsule, 0.2f, true);
        Scatter(veg, Nature, new[] { "tree_small_dark" }, 12, -22f, 16f, 2f, 38f, 2.2f, 3f, null, ColliderKind.Capsule, 0.2f, true);
        Scatter(veg, Nature, new[] { "plant_bushLarge", "plant_bush", "plant_bushTriangle", "plant_bushLargeTriangle" }, 24, -22f, 16f, 2f, 38f, 2.5f, 3.5f, null, ColliderKind.None, 0.12f, true);
        Scatter(veg, OldNature, new[] { "plant_bushDetailed", "plant_bushSmall" }, 16, -22f, 16f, 2f, 38f, 2.5f, 3.5f, null, ColliderKind.None, 0.12f, true);
        Scatter(veg, Nature, new[] { "flower_yellowB", "flower_yellowC", "flower_redA", "flower_redB", "flower_redC", "flower_purpleB", "flower_purpleC" }, 44, -22f, 16f, 2f, 38f, 4f, 6f, null, ColliderKind.None, 0.06f, true);
        Scatter(veg, OldNature, new[] { "flower_yellowA", "flower_purpleA" }, 16, -22f, 16f, 2f, 38f, 4f, 6f, null, ColliderKind.None, 0.06f, true);
        Scatter(veg, OldNature, new[] { "grass", "grass_large", "grass_leafs" }, 26, -22f, 16f, 2f, 38f, 3f, 5f, null, ColliderKind.None, 0.05f, false);
    }

    static void BuildRecovered(Transform root)
    {
        var trees = Child(root, "Trees");
        var under = Child(root, "Understory");
        var rocks = Child(root, "RocksAndLogs");

        Scatter(trees, Nature, new[] { "tree_palmDetailedTall", "tree_palmBend", "tree_palmTall", "tree_palmShort" }, 22, -32f, 32f, 38f, 80f, 4.2f, 5.5f, null, ColliderKind.Capsule, 0.25f, true);
        Scatter(trees, Nature, new[] { "tree_detailed_dark", "tree_blocks", "tree_fat", "tree_cone", "tree_pineTallD", "tree_pineRoundF", "tree_pineRoundE" }, 26, -32f, 32f, 38f, 80f, 4f, 5.5f, null, ColliderKind.Capsule, 0.3f, true);
        Scatter(under, Nature, new[] { "plant_bushLarge", "plant_bush", "plant_bushTriangle", "plant_bushLargeTriangle", "plant_flatTall", "plant_flatShort" }, 44, -32f, 32f, 38f, 80f, 2.8f, 4.2f, null, ColliderKind.None, 0.12f, true);
        Scatter(under, OldNature, new[] { "plant_bushDetailed", "plant_bushSmall", "grass", "grass_large", "grass_leafs" }, 46, -32f, 32f, 38f, 80f, 3f, 5f, null, ColliderKind.None, 0.06f, false);
        Scatter(under, Nature, new[] { "flower_yellowB", "flower_yellowC", "flower_redA", "flower_redB", "flower_redC", "flower_purpleB", "flower_purpleC" }, 48, -32f, 32f, 38f, 80f, 4f, 6f, null, ColliderKind.None, 0.05f, true);
        Scatter(under, Nature, new[] { "mushroom_tanGroup", "mushroom_tanTall", "mushroom_redTall", "mushroom_redGroup" }, 14, -32f, 32f, 38f, 80f, 3f, 5f, null, ColliderKind.None, 0.05f, false);
        Scatter(rocks, Nature, new[] { "rock_largeC", "rock_largeD", "rock_largeE", "stone_largeB", "stone_largeC", "stone_tallB", "stone_tallC" }, 18, -32f, 32f, 38f, 80f, 2.5f, 4.5f, null, ColliderKind.Box, 0.3f, true);
        Scatter(rocks, Survival, new[] { "tree-log", "tree-log-small", "tree-trunk" }, 14, -32f, 32f, 38f, 80f, 3f, 4.5f, null, ColliderKind.Box, 0.25f, true);
        Scatter(rocks, OldNature, new[] { "log", "log_stack", "stump_roundDetailed", "stump_old" }, 12, -32f, 32f, 38f, 80f, 2.5f, 4f, null, ColliderKind.Box, 0.2f, true);
    }

    static int Scatter(Transform parent, string folder, string[] names, int count, float xMin, float xMax, float zMin, float zMax, float scaleMin, float scaleMax, Material mat, ColliderKind collider, float margin, bool occupy)
    {
        int placed = 0;
        int attempts = 0;
        int maxAttempts = Mathf.Max(80, count * 100);
        while (placed < count && attempts < maxAttempts)
        {
            attempts++;
            float x = Range(xMin, xMax);
            float z = Range(zMin, zMax);
            string name = names[rng.Next(names.Length)];
            float scale = Range(scaleMin, scaleMax);
            float yaw = Range(0f, 360f);
            var go = PlaceModel(parent, folder, name, new Vector3(x, 0f, z), yaw, scale, mat, collider, true, margin, true, occupy);
            if (go != null) placed++;
        }
        Debug.Log("SCATTER " + parent.name + " " + string.Join(",", names) + " placed=" + placed + "/" + count + " attempts=" + attempts);
        return placed;
    }

    static GameObject PlaceModel(Transform parent, string folder, string modelName, Vector3 pos, float yaw, float scale, Material mat, ColliderKind collider, bool skipIfBlocked, float margin, bool snapToGround, bool occupy)
    {
        string path = folder + modelName + ".fbx";
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null)
        {
            Debug.LogWarning("MODEL_MISSING " + path);
            return null;
        }

        var inst = Object.Instantiate(asset);
        if (PrefabUtility.IsPartOfPrefabInstance(inst))
            PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        inst.name = modelName;
        inst.transform.SetParent(parent, false);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;
        inst.transform.localScale = Vector3.one;

        var local = ComputeLocalBounds(inst);
        Vector3 worldPos = snapToGround ? new Vector3(pos.x, Ground(pos.x, pos.z), pos.z) : pos;
        Quaternion rot = Quaternion.Euler(0f, yaw, 0f);
        Vector3 offset = rot * (local.center * scale);
        var candidate = new Bounds(worldPos + offset, local.size * scale);
        if (skipIfBlocked && Intersects(candidate, margin))
        {
            Object.DestroyImmediate(inst);
            return null;
        }

        inst.transform.position = worldPos;
        inst.transform.rotation = rot;
        inst.transform.localScale = Vector3.one * scale;

        foreach (var r in inst.GetComponentsInChildren<Renderer>(true))
        {
            if (mat != null)
            {
                var mats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                r.sharedMaterials = mats;
            }
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            r.receiveShadows = true;
        }

        var lb = ComputeLocalBounds(inst);
        if (collider == ColliderKind.Box)
        {
            var bc = inst.AddComponent<BoxCollider>();
            bc.center = lb.center;
            bc.size = lb.size;
        }
        else if (collider == ColliderKind.Capsule)
        {
            var cc = inst.AddComponent<CapsuleCollider>();
            cc.center = lb.center;
            cc.height = Mathf.Max(lb.size.y, 0.1f);
            cc.radius = Mathf.Max(lb.size.x, lb.size.z) * 0.3f;
            cc.direction = 1;
        }

        if (occupy) AddOccupied(ComputeWorldBounds(inst), margin);
        return inst;
    }

    static Transform Child(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    static float Ground(float x, float z)
    {
        return terrain.SampleHeight(new Vector3(x, 0f, z)) + terrain.transform.position.y;
    }

    static float Range(float min, float max)
    {
        return min + (float)rng.NextDouble() * (max - min);
    }

    static bool Intersects(Bounds b, float margin)
    {
        var bb = b;
        bb.Expand(margin * 2f);
        foreach (var o in occupied)
            if (o.Intersects(bb)) return true;
        return false;
    }

    static void AddOccupied(Bounds b, float margin)
    {
        var bb = b;
        bb.Expand(margin * 2f);
        occupied.Add(bb);
    }

    static Bounds ComputeWorldBounds(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>(true);
        if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        var b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        return b;
    }

    static Bounds ComputeLocalBounds(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>(true);
        if (rs.Length == 0) return new Bounds(Vector3.zero, Vector3.zero);
        bool has = false;
        var result = new Bounds(Vector3.zero, Vector3.zero);
        foreach (var r in rs)
        {
            var wb = r.bounds;
            if (wb.size.sqrMagnitude < 0.000001f) continue;
            Vector3 min = wb.min;
            Vector3 max = wb.max;
            Vector3[] corners =
            {
                new Vector3(min.x, min.y, min.z),
                new Vector3(min.x, min.y, max.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(min.x, max.y, max.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, min.y, max.z),
                new Vector3(max.x, max.y, min.z),
                new Vector3(max.x, max.y, max.z)
            };
            foreach (var c in corners)
            {
                var p = go.transform.InverseTransformPoint(c);
                if (!has) { result = new Bounds(p, Vector3.zero); has = true; }
                else result.Encapsulate(p);
            }
        }
        return result;
    }

    static int CountRenderers(GameObject go)
    {
        return go.GetComponentsInChildren<Renderer>(true).Length;
    }

    static Material EnsureMaterial(string path, Color color, float smoothness)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m != null) return m;
        EnsureFolder(System.IO.Path.GetDirectoryName(path).Replace('\\', '/'));
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        m = new Material(shader);
        m.SetColor("_BaseColor", color);
        m.SetFloat("_Smoothness", smoothness);
        m.SetFloat("_Metallic", 0f);
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        string name = System.IO.Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
