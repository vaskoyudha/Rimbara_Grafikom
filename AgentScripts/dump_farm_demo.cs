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
foreach (var root in demo.GetRootGameObjects())
{
    sb.AppendLine("ROOT " + root.name);
    Dump(root.transform, 1, sb);
}
EditorSceneManager.CloseScene(demo, true);
return sb.ToString();

void Dump(Transform t, int depth, StringBuilder b)
{
    if (depth > 5) return;
    var pad = new string(' ', depth * 2);
    var go = t.gameObject;
    var rt = t as RectTransform;
    string size = rt != null ? rt.rect.width + "x" + rt.rect.height : "";
    string extra = "";
    var tmp = go.GetComponent<TMP_Text>();
    if (tmp != null) extra += " TMP:" + (tmp.fontAsset != null ? tmp.fontAsset.name : "?") + " sz=" + tmp.fontSize + " col=" + ColorUtility.ToHtmlStringRGB(tmp.color) + " txt=" + Short(tmp.text);
    var img = go.GetComponent<Image>();
    if (img != null) extra += " IMG:" + (img.sprite != null ? img.sprite.name : "null") + " type=" + img.type + " col=" + ColorUtility.ToHtmlStringRGB(img.color);
    if (go.GetComponent<Button>() != null) extra += " BTN";
    if (go.GetComponent<Slider>() != null) extra += " SLIDER";
    b.AppendLine(pad + go.name + " [" + size + "]" + extra);
    foreach (Transform c in t) Dump(c, depth + 1, b);
}

string Short(string s) { return s == null ? "" : (s.Length > 40 ? s.Substring(0, 40) : s).Replace("\n", " / "); }
