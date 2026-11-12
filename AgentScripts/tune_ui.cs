using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

var sb = new StringBuilder();
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.name != "Arunika") throw new System.Exception("wrong scene " + scene.name);

var ui = GameObject.Find("UI - RIMBARA").transform;
Sprite Tile(string n) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/External/KenneyPixelUI/TilesUsed/tile_" + n + ".png");
var sGold = Tile("13");
var sCream = Tile("0");

int frames = 0;
foreach (var img in ui.GetComponentsInChildren<Image>(true))
{
    if (img.gameObject.name != "Pixel frame") continue;
    img.pixelsPerUnitMultiplier = 0.5f;
    var rt = (RectTransform)img.transform;
    rt.offsetMin = new Vector2(-4f, -4f);
    rt.offsetMax = new Vector2(4f, 4f);
    frames++;
}
sb.AppendLine("frames tuned=" + frames);

string[] creamBtns = { "Start restoration", "Enter Arunika", "Help back", "Resume mission" };
foreach (var b in ui.GetComponentsInChildren<Button>(true))
{
    var img = b.GetComponent<Image>();
    if (creamBtns.Contains(b.name))
    {
        img.sprite = b.name == "Start restoration" ? sGold : sCream;
        img.color = Color.white;
        foreach (var t in b.GetComponentsInChildren<TMP_Text>(true)) t.color = new Color32(0x3A, 0x24, 0x12, 0xFF);
    }
    else
    {
        img.sprite = Tile("2");
        img.color = new Color(0.68f, 0.75f, 0.82f, 1f);
        foreach (var t in b.GetComponentsInChildren<TMP_Text>(true)) t.color = new Color32(0xFF, 0xF1, 0xD2, 0xFF);
    }
    img.type = Image.Type.Sliced;
}

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
sb.AppendLine("tuned");
return sb.ToString();
