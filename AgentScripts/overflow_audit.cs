using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
var sb=new StringBuilder();
var ui=GameObject.Find("UI - RIMBARA").transform;
foreach(var tmp in ui.GetComponentsInChildren<TMP_Text>(true)){
  if(tmp.gameObject.name.StartsWith("TMP SubMesh")) continue;
  var rt=(RectTransform)tmp.transform;
  if(rt.rect.width<1||rt.rect.height<1) continue;
  var pref=tmp.GetPreferredValues(tmp.text, rt.rect.width, 0f);
  var raw=tmp.GetPreferredValues(tmp.text);
  bool over = pref.y > rt.rect.height + 2f || (tmp.textWrappingMode==TextWrappingModes.NoWrap && raw.x > rt.rect.width + 2f);
  if(over) sb.AppendLine("OVERFLOW "+tmp.name+" rect="+Mathf.RoundToInt(rt.rect.width)+"x"+Mathf.RoundToInt(rt.rect.height)+" need="+Mathf.RoundToInt(pref.x)+"x"+Mathf.RoundToInt(pref.y)+" size="+tmp.fontSize+" wrap="+tmp.textWrappingMode);
}
// sibling rect overlaps inside each screen container
string[] screens={"Main menu","Briefing screen","Controls screen","Pause screen","Ending screen","Gameplay HUD"};
foreach(var sn in screens){
  var s=ui.Find(sn); if(s==null) continue;
  var cards=new System.Collections.Generic.List<RectTransform>();
  foreach(Transform c in s){ var rt=c as RectTransform; if(rt!=null) cards.Add(rt); }
  foreach(var card in cards){
    var kids=new System.Collections.Generic.List<RectTransform>();
    foreach(Transform c in card){ var rt=c as RectTransform; if(rt!=null && c.gameObject.activeSelf && rt.GetComponent<Image>()!=null || (rt!=null&&c.gameObject.activeSelf&&rt.GetComponent<TMP_Text>()!=null)) kids.Add(rt); }
    for(int i=0;i<kids.Count;i++) for(int j=i+1;j<kids.Count;j++){
      var a=kids[i]; var b=kids[j];
      if(a.anchorMin!=b.anchorMin) continue;
      var ar=new Rect(a.anchoredPosition.x, a.anchoredPosition.y-a.rect.height, a.rect.width, a.rect.height);
      var br=new Rect(b.anchoredPosition.x, b.anchoredPosition.y-b.rect.height, b.rect.width, b.rect.height);
      float ox=Mathf.Min(ar.xMax,br.xMax)-Mathf.Max(ar.xMin,br.xMin);
      float oy=Mathf.Min(ar.yMax,br.yMax)-Mathf.Max(ar.yMin,br.yMin);
      if(ox>2 && oy>2) sb.AppendLine("OVERLAP "+card.name+": "+a.name+" <> "+b.name+" by "+Mathf.RoundToInt(ox)+"x"+Mathf.RoundToInt(oy));
    }
  }
}
return sb.Length==0?"CLEAN":sb.ToString();
