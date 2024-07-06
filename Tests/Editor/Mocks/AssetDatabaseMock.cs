/*
 * Copyright (c) 2024 Willy Alberto Kuster
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
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Willykc.Templ.Editor.Tests.Mocks
{
    using Abstraction;

    internal sealed class AssetDatabaseMock : IAssetDatabase
    {
        internal string mockAssetPath;
        internal string mockTemplatePath;
        internal string mockInputPath;
        internal string mockSettingsPath;
        internal string mockDirectoryPath;
        internal bool mockIsValidFolder;
        internal UnityObject mockLoadAsset;

        internal string ImportAssetPath { get; private set; }
        internal int SaveAssetsCount { get; private set; }
        internal string IsValidFolderPath { get; private set; }

        string IAssetDatabase.GetAssetPath(UnityObject asset)
        {
            if (asset is TemplSettings)
            {
                return mockSettingsPath;
            }
            if (asset is TextAsset)
            {
                return mockInputPath;
            }
            if (asset is ScribanAsset)
            {
                return mockTemplatePath;
            }
            if (asset is DefaultAsset)
            {
                return mockDirectoryPath;
            }
            return mockAssetPath;
        }

        void IAssetDatabase.ImportAsset(string path) => ImportAssetPath = path;

        bool IAssetDatabase.IsValidFolder(string path)
        {
            IsValidFolderPath = path;
            return mockIsValidFolder;
        }

        T IAssetDatabase.LoadAssetAtPath<T>(string path) => mockLoadAsset as T;

        void IAssetDatabase.SaveAssets() => SaveAssetsCount++;
    }
}
