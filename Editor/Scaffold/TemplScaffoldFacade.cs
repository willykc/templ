/*
 * Copyright (c) 2023 Willy Alberto Kuster
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Willykc.Templ.Editor.Scaffold
{
    using Abstraction;
    using static TemplSettings;

    internal sealed class TemplScaffoldFacade : ITemplScaffoldFacade
    {
        private const int MaxOverwriteTries = 20;

        private readonly ILogger log;
        private readonly ISettingsProvider settingsProvider;
        private readonly IAssetDatabase assetDatabase;
        private readonly IEditorUtility editorUtility;
        private readonly ITemplScaffoldWindowManager windowManager;
        private readonly ITemplScaffoldCore scaffoldCore;
        private readonly object lockHandle = new object();

        private bool isGenerating;

        bool ITemplScaffoldFacade.IsGenerating => isGenerating;

        internal TemplScaffoldFacade(
            ILogger log,
            ISettingsProvider settingsProvider,
            IAssetDatabase assetDatabase,
            IEditorUtility editorUtility,
            ITemplScaffoldWindowManager windowManager,
            ITemplScaffoldCore scaffoldCore)
        {
            this.log = log;
            this.settingsProvider = settingsProvider;
            this.assetDatabase = assetDatabase;
            this.editorUtility = editorUtility;
            this.windowManager = windowManager;
            this.scaffoldCore = scaffoldCore;
        }

        [ItemCanBeNull]
        async Task<string[]> ITemplScaffoldFacade.GenerateScaffoldAsync(
            TemplScaffold scaffold,
            string targetPath,
            object input,
            UnityObject selection,
            OverwriteOptions overwriteOption,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            scaffold = scaffold ? scaffold : throw new ArgumentNullException(nameof(scaffold));
            targetPath = !string.IsNullOrWhiteSpace(targetPath)
                ? targetPath
                : throw new ArgumentException($"{nameof(targetPath)} must not be null or empty",
                nameof(targetPath));

            if (!scaffold.IsValid)
            {
                throw new ArgumentException($"{nameof(scaffold)} must be valid",
                    nameof(scaffold));
            }

            targetPath = targetPath.SanitizePath();

            if (!assetDatabase.IsValidFolder(targetPath))
            {
                throw new DirectoryNotFoundException($"Directory does not exist: '{targetPath}'");
            }

            lock (lockHandle)
            {
                if (isGenerating)
                {
                    log.Error("Only one scaffold can be generated at a time");
                    return null;
                }

                isGenerating = true;
            }

            string[] generatedPaths = null;

            try
            {
                generatedPaths = scaffold.DefaultInput && input == null
                    ? await ShowScaffoldInputFormAsync(
                        scaffold,
                        targetPath,
                        selection,
                        overwriteOption,
                        cancellationToken)
                    : await TryGenerateScaffoldAsync(
                        scaffold,
                        targetPath,
                        input,
                        selection,
                        overwriteOption,
                        cancellationToken);
            }
            finally
            {
                isGenerating = false;
            }

            return generatedPaths;
        }

        void ITemplScaffoldFacade.EnableScaffoldForSelection(TemplScaffold scaffold)
        {
            if (!settingsProvider.SettingsExist())
            {
                throw new InvalidOperationException($"{nameof(TemplSettings)} not found");
            }

            scaffold = scaffold ? scaffold : throw new ArgumentNullException(nameof(scaffold));

            if (!scaffold.IsValid)
            {
                throw new ArgumentException("Can not enable for selection an invalid scaffold",
                    nameof(scaffold));
            }

            var settings = settingsProvider.GetSettings();

            lock (lockHandle)
            {
                if (settings.Scaffolds.Contains(scaffold))
                {
                    return;
                }

                settings.Scaffolds.Add(scaffold);

                editorUtility.SetDirty(settings);
                assetDatabase.SaveAssets();
            }
        }

        void ITemplScaffoldFacade.DisableScaffoldForSelection(TemplScaffold scaffold)
        {
            if (!settingsProvider.SettingsExist())
            {
                throw new InvalidOperationException($"{nameof(TemplSettings)} not found");
            }

            scaffold = scaffold ? scaffold : throw new ArgumentNullException(nameof(scaffold));

            var settings = settingsProvider.GetSettings();

            lock (lockHandle)
            {
                if (!settings.Scaffolds.Contains(scaffold))
                {
                    return;
                }

                settings.Scaffolds.Remove(scaffold);

                editorUtility.SetDirty(settings);
                assetDatabase.SaveAssets();
            }
        }

        private async Task<string[]> ShowScaffoldInputFormAsync(
            TemplScaffold scaffold,
            string targetPath,
            UnityObject selection,
            OverwriteOptions overwriteOption,
            CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<string[]>();
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            void OnInputFormClosed() => tokenSource.Cancel();

            var tries = 0;

            while (!tokenSource.IsCancellationRequested &&
                tries++ < MaxOverwriteTries &&
                await windowManager.ShowInputFormAsync(
                scaffold,
                targetPath,
                selection,
                OnInputFormClosed,
                cancellationToken) is { } input)
            {
                string[] generatedPaths = await TryGenerateScaffoldAsync(
                    scaffold,
                    targetPath,
                    input,
                    selection,
                    overwriteOption,
                    tokenSource.Token);

                if (generatedPaths == null && !tokenSource.IsCancellationRequested)
                {
                    continue;
                }

                windowManager.CloseInputForm();
                return generatedPaths;
            }

            windowManager.CloseInputForm();
            return null;
        }

        private async Task<string[]> TryGenerateScaffoldAsync(
            TemplScaffold scaffold,
            string targetPath,
            object input,
            UnityObject selection,
            OverwriteOptions overwriteOption,
            CancellationToken cancellationToken)
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
                OverwriteOptions.OverwriteAll => EmptyStringArray,
                OverwriteOptions.SkipAll => existingFilePaths,
                _ => await GetSkipPaths(scaffold, targetPath, existingFilePaths, cancellationToken)
            };

            if (existingFilePaths.Length > 0 && skipPaths == null)
            {
                return null;
            }

            return scaffoldCore
                .GenerateScaffold(scaffold, targetPath, input, selection, skipPaths);
        }

        private async Task<string[]> GetSkipPaths(
            TemplScaffold scaffold,
            string targetPath,
            string[] existingFilePaths,
            CancellationToken cancellationToken) => existingFilePaths.Length > 0
            ? await windowManager.ShowOverwriteDialogAsync(
                scaffold,
                targetPath,
                existingFilePaths,
                cancellationToken)
            : null;
    }
}
