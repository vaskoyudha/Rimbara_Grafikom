using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
var sb=new StringBuilder();
var ui=GameObject.Find("UI - RIMBARA")?.transform;
if(ui==null) return "none";
void Walk(Transform t,int d){
  string pad=new string(' ',d*2);
  string extra="";
  var img=t.GetComponent<Image>();
  var tmp=t.GetComponent<TMP_Text>();
  var btn=t.GetComponent<Button>();
  if(img!=null) extra+=" [IMG "+(img.sprite?img.sprite.name:"null")+" "+img.type+" #"+ColorUtility.ToHtmlStringRGB(img.color)+"]";
  if(tmp!=null) extra+=" [TXT "+(tmp.font?tmp.font.name:"null")+" "+tmp.fontSize+" #"+ColorUtility.ToHtmlStringRGB(tmp.color)+"]";
  if(btn!=null) extra+=" [BTN "+btn.transition+"]";
  sb.AppendLine(pad+t.name+extra);
  foreach(Transform c in t) Walk(c,d+1);
}
Walk(ui,0);
return sb.ToString();
