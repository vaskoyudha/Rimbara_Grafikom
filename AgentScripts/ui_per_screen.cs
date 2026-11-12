using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

var sb = new StringBuilder();
var ui = GameObject.Find("UI - RIMBARA").transform;
string[] modes = { "menu", "gameplay", "briefing", "controls", "pause", "ending" };
string Target(string m) => m switch {
    "briefing" => "Briefing screen", "controls" => "Controls screen", "pause" => "Pause screen",
    "ending" => "Ending screen", "gameplay" => "Gameplay HUD", _ => "Main menu" };
var dimmer = ui.Find("Menu dimmer");
foreach (var mode in modes)
{
    foreach (var child in ui.Cast<Transform>().Where(t => t.name.EndsWith("screen") || t.name == "Main menu" || t.name == "Gameplay HUD")) child.gameObject.SetActive(false);
    var screen = ui.Find(Target(mode));
    screen.gameObject.SetActive(true);
    dimmer.gameObject.SetActive(mode != "gameplay");
    Time.timeScale = mode == "gameplay" ? 1f : 0f;
    await Task.Delay(250);
    int items = 0, protrude = 0, overflowFlag = 0;
    var detail = new StringBuilder();
    foreach (var tmp in screen.GetComponentsInChildren<TMP_Text>(true))
    {
        if (tmp.gameObject.name.StartsWith("TMP SubMesh")) continue;
        var rt = (RectTransform)tmp.transform;
        if (rt.rect.width < 1 || rt.rect.height < 1) continue;
        items++;
        var tb = tmp.textBounds.size;
        bool prot = tb.x > rt.rect.width + 3f || tb.y > rt.rect.height + 3f;
        if (prot) protrude++;
        if (tmp.isTextOverflowing) overflowFlag++;
        if (prot || tmp.isTextOverflowing)
            detail.AppendLine("   " + tmp.name + " rect=" + Mathf.RoundToInt(rt.rect.width) + "x" + Mathf.RoundToInt(rt.rect.height) + " bounds=" + Mathf.RoundToInt(tb.x) + "x" + Mathf.RoundToInt(tb.y) + " overflowFlag=" + tmp.isTextOverflowing + " mode=" + tmp.overflowMode);
    }
    sb.AppendLine(mode + " active=" + screen.gameObject.activeSelf + " dimmer=" + dimmer.gameObject.activeSelf + " texts=" + items + " protrude=" + protrude + " flagged=" + overflowFlag);
    sb.Append(detail);
    // buttons inside screen
    foreach (var b in screen.GetComponentsInChildren<Button>(true))
    {
        var rt = (RectTransform)b.transform;
        Vector3[] c = new Vector3[4]; rt.GetWorldCorners(c);
        var br = RectTransformUtility.PixelAdjustRect(rt, ui.GetComponentInParent<Canvas>());
        if (br.xMin < -1 || br.xMax > ((RectTransform)ui).rect.width + 1) sb.AppendLine("   BUTTON OUTSIDE " + b.name + " x=" + br.xMin + ".." + br.xMax);
    }
}
Time.timeScale = 1f;
return sb.ToString();
