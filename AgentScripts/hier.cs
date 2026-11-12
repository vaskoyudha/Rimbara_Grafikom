using System.Text;
using UnityEngine;
using Rimbara.Interaction;
var sb=new StringBuilder();
foreach(var s in Object.FindObjectsByType<SeedlingPickup>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
  var chain=s.transform; var p="";
  while(chain!=null){p=chain.name+"/"+p; chain=chain.parent;}
  sb.AppendLine("SEED "+p);
}
foreach(var b in Object.FindObjectsByType<PlantingPoint>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
  var chain=b.transform; var p="";
  while(chain!=null){p=chain.name+"/"+p; chain=chain.parent;}
  sb.AppendLine("BED "+p);
}
foreach(var f in Object.FindObjectsByType<FirePoint>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
  var chain=f.transform; var p="";
  while(chain!=null){p=chain.name+"/"+p; chain=chain.parent;}
  sb.AppendLine("FIRE "+p);
}
return sb.ToString();
