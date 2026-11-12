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

// Button captions should fill the button (like the pack demo does), not sit in a thin strip.
foreach (var btn in ui.GetComponentsInChildren<Button>(true))
{
    var brt = (RectTransform)btn.transform;
    foreach (var tmp in btn.GetComponentsInChildren<TMP_Text>(true))
    {
        var lrt = (RectTransform)tmp.transform;
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.pivot = new Vector2(0.5f, 0.5f);
        lrt.offsetMin = new Vector2(8f, 6f);
        lrt.offsetMax = new Vector2(-8f, -6f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 16f;
        tmp.fontSizeMax = Mathf.Round(Mathf.Clamp(brt.rect.height * 0.44f, 26f, 38f));
        tmp.fontSize = tmp.fontSizeMax;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        sb.AppendLine("label " + tmp.name + " fits " + Mathf.RoundToInt(brt.rect.width - 16) + "x" + Mathf.RoundToInt(brt.rect.height - 12) + " max=" + tmp.fontSizeMax);
    }
}

// Keep the map title rect inside the 260-wide card.
var mapTitle = FindInactive("Map title", ui);
if (mapTitle != null)
{
    var rt = (RectTransform)mapTitle;
    rt.sizeDelta = new Vector2(220f, 24f);
    rt.anchoredPosition = new Vector2(16f, -14f);
    sb.AppendLine("map title rect 220x24");
}

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
sb.AppendLine("button captions fixed");
return sb.ToString();
