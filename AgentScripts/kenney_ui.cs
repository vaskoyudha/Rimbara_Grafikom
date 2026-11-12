using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

var sb = new StringBuilder();
var scene = SceneManager.GetActiveScene();
if (scene.name != "Arunika") throw new System.Exception("wrong scene " + scene.name);

const string K = "Assets/External/KenneyFonts/TMP/";
TMP_FontAsset F(string n) { return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(K + n + " SDF.asset"); }
var future = F("Kenney Future");
var pixel = F("Kenney Pixel");
var mini = F("Kenney Mini Square");
if (future == null || pixel == null || mini == null) throw new System.Exception("kenney fonts missing");

Sprite Tile(string n) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/External/KenneyPixelUI/TilesUsed/tile_" + n + ".png");
var sCream = Tile("0");
var sSlate = Tile("2");
var sGold = Tile("13");

var ui = GameObject.Find("UI - RIMBARA").transform;
Transform FindInactive(string name, Transform root)
{
    if (root.name == name) return root;
    foreach (Transform c in root) { var r = FindInactive(name, c); if (r != null) return r; }
    return null;
}

var cream = new Color32(0xFF, 0xF1, 0xD2, 0xFF);
var steel = new Color32(0x94, 0xAF, 0xC6, 0xFF);
var softSteel = new Color32(0xC9, 0xD6, 0xDF, 0xFF);
var amber = new Color32(0xC5, 0x87, 0x47, 0xFF);
var red = new Color32(0xE2, 0x66, 0x5B, 0xFF);
var brown = new Color32(0x6D, 0x4B, 0x27, 0xFF);
var panelFill = new Color32(0x30, 0x3D, 0x48, 0xF2);

var display = new Dictionary<string, float> {
    {"RIMBARA title", 70f}, {"Mission title", 25f}, {"Briefing title", 40f},
    {"Help title", 38f}, {"Pause title", 34f}, {"Victory title", 38f}
};
var body = new Dictionary<string, float> {
    {"Mission description", 18f}, {"Seedling counter", 16f}, {"Tree counter", 16f}, {"Fire counter", 16f},
    {"Feedback text", 18f}, {"Controls text", 12f}, {"Menu story", 19f}, {"Menu footer", 12f},
    {"Briefing story", 21f}, {"Briefing steps", 19f}, {"No fail promise", 17f}, {"Help tip", 19f},
    {"Control list", 19f}, {"Pause description", 18f}, {"Victory story", 21f}, {"Mission results", 20f},
    {"Interaction text", 20f}
};
var labels = new Dictionary<string, float> {
    {"Mission number", 13f}, {"Map title", 12f}, {"A label", 12f}, {"B label", 12f}, {"C label", 12f},
    {"Menu eyebrow", 13f}, {"Menu scope", 13f}, {"Briefing eyebrow", 13f}, {"Help eyebrow", 13f},
    {"Pause eyebrow", 13f}, {"Victory eyebrow", 14f}, {"Volume label", 12f}, {"Restoration percent", 13f},
    {"Waypoint instruction", 14f}, {"Briefing targets", 15f}
};
var buttons = new HashSet<string> {
    "Start restoration label", "Controls button label", "Menu quit label", "Enter Arunika label",
    "Briefing back label", "Help back label", "Resume mission label", "Pause return menu label",
    "Pause quit label", "Play again label", "Victory return menu label"
};

foreach (var t in ui.GetComponentsInChildren<TMP_Text>(true))
{
    var n = t.name;
    t.color = cream;
    t.characterSpacing = 0f;
    t.wordSpacing = 0f;
    if (display.TryGetValue(n, out float ds)) { t.font = future; t.fontSize = ds; t.characterSpacing = 1f; }
    else if (body.TryGetValue(n, out float bs)) { t.font = pixel; t.fontSize = bs; }
    else if (labels.TryGetValue(n, out float ls)) { t.font = mini; t.fontSize = ls; t.characterSpacing = 3f; }
    else if (buttons.Contains(n)) { t.font = mini; t.fontSize = 18f; t.characterSpacing = 2f; }
    else if (n == "Game subtitle") { t.font = future; t.fontSize = 15f; t.characterSpacing = 3f; }
    else { t.font = pixel; }
}
sb.AppendLine("fonts mapped");

