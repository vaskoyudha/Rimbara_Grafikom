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
an.SetBool("Grounded", true);
var smr = rig.GetComponentsInChildren<SkinnedMeshRenderer>(true).FirstOrDefault(r => r.name.Contains("Sneaker"));
sb.AppendLine("sneaker renderer=" + (smr != null ? smr.name : "null"));
var mesh = new Mesh { name = "probe" };
foreach (float speed in new[] { 1f, 2f, 3f, 4f })
{
    an.SetFloat("Speed", speed); an.SetFloat("MotionSpeed", 1f);
    var lows = new List<float>();
    for (int i = 0; i < 20; i++)
    {
        an.SetFloat("Speed", speed); an.SetFloat("MotionSpeed", 1f);
        smr.BakeMesh(mesh, true);
        var verts = mesh.vertices;
        var m = smr.localToWorldMatrix;
        float minY = float.MaxValue;
        for (int v = 0; v < verts.Length; v++) { float wy = m.MultiplyPoint3x4(verts[v]).y; if (wy < minY) minY = wy; }
        lows.Add(minY);
        await Task.Delay(100);
    }
    sb.AppendLine("Speed=" + speed + " bakedMinVertexY min=" + lows.Min().ToString("0.0000") + " avg=" + lows.Average().ToString("0.0000") + " framesBelow0=" + lows.Count(y => y < 0f) + "/20");
}
return sb.ToString();
