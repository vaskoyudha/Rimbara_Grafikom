using System.Text;
using UnityEditor;
using UnityEngine;

var sb = new StringBuilder();
string dir = "Assets/maanetorn/Farm Game UI - Simple 2D UI/Sprites/128x128/";
string[] names = {
    "panel_1", "panel_inner_1", "panel_title_1", "panel_title_with_stroke_1",
    "button_yellow_default_1", "button_green_default_1", "button_blue_default_1",
    "button_gray_default_1", "button_red_default_1", "button_purple_default_1",
    "slider_background_1", "slider_fill_1", "icon_sprout"
};
foreach (var n in names)
{
    var s = AssetDatabase.LoadAssetAtPath<Sprite>(dir + n + ".png");
    if (s == null) { sb.AppendLine("MISSING " + n); continue; }
    var tex = s.texture;
    var border = s.border;
    sb.AppendLine(n + " | tex=" + tex.width + "x" + tex.height
        + " rect=" + s.rect.width + "x" + s.rect.height
        + " pivot=" + s.pivot + " ppu=" + s.pixelsPerUnit
        + " border=" + border
        + " packed=" + (s.packed ? "y" : "n"));
}

var solid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/External/KenneyPixelUI/pixel_solid.png");
if (solid != null) sb.AppendLine("pixel_solid | tex=" + solid.texture.width + "x" + solid.texture.height + " rect=" + solid.rect.width + "x" + solid.rect.height + " ppu=" + solid.pixelsPerUnit + " border=" + solid.border);

var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
if (knob != null) sb.AppendLine("Knob | tex=" + knob.texture.width + "x" + knob.texture.height + " rect=" + knob.rect.width + "x" + knob.rect.height + " ppu=" + knob.pixelsPerUnit + " border=" + knob.border);

return sb.ToString();
