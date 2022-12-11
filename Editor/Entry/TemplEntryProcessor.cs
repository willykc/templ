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

namespace Willykc.Templ.Editor.Entry
{
    using static TemplManagers;

    [InitializeOnLoad]
    internal sealed class TemplEntryProcessor : AssetPostprocessor
    {
        static TemplEntryProcessor()
        {
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            EntryCore.OnAssetsChanged(new AssetsPaths(
                importedAssets,
                deletedAssets,
                movedAssets,
                movedFromAssetPaths));
        }

        private static void OnAfterAssemblyReload()
        {
            EntryCore.OnAfterAssemblyReload();
        }

        private sealed class AssemblyReferenceDeleteHandler :
            UnityEditor.AssetModificationProcessor
        {
            private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions _) =>
                EntryCore.OnWillDeleteAsset(path)
                ? AssetDeleteResult.DidNotDelete
                : AssetDeleteResult.FailedDelete;
        }
    }
}
