using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

var sb = new StringBuilder();
var ui = GameObject.Find("UI - RIMBARA")?.transform;
if (ui == null) return "no UI";

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

// 1. Size each title banner to the real text width, not the text rect width.
string[] headingNames = { "RIMBARA title", "Mission title", "Briefing title", "Help title", "Pause title", "Victory title", "Map title" };
foreach (var hn in headingNames)
{
    var t = FindInactive(hn, ui);
    if (t == null) { sb.AppendLine("missing " + hn); continue; }
    var tmp = t.GetComponent<TMP_Text>();
    var rt = (RectTransform)t;
    var card = t.parent;
    var banner = card.Find("Title banner");
    if (banner == null) { sb.AppendLine("no banner " + hn); continue; }
    var brt = (RectTransform)banner;

    tmp.enableAutoSizing = false;
    var pref = tmp.GetPreferredValues(tmp.text);
    float w = Mathf.Min(rt.rect.width + 20f, pref.x + 46f);
    float h = Mathf.Max(rt.rect.height + 16f, pref.y + 18f);
    brt.sizeDelta = new Vector2(w, h);

    float leftEdge = rt.anchoredPosition.x;
    float centerX;
    if (tmp.alignment == TextAlignmentOptions.Center || tmp.alignment == TextAlignmentOptions.Top || tmp.alignment == TextAlignmentOptions.Midline || tmp.alignment == TextAlignmentOptions.Bottom)
        centerX = leftEdge + rt.rect.width * 0.5f;
    else
        centerX = leftEdge + pref.x * 0.5f;

    var cardRt = (RectTransform)card;
    float minX = 10f + w * 0.5f;
    float maxX = cardRt.rect.width - 10f - w * 0.5f;
    if (maxX >= minX) centerX = Mathf.Clamp(centerX, minX, maxX);

    brt.anchoredPosition = new Vector2(centerX, rt.anchoredPosition.y - rt.rect.height * 0.5f);
    sb.AppendLine(hn + " banner " + Mathf.RoundToInt(w) + "x" + Mathf.RoundToInt(h));
}

// 2. Legible labels: white on saturated buttons, dark brown on the gray ones.
var blue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/maanetorn/Farm Game UI - Simple 2D UI/Sprites/128x128/button_blue_default_1.png");
var blueP = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/maanetorn/Farm Game UI - Simple 2D UI/Sprites/128x128/button_blue_pressed_1.png");
var green = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/maanetorn/Farm Game UI - Simple 2D UI/Sprites/128x128/button_green_default_1.png");
var greenP = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/maanetorn/Farm Game UI - Simple 2D UI/Sprites/128x128/button_green_pressed_1.png");

void Swap(string name, Sprite normal, Sprite pressed)
{
    var t = FindInactive(name, ui);
    if (t == null) { sb.AppendLine("btn missing " + name); return; }
    var img = t.GetComponent<Image>();
    if (img == null) return;
    img.sprite = normal;
    var btn = t.GetComponent<Button>();
    if (btn != null)
    {
        var st = btn.spriteState;
        st.highlightedSprite = normal;
        st.selectedSprite = normal;
        st.pressedSprite = pressed;
        btn.spriteState = st;
    }
}
Swap("Controls button", blue, blueP);
Swap("Play again", green, greenP);

string[] grayButtons = { "Briefing back", "Help back", "Controls button", "Pause return menu", "Victory return menu", "Play again" };
foreach (var bn in grayButtons)
{
    var t = FindInactive(bn, ui);
    if (t == null) continue;
    var img = t.GetComponent<Image>();
    bool isGray = img != null && img.sprite != null && img.sprite.name.StartsWith("button_gray");
    if (!isGray) continue;
    foreach (var tmp in t.GetComponentsInChildren<TMP_Text>(true))
        tmp.color = new Color32(0x4A, 0x35, 0x24, 0xFF);
    sb.AppendLine("dark label on " + bn);
}

return sb.ToString();
