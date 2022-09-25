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
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace Willykc.Templ.Editor.Scaffold
{
    using DrawAction = Action<TemplScaffoldTreeViewItem, Rect>;
    using static TemplSettingsEditor;

    internal sealed class TemplScaffoldRowView
    {
        private const string BlankName = "blank";
        private const string NamePropertyName = nameof(TemplScaffoldNode.name);
        private const string TemplatePropertyName = nameof(TemplScaffoldFile.template);
        private const int IconWidth = 16;
        private const int Space = 2;
        private const int TextFocusMaxFrames = 10;
        private const int EditPropertyHeight = 19;
        private const int EditPropertyMinWidth = 80;
        private const int EditModeRowHeight = 22;
        private const int DefaultRowHeight = 16;
        private const float Half = .5f;

        private readonly IReadOnlyDictionary<Type, DrawAction> drawActions;

        private int textFocusFrameCounter;
        private int editNodeID;

        internal TemplScaffoldRowView()
        {
            drawActions = new Dictionary<Type, DrawAction>()
            {
                { typeof(TemplScaffoldFile), DrawEditModeFile },
                { typeof(TemplScaffoldDirectory), DrawEditModeDirectory }
            };
        }

        internal void DrawRow(TemplScaffoldTreeViewItem item, Rect rowRect)
        {
            if (item.id == editNodeID)
            {
                DrawEditModeRow(item, rowRect);
            }
            else
            {
                DrawReadOnlyModeRow(item, rowRect);
            }
        }

        internal void ToggleEditMode(int nodeID)
        {
            editNodeID = nodeID != editNodeID ? nodeID : 0;
            textFocusFrameCounter = 0;
        }

        internal void ResetEditMode()
        {
            editNodeID = 0;
            textFocusFrameCounter = 0;
        }

        internal float GetRowHeight(TreeViewItem item) => item.id == editNodeID
            ? EditModeRowHeight
            : DefaultRowHeight;

        private void DrawEditModeRow(TemplScaffoldTreeViewItem item, Rect rowRect)
        {
            if (drawActions.TryGetValue(item.Node.GetType(), out var drawRow))
            {
                drawRow(item, rowRect);
            }
        }

        private void DrawReadOnlyModeRow(TemplScaffoldTreeViewItem item, Rect rowRect)
        {
            var rect = DrawNodeIcon(item, rowRect);
            rect.x += rect.width;
            rect.y--;
            rect.width = rowRect.width;
            var style = new GUIStyle()
            {
                normal = { textColor = item.IsValid ? ValidColor : InvalidColor }
            };

            var displayName = !string.IsNullOrEmpty(item.displayName)
                ? item.displayName
                : BlankName;
            GUI.Label(rect, displayName, style);
        }

        private void DrawEditModeFile(TemplScaffoldTreeViewItem item, Rect rowRect)
        {
            var rect = DrawNodeIcon(item, rowRect);
            rect = DrawNamePropertyField(item, rect, rowRect, Half);
            DrawTemplatePropertyField(item, rect, rowRect);
        }

        private void DrawEditModeDirectory(
            TemplScaffoldTreeViewItem item,
            Rect rowRect)
        {
            var rect = DrawNodeIcon(item, rowRect);
            DrawNamePropertyField(item, rect, rowRect, 1);
        }

        private Rect DrawNodeIcon(TemplScaffoldTreeViewItem item, Rect rowRect)
        {
            var rect = rowRect;
            rect.x += item.Indent;
            rect.width = IconWidth;
            GUI.DrawTexture(rect, item.icon, ScaleMode.ScaleToFit);
            return rect;
        }

        private Rect DrawNamePropertyField(
            TemplScaffoldTreeViewItem item,
            Rect rect,
            Rect rowRect,
            float widthFactor)
        {
            var nameProperty = item.Property.FindPropertyRelative(NamePropertyName);
            rect.x += rect.width + Space;
            var width = (rowRect.width - rect.x) * widthFactor;
            rect.y++;
            rect.width = Mathf.Max(width, EditPropertyMinWidth);
            rect.height = EditPropertyHeight;
            GUI.SetNextControlName(NamePropertyName);
            EditorGUI.PropertyField(rect, nameProperty, GUIContent.none);
            HandleFocusOnText();
            return rect;
        }

        private void DrawTemplatePropertyField(
            TemplScaffoldTreeViewItem item,
            Rect rect,
            Rect rowRect)
        {
            var templateProperty = item.Property.FindPropertyRelative(TemplatePropertyName);
            rect.x += rect.width + Space;
            var width = rowRect.width - rect.x;
            rect.width = Mathf.Max(width, EditPropertyMinWidth) - Space;
            rect.height = EditPropertyHeight;
            GUI.SetNextControlName(TemplatePropertyName);
            EditorGUI.PropertyField(rect, templateProperty, GUIContent.none);
        }

        private void HandleFocusOnText()
        {
            if (textFocusFrameCounter < TextFocusMaxFrames)
            {
                EditorGUI.FocusTextInControl(NamePropertyName);
                textFocusFrameCounter++;
            }
        }
    }
}
