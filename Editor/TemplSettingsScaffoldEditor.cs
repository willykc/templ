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
        private const int ToolbarButtonWidth = 25;
        private const int ToolbarButtonTopPadding = 1;
        private const string ScaffoldsTitle = "Scaffolds";
        private const string ScribanIconPath = "Packages/com.willykc.templ/Icons/sbn_logo.png";
        private const string DirectoryIconName = "Folder Icon";
        private const string DeleteIconName = "Toolbar Minus";
        private const string PlusIconName = "Toolbar Plus";
        private const string CloneIconName = "BuildSettings.N3DS On";
        private const string ScaffoldIconName = "d_VerticalLayoutGroup Icon";
        private const string CreateScaffoldTooltip = "Create Scaffold";
        private const string DirectoryTooltip = "Add Directory";
        private const string FileTooltip = "Add Template";
        private const string DeleteTooltip = "Delete Node(s)";
        private const string CloneTooltip = "Clone Node(s)";
        private const string SessionStateKeyPrefix = "TemplScaffold";
        private const string MiniButtonLeftStyleName = "miniButtonLeft";
        private const string MiniButtonRightStyleName = "miniButtonRight";
        private const string MiniButtonMidStyleName = "miniButtonMid";
        private const string MiniButtonStyleName = "miniButton";

        private static GUIStyle LeftButtonStyle;
        private static GUIStyle RightButtonStyle;
        private static GUIStyle MidButtonStyle;
        private static GUIStyle MiniButtonStyle;
        private static Texture2D PlusIcon;
        private static Texture2D DirectoryIcon;
        private static Texture2D FileIcon;
        private static Texture2D CloneIcon;
        private static Texture2D DeleteIcon;
        private static Texture2D ScaffoldIcon;
        private static GUIContent CreateScaffoldButtonContent;
        private static GUIContent DirectoryButtonContent;
        private static GUIContent FileButtonContent;
        private static GUIContent CloneButtonContent;
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
                serializedObject, ScaffoldIcon, DirectoryIcon, FileIcon);
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
            DrawButton(CreateScaffoldButtonContent, MiniButtonStyle, _ => settings.CreateScaffold(),
                nameof(settings.CreateScaffold));
            GUILayout.FlexibleSpace();
            GUI.enabled = scaffoldsTreeView.HasSelection();
            DrawButton(DirectoryButtonContent, LeftButtonStyle, settings.AddScaffoldDirectoryNode,
                nameof(settings.AddScaffoldDirectoryNode));
            DrawButton(FileButtonContent, MidButtonStyle, settings.AddScaffoldFileNode,
                nameof(settings.AddScaffoldFileNode));
            DrawButton(CloneButtonContent, MidButtonStyle, settings.CloneScaffoldNodes,
                nameof(settings.CloneScaffoldNodes),
                (Event.current.command || Event.current.control) && KeyPress(KeyCode.D));
            DrawButton(DeleteButtonContent, RightButtonStyle, settings.RemoveScaffoldNodes,
                nameof(settings.RemoveScaffoldNodes), KeyPress(KeyCode.Delete));
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            var rect = GUILayoutUtility
                .GetRect(0, MaxScaffoldsWidth, 0, scaffoldsTreeView.totalHeight);
            scaffoldsTreeView.OnGUI(rect);
        }

        private void DrawButton(GUIContent content,
            GUIStyle style,
            Action<TemplScaffoldNode[]> action,
            string name,
            bool key = false)
        {
            if (GUILayout.Button(content, style, GUILayout.MaxWidth(ToolbarButtonWidth)) || key)
            {
                ScaffoldAction(action, name);
            }
        }

        private bool KeyPress(KeyCode code) =>
            Event.current.type == EventType.KeyDown &&
            Event.current.isKey &&
            Event.current.keyCode == code;

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
            CloneIcon = CloneIcon
                ? CloneIcon
                : EditorGUIUtility.FindTexture(CloneIconName);
        }

        private void CreateButtonContents()
        {
            CreateScaffoldButtonContent ??= new GUIContent(PlusIcon, CreateScaffoldTooltip);
            DirectoryButtonContent ??= new GUIContent(DirectoryIcon, DirectoryTooltip);
            FileButtonContent ??= new GUIContent(FileIcon, FileTooltip);
            DeleteButtonContent ??= new GUIContent(DeleteIcon, DeleteTooltip);
            CloneButtonContent ??= new GUIContent(CloneIcon, CloneTooltip);
        }

        private void CreateButtonStyles()
        {
            ToolbarButtonPadding ??= new RectOffset(0, 0, ToolbarButtonTopPadding, 0);
            LeftButtonStyle ??= new GUIStyle(MiniButtonLeftStyleName)
            {
                padding = ToolbarButtonPadding
            };
            MidButtonStyle ??= new GUIStyle(MiniButtonMidStyleName)
            {
                padding = ToolbarButtonPadding
            };
            RightButtonStyle ??= new GUIStyle(MiniButtonRightStyleName)
            {
                padding = ToolbarButtonPadding
            };
            MiniButtonStyle ??= new GUIStyle(MiniButtonStyleName)
            {
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
