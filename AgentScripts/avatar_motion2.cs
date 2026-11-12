using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Rimbara.Player;

var sb = new StringBuilder();
var rig = Object.FindFirstObjectByType<VolunteerRig>(FindObjectsInactive.Include);
if (rig == null) return "NO RIG";
var an = rig.Animator;
if (!Application.isPlaying) return "NOT PLAY";
Time.timeScale = 1f;
var controller = rig.OfficialController as MonoBehaviour;
if (controller != null) controller.enabled = false;
var lf = an.GetBoneTransform(HumanBodyBones.LeftFoot);
var rf = an.GetBoneTransform(HumanBodyBones.RightFoot);
an.SetBool("Grounded", true);

foreach (float speed in new[] { 1f, 2f, 4f })
{
    an.SetFloat("Speed", speed); an.SetFloat("MotionSpeed", 1f);
    var ly = new List<float>(); var ry = new List<float>();
    float minRend = 999f;
    for (int i = 0; i < 21; i++)
    {
        an.SetFloat("Speed", speed); an.SetFloat("MotionSpeed", 1f);
        ly.Add(rig.transform.InverseTransformPoint(lf.position).y);
        ry.Add(rig.transform.InverseTransformPoint(rf.position).y);
        foreach (var r in rig.GetComponentsInChildren<Renderer>(true)) if (r.enabled) minRend = Mathf.Min(minRend, r.bounds.min.y);
        await Task.Delay(100);
    }
    int Turns(List<float> v) { int t = 0; for (int i = 1; i < v.Count - 1; i++) if ((v[i] - v[i - 1]) * (v[i + 1] - v[i]) < 0) t++; return t; }
    sb.AppendLine("Speed=" + speed + " L amp=" + (ly.Max() - ly.Min()).ToString("0.0000") + " R amp=" + (ry.Max() - ry.Min()).ToString("0.0000")
        + " L turns=" + Turns(ly) + " R turns=" + Turns(ry) + " minRendererY=" + minRend.ToString("0.0000"));
}
return sb.ToString();
