using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
var ui = GameObject.Find("UI - RIMBARA").transform;
Transform FindInactive(string name, Transform root)
{
    if (root.name == name) return root;
    foreach (Transform c in root) { var r = FindInactive(name, c); if (r != null) return r; }
    return null;
}
var t = FindInactive("Pause return menu label", ui);
var tmp = t.GetComponent<TMP_Text>();
tmp.overflowMode = TextOverflowModes.Truncate;
tmp.enableAutoSizing = true;
tmp.fontSizeMax = 25f;
tmp.fontSizeMin = 16f;
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
return "pause label capped at 25, truncate";
