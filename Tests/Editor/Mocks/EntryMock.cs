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
using UnityEngine;

namespace Willykc.Templ.Editor.Tests.Mocks
{
    [TemplEntryInfo(ChangeType.All)]
    internal sealed class EntryMock : TemplEntry
    {
        [TemplInput]
        public TextAsset text;
        [SerializeField]
        internal bool valid = true;
        internal bool defer;
        internal bool inputChanged;
        internal bool templateChanged;
        internal string outputAssetPath;

        internal override string OutputAssetPath => string.IsNullOrWhiteSpace(outputAssetPath)
            ? filename
            : outputAssetPath;

        internal override bool Deferred => defer;

        protected override bool IsValidInputField => valid;

        protected override object InputValue => text.text;

        internal override bool IsTemplateChanged(AssetChange change) => templateChanged;

        protected override bool IsInputChanged(AssetChange change) => inputChanged;

        internal void Clear()
        {
            valid = true;
            defer = default;
            inputChanged = default;
            templateChanged = default;
            outputAssetPath = default;
        }
    }
}