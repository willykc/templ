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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Willykc.Templ.Editor.Scaffold
{
    using static TemplScaffoldCore;

    internal sealed class TemplScaffoldOverwriteDialog : EditorWindow
    {
        private const int Width = 600;
        private const int Height = 200;
        private const int ScrollViewLeftPadding = 20;
        private const string DialogOkText = "Continue";
        private const string DialogCancelText = "Abort";
        private const string SelectAllText = "Select all";
        private const string DeselectAllText = "Deselect all";
        private const string LabelStyleName = "label";
        private const string ButtonStyleName = "button";

        private static readonly Task<string[]> NullArrayTask = Task.FromResult<string[]>(null);

        private static TemplScaffoldOverwriteDialog current;

        private string targetPath;
        private TemplScaffold scaffold;
        private Dictionary<string, bool> pathsSelection;
        private TaskCompletionSource<string[]> completionSource;
        private CancellationToken cancellationToken;
        private string[] paths;
        private string[] skipPaths;
        private Vector2 scrollPos;
        private GUIStyle labelStyle;
        private GUIStyle buttonPanelStyle;
        private GUIStyle scrollViewStyle;

        internal static Task<string[]> Show(
            TemplScaffold scaffold,
            string targetPath,
            string[] paths,
            CancellationToken token)
        {
            if (current)
            {
                current.Focus();
                return NullArrayTask;
            }

            if (paths?.Length == 0)
            {
                return NullArrayTask;
            }

            scaffold = scaffold
                ? scaffold
                : throw new ArgumentException($"{nameof(scaffold)} must not be null");
            targetPath = !string.IsNullOrWhiteSpace(targetPath)
                ? targetPath
                : throw new ArgumentException($"{nameof(targetPath)} must not be null or empty");

            var window = CreateInstance<TemplScaffoldOverwriteDialog>();
            window.Center(Width, Height);
            window.labelStyle = new GUIStyle(LabelStyleName) { wordWrap = true };
            var buttonStyle = new GUIStyle(ButtonStyleName);
            window.buttonPanelStyle = new GUIStyle() { fixedHeight = buttonStyle.fixedHeight };
            window.scrollViewStyle =
                new GUIStyle() { padding = new RectOffset(ScrollViewLeftPadding, 0, 0, 0) };
            window.titleContent = new GUIContent(ScaffoldGenerationTitle);
            window.scaffold = scaffold;
            window.targetPath = targetPath;
            window.paths = paths.OrderBy(p => p).ToArray();
            window.pathsSelection = paths.ToDictionary(p => p, _ => true);
            window.completionSource = new TaskCompletionSource<string[]>();
            window.cancellationToken = token;
            window.ShowUtility();
            current = window;
            return window.completionSource.Task;
        }

        private void OnGUI()
        {
            GUILayout.Label($"Generating {scaffold.name} scaffold at {targetPath} will overwrite " +
                "the files selected below. Deselect those that should be skipped.", labelStyle);
            DrawPathToggles();
            DrawButtonPanel();

            if (cancellationToken.IsCancellationRequested)
            {
                Close();
            }
        }

        private void OnDestroy()
        {
            completionSource.SetResult(skipPaths);
            current = null;
        }

        private void DrawPathToggles()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, scrollViewStyle,
                            GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            foreach (var path in paths)
            {
                pathsSelection[path] =
                    EditorGUILayout.ToggleLeft(path, pathsSelection[path]);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawButtonPanel()
        {
            GUILayout.BeginHorizontal(buttonPanelStyle);

            if (GUILayout.Button(SelectAllText))
            {
                ToggleAll(true);
            }

            if (GUILayout.Button(DeselectAllText))
            {
                ToggleAll(false);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(DialogOkText))
            {
                skipPaths = pathsSelection
                    .Where(kvp => !kvp.Value)
                    .Select(kvp => kvp.Key)
                    .ToArray();
                Close();
            }

            if (GUILayout.Button(DialogCancelText))
            {
                Close();
            }

            GUILayout.EndHorizontal();
        }

        private void ToggleAll(bool toggle)
        {
            foreach (var path in paths)
            {
                pathsSelection[path] = toggle;
            }
        }
    }
}
