using System.Linq;
using UnityEngine;
using Rimbara.Mission;
using Rimbara.Interaction;
var flow=Object.FindObjectsByType<GameFlow>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault();
if(flow.Mode!=ScreenMode.Playing){ flow.ShowMenu(); flow.Begin(); }
var ms=Object.FindObjectsByType<MissionSystem>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault();
if(ms.Stage==MissionStage.FirstSteps) ms.ReachInspectionMarker();
var actor=Object.FindObjectsByType<VolunteerActor>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault();
var cc=actor.GetComponent<CharacterController>();
void Warp(Vector3 p){ if(cc)cc.enabled=false; actor.transform.position=p; if(cc)cc.enabled=true; }
var seed=Object.FindObjectsByType<SeedlingPickup>(FindObjectsInactive.Include,FindObjectsSortMode.None).OrderBy(x=>x.Slot).First();
Warp(seed.transform.position+Vector3.up*0.4f);
var r1=seed.Interact(actor);
var bed=Object.FindObjectsByType<PlantingPoint>(FindObjectsInactive.Include,FindObjectsSortMode.None).OrderBy(x=>x.Slot).First();
Warp(bed.transform.position+Vector3.up*0.4f);
var r2=bed.Interact(actor);
Warp(new Vector3(4f,1.0f,-18f));
return $"stage={ms.Stage} seed={r1.Success} bed={r2.Success} planted={bed.Planted} tree={(bed.PlantedTree?bed.PlantedTree.name:"none")}";
