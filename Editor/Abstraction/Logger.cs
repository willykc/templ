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

namespace Willykc.Templ.Editor.Abstraction
{
    internal sealed class Logger : ILogger
    {
        private const string Label = "[Templ]";
        private const string InfoLabel = "<color=green>" + Label + "</color> ";
        private const string ErrorLabel = "<color=red>" + Label + "</color> ";
        private const string WarningLabel = "<color=yellow>" + Label + "</color> ";

        private static ILogger logger;

        internal static ILogger Instance => logger ??= new Logger();

        private Logger() { }

        void ILogger.Error(string message, Exception exception)
        {
            var currentType = Application.GetStackTraceLogType(LogType.Error);
            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
            Debug.LogError(ErrorLabel + message);
            Application.SetStackTraceLogType(LogType.Error, currentType);
            if (exception != null)
            {
                Debug.LogException(exception);
            }
        }

        void ILogger.Info(string message)
        {
            var currentType = Application.GetStackTraceLogType(LogType.Log);
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Debug.Log(InfoLabel + message);
            Application.SetStackTraceLogType(LogType.Log, currentType);
        }

        void ILogger.Warn(string message)
        {
            var currentType = Application.GetStackTraceLogType(LogType.Warning);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            Debug.LogWarning(WarningLabel + message);
            Application.SetStackTraceLogType(LogType.Warning, currentType);
        }
    }
}
