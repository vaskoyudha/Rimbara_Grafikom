using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using Rimbara.Player;
using Rimbara.Interaction;

// Builds Assets/Prefabs/Player/Volunteer_Ch42.prefab from the existing gameplay
// prefab, replacing only the visual model child (Mixamo Y Bot -> Ch42) and
// re-pointing the root Animator at the Ch42 humanoid avatar.
// Run: unity command script eval --file AgentScripts/build_avatar_ch42.cs

const string FBX = "Assets/Art/Characters/Mixamo/Ch42_nonPBR.fbx";
const string YBOT = "Assets/Art/Characters/Mixamo/YBot.fbx";
const string ARMATURE = "Assets/Starter Assets/Runtime/ThirdPersonController/Character/Models/Armature.fbx";
const string CONTROLLER = "Assets/Art/Characters/Mixamo/Volunteer.controller";
const string SRC_PREFAB = "Assets/Prefabs/Player/Volunteer.prefab";
const string OUT_PREFAB = "Assets/Prefabs/Player/Volunteer_Ch42.prefab";
const string MAT_DIR = "Assets/Art/Characters/Mixamo/Ch42";
const string TEX_1001 = "Assets/Art/Characters/Mixamo/Ch42_1001_Diffuse.png";
const string TEX_1002 = "Assets/Art/Characters/Mixamo/Ch42_1002_Diffuse.png";
const string TEX_NORMAL = "Assets/Art/Characters/Mixamo/Ch42_1001_Normal.png";

var report = new StringBuilder();
int passed = 0, failed = 0;
void Check(string label, bool ok, string detail = "")
{
    string line = (ok ? "PASS  " : "FAIL  ") + label + (string.IsNullOrEmpty(detail) ? "" : " [" + detail + "]");
    report.AppendLine(line);
    Debug.Log("[CH42] " + line);
    if (ok) passed++; else failed++;
}
void Warn(string label, string detail)
{
    report.AppendLine("WARN  " + label + " [" + detail + "]");
    Debug.LogWarning("[CH42] WARN " + label + " [" + detail + "]");
}

// ---------------------------------------------------------------- textures
foreach (var tex in new[] { TEX_1001, TEX_1002, TEX_NORMAL,
             "Assets/Art/Characters/Mixamo/Ch42_1001_Glossiness.png",
             "Assets/Art/Characters/Mixamo/Ch42_1001_Specular.png" })
{
    if (AssetDatabase.LoadAssetAtPath<Texture2D>(tex) == null)
    {
        AssetDatabase.ImportAsset(tex, ImportAssetOptions.ForceSynchronousImport);
    }
}
var normalImporter = AssetImporter.GetAtPath(TEX_NORMAL) as TextureImporter;
if (normalImporter != null && normalImporter.textureType != TextureImporterType.NormalMap)
{
    normalImporter.textureType = TextureImporterType.NormalMap;
    normalImporter.SaveAndReimport();
}
var normalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_NORMAL);
var diffuse1001 = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_1001);
var diffuse1002 = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_1002);
Check("Ch42 diffuse 1001/1002 imported", diffuse1001 != null && diffuse1002 != null);
Check("Ch42 normal map imported as NormalMap", normalTex != null &&
      (AssetImporter.GetAtPath(TEX_NORMAL) as TextureImporter)?.textureType == TextureImporterType.NormalMap);

// ------------------------------------------------------------- fbx humanoid
AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
if (AssetImporter.GetAtPath(FBX) == null)
    AssetDatabase.ImportAsset(FBX, ImportAssetOptions.ForceSynchronousImport);
var importer = AssetImporter.GetAtPath(FBX) as ModelImporter;
Check("Ch42 fbx present and importable", importer != null, FBX);
if (importer == null) throw new Exception("Cannot import " + FBX);

importer.animationType = ModelImporterAnimationType.Human;
importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
importer.autoGenerateAvatarMappingIfUnspecified = true;
importer.importAnimation = false;
importer.importCameras = false;
importer.importLights = false;
importer.optimizeGameObjects = false;
importer.globalScale = 1f;
importer.useFileUnits = true;
importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
importer.SaveAndReimport();

