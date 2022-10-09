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
using UnityEngine;
using Object = UnityEngine.Object;

namespace Willykc.Templ.Editor
{
    using Scaffold;
    using static TemplEditorInitialization;
    using static TemplScaffoldCore;
    using FileSystem = Abstraction.FileSystem;
    using ILogger = Abstraction.ILogger;
    using Logger = Abstraction.Logger;

    internal static class TemplScaffoldGenerator
    {
        private const string DialogOkText = "Generate anyway";
        private const string DialogCancelText = "Abort";
        private const string NewLine = "\n";

        private static readonly ILogger Log;
        private static readonly TemplScaffoldCore ScaffoldCore;

        private static bool IsGenerating;

        static TemplScaffoldGenerator()
        {
            IsGenerating = false;
            Log = Logger.Instance;
            ScaffoldCore = new TemplScaffoldCore(
                FileSystem.Instance,
                Log,
                Abstraction.EditorUtility.Instance,
                TypeCache);
        }

        internal static void GenerateScaffold(
            TemplScaffold scaffold,
            string targetPath,
            ScriptableObject input = null,
            Object selection = null)
        {
            if (IsGenerating)
            {
                Log.Error("Only one scaffold can be generated at a time");
                return;
            }

            IsGenerating = true;

            if (scaffold.DefaultInput && !input)
            {
                ShowScaffoldInputForm(scaffold, targetPath, selection);
                return;
            }

            ValidateAndGenerateScaffold(scaffold, targetPath, input, selection);
            IsGenerating = false;
        }

        private static void ShowScaffoldInputForm(
            TemplScaffold scaffold,
            string targetPath,
            Object selection)
        {
            var form = TemplScaffoldInputForm
                .ShowScaffoldInputForm(scaffold, targetPath, selection);
            form.GenerateClicked += OnGenerateClicked;
            form.Closed += OnInputFormClosed;

            void OnGenerateClicked(ScriptableObject input)
            {
                if (!ValidateAndGenerateScaffold(scaffold, targetPath, input, selection))
                {
                    return;
                }

                form.Close();
            }

            void OnInputFormClosed()
            {
                form.GenerateClicked -= OnGenerateClicked;
                form.Closed -= OnInputFormClosed;
                IsGenerating = false;
            }
        }

        private static bool ValidateAndGenerateScaffold(
            TemplScaffold scaffold,
            string targetPath,
            ScriptableObject input,
            Object selection)
        {
            var errors =
                ScaffoldCore.ValidateScaffoldGeneration(scaffold, targetPath, input, selection);

            if (Aborting(scaffold, targetPath, errors))
            {
                return false;
            }

            var generatedPaths =
                ScaffoldCore.GenerateScaffold(scaffold, targetPath, input, selection);

            AssetDatabase.Refresh();
            SelectFirstReneratedAsset(generatedPaths);
            return true;
        }

        private static bool Aborting(
            TemplScaffold scaffold,
            string targetPath,
            TemplScaffoldError[] errors)
        {
            var overwritePaths = errors
                .Where(e => e.Type == TemplScaffoldErrorType.Overwrite)
                .Select(e => e.Message)
                .ToArray();

            if (errors.Any(e => e.Type != TemplScaffoldErrorType.Overwrite))
            {
                Log.Error($"Aborted generation of {scaffold.name} scaffold due to errors");
                return true;
            }

            return OverwriteDialogAborted(scaffold, targetPath, overwritePaths);
        }

        private static void SelectFirstReneratedAsset(string[] generatedPaths)
        {
            if (generatedPaths.Length == 0)
            {
                return;
            }

            var first = generatedPaths[0];
            Selection.activeObject = AssetDatabase.LoadAssetAtPath(first, typeof(Object));
        }

        private static bool OverwriteDialogAborted(
            TemplScaffold scaffold,
            string targetPath,
            string[] paths)
        {
            var joinedPaths = string.Join(NewLine, paths);
            return paths.Length > 0 && !EditorUtility.DisplayDialog(
                ScaffoldGenerationTitle,
                $"Generating {scaffold.name} scaffold at {targetPath} will " +
                $"overwrite the following files:{NewLine}{joinedPaths}",
                DialogOkText, DialogCancelText);
        }
    }
}
