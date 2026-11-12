using System;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

var sb = new StringBuilder();
Type scType = null, installerType = null;
foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
{
    Type[] types; try { types = asm.GetTypes(); } catch { continue; }
    foreach (var t in types)
    {
        if (t.FullName == "UnityEditor.PackageManager.UI.Internal.ServicesContainer") scType = t;
        if (t.FullName == "UnityEditor.PackageManager.UI.Internal.AssetStorePackageInstaller") installerType = t;
    }
}
if (scType == null || installerType == null) return "types missing";

EditorApplication.ExecuteMenuItem("Window/Package Manager");
var sc = Resources.FindObjectsOfTypeAll(scType).FirstOrDefault();
if (sc == null) return "no ServicesContainer instance (open Package Manager failed?)";
sb.AppendLine("container=" + sc.name);

var resolve = scType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
    .FirstOrDefault(m => m.Name == "Resolve" && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1);
if (resolve == null) return "no Resolve<T>";
object installer;
try { installer = resolve.MakeGenericMethod(installerType).Invoke(sc, null); }
catch (Exception e) { return "resolve failed: " + (e.InnerException ?? e).Message; }
if (installer == null) return "installer null";
sb.AppendLine("installer=" + installer.GetType().Name);

var install = installerType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
    .FirstOrDefault(m => m.Name == "Install");
if (install == null) return "no Install";
try { install.Invoke(installer, new object[] { 318607L, false }); }
catch (Exception e) { return "install failed: " + (e.InnerException ?? e).Message; }
sb.AppendLine("Install(318607) requested");
return sb.ToString();
