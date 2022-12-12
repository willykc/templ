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
using Object = UnityEngine.Object;

namespace Willykc.Templ.Editor.Entry
{
    using Abstraction;

    internal sealed class TemplEntryFacade : ITemplEntryFacade
    {
        private const int ValidInputFieldCount = 1;

        private readonly object lockHandle = new object();
        private readonly ISettingsProvider settingsProvider;
        private readonly IAssetDatabase assetDatabase;
        private readonly IEditorUtility editorUtility;
        private readonly ITemplEntryCore entryCore;

        public TemplEntryFacade(
            ISettingsProvider settingsProvider,
            IAssetDatabase assetDatabase,
            IEditorUtility editorUtility,
            ITemplEntryCore entryCore)
        {
            this.settingsProvider = settingsProvider;
            this.assetDatabase = assetDatabase;
            this.editorUtility = editorUtility;
            this.entryCore = entryCore;
        }

        TemplEntry[] ITemplEntryFacade.GetEntries()
        {
            if (!settingsProvider.SettingsExist())
            {
                throw new InvalidOperationException($"{nameof(TemplSettings)} not found");
            }

            lock (lockHandle)
            {
                return settingsProvider.GetSettings().Entries.ToArray();
            }
        }

        string ITemplEntryFacade.AddEntry<T>(
            Object inputAsset,
            ScribanAsset template,
            string outputAssetPath)
        {
            if (!settingsProvider.SettingsExist())
            {
                throw new InvalidOperationException($"{nameof(TemplSettings)} not found");
            }

            inputAsset = inputAsset
                ? inputAsset
                : throw new ArgumentNullException(nameof(inputAsset));
            template = template
                ? template
                : throw new ArgumentNullException(nameof(template));
            outputAssetPath = outputAssetPath
                ?? throw new ArgumentNullException(nameof(outputAssetPath));

            var settings = settingsProvider.GetSettings();

            if (inputAsset == settings)
            {
                throw new ArgumentException(
                    $"{nameof(inputAsset)} can not be {nameof(TemplSettings)}", nameof(inputAsset));
            }

            if (inputAsset.GetType() == typeof(ScribanAsset))
            {
                throw new ArgumentException(
                    $"{nameof(inputAsset)} can not be a {nameof(ScribanAsset)}",
                    nameof(inputAsset));
            }

            if (template.HasErrors)
            {
                throw new ArgumentException($"{nameof(template)} has syntax errors",
                    nameof(template));
            }

            string filename = null;

            try
            {
                filename = Path.GetFileName(outputAssetPath);
            }
            catch (Exception exception)
            {
                throw new ArgumentException($"{outputAssetPath} is not a valid path",
                    nameof(outputAssetPath), exception);
            }

            if(filename.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new ArgumentException($"{filename} is not a valid file name",
                    nameof(outputAssetPath));
            }

            var directoryPath = Path.GetDirectoryName(outputAssetPath);
            var directory = assetDatabase.LoadAssetAtPath<DefaultAsset>(directoryPath);

            if (!directory || !assetDatabase.IsValidFolder(directoryPath))
            {
                throw new DirectoryNotFoundException(
                    $"{nameof(outputAssetPath)}'s directory does not exist");
            }

            var entryType = typeof(T);

            if (!IsValidEntryType(entryType))
            {
                throw new InvalidOperationException($"{entryType.Name} is not a valid entry type");
            }

            if (settings.Entries.Select(e => e.OutputAssetPath).Contains(outputAssetPath))
            {
                throw new InvalidOperationException("Existing entry already uses " +
                    $"'{outputAssetPath}' as output asset path");
            }

            var newEntry = Activator.CreateInstance(entryType) as TemplEntry;

            try
            {
                newEntry.InputAsset = inputAsset;
            }
            catch (Exception exception)
            {
                throw new ArgumentException(
                    $"Could not assign {nameof(inputAsset)} to entry. " +
                    "Type must match entry input field",
                    nameof(inputAsset), exception);
            }

            newEntry.Template = template;
            newEntry.Directory = directory;
            newEntry.Filename = filename;

            lock (lockHandle)
            {
                settingsProvider.GetSettings().Entries.Add(newEntry);

                editorUtility.SetDirty(settings);
                assetDatabase.SaveAssets();
            }

            return newEntry.Id;
        }

