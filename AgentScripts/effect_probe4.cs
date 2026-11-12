using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

var sb = new StringBuilder();
var ui = GameObject.Find("UI - RIMBARA").transform;
foreach (var n in new[] { "Main menu card", "Restoration counters" })
{
    var t = ui.GetComponentsInChildren<Transform>(true).First(c => c.name == n);
    sb.AppendLine("== " + n + " active=" + t.gameObject.activeInHierarchy);
    foreach (var eff in t.GetComponents<BaseMeshEffect>())
    {
        var sh = eff as Shadow;
        sb.AppendLine("  " + eff.GetType().FullName + " enabled=" + eff.enabled + " color=" + (sh != null ? sh.effectColor.ToString() : "n/a") + " dist=" + (sh != null ? sh.effectDistance.ToString() : "n/a"));
    }
    for (var p = t; p != null; p = p.parent)
    {
        var m = p.GetComponent<Mask>(); var rm = p.GetComponent<RectMask2D>(); var cg = p.GetComponent<CanvasGroup>();
        if (m != null || rm != null || cg != null) sb.AppendLine("  parent " + p.name + " mask=" + (m != null) + " rectMask=" + (rm != null) + " cgAlpha=" + (cg != null ? cg.alpha : 1f));
    }
    var img = t.GetComponent<Image>();
    var rt = (RectTransform)t;
    sb.AppendLine("  rect=" + rt.rect + " pivot=" + rt.pivot + " anchors=" + rt.anchorMin + ".." + rt.anchorMax);
}
return sb.ToString();
