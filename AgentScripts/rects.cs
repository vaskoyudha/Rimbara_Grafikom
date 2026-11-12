using System.Text;
using UnityEngine;
using UnityEngine.UI;
var sb=new StringBuilder();
foreach(var cardName in new[]{"Main menu card","Mission card","Restoration counters","Volunteer briefing","How to play card","Pause card","Restoration ending"}){
  var card=GameObject.Find(cardName); if(card==null){sb.AppendLine("MISSING "+cardName);continue;}
  var crt=(RectTransform)card.transform;
  sb.AppendLine("== "+cardName+" size="+crt.rect.width+"x"+crt.rect.height+" anchors="+crt.anchorMin+"-"+crt.anchorMax);
  foreach(Transform c in card.transform){
    var rt=c as RectTransform; if(rt==null) continue;
    sb.AppendLine("   "+c.name+" size="+Mathf.RoundToInt(rt.rect.width)+"x"+Mathf.RoundToInt(rt.rect.height)+" pos="+Mathf.RoundToInt(rt.anchoredPosition.x)+","+Mathf.RoundToInt(rt.anchoredPosition.y)+" anc="+rt.anchorMin+"-"+rt.anchorMax+" piv="+rt.pivot);
  }
}
return sb.ToString();
