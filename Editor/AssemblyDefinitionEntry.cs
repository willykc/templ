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
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;
using Assembly = System.Reflection.Assembly;

namespace Willykc.Templ.Editor
{
    [TemplEntryInfo(ChangeType.All, deferred: true)]
    internal sealed class AssemblyDefinitionEntry : TemplEntry
    {
        private const string AssemblyReferenceExtension = ".asmref";
        private const string ScriptExtension = ".cs";

        [TemplInput]
        public AssemblyDefinitionAsset assembly;

        protected override bool IsValidInputField => assembly;

        protected override object InputValue => Assembly.Load(GetAssemblyDefinition().name);

        protected override bool IsInputChanged(AssetChange change) =>
            IsAssemblyScriptChanged(change) || IsAssemblyReferenceChanged(change);

        private bool IsAssemblyScriptChanged(AssetChange change) =>
            new[] { change.currentPath, change.previousPath }
            .Where(a => a.ToLowerInvariant().EndsWith(ScriptExtension))
            .Select(CompilationPipeline.GetAssemblyDefinitionFilePathFromScriptPath)
            .Contains(AssetDatabase.GetAssetPath(assembly));

        private bool IsAssemblyReferenceChanged(AssetChange change) =>
            new[] { change.currentPath, change.previousPath }
            .Where(a => a.ToLowerInvariant().EndsWith(AssemblyReferenceExtension))
            .Select(a => GetAssemblyDefinitionReference(a))
            .Any(IsReference);

        private bool IsReference(AssemblyDefinitionReference asmref)
        {
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(assembly));
            return !string.IsNullOrEmpty(asmref.reference) &&
                (asmref.reference.Contains(guid) || asmref.reference == assembly.name);
        }

        private AssemblyDefinition GetAssemblyDefinition() =>
            JsonUtility.FromJson<AssemblyDefinition>(assembly.text);

        private static AssemblyDefinitionReference GetAssemblyDefinitionReference(string path)
        {
            var json = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionReferenceAsset>(path)?.text;
            return json != null
                ? JsonUtility.FromJson<AssemblyDefinitionReference>(json)
                : default;
        }

        private struct AssemblyDefinition
        {
            public string name;
        }

        private struct AssemblyDefinitionReference
        {
            public string reference;
        }
    }
}
