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
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityObject = UnityEngine.Object;

namespace Willykc.Templ.Editor.Tests
{
    internal static class TemplTestUtility
    {
        private const string AssetsPath = "Assets/";
        private const string MetaExtension = ".meta";
        private const int DefaultCoroutineTimeoutMilliseconds = 5000;

        internal static T CreateTestAsset<T>(string path, out string testAssetPath)
            where T : UnityObject
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!File.Exists(path))
            {
                throw new InvalidOperationException($"Could not find any file with path {path}");
            }

            Guid guid = Guid.NewGuid();
            var filename = Path.GetFileName(path);
            var directory = $"{AssetsPath}{guid}";
            Directory.CreateDirectory(directory);
            testAssetPath = $"{directory}/{filename}";
            File.Copy(path, testAssetPath);

            if (File.Exists(path + MetaExtension))
            {
                File.Copy(path + MetaExtension, testAssetPath + MetaExtension);
            }

            AssetDatabase.ImportAsset(testAssetPath);
            return AssetDatabase.LoadAssetAtPath<T>(testAssetPath);
        }

        internal static bool DeleteTestAsset(UnityObject asset)
        {
            if (!asset)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            var path = AssetDatabase.GetAssetPath(asset);
            var dirPath = Path.GetDirectoryName(path);

            return AssetDatabase.DeleteAsset(path) && AssetDatabase.DeleteAsset(dirPath);
        }

        internal static IEnumerator ToCoroutine(
            Task task,
            int timeoutMilliseconds = DefaultCoroutineTimeoutMilliseconds)
        {
            var stopwatch = Stopwatch.StartNew();
            while (!task.IsCompleted && !task.IsCanceled)
            {
                if (stopwatch.ElapsedMilliseconds > timeoutMilliseconds)
                {
                    throw new TimeoutException();
                }

                yield return null;
            }

            if (task.IsFaulted)
            {
                throw task.Exception;
            }
        }

        internal static IEnumerator ToCoroutine(Func<Task> taskDelegate,
            int timeoutMilliseconds = DefaultCoroutineTimeoutMilliseconds) =>
            ToCoroutine(taskDelegate.Invoke(), timeoutMilliseconds);
    }
}
