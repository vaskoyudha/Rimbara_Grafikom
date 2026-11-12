using System.Collections.Generic;
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

void SetCard(string name, float w, float h)
{
    var t = FindInactive(name, ui);
    if (t == null) { sb.AppendLine("no card " + name); return; }
    var rt = (RectTransform)t;
    var pos = rt.anchoredPosition;
    float dw = w - rt.rect.width, dh = h - rt.rect.height;
    rt.sizeDelta = new Vector2(w, h);
    float sx = rt.pivot.x <= 0.01f ? -dw : (rt.pivot.x >= 0.99f ? 0f : -dw * 0.5f);
    float sy = rt.pivot.y <= 0.01f ? 0f : (rt.pivot.y >= 0.99f ? -dh : dh * 0.5f);
    rt.anchoredPosition = pos + new Vector2(sx, sy);
    sb.AppendLine("card " + name + " -> " + w + "x" + h);
}

// Buttons get tall enough that a 30px nine-slice border reads as a proper rounded cap.
var layout = new (string name, float x, float y, float w, float h)[] {
    ("Start restoration", 38f, -441f, 584f, 88f),
    ("Controls button",    38f, -540f, 282f, 76f),
    ("Menu quit",         340f, -540f, 282f, 76f),

    ("Enter Arunika",      46f, -552f, 546f, 84f),
    ("Briefing back",     610f, -552f, 164f, 84f),

    ("Help back",          45f, -548f, 670f, 80f),

    ("Resume mission",     38f, -308f, 434f, 78f),
    ("Pause return menu",  38f, -398f, 266f, 66f),
    ("Pause quit",        322f, -398f, 150f, 66f),

    ("Play again",         46f, -520f, 320f, 78f),
    ("Victory return menu",384f, -520f, 210f, 78f),
    ("Victory quit",      612f, -520f, 142f, 78f),

    ("Briefing back",     610f, -552f, 164f, 84f)
};

foreach (var item in layout)
{
    var t = FindInactive(item.name, ui);
    if (t == null) { sb.AppendLine("no button " + item.name); continue; }
    var rt = (RectTransform)t;
    rt.sizeDelta = new Vector2(item.w, item.h);
    rt.anchoredPosition = new Vector2(item.x, item.y);
    var img = t.GetComponent<Image>();
    if (img != null && img.sprite != null && img.sprite.name.StartsWith("button_"))
        img.pixelsPerUnitMultiplier = Mathf.Round(Mathf.Clamp(30f / (0.28f * item.h), 1f, 3f) * 4f) / 4f;
    foreach (var tmp in t.GetComponentsInChildren<TMP_Text>(true))
    {
        tmp.fontSize = Mathf.Round(Mathf.Clamp(item.h * 0.42f, 26f, 38f));
        tmp.fontSizeMax = tmp.fontSize;
        tmp.fontSizeMin = 16f;
        tmp.enableAutoSizing = true;
    }
    sb.AppendLine("btn " + item.name + " " + item.w + "x" + item.h + " ppm=" + (img != null ? img.pixelsPerUnitMultiplier : 0f));
}

// Footer and card heights follow the taller buttons.
var mainCard = FindInactive("Main menu card", ui);
if (mainCard != null)
{
    SetCard("Main menu card", 660f, 712f);
    var footer = FindInactive("Menu footer", ui);
    if (footer != null) { var frt = (RectTransform)footer; frt.anchoredPosition = new Vector2(38f, -652f); frt.sizeDelta = new Vector2(584f, 38f); }
}
SetCard("Volunteer briefing", 820f, 700f);
SetCard("How to play card", 760f, 672f);
SetCard("Pause card", 510f, 500f);
SetCard("Restoration ending", 800f, 650f);

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
sb.AppendLine("button reflow applied");
return sb.ToString();
