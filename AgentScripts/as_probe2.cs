using System;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

var sb = new StringBuilder();
var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
{
    Type[] types;
    try { types = asm.GetTypes(); } catch { continue; }
    foreach (var t in types)
    {
        if (t.FullName == null) continue;
        if (!t.FullName.StartsWith("UnityEditor.PackageManager.UI.Internal")) continue;
        if (!(t.Name.Contains("DownloadManager") || t.Name.Contains("ServicesContainer") || t.Name == "Application" || t.Name.Contains("AssetStoreCache") || t.Name.Contains("PurchaseInfo"))) continue;
        sb.AppendLine("TYPE " + t.FullName + " abstract=" + t.IsAbstract);
        foreach (var m in t.GetMethods(flags).Where(m => m.DeclaringType == t && (m.Name.Contains("Download") || m.Name.Contains("Import") || m.Name.Contains("Get") || m.Name.Contains("Resolve") || m.Name.Contains("Instance"))))
            sb.AppendLine("  " + (m.IsStatic ? "static " : "") + m.ReturnType.Name + " " + m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")");
        foreach (var p in t.GetProperties(flags).Where(p => p.Name.Contains("Instance") || p.Name.Contains("Download")))
            sb.AppendLine("  prop " + p.PropertyType.Name + " " + p.Name + (p.GetGetMethod(true)?.IsStatic == true ? " (static)" : ""));
    }
}
return sb.Length == 0 ? "nothing" : sb.ToString();
