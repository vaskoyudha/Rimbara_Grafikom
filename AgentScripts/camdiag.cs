using System.Linq;
using System.Text;
using UnityEngine;
using Unity.Cinemachine;
var sb=new StringBuilder();
var brain=Object.FindFirstObjectByType<CinemachineBrain>();
sb.AppendLine("brain="+(brain!=null?brain.name+" active="+(brain.ActiveVirtualCamera!=null?brain.ActiveVirtualCamera.Name:"none"):"null"));
foreach(var c in Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
  sb.AppendLine($"cam {c.name} goActive={c.gameObject.activeInHierarchy} compEnabled={c.enabled} prio={c.Priority} pos={c.transform.position.ToString("F1")} follow={(c.Follow!=null?c.Follow.name:"null")}");
}
var vol=Object.FindObjectsByType<Rimbara.Player.VolunteerRig>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault();
if(vol!=null) sb.AppendLine("rig go="+vol.gameObject.name+" pos="+vol.transform.position.ToString("F1"));
return sb.ToString();
