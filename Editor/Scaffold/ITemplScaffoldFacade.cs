/*
 * Copyright (c) 2023 Willy Alberto Kuster
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
using System.Threading;
using System.Threading.Tasks;
using UnityObject = UnityEngine.Object;

namespace Willykc.Templ.Editor.Scaffold
{
    public interface ITemplScaffoldFacade
    {
        internal bool IsGenerating { get; }

        /// <summary>
        /// Generates scaffold at target path. Asset database must be refreshed afterwards
        /// for the editor to show the generated assets.
        /// </summary>
        /// <param name="scaffold">The scaffold to generate.</param>
        /// <param name="targetPath">The asset path where to generate the scaffold.</param>
        /// <param name="input">The input value to use during generation.</param>
        /// <param name="selection">The selection value to use during generation.</param>
        /// <param name="overwriteOption">The options to control asset overwrite behaviour.</param>
        /// <param name="cancellationToken">The cancellation token. It can only cancel UI prompts, once generation starts it must fail or conclude.</param>
        /// <returns>The array of generated asset paths. Null is returned in case user cancels UI prompts or generation errors are found.</returns>
        Task<string[]> GenerateScaffoldAsync(
            TemplScaffold scaffold,
            string targetPath,
            object input = null,
            UnityObject selection = null,
            OverwriteOptions overwriteOption = OverwriteOptions.ShowPrompt,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Enables scaffold in settings for selection from the context menu.
        /// </summary>
        /// <param name="scaffold">The scaffold to enable for selection.</param>
        void EnableScaffoldForSelection(TemplScaffold scaffold);

        /// <summary>
        /// Disables scaffold in settings for selection from the context menu.
        /// </summary>
        /// <param name="scaffold">The scaffold to disable for selection.</param>
        void DisableScaffoldForSelection(TemplScaffold scaffold);
    }
}