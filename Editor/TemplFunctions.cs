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
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using CompilationPipeline = UnityEditor.Compilation.CompilationPipeline;
using UnityObject = UnityEngine.Object;

namespace Willykc.Templ.Editor
{
    internal static class TemplFunctions
    {
        #region Reflection
        /// <summary>
        /// Gets a Type object from the specified assembly and name.
        /// </summary>
        /// <param name="assembly">The target assembly.</param>
        /// <param name="typeName">The type name.</param>
        /// <returns>The Type object.</returns>
        public static Type GetType(Assembly assembly, string typeName) =>
            Type.GetType($"{typeName}, {assembly.FullName}");

        /// <summary>
        /// Gets all Type objects from the specified assembly.
        /// </summary>
        /// <param name="assembly">The target assembly.</param>
        /// <returns>The array of Type objects.</returns>
        public static Type[] GetTypes(Assembly assembly) => assembly.GetTypes();

        /// <summary>
        /// Gets all Type objects without a parent type from the specified assembly.
        /// </summary>
        /// <param name="assembly">The target assembly.</param>
        /// <returns>The array of Type objects.</returns>
        public static Type[] GetMainTypes(Assembly assembly) =>
            assembly.GetTypes().Where(t => t.DeclaringType == null).ToArray();

        /// <summary>
        /// Gets all types decorated with the specified Attribute from the specified assembly.
        /// </summary>
        /// <param name="assembly">The target assembly.</param>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>The array of Type objects.</returns>
        public static Type[] GetTypesWithAttribute(Assembly assembly, string attributeName) =>
            assembly.GetTypes()
            .Where(t =>
            Attribute.GetCustomAttribute(t,
                Type.GetType($"{attributeName}{nameof(Attribute)}, {assembly.FullName}")) != null)
            .ToArray();

        /// <summary>
        /// Gets all Type objects which are subclasses of the specified Type name from the specified
        /// assembly.
        /// </summary>
        /// <param name="assembly">The target assembly.</param>
        /// <param name="parentName">The parent Type name.</param>
        /// <returns>The array of Type objects.</returns>
        public static Type[] GetSubclassesOf(Assembly assembly, string parentName) =>
            assembly.GetTypes()
            .Where(t => t.IsSubclassOf(Type.GetType($"{parentName}, {assembly.FullName}")))
            .ToArray();

        /// <summary>
        /// Gets an Attribute object from a specified Type object and Attribute name.
        /// </summary>
        /// <param name="type">The target Type object.</param>
        /// <param name="attributeName">The Attribute name.</param>
        /// <returns>The Attribute object.</returns>
        public static Attribute GetAttribute(Type type, string attributeName) =>
            Attribute.GetCustomAttribute(type,
                Type.GetType($"{attributeName}{nameof(Attribute)}, {type.Assembly.FullName}"));

        /// <summary>
        /// Gets all Attribute objects from the specified Type object.
        /// </summary>
        /// <param name="type">The target Type object.</param>
        /// <returns>The array of Attribute objects.</returns>
        public static Attribute[] GetAttributes(Type type) => Attribute.GetCustomAttributes(type);

        /// <summary>
        /// Gets a MethodInfo object from the specified Type object and method name.
        /// </summary>
        /// <param name="type">The target Type object.</param>
        /// <param name="name">The method name.</param>
        /// <returns>The MethodInfo object.</returns>
        public static MethodInfo GetMethod(Type type, string name) => type.GetMethod(name);

        /// <summary>
        /// Gets all MethodInfo objects from the specified Type object.
        /// </summary>
        /// <param name="type">The target Type object.</param>
        /// <returns>The array of MethodInfo objects.</returns>
        public static MethodInfo[] GetMethods(Type type) => type.GetMethods(
            BindingFlags.DeclaredOnly |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.Static);

        /// <summary>
        /// Gets a FieldInfo object from the specified Type object and field name.
        /// </summary>
        /// <param name="type">The target Type object.</param>
        /// <param name="name">The field name.</param>
        /// <returns>The FieldInfo object.</returns>
        public static FieldInfo GetField(Type type, string name) => type.GetField(name);

        /// <summary>
        /// Gets all FieldInfo objects from the specified Type object.
        /// </summary>
        /// <param name="type">The target Type object.</param>
        /// <returns>The array of FieldInfo objects.</returns>
        public static FieldInfo[] GetFields(Type type) => type.GetFields(
            BindingFlags.DeclaredOnly |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.Static);

        /// <summary>
        /// Gets a PropertyInfo object from the specified Type object and property name.
        /// </summary>
        /// <param name="type">The target Type object.</param>
        /// <param name="name">The property name.</param>
        /// <returns>The PropertyInfo object.</returns>
        public static PropertyInfo GetProperty(Type type, string name) => type.GetProperty(name);

        /// <summary>
        /// Gets all PropertyInfo objects from the specified Type object.
        /// </summary>
        /// <param name="type">The target Type object.</param>
        /// <returns>The array of PropertyInfo objects.</returns>
        public static PropertyInfo[] GetProperties(Type type) => type.GetProperties(
            BindingFlags.DeclaredOnly |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.Static);

