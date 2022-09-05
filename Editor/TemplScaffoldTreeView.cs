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
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Willykc.Templ.Editor
{
    internal class TemplScaffoldTreeView : TreeView
    {
        private const string RootName = "Root";

        private readonly TemplSettings settings;
        private readonly List<TreeViewItem> rows = new List<TreeViewItem>(100);
        private readonly Dictionary<TemplScaffoldNode, int> ids =
            new Dictionary<TemplScaffoldNode, int>();

        internal TemplScaffoldTreeView(TreeViewState treeViewState, TemplSettings settings)
            : base(treeViewState)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            settings.ScaffoldChange += Reload;
            settings.FullReset += Reload;
            showAlternatingRowBackgrounds = true;
            Reload();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);
        }

        protected override TreeViewItem BuildRoot() =>
            new TreeViewItem() { id = 0, depth = -1, displayName = RootName };

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            rows.Clear();
            AddChildrenRecursive(settings.Scaffolds, 0, rows);
            SetupParentsAndChildrenFromDepths(root, rows);
            return rows;
        }

        private void AddChildrenRecursive(
            IReadOnlyList<TemplScaffoldNode> children,
            int depth,
            List<TreeViewItem> rows)
        {
            foreach (var child in children)
            {
                var id = GetId(child);
                var item = new TemplScaffoldTreeViewItem(id, depth, child);
                rows.Add(item);

                if (child.children.Count > 0)
                {
                    if (IsExpanded(id))
                    {
                        AddChildrenRecursive(child.children, depth + 1, rows);
                    }
                    else
                    {
                        item.children = CreateChildListForCollapsedParent();
                    }
                }
            }
        }

        private int GetId(TemplScaffoldNode node)
        {
            if(!ids.TryGetValue(node, out var id))
            {
                var last = ids.LastOrDefault();
                id = last.Value + 1;
                ids.Add(node, id);
            }
            return id;
        }

        internal TemplScaffoldNode[] GetNodeSelection() => rows
            .Where(r => GetSelection()
            .Contains(r.id))
            .Cast<TemplScaffoldTreeViewItem>()
            .Select(r => r.Node)
            .ToArray();
    }
}
