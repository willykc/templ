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
using System.Collections.Generic;
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
            // Verify
            Assert.IsFalse(subject.IsValid, "Expected invalid dynamic scaffold");
        }

        [Test]
        public void GivenValidDynamicScaffold_WhenCheckingValidity_ThenShouldBeTrue()
        {
            // Verify
            Assert.IsTrue(loadedSubject.IsValid, "Expected valid dynamic scaffold");
        }
    }
}
