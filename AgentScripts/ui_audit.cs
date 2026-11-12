using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
var sb=new StringBuilder();
sb.AppendLine("SCENE " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
var ui=GameObject.Find("UI - RIMBARA")?.transform;
if(ui==null){ return "no UI - RIMBARA: " + sb; }
var cs=ui.GetComponentInChildren<CanvasScaler>(true);
if(cs!=null) sb.AppendLine("CanvasScaler mode="+cs.uiScaleMode+" ref="+cs.referenceResolution+" match="+cs.matchWidthOrHeight);
sb.AppendLine("-- TEXT --");
foreach(var t in ui.GetComponentsInChildren<TMP_Text>(true)){
  var rt=(RectTransform)t.transform;
  string txt=t.text.Length>22?t.text.Substring(0,22):t.text;
  txt=txt.Replace("\n"," / ");
  sb.AppendLine(t.name+" | font="+(t.font?t.font.name:"null")+" size="+t.fontSize+" color=#"+ColorUtility.ToHtmlStringRGB(t.color)+" rect="+Mathf.RoundToInt(rt.rect.width)+"x"+Mathf.RoundToInt(rt.rect.height)+" | "+txt);
}
sb.AppendLine("-- IMAGES --");
foreach(var i in ui.GetComponentsInChildren<Image>(true)){
  sb.AppendLine(i.name+" | sprite="+(i.sprite?i.sprite.name:"null")+" type="+i.type+" color=#"+ColorUtility.ToHtmlStringRGB(i.color)+" rect="+Mathf.RoundToInt(((RectTransform)i.transform).rect.width)+"x"+Mathf.RoundToInt(((RectTransform)i.transform).rect.height));
}
return sb.ToString();
