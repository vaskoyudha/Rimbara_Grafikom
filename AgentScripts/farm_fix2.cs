using System.Text;
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

// Primary yellow button needs dark text for contrast.
var start = FindInactive("Start restoration", ui);
if (start != null)
{
    foreach (var tmp in start.GetComponentsInChildren<TMP_Text>(true))
        tmp.color = new Color32(0x4A, 0x35, 0x24, 0xFF);
    sb.AppendLine("dark label on Start restoration");
}

// Tighten banner height and lift it 2px so the tagline clears it.
string[] headingNames = { "RIMBARA title", "Mission title", "Briefing title", "Help title", "Pause title", "Victory title", "Map title" };
foreach (var hn in headingNames)
{
    var t = FindInactive(hn, ui);
    if (t == null) continue;
    var rt = (RectTransform)t;
    var banner = t.parent.Find("Title banner");
    if (banner == null) continue;
    var brt = (RectTransform)banner;
    brt.sizeDelta = new Vector2(brt.sizeDelta.x, rt.rect.height + 10f);
    brt.anchoredPosition = new Vector2(brt.anchoredPosition.x, rt.anchoredPosition.y - rt.rect.height * 0.5f - 2f);
    sb.AppendLine(hn + " banner h=" + Mathf.RoundToInt(brt.sizeDelta.y));
}

return sb.ToString();
