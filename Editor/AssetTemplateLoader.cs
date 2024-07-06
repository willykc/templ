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
using Scriban.Parsing;
using Scriban.Runtime;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;

namespace Willykc.Templ.Editor
{
    internal sealed class AssetTemplateLoader : ITemplateLoader
    {
        private static AssetTemplateLoader instance;

        internal static AssetTemplateLoader Instance => instance ??= new AssetTemplateLoader();

        private AssetTemplateLoader() { }

        string ITemplateLoader.GetPath(
            TemplateContext context,
            SourceSpan callerSpan,
            string templateName)
        {
            if (GUID.TryParse(templateName, out _) &&
                AssetDatabase.GUIDToAssetPath(templateName) is string path &&
                !string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            return templateName;
        }

        string ITemplateLoader.Load(
            TemplateContext context,
            SourceSpan callerSpan,
            string templatePath) => Load(templatePath);

        ValueTask<string> ITemplateLoader.LoadAsync(
            TemplateContext context,
            SourceSpan callerSpan,
            string templatePath) => new ValueTask<string>(Load(templatePath));

        private string Load(string templatePath)
        {
            if (AssetDatabase.LoadAssetAtPath<ScribanAsset>(templatePath) is ScribanAsset template)
            {
                return template.Text;
            }

            throw new FileNotFoundException(
                $"Could not load {nameof(ScribanAsset)} at '{templatePath}'");
        }
    }
}
