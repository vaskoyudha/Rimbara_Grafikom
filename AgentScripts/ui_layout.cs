using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
var sb=new StringBuilder();
var panel=GameObject.Find("Restoration counters");
foreach(var tmp in panel.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true)){
  var rt=tmp.rectTransform;
  sb.AppendLine($"{tmp.name} align={tmp.alignment} margin={tmp.margin} anchorMin={rt.anchorMin} anchorMax={rt.anchorMax} pivot={rt.pivot} ap={rt.anchoredPosition} size={rt.rect.size} pos={rt.position}");
}
var card=GameObject.Find("Mission card");
foreach(var tmp in card.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true)){
  var rt=tmp.rectTransform;
  sb.AppendLine($"{tmp.name} align={tmp.alignment} ap={rt.anchoredPosition} size={rt.rect.size} pos={rt.position}");
}
return sb.ToString();
