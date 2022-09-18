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
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Willykc.Templ.Editor.Scaffold
{
    [CustomEditor(typeof(TemplScaffold))]
    public class TemplScaffoldEditor : UnityEditor.Editor
    {
        private const int MaxScaffoldsWidth = 1000;
        private const int ToolbarButtonWidth = 30;
        private const int ToolbarButtonVerticalPadding = 1;
        private const string DirectoryIconName = "Folder Icon";
        private const string DeleteIconName = "Toolbar Minus";
        private const string CloneIconName = "BuildSettings.N3DS On";
        private const string DirectoryTooltip = "Add Directory";
        private const string FileTooltip = "Add Template";
        private const string DeleteTooltip = "Delete Node(s)";
        private const string CloneTooltip = "Clone Node(s)";
        private const string SessionStateKeyPrefix = "TemplScaffold";
        private const string MiniButtonLeftStyleName = "miniButtonLeft";
        private const string MiniButtonRightStyleName = "miniButtonRight";
        private const string MiniButtonMidStyleName = "miniButtonMid";
        private const string MiniButtonStyleName = "miniButton";
        private const string ScribanIconPath = "Packages/com.willykc.templ/Icons/sbn_logo.png";
        private const string ScaffoldIconPath =
            "Packages/com.willykc.templ/Icons/scaffold_logo.png";

        private static GUIStyle LeftButtonStyle;
        private static GUIStyle RightButtonStyle;
        private static GUIStyle MidButtonStyle;
        private static GUIStyle MiniButtonStyle;
        private static Texture2D DirectoryIcon;
        private static Texture2D FileIcon;
        private static Texture2D CloneIcon;
        private static Texture2D DeleteIcon;
        private static Texture2D ScaffoldIcon;
        private static GUIContent DirectoryButtonContent;
        private static GUIContent FileButtonContent;
        private static GUIContent CloneButtonContent;
        private static GUIContent DeleteButtonContent;
        private static RectOffset ToolbarButtonPadding;

        private TemplScaffold scaffold;
        private TemplScaffoldTreeView scaffoldsTreeView;

        private bool IsRootSelected =>
            !scaffoldsTreeView.GetNodeSelection().Contains(scaffold.Root) &&
            scaffoldsTreeView.HasSelection();

        public override void OnInspectorGUI()
        {
            CreateButtonStyles();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawButton(DirectoryButtonContent, LeftButtonStyle, scaffold.AddScaffoldDirectoryNode,
                nameof(scaffold.AddScaffoldDirectoryNode));
            DrawButton(FileButtonContent, MidButtonStyle, scaffold.AddScaffoldFileNode,
                nameof(scaffold.AddScaffoldFileNode));
            GUI.enabled = IsRootSelected;
            DrawButton(CloneButtonContent, MidButtonStyle, scaffold.CloneScaffoldNodes,
                nameof(scaffold.CloneScaffoldNodes),
                (Event.current.command || Event.current.control) && KeyPress(KeyCode.D));
            DrawButton(DeleteButtonContent, RightButtonStyle, scaffold.RemoveScaffoldNodes,
                nameof(scaffold.RemoveScaffoldNodes), KeyPress(KeyCode.Delete));
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            var rect = GUILayoutUtility
                .GetRect(0, MaxScaffoldsWidth, 0, scaffoldsTreeView.totalHeight);
            scaffoldsTreeView.OnGUI(rect);
        }

        private void OnEnable()
        {
            scaffold = target as TemplScaffold;
            LoadIcons();
            CreateButtonContents();
            var treeViewState = new TreeViewState();
            var jsonState = SessionState
                .GetString(SessionStateKeyPrefix + scaffold.GetInstanceID(), string.Empty);
            if (!string.IsNullOrEmpty(jsonState))
                JsonUtility.FromJsonOverwrite(jsonState, treeViewState);
            scaffoldsTreeView = new TemplScaffoldTreeView(
                treeViewState,
                scaffold, ScaffoldIcon, DirectoryIcon, FileIcon);
            scaffoldsTreeView.BeforeDrop += OnBeforeScaffoldDrop;
            Undo.undoRedoPerformed += OnChangeScaffolds;
            scaffold.FullReset += OnChangeScaffolds;
            OnChangeScaffolds();
        }

        private void OnDisable()
        {
            scaffoldsTreeView.BeforeDrop -= OnBeforeScaffoldDrop;
            Undo.undoRedoPerformed -= OnChangeScaffolds;
            scaffold.FullReset -= OnChangeScaffolds;
            SessionState.SetString(SessionStateKeyPrefix + scaffold.GetInstanceID(),
                JsonUtility.ToJson(scaffoldsTreeView.state));
        }

        private void OnChangeScaffolds() => scaffoldsTreeView.Reload();

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
            DirectoryIcon = DirectoryIcon
                ? DirectoryIcon
                : EditorGUIUtility.FindTexture(DirectoryIconName);
            FileIcon = FileIcon
                ? FileIcon
                : AssetDatabase.LoadAssetAtPath<Texture2D>(ScribanIconPath);
            DeleteIcon = DeleteIcon
                ? DeleteIcon
                : EditorGUIUtility.FindTexture(DeleteIconName);
            CloneIcon = CloneIcon
                ? CloneIcon
                : EditorGUIUtility.FindTexture(CloneIconName);
            ScaffoldIcon = ScaffoldIcon
                ? ScaffoldIcon
                : AssetDatabase.LoadAssetAtPath<Texture2D>(ScaffoldIconPath);
        }

        private void CreateButtonContents()
        {
            DirectoryButtonContent ??= new GUIContent(DirectoryIcon, DirectoryTooltip);
            FileButtonContent ??= new GUIContent(FileIcon, FileTooltip);
            DeleteButtonContent ??= new GUIContent(DeleteIcon, DeleteTooltip);
            CloneButtonContent ??= new GUIContent(CloneIcon, CloneTooltip);
        }

        private void CreateButtonStyles()
        {
            ToolbarButtonPadding ??=
                new RectOffset(0, 0, ToolbarButtonVerticalPadding, ToolbarButtonVerticalPadding);
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
            Undo.RecordObject(scaffold, name);
            var selectedNodes = scaffoldsTreeView.GetNodeSelection();
            action(selectedNodes);
        }

        private void OnBeforeScaffoldDrop() =>
            Undo.RecordObject(scaffold, nameof(scaffold.MoveScaffoldNodes));
    }
}
