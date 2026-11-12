using System.Text;
using UnityEngine;
var sb=new StringBuilder();
var pts = new (float,float)[]{(-2,-30),(6,-30),(-2,-24),(6,-24),(2,-27),(-4,-34),(8,-34),(-4,-20),(8,-20),(2,-18),(-6,-12),(8,-12),(8,-40),(10,-41),(4,-36),(6,-39),(-8,-8),(10,-6),(0,-38)};
foreach(var (x,z) in pts){
  if(Physics.Raycast(new Vector3(x,200,z), Vector3.down, out var h, 500f))
    sb.AppendLine($"({x},{z}) y={h.point.y:F2} hit={h.collider.name} root={h.collider.transform.root.name}");
  else sb.AppendLine($"({x},{z}) NOHIT");
}
return sb.ToString();
