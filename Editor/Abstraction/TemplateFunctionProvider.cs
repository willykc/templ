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
using System.Linq;

namespace Willykc.Templ.Editor.Abstraction
{
    using static TemplEditorInitialization;

    internal sealed class TemplateFunctionProvider : ITemplateFunctionProvider
    {
        private Type[] functionTypeCache;
        private string[] duplicateFunctionNamesCache;

        private static ITemplateFunctionProvider templateFunctionProvider;

        internal static ITemplateFunctionProvider Instance =>
            templateFunctionProvider ??= new TemplateFunctionProvider();

        private TemplateFunctionProvider() { }

        Type[] ITemplateFunctionProvider.GetTemplateFunctionTypes() =>
            GetTemplateFunctionTypes();

        string[] ITemplateFunctionProvider.GetDuplicateTemplateFunctionNames() =>
            duplicateFunctionNamesCache ??= GetTemplateFunctionTypes()
            .SelectMany(t => t.GetMethods())
            .Union(typeof(TemplFunctions).GetMethods())
            .Where(m => m.IsStatic)
            .Select(m => m.Name)
            .GroupBy(n => n)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        private Type[] GetTemplateFunctionTypes() =>
            functionTypeCache ??= TypeCache
            .Where(t => Attribute.GetCustomAttribute(t, typeof(TemplFunctionsAttribute)) != null &&
            t.IsAbstract &&
            t.IsSealed)
            .ToArray();
    }
}
