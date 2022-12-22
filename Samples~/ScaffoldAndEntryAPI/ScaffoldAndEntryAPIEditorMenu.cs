using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Willykc.Templ.Samples
{
    using Editor;
    using Editor.Entry;
    using Editor.Scaffold;

    [InitializeOnLoad]
    public static class ScaffoldAndEntryAPIEditorMenu
    {
        private const string AssetExtension = ".asset";
        private const string ScaffoldGUID = "b9dd2d304b92d6a48b067a70fa44c3d5";
        private const string TemplateGUID = "3b28fa1dec98e1145859ba7b82b28979";
        private const string OutputFileName = "output.txt";

        private static readonly string SessionKey = typeof(ScaffoldAndEntryAPIEditorMenu).GUID.ToString();
        private static readonly string ScaffoldPath = AssetDatabase.GUIDToAssetPath(ScaffoldGUID);
        private static readonly string TargetPath = Path.GetDirectoryName(ScaffoldPath);
        private static readonly string TemplatePath = AssetDatabase.GUIDToAssetPath(TemplateGUID);

        static ScaffoldAndEntryAPIEditorMenu()
        {
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        [MenuItem(itemName: "Templ Samples/Generate Key Data")]
        public static async void GenerateKeyData()
        {
            var scaffoldManager = TemplManagers.ScaffoldManager;
            var scaffold = AssetDatabase.LoadAssetAtPath<TemplScaffold>(ScaffoldPath);

            try
            {
                var paths = await scaffoldManager.GenerateScaffoldAsync(
                scaffold,
                TargetPath,
                overwriteOption: OverwriteOptions.SkipAll);

                var dataPath = paths.Length > 0
                    ? paths[paths.Length - 1]
                    : string.Empty;
                SessionState.SetString(SessionKey, dataPath);

                AssetDatabase.Refresh();
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }

        private static void OnAfterAssemblyReload()
        {
            var dataPath = SessionState.GetString(SessionKey, null);

            if (string.IsNullOrEmpty(dataPath) || !dataPath.EndsWith(AssetExtension))
            {
                return;
            }

            var entryManager = TemplManagers.EntryManager;
            var data = AssetDatabase.LoadAssetAtPath<UnityObject>(dataPath);
            var template = AssetDatabase.LoadAssetAtPath<ScribanAsset>(TemplatePath);
            var outputPath = $"{TargetPath}/{OutputFileName}";

            try
            {
                if (entryManager.EntryExist(outputPath))
                {
                    return;
                }

                var id = entryManager.AddEntry<ScriptableObjectEntry>(data, template, outputPath);
                entryManager.ForceRenderEntry(id);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }

            SessionState.EraseString(SessionKey);
        }
    }
}
