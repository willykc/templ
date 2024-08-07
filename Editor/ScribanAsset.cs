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
using System.Linq;
using UnityEngine;

namespace Willykc.Templ.Editor
{
    using static TemplSettings;

    public sealed class ScribanAsset : ScriptableObject
    {
        /// <summary>
        /// Template text.
        /// </summary>
        public string Text => text;
        /// <summary>
        /// Template validity.
        /// </summary>
        public bool HasErrors => hasErrors;
        /// <summary>
        /// Array of syntax errors.
        /// </summary>
        public string[] ParsingErrors { get; private set; }

        [SerializeField, HideInInspector]
        private string text;

        [SerializeField, HideInInspector]
        private bool hasErrors;

        internal ScribanAsset Init(string text)
        {
            this.text = text;
            var template = Template.Parse(text);
            ParsingErrors = (hasErrors = template.HasErrors)
                ? template.Messages
                .Select(m => $"{m.Type}: {m.Message}")
                .ToArray()
                : EmptyStringArray;
            return this;
        }
    }
}
