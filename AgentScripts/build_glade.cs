using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Rimbara.Interaction;

var sb = new StringBuilder();
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.name != "VillageRestoration") throw new System.Exception("Open VillageRestoration first, got " + scene.name);

float cx = 20f, cz = -60f;
const float clearRadius = 21f;

// 1. Clear scenery inside the glade footprint.
string[] sceneryRoots = { "Bushes", "Props", "rest", "Flats", "Grasses", "Plants", "Rocks", "treeses", "Clifs_MDG" };
int cleared = 0;
foreach (var rn in sceneryRoots) {
    var root = scene.GetRootGameObjects().FirstOrDefault(x => x.name == rn);
    if (root == null) continue;
    var doomed = new List<GameObject>();
    foreach (var t in root.GetComponentsInChildren<Transform>(true)) {
        if (t == root.transform) continue;
        if (t.gameObject.GetComponent<Renderer>() == null && t.gameObject.GetComponent<Collider>() == null) continue;
        if (Vector2.Distance(new Vector2(t.position.x, t.position.z), new Vector2(cx, cz)) < clearRadius)
            doomed.Add(t.gameObject);
    }
    foreach (var go in doomed) { if (go != null) { Object.DestroyImmediate(go); cleared++; } }
}
sb.AppendLine("cleared props=" + cleared);

// 2. Sample the ground.
float terrainY = 0f;
var terrainHits = Physics.RaycastAll(new Vector3(cx, 200f, cz), Vector3.down, 600f)
    .Where(h => h.collider.transform.root.name == "Terrain" || h.collider.transform.root.name == "Ground")
    .OrderBy(h => h.point.y).ToArray();
if (terrainHits.Length > 0) terrainY = terrainHits[0].point.y;
float top = terrainY + 0.75f;
sb.AppendLine($"terrainY={terrainY:F2} gladeTop={top:F2}");

// 3. Build the land platform.
var existing = GameObject.Find("Restoration Ground");
if (existing != null) Object.DestroyImmediate(existing);
var rootGo = new GameObject("Restoration Ground");
rootGo.transform.position = new Vector3(cx, top, cz);
var root = rootGo.transform;

var grassMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/EmaceArt/Slavic World Free/Materials/Grass.mat");
var mudMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/EmaceArt/Slavic World Free/Materials/Mud.mat");

GameObject MakeDisc(string name_, float radius, float thickness, Material mat, bool collider) {
    var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    go.name = name_;
    go.transform.SetParent(root, false);
    go.transform.localPosition = new Vector3(0f, -thickness * 0.5f, 0f);
    go.transform.localScale = new Vector3(radius * 2f, thickness * 0.5f, radius * 2f);
    if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
    else {
        Object.DestroyImmediate(go.GetComponent<Collider>());
        var mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
    }
    if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
    return go;
}

MakeDisc("Glade ground", 22f, 1f, grassMat, true);
MakeDisc("Glade soil", 20.4f, 0.06f, mudMat, false);

// 4. Border rocks and outer forest.
var rockGuids = new[] {
    "Assets/EmaceArt/Slavic World Free/Prefabs/Environment/Rock/EA03_EnvRock_01a_PRE.prefab",
    "Assets/EmaceArt/Slavic World Free/Prefabs/Environment/Rock/EA03_Environment_Rock_Flat_04c_PRE.prefab",
    "Assets/EmaceArt/Slavic World Free/Prefabs/Environment/Rock/EA03_Environment_Rock_Big_Head_01a_PRE.prefab"
};
var rocks = rockGuids.Select(p => AssetDatabase.LoadAssetAtPath<GameObject>(p)).Where(x => x != null).ToArray();
for (int i = 0; i < 26 && rocks.Length > 0; i++) {
    float ang = i * (360f / 26f) + Random.Range(-4f, 4f);
    float rad = 21.2f + Random.Range(-0.4f, 1.2f);
    var prefab = rocks[i % rocks.Length];
    var rock = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
    rock.transform.SetParent(root, true);
    rock.transform.position = new Vector3(cx + Mathf.Cos(ang * Mathf.Deg2Rad) * rad, top - 0.2f, cz + Mathf.Sin(ang * Mathf.Deg2Rad) * rad);
    rock.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    rock.transform.localScale = Vector3.one * Random.Range(0.75f, 1.25f);
}

