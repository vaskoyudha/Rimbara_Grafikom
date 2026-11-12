using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

var sb = new StringBuilder();
var ui = GameObject.Find("UI - RIMBARA").transform;
var t = ui.GetComponentsInChildren<Transform>(true).First(c => c.name == "Main menu card");
var img = t.GetComponent<Image>();
sb.AppendLine("img type=" + img.type + " sprite=" + (img.sprite != null ? img.sprite.name + " border=" + img.sprite.border : "null") + " color=" + img.color + " alpha=" + img.color.a + " maskable=" + img.maskable + " mat=" + (img.material != null ? img.material.name : "null"));
for (var p = t; p != null; p = p.parent)
{
    var m = p.GetComponent<Mask>(); var rm = p.GetComponent<RectMask2D>(); var cv = p.GetComponent<Canvas>();
    if (m != null || rm != null || cv != null) sb.AppendLine("parent " + p.name + " mask=" + (m != null) + " rectMask=" + (rm != null) + " canvas=" + (cv != null));
}
foreach (var eff in t.GetComponents<BaseMeshEffect>())
{
    sb.AppendLine("effect " + eff.GetType().Name + " enabled=" + eff.enabled + " active=" + eff.gameObject.activeInHierarchy);
    var so = new UnityEditor.SerializedObject(eff);
    var it = so.GetIterator();
    while (it.NextVisible(true)) if (it.name.StartsWith("m_Effect") || it.name == "m_UseGraphicAlpha") sb.AppendLine("   " + it.name + " = " + it.propertyType + " " + (it.propertyType == UnityEditor.SerializedPropertyType.Color ? it.colorValue.ToString() : it.propertyType == UnityEditor.SerializedPropertyType.Vector2 ? it.vector2Value.ToString() : it.boolValue.ToString()));
}
return sb.ToString();
