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
using System.Linq;
using UnityEditor;

namespace Willykc.Templ.Editor.Scaffold
{
    using static TemplScaffoldEditor;

    [CustomEditor(typeof(TemplDynamicScaffold))]
    internal sealed class TemplDynamicScaffoldEditor : UnityEditor.Editor
    {
        private const string ErrorMessage = "Tree template must be not null and valid.";

        private TemplDynamicScaffold scaffold;
        private SerializedProperty treeTemplateProperty;
        private SerializedProperty inputProperty;
        private bool isScaffoldValid;
        private bool isReferencedInSettings;

        public override void OnInspectorGUI()
        {
            if (!isReferencedInSettings)
            {
                EditorGUILayout.HelpBox(WarningMessage, MessageType.Warning);
            }

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            DrawProperties();
            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                CheckValidity();
            }

            if (!isScaffoldValid)
            {
                EditorGUILayout.HelpBox(ErrorMessage, MessageType.Error);
            }
        }

        private void OnEnable()
        {
            scaffold = target as TemplDynamicScaffold;
            var settings = TemplSettings.Instance;
            isReferencedInSettings = settings && settings.Scaffolds.Contains(scaffold);
            inputProperty = serializedObject
                .FindProperty(TemplScaffold.NameOfDefaultInput);
            treeTemplateProperty = serializedObject
                .FindProperty(TemplDynamicScaffold.NameOfTreeTemplate);

            Undo.undoRedoPerformed += CheckValidity;
            scaffold.AfterReset += CheckValidity;
            CheckValidity();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= CheckValidity;
            scaffold.AfterReset -= CheckValidity;
        }

        private void CheckValidity() => isScaffoldValid = scaffold.IsValid;

        private void DrawProperties()
        {
            EditorGUILayout.PropertyField(inputProperty);
            EditorGUILayout.PropertyField(treeTemplateProperty);
            EditorGUILayout.Separator();
        }

    }
}
