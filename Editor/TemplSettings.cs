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
    using ILogger = Abstraction.ILogger;
    using Logger = Abstraction.Logger;

    internal sealed class TemplSettings : ScriptableObject
    {
        internal const string DefaultConfigFolder = "Assets/Editor/TemplData";
        private const string DefaultConfigObjectName = "com.willykc.templ";
        private const string DefaultConfigAssetName = "TemplSettings.asset";

        private static readonly List<TemplScaffoldNode> EmptyList = new List<TemplScaffoldNode>(0);

        private static TemplSettings instance;

        private readonly ILogger log;

        internal TemplSettings()
        {
            log = Logger.Instance;
        }

        [SerializeReference]
        private List<TemplEntry> entries = new List<TemplEntry>();

        [SerializeReference]
        private List<TemplScaffold> scaffolds = new List<TemplScaffold>();

        internal event Action FullReset;
        internal event Action<IReadOnlyList<TemplScaffoldNode>> ScaffoldChange;

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

        internal bool HasInvalidEntries => Entries.Count != ValidEntries.Count;

        private void Reset()
        {
            FullReset?.Invoke();
            entries.Clear();
            scaffolds.Clear();
        }

        internal void CreateScaffold()
        {
            var newScaffold = new TemplScaffold()
            {
                name = "scaffold"
            };
            scaffolds.Add(newScaffold);
            ScaffoldChange?.Invoke(scaffolds.FindAll(s => s == newScaffold));
        }

        internal void AddScaffoldFileNode(TemplScaffoldNode[] nodes)
        {
            nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            var newNodes = nodes.Select(n => AddNode<TemplScaffoldFile>(n, "file")).ToList();
            ScaffoldChange?.Invoke(newNodes);
        }

        internal void AddScaffoldDirectoryNode(TemplScaffoldNode[] nodes)
        {
            nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            var newNodes = nodes.Select(n => AddNode<TemplScaffoldDirectory>(n, "dir")).ToList();
            ScaffoldChange?.Invoke(newNodes);
        }

        internal void RemoveScaffoldNodes(TemplScaffoldNode[] nodes)
        {
            nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            foreach (var node in nodes)
            {
                RemoveNode(node);
            }
            ScaffoldChange?.Invoke(EmptyList);
        }

        internal void MoveScaffoldNodes(
            TemplScaffoldNode parent,
            int insertIndex,
            TemplScaffoldNode[] draggedNodes)
        {
            if (parent == null || draggedNodes == null || draggedNodes.Length == 0)
            {
                return;
            }

            if (insertIndex < 0)
            {
                throw new ArgumentException($"Invalid input: {nameof(insertIndex)} " +
                    "must not be negative");
            }

            var scaffoldIsDragged = draggedNodes
                .Any(n => n is TemplScaffold);
            if (scaffoldIsDragged)
            {
                throw new InvalidOperationException("Scaffold nodes can not be reparented");
            }

            if (parent is TemplScaffoldFile)
            {
                throw new InvalidOperationException("Scaffold File nodes can not be parent nodes");
            }

            if (insertIndex > 0)
            {
                insertIndex -= parent.children
                    .GetRange(0, insertIndex)
                    .Count(draggedNodes.Contains);
            }

            foreach (var node in draggedNodes)
            {
                node.parent.children.Remove(node);
                node.parent = parent;
            }

            parent.children.InsertRange(insertIndex, draggedNodes);
            ScaffoldChange?.Invoke(draggedNodes);
        }

        internal void CloneScaffoldNodes(TemplScaffoldNode[] nodes)
        {
            if (nodes == null || nodes.Length == 0)
            {
                return;
            }
            var clones = nodes.Select(n => n.Clone()).ToList();
            scaffolds.AddRange(clones.OfType<TemplScaffold>());
            ScaffoldChange?.Invoke(clones);
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

        private void RemoveNode(TemplScaffoldNode node)
        {
            if (node.parent != null)
            {
                node.parent.children.Remove(node);
            }
            else if (node is TemplScaffold scaffold)
            {
                scaffolds.Remove(scaffold);
            }
            else
            {
                log.Error($"Could not remove scaffold node {node.name}");
            }
        }

        private static TemplScaffoldNode AddNode<T>(TemplScaffoldNode node, string name)
            where T : TemplScaffoldNode, new()
        {
            var current = node;
            if (node is TemplScaffoldFile)
            {
                current = node.parent;
            }
            var newNode = new T()
            {
                name = name,
                parent = current
            };
            current.children.Add(newNode);
            return newNode;
        }

        private static TemplSettings GetSettings() =>
            EditorBuildSettings.TryGetConfigObject(
                DefaultConfigObjectName,
                out TemplSettings settings)
            ? settings
            : null;
    }
}
