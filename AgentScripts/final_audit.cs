using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
var sb=new StringBuilder();
var ui=GameObject.Find("UI - RIMBARA").transform;
int bad=0;
foreach(var tmp in ui.GetComponentsInChildren<TMP_Text>(true)){
  if(tmp.gameObject.name.StartsWith("TMP SubMesh")) continue;
  var rt=(RectTransform)tmp.transform;
  if(rt.rect.width<1||rt.rect.height<1) continue;
  var b=tmp.textBounds.size;
  bool over=b.x>rt.rect.width+3f || b.y>rt.rect.height+3f;
  bool trunc=tmp.overflowMode==TextOverflowModes.Truncate && (tmp.preferredWidth>rt.rect.width+3f||tmp.preferredHeight>rt.rect.height+3f);
  if(over||trunc){ bad++; sb.AppendLine("STILL "+tmp.name+" rect="+Mathf.RoundToInt(rt.rect.width)+"x"+Mathf.RoundToInt(rt.rect.height)+" bounds="+Mathf.RoundToInt(b.x)+"x"+Mathf.RoundToInt(b.y)+" pref="+Mathf.RoundToInt(tmp.preferredWidth)+"x"+Mathf.RoundToInt(tmp.preferredHeight)+" size="+tmp.fontSize+" min="+tmp.fontSizeMin); }
}
sb.AppendLine("issues="+bad);
return sb.ToString();
