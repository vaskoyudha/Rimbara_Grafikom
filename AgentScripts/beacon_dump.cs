using System.Text;
using UnityEngine;
var sb=new StringBuilder();
foreach(var n in new[]{"Inspection marker","Volunteer post return zone","Volunteer mission board"}){
  var go=GameObject.Find(n);
  sb.AppendLine(n+" -> "+(go?go.transform.position.ToString("F2"):"missing"));
  if(go!=null) foreach(Transform c in go.transform) sb.AppendLine("   "+c.name+" active="+c.gameObject.activeSelf+" pos="+c.position.ToString("F2"));
}
return sb.ToString();
