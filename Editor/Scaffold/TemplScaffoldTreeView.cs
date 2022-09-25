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
using UnityEngine;

namespace Willykc.Templ.Editor.Scaffold
{
    internal sealed class TemplScaffoldTreeView : TreeView
    {
        private const string GenericDragID = "GenericDragColumnDragging";
        private const string MultipleDragTitle = "< Multiple >";
        private const string RootName = nameof(TemplScaffold.Root);
        private const string ChildrenPropertyName = nameof(TemplScaffoldNode.children);

        private readonly TemplScaffold scaffold;
        private readonly SerializedObject serializedObject;
        private readonly List<TreeViewItem> rows = new List<TreeViewItem>(100);
        private readonly Dictionary<TemplScaffoldNode, int> nodeIDs =
            new Dictionary<TemplScaffoldNode, int>();
        private readonly IReadOnlyDictionary<Type, Texture2D> icons;
        private readonly TemplScaffoldRowView rowView;

        internal event Action BeforeDrop;
        internal event Action AfterDrop;

        internal TemplScaffoldTreeView(TreeViewState treeViewState,
            TemplScaffold scaffold,
            Texture2D scaffoldIcon,
            Texture2D directoryIcon,
            Texture2D fileIcon)
            : base(treeViewState)
        {
            this.scaffold = scaffold
                ? scaffold
                : throw new ArgumentNullException(nameof(scaffold));
            serializedObject = new SerializedObject(scaffold);
            icons = new Dictionary<Type, Texture2D>()
            {
                { typeof(TemplScaffoldRoot), scaffoldIcon },
                { typeof(TemplScaffoldFile), fileIcon },
                { typeof(TemplScaffoldDirectory), directoryIcon }
            };
            rowView = new TemplScaffoldRowView();
            scaffold.Change += OnScaffoldChange;
            scaffold.FullReset += Reload;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
        }

        internal TemplScaffoldNode[] GetNodeSelection() => rows
            .Where(r => GetSelection()
            .Contains(r.id))
            .Cast<TemplScaffoldTreeViewItem>()
            .Select(r => r.Node)
            .ToArray();

        internal void EditSelectedNode()
        {
            var firstID = GetSelection().FirstOrDefault();
            SetEditMode(firstID);
        }

        internal bool IsNodeExpanded(TemplScaffoldNode node) => IsExpanded(GetID(node));

        protected override void DoubleClickedItem(int id)
        {
            SetEditMode(id);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as TemplScaffoldTreeViewItem;
            item.Indent = GetContentIndent(item);
            EditorGUI.BeginChangeCheck();
            rowView.DrawRow(item, args.rowRect);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                item.UpdateValidity();
            }
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item) =>
            rowView.GetRowHeight(item);

        protected override IList<int> GetAncestors(int id)
        {
            var node = nodeIDs.First(kvp => kvp.Value == id).Key;
            var parentID = GetID(node);
            var parentIDs = new List<int>() { parentID };

            while (node.parent != null)
            {
                node = node.parent;
                parentID = GetID(node);
                parentIDs.Add(parentID);
            }

            return parentIDs;
        }

        protected override IList<int> GetDescendantsThatHaveChildren(int id)
        {
            var stack = new Stack<TemplScaffoldNode>();
            var start = nodeIDs.FirstOrDefault(kvp => kvp.Value == id).Key ?? scaffold.Root;
            stack.Push(start);

            var descendantsWithChildren = new List<int>();

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (current.children.Count == 0)
                {
                    continue;
                }

                descendantsWithChildren.Add(GetID(current));

                foreach (var child in current.children)
                {
                    stack.Push(child);
                }
            }

