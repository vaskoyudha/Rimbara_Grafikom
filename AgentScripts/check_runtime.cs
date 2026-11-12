using System;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Rimbara.Mission;
using Rimbara.Interaction;
var sb=new StringBuilder();
sb.AppendLine("play="+Application.isPlaying+" scene="+UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
var ms=UnityEngine.Object.FindObjectsByType<MissionSystem>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault();
var gf=UnityEngine.Object.FindObjectsByType<GameFlow>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault();
sb.AppendLine("MissionSystem="+(ms!=null)+" GameFlow="+(gf!=null));
if(ms!=null){
  foreach(var f in ms.GetType().GetFields(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public)){
    if(typeof(UnityEngine.Object).IsAssignableFrom(f.FieldType)){
      var v=f.GetValue(ms) as UnityEngine.Object;
      sb.AppendLine($"  ref {f.Name} => {(v==null?"NULL":v.name)}");
    }
  }
}
foreach(var t in new Type[]{typeof(VolunteerActor)}){
  var list=UnityEngine.Object.FindObjectsByType(t,FindObjectsInactive.Include,FindObjectsSortMode.None);
  sb.AppendLine(t.Name+" count="+list.Length);
  foreach(var o in list){ var c=o as Component; sb.AppendLine("  at "+c.transform.position+" active="+c.gameObject.activeInHierarchy); }
}
foreach(var s in UnityEngine.Object.FindObjectsByType<SeedlingPickup>(FindObjectsInactive.Include,FindObjectsSortMode.None).OrderBy(x=>x.Slot)){
  sb.AppendLine($"seed {s.Slot} pos={s.transform.position} active={s.gameObject.activeInHierarchy} collected={s.Collected}");
}
foreach(var b in UnityEngine.Object.FindObjectsByType<PlantingPoint>(FindObjectsInactive.Include,FindObjectsSortMode.None).OrderBy(x=>x.Slot)){
  sb.AppendLine($"bed {b.Slot} pos={b.transform.position} active={b.gameObject.activeInHierarchy} planted={b.Planted}");
}
foreach(var f in UnityEngine.Object.FindObjectsByType<FirePoint>(FindObjectsInactive.Include,FindObjectsSortMode.None).OrderBy(x=>x.Slot)){
  sb.AppendLine($"fire {f.Slot} pos={f.transform.position} active={f.gameObject.activeInHierarchy} ext={f.Extinguished}");
}
return sb.ToString();
