using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

var sb = new StringBuilder();
var scenePath = "Assets/maanetorn/Farm Game UI - Simple 2D UI/DemoScene/FGDemoScene.unity";
var demo = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
if (!demo.IsValid()) return "demo scene failed to open";
sb.AppendLine("scene=" + demo.name);
var scaler = Object.FindFirstObjectByType<CanvasScaler>(FindObjectsInactive.Include);
if (scaler != null) sb.AppendLine("scaler mode=" + scaler.uiScaleMode + " ref=" + scaler.referenceResolution + " match=" + scaler.matchWidthOrHeight);
foreach (var root in demo.GetRootGameObjects())
{
    sb.AppendLine("ROOT " + root.name);
    Dump(root.transform, 1, sb);
}
EditorSceneManager.CloseScene(demo, true);
return sb.ToString();

void Dump(Transform t, int depth, StringBuilder b)
{
    if (depth > 7) return;
    var pad = new string(' ', depth * 2);
    var go = t.gameObject;
    var rt = t as RectTransform;
    string size = rt != null ? Mathf.RoundToInt(rt.rect.width) + "x" + Mathf.RoundToInt(rt.rect.height) : "";
    string extra = "";
    var tmp = go.GetComponent<TMP_Text>();
    if (tmp != null) extra += " TMP:" + (tmp.font != null ? tmp.font.name : "?") + " sz=" + tmp.fontSize + " col=" + ColorUtility.ToHtmlStringRGB(tmp.color) + " align=" + tmp.alignment + " txt=" + Short(tmp.text);
    var img = go.GetComponent<Image>();
    if (img != null) extra += " IMG:" + (img.sprite != null ? img.sprite.name : "null") + " type=" + img.type + " col=" + ColorUtility.ToHtmlStringRGB(img.color) + " ppm=" + img.pixelsPerUnitMultiplier;
    if (go.GetComponent<Button>() != null) extra += " BTN";
    if (go.GetComponent<Slider>() != null) extra += " SLIDER";
    b.AppendLine(pad + go.name + " [" + size + "]" + extra);
    foreach (Transform c in t) Dump(c, depth + 1, b);
}

string Short(string s) { return s == null ? "" : (s.Length > 60 ? s.Substring(0, 60) : s).Replace("\n", " / "); }
