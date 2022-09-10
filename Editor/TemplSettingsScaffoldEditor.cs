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
        private const int ToolbarButtonWidth = 30;
        private const int ToolbarButtonTopPadding = 1;
        private const string ScaffoldsTitle = "Scaffolds";
        private const string ScribanIconPath = "Packages/com.willykc.templ/Icons/sbn_logo.png";
        private const string DirectoryIconName = "Folder Icon";
        private const string DeleteIconName = "Toolbar Minus";
        private const string PlusIconName = "Toolbar Plus";
        private const string ScaffoldIconName = "d_VerticalLayoutGroup Icon";
        private const string CreateScaffoldTooltip = "Create Scaffold";
        private const string DirectoryTooltip = "Add Directory";
        private const string FileTooltip = "Add Template";
        private const string DeleteTooltip = "Delete Node";
        private const string SessionStateKeyPrefix = "TemplScaffold";
        private const string MiniButtonLeftStyleName = "miniButtonLeft";
        private const string MiniButtonRightStyleName = "miniButtonRight";
        private const string MiniButtonStyleName = "miniButton";

        private static GUIStyle DirectoryButtonStyle;
        private static GUIStyle FileButtonStyle;
        private static GUIStyle MiniButtonStyle;
        private static Texture2D PlusIcon;
        private static Texture2D DirectoryIcon;
        private static Texture2D FileIcon;
        private static Texture2D DeleteIcon;
        private static Texture2D ScaffoldIcon;
        private static GUIContent CreateScaffoldButtonContent;
        private static GUIContent DirectoryButtonContent;
        private static GUIContent FileButtonContent;
        private static GUIContent DeleteButtonContent;
        private static RectOffset ToolbarButtonPadding;

        private SerializedProperty scaffoldsProperty;
        private TemplScaffoldTreeView scaffoldsTreeView;

        private void OnEnableScaffolds()
        {
            LoadIcons();
            CreateButtonContents();
            var scaffoldsPropertyName = nameof(TemplSettings.Scaffolds).ToLower();
            scaffoldsProperty = serializedObject.FindProperty(scaffoldsPropertyName);
            var treeViewState = new TreeViewState();
            var jsonState = SessionState
                .GetString(SessionStateKeyPrefix + settings.GetInstanceID(), string.Empty);
            if (!string.IsNullOrEmpty(jsonState))
                JsonUtility.FromJsonOverwrite(jsonState, treeViewState);
            scaffoldsTreeView = new TemplScaffoldTreeView(
                treeViewState,
                settings, ScaffoldIcon, DirectoryIcon, FileIcon);
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
            CreateButtonStyles();
            if (!Foldout(scaffoldsProperty, ScaffoldsTitle))
            {
                return;
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(CreateScaffoldButtonContent, MiniButtonStyle))
            {
                ScaffoldAction(_ => settings.CreateNewScaffold(),
                    nameof(settings.CreateNewScaffold));
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(DirectoryButtonContent, DirectoryButtonStyle))
            {
                ScaffoldAction(settings.AddScaffoldDirectoryNode,
                    nameof(settings.AddScaffoldDirectoryNode));
            }
            if (GUILayout.Button(FileButtonContent, FileButtonStyle))
            {
                ScaffoldAction(settings.AddScaffoldFileNode,
                    nameof(settings.AddScaffoldFileNode));
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(DeleteButtonContent, MiniButtonStyle))
            {
                ScaffoldAction(settings.RemoveScaffoldNodes,
                    nameof(settings.RemoveScaffoldNodes));
            }
            EditorGUILayout.EndHorizontal();
            var rect = GUILayoutUtility
                .GetRect(0, MaxScaffoldsWidth, 0, scaffoldsTreeView.totalHeight);
            scaffoldsTreeView.OnGUI(rect);
        }

        private void LoadIcons()
        {
            ScaffoldIcon = ScaffoldIcon
                ? ScaffoldIcon
                : EditorGUIUtility.FindTexture(ScaffoldIconName);
            DirectoryIcon = DirectoryIcon
                ? DirectoryIcon
                : EditorGUIUtility.FindTexture(DirectoryIconName);
            FileIcon = FileIcon
                ? FileIcon
                : AssetDatabase.LoadAssetAtPath<Texture2D>(ScribanIconPath);
            DeleteIcon = DeleteIcon
                ? DeleteIcon
                : EditorGUIUtility.FindTexture(DeleteIconName);
            PlusIcon = PlusIcon
                ? PlusIcon
                : EditorGUIUtility.FindTexture(PlusIconName);
        }

        private void CreateButtonContents()
        {
            CreateScaffoldButtonContent ??= new GUIContent(PlusIcon, CreateScaffoldTooltip);
            DirectoryButtonContent ??= new GUIContent(DirectoryIcon, DirectoryTooltip);
            FileButtonContent ??= new GUIContent(FileIcon, FileTooltip);
            DeleteButtonContent ??= new GUIContent(DeleteIcon, DeleteTooltip);
        }

        private void CreateButtonStyles()
        {
            ToolbarButtonPadding ??= new RectOffset(0, 0, ToolbarButtonTopPadding, 0);
            DirectoryButtonStyle ??= new GUIStyle(MiniButtonLeftStyleName)
            {
                fixedWidth = ToolbarButtonWidth,
                padding = ToolbarButtonPadding
            };
            FileButtonStyle ??= new GUIStyle(MiniButtonRightStyleName)
            {
                fixedWidth = ToolbarButtonWidth,
                padding = ToolbarButtonPadding
            };
            MiniButtonStyle ??= new GUIStyle(MiniButtonStyleName)
            {
                fixedWidth = ToolbarButtonWidth,
                padding = ToolbarButtonPadding
            };
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
