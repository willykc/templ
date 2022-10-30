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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Willykc.Templ.Editor
{
    internal static class TemplStringExtensions
    {
        private const char Dash = '-';
        private const char Underscore = '_';
        private const char Whitespace = ' ';

        private static readonly char[] Separators = new[] { Dash, Underscore, Whitespace };

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

        internal static string ToSnakeCase(this string text) =>
            ConvertCase(text,
                first: char.ToLowerInvariant,
                currentSeparator: _ => Underscore,
                previousSeparator: char.ToLowerInvariant,
                nextUpper: char.ToLowerInvariant,
                nextUpperSuffix: _ => Underscore,
                anyOther: char.ToLowerInvariant);

        internal static string ToKebabCase(this string text) =>
            ConvertCase(text,
                first: char.ToLowerInvariant,
                currentSeparator: _ => Dash,
                previousSeparator: char.ToLowerInvariant,
                nextUpper: char.ToLowerInvariant,
                nextUpperSuffix: _ => Dash,
                anyOther: char.ToLowerInvariant);

        internal static string ToPascalCase(this string text) =>
            ConvertCase(text,
                first: char.ToUpperInvariant,
                currentSeparator: _ => default,
                previousSeparator: char.ToUpperInvariant,
                nextUpper: c => c,
                nextUpperSuffix: _ => default,
                anyOther: c => c);

        internal static string ToCamelCase(this string text) =>
            ConvertCase(text,
                first: char.ToLowerInvariant,
                currentSeparator: _ => default,
                previousSeparator: char.ToUpperInvariant,
                nextUpper: c => c,
                nextUpperSuffix: _ => default,
                anyOther: c => c);

        private static string ConvertCase(
            string text,
            Func<char, char> first,
            Func<char, char> currentSeparator,
            Func<char, char> previousSeparator,
            Func<char, char> nextUpper,
            Func<char, char> nextUpperSuffix,
            Func<char, char> anyOther)
        {
            text = Sanitize(text);
            var builder = new StringBuilder();

            for (var i = 0; i < text.Length; i++)
            {
                Decompose(text, i, out var current, out var previous, out var next);

                if (i == 0)
                {
                    SafeAppend(builder, first(current));
                }
                else if (Separators.Contains(previous))
                {
                    SafeAppend(builder, previousSeparator(current));
                }
                else if (Separators.Contains(current))
                {
                    SafeAppend(builder, currentSeparator(current));
                }
                else if (char.IsUpper(next))
                {
                    SafeAppend(builder, nextUpper(current));
                    SafeAppend(builder, nextUpperSuffix(current));
                }
                else
                {
                    SafeAppend(builder, anyOther(current));
                }
            }

            return builder.ToString();
        }

        private static void SafeAppend(StringBuilder builder, char toAppend)
        {
            if (toAppend != default)
            {
                builder.Append(toAppend);
            }
        }

        private static void Decompose(
            string text,
            int index,
            out char current,
            out char previous,
            out char next)
        {
            current = text[index];
            previous = index > 0
                ? text[index - 1]
                : default;
            next = index < text.Length - 1
                ? text[index + 1]
                : default;
        }

        private static string Sanitize(string text)
        {
            text ??= string.Empty;
            text = text.Trim();
            text = text.Trim(Separators);
            var separators = string.Join(string.Empty, Separators);
            var expression = $"[{separators}]+";
            return Regex.Replace(text, expression, Separators[0].ToString());
        }
    }
}
