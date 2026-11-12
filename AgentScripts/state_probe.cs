using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

var s = SceneManager.GetActiveScene();
var sb = new StringBuilder();
sb.AppendLine("play=" + EditorApplication.isPlaying + " playingOrWill=" + EditorApplication.isPlayingOrWillChangePlaymode);
sb.AppendLine("scene=" + s.name + " path=" + s.path + " dirty=" + s.isDirty);
sb.AppendLine("unity=" + Application.unityVersion + " log=" + Application.consoleLogPath);
var ui = GameObject.Find("UI - RIMBARA");
if (ui != null) {
  var t = ui.transform;
  foreach (var c in t.GetComponentsInChildren<Transform>(true)) {
    if (c.parent != t) continue;
    sb.AppendLine("screen " + c.name + " active=" + c.gameObject.activeSelf);
  }
} else sb.AppendLine("UI - RIMBARA not found");
var rig = Object.FindFirstObjectByType<Rimbara.Player.VolunteerRig>();
sb.AppendLine("rig=" + (rig != null ? rig.name + " prefab=" + PrefabUtility.IsPartOfPrefabInstance(rig.gameObject) : "none"));
File.WriteAllText("AgentScripts/_state.txt", sb.ToString());
return sb.ToString();
