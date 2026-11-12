using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using Rimbara.Interaction;
using Rimbara.Mission;

var report = new StringBuilder();
var village = SceneManager.GetActiveScene();
if (village.name != "VillageRestoration") throw new Exception("Open VillageRestoration first, current=" + village.name);

var main = EditorSceneManager.OpenScene("Assets/Scenes/Main.unity", OpenSceneMode.Additive);
var mainRoots = main.GetRootGameObjects();

// Remove the village demo's own controller and camera before RIMBARA's same-named roots arrive.
foreach (var n in new[] { "Player", "Main Camera" }) {
    var go = village.GetRootGameObjects().FirstOrDefault(x => x.name == n);
    if (go != null) { UnityEngine.Object.DestroyImmediate(go); report.AppendLine("REMOVED village " + n); }
}

string[] roots = {
    "Game systems - profiles, missions and flow",
    "Volunteer",
    "Main Camera",
    "Cinemachine - volunteer third-person",
    "Cinemachine - Arunika overview",
    "UI - RIMBARA",
    "EventSystem",
    "Audio - original procedural soundscape"
};
foreach (var name in roots) {
    var go = mainRoots.FirstOrDefault(x => x.name == name);
    if (go == null) { report.AppendLine("MISSING ROOT " + name); continue; }
    SceneManager.MoveGameObjectToScene(go, village);
    report.AppendLine("MOVED " + name);
}

var map = mainRoots.FirstOrDefault(x => x.name == "Arunika - authored restoration map");
if (map == null) throw new Exception("map root missing");
string[] zones = {
    "Zone A - volunteer post and damaged forest",
    "Zone B - seedling nursery and planting clearings",
    "Zone C - remaining fires"
};
foreach (var zn in zones) {
    var z = map.transform.Find(zn);
    if (z == null) { report.AppendLine("MISSING ZONE " + zn); continue; }
    z.SetParent(null, true);
    SceneManager.MoveGameObjectToScene(z.gameObject, village);
    report.AppendLine("MOVED ZONE " + zn);
}
EditorSceneManager.CloseScene(main, true);

// Remove the Kenney dressing inside the zones.
foreach (var n in new[] { "A / DAMAGED FOREST", "B / NEW ROOTS", "C / LAST FLAMES" }) {
    var t = GameObject.Find(n);
    if (t != null) { UnityEngine.Object.DestroyImmediate(t); report.AppendLine("REMOVED dressing " + n); }
}

float Ground(float x, float z) {
    if (Physics.Raycast(new Vector3(x, 200f, z), Vector3.down, out var hit, 500f)) return hit.point.y;
    return 0f;
}
void Put(Transform t, float x, float z, float lift = 0.05f) {
    t.position = new Vector3(x, Ground(x, z) + lift, z);
}

var seeds = UnityEngine.Object.FindObjectsByType<SeedlingPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(s => s.Slot).ToArray();
var beds = UnityEngine.Object.FindObjectsByType<PlantingPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(b => b.Slot).ToArray();
var fires = UnityEngine.Object.FindObjectsByType<FirePoint>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(f => f.Slot).ToArray();

var seedSpots = new (float x, float z)[] { (-8f, -22f), (-4f, -22f), (0f, -22f), (-8f, -20f), (-2f, -20f) };
var bedSpots  = new (float x, float z)[] { (6f, -40f), (8f, -40f), (10f, -40f), (6f, -36f), (8f, -36f) };
var fireSpots = new (float x, float z)[] { (-8f, -26f), (0f, -26f) };

for (int i = 0; i < seeds.Length && i < seedSpots.Length; i++) Put(seeds[i].transform, seedSpots[i].x, seedSpots[i].z);
for (int i = 0; i < beds.Length && i < bedSpots.Length; i++) Put(beds[i].transform, bedSpots[i].x, bedSpots[i].z);
for (int i = 0; i < fires.Length && i < fireSpots.Length; i++) Put(fires[i].transform, fireSpots[i].x, fireSpots[i].z);

var postZone = GameObject.Find("Volunteer post return zone");
var inspection = GameObject.Find("Inspection marker");
var board = GameObject.Find("Volunteer mission board");
var plantDest = GameObject.Find("Planting zone destination");
var fireDest = GameObject.Find("Fire zone destination");
var volunteer = UnityEngine.Object.FindObjectsByType<VolunteerActor>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();

if (postZone != null) Put(postZone.transform, 0f, -40f);
if (inspection != null) Put(inspection.transform, 4f, -38f);
if (board != null) Put(board.transform, 2f, -41f);
if (plantDest != null) Put(plantDest.transform, 8f, -38f);
if (fireDest != null) Put(fireDest.transform, -4f, -26f);
if (volunteer != null) { Put(volunteer.transform, 0f, -39f, 0.2f); report.AppendLine("VOLUNTEER placed"); }

report.AppendLine($"counts seed={seeds.Length} bed={beds.Length} fire={fires.Length}");

// Register the scene for play/build.
var list = EditorBuildSettings.scenes.ToList();
if (!list.Any(s => s.path == village.path)) {
    list.Insert(0, new EditorBuildSettingsScene(village.path, true));
    EditorBuildSettings.scenes = list.ToArray();
    report.AppendLine("BUILD SETTINGS added " + village.path);
}

EditorSceneManager.MarkSceneDirty(village);
EditorSceneManager.SaveScene(village);
report.AppendLine("SAVED " + village.path);
return report.ToString();
