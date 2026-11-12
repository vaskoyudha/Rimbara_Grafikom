using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Cinemachine;
using Rimbara.Player;
using Rimbara.Interaction;

// Replaces the Volunteer prefab instance in the OPEN scene with
// Assets/Prefabs/Player/Volunteer_Ch42.prefab, preserving world transform,
// scene overrides and every reference that pointed into the player hierarchy.
// Idempotent. Saves the scene. Run AFTER build_avatar_ch42.cs.
// Run: unity command script eval --file AgentScripts/swap_avatar_scene.cs

const string OUT_PREFAB = "Assets/Prefabs/Player/Volunteer_Ch42.prefab";
const string CONTROLLER = "Assets/Art/Characters/Mixamo/Volunteer.controller";

var report = new StringBuilder();
int passed = 0, failed = 0;
void Check(string label, bool ok, string detail = "")
{
    string line = (ok ? "PASS  " : "FAIL  ") + label + (string.IsNullOrEmpty(detail) ? "" : " [" + detail + "]");
    report.AppendLine(line);
    Debug.Log("[SWAP] " + line);
    if (ok) passed++; else failed++;
}

UnityEngine.Object Ref(Component c, string path)
{
    var p = new SerializedObject(c).FindProperty(path);
    return p != null && p.propertyType == SerializedPropertyType.ObjectReference ? p.objectReferenceValue : null;
}
Vector3 Vec(Component c, string path, Vector3 fallback)
{
    var p = new SerializedObject(c).FindProperty(path);
    return p != null && p.propertyType == SerializedPropertyType.Vector3 ? p.vector3Value : fallback;
}
void SetRef(Component c, string path, UnityEngine.Object value, string label)
{
    var so = new SerializedObject(c);
    var p = so.FindProperty(path);
    if (p == null || p.propertyType != SerializedPropertyType.ObjectReference)
    { Check(label, false, "property not found"); return; }
    p.objectReferenceValue = value;
    so.ApplyModifiedPropertiesWithoutUndo();
    Check(label, p.objectReferenceValue == value);
}

var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
Check("A scene is open", scene.IsValid() && !string.IsNullOrEmpty(scene.path), scene.name + " " + scene.path);
if (!scene.IsValid() || string.IsNullOrEmpty(scene.path)) throw new Exception("Open the target scene first (unsaved/untitled scenes are not supported).");

var rigs = UnityEngine.Object.FindObjectsByType<VolunteerRig>(FindObjectsInactive.Include, FindObjectsSortMode.None);
Check("Exactly one VolunteerRig in scene", rigs.Length == 1,
      rigs.Length + ": " + string.Join(", ", rigs.Select(r => r.gameObject.name)));
if (rigs.Length != 1)
    throw new Exception("Cannot identify a single player. Found " + rigs.Length + " VolunteerRig: " +
        string.Join(", ", rigs.Select(r => r.transform.name)));

var oldRig = rigs[0];
var oldRoot = oldRig.gameObject;
var oldT = oldRoot.transform;
string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(oldRoot);
var avatar = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Characters/Mixamo/Ch42_nonPBR.fbx")
                .OfType<Avatar>().FirstOrDefault();
var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CONTROLLER);

if (prefabPath == OUT_PREFAB)
{
    Check("Player already uses Volunteer_Ch42 (idempotent no-op)", true, prefabPath);
    var anim0 = oldRoot.GetComponent<Animator>();
    Check("Avatar is Ch42 humanoid", anim0 != null && anim0.avatar != null && anim0.avatar == avatar && anim0.avatar.isHuman);
    Check("Controller intact", anim0 != null && anim0.runtimeAnimatorController == controller);
    foreach (var f in new[] { "viewCamera", "gameplayCamera", "overviewCamera", "cameraTarget" })
        Check("VolunteerRig." + f + " wired", Ref(oldRig, f) != null);
    report.AppendLine("RESULT: " + passed + " passed, " + failed + " failed");
    Debug.Log("[SWAP]\n" + report);
    if (failed > 0) throw new Exception("Already-swapped player failed validation; scene not saved.\n" + report);
    EditorSceneManager.MarkSceneDirty(scene);
    Check("Scene saved", EditorSceneManager.SaveScene(scene), scene.path);
    return report.ToString();
}

