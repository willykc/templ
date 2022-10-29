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
        private TemplScaffoldNode[] emptyNodeArray;
        private bool changedTriggered;

        [SetUp]
        public void BeforeEach()
        {
            changedTriggered = false;
            emptyNodeArray = new TemplScaffoldNode[0];
            subject = ScriptableObject.CreateInstance<TemplScaffold>();
        }

        [Test]
        public void GivenNewScaffold_WhenInstantiated_ThenRootShouldNotBeNull()
        {
            // Verify
            Assert.NotNull(subject.Root, "Expected not null root");
        }

        [Test]
        public void GivenNewScaffold_WhenAddingFileNodes_ThenShouldTriggerChangedEvent()
        {
            // Setup
            subject.Change += OnChanged;

            // Act
            subject.AddScaffoldFileNode(emptyNodeArray);

            // Verify
            Assert.IsTrue(changedTriggered, "Change event did not trigger");
        }

        [Test]
        public void GivenNewScaffold_WhenAddingDirectoryNodes_ThenShouldTriggerChangedEvent()
        {
            // Setup
            subject.Change += OnChanged;

            // Act
            subject.AddScaffoldDirectoryNode(emptyNodeArray);

            // Verify
            Assert.IsTrue(changedTriggered, "Change event did not trigger");
        }

        [Test]
        public void GivenScaffoldWithNodes_WhenMovingNodes_ThenShouldTriggerChangedEvent()
        {
            // Setup
            subject.AddScaffoldDirectoryNode(emptyNodeArray);
            subject.AddScaffoldFileNode(emptyNodeArray);
            var directoryNode = subject.Root.Children[0];
            var fileNode = subject.Root.Children[1];
            subject.Change += OnChanged;

            // Act
            subject.MoveScaffoldNodes(directoryNode, 0, new[] { fileNode });

            // Verify
            Assert.IsTrue(changedTriggered, "Change event did not trigger");
        }

        [Test]
        public void GivenScaffoldWithNodes_WhenRemovingNodes_ThenShouldTriggerChangedEvent()
        {
            // Setup
            subject.AddScaffoldDirectoryNode(emptyNodeArray);
            var directoryNode = subject.Root.Children[0];
            subject.Change += OnChanged;

            // Act
            subject.RemoveScaffoldNodes(new[] { directoryNode });

            // Verify
            Assert.IsTrue(changedTriggered, "Change event did not trigger");
        }

        [Test]
        public void GivenScaffoldWithNodes_WhenCloningNodes_ThenShouldTriggerChangedEvent()
        {
            // Setup
            subject.AddScaffoldDirectoryNode(emptyNodeArray);
            var directoryNode = subject.Root.Children[0];
            subject.Change += OnChanged;

            // Act
            subject.CloneScaffoldNodes(new[] { directoryNode });

            // Verify
            Assert.IsTrue(changedTriggered, "Change event did not trigger");
        }

        [Test]
        public void GivenNewScaffold_WhenInstantiated_ThenShouldBeValid()
        {
            // Verify
            Assert.IsTrue(subject.IsValid, "Expected valid scaffold");
        }

        [Test]
        public void GivenNewScaffold_WhenInstantiated_ThenDefaultInputShouldBeNull()
        {
            // Verify
            Assert.IsNull(subject.DefaultInput, "Expected null default input");
        }

        [Test]
        public void GivenNewScaffold_WhenAddingFileNodes_ThenShouldAddFileChildToRoot()
        {
            // Act
            subject.AddScaffoldFileNode(emptyNodeArray);

            // Verify
            Assert.AreEqual(1, subject.Root.Children.Count, "Unexpected number of children");
            Assert.That(subject.Root.Children[0], Is.TypeOf<TemplScaffoldFile>(),
                "Unexpected child type");
        }

        [Test]
        public void GivenNewScaffold_WhenAddingDirectoryNodes_ThenShouldAddDirectoryChildToRoot()
        {
            // Act
            subject.AddScaffoldDirectoryNode(emptyNodeArray);

            // Verify
            Assert.AreEqual(1, subject.Root.Children.Count, "Unexpected number of children");
            Assert.That(subject.Root.Children[0], Is.TypeOf<TemplScaffoldDirectory>(),
                "Unexpected child type");
        }

        [Test]
        public void GivenScaffoldWithNodes_WhenAddingFileNodes_ThenShouldAddToInputNode()
        {
            // Setup
            subject.AddScaffoldDirectoryNode(emptyNodeArray);
            var directoryNode = subject.Root.Children[0];
            subject.Change += OnChanged;

            // Act
            subject.AddScaffoldFileNode(new[] { directoryNode });

            // Verify
            Assert.AreEqual(1, directoryNode.Children.Count, "Unexpected number of children");
            Assert.That(directoryNode.Children[0], Is.TypeOf<TemplScaffoldFile>(),
                "Unexpected child type");
        }

        [Test]
        public void GivenScaffoldWithNodes_WhenAddingDirectoryNodes_ThenShouldAddToInputNode()
        {
            // Setup
            subject.AddScaffoldDirectoryNode(emptyNodeArray);
            var directoryNode = subject.Root.Children[0];
            subject.Change += OnChanged;

            // Act
            subject.AddScaffoldDirectoryNode(new[] { directoryNode });

            // Verify
            Assert.AreEqual(1, directoryNode.Children.Count, "Unexpected number of children");
            Assert.That(directoryNode.Children[0], Is.TypeOf<TemplScaffoldDirectory>(),
                "Unexpected child type");
        }

        [Test]
        public void GivenScaffoldWithNodes_WhenMovingNodes_ThenShouldMoveThemCorrectly()
        {
            // Setup
            subject.AddScaffoldDirectoryNode(emptyNodeArray);
            subject.AddScaffoldFileNode(emptyNodeArray);
            var directoryNode = subject.Root.Children[0];
            subject.AddScaffoldFileNode(new[] { directoryNode });
            var fileNode = subject.Root.Children[1];
            subject.Change += OnChanged;

            // Act
            subject.MoveScaffoldNodes(directoryNode, 1, new[] { fileNode });

            // Verify
            Assert.That(subject.Root.Children, Does.Not.Contains(fileNode), "Unexpected child");
            Assert.That(directoryNode.Children[1], Is.EqualTo(fileNode), "Expected child");
        }

        private void OnChanged(IReadOnlyList<TemplScaffoldNode> _) => changedTriggered = true;
    }
}