Avatar FindAvatar(string path) =>
    AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().FirstOrDefault();

var avatar = FindAvatar(FBX);
Avatar sourceAvatar = FindAvatar(YBOT) ?? FindAvatar(ARMATURE);

bool UsesMixamoRig(Avatar a) => a != null && a.humanDescription.human != null &&
    a.humanDescription.human.Any(h => h.boneName.ToLowerInvariant().Contains("mixamorig"));

if (avatar != null && avatar.isHuman && avatar.isValid && !UsesMixamoRig(avatar))
{
    // Humanoid mapping exists but was auto-built against the wrong skeleton
    // (the fbx meta points at Starter Assets Armature as avatar source).
    importer = (ModelImporter)AssetImporter.GetAtPath(FBX);
    var ybot = FindAvatar(YBOT);
    if (ybot != null) importer.humanDescription = ybot.humanDescription;
    importer.SaveAndReimport();
    avatar = FindAvatar(FBX);
}
if ((avatar == null || !avatar.isHuman || !avatar.isValid) && sourceAvatar != null)
{
    importer = (ModelImporter)AssetImporter.GetAtPath(FBX);
    importer.humanDescription = sourceAvatar.humanDescription;
    importer.SaveAndReimport();
    avatar = FindAvatar(FBX);
}
if ((avatar == null || !avatar.isHuman || !avatar.isValid) && sourceAvatar != null &&
    (sourceAvatar.humanDescription.human?.Length ?? 0) > 0)
{
    var arm = FindAvatar(ARMATURE);
    if (arm != null && arm != sourceAvatar)
    {
        importer = (ModelImporter)AssetImporter.GetAtPath(FBX);
        importer.humanDescription = arm.humanDescription;
        importer.SaveAndReimport();
        avatar = FindAvatar(FBX);
    }
}
Check("Ch42 avatar exists", avatar != null);
Check("Ch42 avatar is Humanoid", avatar != null && avatar.isHuman);
Check("Ch42 avatar is valid", avatar != null && avatar.isValid);
Check("Avatar bone mapping targets mixamorig skeleton", UsesMixamoRig(avatar) ||
      (avatar != null && avatar.isHuman && avatar.humanDescription.human.Length == 0),
      avatar != null ? avatar.humanDescription.human.Length + " mapped bone(s)" : "no avatar");
bool mappingOk = avatar != null && avatar.isHuman && avatar.isValid &&
                 (UsesMixamoRig(avatar) || avatar.humanDescription.human.Length == 0);
if (!mappingOk)
{
    Debug.Log("[CH42]\n" + report);
    throw new Exception("Ch42 avatar is not a valid Mixamo humanoid; aborting before prefab build.\n" + report);
}

// --------------------------------------------------------------- materials
if (!AssetDatabase.IsValidFolder(MAT_DIR))
{
    AssetDatabase.CreateFolder("Assets/Art/Characters/Mixamo", "Ch42");
}

var urpLit = Shader.Find("Universal Render Pipeline/Lit");
Check("URP Lit shader found", urpLit != null, urpLit != null ? urpLit.name : "missing");
if (urpLit == null) throw new Exception("Universal Render Pipeline/Lit shader not found; is the URP package imported?");

Material MakeMaterial(string assetPath, string matName, Texture2D diffuse, Texture2D normal, float smoothness)
{
    var mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
    if (mat == null)
    {
        mat = new Material(urpLit);
        AssetDatabase.CreateAsset(mat, assetPath);
    }
    mat.shader = urpLit;
    mat.name = matName;
    mat.SetTexture("_BaseMap", diffuse);
    mat.SetTexture("_MainTex", diffuse);
    mat.SetColor("_BaseColor", Color.white);
    mat.SetFloat("_Metallic", 0f);
    mat.SetFloat("_Smoothness", smoothness);
    mat.SetFloat("_Surface", 0f);
    mat.SetFloat("_WorkflowMode", 1f);
    mat.SetFloat("_AlphaClip", 0f);
    if (normal != null)
    {
        mat.SetTexture("_BumpMap", normal);
        mat.SetFloat("_BumpScale", 1f);
        mat.EnableKeyword("_NORMALMAP");
    }
    EditorUtility.SetDirty(mat);
    return mat;
}

