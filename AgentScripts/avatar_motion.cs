using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Rimbara.Player;
using Rimbara.Interaction;

var sb = new StringBuilder();
var rig = Object.FindFirstObjectByType<VolunteerRig>(FindObjectsInactive.Include);
if (rig == null) return "NO RIG";
var an = rig.Animator;
if (an == null) return "NO ANIMATOR";
if (!Application.isPlaying) return "NOT IN PLAY MODE";

var speedParam = an.parameters.FirstOrDefault(p => p.name == "Speed");
var motionParam = an.parameters.FirstOrDefault(p => p.name == "MotionSpeed");
sb.AppendLine("Speed param=" + (speedParam.name ?? "null") + " MotionSpeed param=" + (motionParam.name ?? "null"));

var controller = rig.OfficialController as MonoBehaviour;
if (controller != null) controller.enabled = false;
an.SetFloat("Speed", 1f);
an.SetFloat("MotionSpeed", 1f);
an.SetBool("Grounded", true);
an.SetBool("Jump", false);
an.SetBool("FreeFall", false);

Time.timeScale = 1f;
Transform lf = an.GetBoneTransform(HumanBodyBones.LeftFoot);
Transform rf = an.GetBoneTransform(HumanBodyBones.RightFoot);
Transform lt = an.GetBoneTransform(HumanBodyBones.LeftToes);
Transform rt = an.GetBoneTransform(HumanBodyBones.RightToes);
Vector3 basePos = rig.transform.position;

var lfy = new List<float>(); var rfy = new List<float>();
var lfl = new List<float>(); var rfl = new List<float>();
float minSole = 999f;
sb.AppendLine("i  LeftFootY  RightFootY  LeftFootLocalY  RightFootLocalY  LeftToeY  RightToeY  minRendererY");
for (int i = 0; i < 11; i++)
{
    an.SetFloat("Speed", 1f);
    an.SetFloat("MotionSpeed", 1f);
    float minR = 999f;
    foreach (var r in rig.GetComponentsInChildren<Renderer>(true)) if (r.enabled) minR = Mathf.Min(minR, r.bounds.min.y);
    minSole = Mathf.Min(minSole, minR);
    float lly = rig.transform.InverseTransformPoint(lf.position).y;
    float rly = rig.transform.InverseTransformPoint(rf.position).y;
    lfy.Add(lf.position.y); rfy.Add(rf.position.y); lfl.Add(lly); rfl.Add(rly);
    sb.AppendLine(i.ToString().PadLeft(2) + " " + lf.position.y.ToString("0.0000").PadLeft(9) + " " + rf.position.y.ToString("0.0000").PadLeft(10) + " " + lly.ToString("0.0000").PadLeft(14) + " " + rly.ToString("0.0000").PadLeft(15) + " " + lt.position.y.ToString("0.0000").PadLeft(9) + " " + rt.position.y.ToString("0.0000").PadLeft(10) + " " + minR.ToString("0.0000").PadLeft(12));
    await Task.Delay(200);
}
float Amin(List<float> v) => v.Max() - v.Min();
sb.AppendLine("LEFT foot amplitude=" + Amin(lfy).ToString("0.0000") + " local=" + Amin(lfl).ToString("0.0000"));
sb.AppendLine("RIGHT foot amplitude=" + Amin(rfy).ToString("0.0000") + " local=" + Amin(rfl).ToString("0.0000"));
sb.AppendLine("min renderer bounds Y over samples=" + minSole.ToString("0.0000") + " rigY=" + rig.transform.position.y.ToString("0.0000"));

var ep = rig.GetComponent<EquipmentPose>();
var sp = rig.GetComponent<WaterSprayer>();
if (ep != null && sp != null)
{
    var sprayFlag = typeof(WaterSprayer).GetField("<Spraying>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
    sp.enabled = false;
    sprayFlag.SetValue(sp, true);
    await Task.Delay(300);
    var hand = an.GetBoneTransform(HumanBodyBones.RightHand);
    var grip = (Transform)typeof(EquipmentPose).GetField("handGrip", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ep);
    var nozzle = rig.transform.Find("Water nozzle");
    sb.AppendLine("EP active=" + ep.enabled + " spraying(flag)=" + sp.Spraying);
    sb.AppendLine("RightHand=" + hand.position.ToString("F3") + " grip=" + (grip != null ? grip.position.ToString("F3") : "null"));
    sb.AppendLine("nozzle=" + (nozzle != null ? nozzle.position.ToString("F3") : "null") + " dist(nozzle,hand)=" + (nozzle != null ? Vector3.Distance(nozzle.position, hand.position).ToString("0.0000") : "n/a"));
    if (grip != null) sb.AppendLine("dist(grip,hand)=" + Vector3.Distance(grip.position, hand.position).ToString("0.0000"));
    sprayFlag.SetValue(sp, false);
    sp.enabled = true;
}
sb.AppendLine("LeftToes range=" + (lt.position.y).ToString("0.000") );
return sb.ToString();
