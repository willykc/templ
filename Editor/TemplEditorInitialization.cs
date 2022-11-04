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

namespace Willykc.Templ.Editor
{
    [InitializeOnLoad]
    internal static class TemplEditorInitialization
    {
        private const string UnityName = "Unity";
        private const string UniTaskName = "UniTask";
        private const string NewtonsoftName = "Newtonsoft";
        private const string PsdPluginName = "PsdPlugin";
        private const string Log4netName = "log4net";
        private const string NUnitName = "nunit";
        private const string UnityplasticName = "unityplastic";
        private const string TemplTestsName = "Willykc.Templ.Editor.Tests";
        private const string TemplInitKey = "templ.init";

        private static string currentDir;
        private static Type[] typeCache;

        internal static Type[] TypeCache => typeCache ??= CollectTypes();

        static TemplEditorInitialization()
        {
            EditorApplication.delayCall += OnEditorLoaded;
        }

        private static void OnEditorLoaded()
        {
            if (!TemplSettings.Instance && !SessionState.GetBool(TemplInitKey, false))
            {
                ConsentPopup.ShowPopupCentered();
                SessionState.SetBool(TemplInitKey, true);
            }
        }

        private static Type[] CollectTypes() =>
            AppDomain.CurrentDomain.GetAssemblies()
            .Where(IsCandidate)
            .SelectMany(a => a.GetTypes())
            .ToArray();

        private static bool IsCandidate(Assembly assembly) =>
            !assembly.IsDynamic &&
            assembly.Location.StartsWith(currentDir ??= Directory.GetCurrentDirectory()) &&
            !assembly.FullName.StartsWith(UnityName) &&
            !assembly.FullName.StartsWith(UniTaskName) &&
            !assembly.FullName.StartsWith(NewtonsoftName) &&
            !assembly.FullName.StartsWith(PsdPluginName) &&
            !assembly.FullName.StartsWith(Log4netName) &&
            !assembly.FullName.StartsWith(NUnitName) &&
            !assembly.FullName.StartsWith(UnityplasticName) &&
            !assembly.FullName.StartsWith(TemplTestsName);
    }
}
