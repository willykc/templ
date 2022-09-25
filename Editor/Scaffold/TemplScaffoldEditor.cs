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
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Willykc.Templ.Editor.Scaffold
{
    [CustomEditor(typeof(TemplScaffold))]
    internal sealed class TemplScaffoldEditor : UnityEditor.Editor
    {
        private const int MaxScaffoldsWidth = 1000;
        private const int ToolbarButtonWidth = 30;
        private const int ToolbarButtonVerticalPadding = 1;
        private const string DirectoryIconName = "Folder Icon";
        private const string DeleteIconName = "Toolbar Minus";
        private const string CloneIconName = "BuildSettings.N3DS On@2x";
        private const string EditIconName = "CustomTool@2x";
        private const string ExpandCollapseIconName = "UnityEditor.SceneHierarchyWindow@2x";
        private const string DirectoryTooltip = "Add Directory";
        private const string FileTooltip = "Add Template";
        private const string DeleteTooltip = "Delete Node(s)";
        private const string CloneTooltip = "Clone Node(s)";
        private const string EditTooltip = "Edit Node (Double Click)";
        private const string ExpandCollapseTooltip = "Expand/Collapse All";
        private const string SessionStateKeyPrefix = "TemplScaffold";
        private const string MiniButtonLeftStyleName = "miniButtonLeft";
        private const string MiniButtonRightStyleName = "miniButtonRight";
        private const string MiniButtonMidStyleName = "miniButtonMid";
        private const string MiniButtonStyleName = "miniButton";
        private const string ScribanIconPath = "Packages/com.willykc.templ/Icons/sbn_logo.png";
        private const string ScaffoldIconPath =
            "Packages/com.willykc.templ/Icons/scaffold_logo.png";
        private const string WarningMessage = "This scaffold is not currently referenced in " +
            "Templ settings.";

        private static readonly int[] NoIDs = new int[] { };
        private static readonly string ErrorMessage = "Invalid nodes detected. All node fields " +
            $"must have values. {nameof(TemplScaffoldFile.template).Capitalize()}s must be " +
            "valid. File and Directory names must not contain invalid characters and must be " +
            "unique under the same parent node. Directories must not be empty. " +
            "Templ will only deploy valid scaffolds.";

        private static GUIStyle LeftButtonStyle;
        private static GUIStyle RightButtonStyle;
        private static GUIStyle MidButtonStyle;
        private static GUIStyle MiniButtonStyle;
        private static Texture2D DirectoryIcon;
        private static Texture2D FileIcon;
        private static Texture2D CloneIcon;
        private static Texture2D DeleteIcon;
        private static Texture2D ScaffoldIcon;
        private static Texture2D EditIcon;
        private static Texture2D ExpandCollapseIcon;
        private static GUIContent DirectoryButtonContent;
        private static GUIContent FileButtonContent;
        private static GUIContent CloneButtonContent;
        private static GUIContent DeleteButtonContent;
        private static GUIContent EditButtonContent;
        private static GUIContent ExpandCollapseButtonContent;
        private static RectOffset ToolbarButtonPadding;

        private TemplScaffold scaffold;
        private TemplScaffoldTreeView scaffoldTreeView;
        private SerializedProperty inputProperty;
        private bool isScaffoldValid;
        private bool isReferencedInSettings;

        private bool IsRootSelected =>
            !scaffoldTreeView.GetNodeSelection().Contains(scaffold.Root) &&
            scaffoldTreeView.HasSelection();

        private bool RootHasChildren => scaffold.Root.children.Count > 0;

        public override void OnInspectorGUI()
        {
            CreateButtonStyles();

            if (!isReferencedInSettings)
            {
                EditorGUILayout.HelpBox(WarningMessage, MessageType.Warning);
            }

            DrawInputProperty();
            DrawToolbar();
            var rect = GUILayoutUtility
                .GetRect(0, MaxScaffoldsWidth, 0, scaffoldTreeView.totalHeight);
            EditorGUI.BeginChangeCheck();
            scaffoldTreeView.OnGUI(rect);

            if (EditorGUI.EndChangeCheck())
            {
                OnChangeNodeFields();
            }

            if (!isScaffoldValid)
            {
                EditorGUILayout.HelpBox(ErrorMessage, MessageType.Error);
            }
        }

        private void OnEnable()
        {
            scaffold = target as TemplScaffold;
            var settings = TemplSettings.Instance;
            isReferencedInSettings = settings && settings.Scaffolds.Contains(scaffold);
            inputProperty = serializedObject.FindProperty(nameof(TemplScaffold.defaultInput));
            LoadIcons();
            CreateButtonContents();
            var treeViewState = new TreeViewState();
            var jsonState = SessionState
                .GetString(SessionStateKeyPrefix + scaffold.GetInstanceID(), string.Empty);
            if (!string.IsNullOrEmpty(jsonState))
                JsonUtility.FromJsonOverwrite(jsonState, treeViewState);
            scaffoldTreeView = new TemplScaffoldTreeView(
                treeViewState,
                scaffold, ScaffoldIcon, DirectoryIcon, FileIcon);
            scaffoldTreeView.BeforeDrop += OnBeforeScaffoldDrop;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            scaffold.Change += OnScaffoldChange;
            scaffold.FullReset += OnScaffoldReset;
            OnUndoRedoPerformed();
        }

        private void OnDisable()
        {
            scaffoldTreeView.BeforeDrop -= OnBeforeScaffoldDrop;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            scaffold.Change -= OnScaffoldChange;
            scaffold.FullReset -= OnScaffoldReset;
            SessionState.SetString(SessionStateKeyPrefix + scaffold.GetInstanceID(),
                JsonUtility.ToJson(scaffoldTreeView.state));
        }

        private void OnScaffoldReset()
        {
            OnUndoRedoPerformed();
            scaffoldTreeView.SetSelection(NoIDs);
        }

        private void OnScaffoldChange(IReadOnlyList<TemplScaffoldNode> nodes) =>
            CheckValidity();

        private void OnChangeNodeFields() => CheckValidity();

        private void CheckValidity() => isScaffoldValid = scaffold.Root.IsValid;

        private void DrawInputProperty()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(inputProperty);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();
            DrawLeftToolbar();
            GUILayout.FlexibleSpace();
            DrawRightToolbar();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawLeftToolbar()
        {
            GUI.enabled = RootHasChildren;
            DrawButton(ExpandCollapseButtonContent, MiniButtonStyle, _ => ToggleExpandCollapse(),
                nameof(ToggleExpandCollapse));
            GUI.enabled = IsRootSelected;
            DrawButton(EditButtonContent, MiniButtonStyle, _ => scaffoldTreeView.EditSelectedNode(),
                nameof(scaffoldTreeView.EditSelectedNode),
                KeyPress(KeyCode.F2) || KeyPress(KeyCode.Return));
            GUI.enabled = true;
        }

        private void DrawRightToolbar()
        {
            DrawButton(DirectoryButtonContent, LeftButtonStyle, AddScaffoldDirectoryNode,
                nameof(scaffold.AddScaffoldDirectoryNode));
            DrawButton(FileButtonContent, MidButtonStyle, AddScaffoldFileNode,
                nameof(scaffold.AddScaffoldFileNode));
            GUI.enabled = IsRootSelected;
            DrawButton(CloneButtonContent, MidButtonStyle, scaffold.CloneScaffoldNodes,
                nameof(scaffold.CloneScaffoldNodes),
                (Event.current.command || Event.current.control) && KeyPress(KeyCode.D));
            DrawButton(DeleteButtonContent, RightButtonStyle, scaffold.RemoveScaffoldNodes,
                nameof(scaffold.RemoveScaffoldNodes), KeyPress(KeyCode.Delete));
            GUI.enabled = true;
        }

        private void AddScaffoldFileNode(TemplScaffoldNode[] parents)
        {
            scaffold.AddScaffoldFileNode(parents);
            scaffoldTreeView.EditSelectedNode();
        }

        private void AddScaffoldDirectoryNode(TemplScaffoldNode[] parents)
        {
            scaffold.AddScaffoldDirectoryNode(parents);
            scaffoldTreeView.EditSelectedNode();
        }

        private void ToggleExpandCollapse()
        {
            if (!scaffoldTreeView.IsNodeExpanded(scaffold.Root))
            {
                scaffoldTreeView.ExpandAll();
            }
            else
            {
                scaffoldTreeView.CollapseAll();
            }
        }

        private void OnUndoRedoPerformed()
        {
            CheckValidity();
            scaffoldTreeView.Reload();
        }

        private void DrawButton(GUIContent content,
            GUIStyle style,
            Action<TemplScaffoldNode[]> action,
            string name,
            bool keyPress = false)
        {
            var maxWidth = GUILayout.MaxWidth(ToolbarButtonWidth);
            var minWidth = GUILayout.MinWidth(ToolbarButtonWidth);

            if (GUILayout.Button(content, style, maxWidth, minWidth) || keyPress)
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
            EditIcon = EditIcon
                ? EditIcon
                : EditorGUIUtility.FindTexture(EditIconName);
            ExpandCollapseIcon = ExpandCollapseIcon
                ? ExpandCollapseIcon
                : EditorGUIUtility.FindTexture(ExpandCollapseIconName);
        }

        private void CreateButtonContents()
        {
            DirectoryButtonContent ??= new GUIContent(DirectoryIcon, DirectoryTooltip);
            FileButtonContent ??= new GUIContent(FileIcon, FileTooltip);
            DeleteButtonContent ??= new GUIContent(DeleteIcon, DeleteTooltip);
            CloneButtonContent ??= new GUIContent(CloneIcon, CloneTooltip);
            EditButtonContent ??= new GUIContent(EditIcon, EditTooltip);
            ExpandCollapseButtonContent ??=
                new GUIContent(ExpandCollapseIcon, ExpandCollapseTooltip);
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
            var selectedNodes = scaffoldTreeView.GetNodeSelection();
            action(selectedNodes);
        }

        private void OnBeforeScaffoldDrop() =>
            Undo.RecordObject(scaffold, nameof(scaffold.MoveScaffoldNodes));
    }
}