        void ITemplEntryFacade.UpdateEntry(
            string id,
            Object inputAsset,
            ScribanAsset template,
            string outputAssetPath)
        {
            if (!settingsProvider.SettingsExist())
            {
                throw new InvalidOperationException($"{nameof(TemplSettings)} not found");
            }

            id = id ?? throw new ArgumentNullException(nameof(id));

            var settings = settingsProvider.GetSettings();

            if (inputAsset && inputAsset == settings)
            {
                throw new ArgumentException(
                    $"{nameof(inputAsset)} can not be {nameof(TemplSettings)}", nameof(inputAsset));
            }

            if (inputAsset && inputAsset.GetType() == typeof(ScribanAsset))
            {
                throw new ArgumentException(
                    $"{nameof(inputAsset)} can not be a {nameof(ScribanAsset)}",
                    nameof(inputAsset));
            }

            if (template && template.HasErrors)
            {
                throw new ArgumentException($"{nameof(template)} has syntax errors",
                    nameof(template));
            }

            string filename = null;

            try
            {
                filename = outputAssetPath != null
                    ? Path.GetFileName(outputAssetPath)
                    : filename;
            }
            catch (Exception exception)
            {
                throw new ArgumentException($"{outputAssetPath} is not a valid path",
                    nameof(outputAssetPath), exception);
            }

            if (outputAssetPath != null &&
                filename.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new ArgumentException($"{filename} is not a valid file name",
                    nameof(outputAssetPath));
            }

            var directoryPath = outputAssetPath != null
                ? Path.GetDirectoryName(outputAssetPath)
                : string.Empty;
            var directory = assetDatabase.LoadAssetAtPath<DefaultAsset>(directoryPath);

            if (outputAssetPath != null &&
                (!directory || !assetDatabase.IsValidFolder(directoryPath)))
            {
                throw new DirectoryNotFoundException(
                    $"{nameof(outputAssetPath)}'s directory does not exist");
            }

            var entry = settings.Entries.FirstOrDefault(e => e.Id == id);

            if (entry == null)
            {
                throw new InvalidOperationException($"No entry could be found with id '{id}'");
            }

            var otherEntriesPaths = settings.Entries
                .Where(e => e != entry)
                .Select(e => e.OutputAssetPath);

            if (otherEntriesPaths.Contains(outputAssetPath))
            {
                throw new InvalidOperationException("Existing entry already uses " +
                    $"'{outputAssetPath}' as output asset path");
            }

            lock (lockHandle)
            {
                try
                {
                    entry.InputAsset = inputAsset ? inputAsset : entry.InputAsset;
                }
                catch (Exception exception)
                {
                    throw new ArgumentException(
                        $"Could not assign {nameof(inputAsset)} to entry. " +
                        "Type must match entry input field",
                        nameof(inputAsset), exception);
                }

                entry.Template = template ? template : entry.Template;
                entry.Directory = outputAssetPath != null ? directory : entry.Directory;
                entry.Filename = outputAssetPath != null ? filename : entry.Filename;

                editorUtility.SetDirty(settings);
                assetDatabase.SaveAssets();
            }
        }

        void ITemplEntryFacade.RemoveEntry(string id)
        {
            if (!settingsProvider.SettingsExist())
            {
                throw new InvalidOperationException($"{nameof(TemplSettings)} not found");
            }

            id = id ?? throw new ArgumentNullException(nameof(id));

            var settings = settingsProvider.GetSettings();

            var entry = settings.Entries.FirstOrDefault(e => e.Id == id);

            if (entry == null)
            {
                throw new InvalidOperationException($"No entry could be found with id '{id}'");
            }

            lock (lockHandle)
            {
                settings.Entries.Remove(entry);

                editorUtility.SetDirty(settings);
                assetDatabase.SaveAssets();
            }
        }

        void ITemplEntryFacade.ForceRenderEntry(string id)
        {
            if (!settingsProvider.SettingsExist())
            {
                throw new InvalidOperationException($"{nameof(TemplSettings)} not found");
            }

            id = id ?? throw new ArgumentNullException(nameof(id));

            var settings = settingsProvider.GetSettings();

            var entry = settings.Entries.FirstOrDefault(e => e.Id == id);

            if (entry == null)
            {
                throw new InvalidOperationException($"No entry could be found with id '{id}'");
            }

            if (!entry.IsValid)
            {
                throw new InvalidOperationException($"Can not render invalid entry with id '{id}'");
            }

            lock (lockHandle)
            {
                entryCore.RenderEntry(id);
            }
        }

        void ITemplEntryFacade.ForceRenderAllValidEntries()
        {
            if (!settingsProvider.SettingsExist())
            {
                throw new InvalidOperationException($"{nameof(TemplSettings)} not found");
            }

            lock (lockHandle)
            {
                entryCore.RenderAllValidEntries();
            }
        }

        internal static bool IsValidEntryType(Type type) =>
            type.IsSubclassOf(typeof(TemplEntry)) && !type.IsAbstract &&
            type.IsDefined(typeof(TemplEntryInfoAttribute), false) &&
            type.GetFields().Count(IsValidInputField) == ValidInputFieldCount;

        private static bool IsValidInputField(FieldInfo field) =>
            field.IsDefined(typeof(TemplInputAttribute), false) &&
            field.FieldType.IsSubclassOf(typeof(Object));
    }
}