void C(string n, Color c) { var t = FindInactive(n, ui); if (t == null) return; var x = t.GetComponent<TMP_Text>(); if (x != null) x.color = c; }
C("Game subtitle", amber);
C("Menu scope", amber);
C("Briefing targets", amber);
C("Waypoint instruction", amber);
C("Restoration percent", amber);
C("Mission number", steel);
C("Menu footer", steel);
C("Controls text", steel);
C("No fail promise", softSteel);
C("Volume label", steel);
C("Mission title", cream);
C("Victory title", cream);
C("Menu eyebrow", amber);
C("Briefing eyebrow", amber);
C("Help eyebrow", amber);
C("Pause eyebrow", amber);
C("Victory eyebrow", amber);

string[] panels = {
    "Mission card", "Restoration counters", "Next destination", "Interaction prompt",
    "Feedback toast", "Controls reminder", "Arunika field map", "Main menu card",
    "Volunteer briefing", "How to play card", "Pause card", "Restoration ending"
};
foreach (var pn in panels)
{
    var t = FindInactive(pn, ui);
    if (t == null) continue;
    var img = t.GetComponent<Image>();
    if (img != null) img.color = panelFill;
}

// Bars and slider.
void Img(string n, Color c, bool keepSprite = true)
{
    var t = FindInactive(n, ui); if (t == null) return;
    var img = t.GetComponent<Image>(); if (img == null) return;
    img.color = c;
}
Img("Restoration bar background", new Color32(0x3B, 0x2A, 0x1D, 0xFF));
Img("Water contact progress", new Color32(0x3B, 0x2A, 0x1D, 0xFF));
Img("Restoration bar fill", amber);
Img("Water contact fill", steel);
Img("Volume background", new Color32(0x3B, 0x2A, 0x1D, 0xFF));
var volHandle = FindInactive("Volume handle", ui);
if (volHandle != null) { var hi = volHandle.GetComponent<Image>(); if (hi != null) hi.color = cream; }

// Map dots.
var dotColors = new Dictionary<string, Color> {
    {"Zone A", cream}, {"Zone B", amber}, {"Zone C", red},
    {"Destination dot", amber}, {"Volunteer dot", steel}
};
foreach (var kv in dotColors)
{
    var t = FindInactive(kv.Key, ui);
    if (t == null) continue;
    var img = t.GetComponent<Image>();
    if (img != null) img.color = kv.Value;
}

// Buttons: cream primary, gold confirm, dark steel secondary.
string[] creamBtns = { "Start restoration", "Enter Arunika", "Help back", "Resume mission" };
foreach (var b in ui.GetComponentsInChildren<Button>(true))
{
    var img = b.GetComponent<Image>();
    if (img == null) continue;
    img.type = Image.Type.Sliced;
    var cb = b.colors;
    cb.normalColor = Color.white;
    cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
    cb.pressedColor = new Color(0.84f, 0.84f, 0.84f, 1f);
    cb.selectedColor = Color.white;
    cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.55f);
    b.colors = cb;
    if (b.name == "Start restoration") { img.sprite = sCream; img.color = Color.white; }
    else if (creamBtns.Contains(b.name)) { img.sprite = sGold; img.color = Color.white; }
    else { img.sprite = sSlate; img.color = new Color(0.55f, 0.63f, 0.66f, 1f); }
    var dark = b.name == "Start restoration" || creamBtns.Contains(b.name);
    foreach (var t in b.GetComponentsInChildren<TMP_Text>(true))
    {
        t.color = dark ? brown : cream;
        t.font = mini;
    }
}

// Frame sprites keep pack colours.
foreach (var img in ui.GetComponentsInChildren<Image>(true))
{
    if (img.gameObject.name == "Pixel frame") img.color = Color.white;
}

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
AssetDatabase.SaveAssets();
sb.AppendLine("palette applied");
return sb.ToString();
