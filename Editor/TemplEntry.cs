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
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Willykc.Templ.Editor
{
    [Serializable]
    public abstract class TemplEntry
    {
        public ScribanAsset template;
        public DefaultAsset directory;
        public string filename;

        [SerializeField]
        internal string guid = Guid.NewGuid().ToString();

        [NonSerialized]
        internal string fullPathCache;

        internal bool IsValid =>
            IsValidInput &&
            template &&
            !template.HasErrors &&
            directory &&
            !string.IsNullOrWhiteSpace(filename) &&
            filename.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;

        internal string FullPath =>
            directory &&
            !string.IsNullOrWhiteSpace(filename) &&
            filename.IndexOfAny(Path.GetInvalidFileNameChars()) == -1
            ? Path.GetFullPath(OutputAssetPath)
            : string.Empty;

        internal bool ShouldRender(AssetChanges changes) =>
            (IsInputChanged(changes) || IsTemplateChanged(changes)) &&
            !changes.importedAssets.Contains(OutputAssetPath);

        internal virtual string OutputAssetPath =>
            $"{AssetDatabase.GetAssetPath(directory)}/{filename.Trim()}";

        internal virtual bool IsTemplateChanged(AssetChanges changes) =>
            changes.importedAssets.Contains(AssetDatabase.GetAssetPath(template));

        public abstract string InputFieldName { get; }
        public abstract bool IsValidInput { get; }
        public abstract object InputValue { get; }
        public abstract bool DelayRender { get; }

        public abstract bool IsInputChanged(AssetChanges changes);
        public abstract bool WillDeleteInput(string path);
    }
}
