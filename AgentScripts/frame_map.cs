using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

var sb = new System.Text.StringBuilder();
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.name != "Arunika") throw new System.Exception("wrong scene " + scene.name);

var overview = GameObject.Find("Cinemachine - Arunika overview");
if (overview != null)
{
    overview.transform.position = new Vector3(42f, 30f, -58f);
    overview.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 2f, -6f) - overview.transform.position, Vector3.up);
    sb.AppendLine("overview placed");
}

var sv = SceneView.lastActiveSceneView;
if (sv != null)
{
    sv.LookAt(new Vector3(0f, 4f, -8f), Quaternion.Euler(52f, 0f, 0f), 95f);
    sv.Repaint();
    sb.AppendLine("scene view framed");
}

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
sb.AppendLine("SAVED");
return sb.ToString();
