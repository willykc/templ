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
    [CreateAssetMenu(fileName = NewPrefix + nameof(TemplScaffold), menuName = MenuName, order = 2)]
    public class TemplScaffold : ScriptableObject
    {
        internal const string NameOfDefaultInput = nameof(defaultInput);
        internal const string NameOfRoot = nameof(root);

        protected const string NewPrefix = "New";

        private const string MenuName = "Templ/Scaffold Definition";
        private const string DefaultFileName = NewPrefix + "File";
        private const string DefaultDirectoryName = NewPrefix + "Directory";

        private static readonly List<TemplScaffoldNode> EmptyList = new List<TemplScaffoldNode>(0);

        internal event Action AfterReset;
        internal event Action<IReadOnlyList<TemplScaffoldNode>> Changed;

        [SerializeField]
        protected ScriptableObject defaultInput;
        [SerializeReference]
        private TemplScaffoldRoot root = GetNewRoot();

        /// <summary>
        /// Default input instance.
        /// </summary>
        public ScriptableObject DefaultInput => defaultInput;

        internal virtual TemplScaffoldRoot Root => root;
        internal virtual bool IsValid => root.IsValid;

        protected void Reset()
        {
            root = GetNewRoot();
            defaultInput = null;
            AfterReset?.Invoke();
        }

        internal void AddScaffoldFileNodes(TemplScaffoldNode[] nodes)
        {
            nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            if (nodes.Length == 0)
            {
                nodes = new[] { root };
            }
            var newNodes = nodes
                .Select(n => AddNode<TemplScaffoldFile>(n, DefaultFileName))
                .ToList();
            Changed?.Invoke(newNodes);
        }

        internal void AddScaffoldDirectoryNodes(TemplScaffoldNode[] nodes)
        {
            nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            if (nodes.Length == 0)
            {
                nodes = new[] { root };
            }
            var newNodes = nodes
                .Select(n => AddNode<TemplScaffoldDirectory>(n, DefaultDirectoryName))
                .ToList();
            Changed?.Invoke(newNodes);
        }

        internal void RemoveScaffoldNodes(TemplScaffoldNode[] nodes)
        {
            nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            foreach (var node in nodes)
            {
                node.Parent?.RemoveChild(node);
            }
            Changed?.Invoke(EmptyList);
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

            parent.InsertChildrenRange(insertIndex, draggedNodes);
            Changed?.Invoke(draggedNodes);
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
            Changed?.Invoke(clones);
        }

        internal virtual bool ContainsTemplate(ScribanAsset template) =>
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
                name = name
            };

            current.AddChild(newNode);
            return newNode;
        }
    }
}
