using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rimbara.Interaction;
using Rimbara.Player;

var sb = new StringBuilder();
var scene = SceneManager.GetActiveScene();
if (scene.name != "Arunika") throw new System.Exception("Open Arunika first, got " + scene.name);

var terrain = Object.FindFirstObjectByType<Terrain>();
if (terrain == null) throw new System.Exception("no terrain in scene");
var tPos = terrain.transform.position;
float GroundY(float x, float z) => terrain.SampleHeight(new Vector3(x, 0f, z)) + tPos.y;

// Layout of the restoration trail.
var post = new Vector3(0f, 0f, -30f);
var board = new Vector3(3.4f, 0f, -31.6f);
var tent = new Vector3(-9f, 0f, -33.5f);
var logs = new Vector3(9f, 0f, -28.5f);
var inspection = new Vector3(0f, 0f, -8f);
var plantDest = new Vector3(12f, 0f, 3f);
var fireDest = new Vector3(-2f, 0f, 16f);
var seedSpots = new[] { new Vector3(-9f, 0f, -14f), new Vector3(-11f, 0f, -10f), new Vector3(-12f, 0f, -6f), new Vector3(-11f, 0f, -1f), new Vector3(-8f, 0f, 3f) };
var bedSpots = new[] { new Vector3(9f, 0f, -14f), new Vector3(12f, 0f, -10f), new Vector3(13f, 0f, -6f), new Vector3(12f, 0f, -1f), new Vector3(9f, 0f, 3f) };
var fireSpots = new[] { new Vector3(-8f, 0f, 14f), new Vector3(4f, 0f, 16f) };
var spawn = new Vector3(0f, 0f, -33f);

Vector2[] path =
{
    new Vector2(0f, -40f), new Vector2(0f, -20f), new Vector2(0f, -8f),
    new Vector2(5f, -3f), new Vector2(12f, 3f)
};
Vector2[] sidePath = { new Vector2(0f, -8f), new Vector2(-8f, -12f), new Vector2(-12f, -8f) };
Vector2[] firePath = { new Vector2(0f, -8f), new Vector2(-4f, 4f), new Vector2(-5f, 12f) };

float DistToPath(float x, float z, Vector2[] pts)
{
    float best = float.MaxValue;
    for (int i = 0; i < pts.Length - 1; i++)
    {
        Vector2 a = pts[i], b = pts[i + 1];
        Vector2 ab = b - a, ap = new Vector2(x, z) - a;
        float t = Mathf.Clamp01(Vector2.Dot(ap, ab) / Mathf.Max(0.0001f, ab.sqrMagnitude));
        best = Mathf.Min(best, Vector2.Distance(new Vector2(x, z), a + ab * t));
    }
    return best;
}
float PathDist(float x, float z) => Mathf.Min(Mathf.Min(DistToPath(x, z, path), DistToPath(x, z, sidePath)), DistToPath(x, z, firePath));

float PoiDist(float x, float z)
{
    var pts = new List<Vector3> { post, board, inspection, plantDest, fireDest };
    pts.AddRange(seedSpots); pts.AddRange(bedSpots); pts.AddRange(fireSpots);
    float best = float.MaxValue;
    foreach (var p in pts) best = Mathf.Min(best, Vector2.Distance(new Vector2(x, z), new Vector2(p.x, p.z)));
    return best;
}

// Scenery root.
var scenery = GameObject.Find("Arunika Scenery");
if (scenery != null) Object.DestroyImmediate(scenery);
var sceneryGo = new GameObject("Arunika Scenery");
var sceneryRoot = sceneryGo.transform;

