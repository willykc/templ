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
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Willykc.Templ.Editor
{
    [Serializable]
    public abstract class TemplEntry
    {
        private string inputFieldName;
        private bool? deferred;
        private ChangeType? changeTypes;

        public ScribanAsset template;
        public DefaultAsset directory;
        public string filename;

        [SerializeField]
        internal string guid = Guid.NewGuid().ToString();

        [NonSerialized]
        internal string fullPathCache;

        private ChangeType ChangeTypes => changeTypes ??=
            GetType().GetCustomAttribute<TemplEntryInfoAttribute>().ChangeTypes;

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

        internal object TheInputValue => InputValue;

        internal bool IsValidInput => IsValidInputField;

        internal string InputFieldName => inputFieldName ??= GetType()
            .GetFields()
            .Single(f => f.IsDefined(typeof(TemplInputAttribute))).Name;

        internal virtual bool Deferred => deferred ??=
            GetType().GetCustomAttribute<TemplEntryInfoAttribute>().Deferred;

        protected abstract bool IsValidInputField { get; }

        protected abstract object InputValue { get; }

        internal virtual string OutputAssetPath =>
            $"{AssetDatabase.GetAssetPath(directory)}/{filename.Trim()}";

        internal bool ShouldRender(AssetChange change) =>
            (IsInputChanged(change) || IsTemplateChanged(change)) &&
            change.currentPath != OutputAssetPath;

        internal virtual bool IsTemplateChanged(AssetChange change) =>
            change.currentPath == AssetDatabase.GetAssetPath(template);

        internal bool DeclaresChangeType(ChangeType type) => (ChangeTypes & type) == type;

        protected abstract bool IsInputChanged(AssetChange change);
    }
}
