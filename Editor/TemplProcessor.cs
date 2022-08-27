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
using UnityEditor;

namespace Willykc.Templ.Editor
{
    using static TemplEditorInitialization;

    [InitializeOnLoad]
    internal sealed class TemplProcessor : AssetPostprocessor
    {
        internal static TemplCore Core { get; }

        static TemplProcessor()
        {
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            Core = new TemplCore(
                Abstraction.AssetDatabase.Instance,
                Abstraction.File.Instance,
                Abstraction.SessionState.Instance,
                Abstraction.Logger.Instance,
                Abstraction.SettingsProvider.Instance,
                TypeCache);
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            Core.OnAssetsChanged(new AssetsPaths(
                importedAssets,
                deletedAssets,
                movedAssets,
                movedFromAssetPaths));
        }

        private static void OnAfterAssemblyReload()
        {
            Core.OnAfterAssemblyReload();
        }

        private sealed class AssemblyReferenceDeleteHandler :
            UnityEditor.AssetModificationProcessor
        {
            private static AssetDeleteResult OnWillDeleteAsset(
                string path,
                RemoveAssetOptions options)
            {
                Core.OnWillDeleteAsset(path);
                return AssetDeleteResult.DidNotDelete;
            }
        }
    }
}
