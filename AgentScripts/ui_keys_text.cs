using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

var sb = new StringBuilder();
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.name != "Arunika") throw new System.Exception("wrong scene " + scene.name);
var ui = GameObject.Find("UI - RIMBARA").transform;
Transform FindInactive(string name, Transform root)
{
    if (root.name == name) return root;
    foreach (Transform c in root)
    {
        var r = FindInactive(name, c);
        if (r != null) return r;
    }
    return null;
}
var list = FindInactive("Control list", ui)?.GetComponent<TMP_Text>();
if (list != null)
{
    list.text = "WASD                      Move through the forest\n"
              + "MOUSE                   Turn the third-person camera\n"
              + "SHIFT                      Run\n"
              + "SPACE                     Jump\n"
              + "E                              Collect seedlings / plant trees / read the board\n"
              + "LEFT MOUSE          Hold to spray water at the crosshair\n"
              + "ESC                          Pause or resume";
    sb.AppendLine("control list updated; lines=" + list.text.Split('\n').Length);
}
var reminder = FindInactive("Controls text", ui)?.GetComponent<TMP_Text>();
if (reminder != null)
{
    reminder.text = "WASD move   /   SHIFT run   /   SPACE jump   /   E interact   /   LEFT MOUSE water   /   ESC pause";
    sb.AppendLine("reminder updated");
}
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
return sb.ToString();
