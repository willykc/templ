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
using Scriban;
using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityObject = UnityEngine.Object;

namespace Willykc.Templ.Editor
{
    using Abstraction;

    internal sealed class TemplCore
    {
        private const string TemplChangedKey = "templ.changed";
        private const string TemplDelayedKey = "templ.delayed";

        private readonly IAssetDatabase assetDatabase;
        private readonly IFile file;
        private readonly ISessionState sessionState;
        private readonly ILogger log;
        private readonly ISettingsProvider settingsProvider;
        private readonly Type[] typeCache;

        private readonly List<Type> functions;
        private readonly string[] functionConflicts;

        internal TemplCore(
            IAssetDatabase assetDatabase,
            IFile file,
            ISessionState sessionState,
            ILogger log,
            ISettingsProvider settingsProvider,
            Type[] typeCache)
        {
            this.assetDatabase = assetDatabase ??
                throw new ArgumentNullException(nameof(assetDatabase));
            this.file = file ??
                throw new ArgumentNullException(nameof(file));
            this.sessionState = sessionState ??
                throw new ArgumentNullException(nameof(sessionState));
            this.log = log ??
                throw new ArgumentNullException(nameof(log));
            this.settingsProvider = settingsProvider ??
                throw new ArgumentNullException(nameof(settingsProvider));
            this.typeCache = typeCache ??
                throw new ArgumentNullException(nameof(typeCache));
            functions = GetTemplateFunctions();
            functionConflicts = GetFunctionNameConflicts();
            if (functionConflicts.Length > 0)
            {
                log.Error("Function name conflicts detected: " +
                    string.Join(", ", functionConflicts));
            }
        }

        internal void OnAssetsChanged(AssetChanges changes)
        {
            if (!settingsProvider.SettingsExist() || FunctionConflictsDetected())
            {
                return;
            }
            if (IsSettingsChanged(changes))
            {
                RenderChangedEntries();
                return;
            }
            var entriesToRender = settingsProvider.GetSettings().ValidEntries
                .Where(e => e.ShouldRender(changes));
            FlagDelayedEntries(entriesToRender, changes);
            RenderEagerEntries(entriesToRender, changes);
        }

        internal void OnAfterAssemblyReload()
        {
            if (!settingsProvider.SettingsExist() || FunctionConflictsDetected())
            {
                return;
            }
            var delayed = sessionState.GetString(TemplDelayedKey);
            if (string.IsNullOrWhiteSpace(delayed))
            {
                return;
            }
            var delayedEntries = settingsProvider.GetSettings().ValidEntries
                .Where(e => delayed.Contains(e.guid));
            RenderEntries(delayedEntries);
            sessionState.EraseString(TemplDelayedKey);
        }

        internal void OnWillDeleteAsset(string path)
        {
            if (!settingsProvider.SettingsExist() || FunctionConflictsDetected())
            {
                return;
            }
            FlagInputDeletedEntries(path);
        }

        internal void FlagChangedEntry(TemplEntry entry)
        {
            if (entry == null)
            {
                return;
            }
            var existing = sessionState.GetString(TemplChangedKey);
            if (!existing.Contains(entry.guid))
            {
                sessionState.SetString(TemplChangedKey, existing + entry.guid);
            }
        }

        internal void RenderAllValidEntries()
        {
            if (!settingsProvider.SettingsExist() || FunctionConflictsDetected())
            {
                return;
            }
            RenderEntries(settingsProvider.GetSettings().ValidEntries);
        }

        private static UnityObject GetInput(TemplEntry e) =>
            e.GetType()
            .GetField(e.InputFieldName)
            .GetValue(e) as UnityObject;

        private bool FunctionConflictsDetected()
        {
            if (functionConflicts.Length > 0)
            {
                log.Error("Templates will not render due to function name conflicts");
                return true;
            }
            return false;
        }

        private List<Type> GetTemplateFunctions() =>
            typeCache
            .Where(t => Attribute.GetCustomAttribute(t, typeof(TemplFunctionsAttribute)) != null &&
            t.IsAbstract &&
            t.IsSealed)
            .ToList();

        private string[] GetFunctionNameConflicts() =>
            functions
            .SelectMany(t => t.GetMethods())
            .Union(typeof(TemplFunctions).GetMethods())
            .Where(m => m.IsStatic)
            .Select(m => m.Name)
            .GroupBy(n => n)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        private bool IsSettingsChanged(AssetChanges changes)
        {
            var settingsPath = assetDatabase.GetAssetPath(settingsProvider.GetSettings());
            return changes.importedAssets.Contains(settingsPath);
        }

