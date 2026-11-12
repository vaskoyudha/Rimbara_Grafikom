using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Rimbara.Interaction;

var sb = new StringBuilder();
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.name != "Arunika") throw new System.Exception("wrong scene " + scene.name);

var mud = AssetDatabase.LoadAssetAtPath<Material>("Assets/EmaceArt/Slavic World Free/Materials/Mud.mat");
var beds = Object.FindObjectsByType<PlantingPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(x => x.Slot).ToArray();
foreach (var bed in beds)
{
    var existing = bed.transform.Find("Planting soil patch");
    if (existing != null) Object.DestroyImmediate(existing.gameObject);

    var patch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    patch.name = "Planting soil patch";
    Object.DestroyImmediate(patch.GetComponent<Collider>());
    patch.transform.SetParent(bed.transform, false);
    patch.transform.localPosition = new Vector3(0f, -0.01f, 0f);
    patch.transform.localScale = new Vector3(2.6f, 0.015f, 2.6f);
    if (mud != null) patch.GetComponent<Renderer>().sharedMaterial = mud;

    for (int i = 0; i < 5; i++)
    {
        float ang = i * 72f + 18f;
        var stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        stone.name = "Bed edge stone";
        Object.DestroyImmediate(stone.GetComponent<Collider>());
        stone.transform.SetParent(bed.transform, false);
        stone.transform.localPosition = new Vector3(Mathf.Cos(ang * Mathf.Deg2Rad) * 1.45f, 0.06f, Mathf.Sin(ang * Mathf.Deg2Rad) * 1.45f);
        stone.transform.localScale = new Vector3(0.34f, 0.18f, 0.28f);
        stone.GetComponent<Renderer>().sharedMaterial = mud;
    }
}
sb.AppendLine("patched beds=" + beds.Length);

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
sb.AppendLine("SAVED");
return sb.ToString();
