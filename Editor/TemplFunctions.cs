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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using CompilationPipeline = UnityEditor.Compilation.CompilationPipeline;

namespace Willykc.Templ.Editor
{
    internal static class TemplFunctions
    {
        #region Reflection
        public static Type GetType(Assembly assembly, string typeName) =>
            Type.GetType($"{typeName}, {assembly.FullName}");

        public static Type[] GetTypes(Assembly assembly) => assembly.GetTypes();

        public static Type[] GetMainTypes(Assembly assembly) =>
            assembly.GetTypes().Where(t => t.DeclaringType == null).ToArray();

        public static Type[] GetTypesWithAttribute(Assembly assembly, string attributeName) =>
            assembly.GetTypes()
            .Where(t =>
            Attribute.GetCustomAttribute(t,
                Type.GetType($"{attributeName}{nameof(Attribute)}, {assembly.FullName}")) != null)
            .ToArray();

        public static Type[] GetSubclassesOf(Assembly assembly, string parentName) =>
            assembly.GetTypes()
            .Where(t => t.IsSubclassOf(Type.GetType($"{parentName}, {assembly.FullName}")))
            .ToArray();

        public static Attribute GetAttribute(Type type, string attributeName) =>
            Attribute.GetCustomAttribute(type,
                Type.GetType($"{attributeName}{nameof(Attribute)}, {type.Assembly.FullName}"));

        public static Attribute[] GetAttributes(Type type) => Attribute.GetCustomAttributes(type);

        public static MethodInfo GetMethod(Type type, string name) => type.GetMethod(name);

        public static MethodInfo[] GetMethods(Type type) => type.GetMethods(
            BindingFlags.DeclaredOnly |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.Static);

        public static FieldInfo GetField(Type type, string name) => type.GetField(name);

        public static FieldInfo[] GetFields(Type type) => type.GetFields(
            BindingFlags.DeclaredOnly |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.Static);

        public static PropertyInfo GetProperty(Type type, string name) => type.GetProperty(name);

        public static PropertyInfo[] GetProperties(Type type) => type.GetProperties(
            BindingFlags.DeclaredOnly |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.Static);

        public static string GetTypeGuid(Type type) =>
            AssetDatabase.FindAssets($"{type.Name} t:script")
            .Select(g => new { path = AssetDatabase.GUIDToAssetPath(g), guid = g })
            .SingleOrDefault(p => $"{type.Assembly.GetName().Name}.dll" ==
            CompilationPipeline.GetAssemblyNameFromScriptPath(p.path) &&
            Path.GetFileNameWithoutExtension(p.path) == type.Name)
            ?.guid ?? string.Empty;

        public static string GetTypeFileId(Type type) =>
            ulong.Parse(GetTypeGuid(type).Substring(0, 16), NumberStyles.HexNumber)
            .ToString()
            .Substring(0, 19);
        #endregion

        #region Path
        public static string GetFileName(string path) => Path.GetFileName(path);

        public static string GetFileNameWithoutExtension(string path) =>
            Path.GetFileNameWithoutExtension(path);

        public static string GetDirectoryName(string path) => Path.GetDirectoryName(path);
        #endregion

        #region Assets
        public static int GetAssetInstanceID(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Asset file does not exist at path: {path}");
            }

            return AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)).GetInstanceID();
        }
        #endregion

    }
}
