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
using System.Collections.Generic;

namespace Willykc.Templ.Editor.Tests.Mocks
{
    using Abstraction;

    internal sealed class FileSystemMock : IFileSystem
    {
        internal List<string> Path { get; } = new List<string>();
        internal List<string> DirectoryPath { get; } = new List<string>();
        internal List<string> Contents { get; } = new List<string>();
        internal List<string> FileExists { get; } = new List<string>();
        internal int WriteAllTextCount { get; private set; }
        internal int CreateDirectoryCount { get; private set; }
        internal bool ThrowException { get; set; }

        void IFileSystem.WriteAllText(string path, string contents)
        {
            if (ThrowException)
            {
                throw new Exception();
            }
            Path.Add(path);
            Contents.Add(contents);
            WriteAllTextCount++;
        }

        void IFileSystem.CreateDirectory(string path)
        {
            DirectoryPath.Add(path);
            CreateDirectoryCount++;
        }

        bool IFileSystem.FileExists(string path) => FileExists.Contains(path);

        internal void Clear()
        {
            Path.Clear();
            Contents.Clear();
        }
    }
}
