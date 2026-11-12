using UnityEngine;
using UnityEditor;
var sv = SceneView.lastActiveSceneView;
if (sv != null) { sv.LookAt(new Vector3(0f, 2f, 18f), Quaternion.Euler(18f, 0f, 0f), 70f, true); sv.Repaint(); }
return "framed";
