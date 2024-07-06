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
using System;

namespace Willykc.Templ.Editor.Entry
{
    /// <summary>
    /// Change type as flags enum.
    /// </summary>
    [Flags]
    public enum ChangeType
    {
        /// <summary>
        /// No change.
        /// </summary>
        None = 0,
        /// <summary>
        /// When an asset is imported or reimported (updated).
        /// </summary>
        Import = 1,
        /// <summary>
        /// When an asset is moved.
        /// </summary>
        Move = 2,
        /// <summary>
        /// When an asset is deleted.
        /// </summary>
        Delete = 4,
        /// <summary>
        /// All types of changes at once.
        /// </summary>
        All = Import | Move | Delete,
    }
}
