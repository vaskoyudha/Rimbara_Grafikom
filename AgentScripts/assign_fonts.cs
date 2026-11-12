using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

var sb = new StringBuilder();
var scene = SceneManager.GetActiveScene();
if (scene.name != "Arunika") throw new System.Exception("wrong scene " + scene.name);

const string D = "Assets/External/Fonts/TMP/";
TMP_FontAsset F(string n)
{
    var a = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(D + n + " SDF.asset");
    if (a == null) throw new System.Exception("font missing " + n);
    return a;
}
var nunitoRegular = F("Nunito-Regular");
var nunitoSemi = F("Nunito-SemiBold");
var nunitoBold = F("Nunito-Bold");
var nunitoExtra = F("Nunito-ExtraBold");
var silkBold = F("Silkscreen-Bold");
var silkRegular = F("Silkscreen-Regular");

var ui = GameObject.Find("UI - RIMBARA").transform;

var display = new HashSet<string> {
    "RIMBARA title", "Mission title", "Briefing title", "Help title", "Pause title", "Victory title"
};
var paragraph = new HashSet<string> {
    "Menu story", "Briefing story", "Briefing steps", "No fail promise", "Help tip", "Control list",
    "Mission description", "Pause description", "Victory story", "Mission results", "Menu footer",
    "Feedback text", "Controls text", "Menu scope", "Briefing targets", "Waypoint instruction"
};

int changed = 0;
foreach (var t in ui.GetComponentsInChildren<TMP_Text>(true))
{
    var nm = t.name;
    if (nm == "Game subtitle")
    {
        t.font = silkBold;
        t.fontSize = 15f;
        t.characterSpacing = 2f;
        changed++;
        continue;
    }
    if (display.Contains(nm))
    {
        t.font = silkBold;
        t.characterSpacing = 1f;
        t.wordSpacing = 0f;
        if (nm == "RIMBARA title") { t.fontSize = 76f; t.characterSpacing = 0f; }
        if (nm == "Mission title") t.fontSize = 26f;
        if (nm == "Help title") t.fontSize = 42f;
        if (nm == "Pause title") t.fontSize = 38f;
        if (nm == "Victory title") t.fontSize = 42f;
        if (nm == "Briefing title") t.fontSize = 44f;
        changed++;
        continue;
    }
    if (paragraph.Contains(nm))
    {
        t.font = t.fontStyle == FontStyles.Bold ? nunitoBold : nunitoRegular;
    }
    else
    {
        t.font = t.fontStyle == FontStyles.Bold ? nunitoBold : nunitoSemi;
    }
    changed++;
}

var title = ui.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(x => x.name == "RIMBARA title");
if (title != null)
{
    sb.AppendLine("title children:");
    foreach (Transform c in title.transform)
        sb.AppendLine("  child " + c.name + " components=" + string.Join(",", c.GetComponents<Component>().Select(x => x.GetType().Name)));
    var parent = title.transform.parent;
    sb.AppendLine("title parent=" + parent.name);
    foreach (Transform c in parent)
        sb.AppendLine("  sibling " + c.name + " active=" + c.gameObject.activeSelf);
}

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
AssetDatabase.SaveAssets();
sb.AppendLine("fonts assigned=" + changed);
return sb.ToString();
