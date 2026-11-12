using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
var sb=new StringBuilder();
Transform Find(string n, Transform root){
  if(root.name==n) return root;
  foreach(Transform c in root){ var r=Find(n,c); if(r) return r; }
  return null;
}
var ui=GameObject.Find("UI - RIMBARA").transform;
var panel=Find("Restoration counters", ui);
sb.AppendLine("panel="+(panel?panel.name:"null"));
if(panel){
  foreach(var tmp in panel.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true)){
    var rt=tmp.rectTransform;
    sb.AppendLine($"{tmp.name} align={tmp.alignment} margin={tmp.margin} pivot={rt.pivot} ap={rt.anchoredPosition} size={rt.rect.size} localPos={rt.localPosition}");
  }
}
return sb.ToString();
