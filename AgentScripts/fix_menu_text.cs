using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
var ui=GameObject.Find("UI - RIMBARA").transform;
TMP_Text FindT(string n, Transform root){
  if(root.name==n) return root.GetComponent<TMP_Text>();
  foreach(Transform c in root){ var r=FindT(n,c); if(r) return r; }
  return null;
}
var story=FindT("Menu story", ui);
if(story){ story.fontSize=18f; story.lineSpacing=-2f; story.characterSpacing=0f; story.ForceMeshUpdate(); }
var footer=FindT("Menu footer", ui);
if(footer){ footer.fontSize=12.5f; footer.color=new Color32(0xB9,0xC7,0xA8,0xFF); }
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
return "menu text fixed";
