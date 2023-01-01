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
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Willykc.Templ.Editor.Tests.Mocks
{
    using Scaffold;

    internal sealed class TemplScaffoldWindowManagerMock : ITemplScaffoldWindowManager
    {
        internal Task<ScriptableObject> ShowInputFormSourceTask =
            Task.FromResult<ScriptableObject>(null);
        internal Task<string[]> ShowOverwriteDialogTask =
            Task.FromResult<string[]>(null);

        private Action closedAction;
        private Func<Task<ScriptableObject>> showInputFormAction;
        private Func<Task<string[]>> showOverwriteDialogAction;

        internal int CloseInputFormCount { get; private set; }
        internal int ShowInputFormCount { get; private set; }
        internal int ShowOverwriteDialogCount { get; private set; }
        internal CancellationToken OverwriteDialogToken { get; private set; }

        internal void SetShowInputFormDelay(int millisecondsDelay, ScriptableObject result)
        {
            async Task<ScriptableObject> Delay()
            {
                await Task.Delay(millisecondsDelay);

                if (result == null)
                {
                    closedAction?.Invoke();
                }

                return result;
            }

            showInputFormAction = Delay;
        }

        internal void SetShowOverwriteDialogDelay(int millisecondsDelay, string[] result)
        {
            async Task<string[]> Delay()
            {
                await Task.Delay(millisecondsDelay);
                return result;
            }

            showOverwriteDialogAction = Delay;
        }

        internal async void CloseInputFormAfter(int millisecondsDelay)
        {
            await Task.Delay(millisecondsDelay);
            closedAction?.Invoke();
        }

        Task<ScriptableObject> ITemplScaffoldWindowManager.ShowInputFormAsync(
            TemplScaffold scaffold,
            string targetPath,
            UnityObject selection,
            Action closed,
            CancellationToken cancellationToken)
        {
            ShowInputFormCount++;
            closedAction = closed;
            return showInputFormAction != null
                ? showInputFormAction.Invoke()
                : ShowInputFormSourceTask;
        }

        void ITemplScaffoldWindowManager.CloseInputForm() => CloseInputFormCount++;

        Task<string[]> ITemplScaffoldWindowManager.ShowOverwriteDialogAsync(
            TemplScaffold scaffold,
            string targetPath,
            string[] paths,
            CancellationToken cancellationToken)
        {
            ShowOverwriteDialogCount++;
            OverwriteDialogToken = cancellationToken;
            return showOverwriteDialogAction != null
                ? showOverwriteDialogAction.Invoke()
                : ShowOverwriteDialogTask;
        }
    }
}