var treeGuids = new[] {
    "Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Tree/EA03_Nature_Tree_01b_PRE.prefab",
    "Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Tree/EA03_Nature_Tree_02b_PRE.prefab",
    "Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Tree/EA03_Nature_Tree_03c_PRE.prefab",
    "Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Tree/EA03_Nature_Tree_06b_PRE.prefab"
};
var trees = treeGuids.Select(p => AssetDatabase.LoadAssetAtPath<GameObject>(p)).Where(x => x != null).ToArray();
for (int i = 0; i < 18 && trees.Length > 0; i++) {
    float ang = i * (360f / 18f) + Random.Range(-5f, 5f);
    float rad = 25f + Random.Range(0f, 6f);
    var tree = (GameObject)PrefabUtility.InstantiatePrefab(trees[i % trees.Length]);
    tree.transform.SetParent(root, true);
    float tx = cx + Mathf.Cos(ang * Mathf.Deg2Rad) * rad, tz = cz + Mathf.Sin(ang * Mathf.Deg2Rad) * rad;
    float gy = Physics.Raycast(new Vector3(tx, 200f, tz), Vector3.down, out var gh, 600f) ? gh.point.y : terrainY;
    tree.transform.position = new Vector3(tx, gy, tz);
    tree.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    tree.transform.localScale = Vector3.one * Random.Range(0.9f, 1.3f);
}

// 5. Lay out gameplay inside the glade.
Vector3 At(float lx, float lz, float lift = 0.02f) => new Vector3(cx + lx, top + lift, cz + lz);

void Place(string name_, Vector3 p) {
    var go = GameObject.Find(name_);
    if (go == null) { sb.AppendLine("MISSING " + name_); return; }
    go.transform.position = p;
    if (go.transform.parent != root) go.transform.SetParent(root, true);
}

var zoneA = GameObject.Find("Zone A - volunteer post and damaged forest");
var zoneB = GameObject.Find("Zone B - seedling nursery and planting clearings");
var zoneC = GameObject.Find("Zone C - remaining fires");
foreach (var z in new[] { zoneA, zoneB, zoneC }) if (z != null) z.transform.SetParent(root, true);

Place("Volunteer post return zone", At(0f, -13f));
Place("Volunteer mission board", At(2.6f, -14.3f));
Place("Inspection marker", At(0f, -4.5f));
Place("Planting zone destination", At(4f, 4f));
Place("Fire zone destination", At(0f, 11f));
Place("tent_detailedOpen_2_9", At(-13.5f, -8f));
Place("log_stack_0_8", At(13.5f, -7f));

var seedSpots = new[] { (-7f, -2f), (-8.5f, 1.5f), (-8f, 5f), (-5f, 8f), (-1.5f, 9.5f) };
var bedSpots  = new[] { (5f, -2f), (7.5f, 1f), (7.5f, 4.5f), (5.5f, 7.5f), (2f, 9.5f) };
var fireSpots = new[] { (-4f, 11.5f), (3f, 12f) };

var seeds = Object.FindObjectsByType<SeedlingPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(x => x.Slot).ToArray();
var beds = Object.FindObjectsByType<PlantingPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(x => x.Slot).ToArray();
var fires = Object.FindObjectsByType<FirePoint>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(x => x.Slot).ToArray();

for (int i = 0; i < seeds.Length && i < seedSpots.Length; i++) {
    seeds[i].transform.position = At(seedSpots[i].Item1, seedSpots[i].Item2, 0.03f);
    if (seeds[i].transform.parent != zoneB?.transform) seeds[i].transform.SetParent(zoneB != null ? zoneB.transform : root, true);
}
for (int i = 0; i < beds.Length && i < bedSpots.Length; i++) {
    beds[i].transform.position = At(bedSpots[i].Item1, bedSpots[i].Item2, 0.01f);
    if (beds[i].transform.parent != zoneB?.transform) beds[i].transform.SetParent(zoneB != null ? zoneB.transform : root, true);
}
for (int i = 0; i < fires.Length && i < fireSpots.Length; i++) {
    fires[i].transform.position = At(fireSpots[i].Item1, fireSpots[i].Item2, 0.01f);
    if (fires[i].transform.parent != zoneC?.transform) fires[i].transform.SetParent(zoneC != null ? zoneC.transform : root, true);
}

// 6. New planted-tree assets.
var newTrees = new[] { "RestoredTree_01", "RestoredTree_02", "RestoredTree_03" }
    .Select(n => AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/" + n + ".prefab"))
    .Where(x => x != null).ToArray();
if (newTrees.Length > 0) {
    for (int i = 0; i < beds.Length; i++) {
        var so = new SerializedObject(beds[i]);
        var prop = so.FindProperty("treePrefab");
        if (prop == null) { sb.AppendLine("NO treePrefab field on bed " + i); continue; }
        prop.objectReferenceValue = newTrees[i % newTrees.Length];
        so.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.RecordPrefabInstancePropertyModifications(beds[i]);
    }
    sb.AppendLine("beds retreed=" + beds.Length);
}

// 7. Player start.
var actor = Object.FindObjectsByType<VolunteerActor>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
if (actor != null) {
    var cc = actor.GetComponent<CharacterController>();
    if (cc != null) cc.enabled = false;
    actor.transform.position = At(0f, -14.6f, 0.15f);
    actor.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    if (cc != null) cc.enabled = true;
    sb.AppendLine("volunteer=" + actor.transform.position);
}

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
sb.AppendLine("SAVED " + scene.path);
return sb.ToString();
