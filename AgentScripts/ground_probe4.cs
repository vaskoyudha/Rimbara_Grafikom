using System.Linq;
using System.Text;
using UnityEngine;
using Rimbara.Player;

var sb = new StringBuilder();
var rig = Object.FindFirstObjectByType<VolunteerRig>(FindObjectsInactive.Include);
var an = rig.Animator;
sb.AppendLine("play=" + Application.isPlaying + " rig=" + rig.transform.position.ToString("F3") + " lossyScale=" + rig.transform.lossyScale.ToString("F3"));
foreach (var b in new[] { HumanBodyBones.Head, HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes })
{ var t = an.GetBoneTransform(b); sb.AppendLine("bone " + b + "=" + t.position.ToString("F3")); }
var terr = Object.FindFirstObjectByType<Terrain>();
sb.AppendLine("terrain height at rig=" + (terr != null ? terr.SampleHeight(rig.transform.position).ToString("F3") : "n/a"));
var rays = Physics.RaycastAll(rig.transform.position + Vector3.up * 1.5f, Vector3.down, 20f, ~(1 << 9), QueryTriggerInteraction.Ignore)
    .OrderBy(h => h.distance).ToArray();
if (rays.Length > 0) foreach (var h in rays.Take(3)) sb.AppendLine("ground hit y=" + h.point.y.ToString("F3") + " dist=" + h.distance.ToString("F2") + " collider=" + h.collider.name + " layer=" + h.collider.gameObject.layer);
else sb.AppendLine("no non-player ground hit");
foreach (var name in new[] { "Body", "Sneakers" })
{
    var smr = rig.GetComponentsInChildren<SkinnedMeshRenderer>(true).FirstOrDefault(r => r.name.Contains(name));
    if (smr == null) { sb.AppendLine(name + " missing"); continue; }
    var mesh = new Mesh(); smr.BakeMesh(mesh, true);
    var verts = mesh.vertices;
    var m = smr.localToWorldMatrix;
    float rawMin = verts.Min(v => v.y), rawMax = verts.Max(v => v.y);
    float wMin = verts.Min(v => m.MultiplyPoint3x4(v).y), wMax = verts.Max(v => m.MultiplyPoint3x4(v).y);
    sb.AppendLine(name + " t=" + smr.transform.position.ToString("F3") + " rawY=" + rawMin.ToString("F3") + ".." + rawMax.ToString("F3") + " L2WY=" + wMin.ToString("F3") + ".." + wMax.ToString("F3") + " smrBoundsY=" + smr.bounds.min.y.ToString("F3") + ".." + smr.bounds.max.y.ToString("F3"));
}
var cc = rig.GetComponent<CharacterController>();
sb.AppendLine("CC bottomY=" + (rig.transform.position.y + cc.center.y - cc.height * 0.5f).ToString("F3") + " height=" + cc.height + " skin=" + cc.skinWidth);
return sb.ToString();
