using System.Linq;
using UnityEngine;
using Rimbara.Mission;
var flow=Object.FindObjectsByType<GameFlow>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault();
if(flow!=null){ if(flow.Mode==ScreenMode.Paused) flow.Resume(); else if(flow.Mode!=ScreenMode.Playing){ flow.ShowMenu(); flow.Begin(); } }
var cam=Camera.main;
return "mode="+(flow!=null?flow.Mode.ToString():"null")+" cam="+(cam!=null?cam.transform.position.ToString("F2"):"null");
