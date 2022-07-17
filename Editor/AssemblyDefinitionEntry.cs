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
    internal sealed class AssemblyDefinitionEntry : TemplEntry
    {
        private const string AssemblyReferenceExtension = ".asmref";
        private const string ScriptExtension = ".cs";

        public AssemblyDefinitionAsset assembly;

        public override string InputFieldName => nameof(assembly);
        public override bool IsValidInput => assembly;
        public override object InputValue => Assembly.Load(GetAssemblyDefinition().name);
        public override bool DelayRender => true;

        public override bool IsInputChanged(AssetChanges changes) =>
            IsValidInput &&
            (IsAssemblyScriptChanged(changes) || IsAssemblyReferenceChanged(changes));

        public override bool WillDeleteInput(string path)
        {
            if (!path.ToLowerInvariant().EndsWith(AssemblyReferenceExtension))
            {
                return false;
            }
            var asmref = GetAssemblyDefinitionReference(path);
            return IsReference(asmref);
        }

        private bool IsAssemblyScriptChanged(AssetChanges changes) =>
            changes.importedAssets
            .Union(changes.deletedAssets)
            .Union(changes.movedAssets)
            .Union(changes.movedFromAssetPaths)
            .Where(a => a.ToLowerInvariant().EndsWith(ScriptExtension))
            .Select(CompilationPipeline.GetAssemblyDefinitionFilePathFromScriptPath)
            .Contains(AssetDatabase.GetAssetPath(assembly));

        private bool IsAssemblyReferenceChanged(AssetChanges changes) =>
            changes.importedAssets
            .Union(changes.movedAssets)
            .Union(changes.movedFromAssetPaths)
            .Where(a => a.ToLowerInvariant().EndsWith(AssemblyReferenceExtension))
            .Select(a => GetAssemblyDefinitionReference(a))
            .Any(IsReference);

        private bool IsReference(AssemblyDefinitionReference asmref)
        {
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(assembly));
            return asmref.reference.Contains(guid) || asmref.reference == assembly.name;
        }

        private AssemblyDefinition GetAssemblyDefinition() =>
            JsonUtility.FromJson<AssemblyDefinition>(assembly.text);

        private static AssemblyDefinitionReference GetAssemblyDefinitionReference(string path)
        {
            var json = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionReferenceAsset>(path).text;
            return JsonUtility.FromJson<AssemblyDefinitionReference>(json);
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
