using System.Text;
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

// Main menu: ribbon spans the 584 content column, sits 9px above the display type.
var menu = FindInactive("Main menu card", ui);
var menuBanner = menu.Find("Title banner") as RectTransform;
if (menuBanner == null) throw new System.Exception("menu banner missing");
menuBanner.anchorMin = new Vector2(0f, 1f);
menuBanner.anchorMax = new Vector2(0f, 1f);
menuBanner.pivot = new Vector2(0f, 1f);
menuBanner.sizeDelta = new Vector2(584f, 130f);
menuBanner.anchoredPosition = new Vector2(38f, -51f);
sb.AppendLine("menu banner = 584x130 at (38,-51)");

// Mission card: restore the banner that the global lookup accidentally moved.
var mission = FindInactive("Mission card", ui);
var missionBanner = mission.Find("Title banner") as RectTransform;
var missionTitle = FindInactive("Mission title", ui) as RectTransform;
if (missionBanner != null && missionTitle != null)
{
    missionBanner.anchorMin = missionTitle.anchorMin;
    missionBanner.anchorMax = missionTitle.anchorMax;
    missionBanner.pivot = new Vector2(0f, 1f);
    missionBanner.sizeDelta = missionTitle.sizeDelta + new Vector2(20f, 18f);
    missionBanner.anchoredPosition = missionTitle.anchoredPosition + new Vector2(-10f, 9f);
    sb.AppendLine("mission banner restored to " + missionBanner.sizeDelta + " at " + missionBanner.anchoredPosition);
}

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
return sb.ToString();
