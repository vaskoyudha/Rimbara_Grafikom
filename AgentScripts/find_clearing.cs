using System.Text;
using UnityEngine;
var sb=new StringBuilder();
var cands=new (float,float)[]{(0,60),(0,90),(-40,60),(-60,20),(40,-10),(40,40),(-30,-60),(30,70),(60,60),(0,-60),(-40,-60),(60,0),(-70,0)};
foreach(var c in cands){
  float min=999,max=-999; int bad=0;
  for(float a=0;a<360;a+=30) for(float rr=0;rr<=20;rr+=5){
    float x=c.Item1+Mathf.Cos(a*Mathf.Deg2Rad)*rr, z=c.Item2+Mathf.Sin(a*Mathf.Deg2Rad)*rr;
    if(Physics.Raycast(new Vector3(x,300,z), Vector3.down, out var h, 800f)){
      min=Mathf.Min(min,h.point.y); max=Mathf.Max(max,h.point.y);
      var root=h.collider.transform.root.name;
      bool ground = root=="Terrain"||root=="Ground"||root=="Flats"||root.StartsWith("rest")||root.StartsWith("Rocks")||root.StartsWith("Clifs")||root.StartsWith("treese")||root.StartsWith("Props")||root.StartsWith("Bushes");
      bool solid = root.StartsWith("Build")||root.StartsWith("Fence")||root.StartsWith("Enviro")||root.StartsWith("Village");
      if(!ground && !solid) bad++;
      if(solid) bad+=3;
    } else bad+=5;
  }
  sb.AppendLine($"({c.Item1:F0},{c.Item2:F0}) height {min:F1}..{max:F1} bad={bad}");
}
return sb.ToString();
