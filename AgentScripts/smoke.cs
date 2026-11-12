using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Rimbara.Mission;
using Rimbara.Interaction;

var sb = new StringBuilder();
var flow = UnityEngine.Object.FindObjectsByType<GameFlow>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
var ms = UnityEngine.Object.FindObjectsByType<MissionSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
var actor = UnityEngine.Object.FindObjectsByType<VolunteerActor>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
if (flow == null || ms == null || actor == null) return "MISSING flow=" + (flow!=null) + " ms=" + (ms!=null) + " actor=" + (actor!=null);

var cc = actor.GetComponent<CharacterController>();
void Warp(Vector3 p) {
    if (cc != null) cc.enabled = false;
    actor.transform.position = p;
    if (cc != null) cc.enabled = true;
}
async Task Tick(int msDelay) { await Task.Delay(msDelay); }

flow.Begin(); await Tick(300);
sb.AppendLine("after Begin stage=" + ms.Stage + " running=" + ms.State.SessionRunning);

var insp = GameObject.Find("Inspection marker");
Warp(insp.transform.position + Vector3.up * 0.5f); await Tick(600);
sb.AppendLine("at inspection stage=" + ms.Stage);

foreach (var s in UnityEngine.Object.FindObjectsByType<SeedlingPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(x=>x.Slot)) {
    Warp(s.transform.position + Vector3.up * 0.5f); await Tick(120);
    var r = s.Interact(actor);
    sb.AppendLine($"seed{s.Slot} interact={r} collected={s.Collected}");
}
sb.AppendLine("stage after seeds=" + ms.Stage + " inventory=" + ms.State.Snapshot.Seedlings);

foreach (var b in UnityEngine.Object.FindObjectsByType<PlantingPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(x=>x.Slot)) {
    Warp(b.transform.position + Vector3.up * 0.5f); await Tick(120);
    var r = b.Interact(actor);
    sb.AppendLine($"bed{b.Slot} interact={r} planted={b.Planted}");
}
await Tick(400);
sb.AppendLine("stage after beds=" + ms.Stage + " progress=" + ms.State.Snapshot.Trees);

foreach (var f in UnityEngine.Object.FindObjectsByType<FirePoint>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(x=>x.Slot)) {
    Warp(f.transform.position + Vector3.up * 0.5f); await Tick(120);
    for (int i = 0; i < 200 && !f.Extinguished; i++) { f.ReceiveWater(0.1f); await Tick(5); }
    sb.AppendLine($"fire{f.Slot} ext={f.Extinguished}");
}
await Tick(500);
sb.AppendLine("stage after fires=" + ms.Stage + " ext=" + ms.State.Snapshot.Fires);

var post = GameObject.Find("Volunteer post return zone");
Warp(post.transform.position + Vector3.up * 0.5f); await Tick(800);
sb.AppendLine("final stage=" + ms.Stage + " mode=" + flow.Mode);
return sb.ToString();
