using System.IO;
using UnityEngine;
using UnityEngine.UI;

var mode = File.ReadAllText("AgentScripts/_show.txt").Trim();
var ui = GameObject.Find("UI - RIMBARA").transform;
string[] screens = { "Main menu", "Briefing screen", "Controls screen", "Pause screen", "Ending screen", "Gameplay HUD" };
foreach (var s in screens)
{
    var t = ui.Find(s);
    if (t != null) t.gameObject.SetActive(false);
}
var dimmer = ui.Find("Menu dimmer");
if (dimmer != null) dimmer.gameObject.SetActive(mode != "gameplay");

string target = mode switch
{
    "briefing" => "Briefing screen",
    "controls" => "Controls screen",
    "pause" => "Pause screen",
    "ending" => "Ending screen",
    "gameplay" => "Gameplay HUD",
    _ => "Main menu"
};
var go = ui.Find(target);
if (go == null) return "missing " + target;
go.gameObject.SetActive(true);
Time.timeScale = mode == "gameplay" ? 1f : 0f;
return "showing " + target;
