using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Rimbara.Player;
using Rimbara.Interaction;

var sb = new StringBuilder();
var rig = Object.FindFirstObjectByType<VolunteerRig>(FindObjectsInactive.Include);
sb.AppendLine("rig=" + rig.name + " pos=" + rig.transform.position);
var an = rig.Animator;
sb.AppendLine("anim ctrl=" + an.runtimeAnimatorController.name + " layers=" + an.layerCount);
foreach (var p in an.parameters) sb.AppendLine("  param " + p.name + " type=" + p.type + " default=" + p.defaultFloat);
var ep = rig.GetComponent<EquipmentPose>();
sb.AppendLine("EquipmentPose=" + (ep != null) + " enabled=" + (ep != null && ep.enabled));
var sp = rig.GetComponent<WaterSprayer>();
sb.AppendLine("WaterSprayer=" + (sp != null) + " spraying=" + (sp != null && sp.Spraying));
foreach (var f in typeof(EquipmentPose).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
{
    var v = f.GetValue(ep) as Object;
    sb.AppendLine("  EP." + f.Name + " => " + (v == null ? "NULL" : v.name + " @" + (v is Component c ? c.transform.position.ToString() : "")));
    if (v is Component cc) { var chain = new List<string>(); var t = cc.transform; while (t != null && t != rig.transform) { chain.Add(t.name); t = t.parent; } sb.AppendLine("     chain=" + string.Join(" < ", chain)); }
}
foreach (var f in typeof(WaterSprayer).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
{
    var v = f.GetValue(sp) as Object;
    sb.AppendLine("  WS." + f.Name + " => " + (v == null ? "NULL" : v.name));
}
var hand = an.GetBoneTransform(HumanBodyBones.RightHand);
sb.AppendLine("RightHand=" + (hand != null ? hand.position.ToString() : "null"));
var nozzle = rig.transform.Find("Water nozzle");
sb.AppendLine("Water nozzle=" + (nozzle != null ? nozzle.position + " dist=" + Vector3.Distance(nozzle.position, hand.position).ToString("0.000") : "null"));
var grip = rig.transform.Find("Nozzle hand grip");
sb.AppendLine("Nozzle hand grip=" + (grip != null ? grip.position + " dist=" + Vector3.Distance(grip.position, hand.position).ToString("0.000") : "null"));
var feet = new[] { HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot, HumanBodyBones.LeftToes, HumanBodyBones.RightToes };
foreach (var b in feet) { var t = an.GetBoneTransform(b); sb.AppendLine("bone " + b + "=" + (t != null ? t.position.ToString() : "null")); }
var rends = rig.GetComponentsInChildren<Renderer>(true);
float minY = 9999f;
foreach (var r in rends) if (r.enabled) minY = Mathf.Min(minY, r.bounds.min.y);
sb.AppendLine("lowest renderer Y=" + minY.ToString("0.000"));
sb.AppendLine("animator enabled=" + an.enabled + " culling=" + an.cullingMode);
return sb.ToString();
