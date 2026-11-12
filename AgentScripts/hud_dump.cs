using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
var sb=new StringBuilder();
var root=GameObject.Find("UI - RIMBARA");
void Dump(Transform t,int d){
  if(d>4) return;
  var img=t.GetComponent<UnityEngine.UI.Image>();
  var tmp=t.GetComponent<TMPro.TextMeshProUGUI>();
  var btn=t.GetComponent<UnityEngine.UI.Button>();
  var slider=t.GetComponent<UnityEngine.UI.Slider>();
  var rt=t as RectTransform;
  string extra="";
  if(img) extra+=$" IMG({(img.sprite?img.sprite.name:"none")},{(img.type)},col={ColorUtility.ToHtmlStringRGBA(img.color)},rt={img.raycastTarget})";
  if(tmp) extra+=$" TMP('{(tmp.text.Length>22?tmp.text.Substring(0,22):tmp.text)}',{tmp.fontSize:F0},col={ColorUtility.ToHtmlStringRGBA(tmp.color)})";
  if(btn) extra+=" BTN";
  if(slider) extra+=" SLIDER";
  if(rt) extra+=$" size={rt.rect.width:F0}x{rt.rect.height:F0}";
  sb.AppendLine(new string(' ',(d)*2)+t.name+extra);
  foreach(Transform c in t) Dump(c,d+1);
}
if(root) Dump(root.transform,0); else sb.AppendLine("NO UI ROOT");
return sb.ToString();
