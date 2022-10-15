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
using UnityEngine;

namespace Willykc.Templ.Editor.Scaffold
{
    [CreateAssetMenu(
        fileName = NewPrefix + nameof(TemplDynamicScaffold),
        menuName = MenuName,
        order = 3)]
    internal sealed class TemplDynamicScaffold : TemplScaffold
    {
        internal const string NameOfStructureTemplate = nameof(structureTemplate);

        private const string MenuName = "Templ/Dynamic Scaffold Definition";

        [SerializeField]
        private ScribanAsset structureTemplate;

        [NonSerialized]
        private TemplScaffold innerScaffold;

        internal override TemplScaffoldRoot Root => innerScaffold.Root;

        internal ScribanAsset StructureTemplate => structureTemplate;

        internal override bool IsValid => structureTemplate && !structureTemplate.HasErrors;

        internal override bool ContainsTemplate(ScribanAsset template) =>
            structureTemplate == template;

        internal bool IsInnerValid => innerScaffold.IsValid;

        internal void Deserialize(string serialized)
        {
            if (!innerScaffold)
            {
                innerScaffold = CreateInstance<TemplScaffold>();
                innerScaffold.hideFlags = HideFlags.HideAndDontSave;
            }

            JsonUtility.FromJsonOverwrite(serialized, innerScaffold);
        }

        private new void Reset()
        {
            base.Reset();
            structureTemplate = null;
            DestroyImmediate(innerScaffold);
        }
    }
}
