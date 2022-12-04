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
        private const int VerticalTotalPadding = 2;
        private const int BorderLineHeight = 1;

        private static readonly Rect EmptyRect = new Rect();
        private static readonly ITemplScaffoldFacade ScaffoldManager = Templ.ScaffoldManager;
        private static readonly Color BorderColor = new Color(0.15f, 0.15f, 0.15f, 1);

        private bool initialized;
        private GUIStyle menuEntryStyle;
        private GUIStyle borderStyle;
        private TemplScaffold[] selectableScaffolds;

        [MenuItem(MenuName, priority = 1)]
        private static void ShowScaffoldsMenu()
        {
            var window = CreateInstance<TemplScaffoldSelectionMenu>();

            var settings = TemplSettings.Instance;
            window.selectableScaffolds = settings
                ? settings.ValidScaffolds.Distinct().ToArray()
                : TemplSettings.EmptyScaffoldArray;

            var height = (MenuEntryHeight * window.selectableScaffolds.Length) +
                VerticalTotalPadding;
            var size = new Vector2(MenuWidth, height);
            window.ShowAsDropDown(EmptyRect, size);
            window.maxSize = size;
            window.minSize = size;
        }

        [MenuItem(MenuName, isValidateFunction: true)]
        private static bool ValidateShowScaffoldsMenu() =>
            !EditorApplication.isCompiling &&
            TemplSettings.Instance &&
            TemplSettings.Instance.ValidScaffolds.Count > 0 &&
            !Templ.ScaffoldManager.IsGenerating &&
            !Selection.activeObject.IsReadOnly();

        private void OnGUI()
        {
            Initialize();

            DrawBorderLine();

            foreach (var scaffold in selectableScaffolds)
            {
                DrawMenuEntry(scaffold);
            }

            DrawBorderLine();
        }

        private void DrawBorderLine()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, BorderLineHeight, style: borderStyle);
            rect.height = BorderLineHeight;
            EditorGUI.DrawRect(rect, BorderColor);
        }

        private void Initialize()
        {
            if (initialized || selectableScaffolds.Length == 0)
            {
                return;
            }

            menuEntryStyle = GetMenuEntryStyle();
            borderStyle = GetBorderStyle();
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

        private async void OnScaffoldSelected(TemplScaffold scaffold)
        {
            var selectedAsset = Selection.activeObject;
            var targetPath = selectedAsset.GetAssetDirectoryPath();
            Close();
            var generatedPaths = await ScaffoldManager.GenerateScaffoldAsync(
                scaffold,
                targetPath,
                selection: selectedAsset);
            AssetDatabase.Refresh();
            SelectFirstGeneratedAsset(generatedPaths);
        }

        private static GUIStyle GetMenuEntryStyle() => new GUIStyle(ToolbarButtonStyleName)
        {
            alignment = TextAnchor.MiddleLeft,
            fixedHeight = MenuEntryHeight,
            padding = new RectOffset(
                HorizontalMenuEntryPadding,
                HorizontalMenuEntryPadding,
                VerticalMenuEntryPadding,
                VerticalMenuEntryPadding),
            margin = new RectOffset(BorderLineHeight, 0, 0, 0)
        };

        private static GUIStyle GetBorderStyle() => new GUIStyle()
        {
            margin = new RectOffset(0, 0, 0, 0)
        };

        private static void SelectFirstGeneratedAsset(string[] generatedPaths)
        {
            if (generatedPaths == null || generatedPaths.Length == 0)
            {
                return;
            }

            var first = generatedPaths[0];
            Selection.activeObject = AssetDatabase.LoadAssetAtPath(first, typeof(Object));
        }
    }
}
