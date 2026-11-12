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
TMP_Text T(string n)
{
    var t = FindInactive(n, ui);
    return t == null ? null : t.GetComponent<TMP_Text>();
}

var card = FindInactive("Main menu card", ui);
if (card == null) throw new System.Exception("no main menu card");

const float L = 38f, W = 584f;
var amberDark = new Color32(0x8A, 0x5A, 0x16, 0xFF);
var subBrown = new Color32(0x6B, 0x44, 0x23, 0xFF);

var eyebrow = T("Menu eyebrow");
var head = T("RIMBARA title");
var banner = card.Find("Title banner");
var subtitle = T("Game subtitle");
var story = T("Menu story");
var scope = T("Menu scope");
var start = FindInactive("Start restoration", ui);
var row1 = FindInactive("Controls button", ui);
var row2 = FindInactive("Menu quit", ui);
var footer = T("Menu footer");
if (eyebrow == null || head == null || banner == null || subtitle == null || story == null
    || scope == null || start == null || row1 == null || row2 == null || footer == null)
    throw new System.Exception("menu element missing");

void Place(Transform t, float top, float h, float x = L, float w = W)
{
    var rt = (RectTransform)t;
    rt.anchorMin = new Vector2(0f, 1f);
    rt.anchorMax = new Vector2(0f, 1f);
    rt.pivot = new Vector2(0f, 1f);
    rt.sizeDelta = new Vector2(w, h);
    rt.anchoredPosition = new Vector2(x, -top);
}

float cursor = 24f;

// eyebrow
Place(eyebrow.transform, cursor, 26f);
eyebrow.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
    "Assets/maanetorn/Farm Game UI - Simple 2D UI/Font/LilitaOne-RegularSDF.asset");
eyebrow.fontSize = 17f;
eyebrow.color = amberDark;
eyebrow.alignment = TextAlignmentOptions.Left;
eyebrow.enableAutoSizing = false;
cursor += 24f + 12f;

// title: full-width ribbon, centred display type
float headH = 112f;
Place(head.transform, cursor, headH);
head.fontSize = 82f;
head.alignment = TextAlignmentOptions.Center;
head.enableAutoSizing = true;
head.fontSizeMax = 82f;
head.fontSizeMin = 56f;
var brt = (RectTransform)banner;
brt.anchorMin = new Vector2(0f, 1f);
brt.anchorMax = new Vector2(0f, 1f);
brt.pivot = new Vector2(0.5f, 0.5f);
brt.sizeDelta = new Vector2(W, headH + 18f);
brt.anchoredPosition = new Vector2(L + W * 0.5f, -(cursor + headH * 0.5f));
cursor += headH + 16f;

// subtitle: a real second level, not a third eyebrow
Place(subtitle.transform, cursor, 40f);
subtitle.fontSize = 27f;
subtitle.color = subBrown;
subtitle.alignment = TextAlignmentOptions.Left;
subtitle.characterSpacing = 3f;
subtitle.enableAutoSizing = true;
subtitle.fontSizeMax = 27f;
subtitle.fontSizeMin = 20f;
cursor += 34f + 22f;

// body copy
float storyH = Mathf.Max(142f, ((RectTransform)story.transform).rect.height);
Place(story.transform, cursor, storyH);
story.alignment = TextAlignmentOptions.TopLeft;
cursor += storyH + 18f;

// stats line
Place(scope.transform, cursor, 28f);
scope.fontSize = 17f;
scope.color = amberDark;
scope.alignment = TextAlignmentOptions.Left;
scope.enableAutoSizing = false;
cursor += 24f + 30f;

// action group
Place(start.transform, cursor, 88f);
cursor += 88f + 20f;
Place(row1.transform, cursor, 76f, L, 282f);
Place(row2.transform, cursor, 76f, L + 302f, 282f);
cursor += 76f + 24f;

Place(footer.transform, cursor, 40f);
footer.color = new Color32(0x7A, 0x55, 0x38, 0xFF);
footer.fontSize = 16f;

sb.AppendLine("menu relaid out, bottom=" + (cursor + 34f) + " of 712");
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
return sb.ToString();
