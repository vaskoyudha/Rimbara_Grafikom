using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

// 1. Cards: the pack's panel sprite has no stroke or shadow, so on a bright scene it reads
//    as a flat blob. Add a warm rim + soft drop shadow for real definition.
foreach (var cn in cards)
{
    var t = FindInactive(cn, ui);
    if (t == null) { sb.AppendLine("card missing " + cn); continue; }
    var img = t.GetComponent<Image>();
    if (img == null) continue;

    var outline = t.GetComponent<Outline>();
    if (outline == null) outline = t.gameObject.AddComponent<Outline>();
    outline.effectColor = new Color32(0x8A, 0x62, 0x42, 0xFF);
    outline.effectDistance = new Vector2(2f, -2f);
    outline.useGraphicAlpha = true;

    var shadow = t.GetComponent<Shadow>();
    if (shadow == null || shadow is Outline)
    {
        Shadow real = null;
        foreach (var candidate in t.GetComponents<Shadow>()) if (!(candidate is Outline)) { real = candidate; break; }
        if (real == null) real = t.gameObject.AddComponent<Shadow>();
        shadow = real;
    }
    shadow.effectColor = new Color32(0x2E, 0x1C, 0x0E, 0x55);
    shadow.effectDistance = new Vector2(0f, -6f);
    shadow.useGraphicAlpha = true;
    sb.AppendLine("defined card " + cn);
}

// 2. Pills were too short for a 30px nine-slice border (they collapsed into slivers).
void ResizePill(string name, float h)
{
    var t = FindInactive(name, ui);
    if (t == null) { sb.AppendLine("pill missing " + name); return; }
    var rt = (RectTransform)t;
    float dh = h - rt.rect.height;
    rt.sizeDelta = new Vector2(rt.rect.width, h);
    rt.anchoredPosition += new Vector2(0f, -dh * (0.5f - rt.pivot.y));
    var img = t.GetComponent<Image>();
    if (img != null && img.sprite != null)
        img.pixelsPerUnitMultiplier = Mathf.Round(Mathf.Clamp(30f / (0.30f * h), 1f, 3f) * 4f) / 4f;

    // Centre the caption vertically inside the taller pill.
    foreach (var tmp in t.GetComponentsInChildren<TMP_Text>(true))
    {
        var lrt = (RectTransform)tmp.transform;
        if (lrt.anchorMin != lrt.anchorMax) continue;
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = new Vector2(14f, 6f);
        lrt.offsetMax = new Vector2(-14f, -6f);
        tmp.alignment = TextAlignmentOptions.Center;
    }
    sb.AppendLine("pill " + name + " -> " + h);
}
ResizePill("Next destination", 76f);
ResizePill("Interaction prompt", 88f);
ResizePill("Feedback toast", 80f);
ResizePill("Controls reminder", 56f);

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
sb.AppendLine("card definition + pill proportions applied");
return sb.ToString();
