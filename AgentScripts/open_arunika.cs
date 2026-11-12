using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Rimbara.Player;

var s = EditorSceneManager.OpenScene("Assets/Scenes/Arunika.unity", OpenSceneMode.Single);
var sb = new StringBuilder();
sb.AppendLine("opened=" + s.name + " dirty=" + s.isDirty);
var rig = Object.FindFirstObjectByType<VolunteerRig>(FindObjectsInactive.Include);
if (rig == null) sb.AppendLine("NO RIG");
else {
    var src = PrefabUtility.GetCorrespondingObjectFromSource(rig.gameObject);
    var path = src != null ? AssetDatabase.GetAssetPath(src) : "none";
    sb.AppendLine("rig=" + rig.name + " srcPrefab=" + path);
    var an = rig.Animator;
    sb.AppendLine("avatar=" + (an != null && an.avatar != null ? an.avatar.name + " human=" + an.avatar.isHuman : "null"));
    sb.AppendLine("ctrl=" + (an != null && an.runtimeAnimatorController != null ? an.runtimeAnimatorController.name + " clips=" + an.runtimeAnimatorController.animationClips.Length : "null"));
    var rends = rig.GetComponentsInChildren<Renderer>(true);
    sb.AppendLine("renderers=" + rends.Length);
    foreach (var r in rends) sb.AppendLine("  " + r.name + " mats=" + string.Join(",", System.Linq.Enumerable.Select(r.sharedMaterials, m => m != null ? m.name : "null")));
}
var ui = GameObject.Find("UI - RIMBARA");
sb.AppendLine("ui=" + (ui != null ? ui.name : "null"));
return sb.ToString();
