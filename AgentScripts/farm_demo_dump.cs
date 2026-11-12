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
    var tmp=t.GetComponent<TMP_Text>();
    var img=t.GetComponent<Image>();
    string info="  "+t.name+" | rect="+Mathf.RoundToInt(((RectTransform)t).rect.width)+"x"+Mathf.RoundToInt(((RectTransform)t).rect.height);
    if(img!=null) info+=" | img="+(img.sprite?img.sprite.name:"null")+" color="+img.color+" type="+img.type;
    if(tmp!=null) info+=" | tmp="+(tmp.font?tmp.font.name:"null")+" size="+tmp.fontSize+" color="+tmp.color+" text="+(tmp.text.Length>18?tmp.text.Substring(0,18):tmp.text);
    sb.AppendLine(info);
  }
}
EditorSceneManager.CloseScene(demo, true);
return sb.ToString();
