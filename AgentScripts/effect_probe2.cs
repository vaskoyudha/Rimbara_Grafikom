using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

var sb = new StringBuilder();
var ui = GameObject.Find("UI - RIMBARA").transform;
foreach (var n in new[] { "Main menu card", "Restoration counters" })
{
    var t = ui.GetComponentsInChildren<Transform>(true).First(c => c.name == n);
    sb.AppendLine("== " + n);
    foreach (var c in t.GetComponents<Component>())
    {
        sb.AppendLine("   " + c.GetType().FullName + " id=" + c.GetInstanceID());
        var sh = c as Shadow;
        if (sh != null) sb.AppendLine("      effectColor=" + sh.effectColor + " dist=" + sh.effectDistance + " type=" + c.GetType().Name);
    }
}
return sb.ToString();
