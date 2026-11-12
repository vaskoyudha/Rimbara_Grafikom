using System.Collections.Generic;
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

const string S = "Assets/maanetorn/Farm Game UI - Simple 2D UI/Sprites/128x128/";
const string F = "Assets/maanetorn/Farm Game UI - Simple 2D UI/Font/";

Sprite Sp(string n)
{
    var s = AssetDatabase.LoadAssetAtPath<Sprite>(S + n + ".png");
    if (s == null) sb.AppendLine("MISSING sprite " + n);
    return s;
}

var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(F + "LilitaOne-RegularSDF.asset");
var fontOut = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(F + "LilitaOne-with outline.asset");
if (font == null || fontOut == null) throw new System.Exception("font missing");

var panel = Sp("panel_1");
var yellow = Sp("button_yellow_default_1"); var yellowP = Sp("button_yellow_pressed_1");
var green = Sp("button_green_default_1"); var greenP = Sp("button_green_pressed_1");
var blue = Sp("button_blue_default_1"); var blueP = Sp("button_blue_pressed_1");
var gray = Sp("button_gray_default_1"); var grayP = Sp("button_gray_pressed_1");
var red = Sp("button_red_default_1"); var redP = Sp("button_red_pressed_1");
var solid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/External/KenneyPixelUI/pixel_solid.png");
var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

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

TMP_Text Txt(string name)
{
    var t = FindInactive(name, ui);
    if (t == null) return null;
    var tmp = t.GetComponent<TMP_Text>();
    if (tmp == null) sb.AppendLine("not text " + name);
    return tmp;
}

var brown = new Color32(0x69, 0x42, 0x23, 0xFF);
var brownMuted = new Color32(0x8A, 0x62, 0x42, 0xFF);
var olive = new Color32(0x5E, 0x6B, 0x3E, 0xFF);
var amber = new Color32(0xA6, 0x5B, 0x12, 0xFF);

string[] cards = {
    "Mission card", "Restoration counters", "Next destination", "Interaction prompt",
    "Feedback toast", "Controls reminder", "Arunika field map", "Main menu card",
    "Volunteer briefing", "How to play card", "Pause card", "Restoration ending"
};

foreach (var cn in cards)
{
    var t = FindInactive(cn, ui);
    if (t == null) { sb.AppendLine("card missing " + cn); continue; }
    var img = t.GetComponent<Image>();
    if (img != null)
    {
        img.sprite = panel;
        img.type = Image.Type.Sliced;
        img.color = Color.white;
    }
    var frame = t.Find("Pixel frame");
    if (frame != null) frame.gameObject.SetActive(false);
}

var headings = new Dictionary<string, int> {
    {"RIMBARA title", 84}, {"Mission title", 34}, {"Briefing title", 50},
    {"Help title", 48}, {"Pause title", 44}, {"Victory title", 46}, {"Map title", 18}
};

