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
if (glade == null) throw new System.Exception("Restoration Ground missing in VillageRestoration");

var zones = new[] {
    "Zone A - volunteer post and damaged forest",
    "Zone B - seedling nursery and planting clearings",
    "Zone C - remaining fires"
};

foreach (var zn in zones)
{
    var t = glade.transform.Find(zn);
    if (t == null) { sb.AppendLine("MISSING " + zn); continue; }
    t.SetParent(null, true);
    SceneManager.MoveGameObjectToScene(t.gameObject, arunika);
    sb.AppendLine("moved " + zn + " children=" + t.childCount);
}

EditorSceneManager.CloseScene(village, true);

foreach (var n in zones)
    sb.AppendLine(n + " -> " + (GameObject.Find(n) != null));

EditorSceneManager.MarkSceneDirty(arunika);
EditorSceneManager.SaveScene(arunika);
sb.AppendLine("SAVED");
return sb.ToString();
