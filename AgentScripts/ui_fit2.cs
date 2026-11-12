using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

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

// Grow the containers the overflow audit flagged so autosize never has to crush the type.
void Grow(string name, float h)
{
    var t = FindInactive(name, ui);
    if (t == null) { sb.AppendLine("missing " + name); return; }
    var rt = (RectTransform)t;
    rt.sizeDelta = new Vector2(rt.sizeDelta.x, h);
    sb.AppendLine(name + " -> h=" + h);
}
Grow("Briefing story", 122f);
Grow("Help tip", 88f);
Grow("Pause description", 62f);

// Button captions must not exceed their pill; let them shrink instead of clipping.
foreach (var n in new[] { "Pause return menu label", "Pause quit label", "Menu quit label" })
{
    var t = FindInactive(n, ui);
    var tmp = t != null ? t.GetComponent<TMP_Text>() : null;
    if (tmp == null) { sb.AppendLine("no label " + n); continue; }
    tmp.enableAutoSizing = true;
    tmp.fontSizeMin = 16f;
    if (tmp.fontSizeMax < 24f) tmp.fontSizeMax = 28f;
    tmp.overflowMode = TextOverflowModes.Overflow;
    sb.AppendLine("label autosize " + n);
}

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
return sb.ToString();
