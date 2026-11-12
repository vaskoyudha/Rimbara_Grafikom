using System.Linq;
using System.Text;
using UnityEngine;
using Rimbara.Player;

var sb = new StringBuilder();
var rig = Object.FindFirstObjectByType<VolunteerRig>(FindObjectsInactive.Include);
var an = rig.Animator;
sb.AppendLine("rig=" + rig.transform.position.ToString("F3") + " lossyScale=" + rig.transform.lossyScale.ToString("F3") + " localScale=" + rig.transform.localScale.ToString("F3"));
foreach (var b in new[] { HumanBodyBones.Head, HumanBodyBones.RightHand, HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes })
{ var t = an.GetBoneTransform(b); sb.AppendLine("bone " + b + "=" + t.position.ToString("F3")); }
var terr = Object.FindFirstObjectByType<Terrain>();
sb.AppendLine("terrain height at rig=" + (terr != null ? terr.SampleHeight(rig.transform.position).ToString("F3") : "n/a"));
foreach (var name in new[] { "Body", "Sneakers" })
{
    var smr = rig.GetComponentsInChildren<SkinnedMeshRenderer>(true).FirstOrDefault(r => r.name.Contains(name));
    if (smr == null) { sb.AppendLine(name + " missing"); continue; }
    var mesh = new Mesh(); smr.BakeMesh(mesh, true);
    var verts = mesh.vertices;
    var m = smr.localToWorldMatrix;
    var rawMin = verts.OrderBy(v => v.y).First();
    var rawMax = verts.OrderByDescending(v => v.y).First();
    var wMin = verts.OrderBy(v => m.MultiplyPoint3x4(v).y).First();
    var wMax = verts.OrderByDescending(v => m.MultiplyPoint3x4(v).y).First();
    sb.AppendLine(name + " t=" + smr.transform.position.ToString("F3") + " rawY=" + rawMin.y.ToString("F3") + ".." + rawMax.y.ToString("F3") + " rawLowestVert=" + rawMin.ToString("F3"));
    sb.AppendLine("   L2W Y=" + m.MultiplyPoint3x4(wMin).y.ToString("F3") + ".." + m.MultiplyPoint3x4(wMax).y.ToString("F3") + " lowestWorldVert=" + m.MultiplyPoint3x4(wMin).ToString("F3") + " smrBoundsMin=" + smr.bounds.min.ToString("F3") + " max=" + smr.bounds.max.ToString("F3"));
}
return sb.ToString();
