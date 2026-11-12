using System.Text;
using UnityEngine;
var sb=new StringBuilder();
var best=new System.Collections.Generic.List<(float score,float x,float z,float min,float max)>();
for(float cz=-100; cz<=100; cz+=10){
  for(float cx=-100; cx<=100; cx+=10){
    float min=999,max=-999; int bad=0, terrainOnly=0, total=0;
    bool ok=true;
    for(float a=0;a<360;a+=22.5f){
      for(float rr=8; rr<=18; rr+=5){
        float x=cx+Mathf.Cos(a*Mathf.Deg2Rad)*rr, z=cz+Mathf.Sin(a*Mathf.Deg2Rad)*rr;
        total++;
        if(Physics.Raycast(new Vector3(x,300,z), Vector3.down, out var h, 800f)){
          var root=h.collider.transform.root.name;
          if(root=="Terrain"||root=="Ground"||root=="Flats"){ terrainOnly++; min=Mathf.Min(min,h.point.y); max=Mathf.Max(max,h.point.y); }
          else bad++;
        } else { bad+=2; }
      }
    }
    if(bad>0 || terrainOnly<total) continue;
    if(max-min>1.2f) continue;
    best.Add((max-min,cx,cz,min,max));
  }
}
best.Sort((a,b)=>a.score.CompareTo(b.score));
foreach(var b in best) sb.AppendLine($"({b.x:F0},{b.z:F0}) flat {b.min:F2}..{b.max:F2} var={b.score:F2}");
sb.AppendLine("count="+best.Count);
return sb.ToString();