GameObject Load(string p) => AssetDatabase.LoadAssetAtPath<GameObject>(p);
var treePrefabs = new[]
{
    Load("Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Tree/EA03_Nature_Tree_01b_PRE.prefab"),
    Load("Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Tree/EA03_Nature_Tree_02b_PRE.prefab"),
    Load("Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Tree/EA03_Nature_Tree_02c_PRE.prefab"),
    Load("Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Tree/EA03_Nature_Tree_03b_PRE.prefab"),
    Load("Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Tree/EA03_Nature_Tree_03c_PRE.prefab"),
    Load("Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Tree/EA03_Nature_Tree_05a_PRE.prefab"),
    Load("Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Tree/EA03_Nature_Tree_06b_PRE.prefab"),
}.Where(x => x != null).ToArray();

var rockPrefabs = new[]
{
    Load("Assets/EmaceArt/Slavic World Free/Prefabs/Environment/Rock/EA03_EnvRock_01a_PRE.prefab"),
    Load("Assets/EmaceArt/Slavic World Free/Prefabs/Environment/Rock/EA03_Environment_Rock_Flat_04c_PRE.prefab"),
    Load("Assets/EmaceArt/Slavic World Free/Prefabs/Environment/Rock/EA03_Environment_Rock_Big_Head_01a_PRE.prefab"),
    Load("Assets/EmaceArt/Slavic World Free/Prefabs/Environment/Rock/EA03_Environment_Rock_Mini_Head_01a_PRE.prefab"),
}.Where(x => x != null).ToArray();

var bushPrefabs = new[]
{
    Load("Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Bushes/EA03_Nature_Bush_01a_PRE.prefab"),
    Load("Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Bushes/EA03_Nature_Bush_02a_PRE.prefab"),
    Load("Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Bushes/EA03_Nature_Bush_03b_PRE.prefab"),
    Load("Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Bushes/EA03_Nature_Bush_03d_PRE.prefab"),
}.Where(x => x != null).ToArray();

var grassPrefabs = new[]
{
    Load("Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Grass/EA03_Plant_Grass_01c_PRE.prefab"),
    Load("Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Grass/EA03_Plant_Grass_02a_PRE.prefab"),
}.Where(x => x != null).ToArray();
sb.AppendLine($"prefabs tree={treePrefabs.Length} rock={rockPrefabs.Length} bush={bushPrefabs.Length} grass={grassPrefabs.Length}");

Random.InitState(20261111);

int Scatter(GameObject[] pool, int count, float minR, float maxR, float minPath, float minPoi, float scaleMin, float scaleMax, bool needGround = true)
{
    int placed = 0, attempts = 0;
    while (placed < count && attempts < count * 30)
    {
        attempts++;
        float ang = Random.value * Mathf.PI * 2f;
        float r = Mathf.Lerp(minR, maxR, Mathf.Sqrt(Random.value));
        float x = Mathf.Cos(ang) * r, z = Mathf.Sin(ang) * r;
        if (PathDist(x, z) < minPath) continue;
        if (PoiDist(x, z) < minPoi) continue;
        var p = new Vector3(x, 0f, z);
        p.y = needGround ? GroundY(x, z) : 0f;
        var go = (GameObject)PrefabUtility.InstantiatePrefab(pool[Random.Range(0, pool.Length)]);
        go.transform.SetParent(sceneryRoot, true);
        go.transform.position = p;
        go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        go.transform.localScale = Vector3.one * Random.Range(scaleMin, scaleMax);
        placed++;
    }
    return placed;
}

sb.AppendLine("trees=" + Scatter(treePrefabs, 170, 24f, 86f, 4.5f, 7f, 0.8f, 1.35f));
sb.AppendLine("rocks=" + Scatter(rockPrefabs, 80, 10f, 86f, 3.2f, 4f, 0.6f, 1.5f));
sb.AppendLine("bushes=" + Scatter(bushPrefabs, 140, 8f, 70f, 2.6f, 3f, 0.7f, 1.3f));
sb.AppendLine("grass=" + Scatter(grassPrefabs, 260, 5f, 60f, 1.2f, 1.2f, 0.8f, 1.4f));

