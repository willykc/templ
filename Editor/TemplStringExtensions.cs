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
namespace Willykc.Templ.Editor
{
    internal static class TemplStringExtensions
    {
        internal static string Capitalize(this string input) =>
            !string.IsNullOrEmpty(input)
            ? char.ToUpperInvariant(input[0]) + input.Substring(1)
            : string.Empty;

        internal static string ReplaceFirst(this string input, string search, string replace) =>
            string.IsNullOrEmpty(input) ||
            string.IsNullOrEmpty(search) ||
            string.IsNullOrEmpty(replace) ||
            (input.IndexOf(search) is int pos && pos < 0)
            ? input
            : input.Substring(0, pos) + replace + input.Substring(pos + search.Length);

        internal static string RemoveLast(this string input, int count) =>
            string.IsNullOrEmpty(input) || input.Length < count
            ? string.Empty
            : input.Remove(input.Length - count);
    }
}