        /// <summary>
        /// Gets a Unity GUID from the specified Type object.
        /// </summary>
        /// <param name="type">The target Type object.</param>
        /// <returns>The Unity GUID string.</returns>
        public static string GetTypeGuid(Type type) =>
            AssetDatabase.FindAssets($"{type.Name} t:script")
            .Select(g => new { path = AssetDatabase.GUIDToAssetPath(g), guid = g })
            .SingleOrDefault(p => $"{type.Assembly.GetName().Name}.dll" ==
            CompilationPipeline.GetAssemblyNameFromScriptPath(p.path) &&
            Path.GetFileNameWithoutExtension(p.path) == type.Name)
            ?.guid ?? string.Empty;

        /// <summary>
        /// Gets a Unity file ID from the specified Type object.
        /// </summary>
        /// <param name="type">The target Type object.</param>
        /// <returns>The Unity field ID string.</returns>
        public static string GetTypeFileId(Type type) =>
            ulong.Parse(GetTypeGuid(type).Substring(0, 16), NumberStyles.HexNumber)
            .ToString()
            .Substring(0, 19);
        #endregion

        #region Path
        /// <summary>
        /// Gets the filename portion of the specified path.
        /// </summary>
        /// <param name="path">The target path.</param>
        /// <returns>The filename string.</returns>
        public static string GetFileName(string path) => Path.GetFileName(path);

        /// <summary>
        /// Gets the filename portion without extension from the specified path.
        /// </summary>
        /// <param name="path">The target path.</param>
        /// <returns>The filename string.</returns>
        public static string GetFileNameWithoutExtension(string path) =>
            Path.GetFileNameWithoutExtension(path);

        /// <summary>
        /// Gets the file extension from the specified path.
        /// </summary>
        /// <param name="path">The target path.</param>
        /// <returns>The file extension string.</returns>
        public static string GetFileExtension(string path) => Path.GetExtension(path);

        /// <summary>
        /// Gets the directory portion of the specified path.
        /// </summary>
        /// <param name="path">The target path.</param>
        /// <returns>The directory string.</returns>
        public static string GetDirectoryName(string path) => Path.GetDirectoryName(path);
        #endregion

        #region Utility
        /// <summary>
        /// Evaluates a boolean expression and throws an exception with the specified message
        /// when false.
        /// </summary>
        /// <param name="condition">The boolean expression.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="Exception">The exception with message.</exception>
        public static void Assert(bool condition, string message = default)
        {
            if (!condition)
            {
                message = string.IsNullOrWhiteSpace(message)
                    ? message
                    : $" with message: {message}";

                throw new Exception($"Assertion failed{message}");
            }
        }

        /// <summary>
        /// Transforms the specified text string to camelCase.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <returns>The camelCase string.</returns>
        public static string CamelCase(string text) => text.ToCamelCase();

        /// <summary>
        /// Transforms the specified text string to PascalCase.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <returns>The PascalCase string.</returns>
        public static string PascalCase(string text) => text.ToPascalCase();

        /// <summary>
        /// Transforms the specified text string to kebab-case.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <returns>The kebab-case string.</returns>
        public static string KebabCase(string text) => text.ToKebabCase();

        /// <summary>
        /// Transforms the specified text string to snake_case.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <returns>The snake_case string.</returns>
        public static string SnakeCase(string text) => text.ToSnakeCase();
        #endregion

        #region Asset Database
        /// <summary>
        /// Gets the asset path of the specified UnityEngine.Object instance.
        /// </summary>
        /// <param name="asset">The UnityEngine.Object instance.</param>
        /// <returns>The asset path string.</returns>
        public static string GetAssetPath(UnityObject asset) =>
            AssetDatabase.GetAssetPath(asset);

        /// <summary>
        /// Gets the asset GUID of the specified UnityEngine.Object instance.
        /// </summary>
        /// <param name="asset">The UnityEngine.Object instance.</param>
        /// <returns>The asset GUID string.</returns>
        public static string GetAssetGuid(UnityObject asset) =>
            AssetDatabase.AssetPathToGUID(GetAssetPath(asset));

        /// <summary>
        /// Gets the asset file ID of the specified UnityEngine.Object instance.
        /// </summary>
        /// <param name="asset">The UnityEngine.Object instance.</param>
        /// <returns>The asset file ID string.</returns>
        public static string GetAssetFileId(UnityObject asset) =>
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out var _, out long id)
            ? id.ToString()
            : string.Empty;


        /// <summary>
        /// Gets the asset instance ID of the specified UnityEngine.Object instance.
        /// </summary>
        /// <param name="asset">The UnityEngine.Object instance.</param>
        /// <returns>The asset instance ID string.</returns>
        public static string GetAssetInstanceID(UnityObject asset) => asset
            ? asset.GetInstanceID().ToString()
            : string.Empty;
        #endregion
    }
}
