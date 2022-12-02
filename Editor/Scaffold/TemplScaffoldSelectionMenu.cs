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
using UnityEngine;

namespace Willykc.Templ.Editor.Scaffold
{
    internal sealed class TemplScaffoldSelectionMenu : EditorWindow
    {
        private const string MenuName = "Assets/Generate Templ Scaffold";
        private const string ToolbarButtonStyleName = "toolbarButton";
        private const int MenuWidth = 250;
        private const int MenuEntryHeight = 25;
        private const int HorizontalMenuEntryPadding = 10;
        private const int VerticalMenuEntryPadding = 0;

        private static readonly Rect EmptyRect = new Rect();

        private bool initialized;
        private GUIStyle menuEntryStyle;
        private TemplScaffold[] selectableScaffolds;

        [MenuItem(MenuName, priority = 1)]
        private static void ShowScaffoldsMenu()
        {
            var window = CreateInstance<TemplScaffoldSelectionMenu>();

            var settings = TemplSettings.Instance;
            window.selectableScaffolds = settings
                ? settings.ValidScaffolds.Distinct().ToArray()
                : TemplSettings.EmptyScaffoldArray;

            var height = MenuEntryHeight * window.selectableScaffolds.Length;
            var size = new Vector2(MenuWidth, height);
            window.ShowAsDropDown(EmptyRect, size);
        }

        [MenuItem(MenuName, isValidateFunction: true)]
        private static bool ValidateShowScaffoldsMenu() =>
            TemplSettings.Instance &&
            TemplSettings.Instance.ValidScaffolds.Count > 0 &&
            !TemplScaffoldGenerator.IsGenerating &&
            !Selection.activeObject.IsReadOnly();

        private void OnGUI()
        {
            Initialize();

            foreach (var scaffold in selectableScaffolds)
            {
                DrawMenuEntry(scaffold);
            }
        }

        private void Initialize()
        {
            if (initialized || selectableScaffolds.Length == 0)
            {
                return;
            }

            menuEntryStyle = GetMenuEntryStyle();
            var mousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            position = new Rect(mousePosition, position.size);
            initialized = true;
        }

        private void DrawMenuEntry(TemplScaffold scaffold)
        {
            if (GUILayout.Button(scaffold.name, menuEntryStyle))
            {
                OnScaffoldSelected(scaffold);
            }
        }

        private void OnScaffoldSelected(TemplScaffold scaffold)
        {
            var selectedAsset = Selection.activeObject;
            var targetPath = selectedAsset.GetAssetDirectoryPath();
            TemplScaffoldGenerator.GenerateScaffold(
                scaffold,
                targetPath,
                selection: selectedAsset);
            Close();
        }

        private static GUIStyle GetMenuEntryStyle() => new GUIStyle(ToolbarButtonStyleName)
        {
            alignment = TextAnchor.MiddleLeft,
            fixedHeight = MenuEntryHeight,
            padding = new RectOffset(
                HorizontalMenuEntryPadding,
                HorizontalMenuEntryPadding,
                VerticalMenuEntryPadding,
                VerticalMenuEntryPadding)
        };
    }
}