var mat1001 = MakeMaterial(MAT_DIR + "/Ch42_1001.mat", "Ch42_1001", diffuse1001, normalTex, 0.32f);
var mat1002 = MakeMaterial(MAT_DIR + "/Ch42_1002.mat", "Ch42_1002", diffuse1002, normalTex, 0.22f);
AssetDatabase.SaveAssets();
Check("URP materials created", mat1001 != null && mat1002 != null,
      AssetDatabase.GetAssetPath(mat1001) + " + " + AssetDatabase.GetAssetPath(mat1002));

// Learn the imported material/texture mapping without touching the asset.
var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FBX);
var importedMaterials = new Dictionary<string, Material>();
var probe = UnityEngine.Object.Instantiate(fbxAsset);
foreach (var r in probe.GetComponentsInChildren<Renderer>(true))
    foreach (var m in r.sharedMaterials)
        if (m != null && !importedMaterials.ContainsKey(m.name)) importedMaterials[m.name] = m;

Material PickMaterial(Renderer r, Material imported)
{
    string rn = r.gameObject.name.ToLowerInvariant();
    string mn = imported != null ? imported.name.ToLowerInvariant() : "";
    bool clothing = rn.Contains("shirt") || rn.Contains("short") || rn.Contains("sneaker") || mn.Contains("1002");
    var main = imported != null ? imported.mainTexture : null;
    if (main != null)
    {
        string p = AssetDatabase.GetAssetPath(main);
        if (p.Contains("1002")) clothing = true;
        else if (p.Contains("1001")) clothing = false;
    }
    return clothing ? mat1002 : mat1001;
}
UnityEngine.Object.DestroyImmediate(probe);
Check("Ch42 model exposes renderers", importedMaterials.Count >= 0, importedMaterials.Count + " imported material(s)");

// ---------------------------------------------------------------- prefab
var oldRoot = PrefabUtility.LoadPrefabContents(SRC_PREFAB);
Check("Source prefab loaded", oldRoot != null, SRC_PREFAB);
if (oldRoot == null) throw new Exception("Cannot load " + SRC_PREFAB);
var animator = oldRoot.GetComponent<Animator>();
var cc = oldRoot.GetComponent<CharacterController>();
var rig = oldRoot.GetComponent<VolunteerRig>();
Check("Animator on prefab root", animator != null);
Check("CharacterController on prefab root", cc != null);
Check("VolunteerRig on prefab root", rig != null);

Transform oldModel = oldRoot.transform.Cast<Transform>()
    .FirstOrDefault(t => t.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length > 0);
Check("Y Bot visual child found", oldModel != null, oldModel != null ? oldModel.name : "none");
if (oldModel == null) { PrefabUtility.UnloadPrefabContents(oldRoot); throw new Exception("Cannot locate Y Bot visual child in " + SRC_PREFAB); }

Vector3 keepPos = oldModel.localPosition;
Quaternion keepRot = oldModel.localRotation;
Vector3 keepScale = oldModel.localScale;
int oldLayer = oldModel.gameObject.layer;
int keepSibling = oldModel.GetSiblingIndex();
UnityEngine.Object.DestroyImmediate(oldModel.gameObject);

GameObject newModel = null;
try { newModel = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, oldRoot.transform); }
catch (Exception e)
{
    Warn("Nested Ch42 prefab instantiation fell back to plain copy", e.Message);
    newModel = (GameObject)UnityEngine.Object.Instantiate(fbxAsset, oldRoot.transform);
    var inst = PrefabUtility.GetOutermostPrefabInstanceRoot(newModel);
    if (inst != null) PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
}
newModel.name = "Ch42";
newModel.transform.SetSiblingIndex(keepSibling);
newModel.transform.localPosition = keepPos;
newModel.transform.localRotation = keepRot;
newModel.transform.localScale = keepScale;
foreach (var t in newModel.GetComponentsInChildren<Transform>(true))
    t.gameObject.layer = oldLayer;

