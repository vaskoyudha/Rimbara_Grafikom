using System.Linq;
using System.Text;
using UnityEngine;
var sb=new StringBuilder();
var cam=Camera.main;
sb.AppendLine("mainCam="+(cam!=null?cam.name+" pos="+cam.transform.position+" rot="+cam.transform.eulerAngles.ToString("F0"):"null"));
foreach(var c in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include,FindObjectsSortMode.None))
  sb.AppendLine(" cam "+c.name+" enabled="+c.enabled+" depth="+c.depth+" pos="+c.transform.position);
var root=GameObject.Find("Restoration Ground");
if(root!=null){
  var rs=root.GetComponentsInChildren<Renderer>(true);
  var b=new Bounds(root.transform.position,Vector3.zero); bool first=true;
  foreach(var r in rs){ if(first){b=r.bounds;first=false;} else b.Encapsulate(r.bounds); }
  sb.AppendLine("gladeRoot="+root.transform.position+" children="+root.transform.childCount+" renderers="+rs.Length+" bounds="+b.center+" size="+b.size);
  foreach(Transform c in root.transform) if(c.name.Contains("ground")||c.name.Contains("soil")) sb.AppendLine("  "+c.name+" pos="+c.position+" scale="+c.localScale);
} else sb.AppendLine("NO GLADE ROOT");
var vol=Object.FindObjectsByType<Rimbara.Interaction.VolunteerActor>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault();
if(vol!=null) sb.AppendLine("volunteer="+vol.transform.position);
return sb.ToString();
