using System;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

var sb = new StringBuilder();
var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
Type sc = null;
foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
{
    Type[] types; try { types = asm.GetTypes(); } catch { continue; }
    foreach (var t in types)
    {
        if (t.FullName == "UnityEditor.PackageManager.UI.Internal.ServicesContainer") sc = t;
        if (t.FullName == null) continue;
        if (!t.FullName.StartsWith("UnityEditor.PackageManager.UI.Internal")) continue;
        var props = t.GetProperties(flags).Where(p => p.PropertyType == sc || p.PropertyType.Name == "ServicesContainer").ToList();
        if (props.Count > 0 && sc != null)
            foreach (var p in props) sb.AppendLine("HOST " + t.FullName + "." + p.Name + " static=" + (p.GetGetMethod(true)?.IsStatic ?? false));
    }
}
if (sc != null)
{
    sb.AppendLine("== ServicesContainer members ==");
    foreach (var f in sc.GetFields(flags)) sb.AppendLine(" field " + (f.IsStatic ? "static " : "") + f.FieldType.Name + " " + f.Name);
    foreach (var p in sc.GetProperties(flags)) sb.AppendLine(" prop " + p.PropertyType.Name + " " + p.Name + " static=" + (p.GetGetMethod(true)?.IsStatic ?? false));
    foreach (var m in sc.GetMethods(flags).Where(m => m.DeclaringType == sc)) sb.AppendLine(" method " + (m.IsStatic ? "static " : "") + m.ReturnType.Name + " " + m.Name + "(" + string.Join(", ", m.GetParameters().Select(x => x.ParameterType.Name)) + ")");
}
return sb.ToString();
