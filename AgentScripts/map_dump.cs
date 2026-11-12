using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
var sb=new StringBuilder();
var ui=GameObject.Find("UI - RIMBARA").transform;
Transform F(string n, Transform r){ if(r.name==n) return r; foreach(Transform c in r){var x=F(n,c); if(x!=null) return x;} return null; }
foreach(var cn in new[]{"Arunika field map","Controls reminder","Next destination","Feedback toast","Interaction prompt","Main menu card","Mission card","Restoration counters"}){
  var t=F(cn,ui); if(t==null) continue;
  var rt=(RectTransform)t; var img=t.GetComponent<Image>();
  sb.AppendLine("== "+cn+" rect="+rt.rect.width+"x"+rt.rect.height+" img="+(img?img.sprite.name+"/"+img.type+"/ppm"+img.pixelsPerUnitMultiplier:"none"));
  foreach(Transform c in t){
    var crt=c as RectTransform; if(crt==null) continue;
    var ci=c.GetComponent<Image>(); var ct=c.GetComponent<TMP_Text>();
    sb.AppendLine("   "+c.name+" size="+Mathf.RoundToInt(crt.rect.width)+"x"+Mathf.RoundToInt(crt.rect.height)+" pos="+Mathf.RoundToInt(crt.anchoredPosition.x)+","+Mathf.RoundToInt(crt.anchoredPosition.y)+" anc="+crt.anchorMin+" piv="+crt.pivot
      +(ci!=null?" img="+(ci.sprite!=null?ci.sprite.name:"null")+"/"+ci.type+"/f="+ci.fillAmount:"")
      +(ct!=null?" txt="+(ct.font!=null?ct.font.name:"null")+" "+ct.fontSize+" align="+ct.alignment+" wrap="+ct.textWrappingMode:""));
    foreach(Transform gc in c){ var grt=gc as RectTransform; if(grt==null) continue;
      var gi=gc.GetComponent<Image>(); var gt=gc.GetComponent<TMP_Text>();
      sb.AppendLine("      "+gc.name+" size="+Mathf.RoundToInt(grt.rect.width)+"x"+Mathf.RoundToInt(grt.rect.height)+" pos="+Mathf.RoundToInt(grt.anchoredPosition.x)+","+Mathf.RoundToInt(grt.anchoredPosition.y)
        +(gi!=null?" img="+(gi.sprite!=null?gi.sprite.name:"null")+"/"+gi.type:"")+(gt!=null?" txt="+gt.fontSize:""));
    }
  }
}
return sb.ToString();
