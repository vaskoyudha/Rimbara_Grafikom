using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

var sb = new StringBuilder();
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.name != "Arunika") throw new System.Exception("wrong scene " + scene.name);

// GroundLayers on the motor is World (bit 256), but the terrain sits on Default (bit 1),
// so GroundedCheck never found ground: no jump, no free-fall, no grounded animator state.
int world = LayerMask.NameToLayer("World");
foreach (var terrain in Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None))
{
    terrain.gameObject.layer = world;
    foreach (Transform child in terrain.transform) child.gameObject.layer = world;
    sb.AppendLine("terrain '" + terrain.name + "' -> layer " + world);
}

// Any other big walkable mesh on Default that the player can stand on.
foreach (var col in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
{
    if (col.gameObject.layer != 0) continue;
    if (col is TerrainCollider) { col.gameObject.layer = world; sb.AppendLine("terrain collider " + col.name); }
    else if (col.name.ToLower().Contains("ground")) { col.gameObject.layer = world; sb.AppendLine("ground object " + col.name); }
}

int count = 0;
foreach (var col in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None)) if (col.gameObject.layer == world) count++;
sb.AppendLine("world-layer colliders now: " + count);

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
return sb.ToString();
