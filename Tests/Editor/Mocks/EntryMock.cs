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
    internal sealed class EntryMock : TemplEntry
    {
        public TextAsset text;
        [SerializeField]
        internal bool valid = true;
        internal bool delay;
        internal bool inputChanged;
        internal bool willDelete;
        internal bool templateChanged;
        internal string outputAssetPath;

        public override string InputFieldName => nameof(text);

        public override bool IsValidInput => valid;

        public override object InputValue => text.text;

        public override bool DelayRender => delay;

        public override bool IsInputChanged(AssetChanges changes) => inputChanged;

        public override bool WillDeleteInput(string path) => willDelete;

        internal override bool IsTemplateChanged(AssetChanges changes) => templateChanged;

        internal override string OutputAssetPath => string.IsNullOrWhiteSpace(outputAssetPath)
            ? filename
            : outputAssetPath;

        internal void Clear()
        {
            valid = true;
            delay = default;
            inputChanged = default;
            willDelete = default;
            templateChanged = default;
            outputAssetPath = default;
        }
    }
}
