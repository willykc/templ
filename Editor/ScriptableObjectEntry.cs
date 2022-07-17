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
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Willykc.Templ.Editor
{
    internal sealed class ScriptableObjectEntry : TemplEntry
    {
        public ScriptableObject scriptableObject;

        public override string InputFieldName => nameof(scriptableObject);
        public override bool IsValidInput =>
            scriptableObject &&
            !(scriptableObject is ScribanAsset) &&
            !(scriptableObject is TemplSettings);
        public override object InputValue => scriptableObject;
        public override bool DelayRender => false;

        public override bool IsInputChanged(AssetChanges changes) =>
            IsValidInput &&
            changes.importedAssets.Contains(AssetDatabase.GetAssetPath(scriptableObject));

        public override bool WillDeleteInput(string path) => false;
    }
}
