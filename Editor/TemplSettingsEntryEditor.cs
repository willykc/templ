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
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Willykc.Templ.Editor
{
    using Entry;
    using static TemplEditorInitialization;
    using static TemplEntryProcessor;

    internal partial class TemplSettingsEditor
    {
        private const int MaxFilenameLength = 64;
        private const int ValidInputFieldCount = 1;
        private const int HeaderLineOffset = 6;
        private const string ForceRenderButtonText = "Force Render Templates";
        private const string LiveTitle = "Live Entries";
        private static readonly string ErrorMessage = "Invalid entries detected. All fields must " +
            $"have values. {nameof(ScribanAsset)} or {nameof(TemplSettings)} can not be used as " +
            $"input. {nameof(TemplEntry.template).Capitalize()} must be valid. " +
            $"{nameof(TemplEntry.filename).Capitalize()} field must not contain invalid " +
            "characters and must be unique under the same " +
            $"{nameof(TemplEntry.directory).Capitalize()}. Templ will only render templates for " +
            "valid entries.";

        private ReorderableList entryList;
        private string[] fullPathDuplicates;
        private Type[] entryTypes;
        private SerializedProperty entriesProperty;
        private bool isValidEntries;

        private void OnEnableEntries()
        {
            entryTypes = TypeCache
                .Where(IsEntryType)
                .ToArray();
            entriesProperty =
                serializedObject.FindProperty(TemplSettings.NameOfEntries);
            entryList = new ReorderableList(serializedObject, entriesProperty,
                false, false, true, true)
            {
                elementHeight = (DoubleLine * Double) + Spacing + Padding
            };
            entryList.drawElementCallback += OnDrawEntryElement;
            entryList.onAddDropdownCallback += OnAddEntryDropdown;
            Undo.undoRedoPerformed += OnChangeEntries;
            settings.FullReset += OnChangeEntries;
            OnChangeEntries();
        }

        private void OnDisableEntries()
        {
            entryList.drawElementCallback -= OnDrawEntryElement;
            entryList.onAddDropdownCallback -= OnAddEntryDropdown;
            Undo.undoRedoPerformed -= OnChangeEntries;
            settings.FullReset -= OnChangeEntries;
        }

        private void OnChangeEntries()
        {
            CollectEntryDuplicates();
            CheckEntriesValidity();
        }

        private void DrawLiveTemplEntries()
        {
            if (!Foldout(entriesProperty, LiveTitle))
            {
                return;
            }

            EditorGUI.BeginChangeCheck();
            entryList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                OnChangeEntries();
            }

            GUI.enabled = settings.Entries.Count > 0;
            GUILayout.Space(Padding);

            if (GUILayout.Button(ForceRenderButtonText))
            {
                EntryCore.RenderAllValidEntries();
            }

            GUI.enabled = true;

            if (!isValidEntries)
            {
                EditorGUILayout.HelpBox(ErrorMessage, MessageType.Error);
            }
        }

        private void OnDrawEntryElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            var element = entryList.serializedProperty.GetArrayElementAtIndex(index);
            var entry = settings.Entries[index];
            DrawHeaderLine(rect);
            rect.y += 4;
            DrawFirstRow(rect, element, entry);
            DrawSecondRow(rect, element, entry);

            if (EditorGUI.EndChangeCheck())
            {
                EntryCore.FlagChangedEntry(entry);
            }
        }

        private void DrawFirstRow(Rect rect, SerializedProperty element, TemplEntry entry)
        {
            DrawPropertyField(new Rect(
                rect.x,
                rect.y,
                (rect.width * Half) - Padding,
                Line),
                element.FindPropertyRelative(entry.InputFieldName),
                _ => entry.IsValidInput);
            DrawPropertyField(new Rect(
                rect.x + (rect.width * Half) + Padding,
                rect.y,
                (rect.width * Half) - Padding,
                Line),
                element.FindPropertyRelative(nameof(TemplEntry.directory)),
                p => NotNullReference(p));
        }

        private void DrawSecondRow(Rect rect, SerializedProperty element, TemplEntry entry)
        {
            DrawPropertyField(new Rect(
                rect.x,
                rect.y + Spacing + DoubleLine,
                (rect.width * Half) - Padding,
                Line),
                element.FindPropertyRelative(nameof(TemplEntry.template)),
                p => NotNullReference(p) && IsValidTemplate(p));
            DrawPropertyField(new Rect(
                rect.x + (rect.width * Half) + Padding,
                rect.y + Spacing + DoubleLine,
                (rect.width * Half) - Padding,
                Line),
                element.FindPropertyRelative(nameof(TemplEntry.filename)),
                p => ValidFilename(p, entry));
        }

        private void OnAddEntryDropdown(Rect buttonRect, ReorderableList list)
        {
            var menu = new GenericMenu();

            foreach (var entryType in entryTypes)
            {
                menu.AddItem(new GUIContent(entryType.Name),
                false, OnAddEntryElement,
                entryType);
            }

            menu.ShowAsContext();
        }

        private void OnAddEntryElement(object target)
        {
            var index = entryList.serializedProperty.arraySize;
            entryList.serializedProperty.arraySize++;
            entryList.index = index;
            var element = entryList.serializedProperty.GetArrayElementAtIndex(index);
            element.managedReferenceValue = Activator.CreateInstance(target as Type);
            serializedObject.ApplyModifiedProperties();
            OnChangeEntries();
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

        private void CollectEntryDuplicates() =>
            fullPathDuplicates = settings.Entries
            .Select(e => e.fullPathCache = e.FullPath)
            .GroupBy(p => p)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        private void CheckEntriesValidity() =>
            isValidEntries = settings.Entries.All(e => e.IsValid) && fullPathDuplicates.Length == 0;

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

        private static bool IsEntryType(Type type) =>
            type.IsSubclassOf(typeof(TemplEntry)) && !type.IsAbstract &&
            type.IsDefined(typeof(TemplEntryInfoAttribute), false) &&
            type.GetFields().Count(IsValidInputField) == ValidInputFieldCount;

        private static bool IsValidInputField(FieldInfo field) =>
            field.IsDefined(typeof(TemplInputAttribute), false) &&
            field.FieldType.IsSubclassOf(typeof(UnityEngine.Object));

        private static void DrawHeaderLine(Rect rect) =>
            EditorGUI.LabelField(new Rect(
                rect.x,
                rect.y - HeaderLineOffset,
                rect.width,
                rect.height), string.Empty, GUI.skin.horizontalSlider);

        private static bool NotNullReference(SerializedProperty property) =>
            property.objectReferenceValue;

        private static bool IsValidTemplate(SerializedProperty property) =>
            property.objectReferenceValue is ScribanAsset template && !template.HasErrors;
    }
}
