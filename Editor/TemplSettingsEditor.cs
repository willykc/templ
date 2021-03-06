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
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Willykc.Templ.Editor
{
    using static TemplEditorInitialization;
    using static TemplProcessor;

    [CustomEditor(typeof(TemplSettings))]
    internal sealed class TemplSettingsEditor : UnityEditor.Editor
    {
        internal const string MenuName = "Window/Templ/Settings";
        private const int Padding = 5;
        private const int MaxFilenameLength = 64;
        private const string Header = "Templ Entries";
        private const string ButtonText = "Force Render Templates";
        private static readonly string ErrorMessage = "Invalid entries detected. All fields must " +
            $"have values. {nameof(ScribanAsset)} or {nameof(TemplSettings)} can not be used as " +
            $"input. {Capitalize(nameof(TemplEntry.template))} must be valid. " +
            $"{Capitalize(nameof(TemplEntry.filename))} field must not contain invalid " +
            "characters and must be unique under the same " +
            $"{Capitalize(nameof(TemplEntry.directory))}. Templ will only render templates for " +
            "valid entries.";
        private static readonly Color InvalidColor = new Color(1, .3f, .3f, 1);
        private static readonly Color ValidColor = Color.white;
        private static readonly float Line = EditorGUIUtility.singleLineHeight;
        private static readonly float DoubleLine = EditorGUIUtility.singleLineHeight * 2;

        private ReorderableList list;
        private string[] fullPathDuplicates;
        private TemplSettings settings;
        private bool isValid;
        private Type[] entryTypes;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                OnChange();
            }
            GUI.enabled = settings.Entries.Count > 0;
            if (GUILayout.Button(ButtonText))
            {
                Core.RenderAllValidEntries();
            }
            GUI.enabled = true;
            if (!isValid)
            {
                EditorGUILayout.HelpBox(ErrorMessage, MessageType.Error);
            }
        }

        private void OnEnable()
        {
            entryTypes = TypeCache
                .Where(t => t.IsSubclassOf(typeof(TemplEntry)) && !t.IsAbstract)
                .ToArray();
            settings = serializedObject.targetObject as TemplSettings;
            list = new ReorderableList(serializedObject,
                serializedObject.FindProperty(nameof(TemplSettings.Entries).ToLower()),
                true, true, true, true);
            list.elementHeight = (DoubleLine * 2) + 2 + Padding;
            list.drawElementCallback += OnDrawElement;
            list.drawHeaderCallback += OnDrawHeader;
            list.onAddDropdownCallback += OnAddDropdown;
            Undo.undoRedoPerformed += OnChange;
            settings.OnReset += OnChange;
            OnChange();
        }

        private void OnDisable()
        {
            list.drawElementCallback -= OnDrawElement;
            list.drawHeaderCallback -= OnDrawHeader;
            list.onAddDropdownCallback -= OnAddDropdown;
            Undo.undoRedoPerformed -= OnChange;
            settings.OnReset -= OnChange;
        }

        private void OnChange()
        {
            CollectDuplicates();
            CheckValidity();
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            var entry = settings.Entries[index];
            DrawHeaderLine(rect);
            rect.y += 4;
            DrawFirstRow(rect, element, entry);
            DrawSecondRow(rect, element, entry);
            if (EditorGUI.EndChangeCheck())
            {
                Core.FlagChangedEntry(entry);
            }
        }

        private void DrawFirstRow(Rect rect, SerializedProperty element, TemplEntry entry)
        {
            DrawPropertyField(new Rect(
                rect.x,
                rect.y,
                (rect.width / 2) - Padding,
                Line),
                element.FindPropertyRelative(entry.InputFieldName),
                _ => entry.IsValidInput);
            DrawPropertyField(new Rect(
                rect.x + (rect.width / 2) + Padding,
                rect.y,
                (rect.width / 2) - Padding,
                Line),
                element.FindPropertyRelative(nameof(TemplEntry.directory)),
                p => NotNullReference(p));
        }

        private void DrawSecondRow(Rect rect, SerializedProperty element, TemplEntry entry)
        {
            DrawPropertyField(new Rect(
                rect.x,
                rect.y + 2 + DoubleLine,
                (rect.width / 2) - Padding,
                Line),
                element.FindPropertyRelative(nameof(TemplEntry.template)),
                p => NotNullReference(p) && IsValidTemplate(p));
            DrawPropertyField(new Rect(
                rect.x + (rect.width / 2) + Padding,
                rect.y + 2 + DoubleLine,
                (rect.width / 2) - Padding,
                Line),
                element.FindPropertyRelative(nameof(TemplEntry.filename)),
                p => ValidFilename(p, entry));
        }

        private void OnDrawHeader(Rect rect) => EditorGUI.LabelField(rect, Header);

        private void OnAddDropdown(Rect buttonRect, ReorderableList list)
        {
            var menu = new GenericMenu();
            foreach (var entryType in entryTypes)
            {
                menu.AddItem(new GUIContent(entryType.Name),
                false, OnAddElement,
                entryType);
            }
            menu.ShowAsContext();
        }

        private void OnAddElement(object target)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            element.managedReferenceValue = Activator.CreateInstance(target as Type);
            serializedObject.ApplyModifiedProperties();
            OnChange();
        }

        private void DrawPropertyField(
            Rect rect,
            SerializedProperty property,
            Func<SerializedProperty, bool> isValid)
        {
            var style = new GUIStyle()
            {
                normal = { textColor = isValid(property) ? ValidColor : InvalidColor }
            };
            EditorGUI.LabelField(
                new Rect(rect.x, rect.y, rect.width, rect.height),
                property.displayName, style);
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y + Line, rect.width, rect.height),
                property, GUIContent.none);
        }

        private void CollectDuplicates() =>
            fullPathDuplicates = settings.Entries
            .Select(e => e.fullPathCache = e.FullPath)
            .GroupBy(p => p)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        private void CheckValidity() =>
            isValid = settings.Entries.All(e => e.IsValid) && fullPathDuplicates.Length == 0;

        private bool ValidFilename(SerializedProperty property, TemplEntry entry)
        {
            property.stringValue = property.stringValue.Trim();
            property.stringValue = property.stringValue.Length > MaxFilenameLength
                ? property.stringValue.Substring(0, MaxFilenameLength)
                : property.stringValue;
            return !string.IsNullOrWhiteSpace(property.stringValue) &&
            property.stringValue.IndexOfAny(Path.GetInvalidFileNameChars()) == -1 &&
            (!fullPathDuplicates?.Contains(entry.fullPathCache) ?? true);
        }

        [MenuItem(MenuName, priority = 10)]
        private static void FindSettings()
        {
            var settings = TemplSettings.Instance
                ? TemplSettings.Instance
                : TemplSettings.CreateNewSettings();
            Selection.activeObject = settings;
        }

        private static void DrawHeaderLine(Rect rect) =>
            EditorGUI.LabelField(new Rect(
                rect.x,
                rect.y - 6,
                rect.width,
                rect.height), string.Empty, GUI.skin.horizontalSlider);

        private static bool NotNullReference(SerializedProperty property) =>
            property.objectReferenceValue;

        private static bool IsValidTemplate(SerializedProperty property) =>
            property.objectReferenceValue is ScribanAsset template && !template.HasErrors;

        private static string Capitalize(string input) =>
            char.ToUpper(input[0]) + input.Substring(1);
    }
}
