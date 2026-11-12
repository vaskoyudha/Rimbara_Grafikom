using System.Linq;
using System.Text;
using UnityEngine;
using Rimbara.Interaction;
using Rimbara.Mission;

var sb = new StringBuilder();

float GroundY(float x, float z) {
    var hits = Physics.RaycastAll(new Vector3(x, 100f, z), Vector3.down, 300f)
        .Where(h => {
            var root = h.collider.transform.root.name;
            return (root == "Terrain" || root == "Ground" || root == "Flats") && h.point.y > -1.5f
                || h.collider.name.StartsWith("EA03_Environment_Road")
                || h.collider.name.Contains("Sand") || h.collider.name.Contains("Mud");
        })
        .OrderBy(h => h.point.y)
        .ToArray();
    if (hits.Length > 0) return hits[0].point.y;
    return Physics.Raycast(new Vector3(x, 100f, z), Vector3.down, out var any, 300f) ? any.point.y : 0f;
}
void Settle(Transform t, float lift) {
    var p = t.position;
    t.position = new Vector3(p.x, GroundY(p.x, p.z) + lift, p.z);
}

foreach (var s in Object.FindObjectsByType<SeedlingPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
    Settle(s.transform, 0.05f);
foreach (var b in Object.FindObjectsByType<PlantingPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
    Settle(b.transform, 0.02f);
foreach (var f in Object.FindObjectsByType<FirePoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
    Settle(f.transform, 0.02f);

foreach (var name in new[] { "Inspection marker", "Volunteer post return zone", "Volunteer mission board", "Planting zone destination", "Fire zone destination" }) {
    var go = GameObject.Find(name);
    if (go != null) { Settle(go.transform, 0.05f); sb.AppendLine("settled " + name + " -> " + go.transform.position); }
}

var v = Object.FindObjectsByType<VolunteerActor>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
if (v != null) {
    var p = v.transform.position;
    v.transform.position = new Vector3(p.x, GroundY(p.x, p.z) + 0.1f, p.z);
    sb.AppendLine("volunteer -> " + v.transform.position);
}

foreach (var s in Object.FindObjectsByType<SeedlingPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(x => x.Slot))
    sb.AppendLine($"seed {s.Slot} {s.transform.position}");
foreach (var f in Object.FindObjectsByType<FirePoint>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(x => x.Slot))
    sb.AppendLine($"fire {f.Slot} {f.transform.position}");

return sb.ToString();
