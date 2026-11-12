using System.Linq;
using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

var sb = new StringBuilder();
var arunika = SceneManager.GetActiveScene();
if (arunika.name != "Arunika") throw new System.Exception("open Arunika first, got " + arunika.name);

var village = EditorSceneManager.OpenScene("Assets/Scenes/VillageRestoration.unity", OpenSceneMode.Additive);
var glade = village.GetRootGameObjects().FirstOrDefault(x => x.name == "Restoration Ground");
if (glade == null) throw new System.Exception("Restoration Ground missing");

string[] wanted = {
    "Volunteer post return zone", "Inspection marker", "Volunteer mission board",
    "tent_detailedOpen_2_9", "log_stack_0_8"
};
foreach (var name in wanted)
{
    Transform found = null;
    foreach (var t in glade.GetComponentsInChildren<Transform>(true))
        if (t.name == name) { found = t; break; }
    if (found == null) { sb.AppendLine("MISSING " + name); continue; }
    found.SetParent(null, true);
    SceneManager.MoveGameObjectToScene(found.gameObject, arunika);
    sb.AppendLine("moved " + name + " -> " + found.position.ToString("F2"));
}

EditorSceneManager.CloseScene(village, true);
EditorSceneManager.MarkSceneDirty(arunika);
EditorSceneManager.SaveScene(arunika);
sb.AppendLine("rootA children=" + GameObject.Find("Zone A - volunteer post and damaged forest").transform.childCount);
return sb.ToString();
