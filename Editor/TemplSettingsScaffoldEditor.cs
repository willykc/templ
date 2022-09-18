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
    internal partial class TemplSettingsEditor
    {
        private const int MaxSize = 1000;
        private const int MinSize = 0;
        private const string ScaffoldsName = nameof(TemplSettings.Scaffolds);

        private SerializedProperty scaffoldsProperty;

        private void OnEnableScaffolds()
        {
            var scaffoldsPropertyName = ScaffoldsName.ToLower();
            scaffoldsProperty = serializedObject.FindProperty(scaffoldsPropertyName);
        }

        private void OnDisableScaffolds()
        {
        }

        private void DrawTemplScaffolds()
        {
            var rect = GUILayoutUtility.GetRect(MinSize, MaxSize, MinSize, MaxSize);
            EditorGUI.PropertyField(rect, scaffoldsProperty);
        }
    }
}
