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
        if (t.FullName.Contains("AssetStore") && (t.Name == "AssetStore" || t.Name.Contains("Utils") || t.Name.Contains("Client") || t.Name.Contains("Download")))
        {
            sb.AppendLine("TYPE " + t.FullName);
            foreach (var m in t.GetMethods(flags).Where(m => m.Name.Contains("Down") || m.Name.Contains("Import") || m.Name.Contains("Open") || m.Name.Contains("Purchase") || m.Name.Contains("Package")))
                sb.AppendLine("  " + m.ReturnType.Name + " " + m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")");
        }
    }
}
return sb.Length == 0 ? "nothing" : sb.ToString();
