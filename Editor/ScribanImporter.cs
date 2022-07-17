/*
 * Copyright (c) 2022 Willy Alberto Kuster
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Willykc.Templ.Editor
{
    using ILogger = Abstraction.ILogger;
    using Logger = Abstraction.Logger;

    [ScriptedImporter(1, new[] {
        DefaultExtension,
        "sbncs",
        "sbn-cs",
        "scriban-cs",
        "sbntxt",
        "sbn-txt",
        "scriban-txt",
        "scriban",
    })]
    internal sealed class ScribanImporter : ScriptedImporter
    {
        private const string DefaultExtension = "sbn";
        private const string FileName = "NewScribanTemplate";
        private const string MenuName = "Assets/Create/Scriban Template";
        private const string DefaultTemplate = "Hello {{variable}}!";

        private readonly ILogger log;

        public ScribanImporter()
        {
            log = Logger.Instance;
        }

        [SerializeField]
        internal string text;

        [SerializeField]
        internal string[] parsingErrors;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (ctx == null)
            {
                return;
            }
            var templateAsset = ScriptableObject.CreateInstance<ScribanAsset>()
                .Init(text = File.ReadAllText(ctx.assetPath));
            parsingErrors = templateAsset.ParsingErrors;
            ctx.AddObjectToAsset(nameof(ScribanAsset), templateAsset);
            ctx.SetMainObject(templateAsset);
            if (templateAsset.HasErrors)
            {
                var errors = string.Join("\n", parsingErrors);
                log.Error($"Template errors detected when importing {ctx.assetPath}:\n{errors}");
            }
        }

        [MenuItem(MenuName, priority = 1)]
        private static void CreateNewTemplate()
        {
            var selected = Selection.activeObject;
            var newTemplatePath = GetNewTemplatePath(selected);
            File.WriteAllText(newTemplatePath, DefaultTemplate);
            AssetDatabase.ImportAsset(newTemplatePath);
            var asset = AssetDatabase.LoadAssetAtPath(newTemplatePath, typeof(ScribanAsset));
            Selection.selectionChanged += OnSelectionChanged;
            Selection.activeObject = asset;
        }

        private static string GetNewTemplatePath(UnityObject selected)
        {
            var assetPath = AssetDatabase.GetAssetPath(selected.GetInstanceID());
            var dir = File.Exists(assetPath)
                ? Path.GetDirectoryName(assetPath)
                : Path.Combine(assetPath);
            var newTemplatePath = Path.Combine(dir, $"{FileName}.{DefaultExtension}");
            var count = 0;
            while (File.Exists(newTemplatePath))
            {
                newTemplatePath = Path.Combine(dir, $"{FileName}{++count}.{DefaultExtension}");
            }
            return newTemplatePath;
        }

        private static void OnSelectionChanged()
        {
            TriggerRename();
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private static void TriggerRename()
        {
            var @event = new Event { keyCode = KeyCode.F2, type = EventType.KeyDown };
            EditorWindow.focusedWindow.SendEvent(@event);
        }

    }
}