// Burnt dressing around the fire site.
var burntProps = new[]
{
    Load("Assets/EmaceArt/Slavic World Free/Prefabs/Prop/Forester/EA03_Prop_Forester_CutOutTrees_03_PRE.prefab")
}.Where(x => x != null).ToArray();
for (int i = 0; i < 7 && burntProps.Length > 0; i++)
{
    float ang = i * 0.9f;
    float x = -2f + Mathf.Cos(ang) * Random.Range(5f, 10f);
    float z = 15f + Mathf.Sin(ang) * Random.Range(4f, 8f);
    var go = (GameObject)PrefabUtility.InstantiatePrefab(burntProps[0]);
    go.transform.SetParent(sceneryRoot, true);
    go.transform.position = new Vector3(x, GroundY(x, z), z);
    go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    go.transform.localScale = Vector3.one * Random.Range(0.8f, 1.2f);
}

// Mission objects.
void Place(string name_, Vector3 p, float lift = 0.03f)
{
    var go = GameObject.Find(name_);
    if (go == null) { sb.AppendLine("MISSING " + name_); return; }
    go.transform.position = new Vector3(p.x, GroundY(p.x, p.z) + lift, p.z);
}

Place("Volunteer post return zone", post);
Place("Volunteer mission board", board);
Place("Inspection marker", inspection);
Place("Planting zone destination", plantDest);
Place("Fire zone destination", fireDest);
Place("tent_detailedOpen_2_9", tent);
Place("log_stack_0_8", logs);

var seeds = Object.FindObjectsByType<SeedlingPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(x => x.Slot).ToArray();
var beds = Object.FindObjectsByType<PlantingPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(x => x.Slot).ToArray();
var fires = Object.FindObjectsByType<FirePoint>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(x => x.Slot).ToArray();
for (int i = 0; i < seeds.Length && i < seedSpots.Length; i++) Place(seeds[i].name, seedSpots[i]);
for (int i = 0; i < beds.Length && i < bedSpots.Length; i++) Place(beds[i].name, bedSpots[i]);
for (int i = 0; i < fires.Length && i < fireSpots.Length; i++) Place(fires[i].name, fireSpots[i]);

// Tame the oversized mission board.
var boardGo = GameObject.Find("Volunteer mission board");
if (boardGo != null)
{
    var rs = boardGo.GetComponentsInChildren<Renderer>(true);
    if (rs.Length > 0)
    {
        var b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        sb.AppendLine("board height=" + b.size.y.ToString("F2"));
        if (b.size.y > 2.6f)
        {
            float k = 2.2f / b.size.y;
            boardGo.transform.localScale *= k;
            boardGo.transform.position += Vector3.up * (b.size.y - b.size.y * k) * 0.5f;
            sb.AppendLine("board scaled by " + k.ToString("F2"));
        }
    }
}

// Player start.
var actor = Object.FindObjectsByType<VolunteerActor>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
if (actor != null)
{
    var rig = actor.GetComponent<VolunteerRig>();
    var spawnPos = new Vector3(spawn.x, GroundY(spawn.x, spawn.z) + 0.1f, spawn.z);
    if (rig != null)
    {
        var so = new SerializedObject(rig);
        var prop = so.FindProperty("spawnPosition");
        if (prop != null) { prop.vector3Value = spawnPos; so.ApplyModifiedPropertiesWithoutUndo(); }
    }
    var cc = actor.GetComponent<CharacterController>();
    if (cc != null) cc.enabled = false;
    actor.transform.position = spawnPos;
    actor.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    if (cc != null) cc.enabled = true;
    sb.AppendLine("spawn=" + spawnPos);
}

// Build settings: Arunika becomes the playable scene.
var list = EditorBuildSettings.scenes.Where(s => s.path != "Assets/Scenes/Arunika.unity").ToList();
list.Insert(0, new EditorBuildSettingsScene("Assets/Scenes/Arunika.unity", true));
EditorBuildSettings.scenes = list.ToArray();
sb.AppendLine("build settings updated");

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
sb.AppendLine("SAVED " + scene.path);
return sb.ToString();
