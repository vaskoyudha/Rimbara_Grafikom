using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Rimbara.Interaction;
using Rimbara.Player;

var sb = new StringBuilder();
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.name != "VillageRestoration") throw new System.Exception("wrong scene " + scene.name);

var actor = Object.FindObjectsByType<VolunteerActor>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
if (actor == null) throw new System.Exception("no volunteer");
var rig = actor.GetComponent<VolunteerRig>();
if (rig == null) throw new System.Exception("no VolunteerRig on actor");

var spawn = new Vector3(20f, 1.05f, -74.6f);
var so = new SerializedObject(rig);
var prop = so.FindProperty("spawnPosition");
if (prop != null) { prop.vector3Value = spawn; so.ApplyModifiedPropertiesWithoutUndo(); sb.AppendLine("spawn set " + spawn); }
else sb.AppendLine("spawnPosition field not found");

var cc = actor.GetComponent<CharacterController>();
if (cc != null) cc.enabled = false;
actor.transform.position = spawn;
actor.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
if (cc != null) cc.enabled = true;

var cam = Camera.main;
sb.AppendLine("camera=" + (cam != null ? cam.transform.position.ToString("F2") : "null"));

foreach (var name in new[] { "Inspection marker", "Volunteer post return zone", "Native seedling 01", "Planting bed 01", "Remaining fire 01" }) {
    var go = GameObject.Find(name);
    sb.AppendLine(name + " = " + (go != null ? go.transform.position.ToString("F2") : "missing"));
}

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
sb.AppendLine("SAVED");
return sb.ToString();
