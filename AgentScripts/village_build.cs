using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Builds a Slavic village settlement into the currently open RIMBARA Main scene,
// replacing the Kenney forest props while leaving all gameplay objects in place.

const string Root = "Assets/EmaceArt/Slavic World Free/Prefabs/";
var terrain = Terrain.activeTerrain;
if (terrain == null) throw new Exception("No active terrain");

var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
var report = new StringBuilder();

float Ground(float x, float z) => terrain.SampleHeight(new Vector3(x, 0f, z)) + terrain.transform.position.y;

GameObject Load(string rel) {
    var p = Root + rel;
    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(p);
    if (prefab == null) throw new Exception("Missing prefab " + p);
    return prefab;
}

var village = GameObject.Find("Village - Slavic World settlement");
if (village == null) village = new GameObject("Village - Slavic World settlement");
village.transform.SetParent(null, true);

int spawned = 0;
GameObject Place(string rel, float x, float z, float yaw, float scale = 1f, float yOffset = 0f, Transform parent = null) {
    var prefab = Load(rel);
    var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
    go.transform.SetParent(parent == null ? village.transform : parent, true);
    float y = Ground(x, z) + yOffset;
    go.transform.position = new Vector3(x, y, z);
    go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    go.transform.localScale = prefab.transform.localScale * scale;
    spawned++;
    return go;
}

void AddBox(GameObject go, string name) {
    var rs = go.GetComponentsInChildren<Renderer>();
    if (rs.Length == 0) return;
    var b = rs[0].bounds;
    foreach (var r in rs) b.Encapsulate(r.bounds);
    var holder = new GameObject(name + " collision");
    holder.transform.SetParent(go.transform, false);
    holder.transform.localPosition = go.transform.InverseTransformPoint(b.center);
    holder.transform.localRotation = Quaternion.Inverse(go.transform.rotation);
    var box = holder.AddComponent<BoxCollider>();
    box.size = b.size;
}

// 1. Remove the old Kenney forest dressing.
var map = GameObject.Find("Arunika - authored restoration map");
if (map != null) {
    var forest = map.transform.Find("Forest edge and static scenery");
    if (forest != null) { UnityEngine.Object.DestroyImmediate(forest.gameObject); report.AppendLine("REMOVED Forest edge and static scenery"); }
    else report.AppendLine("WARN forest root not found");
}

// 2. Cobble road along the mission corridor (Z axis).
for (float z = -32f; z <= 64f; z += 1.7f) {
    Place("Environment/Road/EA03_Environment_Road_Cobble_01a_PRE.prefab", 0f, z, 0f, 1f, 0.02f);
}
report.AppendLine("ROAD pieces=" + spawned);

// 3. Houses set back from the corridor, facing the road.
var houses = new (string path, float x, float z, float yaw, float lift)[] {
    ("Town/Building/EA03_Town_House_Comp_01a_PRE.prefab", -16f, -2f, 90f, 0f),
    ("Town/Building/EA03_Town_House_Comp_03a_PRE.prefab", -19f, 14f, 90f, 4.44f),
    ("Town/Building/EA03_Town_House_Comp_01a_PRE.prefab", -15f, 32f, 90f, 0f),
    ("Town/Building/EA03_Town_House_Comp_02a_PRE.prefab", 16f, 6f, 270f, 4.13f),
    ("Town/Building/EA03_Town_House_Comp_01a_PRE.prefab", 15f, 24f, 270f, 0f),
    ("Town/Building/EA03_Town_House_Comp_03a_PRE.prefab", 18f, 44f, 270f, 4.44f),
};
foreach (var h in houses) {
    var go = Place(h.path, h.x, h.z, h.yaw, 1f, h.lift + 0.05f);
    go.name = "Village house " + h.x + "/" + h.z;
    AddBox(go, go.name);
}
report.AppendLine("HOUSES=" + houses.Length);

// 4. Tower landmark and a shed behind the right-hand row.
var tower = Place("Village/Building/Other/EA03_Village_Tover_01a_PRE.prefab", 27f, 36f, 315f, 1f, 0.5f);
tower.name = "Village tower"; AddBox(tower, tower.name);
var shed = Place("Village/Building/Shed/EA03_Village_OutBuilding_Shed_03b_PRE.prefab", -26f, 46f, 90f);
shed.name = "Village shed"; AddBox(shed, shed.name);

