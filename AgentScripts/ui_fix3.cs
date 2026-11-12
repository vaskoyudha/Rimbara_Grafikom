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

var fontRegular = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/maanetorn/Farm Game UI - Simple 2D UI/Font/LilitaOne-RegularSDF.asset");

// 1. Consistent optical corner radius for every nine-sliced card.
foreach (var img in ui.GetComponentsInChildren<Image>(true))
{
    if (img.sprite == null) continue;
    if (img.sprite.name != "panel_1" && img.sprite.name != "panel_inner_1") continue;
    var rt = (RectTransform)img.transform;
    float hmin = Mathf.Min(rt.rect.width, rt.rect.height);
    float m = Mathf.Clamp(30f / (0.18f * hmin), 1f, 3.2f);
    img.pixelsPerUnitMultiplier = Mathf.Round(m * 4f) / 4f;
}

// 2. Buttons: keep the 9-slice border proportional to the button height.
foreach (var btn in ui.GetComponentsInChildren<Button>(true))
{
    var img = btn.targetGraphic as Image;
    if (img == null) img = btn.GetComponent<Image>();
    if (img == null || img.sprite == null || !img.sprite.name.StartsWith("button_")) continue;
    var rt = (RectTransform)img.transform;
    float m = Mathf.Clamp(30f / (0.22f * rt.rect.height), 1.4f, 3.2f);
    img.pixelsPerUnitMultiplier = Mathf.Round(m * 4f) / 4f;
}

// 3. Progress bars must be Filled images, otherwise fillAmount is ignored (bar reads 100%).
foreach (var name in new[] { "Restoration bar fill", "Water contact fill" })
{
    var t = FindInactive(name, ui);
    var img = t != null ? t.GetComponent<Image>() : null;
    if (img == null) { sb.AppendLine("missing fill " + name); continue; }
    img.type = Image.Type.Filled;
    img.fillMethod = Image.FillMethod.Horizontal;
    img.fillOrigin = (int)Image.OriginHorizontal.Left;
    img.fillAmount = 1f;
    sb.AppendLine("filled bar " + name);
}

// 4. Counter rows still used the Kenney pixel font in cream (invisible on cream).
foreach (var name in new[] { "Seedling counter", "Tree counter", "Fire counter" })
{
    var t = FindInactive(name, ui);
    var tmp = t != null ? t.GetComponent<TMP_Text>() : null;
    if (tmp == null) { sb.AppendLine("missing counter " + name); continue; }
    tmp.font = fontRegular;
    tmp.color = new Color32(0x69, 0x42, 0x23, 0xFF);
    tmp.fontSize = 20;
    tmp.enableAutoSizing = true;
    tmp.fontSizeMin = 14;
    tmp.fontSizeMax = 20;
    tmp.textWrappingMode = TextWrappingModes.NoWrap;
    sb.AppendLine("counter " + name);
}

// 5. Field map: the title was wider than its rect (wrapped + banner overflowed), and the
//    zone dots were almost touching, which read as a broken slider.
var mapCard = FindInactive("Arunika field map", ui);
if (mapCard != null)
{
    var mrt = (RectTransform)mapCard;
    var oldPos = mrt.anchoredPosition;
    float dw = 260f - mrt.rect.width;
    float dh = 250f - mrt.rect.height;
    mrt.sizeDelta = new Vector2(260f, 250f);
    float sx = mrt.pivot.x <= 0.01f ? -dw : (mrt.pivot.x >= 0.99f ? 0f : -dw * 0.5f);
    mrt.anchoredPosition = oldPos + new Vector2(sx, 0f);
    sb.AppendLine("map card resized, dx=" + sx);

    var title = FindInactive("Map title", ui);
    var tmp = title.GetComponent<TMP_Text>();
    var trt = (RectTransform)title;
    trt.sizeDelta = new Vector2(300f, 24f);
    trt.anchoredPosition = new Vector2(16f, -14f);
    tmp.fontSize = 16;
    tmp.enableAutoSizing = false;
    tmp.textWrappingMode = TextWrappingModes.NoWrap;

    var banner = mapCard.Find("Title banner");
    if (banner != null)
    {
        var brt = (RectTransform)banner;
        var pref = tmp.GetPreferredValues(tmp.text);
        float bw = Mathf.Min(232f, pref.x + 44f);
        brt.sizeDelta = new Vector2(bw, 38f);
        brt.anchoredPosition = new Vector2(16f + bw * 0.5f, -14f - 12f);
    }

    var coords = FindInactive("Map coordinates", ui);
    if (coords != null)
    {
        var crt = (RectTransform)coords;
        crt.sizeDelta = new Vector2(210f, 190f);
        crt.anchoredPosition = new Vector2(20f, -48f);
    }

    foreach (var n in new[] { "Zone A", "Zone B", "Zone C" })
    {
        var t = FindInactive(n, ui);
        if (t == null) continue;
        ((RectTransform)t).sizeDelta = new Vector2(26f, 26f);
    }
    foreach (var n in new[] { "A label", "B label", "C label" })
    {
        var t = FindInactive(n, ui);
        if (t == null) continue;
        var lrt = (RectTransform)t;
        lrt.sizeDelta = new Vector2(40f, 22f);
        lrt.anchoredPosition = new Vector2(24f, lrt.anchoredPosition.y);
        var ltmp = t.GetComponent<TMP_Text>();
        ltmp.fontSize = 16; ltmp.enableAutoSizing = false;
        ltmp.color = new Color32(0x69, 0x42, 0x23, 0xFF);
    }
    foreach (var n in new[] { "Destination dot", "Volunteer dot" })
    {
        var t = FindInactive(n, ui);
        if (t != null) ((RectTransform)t).sizeDelta = new Vector2(12f, 12f);
    }
    var trail = FindInactive("Trail", ui);
    if (trail != null) ((RectTransform)trail).sizeDelta = new Vector2(3f, ((RectTransform)trail).sizeDelta.y);
}

EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);
sb.AppendLine("layout fixes applied");
return sb.ToString();
