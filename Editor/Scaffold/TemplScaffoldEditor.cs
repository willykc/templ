/*
 * Copyright (c) 2024 Willy Alberto Kuster
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
    [CustomEditor(typeof(TemplScaffold))]
    internal sealed class TemplScaffoldEditor : UnityEditor.Editor
    {
        internal const string WarningMessage = "This scaffold is not currently referenced in " +
            "Templ settings.";

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
        private const string DeleteTooltip = "Delete Node(s) (Ctrl/Cmd + Delete)";
        private const string CloneTooltip = "Clone Node(s) (Ctrl/Cmd + D)";
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
        private const string ContextPrefix = "CONTEXT/";
        private const string CopyYamlMenuName = ContextPrefix + nameof(TemplScaffold) +
            "/Copy YAML Tree";
        private const string CopyYamlWithGuidsMenuName = ContextPrefix + nameof(TemplScaffold) +
            "/Copy YAML Tree with GUIDs";

        private static readonly string RemoveNodesActionName =
            nameof(TemplScaffold.RemoveScaffoldNodes).ToTitleCase();
        private static readonly string CloneNodesActionName =
            nameof(TemplScaffold.CloneScaffoldNodes).ToTitleCase();
        private static readonly string AddFileNodeActionName =
            nameof(TemplScaffold.AddScaffoldFileNodes).ToTitleCase();
        private static readonly string AddDirectoryActionName =
            nameof(TemplScaffold.AddScaffoldDirectoryNodes).ToTitleCase();
        private static readonly string MoveNodesActionName =
            nameof(TemplScaffold.MoveScaffoldNodes).ToTitleCase();
        private static readonly int[] NoIDs = new int[] { };
        private static readonly string ErrorMessage = "Invalid nodes detected. All node fields " +
            $"must have values. {nameof(TemplScaffoldFile.Template)}s must be " +
            "valid. File and Directory names must not contain invalid characters and must be " +
            "unique under the same parent node. Directories must not be empty. " +
            "Templ will only generate valid scaffolds.";

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

        private bool RootHasChildren => scaffold.Root.Children.Count > 0;

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
            inputProperty = serializedObject.FindProperty(TemplScaffold.NameOfDefaultInput);
            LoadIcons();
            CreateButtonContents();
            var treeViewState = new TreeViewState();
            var jsonState = SessionState
                .GetString(SessionStateKeyPrefix + scaffold.GetInstanceID(), string.Empty);

            if (!string.IsNullOrEmpty(jsonState))
            {
                JsonUtility.FromJsonOverwrite(jsonState, treeViewState);
            }

            scaffoldTreeView = new TemplScaffoldTreeView(
                treeViewState,
                scaffold, ScaffoldIcon, DirectoryIcon, FileIcon);
            scaffoldTreeView.BeforeDropped += OnBeforeScaffoldDrop;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            scaffold.Changed += OnScaffoldChange;
            scaffold.AfterReset += OnScaffoldReset;
            ReloadTreeView();
        }

        private void OnDisable()
        {
            scaffoldTreeView.BeforeDropped -= OnBeforeScaffoldDrop;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            scaffold.Changed -= OnScaffoldChange;
            scaffold.AfterReset -= OnScaffoldReset;
            SessionState.SetString(SessionStateKeyPrefix + scaffold.GetInstanceID(),
                JsonUtility.ToJson(scaffoldTreeView.state));
        }

        private void OnScaffoldReset()
        {
            ReloadTreeView();
            scaffoldTreeView.SetSelection(NoIDs);
        }

        private void OnScaffoldChange(IReadOnlyList<TemplScaffoldNode> nodes)
        {
            CheckValidity();
            EditorUtility.SetDirty(scaffold);
        }

        private void OnChangeNodeFields() => CheckValidity();

        private void CheckValidity() => isScaffoldValid = scaffold.IsValid;

        private void DrawInputProperty()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(inputProperty);
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Separator();
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
            DrawButton(ExpandCollapseButtonContent, MiniButtonStyle, _ => ToggleExpandCollapse());
            GUI.enabled = IsRootSelected;
            DrawButton(EditButtonContent, MiniButtonStyle, _ => scaffoldTreeView.EditSelectedNode(),
                keyPress: KeyPress(KeyCode.F2) || KeyPress(KeyCode.Return));
            GUI.enabled = true;
        }

        private void DrawRightToolbar()
        {
            DrawButton(DirectoryButtonContent, LeftButtonStyle, AddScaffoldDirectoryNode,
                AddDirectoryActionName);
            DrawButton(FileButtonContent, MidButtonStyle, AddScaffoldFileNode,
                AddFileNodeActionName);
            GUI.enabled = IsRootSelected;
            DrawButton(CloneButtonContent, MidButtonStyle, scaffold.CloneScaffoldNodes,
                CloneNodesActionName,
                (Event.current.command || Event.current.control) && KeyPress(KeyCode.D));
            DrawButton(DeleteButtonContent, RightButtonStyle, scaffold.RemoveScaffoldNodes,
                RemoveNodesActionName,
                (Event.current.command || Event.current.control) && KeyPress(KeyCode.Delete));
            GUI.enabled = true;
        }

        private void AddScaffoldFileNode(TemplScaffoldNode[] parents)
        {
            scaffold.AddScaffoldFileNodes(parents);
            scaffoldTreeView.EditSelectedNode();
        }

        private void AddScaffoldDirectoryNode(TemplScaffoldNode[] parents)
        {
            scaffold.AddScaffoldDirectoryNodes(parents);
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
            scaffoldTreeView.SetSelection(NoIDs);
        }

        private void OnUndoRedoPerformed()
        {
            OnDisable();
            OnEnable();
        }

        private void ReloadTreeView()
        {
            CheckValidity();
            scaffoldTreeView.Reload();
        }

        private void DrawButton(GUIContent content,
            GUIStyle style,
            Action<TemplScaffoldNode[]> action,
            string name = null,
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
            if (name != null)
            {
                Undo.RegisterCompleteObjectUndo(scaffold, name);
            }
            var selectedNodes = scaffoldTreeView.GetNodeSelection();
            action(selectedNodes);
        }

        private void OnBeforeScaffoldDrop() =>
            Undo.RegisterCompleteObjectUndo(scaffold, MoveNodesActionName);

        [MenuItem(CopyYamlMenuName)]
        private static void YamlToClipboard(MenuCommand menuCommand)
        {
            var scaffold = menuCommand.context as TemplScaffold;
            EditorGUIUtility.systemCopyBuffer =
                TemplScaffoldYamlSerializer.SerializeTree(scaffold.Root);
        }

        [MenuItem(CopyYamlWithGuidsMenuName)]
        private static void YamlWithGuidsToClipboard(MenuCommand menuCommand)
        {
            var scaffold = menuCommand.context as TemplScaffold;
            EditorGUIUtility.systemCopyBuffer =
                TemplScaffoldYamlSerializer.SerializeTree(scaffold.Root, useGuids: true);
        }

        [MenuItem(CopyYamlMenuName, isValidateFunction: true)]
        [MenuItem(CopyYamlWithGuidsMenuName, isValidateFunction: true)]
        private static bool ValidateYamlToClipboardMenu(MenuCommand menuCommand) =>
            menuCommand.context.GetType() == typeof(TemplScaffold);
    }
}
