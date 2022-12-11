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
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Willykc.Templ.Editor.Entry
{
    [Serializable]
    public abstract class TemplEntry
    {
        internal const string NameOfTemplate = nameof(template);
        internal const string NameOfDirectory = nameof(directory);
        internal const string NameOfFilename = nameof(filename);

        private FieldInfo inputField;
        private bool? deferred;
        private ChangeType? changeTypes;

        [SerializeField]
        private ScribanAsset template;
        [SerializeField]
        private DefaultAsset directory;
        [SerializeField]
        private string filename;
        [SerializeField]
        private string guid = Guid.NewGuid().ToString();

        [NonSerialized]
        internal string fullPathCache;

        public string Id => guid;

        public ScribanAsset Template
        {
            get => template;
            internal set => template = value;
        }

        public UnityObject InputAsset
        {
            get => InputField?.GetValue(this) as UnityObject;
            internal set => InputField?.SetValue(this, value);
        }

        public string OutputPath => OutputAssetPath;

        public bool IsValid =>
            !string.IsNullOrEmpty(InputFieldName) &&
            ChangeTypes != ChangeType.None &&
            IsValidInput &&
            template &&
            !template.HasErrors &&
            directory &&
            !directory.IsReadOnly() &&
            !string.IsNullOrWhiteSpace(filename) &&
            filename.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;

        internal DefaultAsset Directory
        {
            get => directory;
            set => directory = value;
        }

        internal string Filename
        {
            get => filename;
            set => filename = value;
        }

        internal string FullPath =>
            directory &&
            !string.IsNullOrWhiteSpace(filename) &&
            filename.IndexOfAny(Path.GetInvalidFileNameChars()) == -1
            ? Path.GetFullPath(OutputAssetPath)
            : string.Empty;

        internal object TheInputValue => InputValue;

        internal bool IsValidInput => IsValidInputField;

        internal string InputFieldName => InputField?.Name;

        internal string ExposedInputName =>
            InputField?.GetCustomAttribute<TemplInputAttribute>()?.ExposedAs is string exposedAs &&
            !string.IsNullOrWhiteSpace(exposedAs)
            ? exposedAs
            : InputFieldName;

        internal virtual bool Deferred => deferred ??= GetType()
            .GetCustomAttribute<TemplEntryInfoAttribute>()?.Deferred ?? false;

        internal virtual string OutputAssetPath =>
            $"{AssetDatabase.GetAssetPath(directory)}/{filename.Trim()}";

        internal ChangeType ChangeTypes => changeTypes ??= GetType()
            .GetCustomAttribute<TemplEntryInfoAttribute>()?.ChangeTypes ?? ChangeType.None;

        protected virtual bool IsValidInputField => InputAsset;

        protected virtual object InputValue => InputAsset;

        private FieldInfo InputField => inputField ??= Array.Find(GetType()
            .GetFields(), f => f.IsDefined(typeof(TemplInputAttribute)));

        internal static bool IsSubclass(object entry) =>
            entry?.GetType().IsSubclassOf(typeof(TemplEntry)) == true;

        internal bool ShouldRender(AssetChange change) =>
            (IsInputChanged(change) || IsTemplateChanged(change)) &&
            change.currentPath != OutputAssetPath;

        internal bool DeclaresChangeType(ChangeType type) => (ChangeTypes & type) == type;

        internal virtual bool IsTemplateChanged(AssetChange change) =>
            change.currentPath == AssetDatabase.GetAssetPath(template);

        protected virtual bool IsInputChanged(AssetChange change) =>
            InputValue is UnityObject asset &&
            change.currentPath == AssetDatabase.GetAssetPath(asset);
    }
}
