using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Rimbara.Interaction;
using Rimbara.Player;
var sb=new StringBuilder();
foreach(var b in Object.FindObjectsByType<PlantingPoint>(FindObjectsInactive.Include,FindObjectsSortMode.None).OrderBy(x=>x.Slot)){
  var f=b.GetType().GetField("treePrefab",BindingFlags.Instance|BindingFlags.NonPublic);
  var go=f.GetValue(b) as GameObject;
  sb.AppendLine($"bed{b.Slot} tree={(go?go.name:"NULL")} planted={b.Planted} child={(b.PlantedTree?b.PlantedTree.name:"none")}");
}
var actor=Object.FindObjectsByType<VolunteerActor>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault();
var rig=actor.GetComponent<VolunteerRig>();
if(rig!=null){ rig.SetCameraOrbit(0f,12f); }
var cc=actor.GetComponent<CharacterController>(); if(cc)cc.enabled=false;
actor.transform.position=new Vector3(6f,1.0f,-24f); actor.transform.rotation=Quaternion.Euler(0f,0f,0f);
if(cc)cc.enabled=true;
return sb.ToString();