Check("Player instance is a prefab instance", prefabPath != null && prefabPath.Length > 0, prefabPath ?? "none");
var viewCam = (Camera)Ref(oldRig, "viewCamera");
var gameplayCam = Ref(oldRig, "gameplayCamera") as CinemachineCamera;
var overviewCam = Ref(oldRig, "overviewCamera") as CinemachineCamera;
var spawn = Vec(oldRig, "spawnPosition", new Vector3(0f, 0.1f, -21f));
var oldSprayer = oldRoot.GetComponent<WaterSprayer>();
var sprayCam = oldSprayer != null ? Ref(oldSprayer, "viewCamera") as Camera : null;
var oldActor = oldRoot.GetComponent<VolunteerActor>();
var stateRef = oldActor != null ? Ref(oldActor, "state") : null;
var missionsRef = oldActor != null ? Ref(oldActor, "missions") : null;
Vector3 worldPos = oldT.position;
Quaternion worldRot = oldT.rotation;
Vector3 localScale = oldT.localScale;
Transform parent = oldT.parent;
int sibling = oldT.GetSiblingIndex();

var newPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(OUT_PREFAB);
Check("Volunteer_Ch42 prefab exists", newPrefab != null, OUT_PREFAB);
if (newPrefab == null) throw new Exception("Run build_avatar_ch42.cs first; missing " + OUT_PREFAB);

var newRoot = (GameObject)PrefabUtility.InstantiatePrefab(newPrefab);
if (newRoot == null) throw new Exception("Failed to instantiate " + OUT_PREFAB);
newRoot.name = oldRoot.name;
newRoot.transform.SetParent(parent, true);
newRoot.transform.position = worldPos;
newRoot.transform.rotation = worldRot;
newRoot.transform.localScale = localScale;
newRoot.transform.SetSiblingIndex(sibling);

var newRig = newRoot.GetComponent<VolunteerRig>();
var newSprayer = newRoot.GetComponent<WaterSprayer>();
var newActor = newRoot.GetComponent<VolunteerActor>();
newRoot.SetActive(oldRoot.activeSelf);

// ---- Copy scene-level overrides that are not captured by the prefab defaults.
SetRef(newRig, "viewCamera", viewCam, "VolunteerRig.viewCamera rewired");
SetRef(newRig, "gameplayCamera", gameplayCam, "VolunteerRig.gameplayCamera rewired");
SetRef(newRig, "overviewCamera", overviewCam, "VolunteerRig.overviewCamera rewired");
var soRigNew = new SerializedObject(newRig);
var spawnProp = soRigNew.FindProperty("spawnPosition");
spawnProp.vector3Value = spawn;
soRigNew.ApplyModifiedPropertiesWithoutUndo();
Check("Spawn position preserved", Vec(newRig, "spawnPosition", Vector3.zero) == spawn, spawn.ToString("F3"));
if (newSprayer != null && sprayCam != null) SetRef(newSprayer, "viewCamera", sprayCam, "WaterSprayer.viewCamera rewired");
if (newActor != null)
{
    if (stateRef != null) SetRef(newActor, "state", stateRef, "VolunteerActor.state rewired");
    if (missionsRef != null) SetRef(newActor, "missions", missionsRef, "VolunteerActor.missions rewired");
}

// ---- Build old -> new object maps (hierarchy by index path; components by type).
string IndexPath(Transform root, Transform t)
{
    var parts = new List<string>();
    while (t != null && t != root) { parts.Add(t.GetSiblingIndex().ToString()); t = t.parent; }
    parts.Reverse();
    return string.Join("/", parts);
}
Transform ByIndexPath(Transform root, string path)
{
    if (string.IsNullOrEmpty(path)) return root;
    var t = root;
    foreach (var part in path.Split('/'))
    {
        if (!int.TryParse(part, out int i) || i < 0 || i >= t.childCount) return null;
        t = t.GetChild(i);
    }
    return t;
}

