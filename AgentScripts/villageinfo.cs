using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rimbara.Interaction;
using Rimbara.Mission;
var sb=new StringBuilder();
var sc=SceneManager.GetActiveScene();
sb.AppendLine("scene="+sc.name);
var ter=Terrain.activeTerrain;
if(ter!=null){var td=ter.terrainData;var p=ter.transform.position;sb.AppendLine($"TERRAIN pos=({p.x:F1},{p.y:F1},{p.z:F1}) size=({td.size.x:F1},{td.size.y:F1},{td.size.z:F1})");}
else sb.AppendLine("NO ACTIVE TERRAIN");
foreach(var go in sc.GetRootGameObjects()) sb.AppendLine("ROOT "+go.name+" active="+go.activeSelf+" children="+go.transform.childCount);
var seeds=Object.FindObjectsByType<SeedlingPickup>(FindObjectsInactive.Include,FindObjectsSortMode.None);
var beds=Object.FindObjectsByType<PlantingPoint>(FindObjectsInactive.Include,FindObjectsSortMode.None);
var fires=Object.FindObjectsByType<FirePoint>(FindObjectsInactive.Include,FindObjectsSortMode.None);
sb.AppendLine($"seed={seeds.Length} bed={beds.Length} fire={fires.Length}");
return sb.ToString();
