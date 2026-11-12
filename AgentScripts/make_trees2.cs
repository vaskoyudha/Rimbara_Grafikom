using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

var sb = new StringBuilder();
var variants = new (string path, string name)[]
{
    ("Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Tree/EA03_Nature_Tree_02b_PRE.prefab", "RestoredTree_01"),
    ("Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Tree/EA03_Nature_Tree_03b_PRE.prefab", "RestoredTree_02"),
    ("Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Tree/EA03_Nature_Tree_06b_PRE.prefab", "RestoredTree_03"),
};

foreach (var v in variants)
{
    var src = AssetDatabase.LoadAssetAtPath<GameObject>(v.path);
    if (src == null) { sb.AppendLine("MISSING " + v.path); continue; }
    var go = (GameObject)PrefabUtility.InstantiatePrefab(src);
    go.name = v.name;

    var renderers = go.GetComponentsInChildren<Renderer>(true);
    var b = renderers[0].bounds;
    foreach (var r in renderers) b.Encapsulate(r.bounds);
    float scale = 4.4f / Mathf.Max(0.01f, b.size.y);
    go.transform.localScale = Vector3.one * scale;

    renderers = go.GetComponentsInChildren<Renderer>(true);
    b = renderers[0].bounds;
    foreach (var r in renderers) b.Encapsulate(r.bounds);
    go.transform.position = new Vector3(0f, -b.min.y, 0f);

    var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Environment/" + v.name + ".prefab", out bool success);
    sb.AppendLine($"{v.name} <- {System.IO.Path.GetFileNameWithoutExtension(v.path)} h={b.size.y:F1} scale={scale:F2} saved={success}");
    Object.DestroyImmediate(go);
}
AssetDatabase.SaveAssets();
AssetDatabase.Refresh();
return sb.ToString();
