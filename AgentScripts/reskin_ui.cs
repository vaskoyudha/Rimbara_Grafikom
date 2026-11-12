using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

var sb = new StringBuilder();
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.name != "Arunika") throw new System.Exception("wrong scene " + scene.name);

const string T = "Assets/External/KenneyPixelUI/TilesUsed/";
Sprite Tile(string n) => AssetDatabase.LoadAssetAtPath<Sprite>(T + "tile_" + n + ".png");
Sprite Icon(string n) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/External/KenneyPixelUI/Icons/icon_" + n + ".png");
Sprite solid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/External/KenneyPixelUI/pixel_solid.png");

var sFrame = Tile("19");
var sCream = Tile("0");
var sSlate = Tile("2");
var sDark = Tile("3");
var sGold = Tile("13");
var sBar = Tile("43");
var sDisc = Tile("52");
if (sFrame == null || sCream == null || solid == null) throw new System.Exception("sprites missing");

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

void AddFrame(Transform panel, Color? tint = null, float inset = -2f)
{
    var existing = panel.Find("Pixel frame");
    GameObject go;
    if (existing != null) go = existing.gameObject;
    else
    {
        go = new GameObject("Pixel frame", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(panel, false);
    }
    var rt = (RectTransform)go.transform;
    rt.anchorMin = Vector2.zero;
    rt.anchorMax = Vector2.one;
    rt.offsetMin = new Vector2(inset, inset);
    rt.offsetMax = new Vector2(-inset, -inset);
    var img = go.GetComponent<Image>();
    img.sprite = sFrame;
    img.type = Image.Type.Sliced;
    img.color = tint ?? Color.white;
    img.raycastTarget = false;
    go.transform.SetAsLastSibling();
}

string[] panels = {
    "Mission card", "Restoration counters", "Next destination", "Interaction prompt",
    "Feedback toast", "Controls reminder", "Arunika field map", "Main menu card",
    "Volunteer briefing", "How to play card", "Pause card", "Restoration ending"
};
foreach (var pn in panels)
{
    var t = FindInactive(pn, ui);
    if (t == null) { sb.AppendLine("panel missing " + pn); continue; }
    AddFrame(t);
}

void StyleButton(Button b, Sprite sprite, Color labelColor)
{
    var img = b.GetComponent<Image>();
    img.sprite = sprite;
    img.type = Image.Type.Sliced;
    img.color = Color.white;
    var cb = b.colors;
    cb.normalColor = Color.white;
    cb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
    cb.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
    cb.selectedColor = Color.white;
    cb.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.6f);
    b.colors = cb;
    foreach (var tmp in b.GetComponentsInChildren<TMP_Text>(true)) tmp.color = labelColor;
}

var darkText = new Color32(0x3A, 0x24, 0x12, 0xFF);
var creamText = new Color32(0xF2, 0xE6, 0xC8, 0xFF);
string[] primary = { "Start restoration", "Enter Arunika", "Help back", "Resume mission" };
string[] accent = { "Start restoration" };

foreach (var b in ui.GetComponentsInChildren<Button>(true))
{
    if (b.transform.name == "Pixel frame") continue;
    bool isPrimary = primary.Contains(b.name);
    var sprite = isPrimary ? (b.name == "Start restoration" ? sCream : sGold) : sSlate;
    StyleButton(b, sprite, isPrimary ? darkText : creamText);
}

// Bars.
foreach (var path in new[] { "Restoration bar background", "Water contact progress" })
{
    var t = FindInactive(path, ui);
    if (t == null) continue;
    var img = t.GetComponent<Image>();
    if (img != null) { img.sprite = sDark; img.type = Image.Type.Sliced; img.color = new Color(0.20f, 0.13f, 0.09f, 0.95f); }
}
foreach (var path in new[] { "Restoration bar fill", "Water contact fill" })
{
    var t = FindInactive(path, ui);
    if (t == null) continue;
    var img = t.GetComponent<Image>();
    if (img != null) { img.sprite = solid; img.type = Image.Type.Simple; }
}

// Slider.
var slider = ui.GetComponentsInChildren<Slider>(true).FirstOrDefault();
if (slider != null)
{
    var bg = slider.transform.Find("Volume background");
    if (bg != null)
    {
        var img = bg.GetComponent<Image>();
        img.sprite = sDark; img.type = Image.Type.Sliced; img.color = new Color(0.20f, 0.13f, 0.09f, 1f);
    }
    var handle = slider.transform.Find("Volume handle area");
    if (handle != null)
    {
        var img = handle.GetComponent<Image>();
        if (img != null && img.sprite != null)
        {
            var h = handle.Find("Volume handle");
            if (h != null)
            {
                var hi = h.GetComponent<Image>();
                if (hi != null) { hi.sprite = sDisc; hi.type = Image.Type.Simple; hi.color = Color.white; }
                var hrt = h as RectTransform;
                if (hrt != null) hrt.sizeDelta = new Vector2(24f, 24f);
            }
        }
    }
}

// Field map dots become pixel discs.
foreach (var n in new[] { "Zone A", "Zone B", "Zone C", "Destination dot", "Volunteer dot" })
{
    var t = FindInactive(n, ui);
    if (t == null) continue;
    var img = t.GetComponent<Image>();
    if (img != null) { img.sprite = sDisc; img.type = Image.Type.Simple; }
}

// Counter icons + text indent.
var counters = FindInactive("Restoration counters", ui);
if (counters != null)
{
    var lines = new (string text, string icon)[] {
        ("Seedling counter", "sprout"),
        ("Tree counter", "tree"),
        ("Fire counter", "flame"),
        ("Restoration percent", "check")
    };
    foreach (var (textName, iconName) in lines)
    {
        var t = FindInactive(textName, counters);
        var ico = Icon(iconName);
        if (t == null || ico == null) { sb.AppendLine("icon skip " + textName); continue; }
        var tmp = t.GetComponent<TMP_Text>();
        var rt = (RectTransform)t;

        var go = new GameObject("Icon " + iconName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(counters, false);
        var irt = (RectTransform)go.transform;
        irt.anchorMin = new Vector2(0f, 1f);
        irt.anchorMax = new Vector2(0f, 1f);
        irt.pivot = new Vector2(0f, 1f);
        float y = rt.anchoredPosition.y - Mathf.Max(0f, (rt.rect.height - 18f) * 0.5f);
        irt.anchoredPosition = new Vector2(22f, y);
        irt.sizeDelta = new Vector2(18f, 18f);
        var img = go.GetComponent<Image>();
        img.sprite = ico;
        img.type = Image.Type.Simple;
        img.raycastTarget = false;
        img.preserveAspect = true;
        go.transform.SetSiblingIndex(t.GetSiblingIndex());

        rt.anchoredPosition = new Vector2(44f, rt.anchoredPosition.y);
        rt.sizeDelta = new Vector2(rt.sizeDelta.x - 24f, rt.sizeDelta.y);
    }
}

// Slightly warmer palette for the dark cards keeps the pixel frames readable.
foreach (var pn in panels)
{
    var t = FindInactive(pn, ui);
    if (t == null) continue;
    var img = t.GetComponent<Image>();
    if (img != null) img.color = new Color32(0x14, 0x1E, 0x17, 0xF2);
}

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
sb.AppendLine("reskinned");
return sb.ToString();
