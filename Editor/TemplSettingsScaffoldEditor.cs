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
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Willykc.Templ.Editor
{
    using Scaffold;

    internal partial class TemplSettingsEditor
    {
        private const int MaxScaffoldsWidth = 1000;
        private const int NewButtonWidth = 40;
        private const int DirectoryButtonWidth = 30;
        private const int FileButtonWidth = 30;
        private const int RemoveButtonWidth = 30;
        private const string ScaffoldsTitle = "Scaffolds";
        private const string NewButtonText = "New";
        private const string ScribanIconPath = "Packages/com.willykc.templ/Icons/sbn_logo.png";
        private const string FolderIconName = "Folder Icon";
        private const string DeleteIconName = "Toolbar Minus";
        private const string ScaffoldIconName = "d_VerticalLayoutGroup Icon";
        private const string ScaffoldTooltip = "Create New Scaffold";
        private const string FolderTooltip = "Add Directory";
        private const string FileTooltip = "Add Template";
        private const string DeleteTooltip = "Delete";
        private const string SessionStateKeyPrefix = "TemplScaffold";

        private static Texture2D FolderIcon;
        private static Texture2D FileIcon;
        private static Texture2D DeleteIcon;
        private static Texture2D ScaffoldIcon;

        private SerializedProperty scaffoldsProperty;
        private TemplScaffoldTreeView scaffoldsTreeView;

        private void OnEnableScaffolds()
        {
            LoadIcons();
            scaffoldsProperty =
                serializedObject.FindProperty(nameof(TemplSettings.Scaffolds).ToLower());
            var treeViewState = new TreeViewState();
            var jsonState = SessionState
                .GetString(SessionStateKeyPrefix + settings.GetInstanceID(), "");
            if (!string.IsNullOrEmpty(jsonState))
                JsonUtility.FromJsonOverwrite(jsonState, treeViewState);
            scaffoldsTreeView = new TemplScaffoldTreeView(
                treeViewState,
                settings, ScaffoldIcon, FolderIcon, FileIcon);
            settings.ScaffoldChange += OnScaffoldChange;
            scaffoldsTreeView.BeforeDrop += OnBeforeScaffoldDrop;
            Undo.undoRedoPerformed += OnChangeScaffolds;
            settings.FullReset += OnChangeScaffolds;
            OnChangeScaffolds();
        }

        private void OnDisableScaffolds()
        {
            settings.ScaffoldChange -= OnScaffoldChange;
            scaffoldsTreeView.BeforeDrop -= OnBeforeScaffoldDrop;
            Undo.undoRedoPerformed -= OnChangeScaffolds;
            settings.FullReset -= OnChangeScaffolds;
            SessionState.SetString(SessionStateKeyPrefix + settings.GetInstanceID(),
                JsonUtility.ToJson(scaffoldsTreeView.state));
        }

        private void OnChangeScaffolds() => scaffoldsTreeView.Reload();

        private void DrawTemplScaffolds()
        {
            if (!Foldout(scaffoldsProperty, ScaffoldsTitle))
            {
                return;
            }
            var rect = GUILayoutUtility.GetRect(0, Line + Padding);
            var totalWidth = rect.width;
            rect = new Rect(rect.x, rect.y, NewButtonWidth, rect.height - Spacing);
            if (GUI.Button(rect, new GUIContent(NewButtonText, ScaffoldTooltip)))
            {
                ScaffoldAction(_ => settings.CreateNewScaffold(),
                nameof(settings.CreateNewScaffold));
            }
            rect = new Rect(
                totalWidth - FileButtonWidth - RemoveButtonWidth - (Padding * Double) - Spacing,
                rect.y,
                DirectoryButtonWidth,
                rect.height);
            if (GUI.Button(rect, new GUIContent(FolderIcon, FolderTooltip)))
            {
                ScaffoldAction(settings.AddScaffoldDirectoryNode, nameof(settings.AddScaffoldDirectoryNode));
            }
            rect = new Rect(rect.x + DirectoryButtonWidth, rect.y, FileButtonWidth, rect.height);
            if (GUI.Button(rect, new GUIContent(FileIcon, FileTooltip)))
            {
                ScaffoldAction(settings.AddScaffoldFileNode, nameof(settings.AddScaffoldFileNode));
            }
            rect = new Rect(rect.x + FileButtonWidth, rect.y, RemoveButtonWidth, rect.height);
            if (GUI.Button(rect, new GUIContent(DeleteIcon, DeleteTooltip)))
            {
                ScaffoldAction(settings.RemoveScaffoldNodes, nameof(settings.RemoveScaffoldNodes));
            }
            rect = GUILayoutUtility.GetRect(0, MaxScaffoldsWidth, 0, scaffoldsTreeView.totalHeight);
            scaffoldsTreeView.OnGUI(rect);
        }

        private void LoadIcons()
        {
            ScaffoldIcon = ScaffoldIcon
                ? ScaffoldIcon
                : EditorGUIUtility.FindTexture(ScaffoldIconName);
            FolderIcon = FolderIcon
                ? FolderIcon
                : EditorGUIUtility.FindTexture(FolderIconName);
            FileIcon = FileIcon
                ? FileIcon
                : AssetDatabase.LoadAssetAtPath<Texture2D>(ScribanIconPath);
            DeleteIcon = DeleteIcon
                ? DeleteIcon
                : EditorGUIUtility.FindTexture(DeleteIconName);
        }

        private void ScaffoldAction(Action<TemplScaffoldNode[]> action, string name)
        {
            Undo.RecordObject(settings, name);
            var selectedNodes = scaffoldsTreeView.GetNodeSelection();
            action(selectedNodes);
        }

        private void OnScaffoldChange(IReadOnlyList<TemplScaffoldNode> nodes) =>
            EditorUtility.SetDirty(settings);

        private void OnBeforeScaffoldDrop() =>
            Undo.RecordObject(settings, nameof(settings.MoveScaffoldNodes));
    }
}
