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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Willykc.Templ.Editor
{
    using Entry;
    using Scaffold;

    internal sealed class TemplSettings : ScriptableObject
    {
        internal const string DefaultConfigFolder = "Assets/Editor/TemplData";
        private const string DefaultConfigObjectName = "com.willykc.templ";
        private const string DefaultConfigAssetName = "TemplSettings.asset";

        private static TemplSettings instance;

        [SerializeReference]
        private List<TemplEntry> entries = new List<TemplEntry>();

        [SerializeField]
        private List<TemplScaffold> scaffolds;

        internal event Action FullReset;

        internal static TemplSettings Instance =>
            instance ? instance : instance = GetSettings();

        internal IReadOnlyList<TemplEntry> Entries => entries;
        internal IReadOnlyList<TemplScaffold> Scaffolds => scaffolds;

        internal IReadOnlyList<TemplEntry> ValidEntries =>
            entries?
            .GroupBy(e => e.FullPath)
            .Where(g => g.Count() == 1)
            .SelectMany(g => g)
            .Where(e => e.IsValid)
            .ToArray()
            ?? new TemplEntry[] { };

        internal IReadOnlyList<TemplScaffold> ValidScaffolds =>
            scaffolds?
            .Where(s => s && s.IsValid)
            .ToArray()
            ?? new TemplScaffold[] { };

        internal bool HasInvalidEntries => Entries.Count != ValidEntries.Count;

        private void Reset()
        {
            FullReset?.Invoke();
            entries?.Clear();
            scaffolds?.Clear();
        }

        internal static TemplSettings CreateNewSettings()
        {
            if (Instance)
            {
                return Instance;
            }
            var settings = CreateInstance<TemplSettings>();
            Directory.CreateDirectory(DefaultConfigFolder);
            var path = $"{DefaultConfigFolder}/{DefaultConfigAssetName}";
            AssetDatabase.CreateAsset(settings, path);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            EditorBuildSettings.AddConfigObject(DefaultConfigObjectName, settings, true);
            return settings;
        }

        private static TemplSettings GetSettings() =>
            EditorBuildSettings.TryGetConfigObject(
                DefaultConfigObjectName,
                out TemplSettings settings)
            ? settings
            : null;
    }
}
