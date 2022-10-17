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
using SharpYaml.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Willykc.Templ.Editor.Scaffold
{
    internal static class TemplScaffoldYamlSerializer
    {
        private static readonly Serializer YamlSerializer;

        static TemplScaffoldYamlSerializer()
        {
            var settings = new SerializerSettings()
            {
                EmitAlias = false,
                ComparerForKeySorting = null
            };

            YamlSerializer = new Serializer(settings);
        }

        internal static string SerializeTree(TemplScaffoldRoot root)
        {
            root = root ?? throw new ArgumentNullException(nameof(root));

            var dtoTree = BuildDtoTree(root);

            return YamlSerializer.Serialize(dtoTree);
        }

        internal static TemplScaffoldRoot DeserializeTree(string fromText)
        {
            fromText = !string.IsNullOrWhiteSpace(fromText)
                ? fromText
                : throw new ArgumentException($"{nameof(fromText)} must not be null or empty");

            var dtoTree = YamlSerializer.Deserialize(fromText);
            var children = ReadDtoTree(dtoTree);
            var root = new TemplScaffoldRoot() { name = nameof(TemplScaffold.Root) };
            root.AddChildrenRange(children);

            return root;
        }

        private static List<object> BuildDtoTree(TemplScaffoldNode node)
        {
            var childrenList = new List<object>();

            foreach (var child in node.Children)
            {
                var childDto = new Dictionary<string, object>();

                var value = child is TemplScaffoldFile file
                    ? AssetDatabase.GetAssetPath(file.Template) as object
                    : BuildDtoTree(child);

                childDto.Add(child.name, value);

                childrenList.Add(childDto);
            }

            return childrenList;
        }

        private static List<TemplScaffoldNode> ReadDtoTree(object dtoTree)
        {
            var childrenList = new List<TemplScaffoldNode>();

            if (!(dtoTree is IEnumerable<object> nodeEnumerable))
            {
                throw new InvalidOperationException(
                    "Serialized Scaffold tree nodes must always be collections");
            }

            foreach (var child in nodeEnumerable)
            {
                if (!(child is IDictionary<object, object> childDictionary))
                {
                    throw new InvalidOperationException(
                        "Serialized Scaffold child nodes must always be dictionaries");
                }

                if (childDictionary.Count == 0)
                {
                    continue;
                }

                var childNode = GetNode(childDictionary);
                childrenList.Add(childNode);
            }

            return childrenList;
        }

        private static TemplScaffoldNode GetNode(IDictionary<object, object> childDictionary)
        {
            var firstPair = childDictionary.First();

            var name = firstPair.Key.ToString();

            var nodeInputs = childDictionary
                .Skip(1)
                .ToDictionary(p => p.Key.ToString(), p => p.Value);

            var value = firstPair.Value;
            return value is string path
                ? GetFileNode(name, path, nodeInputs)
                : GetDirectoryNode(name, value);
        }

        private static TemplScaffoldNode GetDirectoryNode(string name, object value)
        {
            var directoryNode = new TemplScaffoldDirectory() { name = name };
            var directoryChildren = ReadDtoTree(value);
            directoryNode.AddChildrenRange(directoryChildren);
            return directoryNode;
        }

        private static TemplScaffoldNode GetFileNode(
            string name,
            string path,
            IDictionary<string, object> nodeInputs)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Could not find template asset at {path}");
            }

            var template = AssetDatabase.LoadAssetAtPath<ScribanAsset>(path);
            return new TemplScaffoldFile(template) { name = name, NodeInputs = nodeInputs };
        }
    }
}
