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
using System.Linq;
using UnityEngine;

namespace Willykc.Templ.Editor.Scaffold
{
    using ILogger = Abstraction.ILogger;
    using Logger = Abstraction.Logger;

    [CreateAssetMenu(fileName = NewPrefix + nameof(TemplScaffold), menuName = MenuName, order = 2)]
    internal sealed class TemplScaffold : ScriptableObject
    {
        private const string MenuName = "Templ/Scaffold Definition";
        private const string NewPrefix = "New";
        private const string DefaultFileName = NewPrefix + "File";
        private const string DefaultDirectoryName = NewPrefix + "Directory";

        private static readonly List<TemplScaffoldNode> EmptyList = new List<TemplScaffoldNode>(0);

        internal event Action FullReset;
        internal event Action<IReadOnlyList<TemplScaffoldNode>> Change;

        [SerializeField]
        private ScriptableObject defaultInput;
        [SerializeReference]
        private TemplScaffoldRoot root = GetNewRoot();

        private readonly ILogger log;

        internal ScriptableObject DefaultInput => defaultInput;
        internal TemplScaffoldRoot Root => root;
        internal bool IsValid => root.IsValid;

        internal TemplScaffold()
        {
            log = Logger.Instance;
        }

        private void Reset()
        {
            root = GetNewRoot();
            FullReset?.Invoke();
        }

        internal void AddScaffoldFileNode(TemplScaffoldNode[] nodes)
        {
            nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            if (nodes.Length == 0)
            {
                nodes = new[] { root };
            }
            var newNodes = nodes
                .Select(n => AddNode<TemplScaffoldFile>(n, DefaultFileName))
                .ToList();
            Change?.Invoke(newNodes);
        }

        internal void AddScaffoldDirectoryNode(TemplScaffoldNode[] nodes)
        {
            nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            if (nodes.Length == 0)
            {
                nodes = new[] { root };
            }
            var newNodes = nodes
                .Select(n => AddNode<TemplScaffoldDirectory>(n, DefaultDirectoryName))
                .ToList();
            Change?.Invoke(newNodes);
        }

        internal void RemoveScaffoldNodes(TemplScaffoldNode[] nodes)
        {
            nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            foreach (var node in nodes)
            {
                node.Parent?.RemoveChild(node);
            }
            Change?.Invoke(EmptyList);
        }

        internal void MoveScaffoldNodes(
            TemplScaffoldNode parent,
            int insertIndex,
            TemplScaffoldNode[] draggedNodes)
        {
            parent ??= root;
            if (draggedNodes == null || draggedNodes.Length == 0)
            {
                return;
            }

            if (insertIndex < 0)
            {
                throw new ArgumentException($"Invalid input: {nameof(insertIndex)} " +
                    "must not be negative");
            }

            var isRootDragged = draggedNodes
                .Any(n => n is TemplScaffoldRoot);
            if (isRootDragged)
            {
                throw new InvalidOperationException("Scaffold root node can not be reparented");
            }

            if (parent is TemplScaffoldFile)
            {
                throw new InvalidOperationException("Scaffold File nodes can not be parent nodes");
            }

            if (insertIndex > 0)
            {
                var subSet = new TemplScaffoldNode[insertIndex];
                Array.Copy(parent.Children.ToArray(), subSet, insertIndex);
                insertIndex -= subSet.Count(draggedNodes.Contains);
            }

            foreach (var node in draggedNodes)
            {
                node.Parent.RemoveChild(node);
                node.Parent = parent;
            }

            parent.InsertChildrenRange(insertIndex, draggedNodes);
            Change?.Invoke(draggedNodes);
        }

        internal void CloneScaffoldNodes(TemplScaffoldNode[] nodes)
        {
            if (nodes == null || nodes.Length == 0)
            {
                return;
            }
            var clones = nodes
                .Where(n => !(n is TemplScaffoldRoot))
                .Select(n => n.Clone())
                .ToList();
            Change?.Invoke(clones);
        }

        internal bool ContainsTemplate(ScribanAsset template) =>
            root.Children.Any(c => c.ContainsTemplate(template));

        private static TemplScaffoldRoot GetNewRoot() =>
            new TemplScaffoldRoot() { name = nameof(Root) };

        private static TemplScaffoldNode AddNode<T>(TemplScaffoldNode node, string name)
            where T : TemplScaffoldNode, new()
        {
            var current = node;
            if (node is TemplScaffoldFile)
            {
                current = node.Parent;
            }

            var newNode = new T()
            {
                name = name,
                Parent = current
            };

            current.AddChild(newNode);
            return newNode;
        }
    }
}
