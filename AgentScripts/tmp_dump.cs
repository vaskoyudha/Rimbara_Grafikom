using System.Text;
using UnityEngine;
using TMPro;
var sb=new StringBuilder();
var ui=GameObject.Find("UI - RIMBARA").transform;
foreach(var t in ui.GetComponentsInChildren<TMP_Text>(true)){
  var rt=(RectTransform)t.transform;
  string txt = t.text.Length>24 ? t.text.Substring(0,24) : t.text;
  txt = txt.Replace("\n"," / ");
  sb.AppendLine(t.name + " | font=" + (t.font?t.font.name:"null") + " | size=" + t.fontSize + " auto=" + t.enableAutoSizing + " min=" + t.fontSizeMin + " max=" + t.fontSizeMax + " style=" + t.fontStyle + " weight=" + t.fontWeight + " | rect=" + rt.rect.width + "x" + rt.rect.height + " | " + txt);
}
return sb.ToString();
