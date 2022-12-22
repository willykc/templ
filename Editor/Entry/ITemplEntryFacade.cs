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
using UnityObject = UnityEngine.Object;

namespace Willykc.Templ.Editor.Entry
{
    public interface ITemplEntryFacade
    {
        /// <summary>
        /// Gets all configured entries in settings.
        /// </summary>
        /// <returns>The array of configured entries.</returns>
        TemplEntry[] GetEntries();

        /// <summary>
        /// Adds a new entry in settings.
        /// </summary>
        /// <typeparam name="T">The type of entry.</typeparam>
        /// <param name="inputAsset">The input asset to monitor for changes.</param>
        /// <param name="template">The template to render.</param>
        /// <param name="outputAssetPath">The output asset path.</param>
        /// <returns>The entry ID.</returns>
        string AddEntry<T> (UnityObject inputAsset, ScribanAsset template, string outputAssetPath)
            where T : TemplEntry, new();

        /// <summary>
        /// Updates an existing entry in settings.
        /// </summary>
        /// <param name="id">The ID of the entry.</param>
        /// <param name="inputAsset">The input asset to monitor for changes.</param>
        /// <param name="template">The template to render.</param>
        /// <param name="outputAssetPath">The output asset path.</param>
        void UpdateEntry(
            string id,
            UnityObject inputAsset = null,
            ScribanAsset template = null,
            string outputAssetPath = null);

        /// <summary>
        /// Removes an existing entry from settings.
        /// </summary>
        /// <param name="id">The entry ID.</param>
        void RemoveEntry(string id);

        /// <summary>
        /// Determines if an entry exist with the given outputAssetPath.
        /// </summary>
        /// <param name="outputAssetPath">The output asset path.</param>
        /// <returns>True or false depending on existance of entry.</returns>
        bool EntryExist(string outputAssetPath);

        /// <summary>
        /// Forces to render a specific entry in settings. In case entry ID matches
        /// an invalid entry, it will not be rendered.
        /// </summary>
        /// <param name="id">The entry ID.</param>
        void ForceRenderEntry(string id);

        /// <summary>
        /// Forces to render all valid entries in settings.
        /// </summary>
        void ForceRenderAllValidEntries();
    }
}