// 5. Fence plots around the houses and a gate at the entrance.
float[][] plots = {
    new float[]{-17f, 4f, -26f, 4f}, new float[]{-17f, 4f, -26f, 24f},
    new float[]{-22f, 24f, -26f, 40f}, new float[]{-17f, 40f, -26f, 40f},
    new float[]{15f, 14f, 24f, 14f}, new float[]{15f, 14f, 24f, 34f},
    new float[]{15f, 34f, 24f, 34f}, new float[]{15f, 34f, 24f, 52f}
};
foreach (var l in plots) {
    float x1 = l[0], z1 = l[1], x2 = l[2], z2 = l[3];
    float len = Vector2.Distance(new Vector2(x1, z1), new Vector2(x2, z2));
    int count = Mathf.Max(1, Mathf.RoundToInt(len / 4f));
    for (int i = 0; i < count; i++) {
        float t = (i + 0.5f) / count;
        float x = Mathf.Lerp(x1, x2, t), z = Mathf.Lerp(z1, z2, t);
        float yaw = Mathf.Atan2(x2 - x1, z2 - z1) * Mathf.Rad2Deg + 90f;
        Place("Fence/Plank2/EA03_Fence_Plank_02a_PRE.prefab", x, z, yaw);
    }
}
Place("Fence/Wall/EA03_Fence_WallGate_01a_PRE.prefab", -21.5f, 4f, 90f);

// 6. Fields and props in the gardens.
Place("Terrain/Field/EA03_Environment_Field_Comp_01a.prefab", -30f, 26f, 0f);
Place("Terrain/Field/EA03_Environment_Field_Comp_01a.prefab", -30f, 32f, 0f);
Place("Terrain/Field/EA03_Environment_Field_Comp_01a.prefab", 30f, 16f, 0f);
string[] props = {
    "Prop/Container/EA03_Prop_Container_Barrel_01d_PRE.prefab",
    "Prop/Container/EA03_Prop_Container_Crate_01a_PRE.prefab",
    "Prop/Container/EA03_Prop_Container_Basket_01a_PRE.prefab",
    "Prop/Container/EA03_Prop_Container_Bag_02a_PRE.prefab"
};
var propSpots = new (float x, float z)[] { (-13f,-4f), (-21f,10f), (-13f,34f), (13f,4f), (20f,20f), (13f,42f), (-24f,44f), (22f,48f) };
for (int i = 0; i < propSpots.Length; i++) {
    var p = props[i % props.Length];
    Place(p, propSpots[i].x + UnityEngine.Random.Range(-0.6f, 0.6f), propSpots[i].z + UnityEngine.Random.Range(-0.6f, 0.6f), UnityEngine.Random.Range(0f, 360f));
}

// 7. Slavic nature replacing the Kenney forest: perimeter trees and bushes.
for (int i = 0; i < 40; i++) {
    float side = i % 2 == 0 ? -1f : 1f;
    float x = side * UnityEngine.Random.Range(30f, 56f);
    float z = UnityEngine.Random.Range(-34f, 76f);
    string tree = UnityEngine.Random.value < 0.5f ? "Nature/Tree/EA03_Nature_Tree_01b_PRE.prefab" : "Nature/Tree/EA03_Nature_Tree_02c_PRE.prefab";
    Place(tree, x, z, UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(0.9f, 1.35f));
}
for (int i = 0; i < 45; i++) {
    float x = UnityEngine.Random.Range(-45f, 45f);
    float z = UnityEngine.Random.Range(-34f, 74f);
    if (Mathf.Abs(x) < 10f) continue; // keep the mission corridor clear
    Place("Nature/Bushes/EA03_Nature_Bush_01a_PRE.prefab", x, z, UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(0.8f, 1.4f));
}

// 8. Ground cover greening the burnt terrain around the settlement.
for (int i = 0; i < 90; i++) {
    float x = UnityEngine.Random.Range(-46f, 46f);
    float z = UnityEngine.Random.Range(-34f, 74f);
    if (Mathf.Abs(x) < 8f) continue;
    string grass = UnityEngine.Random.value < 0.5f
        ? "Nature/Grass/EA03_Plant_Grass_01c_PRE.prefab"
        : "Nature/Grass/EA03_Plant_Grass_02a_PRE.prefab";
    Place(grass, x, z, UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(1f, 1.8f));
}

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
report.AppendLine($"SPAWNED total={spawned}");
report.AppendLine("SCENE SAVED " + scene.path);
return report.ToString();
