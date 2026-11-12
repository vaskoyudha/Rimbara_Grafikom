using System.Text;
using UnityEngine;
var sb=new StringBuilder();
for(float z=-42; z<=14; z+=7){
  for(float x=-28; x<=20; x+=8){
    if(Physics.Raycast(new Vector3(x,200f,z), Vector3.down, out var hit, 400f)){
      sb.AppendLine($"({x:F0},{z:F0}) y={hit.point.y:F2} hit={hit.collider.name} go={hit.collider.gameObject.transform.root.name}");
    } else sb.AppendLine($"({x:F0},{z:F0}) NO HIT");
  }
}
return sb.ToString();
