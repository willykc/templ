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
using JetBrains.Annotations;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Willykc.Templ.Editor.Scaffold
{
    using ILogger = Abstraction.ILogger;

    internal sealed class TemplScaffoldFacade : ITemplScaffoldFacade
    {
        private readonly ILogger log;
        private readonly ITemplScaffoldCore scaffoldCore;
        private readonly object lockHandle = new object();

        private bool isGenerating;

        bool ITemplScaffoldFacade.IsGenerating => isGenerating;

        internal TemplScaffoldFacade(
            ILogger log,
            ITemplScaffoldCore scaffoldCore)
        {
            this.log = log;
            this.scaffoldCore = scaffoldCore;
        }

        [ItemCanBeNull]
        async Task<string[]> ITemplScaffoldFacade.GenerateScaffoldAsync(
            TemplScaffold scaffold,
            string targetPath,
            object input,
            Object selection,
            OverwriteOptions overwriteOption,
            CancellationToken cancellationToken)
        {
            scaffold = scaffold
                ? scaffold
                : throw new ArgumentException($"{nameof(scaffold)} must not be null");
            targetPath = !string.IsNullOrWhiteSpace(targetPath)
                ? targetPath
                : throw new ArgumentException($"{nameof(targetPath)} must not be null or empty");

            lock (lockHandle)
            {
                if (isGenerating)
                {
                    log.Error("Only one scaffold can be generated at a time");
                    return null;
                }

                isGenerating = true;
            }

            var generatedPaths = scaffold.DefaultInput && input == null
                ? await ShowScaffoldInputFormAsync(
                    scaffold,
                    targetPath,
                    selection,
                    overwriteOption,
                    cancellationToken)
                : await ValidateAndGenerateScaffoldAsync(
                    scaffold,
                    targetPath,
                    input,
                    selection,
                    overwriteOption,
                    cancellationToken);

            isGenerating = false;
            return generatedPaths;
        }

        private Task<string[]> ShowScaffoldInputFormAsync(
            TemplScaffold scaffold,
            string targetPath,
            Object selection,
            OverwriteOptions overwriteOption,
            CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<string[]>();
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var form =
                TemplScaffoldInputForm.Show(scaffold, targetPath, selection, cancellationToken);
            string[] generatedPaths = null;
            form.GenerateClicked += OnGenerateClicked;
            form.Closed += OnInputFormClosed;
            return completionSource.Task;

            async void OnGenerateClicked(ScriptableObject input)
            {
                generatedPaths = await ValidateAndGenerateScaffoldAsync(
                    scaffold,
                    targetPath,
                    input,
                    selection,
                    overwriteOption,
                    tokenSource.Token);

                if (generatedPaths == null)
                {
                    return;
                }

                form.Close();
            }

            void OnInputFormClosed()
            {
                form.GenerateClicked -= OnGenerateClicked;
                form.Closed -= OnInputFormClosed;
                isGenerating = false;
                tokenSource.Cancel();
                completionSource.SetResult(generatedPaths);
            }
        }

        private async Task<string[]> ValidateAndGenerateScaffoldAsync(
            TemplScaffold scaffold,
            string targetPath,
            object input,
            Object selection,
            OverwriteOptions overwriteOption,
            CancellationToken token)
        {
            var errors =
                scaffoldCore.ValidateScaffoldGeneration(scaffold, targetPath, input, selection);

            var existingFilePaths = errors
                .Where(e => e.Type == TemplScaffoldErrorType.Overwrite)
                .Select(e => e.Message)
                .ToArray();

            if (errors.Any(e => e.Type != TemplScaffoldErrorType.Overwrite))
            {
                log.Error($"Aborted generation of {scaffold.name} scaffold due to errors");
                return null;
            }

            var skipPaths = overwriteOption switch
            {
                OverwriteOptions.OverwriteAll => TemplSettings.EmptyStringArray,
                OverwriteOptions.SkipAll => existingFilePaths,
                _ => await TemplScaffoldOverwriteDialog
                    .ShowAsync(scaffold, targetPath, existingFilePaths, token)
            };

            if (existingFilePaths.Length > 0 && skipPaths == null)
            {
                return null;
            }

            return scaffoldCore
                .GenerateScaffold(scaffold, targetPath, input, selection, skipPaths);
        }
    }
}
