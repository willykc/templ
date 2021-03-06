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
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Willykc.Templ.Editor
{
    [CustomEditor(typeof(ScribanImporter))]
    internal sealed class ScribanImporterEditor : ScriptedImporterEditor
    {
        private const string Label = "Template text";
        private const int Height = 200;
        private const string WarningMessage = "This template is not currently referenced in " +
            "Templ settings.";

        private string text;
        private string path;
        private string[] templatePaths = new string[0];

        public override void OnEnable()
        {
            base.OnEnable();
            templatePaths = TemplSettings.Instance
                ? TemplSettings.Instance.Entries
                .Select(e => AssetDatabase.GetAssetPath(e.template))
                .ToArray()
                : new string[0];
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var importer = serializedObject.targetObject as ScribanImporter;
            var property = serializedObject.FindProperty(nameof(importer.text));
            path = importer.assetPath;
            if (importer.parsingErrors.Length > 0)
            {
                var errors = string.Join("\n", importer.parsingErrors);
                EditorGUILayout.HelpBox($"Template errors found:\n{errors}", MessageType.Error);
            }
            else if (!templatePaths.Contains(path))
            {
                EditorGUILayout.HelpBox(WarningMessage, MessageType.Warning);
            }
            EditorGUILayout.LabelField(Label);
            property.stringValue = text =
                EditorGUILayout.TextArea(importer.text, GUILayout.Height(Height));
            serializedObject.ApplyModifiedProperties();
            ApplyRevertGUI();
        }

        protected override void Apply()
        {
            File.WriteAllText(path, text);
            base.Apply();
        }
    }
}