            return descendantsWithChildren;
        }

        protected override TreeViewItem BuildRoot() =>
            new TreeViewItem() { id = 0, depth = -1, displayName = RootName };

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            rows.Clear();
            serializedObject.Update();
            var rootPropertyName = RootName.ToLower();
            var rootProperty = serializedObject.FindProperty(rootPropertyName);
            AddChildrenRecursive(scaffold.Root, rootProperty, 0, rows);
            SetupParentsAndChildrenFromDepths(root, rows);
            return rows;
        }

        protected override bool CanStartDrag(CanStartDragArgs args) => true;

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            if (hasSearch)
            {
                return;
            }

            DragAndDrop.PrepareStartDrag();
            var draggedRows = GetRows()
                .Where(item => args.draggedItemIDs.Contains(item.id))
                .ToList();
            DragAndDrop.SetGenericData(GenericDragID, draggedRows);
            DragAndDrop.objectReferences = new UnityEngine.Object[] { };
            string title = draggedRows.Count == 1
                ? draggedRows[0].displayName
                : MultipleDragTitle;
            DragAndDrop.StartDrag(title);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            var genericData = DragAndDrop.GetGenericData(GenericDragID);

            if (!(genericData is List<TreeViewItem> draggedItems))
            {
                return DragAndDropVisualMode.None;
            }

            var draggedScaffoldItems = draggedItems
                .Cast<TemplScaffoldTreeViewItem>();

            switch (args.dragAndDropPosition)
            {
                case DragAndDropPosition.UponItem:
                case DragAndDropPosition.BetweenItems:
                    {
                        var parentItem = args.parentItem as TemplScaffoldTreeViewItem;
                        var validDrag = IsValidDrag(parentItem, draggedScaffoldItems);
                        var insertIndex = Mathf.Clamp(args.insertAtIndex, 0, args.insertAtIndex);

                        if (args.performDrop && validDrag)
                        {
                            OnDropItemsAtIndex(draggedScaffoldItems, parentItem.Node, insertIndex);
                        }

                        return validDrag
                            ? DragAndDropVisualMode.Move
                            : DragAndDropVisualMode.None;
                    }
                default:
                    return DragAndDropVisualMode.None;
            }
        }

        private void SetEditMode(int nodeID)
        {
            if (nodeID <= 0 || nodeID == GetID(scaffold.Root))
            {
                return;
            }

            rowView.ToggleEditMode(nodeID);
            Reload();
        }

        private void OnDropItemsAtIndex(
            IEnumerable<TemplScaffoldTreeViewItem> draggedItems,
            TemplScaffoldNode parent,
            int insertIndex)
        {
            BeforeDrop?.Invoke();

            var draggedNodes = draggedItems
                .Select(i => i.Node)
                .ToArray();

            scaffold.MoveScaffoldNodes(parent, insertIndex, draggedNodes);

            AfterDrop?.Invoke();
        }

        private bool IsValidDrag(TemplScaffoldTreeViewItem parent,
            IEnumerable<TemplScaffoldTreeViewItem> draggedItems)
        {
            var isRootDragged = draggedItems
                .Select(i => i.Node)
                .Any(n => n is TemplScaffoldRoot);

            if (parent == null || parent.Node is TemplScaffoldFile || isRootDragged)
            {
                return false;
            }

            TreeViewItem currentParent = parent;

            while (currentParent != null)
            {
                if (draggedItems.Contains(currentParent))
                {
                    return false;
                }

                currentParent = currentParent.parent;
            }

            return true;
        }

        private void OnScaffoldChange(IReadOnlyList<TemplScaffoldNode> nodes)
        {
            var selectedIDs = nodes.Select(n => GetID(n)).ToArray();
            rowView.ResetEditMode();
            Reload();
            SetSelection(selectedIDs, TreeViewSelectionOptions.RevealAndFrame);
        }

        private void AddChildrenRecursive(
            TemplScaffoldNode parent,
            SerializedProperty serializedParent,
            int depth,
            List<TreeViewItem> rows)
        {
            var children = parent.children;
            var serializedChildren = serializedParent.FindPropertyRelative(ChildrenPropertyName);
            var id = GetID(parent);
            var icon = GetIcon(parent);
            var item = new TemplScaffoldTreeViewItem(id, depth, parent, serializedParent)
            {
                icon = icon
            };

            rows.Add(item);

            if (children.Count == 0)
            {
                return;
            }

            if (IsExpanded(id))
            {
                for (var i = 0; i < children.Count; i++)
                {
                    var child = children[i];
                    var serializedChild = serializedChildren.GetArrayElementAtIndex(i);
                    AddChildrenRecursive(child, serializedChild, depth + 1, rows);
                }
            }
            else
            {
                item.children = CreateChildListForCollapsedParent();
            }
        }

        private int GetID(TemplScaffoldNode node)
        {
            if (!nodeIDs.TryGetValue(node, out var id))
            {
                var last = nodeIDs.LastOrDefault();
                id = last.Value + 1;
                nodeIDs.Add(node, id);
            }

            return id;
        }

        private Texture2D GetIcon(TemplScaffoldNode node)
        {
            if (icons.TryGetValue(node.GetType(), out var icon))
            {
                return icon;
            }

            return null;
        }
    }
}
