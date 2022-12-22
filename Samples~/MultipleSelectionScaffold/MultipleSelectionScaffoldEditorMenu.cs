using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Willykc.Templ.Samples
{
    using Editor;
    using Editor.Scaffold;

    public static class MultipleSelectionScaffoldEditorMenu
    {
        private const string ScaffoldGUID = "c71f591dad69cc74fa07ba0614cf889b";
        private const string AssetsPath = "Assets";

        private static readonly string ScaffoldPath = AssetDatabase.GUIDToAssetPath(ScaffoldGUID);

        [MenuItem(itemName: "Templ Samples/Generate Metadata")]
        public static async void GenerateSelectionMetadata()
        {
            var scaffold = AssetDatabase.LoadAssetAtPath<TemplScaffold>(ScaffoldPath);

            try
            {
                foreach (var selection in Selection.objects)
                {
                    await Generate(scaffold, selection);
                }

                AssetDatabase.Refresh();
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }

        private static async Task Generate(TemplScaffold scaffold, UnityObject selection)
        {
            var scaffoldManager = TemplManagers.ScaffoldManager;
            var path = AssetDatabase.GetAssetPath(selection);

            if (string.IsNullOrEmpty(path) || path == AssetsPath)
            {
                return;
            }

            var targetPath = Path.GetDirectoryName(path);

            await scaffoldManager.GenerateScaffoldAsync(
            scaffold,
            targetPath,
            selection: selection,
            overwriteOption: OverwriteOptions.OverwriteAll);
        }
    }
}
