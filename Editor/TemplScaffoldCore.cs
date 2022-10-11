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
using System.IO;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Willykc.Templ.Editor
{
    using Abstraction;
    using Scaffold;

    internal sealed class TemplScaffoldCore
    {

        internal const string ScaffoldGenerationTitle = "Templ Scaffold Generation";

        private const string InputName = "Input";
        private const string SelectionName = "Selection";
        private const string OutputAssetPathName = "OutputAssetPath";
        private const string ProgressBarValidatingInfo = "Validating...";
        private const string ProgressBarGeneratingInfo = "Generating...";

        private readonly IFileSystem fileSystem;
        private readonly ILogger log;
        private readonly IEditorUtility editorUtility;
        private readonly Type[] typeCache;
        private readonly List<Type> functions;
        private readonly string[] functionConflicts;

        internal TemplScaffoldCore(
            IFileSystem fileSystem,
            ILogger log,
            IEditorUtility editorUtility,
            Type[] typeCache)
        {
            this.fileSystem = fileSystem ??
                throw new ArgumentNullException(nameof(fileSystem));
            this.log = log ??
                throw new ArgumentNullException(nameof(log));
            this.editorUtility = editorUtility ??
                throw new ArgumentNullException(nameof(editorUtility));
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

        internal string[] GenerateScaffold(
            TemplScaffold scaffold,
            string targetPath,
            ScriptableObject input = null,
            Object selection = null)
        {
            var errors = ValidateScaffoldGeneration(scaffold, targetPath, input, selection);

            if (errors.Count(e => e.Type != TemplScaffoldErrorType.Overwrite) > 0)
            {
                log.Error($"Found errors when generating {scaffold.name} scaffold at {targetPath}");
                return new string[0];
            }

            var paths = new List<string>();

            var showIncrement =
                GetShowProgressIncrementAction(scaffold.Root.NodeCount, ProgressBarGeneratingInfo);

            GenerateScaffoldTree(scaffold.Root, targetPath, showIncrement, paths);

            editorUtility.ClearProgressBar();

            log.Info($"{scaffold.name} scaffold generated successfully at {targetPath}");
            return paths.ToArray();
        }

        internal TemplScaffoldError[] ValidateScaffoldGeneration(
            TemplScaffold scaffold,
            string targetPath,
            ScriptableObject input = null,
            Object selection = null)
        {
            if (!scaffold || !scaffold.IsValid)
            {
                throw new ArgumentException($"{nameof(scaffold)} must be not null and valid");
            }

            if (string.IsNullOrWhiteSpace(targetPath))
            {
                throw new ArgumentException($"{nameof(targetPath)} must not be null or empty");
            }

            var errors = new List<TemplScaffoldError>();
            var functionConflictErrors = functionConflicts
                .Select(c => new TemplScaffoldError(TemplScaffoldErrorType.Undefined,
                $"Found duplicate template function name: {c}"));
            errors.AddRange(functionConflictErrors);

            var showProgressIncrement =
                GetShowProgressIncrementAction(scaffold.Root.NodeCount, ProgressBarValidatingInfo);

            CollectScaffoldErrors(scaffold.Root,
                targetPath, input, selection, showProgressIncrement, errors);
            editorUtility.ClearProgressBar();
            return errors.ToArray();
        }

        private Action GetShowProgressIncrementAction(float total, string info)
        {
            float progress = 0;
            void ShowProgressIncrement()
            {
                progress++;
                editorUtility.DisplayProgressBar(ScaffoldGenerationTitle,
                    info, progress / total);
            }
            return ShowProgressIncrement;
        }

        private void GenerateScaffoldTree(
            TemplScaffoldNode node,
            string targetNodePath,
            Action showProgressIncrement,
            List<string> paths)
        {
            var renderedPath = node is TemplScaffoldRoot
                ? targetNodePath
                : $"{targetNodePath}/{node.RenderedName}";

            if (node is TemplScaffoldDirectory)
            {
                fileSystem.CreateDirectory(renderedPath);
                paths.Add(renderedPath);
            }
            else if (node is TemplScaffoldFile fileNode)
            {
                fileSystem.WriteAllText(renderedPath, fileNode.RenderedTemplate);
                paths.Add(renderedPath);
            }

            showProgressIncrement();

            foreach (var child in node.Children)
            {
                GenerateScaffoldTree(child, renderedPath, showProgressIncrement, paths);
            }
        }

        private void CollectScaffoldErrors(
            TemplScaffoldNode node,
            string targetNodePath,
            ScriptableObject input,
            Object selection,
            Action showProgressIncrement,
            List<TemplScaffoldError> errors)
        {
            var context = GetContext(input, selection);

            if (!(node is TemplScaffoldRoot))
            {
                node.RenderedName = RenderTemplate(node, node.name, context,
                TemplScaffoldErrorType.Filename, errors);
                ValidateRenderedName(node, errors);
            }

            var renderedPath = node is TemplScaffoldRoot
                ? targetNodePath
                : $"{targetNodePath}/{node.RenderedName}";

            if (!(node is TemplScaffoldRoot) && fileSystem.FileExists(renderedPath))
            {
                var error = new TemplScaffoldError(TemplScaffoldErrorType.Overwrite, renderedPath);
                errors.Add(error);
            }

            if (node is TemplScaffoldFile fileNode)
            {
                context = GetContext(input, selection, renderedPath);
                fileNode.RenderedTemplate = RenderTemplate(node, fileNode.Template.Text, context,
                    TemplScaffoldErrorType.Template, errors);
            }

            showProgressIncrement();

            foreach (var child in node.Children)
            {
                CollectScaffoldErrors(child, renderedPath,
                    input, selection, showProgressIncrement, errors);
            }
        }

        private void ValidateRenderedName(
            TemplScaffoldNode node,
            List<TemplScaffoldError> errors)
        {
            var message = string.Empty;

            if (string.IsNullOrWhiteSpace(node.RenderedName))
            {
                message = $"Empty {nameof(TemplScaffoldErrorType.Filename)} found for " +
                    $"node {node.NodePath}";
            }

            if (node.RenderedName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                message = "Invalid characters found in " +
                    $"{nameof(TemplScaffoldErrorType.Filename)}: {node.RenderedName} for " +
                    $"node {node.NodePath}";
            }

            if (!string.IsNullOrEmpty(message))
            {
                var error = new TemplScaffoldError(TemplScaffoldErrorType.Filename, message);
                errors.Add(error);
                log.Error(message);
            }
        }

        private TemplateContext GetContext(
            ScriptableObject input,
            Object selection,
            string renderedPath = null)
        {
            var scriptObject = new ScriptObject();
            scriptObject.Import(typeof(TemplFunctions), renamer: member => member.Name);
            functions.ForEach(t => scriptObject.Import(t, renamer: member => member.Name));
            scriptObject.Add(InputName, input);
            scriptObject.Add(SelectionName, selection);
            scriptObject.Add(OutputAssetPathName, renderedPath);
            var context = new TemplateContext();
            context.PushGlobal(scriptObject);
            return context;
        }

        private string RenderTemplate(
            TemplScaffoldNode node,
            string text,
            TemplateContext context,
            TemplScaffoldErrorType errorType,
            List<TemplScaffoldError> errors)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var template = Template.Parse(text);
                    return template.Render(context);
                }
                else
                {
                    var message = $"Empty {errorType} found in node {node.NodePath}";
                    log.Error(message);
                    var error = new TemplScaffoldError(
                        errorType,
                        $"{message}");
                    errors.Add(error);
                }
            }
            catch (Exception e)
            {
                var message = $"Error parsing {errorType} of node {node.NodePath}";
                log.Error(message, e);
                var error = new TemplScaffoldError(
                    errorType,
                    $"{message}: {e.Message}");
                errors.Add(error);
            }

            return string.Empty;
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
    }
}