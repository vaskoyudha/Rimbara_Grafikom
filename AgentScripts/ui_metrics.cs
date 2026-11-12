using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rimbara.UI;

var sb = new StringBuilder();
var ui = GameObject.Find("UI - RIMBARA")?.transform;
if (ui == null) return "no UI";
var hud = Object.FindFirstObjectByType<RimbaraHud>();
var v = hud != null ? hud.View : null;
sb.AppendLine("play=" + Application.isPlaying + " timeScale=" + Time.timeScale);
if (v != null)
{
    sb.AppendLine("screens menu=" + (v.Menu != null && v.Menu.activeSelf) + " gameplay=" + (v.Gameplay != null && v.Gameplay.activeSelf)
        + " briefing=" + (v.Briefing != null && v.Briefing.activeSelf) + " controls=" + (v.Controls != null && v.Controls.activeSelf)
        + " pause=" + (v.Pause != null && v.Pause.activeSelf) + " victory=" + (v.Victory != null && v.Victory.activeSelf)
        + " dimmer=" + (v.Dimmer != null && v.Dimmer.activeSelf));
    sb.AppendLine("RestorationFill type=" + v.RestorationFill.type + " fillMethod=" + v.RestorationFill.fillMethod + " amount=" + v.RestorationFill.fillAmount.ToString("0.00") + " sprite=" + (v.RestorationFill.sprite != null ? v.RestorationFill.sprite.name : "null"));
    sb.AppendLine("FireFill type=" + v.FireFill.type + " fillMethod=" + v.FireFill.fillMethod + " amount=" + v.FireFill.fillAmount.ToString("0.00"));
    sb.AppendLine("counters seed=" + (v.Seedlings != null ? v.Seedlings.text : "null") + " trees=" + (v.Trees != null ? v.Trees.text : "null") + " fires=" + (v.Fires != null ? v.Fires.text : "null"));
}
var cs = ui.GetComponentInChildren<CanvasScaler>(true);
if (cs != null) sb.AppendLine("CanvasScaler mode=" + cs.uiScaleMode + " ref=" + cs.referenceResolution + " match=" + cs.matchWidthOrHeight);
var map = FindInactive(ui, "Arunika field map");
if (map != null) { var rt = (RectTransform)map; sb.AppendLine("fieldmap rect=" + Mathf.RoundToInt(rt.rect.width) + "x" + Mathf.RoundToInt(rt.rect.height)); }
sb.AppendLine("-- buttons --");
int btnCount = 0;
foreach (var b in ui.GetComponentsInChildren<Button>(true))
{
    btnCount++;
    var rt = (RectTransform)b.transform;
    var img = b.GetComponent<Image>();
    sb.AppendLine("btn " + b.name + " " + Mathf.RoundToInt(rt.rect.width) + "x" + Mathf.RoundToInt(rt.rect.height) + " ppm=" + (img != null ? img.pixelsPerUnitMultiplier.ToString("0.00") : "noimg") + " active=" + b.gameObject.activeInHierarchy);
}
sb.AppendLine("buttons=" + btnCount);
int bad = 0; var issues = new StringBuilder();
foreach (var tmp in ui.GetComponentsInChildren<TMP_Text>(true))
{
    if (tmp.gameObject.name.StartsWith("TMP SubMesh")) continue;
    var rt = (RectTransform)tmp.transform;
    if (rt.rect.width < 1 || rt.rect.height < 1) continue;
    var b = tmp.textBounds.size;
    bool over = b.x > rt.rect.width + 3f || b.y > rt.rect.height + 3f;
    bool trunc = tmp.overflowMode == TextOverflowModes.Truncate && (tmp.preferredWidth > rt.rect.width + 3f || tmp.preferredHeight > rt.rect.height + 3f);
    if (over || trunc) { bad++; issues.AppendLine("OVERFLOW " + tmp.name + " rect=" + Mathf.RoundToInt(rt.rect.width) + "x" + Mathf.RoundToInt(rt.rect.height) + " bounds=" + Mathf.RoundToInt(b.x) + "x" + Mathf.RoundToInt(b.y)); }
}
sb.AppendLine("overflowIssues=" + bad);
sb.Append(issues);
sb.AppendLine("filled images=" + string.Join(",", ui.GetComponentsInChildren<Image>(true).Where(i => i.type == Image.Type.Filled).Select(i => i.name)));
return sb.ToString();

Transform FindInactive(Transform root, string name)
{
    foreach (var t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
    return null;
}
