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

        private static readonly string[] EmptyStringArray = new string[0];

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
            Object selection = null,
            string[] skipPaths = null)
        {
            skipPaths ??= EmptyStringArray;
            var errors = ValidateScaffoldGeneration(scaffold, targetPath, input, selection);

            if (errors.Count(e => e.Type != TemplScaffoldErrorType.Overwrite) > 0)
            {
                log.Error($"Found errors when generating {scaffold.name} scaffold at {targetPath}");
                return new string[0];
            }

            var paths = new List<string>();

            var showIncrement =
                GetShowProgressIncrementAction(scaffold.Root.NodeCount, ProgressBarGeneratingInfo);

            GenerateScaffoldTree(scaffold.Root, targetPath, showIncrement, paths, skipPaths);

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
            scaffold = scaffold
                ? scaffold
                : throw new ArgumentException($"{nameof(scaffold)} must not be null");
            targetPath = !string.IsNullOrWhiteSpace(targetPath)
                ? targetPath
                : throw new ArgumentException($"{nameof(targetPath)} must not be null or empty");

            var errors = new List<TemplScaffoldError>();
            var functionConflictErrors = functionConflicts
                .Select(c => new TemplScaffoldError(TemplScaffoldErrorType.Undefined,
                $"Found duplicate template function name: {c}"));
            errors.AddRange(functionConflictErrors);

            ProcessDynamicScaffold(scaffold, input, selection, errors);

            if (errors.Count > 0)
            {
                return errors.ToArray();
            }

            var showProgressIncrement =
                GetShowProgressIncrementAction(scaffold.Root.NodeCount, ProgressBarValidatingInfo);

            CollectScaffoldErrors(scaffold.Root,
                targetPath, input, selection, showProgressIncrement, errors);
            editorUtility.ClearProgressBar();
            return errors.ToArray();
        }

        private void ProcessDynamicScaffold(
            TemplScaffold scaffold,
            ScriptableObject input,
            Object selection,
            List<TemplScaffoldError> errors)
        {
            if (!(scaffold is TemplDynamicScaffold dynamicScaffold))
            {
                return;
            }

            var context = GetContext(input, selection);
            var templateText = dynamicScaffold.StructureTemplate.Text;
            var renderedText = string.Empty;

            if (string.IsNullOrWhiteSpace(templateText))
            {
                AddError(errors,
                    $"Empty structure template for scaffold {scaffold.name}",
                    TemplScaffoldErrorType.Template);
                return;
            }

            try
            {
                var template = Template.Parse(templateText);
                renderedText = template.Render(context);
                dynamicScaffold.Deserialize(renderedText);
            }
            catch (Exception e)
            {
                AddError(errors,
                    $"Error processing structure for scaffold {scaffold.name}: {renderedText}",
                    TemplScaffoldErrorType.Template, e);
            }
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
            List<string> paths,
            string[] skipPaths)
        {
            var renderedPath = node is TemplScaffoldRoot
                ? targetNodePath
                : $"{targetNodePath}/{node.RenderedName}";

            if (node is TemplScaffoldDirectory)
            {
                fileSystem.CreateDirectory(renderedPath);
                paths.Add(renderedPath);
            }
            else if (node is TemplScaffoldFile fileNode && !skipPaths.Contains(renderedPath))
            {
                fileSystem.WriteAllText(renderedPath, fileNode.RenderedTemplate);
                paths.Add(renderedPath);
            }

            showProgressIncrement();

            foreach (var child in node.Children)
            {
                GenerateScaffoldTree(child, renderedPath, showProgressIncrement, paths, skipPaths);
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
                CollectFileNodeErrors(fileNode, context, errors);
            }

            showProgressIncrement();

            if (IsNameDuplicated(node))
            {
                AddError(errors, "Different sister node with the same name found for node " +
                    node.NodePath, TemplScaffoldErrorType.Filename);
            }

            foreach (var child in node.Children)
            {
                CollectScaffoldErrors(child, renderedPath,
                    input, selection, showProgressIncrement, errors);
            }
        }

        private void CollectFileNodeErrors(
            TemplScaffoldFile fileNode,
            TemplateContext context,
            List<TemplScaffoldError> errors)
        {
            if (fileNode.Template && !fileNode.Template.HasErrors)
            {
                fileNode.RenderedTemplate = RenderTemplate(fileNode, fileNode.Template.Text,
                    context, TemplScaffoldErrorType.Template, errors);
            }
            else
            {
                AddError(errors, $"Null or invalid template found for node {fileNode.NodePath}",
                    TemplScaffoldErrorType.Template);
            }
        }

        private static bool IsNameDuplicated(TemplScaffoldNode node) =>
            node.Parent?.Children.Any(c => c != node && c.name == node.name) ?? false;

        private void ValidateRenderedName(
            TemplScaffoldNode node,
            List<TemplScaffoldError> errors)
        {
            var message = string.Empty;

            if (string.IsNullOrWhiteSpace(node.RenderedName))
            {
                AddError(errors, $"Empty {nameof(TemplScaffoldErrorType.Filename)} found for " +
                    $"node {node.NodePath}", TemplScaffoldErrorType.Filename);
            }

            if (node.RenderedName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                AddError(errors, "Invalid characters found in " +
                    $"{nameof(TemplScaffoldErrorType.Filename)}: {node.RenderedName} for " +
                    $"node {node.NodePath}", TemplScaffoldErrorType.Filename);
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
                    AddError(errors, $"Empty {errorType} found in node {node.NodePath}", errorType);
                }
            }
            catch (Exception e)
            {
                AddError(errors, $"Error rendering {errorType} of node {node.NodePath}",
                    errorType, e);
            }

            return string.Empty;
        }

        private void AddError(
            List<TemplScaffoldError> errors,
            string message,
            TemplScaffoldErrorType type,
            Exception exception = null)
        {
            if (exception != null)
            {
                log.Error(message, exception);
                message = $"{message}: {exception.Message}";
            }
            else
            {
                log.Error(message);
            }

            var error = new TemplScaffoldError(type, message);
            errors.Add(error);
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
