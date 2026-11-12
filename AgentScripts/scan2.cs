using System.Text;
using UnityEngine;
var sb=new StringBuilder();
var best=new System.Collections.Generic.List<(float score,float x,float z,float min,float max,int props)>();
for(float cz=-120; cz<=120; cz+=10){
  for(float cx=-120; cx<=120; cx+=10){
    float min=999,max=-999; int bad=0, props=0, total=0; bool ok=true;
    for(float a=0;a<360;a+=22.5f) for(float rr=8; rr<=18; rr+=5){
      total++;
      float x=cx+Mathf.Cos(a*Mathf.Deg2Rad)*rr, z=cz+Mathf.Sin(a*Mathf.Deg2Rad)*rr;
      if(Physics.Raycast(new Vector3(x,300,z), Vector3.down, out var h, 800f)){
        var root=h.collider.transform.root.name;
        if(root=="Terrain"||root=="Ground"){ min=Mathf.Min(min,h.point.y); max=Mathf.Max(max,h.point.y); }
        else if(root=="Buildings"||root=="Fences"||root=="Environments"||root=="Fence_Wall_01"||root=="rest") ok=false;
        else props++;
      } else ok=false;
    }
    if(!ok) continue;
    if(max-min>2.0f) continue;
    best.Add((0f,cx,cz,min,max,props));
  }
}
best.Sort((a,b)=>a.min.CompareTo(b.min));
foreach(var b in best) sb.AppendLine($"({b.x:F0},{b.z:F0}) terrain {b.min:F2}..{b.max:F2} propHits={b.props}");
sb.AppendLine("count="+best.Count);
return sb.ToString();
