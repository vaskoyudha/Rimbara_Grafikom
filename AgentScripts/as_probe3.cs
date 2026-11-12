using System;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

var sb = new StringBuilder();
foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
{
    Type[] types;
    try { types = asm.GetTypes(); } catch { continue; }
    foreach (var t in types)
    {
        if (t.FullName == null || !t.FullName.StartsWith("UnityEditor.PackageManager.UI.Internal")) continue;
        if (t.Name.Contains("Container") || t.Name.Contains("Service") || t.Name == "UnityEditor" || t.Name.Contains("PackageManagerUI") || t.Name.StartsWith("AssetStore"))
            sb.AppendLine(t.FullName + "  abstract=" + t.IsAbstract + " static=" + (t.IsAbstract && t.IsSealed));
    }
}
return sb.ToString();
