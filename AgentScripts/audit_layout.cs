using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

var sb = new StringBuilder();
var ui = GameObject.Find("UI - RIMBARA")?.transform;
if (ui == null) return "no UI";

void Walk(Transform t, int d)
{
    if (d > 4) return;
    var pad = new string(' ', d * 2);
    var rt = t as RectTransform;
    string r = "";
    if (rt != null)
        r = " anc=" + rt.anchorMin + ".." + rt.anchorMax + " piv=" + rt.pivot
          + " pos=" + rt.anchoredPosition + " size=" + rt.sizeDelta
          + " rect=" + Mathf.RoundToInt(rt.rect.width) + "x" + Mathf.RoundToInt(rt.rect.height);
    var tmp = t.GetComponent<TMP_Text>();
    if (tmp != null)
        r += " TMP align=" + tmp.alignment + " valign=" + tmp.verticalAlignment + " overflow=" + tmp.overflowMode + " wrap=" + tmp.enableWordWrapping + " margins=" + tmp.margin + " charSp=" + tmp.characterSpacing + " lineSp=" + tmp.lineSpacing;
    var img = t.GetComponent<Image>();
    if (img != null) r += " IMG ppm=" + img.pixelsPerUnitMultiplier + " fill=" + img.fillMethod + "/" + img.fillAmount;
    sb.AppendLine(pad + t.name + r);
    foreach (Transform c in t) Walk(c, d + 1);
}

Walk(ui, 0);
return sb.ToString();
