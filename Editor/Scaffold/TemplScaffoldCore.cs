/*
 * Copyright (c) 2024 Willy Alberto Kuster
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

namespace Willykc.Templ.Editor.Scaffold
{
    using Abstraction;
    using static TemplSettings;

    internal sealed class TemplScaffoldCore : ITemplScaffoldCore
    {
        internal const string ScaffoldGenerationTitle = "Templ Scaffold Generation";

        private const string ProgressBarValidatingInfo = "Validating...";
        private const string ProgressBarGeneratingInfo = "Generating...";

        private readonly IFileSystem fileSystem;
        private readonly ILogger log;
        private readonly IEditorUtility editorUtility;
        private readonly List<Type> functions;
        private readonly string[] functionConflicts;
        private readonly string[] invalidFunctionNames;

        internal TemplScaffoldCore(
            IFileSystem fileSystem,
            ILogger log,
            IEditorUtility editorUtility,
            ITemplateFunctionProvider templateFunctionProvider)
        {
            this.fileSystem = fileSystem ??
                throw new ArgumentNullException(nameof(fileSystem));
            this.log = log ??
                throw new ArgumentNullException(nameof(log));
            this.editorUtility = editorUtility ??
                throw new ArgumentNullException(nameof(editorUtility));
            templateFunctionProvider = templateFunctionProvider ??
                throw new ArgumentNullException(nameof(templateFunctionProvider));

            functions = templateFunctionProvider.GetTemplateFunctionTypes().ToList();
            functionConflicts = templateFunctionProvider.GetDuplicateTemplateFunctionNames();
            invalidFunctionNames = templateFunctionProvider.GetTemplateFunctionNames()
                .Where(fn => ReservedKeywords.Contains(fn))
                .ToArray();

            if (functionConflicts.Length > 0)
            {
                log.Error("Function name conflicts detected: " +
                    string.Join(", ", functionConflicts));
            }

            if(invalidFunctionNames.Length > 0)
            {
                log.Error($"Reserved keyword(s) used as function name: " +
                    string.Join(", ", invalidFunctionNames));
            }
        }

        string[] ITemplScaffoldCore.GenerateScaffold(
            TemplScaffold scaffold,
            string targetPath,
            object input,
            UnityObject selection,
            string[] skipPaths)
        {
            skipPaths ??= EmptyStringArray;
            var errors = ValidateScaffoldGeneration(scaffold, targetPath, input, selection);

            if (errors.Count(e => e.Type != TemplScaffoldErrorType.Overwrite) > 0)
            {
                log.Error($"Found errors when generating {scaffold.name} scaffold at {targetPath}");
                return EmptyStringArray;
            }

            var paths = new List<string>();

            var showIncrement =
                GetShowProgressIncrementAction(scaffold.Root.NodeCount, ProgressBarGeneratingInfo);

            try
            {
                GenerateScaffoldTree(scaffold.Root, targetPath, showIncrement, paths, skipPaths);
            }
            finally
            {
                editorUtility.ClearProgressBar();
            }

            log.Info($"{scaffold.name} scaffold generated successfully at {targetPath}");
            return paths.ToArray();
        }

        TemplScaffoldError[] ITemplScaffoldCore.ValidateScaffoldGeneration(
            TemplScaffold scaffold,
            string targetPath,
            object input,
            UnityObject selection) =>
            ValidateScaffoldGeneration(scaffold, targetPath, input, selection);

        private TemplScaffoldError[] ValidateScaffoldGeneration(
            TemplScaffold scaffold,
            string targetPath,
            object input = null,
            UnityObject selection = null)
        {
            scaffold = scaffold
                ? scaffold
                : throw new ArgumentException($"{nameof(scaffold)} must not be null");
            targetPath = !string.IsNullOrWhiteSpace(targetPath)
                ? targetPath
                : throw new ArgumentException($"{nameof(targetPath)} must not be null or empty");

            var validationContext = new ValidationContext()
            {
                rootPath = targetPath,
                seed = Guid.NewGuid().ToString(),
                input = input,
                path = targetPath,
                selection = selection,
                errors = new List<TemplScaffoldError>()
            };

            CollectTemplateFunctionNameErrors(validationContext);
            ProcessDynamicScaffold(scaffold, validationContext);
            CheckForEmptyScaffoldTree(scaffold, validationContext);

            if (validationContext.errors.Count > 0)
            {
                return validationContext.errors.ToArray();
            }

            var showProgressIncrement =
                GetShowProgressIncrementAction(scaffold.Root.NodeCount, ProgressBarValidatingInfo);

            try
            {
                CollectScaffoldErrors(scaffold.Root, validationContext, showProgressIncrement);
            }
            finally
            {
                editorUtility.ClearProgressBar();
            }

            return validationContext.errors.ToArray();
        }

        private void CheckForEmptyScaffoldTree(
            TemplScaffold scaffold,
            ValidationContext validationContext)
        {
            if (scaffold.Root?.Children.Count == 0)
            {
                AddError(validationContext.errors, $"Found empty tree for scaffold {scaffold.name}",
                    TemplScaffoldErrorType.Undefined);
            }
        }

        private void CollectTemplateFunctionNameErrors(ValidationContext validationContext)
        {
            var functionConflictErrors = functionConflicts
                .Select(c => new TemplScaffoldError(TemplScaffoldErrorType.Undefined,
                $"Found duplicate template function name: {c}"));
            var reservedKeywordErrors = invalidFunctionNames
                .Select(rk => new TemplScaffoldError(TemplScaffoldErrorType.Undefined,
                $"Found reserved keyword used as function name: {rk}"));
            validationContext.errors.AddRange(functionConflictErrors);
            validationContext.errors.AddRange(reservedKeywordErrors);
        }

        private void ProcessDynamicScaffold(
            TemplScaffold scaffold,
            ValidationContext validationContext)
        {
            if (!(scaffold is TemplDynamicScaffold dynamicScaffold))
            {
                return;
            }

            if (!dynamicScaffold.TreeTemplate || dynamicScaffold.TreeTemplate.HasErrors)
            {
                AddError(validationContext.errors,
                    $"Null or invalid tree template for dynamic scaffold {scaffold.name}",
                    TemplScaffoldErrorType.Template);
                return;
            }

            var templateContext = GetTemplateContext(validationContext);
            var templateText = dynamicScaffold.TreeTemplate.Text;
            var renderedText = string.Empty;

            if (string.IsNullOrWhiteSpace(templateText))
            {
                AddError(validationContext.errors,
                    $"Empty tree template for dynamic scaffold {scaffold.name}",
                    TemplScaffoldErrorType.Template);
                return;
            }

            try
            {
                var template = Template.Parse(templateText);
                renderedText = template.Render(templateContext);
                dynamicScaffold.Deserialize(renderedText);
            }
            catch (Exception e)
            {
                AddError(validationContext.errors,
                    "Error parsing tree for dynamic scaffold " +
                    $"{scaffold.name}:\n{renderedText}",
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

            try
            {
                if (node is TemplScaffoldDirectory && !fileSystem.DirectoryExists(renderedPath))
                {
                    fileSystem.CreateDirectory(renderedPath);
                    paths.Add(renderedPath);
                }
                else if (node is TemplScaffoldFile fileNode && !skipPaths.Contains(renderedPath))
                {
                    fileSystem.WriteAllText(renderedPath, fileNode.RenderedTemplate);
                    paths.Add(renderedPath);
                }
            }
            catch (Exception e)
            {
                log.Error($"Error creating node {node.NodePath} at {renderedPath}", e);
            }

            showProgressIncrement();

            foreach (var child in node.Children)
            {
                GenerateScaffoldTree(child, renderedPath, showProgressIncrement, paths, skipPaths);
            }
        }

        private void CollectScaffoldErrors(
            TemplScaffoldNode node,
            ValidationContext validationContext,
            Action showProgressIncrement)
        {
            var templateContext = GetTemplateContext(validationContext);
            CollectRenderNameErrors(node, validationContext, templateContext);

            validationContext.path = node is TemplScaffoldRoot
                ? validationContext.path
                : $"{validationContext.path}/{node.RenderedName}";

            CheckForFileOverwrite(node, validationContext);

            if (node is TemplScaffoldFile fileNode &&
                TryGetContext(fileNode, validationContext, out templateContext))
            {
                CollectFileNodeErrors(fileNode, templateContext, validationContext);
            }
            else if (node is TemplScaffoldDirectory && node.Children.Count == 0)
            {
                AddError(validationContext.errors, $"Empty directory node {node.NodePath}",
                    TemplScaffoldErrorType.Undefined);
            }

            showProgressIncrement();
            CheckForDuplicateNodeNames(node, validationContext);

            foreach (var child in node.Children)
            {
                CollectScaffoldErrors(child, validationContext, showProgressIncrement);
            }
        }

        private void CheckForDuplicateNodeNames(
            TemplScaffoldNode node,
            ValidationContext validationContext)
        {
            if (IsNameDuplicated(node))
            {
                AddError(validationContext.errors,
                    "Different sister node with the same name found for node " +
                    node.NodePath, TemplScaffoldErrorType.Filename);
            }
        }

        private void CollectRenderNameErrors(
            TemplScaffoldNode node,
            ValidationContext validationContext,
            TemplateContext context)
        {
            if (!(node is TemplScaffoldRoot))
            {
                node.RenderedName = RenderTemplate(node, node.name, context,
                TemplScaffoldErrorType.Filename, validationContext.errors);
                ValidateRenderedName(node, validationContext.errors);
            }
        }

        private void CheckForFileOverwrite(
            TemplScaffoldNode node,
            ValidationContext validationContext)
        {
            if (!(node is TemplScaffoldRoot) && fileSystem.FileExists(validationContext.path))
            {
                var error = new TemplScaffoldError(TemplScaffoldErrorType.Overwrite,
                    validationContext.path);
                validationContext.errors.Add(error);
            }
        }

        private bool TryGetContext(
            TemplScaffoldFile fileNode,
            ValidationContext validationContext,
            out TemplateContext templateContext)
        {
            templateContext = null;

            try
            {
                templateContext = GetTemplateContext(validationContext, fileNode.NodeInputs);
                return true;
            }
            catch (Exception e)
            {
                AddError(validationContext.errors,
                    $"Error preparing context for node {fileNode.NodePath}",
                    TemplScaffoldErrorType.Undefined, e);
                return false;
            }
        }

        private void CollectFileNodeErrors(
            TemplScaffoldFile fileNode,
            TemplateContext context,
            ValidationContext validationContext)
        {
            if (fileNode.Template && !fileNode.Template.HasErrors)
            {
                fileNode.RenderedTemplate = RenderTemplate(fileNode, fileNode.Template.Text,
                    context, TemplScaffoldErrorType.Template, validationContext.errors);
            }
            else
            {
                AddError(validationContext.errors,
                    $"Null or invalid template found for node {fileNode.NodePath}",
                    TemplScaffoldErrorType.Template);
            }
        }

        private static bool IsNameDuplicated(TemplScaffoldNode node) =>
            node.Parent?.Children.Any(c => c != node && c.name == node.name) ?? false;

        private void ValidateRenderedName(
            TemplScaffoldNode node,
            List<TemplScaffoldError> errors)
        {
            if (string.IsNullOrWhiteSpace(node.RenderedName))
            {
                AddError(errors, $"Empty {nameof(TemplScaffoldErrorType.Filename)} found for " +
                    $"node {node.NodePath}", TemplScaffoldErrorType.Filename);
            }

            if (!node.RenderedName.IsValidFileName())
            {
                AddError(errors, "Invalid characters found in " +
                    $"{nameof(TemplScaffoldErrorType.Filename)}: {node.RenderedName} for " +
                    $"node {node.NodePath}", TemplScaffoldErrorType.Filename);
            }
        }

        private TemplateContext GetTemplateContext(
            ValidationContext validationContext,
            IDictionary<string, object> nodeInputs = null)
        {
            nodeInputs ??= new Dictionary<string, object>();
            var scriptObject = new ScriptObject();
            scriptObject.Import(typeof(TemplFunctions), renamer: member => member.Name);
            functions.ForEach(t => scriptObject.Import(t, renamer: member => member.Name));
            scriptObject.Add(InputName, validationContext.input);
            scriptObject.Add(SelectionName, validationContext.selection);
            scriptObject.Add(NameOfOutputAssetPath, validationContext.path);
            scriptObject.Add(SeedName, validationContext.seed);
            scriptObject.Add(RootPathName, validationContext.rootPath);

            var inputCollisions = nodeInputs.Keys.Where(scriptObject.ContainsKey).ToArray();
            if (inputCollisions.Length > 0)
            {
                throw new InvalidOperationException("Found node input(s) named as reserved " +
                    "keywords: " + string.Join(", ", inputCollisions));
            }

            foreach (var nodeInput in nodeInputs)
            {
                scriptObject.Add(nodeInput.Key, nodeInput.Value);
            }

            var templateContext = new TemplateContext()
            {
                TemplateLoader = AssetTemplateLoader.Instance
            };

            templateContext.PushGlobal(scriptObject);
            return templateContext;
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

        private struct ValidationContext
        {
            internal object input;
            internal UnityObject selection;
            internal string path;
            internal List<TemplScaffoldError> errors;
            internal string seed;
            internal string rootPath;
        }
    }
}
