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
using UnityEditor;
using UnityEngine;

namespace Willykc.Templ.Editor
{
    [CustomEditor(typeof(TemplSettings))]
    internal sealed partial class TemplSettingsEditor : UnityEditor.Editor
    {
        internal const string MenuName = "Window/Templ/Settings";

        private const int Padding = 5;
        private const int Spacing = 2;
        private const int Double = 2;
        private const float Half = .5f;

        internal static readonly Color InvalidColor = new Color(1, .3f, .3f, 1);
        internal static readonly Color ValidColor = Color.white;

        private static readonly float Line = EditorGUIUtility.singleLineHeight;
        private static readonly float DoubleLine = EditorGUIUtility.singleLineHeight * Double;

        private TemplSettings settings;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawLiveTemplEntries();
            GUILayout.Space(Padding);
            DrawTemplScaffolds();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            settings = serializedObject.targetObject as TemplSettings;
            OnEnableEntries();
            OnEnableScaffolds();
        }

        private void OnDisable()
        {
            OnDisableEntries();
            OnDisableScaffolds();
        }

        [MenuItem(MenuName, priority = 10)]
        private static void FindSettings()
        {
            Selection.activeObject = TemplSettings.Instance
                ? TemplSettings.Instance
                : TemplSettings.CreateNewSettings();
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        private static bool Foldout(SerializedProperty property, string name) =>
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, name);
    }
}
