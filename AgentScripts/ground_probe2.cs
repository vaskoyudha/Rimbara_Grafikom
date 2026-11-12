using System.Linq;
using System.Text;
using UnityEngine;
using Rimbara.Player;

var sb = new StringBuilder();
var rig = Object.FindFirstObjectByType<VolunteerRig>(FindObjectsInactive.Include);
var smr = rig.GetComponentsInChildren<SkinnedMeshRenderer>(true).FirstOrDefault(r => r.name.Contains("Sneaker"));
var mesh = new Mesh();
smr.BakeMesh(mesh, true);
var verts = mesh.vertices;
float rawMin = verts.Min(v => v.y);
var m = smr.localToWorldMatrix;
float worldMin = verts.Min(v => m.MultiplyPoint3x4(v).y);
sb.AppendLine("smr.name=" + smr.name + " smr.transform=" + smr.transform.position.ToString("F3") + " lossyScale=" + smr.transform.lossyScale.ToString("F3"));
sb.AppendLine("baked rawMinY=" + rawMin.ToString("F4") + " afterL2W=" + worldMin.ToString("F4") + " verts=" + verts.Length);
sb.AppendLine("smr.bounds min=" + smr.bounds.min.ToString("F3") + " center=" + smr.bounds.center.ToString("F3") + " extents=" + smr.bounds.extents.ToString("F3"));
sb.AppendLine("smr.localBounds=" + smr.localBounds.ToString("F3"));
var cc = rig.GetComponent<CharacterController>();
sb.AppendLine("CC center=" + cc.center.ToString("F3") + " height=" + cc.height + " radius=" + cc.radius + " skin=" + cc.skinWidth + " minY=" + (rig.transform.position.y + cc.center.y - cc.height * 0.5f).ToString("F3") + " step=" + cc.stepOffset);
RaycastHit hit;
if (Physics.Raycast(rig.transform.position + Vector3.up * 2f, Vector3.down, out hit, 20f, ~0, QueryTriggerInteraction.Ignore))
    sb.AppendLine("ground hit=" + hit.point.ToString("F3") + " collider=" + hit.collider.name + " layer=" + hit.collider.gameObject.layer);
else sb.AppendLine("no ground raycast");
var terr = Object.FindFirstObjectByType<Terrain>();
if (terr != null) sb.AppendLine("terrain height at rig=" + terr.SampleHeight(rig.transform.position).ToString("F3") + " terrainY=" + terr.transform.position.y);
var lf = rig.Animator.GetBoneTransform(HumanBodyBones.LeftFoot);
sb.AppendLine("LeftFoot y=" + lf.position.y.ToString("F3") + " renderer bounds minY=" + smr.bounds.min.y.ToString("F3"));
return sb.ToString();
