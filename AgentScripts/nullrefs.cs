using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
var sb=new StringBuilder();
int count=0;
foreach(var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
  var t=mb.GetType();
  if(!t.Namespace.StartsWith("Rimbara")) continue;
  foreach(var f in t.GetFields(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public)){
    if(!typeof(UnityEngine.Object).IsAssignableFrom(f.FieldType)) continue;
    if(f.GetValue(mb)==null){ sb.AppendLine(t.Name+"."+f.Name+" NULL on "+mb.gameObject.name); count++; }
  }
  if(count>60) break;
}
sb.AppendLine("total nulls listed="+count);
return sb.ToString();
