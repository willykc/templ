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
        private bool changedTriggered;

        [SetUp]
        public void BeforeEach()
        {
            changedTriggered = false;
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
            subject.Change += OnChanged;

            // Act
            subject.AddScaffoldFileNode(new[] { root });

            // Verify
            Assert.IsTrue(changedTriggered, "Change event did not trigger");
        }

        [Test]
        public void GivenNewScaffold_WhenAddingDirectoryNodes_ThenShowTriggerChangedEvent()
        {
            // Setup
            subject.Change += OnChanged;

            // Act
            subject.AddScaffoldDirectoryNode(new[] { root });

            // Verify
            Assert.IsTrue(changedTriggered, "Change event did not trigger");
        }

        [Test]
        public void GivenNewScaffold_WhenMovingNodes_ThenShowTriggerChangedEvent()
        {
            // Setup
            var rootArray = new[] { root };
            subject.AddScaffoldDirectoryNode(rootArray);
            subject.AddScaffoldFileNode(rootArray);
            var directoryNode = subject.Root.Children[0];
            var fileNode = subject.Root.Children[1];
            subject.Change += OnChanged;

            // Act
            subject.MoveScaffoldNodes(directoryNode, 0, new[] { fileNode });

            // Verify
            Assert.IsTrue(changedTriggered, "Change event did not trigger");
        }

        [Test]
        public void GivenNewScaffold_WhenRemovingNodes_ThenShowTriggerChangedEvent()
        {
            // Setup
            var rootArray = new[] { root };
            subject.AddScaffoldDirectoryNode(rootArray);
            var directoryNode = subject.Root.Children[0];
            subject.Change += OnChanged;

            // Act
            subject.RemoveScaffoldNodes(new[] { directoryNode });

            // Verify
            Assert.IsTrue(changedTriggered, "Change event did not trigger");
        }

        [Test]
        public void GivenNewScaffold_WhenCloningNodes_ThenShowTriggerChangedEvent()
        {
            // Setup
            var rootArray = new[] { root };
            subject.AddScaffoldDirectoryNode(rootArray);
            var directoryNode = subject.Root.Children[0];
            subject.Change += OnChanged;

            // Act
            subject.CloneScaffoldNodes(new[] { directoryNode });

            // Verify
            Assert.IsTrue(changedTriggered, "Change event did not trigger");
        }

        private void OnChanged(IReadOnlyList<TemplScaffoldNode> _) => changedTriggered = true;
    }
}
