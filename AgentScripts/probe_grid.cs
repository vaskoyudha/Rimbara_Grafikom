using System.Text;
using UnityEngine;
var sb=new StringBuilder();
for(float z=-44; z<=-16; z+=2){
  var line=new StringBuilder($"z={z:F0}: ");
  for(float x=-10; x<=12; x+=2){
    bool ok=false;
    if(Physics.Raycast(new Vector3(x,200,z), Vector3.down, out var h, 500f)){
      string root=h.collider.transform.root.name;
      ok=(root=="Terrain"||root=="Ground"||root=="Flats") && h.point.y>-0.6f && h.point.y<1.5f;
      if(!ok) ok = h.collider.name.StartsWith("EA03_Environment_Road") || h.collider.name.Contains("Sand") || h.collider.name.Contains("Mud") || h.collider.name.StartsWith("EA03_Environment_RoadIsland");
    }
    line.Append(ok?".":"#");
  }
  sb.AppendLine(line.ToString());
}
return sb.ToString();
