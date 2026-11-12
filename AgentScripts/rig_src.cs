using System.Text;
using UnityEditor;
using UnityEngine;
using Rimbara.Player;

var sb = new StringBuilder();
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
sb.AppendLine("scene=" + scene.name);
var rig = Object.FindFirstObjectByType<VolunteerRig>(FindObjectsInactive.Include);
if (rig == null) { sb.AppendLine("NO RIG"); }
else {
    var src = PrefabUtility.GetCorrespondingObjectFromSource(rig.gameObject);
    var path = src != null ? AssetDatabase.GetAssetPath(src) : "none";
    sb.AppendLine("rig=" + rig.name + " srcPrefab=" + path + " isInstance=" + PrefabUtility.IsPartOfPrefabInstance(rig.gameObject));
    var an = rig.Animator;
    sb.AppendLine("avatar=" + (an != null && an.avatar != null ? an.avatar.name + " human=" + an.avatar.isHuman : "null"));
    sb.AppendLine("ctrl=" + (an != null && an.runtimeAnimatorController != null ? an.runtimeAnimatorController.name : "null"));
    var rends = rig.GetComponentsInChildren<Renderer>(true);
    sb.AppendLine("renderers=" + rends.Length);
    foreach (var r in rends) if (r.name.ToLower().Contains("body") || r.name.ToLower().Contains("ch42")) sb.AppendLine("  mesh " + r.name + " mats=" + string.Join(",", System.Linq.Enumerable.Select(r.sharedMaterials, m => m != null ? m.name : "null")));
}
return sb.ToString();
