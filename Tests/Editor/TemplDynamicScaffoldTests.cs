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
using System;
using UnityEngine;

namespace Willykc.Templ.Editor.Tests
{
    using Scaffold;
    using static TemplScaffoldCoreTests;

    internal class TemplDynamicScaffoldTests
    {
        private TemplDynamicScaffold subject;
        private ScribanAsset testTreeTemplate;
        private TemplDynamicScaffold loadedSubject;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            testTreeTemplate = TemplTestUtility
                .CreateTestAsset<ScribanAsset>(TestTreeTemplatePath, out _);
            loadedSubject = TemplTestUtility
                .CreateTestAsset<TemplDynamicScaffold>(TestDynamicScaffoldPath, out _);
        }

        [SetUp]
        public void BeforeEach()
        {
            subject = ScriptableObject.CreateInstance<TemplDynamicScaffold>();
        }

        [OneTimeTearDown]
        public void AfterAll()
        {
            TemplTestUtility.DeleteTestAsset(testTreeTemplate);
            TemplTestUtility.DeleteTestAsset(loadedSubject);
        }

        [Test]
        public void GivenNewDynamicScaffold_WhenInstantiated_ThenRootShouldBeNull()
        {
            // Verify
            Assert.IsNull(subject.Root, "Expected null root");
        }

        [Test]
        public void GivenNewDynamicScaffold_WhenInstantiated_ThenTreeTemplateShouldBeNull()
        {
            // Verify
            Assert.IsNull(subject.TreeTemplate, "Expected null tree template");
        }

        [Test]
        public void GivenNewDynamicScaffold_WhenInstantiated_ThenShouldBeInvalid()
        {
            // Act
            var isValid = subject.IsValid;

            // Verify
            Assert.IsFalse(isValid, "Expected invalid dynamic scaffold");
        }

        [Test]
        public void GivenValidDynamicScaffold_WhenCheckingValidity_ThenShouldBeTrue()
        {
            // Act
            var isValid = loadedSubject.IsValid;

            // Verify
            Assert.IsTrue(isValid, "Expected valid dynamic scaffold");
        }

        [Test]
        public void GivenDynamicScaffoldWithTemplate_WhenCheckingIfContained_ThenShouldBeTrue()
        {
            // Act
            var containsTemplate = loadedSubject.ContainsTemplate(testTreeTemplate);

            // Verify
            Assert.IsTrue(containsTemplate, "Expected contained template");
        }

        [Test]
        public void GivenNewDynamicScaffold_WhenCheckingIfTemplateContained_ThenShouldBeFalse()
        {
            // Act
            var containsTemplate = subject.ContainsTemplate(testTreeTemplate);

            // Verify
            Assert.IsFalse(containsTemplate, "Expected not contained template");
        }

        [Test]
        public void GivenNewDynamicScaffold_WhenDeserializingValidYAML_ThenShouldBuildTree()
        {
            // Setup
            var directoryName = "TestDirectory";
            var fileName = "test.txt";
            var treeText = "- " + directoryName + ":\r\n    - " +
                fileName + ": 44d4dcb342fdfb340bc506f0cc2bc93a\r\n";

            // Act
            subject.Deserialize(treeText);

            // Verify
            Assert.IsNotNull(subject.Root, "Root should not be null");
            Assert.AreEqual(3, subject.Root.NodeCount, "Unexpected total node count");
            Assert.AreEqual(directoryName, subject.Root.Children[0].name, "Unexpected node name");
            Assert.AreEqual(fileName, subject.Root.Children[0].Children[0].name,
                "Unexpected node name");
        }

        [Test]
        public void GivenNewDynamicScaffold_WhenDeserializingInvalidYAML_ThenShouldThrowException()
        {
            // Setup
            var treeText = "        - TestDirectory:" +
                "\r\n    test.txt: 44d4dcb342fdfb340bc506f0cc2bc93a\r\n";

            void Act()
            {
                // Act
                subject.Deserialize(treeText);
            }

            // Verify
            Assert.Throws(Is.AssignableTo<Exception>(), Act, "Expected exception");
        }
    }
}
