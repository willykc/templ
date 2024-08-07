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
using UnityObject = UnityEngine.Object;

namespace Willykc.Templ.Editor.Tests
{
    using Scaffold;

    internal sealed class TemplScaffoldCoreMock : ITemplScaffoldCore
    {
        internal string[] paths = new string[0];
        internal TemplScaffoldError[] errors = new TemplScaffoldError[0];

        internal string[] SkipPaths { get; private set; }
        internal int GenerateScaffoldCount { get; private set; }

        string[] ITemplScaffoldCore.GenerateScaffold(
            TemplScaffold scaffold,
            string targetPath,
            object input,
            UnityObject selection,
            string[] skipPaths)
        {
            SkipPaths = skipPaths;
            GenerateScaffoldCount++;
            return paths;
        }

        TemplScaffoldError[] ITemplScaffoldCore.ValidateScaffoldGeneration(
            TemplScaffold scaffold,
            string targetPath,
            object input,
            UnityObject selection) => errors;
    }
}
