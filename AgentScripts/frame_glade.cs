using UnityEditor;
var sv=SceneView.lastActiveSceneView;
var target=new UnityEngine.Vector3(20f,1f,-60f);
if(sv!=null){ sv.LookAt(target, UnityEngine.Quaternion.Euler(28f,200f,0f), 55f); sv.Repaint(); }
return "framed="+(sv!=null);