        private void RenderChangedEntries()
        {
            var changed = sessionState.GetString(TemplChangedKey);
            if (string.IsNullOrWhiteSpace(changed))
            {
                return;
            }
            var entriesToRender = settingsProvider.GetSettings().ValidEntries
                .Where(e => changed.Contains(e.guid));
            RenderEntries(entriesToRender);
            sessionState.EraseString(TemplChangedKey);
        }

        private void FlagDelayedEntries(IEnumerable<TemplEntry> entries, AssetChanges changes)
        {
            var delayed = entries
                .Where(e => e.DelayRender && !e.IsTemplateChanged(changes))
                .Select(e => e.guid);
            var delayedFlags = string.Concat(delayed);
            if (!string.IsNullOrWhiteSpace(delayedFlags))
            {
                sessionState.SetString(TemplDelayedKey, delayedFlags);
            }
        }

        private void FlagInputDeletedEntries(string path)
        {
            var delayed = settingsProvider.GetSettings().ValidEntries
                .Where(e => e.WillDeleteInput(path))
                .Select(e => e.guid);
            var delayedFlags = string.Concat(delayed);
            if (!string.IsNullOrWhiteSpace(delayedFlags))
            {
                sessionState.SetString(TemplDelayedKey, delayedFlags);
            }
        }

        private void RenderEagerEntries(
            IEnumerable<TemplEntry> entries,
            AssetChanges changes)
        {
            var entriesToRenderNow = entries
                .Where(e => !e.DelayRender || e.IsTemplateChanged(changes));
            RenderEntries(entriesToRenderNow);
        }

        private void RenderEntries(IEnumerable<TemplEntry> entriesToRender)
        {
            var inputPaths = settingsProvider.GetSettings().ValidEntries
                .Select(e => assetDatabase.GetAssetPath(GetInput(e)))
                .ToArray();
            var templatePaths = settingsProvider.GetSettings().ValidEntries
                .Select(e => assetDatabase.GetAssetPath(e.template))
                .ToArray();
            foreach (var entry in entriesToRender)
            {
                RenderEntry(entry, inputPaths, templatePaths);
            }
            if (entriesToRender.Any() && settingsProvider.GetSettings().HasInvalidEntries)
            {
                log.Warn("Invalid settings found");
            }
        }

        private void RenderEntry(TemplEntry entry, string[] inputPaths, string[] templatePaths)
        {
            if (!CheckOverrides(entry, inputPaths, templatePaths))
            {
                return;
            }
            try
            {
                var context = GetContext(entry);
                var template = Template.Parse(entry.template.Text);
                var result = template.Render(context);
                file.WriteAllText(entry.FullPath, result);
                assetDatabase.ImportAsset(entry.OutputAssetPath);
                log.Info($"Template rendered at {entry.FullPath}");
            }
            catch (Exception e)
            {
                log.Error($"Error while rendering template at {entry.FullPath}", e);
            }
        }

        private bool CheckOverrides(TemplEntry entry, string[] inputPaths, string[] templatePaths)
        {
            if (inputPaths.Contains(entry.OutputAssetPath))
            {
                log.Error("Overwriting an input in settings is not allowed. " +
                    $"Did not render template at {entry.FullPath}");
                return false;
            }
            if (templatePaths.Contains(entry.OutputAssetPath))
            {
                log.Error("Overwriting a template in settings is not allowed. " +
                    $"Did not render template at {entry.FullPath}");
                return false;
            }
            if (entry.OutputAssetPath == assetDatabase.GetAssetPath(settingsProvider.GetSettings()))
            {
                log.Error("Overwriting settings is not allowed. " +
                    $"Did not render template at {entry.FullPath}");
                return false;
            }
            return true;
        }

        private TemplateContext GetContext(TemplEntry entry)
        {
            var scriptObject = new ScriptObject();
            scriptObject.Import(typeof(TemplFunctions), renamer: member => member.Name);
            functions.ForEach(t => scriptObject.Import(t, renamer: member => member.Name));
            scriptObject.Add(entry.InputFieldName, entry.InputValue);
            scriptObject.Add(nameof(entry.OutputAssetPath), entry.OutputAssetPath);
            var context = new TemplateContext();
            context.PushGlobal(scriptObject);
            return context;
        }
    }
}
