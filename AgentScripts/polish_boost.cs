using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

var sb = new StringBuilder();
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.name != "Arunika") throw new System.Exception("wrong scene " + scene.name);
sb.AppendLine("colorSpace=" + PlayerSettings.colorSpace);
var ui = GameObject.Find("UI - RIMBARA").transform;
Transform FindInactive(string name, Transform root)
{
    if (root.name == name) return root;
    foreach (Transform c in root) { var r = FindInactive(name, c); if (r != null) return r; }
    return null;
}
string[] cards = { "Mission card", "Restoration counters", "Next destination", "Interaction prompt", "Feedback toast", "Controls reminder", "Arunika field map", "Main menu card", "Volunteer briefing", "How to play card", "Pause card", "Restoration ending" };
foreach (var cn in cards)
{
    var t = FindInactive(cn, ui);
    if (t == null) { sb.AppendLine("missing " + cn); continue; }
    var outline = t.GetComponent<Outline>();
    if (outline == null) outline = t.gameObject.AddComponent<Outline>();
    outline.enabled = true;
    outline.effectColor = new Color32(0x6B, 0x3E, 0x1E, 0xFF);
    outline.effectDistance = new Vector2(3f, -3f);
    outline.useGraphicAlpha = true;
    var shadow = t.GetComponent<Shadow>();
    if (shadow == null) shadow = t.gameObject.AddComponent<Shadow>();
    shadow.enabled = true;
    shadow.effectColor = new Color32(0x1E, 0x12, 0x08, 0x66);
    shadow.effectDistance = new Vector2(0f, -8f);
    shadow.useGraphicAlpha = true;
    sb.AppendLine("boosted " + cn);
}
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
sb.AppendLine("saved");
return sb.ToString();
