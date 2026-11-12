using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Rimbara.Mission;
using Rimbara.Interaction;

var sb = new StringBuilder();
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.name != "Arunika") throw new System.Exception("open Arunika first, got " + scene.name);

var state = Object.FindFirstObjectByType<GameState>();
var ms = Object.FindFirstObjectByType<MissionSystem>();
var wg = Object.FindFirstObjectByType<WorldGuide>();
if (state == null || ms == null || wg == null) throw new System.Exception("missing game systems");

Transform FindT(string n)
{
    var go = GameObject.Find(n);
    if (go != null) return go.transform;
    foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        if (t.name == n) return t;
    return null;
}

Transform Marker(string name, Vector3 fallback)
{
    var t = FindT(name);
    if (t == null)
    {
        var go = new GameObject(name);
        go.transform.position = fallback;
        t = go.transform;
        sb.AppendLine("created " + name);
    }
    return t;
}

var inspection = FindT("Inspection marker");
var post = FindT("Volunteer post return zone");
var planting = Marker("Planting zone destination", new Vector3(12f, 0.1f, 3f));
var fire = Marker("Fire zone destination", new Vector3(-2f, 0.1f, 16f));
if (inspection == null || post == null) throw new System.Exception("markers missing");

ms.Configure(state, inspection, post, planting, fire);

var seeds = Object.FindObjectsByType<SeedlingPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(x => x.Slot).ToArray();
var beds = Object.FindObjectsByType<PlantingPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(x => x.Slot).ToArray();
var fires = Object.FindObjectsByType<FirePoint>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(x => x.Slot).ToArray();

GameObject Beacon(Transform parent)
{
    foreach (var t in parent.GetComponentsInChildren<Transform>(true))
        if (t.name.StartsWith("Beacon")) return t.gameObject;
    return null;
}

var seedBeacons = seeds.Select(s => Beacon(s.transform)).ToArray();
var bedBeacons = beds.Select(b => Beacon(b.transform)).ToArray();
var fireBeacons = fires.Select(f => Beacon(f.transform)).ToArray();
var inspectionBeacon = Beacon(inspection);
var returnBeacon = Beacon(post);

var volunteer = Object.FindFirstObjectByType<VolunteerActor>();
wg.Configure(state, ms, volunteer.transform, seeds, beds, fires, inspectionBeacon, returnBeacon, seedBeacons, bedBeacons);
wg.ConfigureFireMarkers(fireBeacons);

sb.AppendLine($"wired seeds={seeds.Length} beds={beds.Length} fires={fires.Length}");
sb.AppendLine($"beacons seed={seedBeacons.Count(x => x != null)} bed={bedBeacons.Count(x => x != null)} fire={fireBeacons.Count(x => x != null)} insp={(inspectionBeacon != null)} post={(returnBeacon != null)}");

// HUD reference check.
var hud = Object.FindFirstObjectByType<Rimbara.UI.RimbaraHud>();
if (hud != null)
{
    var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
    foreach (var f in hud.GetType().GetFields(flags))
        if (typeof(Object).IsAssignableFrom(f.FieldType))
        {
            var v = f.GetValue(hud) as Object;
            if (v == null) sb.AppendLine("HUD NULL " + f.Name);
        }
    sb.AppendLine("HUD checked");
}

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
sb.AppendLine("SAVED");
return sb.ToString();