var bannerSprite = Sp("panel_title_with_stroke_1");
foreach (var kv in headings)
{
    var tmp = Txt(kv.Key);
    if (tmp == null) { sb.AppendLine("heading missing " + kv.Key); continue; }
    var rt = (RectTransform)tmp.transform;
    var cardRoot = tmp.transform.parent;

    var existing = cardRoot.Find("Title banner");
    GameObject go;
    if (existing != null) go = existing.gameObject;
    else
    {
        go = new GameObject("Title banner", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(cardRoot, false);
    }
    var brt = (RectTransform)go.transform;
    brt.anchorMin = rt.anchorMin;
    brt.anchorMax = rt.anchorMax;
    brt.pivot = new Vector2(0.5f, 0.5f);
    brt.anchoredPosition = new Vector2(
        rt.anchoredPosition.x + rt.rect.width * 0.5f,
        rt.anchoredPosition.y - rt.rect.height * 0.5f);
    brt.sizeDelta = new Vector2(rt.rect.width + 20f, rt.rect.height + 18f);
    var bimg = go.GetComponent<Image>();
    bimg.sprite = bannerSprite;
    bimg.type = Image.Type.Sliced;
    bimg.color = Color.white;
    bimg.raycastTarget = false;
    // banner lands on the heading's old index, pushing the heading one slot forward
    go.transform.SetSiblingIndex(rt.GetSiblingIndex());

    tmp.font = fontOut;
    tmp.color = Color.white;
    tmp.fontSize = kv.Value;
    tmp.enableAutoSizing = false;
}

var accent = new (string name, int size)[] {
    ("Mission number", 17), ("Menu eyebrow", 17), ("Briefing eyebrow", 17),
    ("Help eyebrow", 17), ("Pause eyebrow", 17), ("Victory eyebrow", 17),
    ("Game subtitle", 19), ("Menu scope", 16), ("Briefing targets", 19),
    ("Waypoint instruction", 18), ("Restoration percent", 18), ("Volume label", 16)
};
foreach (var (name, size) in accent)
{
    var tmp = Txt(name);
    if (tmp == null) continue;
    tmp.font = font;
    tmp.color = amber;
    tmp.fontSize = size;
    tmp.enableAutoSizing = false;
}

var body = new (string name, int size)[] {
    ("Menu story", 24), ("Briefing story", 25), ("Briefing steps", 22),
    ("Control list", 21), ("Help tip", 23), ("Pause description", 24),
    ("Victory story", 25), ("Mission results", 24), ("Mission description", 23),
    ("Interaction text", 26), ("Feedback text", 24)
};
foreach (var (name, size) in body)
{
    var tmp = Txt(name);
    if (tmp == null) continue;
    tmp.font = font;
    tmp.color = brown;
    tmp.fontSize = size;
    tmp.enableAutoSizing = true;
    tmp.fontSizeMin = 14;
    tmp.fontSizeMax = size;
}

foreach (var name in new[] { "No fail promise" })
{
    var tmp = Txt(name);
    if (tmp == null) continue;
    tmp.font = font; tmp.color = olive; tmp.fontSize = 20;
    tmp.enableAutoSizing = true; tmp.fontSizeMin = 14; tmp.fontSizeMax = 20;
}
foreach (var name in new[] { "Controls text", "Menu footer" })
{
    var tmp = Txt(name);
    if (tmp == null) continue;
    tmp.font = font; tmp.color = brownMuted; tmp.fontSize = 16;
    tmp.enableAutoSizing = true; tmp.fontSizeMin = 12; tmp.fontSizeMax = 16;
}

var buttonMap = new (string name, Sprite normal, Sprite pressed)[] {
    ("Start restoration", yellow, yellowP),
    ("Enter Arunika", green, greenP),
    ("Resume mission", blue, blueP),
    ("Briefing back", gray, grayP),
    ("Help back", gray, grayP),
    ("Controls button", gray, grayP),
    ("Pause return menu", gray, grayP),
    ("Victory return menu", gray, grayP),
    ("Play again", gray, grayP),
    ("Menu quit", red, redP),
    ("Pause quit", red, redP),
    ("Victory quit", red, redP)
};

foreach (var (name, normal, pressed) in buttonMap)
{
    var t = FindInactive(name, ui);
    if (t == null) { sb.AppendLine("button missing " + name); continue; }
    var img = t.GetComponent<Image>();
    if (img == null) { sb.AppendLine("button has no image " + name); continue; }
    img.sprite = normal;
    img.type = Image.Type.Sliced;
    img.color = Color.white;
    var btn = t.GetComponent<Button>();
    if (btn != null)
    {
        btn.transition = Selectable.Transition.SpriteSwap;
        btn.targetGraphic = img;
        var st = btn.spriteState;
        st.highlightedSprite = normal;
        st.selectedSprite = normal;
        st.pressedSprite = pressed;
        st.disabledSprite = gray;
        btn.spriteState = st;
    }
    foreach (var tmp in t.GetComponentsInChildren<TMP_Text>(true))
    {
        tmp.font = font;
        tmp.color = Color.white;
        tmp.fontSize = 28;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 16;
        tmp.fontSizeMax = 28;
    }
}

foreach (var name in new[] { "Restoration bar background" })
{
    var t = FindInactive(name, ui);
    var img = t != null ? t.GetComponent<Image>() : null;
    if (img != null) { img.sprite = solid; img.type = Image.Type.Simple; img.color = new Color32(0x7A, 0x5A, 0x3C, 0xFF); }
}
foreach (var name in new[] { "Restoration bar fill" })
{
    var t = FindInactive(name, ui);
    var img = t != null ? t.GetComponent<Image>() : null;
    if (img != null) { img.sprite = solid; img.type = Image.Type.Simple; img.color = new Color32(0x7D, 0xC3, 0x02, 0xFF); }
}
foreach (var name in new[] { "Water contact progress" })
{
    var t = FindInactive(name, ui);
    var img = t != null ? t.GetComponent<Image>() : null;
    if (img != null) { img.sprite = solid; img.type = Image.Type.Simple; img.color = new Color32(0x7A, 0x5A, 0x3C, 0xFF); }
}
foreach (var name in new[] { "Water contact fill" })
{
    var t = FindInactive(name, ui);
    var img = t != null ? t.GetComponent<Image>() : null;
    if (img != null) { img.sprite = solid; img.type = Image.Type.Simple; img.color = new Color32(0x62, 0x89, 0xEB, 0xFF); }
}

var slider = ui.GetComponentsInChildren<Slider>(true).FirstOrDefault();
if (slider != null)
{
    var bg = FindInactive("Volume background", slider.transform);
    if (bg != null)
    {
        var img = bg.GetComponent<Image>();
        if (img != null) { img.sprite = Sp("slider_background_1"); img.type = Image.Type.Simple; img.color = Color.white; }
    }
    var filled = FindInactive("Volume filled", slider.transform);
    if (filled != null)
    {
        var img = filled.GetComponent<Image>();
        if (img != null) { img.sprite = Sp("slider_fill_1"); img.type = Image.Type.Simple; img.color = Color.white; }
    }
    var handle = FindInactive("Volume handle", slider.transform);
    if (handle != null)
    {
        var img = handle.GetComponent<Image>();
        if (img != null) { img.sprite = knob; img.type = Image.Type.Simple; img.color = new Color32(0xFF, 0xE8, 0xC5, 0xFF); }
    }
}

var dotColors = new Dictionary<string, Color32> {
    {"Zone A", new Color32(0x7D, 0xC3, 0x02, 0xFF)},
    {"Zone B", new Color32(0xE6, 0x9D, 0x14, 0xFF)},
    {"Zone C", new Color32(0xF6, 0x3C, 0x30, 0xFF)},
    {"Destination dot", new Color32(0xE6, 0x9D, 0x14, 0xFF)},
    {"Volunteer dot", new Color32(0x62, 0x89, 0xEB, 0xFF)}
};
foreach (var kv in dotColors)
{
    var t = FindInactive(kv.Key, ui);
    var img = t != null ? t.GetComponent<Image>() : null;
    if (img == null) continue;
    img.sprite = knob;
    img.type = Image.Type.Simple;
    img.color = kv.Value;
}

var trail = FindInactive("Trail", ui);
if (trail != null)
{
    var img = trail.GetComponent<Image>();
    if (img != null) img.color = new Color32(0xB0, 0x8A, 0x5E, 0xFF);
}

foreach (var name in new[] { "A label", "B label", "C label" })
{
    var tmp = Txt(name);
    if (tmp == null) continue;
    tmp.font = font; tmp.color = brown; tmp.fontSize = 16; tmp.enableAutoSizing = false;
}

foreach (var name in new[] { "Icon sprout", "Icon tree", "Icon flame", "Icon check" })
{
    var t = FindInactive(name, ui);
    var img = t != null ? t.GetComponent<Image>() : null;
    if (img != null) img.color = brown;
}

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
sb.AppendLine("farm reskin applied");
return sb.ToString();