foreach (var a in newModel.GetComponentsInChildren<Animator>(true))
    UnityEngine.Object.DestroyImmediate(a);

int assignedRenderers = 0;
foreach (var r in newModel.GetComponentsInChildren<Renderer>(true))
{
    int slots = r.sharedMaterials.Length;
    if (slots == 0)
    {
        var mesh = (r as SkinnedMeshRenderer)?.sharedMesh;
        slots = mesh != null ? Mathf.Max(1, mesh.subMeshCount) : 1;
    }
    var chosenSlots = new Material[slots];
    for (int i = 0; i < slots; i++)
    {
        var imported = i < r.sharedMaterials.Length ? r.sharedMaterials[i] : null;
        chosenSlots[i] = PickMaterial(r, imported);
    }
    r.sharedMaterials = chosenSlots;
    assignedRenderers++;
}
Check("Ch42 renderers have URP materials", assignedRenderers > 0, assignedRenderers + " renderer(s)");

if (animator != null)
{
    animator.avatar = avatar;
    animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CONTROLLER);
    animator.applyRootMotion = false;
    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    EditorUtility.SetDirty(animator);
}

// ------------------------------------------------------------- validations
Check("Controller still Volunteer.controller", animator != null &&
      animator.runtimeAnimatorController == AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CONTROLLER));
Check("Animator uses Ch42 avatar", animator != null && animator.avatar == avatar);

var missingScripts = oldRoot.GetComponentsInChildren<MonoBehaviour>(true).Count(c => c == null);
Check("No missing scripts in prefab", missingScripts == 0, missingScripts + " missing");

var soRig = new SerializedObject(rig);
string[] rigPaths = { "controls", "starterController", "starterInputs", "starterPlayerInput", "animator", "cameraTarget" };
foreach (var p in rigPaths)
{
    var prop = soRig.FindProperty(p);
    Check("VolunteerRig." + p + " wired", prop != null && prop.propertyType == SerializedPropertyType.ObjectReference
          && prop.objectReferenceValue != null);
}
var sprayer = oldRoot.GetComponent<WaterSprayer>();
Check("WaterSprayer present", sprayer != null);
if (sprayer != null)
{
    var soS = new SerializedObject(sprayer);
    string[] sprayPaths = { "input", "actor", "nozzle", "spray" };
    foreach (var p in sprayPaths)
    {
        var prop = soS.FindProperty(p);
        Check("WaterSprayer." + p + " wired", prop != null && prop.propertyType == SerializedPropertyType.ObjectReference
              && prop.objectReferenceValue != null);
    }
}
var pose = oldRoot.GetComponent<EquipmentPose>();
Check("EquipmentPose present", pose != null);
if (pose != null)
{
    var soP = new SerializedObject(pose);
    foreach (var p in new[] { "handGrip", "water" })
    {
        var prop = soP.FindProperty(p);
        Check("EquipmentPose." + p + " wired", prop != null && prop.objectReferenceValue != null);
    }
}
Check("ThirdPersonController still present", oldRoot.GetComponent<StarterAssets.ThirdPersonController>() != null);
Check("StarterAssetsInputs still present", oldRoot.GetComponent<StarterAssets.StarterAssetsInputs>() != null);
Check("PlayerInput still present", oldRoot.GetComponent<UnityEngine.InputSystem.PlayerInput>() != null);
Check("CharacterController preserved (1.8m capsule)", cc != null && Mathf.Approximately(cc.height, 1.8f) && cc.center == new Vector3(0f, 0.9f, 0f));

// Material / shader sanity (magenta guard).
var renderersNow = newModel.GetComponentsInChildren<Renderer>(true);
var allMats = renderersNow.SelectMany(r => r.sharedMaterials).ToArray();
bool matsOk = allMats.Length > 0 && allMats.All(m => m != null && m.shader != null &&
              m.shader.name == "Universal Render Pipeline/Lit" && !ShaderUtil.ShaderHasError(m.shader));
Check("All materials are valid URP Lit", matsOk, allMats.Length + " slot(s)");

