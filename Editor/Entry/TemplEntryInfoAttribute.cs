/*
 * Copyright (c) 2023 Willy Alberto Kuster
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
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class TemplEntryInfoAttribute : Attribute
    {
        /// <summary>
        /// Types of changes the entry is meant to monitor.
        /// </summary>
        public ChangeType ChangeTypes { get; }

        /// <summary>
        /// The display name when adding the entry in the settings. Defaults to the name of the
        /// entry class.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Determines if the entry should be rendered before (false) or after (true) recompilation.
        /// Defaults to false.
        /// </summary>
        public bool Deferred { get; set; }

        public TemplEntryInfoAttribute(
            ChangeType changeTypes)
        {
            ChangeTypes = changeTypes;
        }
    }
}
