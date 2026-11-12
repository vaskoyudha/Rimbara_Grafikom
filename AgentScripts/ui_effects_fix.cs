using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

var sb = new StringBuilder();
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.name != "Arunika") throw new System.Exception("wrong scene " + scene.name);
var ui = GameObject.Find("UI - RIMBARA").transform;

Transform FindInactive(string name, Transform root)
{
    if (root.name == name) return root;
    foreach (Transform c in root)
    {
        var r = FindInactive(name, c);
        if (r != null) return r;
    }
    return null;
}

string[] cards = {
    "Mission card", "Restoration counters", "Next destination", "Interaction prompt",
    "Feedback toast", "Controls reminder", "Arunika field map", "Main menu card",
    "Volunteer briefing", "How to play card", "Pause card", "Restoration ending"
};

// Outline derives from Shadow, so GetComponent<Shadow>() returns the Outline and silently
// overwrites it. Strip every Shadow-derived effect, then add one Outline + one Shadow.
foreach (var cn in cards)
{
    var t = FindInactive(cn, ui);
    if (t == null) { sb.AppendLine("missing " + cn); continue; }

    var effects = t.GetComponents<Shadow>();
    foreach (var e in effects) Object.DestroyImmediate(e);

    var outline = t.gameObject.AddComponent<Outline>();
    outline.effectColor = new Color32(0x7A, 0x4E, 0x2A, 0xFF);
    outline.effectDistance = new Vector2(2f, -2f);
    outline.useGraphicAlpha = true;

    var shadow = t.gameObject.AddComponent<Shadow>();
    shadow.effectColor = new Color32(0x2A, 0x18, 0x0C, 0x66);
    shadow.effectDistance = new Vector2(0f, -7f);
    shadow.useGraphicAlpha = true;

    var img = t.GetComponent<Image>();
    if (img != null) img.pixelsPerUnitMultiplier = 1f;   // native 30px border, no squashing
    sb.AppendLine("fixed " + cn);
}

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
return sb.ToString();
