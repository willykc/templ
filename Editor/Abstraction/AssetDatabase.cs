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
using UnityObject = UnityEngine.Object;

namespace Willykc.Templ.Editor.Abstraction
{
    internal sealed class AssetDatabase : IAssetDatabase
    {
        private static AssetDatabase instance;

        internal static AssetDatabase Instance => instance ??= new AssetDatabase();

        private AssetDatabase() { }

        string IAssetDatabase.GetAssetPath(UnityObject asset) =>
            UnityEditor.AssetDatabase.GetAssetPath(asset);

        void IAssetDatabase.ImportAsset(string path) =>
            UnityEditor.AssetDatabase.ImportAsset(path);

        bool IAssetDatabase.IsValidFolder(string path) =>
            UnityEditor.AssetDatabase.IsValidFolder(path);

        T IAssetDatabase.LoadAssetAtPath<T>(string path) =>
            UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);

        void IAssetDatabase.SaveAssets() =>
            UnityEditor.AssetDatabase.SaveAssets();
    }
}
