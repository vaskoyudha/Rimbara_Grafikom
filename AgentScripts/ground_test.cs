using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Rimbara.Player;

var sb = new StringBuilder();
var rig = Object.FindFirstObjectByType<VolunteerRig>(FindObjectsInactive.Include);
var an = rig.Animator;
Time.timeScale = 1f;
var ctrl = rig.OfficialController as MonoBehaviour; if (ctrl != null) ctrl.enabled = false;
var lf = an.GetBoneTransform(HumanBodyBones.LeftFoot);
var rf = an.GetBoneTransform(HumanBodyBones.RightFoot);
var lt = an.GetBoneTransform(HumanBodyBones.LeftToes);
var rt = an.GetBoneTransform(HumanBodyBones.RightToes);
var sneaker = rig.GetComponentsInChildren<Renderer>(true).FirstOrDefault(r => r.name.Contains("Sneaker"));
sb.AppendLine("sneaker=" + (sneaker != null ? sneaker.name : "null") + " bind=" + (sneaker != null ? sneaker.bounds.ToString() : ""));
an.SetBool("Grounded", true);
foreach (float speed in new[] { 0f, 2f, 3f })
{
    an.SetFloat("Speed", speed); an.SetFloat("MotionSpeed", 1f);
    var mins = new List<float>(); var toes = new List<float>(); var feet = new List<float>(); var rootYs = new List<float>();
    for (int i = 0; i < 31; i++)
    {
        an.SetFloat("Speed", speed); an.SetFloat("MotionSpeed", 1f);
        mins.Add(sneaker != null && sneaker.enabled ? sneaker.bounds.min.y : 999f);
        toes.Add(Mathf.Min(lt.position.y, rt.position.y));
        feet.Add(Mathf.Min(lf.position.y, rf.position.y));
        rootYs.Add(rig.transform.position.y);
        await Task.Delay(100);
    }
    sb.AppendLine("Speed=" + speed + " sneakerMinY min=" + mins.Min().ToString("0.0000") + " avg=" + mins.Average().ToString("0.0000") + " max=" + mins.Max().ToString("0.0000")
        + " | toeBoneMinY=" + toes.Min().ToString("0.0000") + " footBoneMinY=" + feet.Min().ToString("0.0000") + " | rigY=" + rootYs.Min().ToString("0.0000") + ".." + rootYs.Max().ToString("0.0000"));
}
return sb.ToString();
