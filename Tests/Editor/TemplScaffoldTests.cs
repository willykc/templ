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
using NUnit.Framework;

namespace Willykc.Templ.Editor.Tests
{
    using Scaffold;
    using System.Collections.Generic;
    using UnityEngine;

    internal class TemplScaffoldTests
    {
        private TemplScaffold subject;
        private TemplScaffoldRoot root;

        [SetUp]
        public void BeforeEach()
        {
            subject = ScriptableObject.CreateInstance<TemplScaffold>();
            root = subject.Root;
        }

        [Test]
        public void GivenNewScaffold_WhenInstantiated_ThenRootShouldNotBeNull()
        {
            // Verify
            Assert.NotNull(root, "Expected not null root");
        }

        [Test]
        public void GivenNewScaffold_WhenAddingFileNodes_ThenShowTriggerChangedEvent()
        {
            // Setup
            var changedTriggered = false;
            subject.Change += OnChanged;

            void OnChanged(IReadOnlyList<TemplScaffoldNode> _)
            {
                changedTriggered = true;
            }

            // Act
            subject.AddScaffoldFileNode(new[] { root });

            // Verify
            Assert.IsTrue(changedTriggered, "Change event did not trigger");
        }
    }
}
