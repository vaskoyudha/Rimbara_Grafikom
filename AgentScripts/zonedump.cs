using System.Text;
using UnityEngine;
var sb=new StringBuilder();
foreach(var zname in new[]{"Zone A - volunteer post and damaged forest","Zone B - seedling nursery and planting clearings","Zone C - remaining fires"}){
  var z=GameObject.Find(zname);
  sb.AppendLine("== "+zname+" @ "+(z?z.transform.position.ToString():"null"));
  if(z!=null) foreach(Transform c in z.transform) sb.AppendLine("   "+c.name+" @ "+c.position.ToString("F2")+" active="+c.gameObject.activeSelf);
}
return sb.ToString();
