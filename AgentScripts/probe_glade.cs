using System.Text;
using UnityEngine;
var c=new Vector2(14,34); float r=20f; var sb=new StringBuilder();
float min=999,max=-999; int nonTerrain=0; var names=new System.Collections.Generic.List<string>();
for(float a=0;a<360;a+=20) for(float rr=0;rr<=r;rr+=5){
  float x=c.x+Mathf.Cos(a*Mathf.Deg2Rad)*rr, z=c.y+Mathf.Sin(a*Mathf.Deg2Rad)*rr;
  if(Physics.Raycast(new Vector3(x,200,z), Vector3.down, out var h, 600f)){
    min=Mathf.Min(min,h.point.y); max=Mathf.Max(max,h.point.y);
    var root=h.collider.transform.root.name;
    if(root!="Terrain"&&root!="Ground"&&root!="Flats"){ nonTerrain++; if(names.Count<8) names.Add(root+":"+h.collider.name); }
  }
}
sb.AppendLine($"center={c} min={min:F2} max={max:F2} nonTerrainHits={nonTerrain}");
foreach(var n in names) sb.AppendLine("  "+n);
return sb.ToString();
