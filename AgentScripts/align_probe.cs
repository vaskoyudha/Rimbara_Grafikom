using System.Linq;
using System.Text;
using UnityEngine;
using Rimbara.Player;

var sb = new StringBuilder();
var rig = Object.FindFirstObjectByType<VolunteerRig>(FindObjectsInactive.Include);
var an = rig.Animator;
sb.AppendLine("play=" + Application.isPlaying + " rigY=" + rig.transform.position.y.ToString("F4"));
var cc = rig.GetComponent<CharacterController>();
float ccBottom = rig.transform.position.y + cc.center.y - cc.height * 0.5f;
sb.AppendLine("ccBottom=" + ccBottom.ToString("F4") + " ccHeight=" + cc.height);
var smr = rig.GetComponentsInChildren<SkinnedMeshRenderer>(true).FirstOrDefault(r => r.name.Contains("Sneaker"));
var mesh = new Mesh(); smr.BakeMesh(mesh, true);
var verts = mesh.vertices; var m = smr.localToWorldMatrix;
float sole = verts.Min(v => m.MultiplyPoint3x4(v).y);
float pinkie = verts.Max(v => m.MultiplyPoint3x4(v).y);
sb.AppendLine("soleY=" + sole.ToString("F4") + " shoeTopY=" + pinkie.ToString("F4") + " deltaFromCCBottom=" + (sole - ccBottom).ToString("F4"));
var lf = an.GetBoneTransform(HumanBodyBones.LeftFoot);
var lt = an.GetBoneTransform(HumanBodyBones.LeftToes);
sb.AppendLine("LeftFoot boneY=" + lf.position.y.ToString("F3") + " toeY=" + lt.position.y.ToString("F3"));
sb.AppendLine("smr.localPos=" + smr.transform.localPosition.ToString("F4") + " worldPos=" + smr.transform.position.ToString("F4"));
var t = smr.transform; while (t != null && t != rig.transform) { sb.AppendLine("  node " + t.name + " localPos=" + t.localPosition.ToString("F4") + " localScale=" + t.localScale.ToString("F3")); t = t.parent; }
var terr = Object.FindFirstObjectByType<Terrain>();
if (terr != null) sb.AppendLine("terrain under rig=" + terr.SampleHeight(rig.transform.position).ToString("F4"));
return sb.ToString();
