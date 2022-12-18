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
namespace Willykc.Templ.Editor
{
    using Entry;
    using Scaffold;
    using Abstraction;

    public static class TemplManagers
    {
        internal static ITemplEntryCore EntryCore { get; }

        static TemplManagers()
        {
            var scaffoldCore = new TemplScaffoldCore(
                FileSystem.Instance,
                Logger.Instance,
                EditorUtility.Instance,
                TemplateFunctionProvider.Instance);

            EntryCore = new TemplEntryCore(
                AssetDatabase.Instance,
                FileSystem.Instance,
                SessionState.Instance,
                Logger.Instance,
                SettingsProvider.Instance,
                TemplateFunctionProvider.Instance,
                EditorUtility.Instance);

            EntryManager = new TemplEntryFacade(
                SettingsProvider.Instance,
                AssetDatabase.Instance,
                EditorUtility.Instance,
                EntryCore);

            ScaffoldManager = new TemplScaffoldFacade(
                Logger.Instance,
                SettingsProvider.Instance,
                AssetDatabase.Instance,
                EditorUtility.Instance,
                TemplScaffoldWindowManager.Instance,
                scaffoldCore);
        }

        public static ITemplEntryFacade EntryManager { get; }
        public static ITemplScaffoldFacade ScaffoldManager { get; }
    }
}
