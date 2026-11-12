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

// Autosize only shrinks text whose overflow mode is not Overflow.
foreach (var name in new[] { "Menu story", "Briefing story", "Help tip", "Pause description", "Mission description", "Control list", "Briefing steps", "Victory story", "Mission results", "Feedback text", "Interaction text", "Seedling counter", "Tree counter", "Fire counter", "No fail promise", "Menu footer", "Controls text" })
{
    var t = FindInactive(name, ui);
    if (t == null) { sb.AppendLine("missing " + name); continue; }
    var tmp = t.GetComponent<TMP_Text>();
    tmp.overflowMode = TextOverflowModes.Truncate;
    if (!tmp.enableAutoSizing)
    {
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = Mathf.Max(11f, tmp.fontSize * 0.62f);
        tmp.fontSizeMax = tmp.fontSize;
    }
    sb.AppendLine("truncate+autosize " + name);
}

foreach (var btn in ui.GetComponentsInChildren<Button>(true))
{
    foreach (var tmp in btn.GetComponentsInChildren<TMP_Text>(true))
    {
        tmp.overflowMode = TextOverflowModes.Truncate;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 14f;
        sb.AppendLine("btn label truncate " + tmp.name);
    }
}

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
sb.AppendLine("overflow fixed");
return sb.ToString();
