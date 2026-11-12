using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

var sb = new StringBuilder();
Type scType = null, dmType = null;
foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
{
    Type[] types; try { types = asm.GetTypes(); } catch { continue; }
    foreach (var t in types)
    {
        if (t.FullName == "UnityEditor.PackageManager.UI.Internal.ServicesContainer") scType = t;
        if (t.FullName == "UnityEditor.PackageManager.UI.Internal.AssetStoreDownloadManager") dmType = t;
    }
}
if (scType == null || dmType == null) return "types missing";
var sc = Resources.FindObjectsOfTypeAll(scType).FirstOrDefault();
if (sc == null) return "no container";
var resolve = scType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
    .First(m => m.Name == "Resolve" && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1);
var dm = resolve.MakeGenericMethod(dmType).Invoke(sc, null);
if (dm == null) return "no download manager";
sb.AppendLine("dm=" + dm.GetType().Name);
var download = dmType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
    .FirstOrDefault(m => m.Name == "Download" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType != typeof(long));
if (download == null) return "no Download(IEnumerable)";
object arg = new List<long> { 318607L };
if (download.GetParameters()[0].ParameterType.IsArray) arg = new long[] { 318607L };
bool ok;
try { ok = (bool)download.Invoke(dm, new[] { arg }); }
catch (Exception e) { return "download threw: " + (e.InnerException ?? e); }
sb.AppendLine("Download returned " + ok);
foreach (var op in new[] { "IsAnyDownloadInProgress", "DownloadInProgressCount" })
{
    var m = dmType.GetMethod(op);
    if (m != null) sb.AppendLine(op + "=" + m.Invoke(dm, null));
}
return sb.ToString();
