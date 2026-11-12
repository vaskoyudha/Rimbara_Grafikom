using System.Text;
using UnityEngine;
var sb=new StringBuilder();
for(float z=-90; z<=60; z+=15){
  var line=new StringBuilder($"z={z:F0}: ");
  for(float x=-90; x<=60; x+=15){
    if(Physics.Raycast(new Vector3(x,200,z), Vector3.down, out var h, 600f)){
      string root=h.collider.transform.root.name;
      line.Append($"{h.point.y:F0}/{root.Substring(0,System.Math.Min(6,root.Length))} ");
    } else line.Append("-- ");
  }
  sb.AppendLine(line.ToString());
}
return sb.ToString();
