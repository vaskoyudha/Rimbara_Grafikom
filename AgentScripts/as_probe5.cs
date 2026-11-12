using System;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

var sb = new StringBuilder();
var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
string[] names = { "AssetStorePackageInstaller", "AssetStoreOAuth", "AssetStoreCachePathProxy", "AssetStoreDownloadOperation", "AssetStorePurchases", "AssetStoreRestAPI" };
foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
{
    Type[] types; try { types = asm.GetTypes(); } catch { continue; }
    foreach (var t in types)
    {
        if (t.FullName == null || !names.Contains(t.Name) || !t.FullName.StartsWith("UnityEditor.PackageManager.UI.Internal")) continue;
        sb.AppendLine("TYPE " + t.FullName);
        foreach (var f in t.GetFields(flags).Where(f => f.DeclaringType == t && (f.Name.Contains("download") || f.Name.Contains("path") || f.Name.Contains("cache") || f.Name.Contains("Id")))) sb.AppendLine("  field " + f.FieldType.Name + " " + f.Name);
        foreach (var m in t.GetMethods(flags).Where(m => m.DeclaringType == t && (m.Name.Contains("Import") || m.Name.Contains("Download") || m.Name.Contains("Install") || m.Name.Contains("Path") || m.Name.Contains("Login") || m.Name.Contains("Fetch"))))
            sb.AppendLine("  " + (m.IsStatic ? "static " : "") + m.ReturnType.Name + " " + m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")");
    }
}
return sb.ToString();
