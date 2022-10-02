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
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Willykc.Templ.Editor.Scaffold;

namespace Willykc.Templ.Editor
{
    internal partial class TemplSettingsEditor
    {
        private const int MaxSize = 1000;
        private const int MinSize = 0;
        private const int IconSize = 15;
        private const int ScaffoldHorizontalPadding = 20;
        private const int ScaffoldVerticalPadding = 1;
        private const string ScaffoldsName = nameof(TemplSettings.Scaffolds);
        private const string ValidScaffoldIconName = "TestPassed";
        private const string InvalidScaffoldIconName = "TestFailed";
        private const string ValidScaffoldTooltip = "Valid Scaffold";
        private const string InvalidScaffoldTooltip = "Invalid Scaffold";
        private const string UniqueScaffoldMessage =
            "There can only be one instance of each scaffold in the list";

        private ReorderableList scaffoldList;
        private SerializedProperty scaffoldsProperty;
        private bool[] scaffoldsValidity;
        private bool showUniqueScaffoldMessage;

        private void OnEnableScaffolds()
        {
            var scaffoldsPropertyName = ScaffoldsName.ToLower();
            scaffoldsProperty = serializedObject.FindProperty(scaffoldsPropertyName);
            scaffoldList = new ReorderableList(serializedObject, scaffoldsProperty,
                false, false, true, true);
            scaffoldList.drawElementCallback += OnDrawScaffoldElement;
            scaffoldList.onAddCallback += OnAddScaffold;
            CheckScaffoldsValidity();
        }

        private void OnDisableScaffolds()
        {
            scaffoldList.drawElementCallback -= OnDrawScaffoldElement;
            scaffoldList.onAddCallback -= OnAddScaffold;
        }

        private void DrawTemplScaffolds()
        {
            if (!Foldout(scaffoldsProperty, ScaffoldsName))
            {
                return;
            }

            EditorGUI.BeginChangeCheck();
            scaffoldList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                CheckScaffoldsValidity();
            }

            if (showUniqueScaffoldMessage)
            {
                EditorGUILayout.HelpBox(UniqueScaffoldMessage, MessageType.Info);
            }
        }

        private void OnDrawScaffoldElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = scaffoldList.serializedProperty.GetArrayElementAtIndex(index);
            DrawValidityIcon(rect, index);
            DrawScaffoldField(rect, element);
        }

        private void OnAddScaffold(ReorderableList list)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            element.objectReferenceValue = null;
            serializedObject.ApplyModifiedProperties();
            CheckScaffoldsValidity();
        }

        private void DrawValidityIcon(Rect rect, int index)
        {
            var isValid = scaffoldsValidity[index];
            var iconRect = new Rect(rect.x, rect.y + (Spacing * Double), IconSize, IconSize);
            var iconName = isValid ? ValidScaffoldIconName : InvalidScaffoldIconName;
            var icon = EditorGUIUtility.IconContent(iconName);
            icon.tooltip = isValid ? ValidScaffoldTooltip : InvalidScaffoldTooltip;
            EditorGUI.LabelField(iconRect, icon);
        }

        private void DrawScaffoldField(Rect rect, SerializedProperty element)
        {
            EditorGUI.BeginChangeCheck();
            var fieldRect = new Rect(
                rect.x + ScaffoldHorizontalPadding,
                rect.y + ScaffoldVerticalPadding,
                rect.width - ScaffoldHorizontalPadding,
                rect.height - Spacing);
            EditorGUI.ObjectField(fieldRect, element, GUIContent.none);

            if (EditorGUI.EndChangeCheck())
            {
                SanitizeRepeatedScaffold(element);
            }
        }

        private void SanitizeRepeatedScaffold(SerializedProperty element)
        {
            if (element.objectReferenceValue is TemplScaffold scaffold && scaffold &&
                settings.Scaffolds.Contains(scaffold))
            {
                element.objectReferenceValue = null;
                showUniqueScaffoldMessage = true;
            }
        }

        private void CheckScaffoldsValidity() =>
            scaffoldsValidity = settings.Scaffolds.Select(s => s && s.IsValid).ToArray();
    }
}
