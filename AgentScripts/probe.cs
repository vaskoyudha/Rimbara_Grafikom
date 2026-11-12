using System;
using System.Text;
using UnityEngine;
using UnityEditor;

string[] paths = {
 "Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_01a_PRE.prefab",
 "Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_02a_PRE.prefab",
 "Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_03a_PRE.prefab",
 "Assets/EmaceArt/Slavic World Free/Prefabs/Environment/Road/EA03_Environment_Road_Cobble_01a_PRE.prefab",
 "Assets/EmaceArt/Slavic World Free/Prefabs/Fence/Plank2/EA03_Fence_Plank_02a_PRE.prefab",
 "Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Tree/EA03_Nature_Tree_01b_PRE.prefab",
 "Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Bushes/EA03_Nature_Bush_01a_PRE.prefab",
 "Assets/EmaceArt/Slavic World Free/Prefabs/Environment/Road/EA03_Village_platform_01a_PRE.prefab",
 "Assets/EmaceArt/Slavic World Free/Prefabs/Terrain/Field/EA03_Environment_Field_Comp_01a.prefab",
 "Assets/EmaceArt/Slavic World Free/Prefabs/Environment/Bridge/EA03_Prop_Pier_03d_PRE.prefab",
 "Assets/EmaceArt/Slavic World Free/Prefabs/Village/Building/Other/EA03_Village_Tover_01a_PRE.prefab",
 "Assets/EmaceArt/Slavic World Free/Prefabs/Prop/Container/EA03_Prop_Container_Barrel_01d_PRE.prefab"
};
var sb = new StringBuilder();
var ps = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
foreach (var p in paths) {
    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(p);
    if (prefab == null) { sb.AppendLine("MISSING " + p); continue; }
    var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, ps);
    var rs = go.GetComponentsInChildren<Renderer>();
    if (rs.Length == 0) { sb.AppendLine("NO RENDERER " + prefab.name); UnityEngine.Object.DestroyImmediate(go); continue; }
    var b = rs[0].bounds;
    foreach (var r in rs) b.Encapsulate(r.bounds);
    var c = go.GetComponent<Collider>();
    sb.AppendLine($"{prefab.name}: size=({b.size.x:F2},{b.size.y:F2},{b.size.z:F2}) centerRel=({b.center.x:F2},{b.center.y:F2},{b.center.z:F2}) baseY={b.min.y:F2} collider={(c!=null)} renderers={rs.Length}");
    UnityEngine.Object.DestroyImmediate(go);
}
UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(ps);
return sb.ToString();
