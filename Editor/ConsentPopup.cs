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
    internal sealed class ConsentPopup : EditorWindow
    {
        private const int Width = 500;
        private const int Height = 200;
        private const string ProceedButtonName = "Proceed";
        private const string QuitButtonName = "X";

        private readonly static string Message = "Thanks for installing Templ. " +
            $"By clicking {ProceedButtonName}, you agree to create the " +
            $"{nameof(TemplSettings)} file at {TemplSettings.DefaultConfigFolder}.";
        private readonly static string Note = $"NOTE: You can also create it " +
            $"later by navigating to the {TemplSettingsEditor.MenuName} menu.";

        internal static void ShowPopupCentered()
        {
            ConsentPopup window = CreateInstance<ConsentPopup>();
            Rect main = EditorGUIUtility.GetMainWindowPosition();
            Rect pos = new Rect(0, 0, Width, Height);
            float centerWidth = (main.width - pos.width) * 0.5f;
            float centerHeight = (main.height - pos.height) * 0.5f;
            pos.x = main.x + centerWidth;
            pos.y = main.y + centerHeight;
            window.position = pos;
            window.ShowPopup();
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(position.width - 40, 10, 30, 25), QuitButtonName))
            {
                this.Close();
            }
            GUILayout.Space(60);
            var style = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                padding = new RectOffset(20, 20, 2, 2)
            };
            EditorGUILayout.LabelField(Message, style);
            GUILayout.Space(10);
            EditorGUILayout.LabelField(Note, style);
            GUILayout.Space(35);
            if (GUILayout.Button(ProceedButtonName))
            {
                Selection.activeObject = TemplSettings.CreateNewSettings();
                this.Close();
            }
        }
    }
}
