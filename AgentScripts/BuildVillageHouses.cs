using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class BuildVillageHouses
{
    const string ModelRoot = "Assets/RIMBARA/Environment/Village/Models/FantasyTown";
    const string PrefabRoot = "Assets/RIMBARA/Environment/Village/Prefabs";
    const string ColormapPath = "Assets/RIMBARA/Materials/Village/M_FantasyTown_Colormap.mat";

    enum Side { South, North, East, West }

    class HouseSpec
    {
        public string Name;
        public float Width;
        public float Depth;
        public string Style;
        public bool RidgeAlongZ;
        public bool Chimney;
        public bool Lantern;
        public bool Windows;
    }

    public static void BuildAll()
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(ColormapPath);
        if (mat == null) throw new System.Exception("Missing material " + ColormapPath);
        EnsureFolder(PrefabRoot);

        var specs = new List<HouseSpec>
        {
            new HouseSpec { Name = "House_A", Width = 6f, Depth = 6f, Style = "plaster", RidgeAlongZ = false, Chimney = true,  Lantern = true,  Windows = true  },
            new HouseSpec { Name = "House_B", Width = 4f, Depth = 6f, Style = "wood",    RidgeAlongZ = true,  Chimney = false, Lantern = true,  Windows = true  },
            new HouseSpec { Name = "House_C", Width = 4f, Depth = 4f, Style = "plaster", RidgeAlongZ = false, Chimney = true,  Lantern = true,  Windows = true  },
            new HouseSpec { Name = "House_D", Width = 4f, Depth = 6f, Style = "wood",    RidgeAlongZ = true,  Chimney = false, Lantern = true,  Windows = true  },
            new HouseSpec { Name = "House_E", Width = 6f, Depth = 6f, Style = "mixed",   RidgeAlongZ = false, Chimney = true,  Lantern = true,  Windows = true  },
            new HouseSpec { Name = "House_F", Width = 4f, Depth = 4f, Style = "plaster", RidgeAlongZ = false, Chimney = false, Lantern = true,  Windows = true  },
            new HouseSpec { Name = "Shed_1",  Width = 4f, Depth = 4f, Style = "wood",    RidgeAlongZ = false, Chimney = false, Lantern = false, Windows = false },
            new HouseSpec { Name = "Shed_2",  Width = 4f, Depth = 4f, Style = "wood",    RidgeAlongZ = false, Chimney = false, Lantern = false, Windows = false },
        };

        foreach (var spec in specs) Build(spec, mat);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("HOUSE_BUILD_DONE count=" + specs.Count);
    }

    static void Build(HouseSpec spec, Material mat)
    {
        var root = new GameObject(spec.Name);
        try
        {
            var floorGroup = new GameObject("Floor");
            floorGroup.transform.SetParent(root.transform, false);
            int nx = Mathf.RoundToInt(spec.Width / 2f);
            int nz = Mathf.RoundToInt(spec.Depth / 2f);
            for (int ix = 0; ix < nx; ix++)
            {
                for (int iz = 0; iz < nz; iz++)
                {
                    float x = -spec.Width / 2f + 1f + ix * 2f;
                    float z = -spec.Depth / 2f + 1f + iz * 2f;
                    AddModel(floorGroup.transform, "planks", new Vector3(x, 0f, z), Quaternion.identity, Vector3.one * 2f, mat, "Floor_" + ix + "_" + iz);
                }
            }

            var wallsGroup = new GameObject("Walls");
            wallsGroup.transform.SetParent(root.transform, false);
            BuildWallSide(wallsGroup.transform, spec, mat, Side.South);
            BuildWallSide(wallsGroup.transform, spec, mat, Side.North);
            BuildWallSide(wallsGroup.transform, spec, mat, Side.East);
            BuildWallSide(wallsGroup.transform, spec, mat, Side.West);

            var roofGroup = new GameObject("Roof");
            roofGroup.transform.SetParent(root.transform, false);
            roofGroup.transform.localRotation = Quaternion.Euler(0f, spec.RidgeAlongZ ? 90f : 0f, 0f);
            BuildRoof(roofGroup.transform, spec, mat);

            if (spec.Chimney)
                AddModel(root.transform, "chimney", new Vector3(1.5f, 2.8f, 0f), Quaternion.identity, Vector3.one * 2f, mat, "Chimney");

            if (spec.Lantern)
                AddModel(root.transform, "lantern", new Vector3(spec.Width / 2f - 0.8f, 0f, -spec.Depth / 2f - 0.25f), Quaternion.identity, Vector3.one * 1.4f, mat, "Lantern");

            float doorX = spec.Width >= 6f ? 0f : 1f;
            AddModel(root.transform, "planks-half", new Vector3(doorX, 0f, -spec.Depth / 2f - 0.55f), Quaternion.identity, new Vector3(2f, 1f, 1f), mat, "DoorStep");

            AddColliders(root, spec);

            string path = PrefabRoot + "/" + spec.Name + ".prefab";
            AssetDatabase.DeleteAsset(path);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path, out bool success);
            if (!success || prefab == null) throw new System.Exception("Failed to save " + path);

            var bounds = ComputeBounds(root);
            int renderers = root.GetComponentsInChildren<MeshRenderer>(true).Length;
            int nullMats = 0;
            foreach (var r in root.GetComponentsInChildren<MeshRenderer>(true))
                if (r.sharedMaterial == null) nullMats++;

            Debug.Log($"HOUSE {spec.Name} path={path} size={spec.Width}x{spec.Depth} boundsSize={bounds.size} boundsCenter={bounds.center} renderers={renderers} nullMats={nullMats}");
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    static void BuildWallSide(Transform parent, HouseSpec spec, Material mat, Side side)
    {
        bool horizontal = side == Side.South || side == Side.North;
        float length = horizontal ? spec.Width : spec.Depth;
        int count = Mathf.RoundToInt(length / 2f);
        float yaw = side == Side.South ? 90f : side == Side.North ? -90f : side == Side.West ? 180f : 0f;

        Vector3 basePos;
        if (side == Side.East) basePos = new Vector3(spec.Width / 2f - 1f, 0f, 0f);
        else if (side == Side.West) basePos = new Vector3(-spec.Width / 2f + 1f, 0f, 0f);
        else if (side == Side.North) basePos = new Vector3(0f, 0f, spec.Depth / 2f - 1f);
        else basePos = new Vector3(0f, 0f, -spec.Depth / 2f + 1f);

        for (int i = 0; i < count; i++)
        {
            float offset = -length / 2f + 1f + i * 2f;
            Vector3 pos = basePos;
            if (horizontal) pos.x = offset; else pos.z = offset;

            string variant = GetWallVariant(spec, side, i, count);
            AddModel(parent, variant, pos, Quaternion.Euler(0f, yaw, 0f), Vector3.one * 2f, mat, side + "_" + i + "_" + variant);
        }
    }

    static string GetWallVariant(HouseSpec spec, Side side, int index, int count)
    {
        bool front = side == Side.South;
        bool wood = spec.Style == "wood" || (spec.Style == "mixed" && front);
        string prefix = wood ? "wall-wood" : "wall";

        if (front)
        {
            int doorIndex = count >= 3 ? count / 2 : count - 1;
            return index == doorIndex ? prefix + "-door" : prefix;
        }

        if (spec.Windows)
        {
            int windowIndex = count >= 3 ? count / 2 : count - 1;
            if (index == windowIndex) return prefix + "-window-small";
        }

        return prefix;
    }

    static void BuildRoof(Transform parent, HouseSpec spec, Material mat)
    {
        float ridgeLength = (spec.RidgeAlongZ ? spec.Depth : spec.Width) + 0.8f;
        float slopeSpan = (spec.RidgeAlongZ ? spec.Width : spec.Depth) + 0.8f;
        int modules = Mathf.RoundToInt((spec.RidgeAlongZ ? spec.Depth : spec.Width) / 2f);

        float scaleX = ridgeLength / (modules * 1.1f);
        float scaleZ = slopeSpan / 1.071f;
        float scaleY = 1.8f / 0.5707107f;
        float pivotY = 2f - 0.1022748f * scaleY;
        float moduleWidth = 1.1f * scaleX;

        for (int i = 0; i < modules; i++)
        {
            float x = -ridgeLength / 2f + moduleWidth / 2f + i * moduleWidth;
            AddModel(parent, "roof-gable", new Vector3(x, pivotY, 0f), Quaternion.identity, new Vector3(scaleX, scaleY, scaleZ), mat, "Roof_" + i);
        }
    }

    static GameObject AddModel(Transform parent, string modelName, Vector3 localPos, Quaternion localRot, Vector3 localScale, Material mat, string name)
    {
        string path = ModelRoot + "/" + modelName + ".fbx";
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null) throw new System.Exception("Missing model " + path);

        var instance = Object.Instantiate(asset);
        if (PrefabUtility.IsPartOfPrefabInstance(instance))
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        instance.name = name;
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPos;
        instance.transform.localRotation = localRot;
        instance.transform.localScale = localScale;

        foreach (var r in instance.GetComponentsInChildren<MeshRenderer>(true))
        {
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            r.receiveShadows = true;
        }

        return instance;
    }

    static void AddColliders(GameObject root, HouseSpec spec)
    {
        float hw = spec.Width / 2f;
        float hd = spec.Depth / 2f;

        AddBox(root, new Vector3(hw - 0.1f, 1f, 0f), new Vector3(0.2f, 2f, spec.Depth));
        AddBox(root, new Vector3(-hw + 0.1f, 1f, 0f), new Vector3(0.2f, 2f, spec.Depth));
        AddBox(root, new Vector3(0f, 1f, hd - 0.1f), new Vector3(spec.Width, 2f, 0.2f));

        float doorX = spec.Width >= 6f ? 0f : 1f;
        float gap = 0.6f;
        float leftStart = -hw;
        float leftEnd = doorX - gap;
        float rightStart = doorX + gap;
        float rightEnd = hw;

        if (leftEnd > leftStart)
            AddBox(root, new Vector3((leftStart + leftEnd) / 2f, 1f, -hd + 0.1f), new Vector3(leftEnd - leftStart, 2f, 0.2f));
        if (rightEnd > rightStart)
            AddBox(root, new Vector3((rightStart + rightEnd) / 2f, 1f, -hd + 0.1f), new Vector3(rightEnd - rightStart, 2f, 0.2f));

        AddBox(root, new Vector3(0f, 0.03f, 0f), new Vector3(spec.Width, 0.06f, spec.Depth));
    }

    static void AddBox(GameObject go, Vector3 center, Vector3 size)
    {
        var c = go.AddComponent<BoxCollider>();
        c.center = center;
        c.size = size;
    }

    static Bounds ComputeBounds(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<MeshRenderer>(true);
        if (renderers.Length == 0) return new Bounds(Vector3.zero, Vector3.zero);
        var b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        return b;
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
