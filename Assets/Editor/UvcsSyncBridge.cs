// Explicit batch-only publishing bridge. Uses a reviewed allow-list, never a workspace switch.
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Rimbara.Editor
{
    public static class UvcsSyncBridge
    {
        private static Task operation;
        public static void Inspect() { Run(false, false); }
        public static void Stage() { Run(false, true); }
        public static void Publish() { Run(true, false); }
        private static void Run(bool publish, bool stage)
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Use explicit batch operation with a reviewed allow-list.");
            operation = Execute(publish, stage);
            EditorApplication.update += Finish;
        }
        private static void Finish()
        {
            if (!operation.IsCompleted) return;
            EditorApplication.update -= Finish;
            if (operation.IsFaulted) { Debug.LogError(operation.Exception.GetBaseException().Message); EditorApplication.Exit(1); }
            else EditorApplication.Exit(0);
        }
        private static async Task<object> Call(object api, Type contract, string name, params object[] arguments)
        {
            Task task = (Task)contract.GetMethod(name).Invoke(api, arguments);
            await task;
            return task.GetType().GetProperty("Result")?.GetValue(task);
        }
        private static string PathOf(object change)
        {
            Type type = change.GetType();
            MethodInfo getter = type.GetMethod("GetFullPath") ?? type.GetMethod("GetPath");
            if (getter != null && getter.GetParameters().Length == 0) return (string)getter.Invoke(change, null);
            return (string)(type.GetProperty("Path")?.GetValue(change) ?? type.GetField("Path")?.GetValue(change));
        }
        private static async Task Execute(bool publish, bool stage)
        {
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetType("Unity.PlasticSCM.Editor.Api.UnityVersionControlApi") != null);
            assembly.GetType("Unity.PlasticSCM.Editor.PlasticApp").GetMethod("InitializeIfNeeded", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            Type contract = assembly.GetType("Unity.PlasticSCM.Editor.Api.IUnityVersionControlApi");
            object api = Activator.CreateInstance(assembly.GetType("Unity.PlasticSCM.Editor.Api.UnityVersionControlApi"), true);
            object workspace = await Call(api, contract, "GetWorkspaceFromPath", Directory.GetCurrentDirectory());
            if (workspace == null) throw new InvalidOperationException("Existing UVCS workspace not found.");
            Type plasticType = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("PlasticGui.Plastic")).First(t => t != null);
            object client = plasticType.GetProperty("API", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            if (stage)
            {
                string[] paths = File.ReadAllLines("Artifacts/uvcs-approved-files.txt").Where(p => !string.IsNullOrWhiteSpace(p)).Select(ApprovedPath).ToArray();
                MethodInfo add = client.GetType().GetMethods().Single(m => m.Name == "Add" && m.GetParameters().Length == 4);
                object options = Activator.CreateInstance(add.GetParameters()[2].ParameterType);
                foreach (string name in new[] { "AddPrivateParents", "CheckoutParent", "SearchForPrivatePaths", "SkipIgnored", "HandleMetaFiles" })
                    options.GetType().GetField(name).SetValue(options, true);
                // Only reviewed files; do not recursively collect cache/generated files.
                options.GetType().GetField("Recurse").SetValue(options, false);
                object[] arguments = { workspace, paths, options, null };
                await Task.Run(() => add.Invoke(client, arguments));
            }
            var pending = ((IEnumerable)await Call(api, contract, "GetPendingChanges", workspace)).Cast<object>().ToArray();
            var lines = pending.Select(c => PathOf(c) + "\t" + c.GetType().GetProperty("ChangeTypes")?.GetValue(c)).ToArray();
            Directory.CreateDirectory("Artifacts");
            File.WriteAllLines("Artifacts/uvcs-pending.txt", lines);
            Debug.Log("UVCS_PENDING_COUNT " + pending.Length);
            if (!publish) return;
            if (lines.Any(l => string.IsNullOrEmpty(l.Split('\t')[0]))) throw new InvalidOperationException("Unresolved pending paths; review before publish.");
            var allowed = File.ReadAllLines("Artifacts/uvcs-approved-files.txt").Where(p => !string.IsNullOrWhiteSpace(p)).Select(ApprovedPath).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (object change in pending)
            {
                string path = ApprovedPath(PathOf(change));
                if (!allowed.Contains(path) && !Directory.Exists(path)) throw new InvalidOperationException("Unreviewed pending file: " + path);
            }
            string comment = File.ReadAllText("Artifacts/uvcs-comment.txt");
            object result = await Call(api, contract, "CheckinPendingChanges", workspace, comment);
            File.WriteAllText("Artifacts/uvcs-published.txt", Convert.ToString(result));
            Debug.Log("UVCS_CHECKIN_RESULT " + result);
            var remaining = ((IEnumerable)await Call(api, contract, "GetPendingChanges", workspace)).Cast<object>().ToArray();
            File.WriteAllLines("Artifacts/uvcs-remaining.txt", remaining.Select(c => PathOf(c)));
        }
        private static string ApprovedPath(string path)
        {
            string root = Path.GetFullPath(Directory.GetCurrentDirectory()) + Path.DirectorySeparatorChar;
            string full = Path.GetFullPath(path);
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Path outside project: " + path);
            string relative = full.Substring(root.Length).Replace('\\', '/');
            string top = relative.Split('/')[0];
            string[] blocked = { ".git", ".plastic", "Library", "Temp", "Builds", "Artifacts", "Backups", "UserSettings", "Logs", "opencode.json", ".env" };
            if (blocked.Contains(top, StringComparer.OrdinalIgnoreCase) || relative.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Generated/private path not publishable: " + relative);
            return full;
        }
    }
}
