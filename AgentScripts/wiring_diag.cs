using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Rimbara.Mission;
using Rimbara.Interaction;
var sb=new StringBuilder();
var ms=Object.FindObjectsByType<MissionSystem>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault();
foreach(var f in ms.GetType().GetFields(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public)){
  if(typeof(UnityEngine.Object).IsAssignableFrom(f.FieldType)){ var v=f.GetValue(ms) as UnityEngine.Object; sb.AppendLine("MS "+f.Name+" = "+(v?v.name:"NULL")); }
}
var wg=Object.FindObjectsByType<WorldGuide>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault();
foreach(var f in wg.GetType().GetFields(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public)){
  var v=f.GetValue(wg);
  if(v is System.Array arr){ var nulls=arr.Cast<object>().Count(o=>o==null||(o is UnityEngine.Object u && u==null)); sb.AppendLine("WG "+f.Name+" len="+arr.Length+" nulls="+nulls); }
  else if(v is UnityEngine.Object u) sb.AppendLine("WG "+f.Name+" = "+(u?u.name:"NULL"));
}
void DumpChildren(string n){ var go=GameObject.Find(n); if(go==null){sb.AppendLine(n+" missing");return;} sb.AppendLine(n+" children:"); foreach(Transform c in go.transform) sb.AppendLine("   "+c.name+" active="+c.gameObject.activeSelf); }
DumpChildren("Native seedling 01"); DumpChildren("Planting bed 01"); DumpChildren("Remaining fire 01");
return sb.ToString();
