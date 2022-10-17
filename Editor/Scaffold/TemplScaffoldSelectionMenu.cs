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
        private const string AssetsPath = "Assets/";
        private const string AssetExtension = ".asset";

        private static readonly Rect EmptyRect = new Rect();
        private static readonly Vector2 Size = new Vector2(1, 1);

        private GenericMenu menu;
        private TemplSettings settings;

        [MenuItem(MenuName, priority = 1)]
        private static void ShowScaffoldsMenu()
        {
            var window = CreateInstance<TemplScaffoldSelectionMenu>();
            window.ShowAsDropDown(EmptyRect, Size);
        }

        [MenuItem(MenuName, isValidateFunction: true)]
        private static bool ValidateShowScaffoldsMenu() =>
            TemplSettings.Instance && TemplSettings.Instance.ValidScaffolds.Count > 0;

        private void Awake() => settings = TemplSettings.Instance;

        private void OnGUI() => menu ??= ShowMenu();

        private GenericMenu ShowMenu()
        {
            var menu = new GenericMenu();

            foreach (var scaffold in settings.ValidScaffolds.Distinct())
            {
                var path = AssetDatabase.GetAssetPath(scaffold)
                    .Replace(AssetsPath, string.Empty)
                    .Replace(AssetExtension, string.Empty);
                menu.AddItem(new GUIContent(path), false, OnScaffoldSelected, scaffold);
            }

            menu.ShowAsContext();
            return menu;
        }

        private void OnScaffoldSelected(object selection)
        {
            var scaffold = selection as TemplScaffold;
            var selectedAsset = Selection.activeObject;
            var targetPath = selectedAsset.GetAssetDirectoryPath();
            TemplScaffoldGenerator.GenerateScaffold(
                scaffold,
                targetPath,
                selection: selectedAsset);
            Close();
        }
    }
}
