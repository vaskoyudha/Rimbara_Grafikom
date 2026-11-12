using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Rimbara.Mission;
using Rimbara.Interaction;

var sb = new StringBuilder();
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.name != "Arunika") throw new System.Exception("wrong scene " + scene.name);

var state = Object.FindFirstObjectByType<GameState>();
var missions = Object.FindFirstObjectByType<MissionSystem>();
if (state == null || missions == null) throw new System.Exception("game systems missing");

int fixedFires = 0, fixedZones = 0;
foreach (var fire in Object.FindObjectsByType<FirePoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
{
    var so = new SerializedObject(fire);
    var s = so.FindProperty("state");
    var m = so.FindProperty("missions");
    if (s != null) s.objectReferenceValue = state;
    if (m != null) m.objectReferenceValue = missions;
    so.ApplyModifiedPropertiesWithoutUndo();
    PrefabUtility.RecordPrefabInstancePropertyModifications(fire);
    fixedFires++;
}
foreach (var zone in Object.FindObjectsByType<MissionZone>(FindObjectsInactive.Include, FindObjectsSortMode.None))
{
    var so = new SerializedObject(zone);
    var m = so.FindProperty("missions");
    if (m != null) m.objectReferenceValue = missions;
    so.ApplyModifiedPropertiesWithoutUndo();
    PrefabUtility.RecordPrefabInstancePropertyModifications(zone);
    fixedZones++;
}

sb.AppendLine($"fires={fixedFires} zones={fixedZones}");
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
sb.AppendLine("SAVED");
return sb.ToString();
