using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
var sb=new StringBuilder();
var demo=EditorSceneManager.OpenScene("Assets/maanetorn/Farm Game UI - Simple 2D UI/DemoScene/FGDemoScene.unity", OpenSceneMode.Additive);
foreach(var root in demo.GetRootGameObjects()){
  sb.AppendLine("ROOT "+root.name);
  foreach(var t in root.GetComponentsInChildren<Transform>(true)){
    var rt=t as RectTransform;
    var tmp=t.GetComponent<TMP_Text>();
    var img=t.GetComponent<Image>();
    string info="  "+t.name;
    if(rt!=null) info+=" | rect="+Mathf.RoundToInt(rt.rect.width)+"x"+Mathf.RoundToInt(rt.rect.height)+" pos="+Mathf.RoundToInt(rt.anchoredPosition.x)+","+Mathf.RoundToInt(rt.anchoredPosition.y);
    if(img!=null) info+=" | img="+(img.sprite?img.sprite.name:"null")+" color=#"+ColorUtility.ToHtmlStringRGB(img.color)+" type="+img.type;
    if(tmp!=null) info+=" | font="+(tmp.font?tmp.font.name:"null")+" size="+tmp.fontSize+" color=#"+ColorUtility.ToHtmlStringRGB(tmp.color)+" text="+(tmp.text.Length>20?tmp.text.Substring(0,20):tmp.text);
    sb.AppendLine(info);
  }
}
EditorSceneManager.CloseScene(demo, true);
return sb.ToString();
