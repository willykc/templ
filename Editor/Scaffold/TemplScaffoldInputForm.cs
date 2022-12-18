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
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Willykc.Templ.Editor.Scaffold
{
    using Editor = UnityEditor.Editor;

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

        private static TemplScaffoldInputForm current;

        private ScriptableObject input;
        private TemplScaffold scaffold;
        private Object selection;
        private Editor inputEditor;
        private Vector2 scrollPos;
        private string targetPath;
        private CancellationToken cancellationToken;
        private TaskCompletionSource<ScriptableObject> completionSource;
        private bool waiting;

        private event Action Closed;

        internal static Task<ScriptableObject> ShowAsync(
            TemplScaffold scaffold,
            string targetPath,
            Object selection,
            Action closed,
            CancellationToken cancellationToken)
        {
            scaffold = scaffold
                ? scaffold
                : throw new ArgumentException($"{nameof(scaffold)} must not be null");
            targetPath = !string.IsNullOrWhiteSpace(targetPath)
                ? targetPath
                : throw new ArgumentException($"{nameof(targetPath)} must not be null or empty");

            if (!scaffold.DefaultInput)
            {
                throw new InvalidOperationException($"{nameof(scaffold)} must have a " +
                    $"{nameof(scaffold.DefaultInput)} instance to be able to show the input form");
            }

            if (current)
            {
                current.Focus();
                current.waiting = false;
                current.completionSource = new TaskCompletionSource<ScriptableObject>();
                return current.completionSource.Task;
            }

            var window = CreateInstance<TemplScaffoldInputForm>();
            window.Center(Width, Height);
            window.scaffold = scaffold;
            window.selection = selection;
            window.targetPath = targetPath;
            window.titleContent = new GUIContent($"{scaffold.name} {InputFormTitleSuffix}");
            window.input = Instantiate(scaffold.DefaultInput);
            window.input.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            window.inputEditor = Editor.CreateEditor(window.input);
            window.completionSource = new TaskCompletionSource<ScriptableObject>();
            window.cancellationToken = cancellationToken;
            window.autoRepaintOnSceneChange = true;
            window.Closed += closed;
            window.ShowUtility();
            current = window;
            return window.completionSource.Task;
        }

        internal static void CloseCurrent()
        {
            if (current)
            {
                current.Close();
            }
        }

        private void OnGUI()
        {

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            DrawInputInspector();

            EditorGUILayout.EndScrollView();

            DrawLine();
            DrawSelectionAndPath();
            DrawGenerateButton();

            if (cancellationToken.IsCancellationRequested)
            {
                Close();
            }
        }

        private void OnDestroy()
        {
            Closed?.Invoke();
            Closed = null;
            DestroyImmediate(input);
            DestroyImmediate(inputEditor);
            current = null;
            completionSource.TrySetResult(null);
        }

        private void DrawInputInspector()
        {
            GUI.enabled = !waiting;
            inputEditor.OnInspectorGUI();
            GUI.enabled = true;
        }

        private void DrawGenerateButton()
        {
            GUI.enabled = !waiting;

            if (GUILayout.Button($"{GenerateButtonPrefix} {scaffold.name}"))
            {
                waiting = true;
                completionSource.TrySetResult(input);
            }

            GUI.enabled = true;
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

        private static void DrawLine()
        {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(LineHeight));
            rect.y -= LineVerticalOffset;
            EditorGUI.LabelField(rect, string.Empty, GUI.skin.horizontalSlider);
        }
    }
}
