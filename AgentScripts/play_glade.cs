using System.Linq;
using UnityEngine;
using Rimbara.Mission;
using Rimbara.Interaction;
var flow=Object.FindObjectsByType<GameFlow>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault();
if(flow!=null && flow.Mode!=ScreenMode.Playing) flow.Begin();
var actor=Object.FindObjectsByType<VolunteerActor>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault();
if(actor!=null){ var cc=actor.GetComponent<CharacterController>(); if(cc)cc.enabled=false; actor.transform.position=new Vector3(20f,1.2f,-67f); actor.transform.rotation=Quaternion.Euler(0f,0f,0f); if(cc)cc.enabled=true; }
return "mode="+(flow!=null?flow.Mode.ToString():"null")+" actor="+(actor!=null?actor.transform.position.ToString():"null");