float meshHeight = 0f;
foreach (var s in newModel.GetComponentsInChildren<SkinnedMeshRenderer>(true))
    if (s.sharedMesh != null) meshHeight = Mathf.Max(meshHeight, s.sharedMesh.bounds.size.y);
meshHeight *= Mathf.Abs(newModel.transform.lossyScale.y);
Check("Model height in human range", meshHeight >= 1.0f && meshHeight <= 2.8f, "h=" + meshHeight.ToString("F2"));

// Optional real render probe; never fails the build on preview limitations.
try
{
    var util = new PreviewRenderUtility();
    util.camera.clearFlags = CameraClearFlags.SolidColor;
    util.camera.backgroundColor = new Color(0.18f, 0.18f, 0.18f, 1f);
    var probeGo = UnityEngine.Object.Instantiate(fbxAsset);
    util.AddSingleGO(probeGo);
    var bounds = new Bounds(Vector3.zero, Vector3.one);
    bool first = true;
    foreach (var r in probeGo.GetComponentsInChildren<Renderer>(true))
    {
        if (first) { bounds = r.bounds; first = false; }
        else bounds.Encapsulate(r.bounds);
    }
    var cam = util.camera;
    cam.transform.position = bounds.center + new Vector3(0f, bounds.size.y * 0.1f, -bounds.size.y * 1.6f);
    cam.transform.LookAt(bounds.center);
    cam.nearClipPlane = 0.05f;
    cam.farClipPlane = bounds.size.magnitude * 4f + 10f;
    util.BeginPreview(new Rect(0, 0, 256, 256), GUIStyle.none);
    cam.Render();
    var rt = util.EndPreview() as RenderTexture;
    if (rt == null) throw new Exception("preview returned no RenderTexture");
    var prevActive = RenderTexture.active;
    RenderTexture.active = rt;
    var tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
    tex.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
    tex.Apply();
    RenderTexture.active = prevActive;
    var px = tex.GetPixels32();
    int magenta = px.Count(c => c.r > 140 && c.b > 140 && c.g < 80);
    UnityEngine.Object.DestroyImmediate(tex);
    UnityEngine.Object.DestroyImmediate(probeGo);
    util.Cleanup();
    if (magenta > px.Length * 0.05f) Check("Renders non-magenta (preview probe)", false, magenta + "/" + px.Length + " magenta px");
    else Check("Renders non-magenta (preview probe)", true, magenta + "/" + px.Length + " magenta px");
}
catch (Exception e)
{
    Warn("Preview magenta probe skipped", e.GetType().Name + ": " + e.Message);
}

// ------------------------------------------------------------------- save
if (failed > 0)
{
    PrefabUtility.UnloadPrefabContents(oldRoot);
    Debug.Log("[CH42]\n" + report);
    throw new Exception("CH42 AVATAR BUILD FAILED (" + failed + " checks); prefab not written.\n" + report);
}
var saved = PrefabUtility.SaveAsPrefabAsset(oldRoot, OUT_PREFAB, out bool saveOk);
PrefabUtility.UnloadPrefabContents(oldRoot);
Check("Prefab saved", saveOk && saved != null, OUT_PREFAB);

if (saved != null)
{
    var savedAnimator = saved.GetComponent<Animator>();
    Check("Saved prefab keeps Ch42 avatar", savedAnimator != null && savedAnimator.avatar == avatar);
    Check("Saved prefab keeps controller", savedAnimator != null &&
          savedAnimator.runtimeAnimatorController == AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CONTROLLER));
    Check("Saved prefab keeps VolunteerRig", saved.GetComponent<VolunteerRig>() != null);
    Check("Saved prefab visual is Ch42", saved.transform.Cast<Transform>().Any(t =>
          t.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length > 0 && t.name == "Ch42"));
}

AssetDatabase.SaveAssets();
report.AppendLine();
report.AppendLine("RESULT: " + passed + " passed, " + failed + " failed");
Debug.Log("[CH42]\n" + report);
if (failed > 0) throw new Exception("CH42 AVATAR BUILD FAILED (" + failed + " checks)\n" + report);
return report.ToString();