var tmap = new Dictionary<Transform, Transform>();
var cmap = new Dictionary<Component, Component>();
foreach (var ot in oldRoot.GetComponentsInChildren<Transform>(true))
{
    var nt = ByIndexPath(newRoot.transform, IndexPath(oldRoot.transform, ot));
    if (nt == null || nt.name != ot.name) continue;
    tmap[ot] = nt;
    var oldComps = ot.GetComponents<Component>();
    var newComps = nt.GetComponents<Component>().ToList();
    foreach (var oc in oldComps)
    {
        if (oc == null) continue;
        int idx = newComps.FindIndex(nc => nc != null && nc.GetType() == oc.GetType());
        if (idx >= 0) { cmap[oc] = newComps[idx]; newComps.RemoveAt(idx); }
    }
}
Check("Hierarchy map covers gameplay branch", tmap.ContainsKey(oldT) && tmap.Count >= 4, tmap.Count + " transforms");

Transform TransformOf(UnityEngine.Object o)
{
    if (o is Transform t) return t;
    if (o is GameObject g) return g.transform;
    if (o is Component c) return c.transform;
    return null;
}

UnityEngine.Object MapObject(UnityEngine.Object o, Transform nt)
{
    if (o is GameObject) return nt.gameObject;
    if (o is Transform) return nt;
    if (o is Component oc)
    {
        if (cmap.TryGetValue(oc, out var mapped)) return mapped;
        var cands = nt.GetComponents(oc.GetType());
        if (cands.Length == 1) return cands[0];
    }
    return null;
}

// ---- Two-phase reference rewire: scan for unmappable refs first, then apply.
var pending = new List<(Component owner, string path, UnityEngine.Object value)>();
var unmapped = new List<string>();
foreach (var rootObj in scene.GetRootGameObjects())
{
    foreach (var comp in rootObj.GetComponentsInChildren<Component>(true))
    {
        if (comp == null) continue;
        if (comp.transform == oldRoot.transform || comp.transform.IsChildOf(oldRoot.transform)) continue;
        var so = new SerializedObject(comp);
        var it = so.GetIterator();
        bool enter = true;
        while (it.NextVisible(enter))
        {
            enter = false;
            if (it.propertyType != SerializedPropertyType.ObjectReference) continue;
            var val = it.objectReferenceValue;
            if (val == null) continue;
            var ot = TransformOf(val);
            if (ot == null || !(ot == oldRoot.transform || ot.IsChildOf(oldRoot.transform))) continue;
            if (!tmap.TryGetValue(ot, out var nt))
            {
                unmapped.Add(comp.GetType().Name + "(" + comp.name + ")." + it.propertyPath + " -> " + ot.name);
                continue;
            }
            var replacement = MapObject(val, nt);
            if (replacement == null)
            {
                unmapped.Add(comp.GetType().Name + "(" + comp.name + ")." + it.propertyPath + " -> " + ot.name + " (no counterpart)");
                continue;
            }
            pending.Add((comp, it.propertyPath, replacement));
        }
    }
}
Check("All player references have a counterpart", unmapped.Count == 0,
      unmapped.Count == 0 ? pending.Count + " refs" : string.Join(" | ", unmapped.Take(6)));
if (unmapped.Count > 0)
{
    UnityEngine.Object.DestroyImmediate(newRoot);
    throw new Exception("Cannot rewire scene safely; aborting before destroying the old player. Unmapped: " +
                        string.Join(" | ", unmapped));
}

foreach (var group in pending.GroupBy(p => p.owner))
{
    var so = new SerializedObject(group.Key);
    bool changed = false;
    foreach (var item in group)
    {
        var p = so.FindProperty(item.path);
        if (p == null) continue;
        p.objectReferenceValue = item.value;
        changed = true;
    }
    if (changed) so.ApplyModifiedPropertiesWithoutUndo();
}
report.AppendLine("INFO  rewired " + pending.Count + " scene reference(s)");

// ---- Explicit camera follow target on the Cinemachine gameplay rig.
if (gameplayCam != null)
{
    var camSo = new SerializedObject(gameplayCam);
    var track = camSo.FindProperty("Target.TrackingTarget");
    if (track != null)
    {
        track.objectReferenceValue = newRoot.transform.Find("Camera target");
        camSo.ApplyModifiedPropertiesWithoutUndo();
        Check("Cinemachine gameplay TrackingTarget -> new Camera target",
              track.objectReferenceValue == newRoot.transform.Find("Camera target"));
    }
    else Check("Cinemachine gameplay TrackingTarget property found", false, "Target.TrackingTarget missing");
}

