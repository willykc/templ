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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Willykc.Templ.Editor.Scaffold
{
    using FileSystem = Abstraction.FileSystem;
    using ILogger = Abstraction.ILogger;
    using Logger = Abstraction.Logger;

    internal static class TemplScaffoldGenerator
    {
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
                Abstraction.TemplateFunctionProvider.Instance);
        }

        internal static async void GenerateScaffold(
            TemplScaffold scaffold,
            string targetPath,
            ScriptableObject input = null,
            Object selection = null)
        {
            scaffold = scaffold
                ? scaffold
                : throw new ArgumentException($"{nameof(scaffold)} must not be null");
            targetPath = !string.IsNullOrWhiteSpace(targetPath)
                ? targetPath
                : throw new ArgumentException($"{nameof(targetPath)} must not be null or empty");

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

            await ValidateAndGenerateScaffold(scaffold, targetPath, input, selection, default);
            IsGenerating = false;
        }

        private static void ShowScaffoldInputForm(
            TemplScaffold scaffold,
            string targetPath,
            Object selection)
        {
            var tokenSource = new CancellationTokenSource();
            var form = TemplScaffoldInputForm
                .Show(scaffold, targetPath, selection);
            form.GenerateClicked += OnGenerateClicked;
            form.Closed += OnInputFormClosed;

            async void OnGenerateClicked(ScriptableObject input)
            {
                if (!await ValidateAndGenerateScaffold(scaffold, targetPath, input,
                    selection, tokenSource.Token))
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
                tokenSource.Cancel();
            }
        }

        private static async Task<bool> ValidateAndGenerateScaffold(
            TemplScaffold scaffold,
            string targetPath,
            ScriptableObject input,
            Object selection,
            CancellationToken token)
        {
            var errors =
                ScaffoldCore.ValidateScaffoldGeneration(scaffold, targetPath, input, selection);

            var overwritePaths = errors
                .Where(e => e.Type == TemplScaffoldErrorType.Overwrite)
                .Select(e => e.Message)
                .ToArray();

            if (errors.Any(e => e.Type != TemplScaffoldErrorType.Overwrite))
            {
                Log.Error($"Aborted generation of {scaffold.name} scaffold due to errors");
                return false;
            }

            var skipPaths = await TemplScaffoldOverwriteDialog
                .Show(scaffold, targetPath, overwritePaths, token);

            if (overwritePaths.Length > 0 && skipPaths == null)
            {
                return false;
            }

            var generatedPaths = ScaffoldCore
                .GenerateScaffold(scaffold, targetPath, input, selection, skipPaths);

            AssetDatabase.Refresh();
            SelectFirstReneratedAsset(generatedPaths);
            return true;
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
    }
}
