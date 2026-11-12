using System;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Rimbara.Interaction;
using Rimbara.Mission;

var sb = new StringBuilder();
var scene = SceneManager.GetActiveScene();
sb.AppendLine("SCENE " + scene.name + " " + scene.path);

var terrain = Terrain.activeTerrain;
if (terrain != null) {
    var td = terrain.terrainData;
    var p = terrain.transform.position;
    sb.AppendLine($"TERRAIN pos=({p.x:F1},{p.y:F1},{p.z:F1}) size=({td.size.x:F1},{td.size.y:F1},{td.size.z:F1})");
}

sb.AppendLine("ROOTS:");
foreach (var go in scene.GetRootGameObjects()) sb.AppendLine("  " + go.name + " active=" + go.activeSelf);

void Dump<T>(string label) where T : Component {
    var all = UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    sb.AppendLine(label + " count=" + all.Length);
    foreach (var c in all) {
        var t = c.transform;
        sb.AppendLine($"  {label} {c.name} pos=({t.position.x:F2},{t.position.y:F2},{t.position.z:F2}) active={c.gameObject.activeInHierarchy}");
    }
}
Dump<SeedlingPickup>("SEED");
Dump<PlantingPoint>("BED");
Dump<FirePoint>("FIRE");
Dump<MissionZone>("ZONE");
Dump<MissionBoard>("BOARD");
Dump<VolunteerActor>("VOLUNTEER");

return sb.ToString();
