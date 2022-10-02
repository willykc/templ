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
using UnityEngine;

namespace Willykc.Templ.Editor.Scaffold
{
    using Editor = UnityEditor.Editor;
    using Logger = Abstraction.Logger;

    internal sealed class TemplScaffoldInputForm : EditorWindow
    {
        private const int Width = 500;
        private const int Height = 400;
        private const int LineHeight = 1;
        private const int LineVerticalOffset = 10;
        private const string InputFormTitleSuffix = "Input Form";
        private const string GenerateButtonPrefix = "Generate";
        private const string SelectionLabel = "Selected Asset";
        private const string TargetPathLabel = "Target Path";

        private ScriptableObject input;
        private TemplScaffold scaffold;
        private Object selection;
        private Editor inputEditor;
        private Vector2 scrollPos;
        private string targetPath;

        internal static void ShowScaffoldInputForm(TemplScaffold scaffold)
        {
            if (!scaffold.DefaultInput)
            {
                return;
            }

            var window = CreateInstance<TemplScaffoldInputForm>();
            window.Center(Width, Height);
            window.scaffold = scaffold;
            window.selection = Selection.activeObject;
            window.targetPath = window.selection.GetAssetDirectoryPath();
            window.titleContent = new GUIContent($"{scaffold.name} {InputFormTitleSuffix}");
            window.input = Instantiate(scaffold.DefaultInput);
            window.input.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            window.inputEditor = Editor.CreateEditor(window.input);
            window.ShowModalUtility();
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            inputEditor.OnInspectorGUI();
            EditorGUILayout.EndScrollView();
            DrawLine();
            DrawSelectionAndPath();

            if (GUILayout.Button($"{GenerateButtonPrefix} {scaffold.name}"))
            {
                GenerateScaffold(scaffold, selection, input);
                Close();
            }
        }

        private void DrawSelectionAndPath()
        {
            if (!selection)
            {
                return;
            }

            GUI.enabled = false;
            EditorGUILayout.ObjectField(SelectionLabel, selection, selection.GetType(), false);
            EditorGUILayout.TextField(TargetPathLabel, targetPath);
            GUI.enabled = true;
        }

        private void OnDestroy()
        {
            DestroyImmediate(inputEditor);
        }

        private static void DrawLine()
        {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(LineHeight));
            rect.y -= LineVerticalOffset;
            EditorGUI.LabelField(rect, string.Empty, GUI.skin.horizontalSlider);
        }

        private static void GenerateScaffold(
            TemplScaffold scaffold,
            Object selection,
            ScriptableObject input) =>
            Logger.Instance.Info($"{scaffold.name} scaffold generated at {selection.name}");
    }
}
