using System.Text;
using UnityEngine;
using UnityEngine.UI;
var ui=GameObject.Find("UI - RIMBARA").transform;
Transform F(string n, Transform r){ if(r.name==n) return r; foreach(Transform c in r){var x=F(n,c); if(x!=null) return x;} return null; }
var sb=new StringBuilder();
foreach(var cardName in new[]{"Mission card","Restoration counters","Volunteer briefing","How to play card","Pause card","Restoration ending"}){
  var t=F(cardName,ui); if(t==null){sb.AppendLine("MISSING "+cardName);continue;}
  var crt=(RectTransform)t;
  sb.AppendLine("== "+cardName+" size="+Mathf.RoundToInt(crt.rect.width)+"x"+Mathf.RoundToInt(crt.rect.height));
  foreach(Transform c in t){
    var rt=c as RectTransform; if(rt==null) continue;
    sb.AppendLine("   "+c.name+" size="+Mathf.RoundToInt(rt.rect.width)+"x"+Mathf.RoundToInt(rt.rect.height)+" pos="+Mathf.RoundToInt(rt.anchoredPosition.x)+","+Mathf.RoundToInt(rt.anchoredPosition.y)+" anc="+rt.anchorMin+"-"+rt.anchorMax+" piv="+rt.pivot);
  }
}
return sb.ToString();