UnityEngine.Object.DestroyImmediate(oldRoot);

// ---- Post-swap validation.
var rigsNow = UnityEngine.Object.FindObjectsByType<VolunteerRig>(FindObjectsInactive.Include, FindObjectsSortMode.None);
Check("Single player remains", rigsNow.Length == 1, rigsNow.Length + " rig(s)");
var finalRig = rigsNow.Length == 1 ? rigsNow[0] : null;
if (finalRig == null) throw new Exception("Player lost during swap.");
var finalRoot = finalRig.gameObject;
Check("Player uses Volunteer_Ch42", PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(finalRoot) == OUT_PREFAB,
      PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(finalRoot));
Check("World position preserved", Vector3.Distance(finalRoot.transform.position, worldPos) < 0.0001f, finalRoot.transform.position.ToString("F3"));
Check("World rotation preserved", Quaternion.Angle(finalRoot.transform.rotation, worldRot) < 0.01f);
Check("Spawn position preserved", Vec(finalRig, "spawnPosition", Vector3.zero) == spawn);
foreach (var f in new[] { "starterController", "starterInputs", "starterPlayerInput", "animator", "cameraTarget",
                          "viewCamera", "gameplayCamera", "overviewCamera" })
    Check("VolunteerRig." + f + " wired", Ref(finalRig, f) != null);
var anim = finalRoot.GetComponent<Animator>();
Check("Animator avatar is Ch42 Humanoid", anim != null && anim.avatar == avatar && anim.avatar.isHuman && anim.avatar.isValid);
Check("Animator controller is Volunteer.controller", anim != null && anim.runtimeAnimatorController == controller);
var camTarget = Ref(finalRig, "cameraTarget") as Transform;
Check("Rig cameraTarget points into new hierarchy", camTarget != null && camTarget.IsChildOf(finalRoot.transform), camTarget != null ? camTarget.name : "null");
var gp = Ref(finalRig, "gameplayCamera") as CinemachineCamera;
if (gp != null)
{
    var tr = new SerializedObject(gp).FindProperty("Target.TrackingTarget");
    Check("Gameplay camera tracks new player", tr != null && tr.objectReferenceValue == camTarget);
}
else Check("Gameplay camera present", false);
var finalSprayer = finalRoot.GetComponent<WaterSprayer>();
Check("WaterSprayer present on player", finalSprayer != null);
if (finalSprayer != null)
{
    var soS = new SerializedObject(finalSprayer);
    foreach (var p in new[] { "input", "actor", "nozzle", "spray", "viewCamera" })
        Check("WaterSprayer." + p + " wired", soS.FindProperty(p) != null &&
              soS.FindProperty(p).objectReferenceValue != null);
}
var finalPose = finalRoot.GetComponent<EquipmentPose>();
Check("EquipmentPose present on player", finalPose != null);
if (finalPose != null)
{
    var soP = new SerializedObject(finalPose);
    foreach (var p in new[] { "handGrip", "water" })
        Check("EquipmentPose." + p + " wired", soP.FindProperty(p) != null &&
              soP.FindProperty(p).objectReferenceValue != null);
}
var missing = finalRoot.GetComponentsInChildren<MonoBehaviour>(true).Count(c => c == null);
Check("No missing scripts on player", missing == 0, missing + " missing");
Check("CharacterController present", finalRoot.GetComponent<CharacterController>() != null);

EditorSceneManager.MarkSceneDirty(scene);
report.AppendLine();
report.AppendLine("RESULT: " + passed + " passed, " + failed + " failed");
Debug.Log("[SWAP]\n" + report);
if (failed > 0) throw new Exception("AVATAR SWAP FAILED (" + failed + " checks); scene left dirty and unsaved.\n" + report);
Check("Scene saved", EditorSceneManager.SaveScene(scene), scene.path);
if (failed > 0) throw new Exception("AVATAR SWAP SAVE FAILED (" + failed + " checks)\n" + report);
return report.ToString();
