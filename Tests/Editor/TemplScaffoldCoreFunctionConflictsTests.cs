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
using NUnit.Framework;

namespace Willykc.Templ.Editor.Tests
{
    using Mocks;
    using Scaffold;
    using static TemplScaffoldCoreTests;

    internal class TemplScaffoldCoreFunctionConflictsTests
    {
        private ITemplScaffoldCore subject;
        private FileSystemMock fileSystemMock;
        private LoggerMock loggerMock;
        private EditorUtilityMock editorUtilityMock;
        private TemplateFunctionProviderMock templateFunctionProviderMock;
        private TemplScaffold testScaffold;
        private ScribanAsset testScaffoldTemplate;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            testScaffoldTemplate = TemplTestUtility
                .CreateTestAsset<ScribanAsset>(TestScaffoldTemplatePath, out _);
            testScaffold = TemplTestUtility.CreateTestAsset<TemplScaffold>(TestScaffoldPath, out _);
        }

        [SetUp]
        public void BeforeEach()
        {
            templateFunctionProviderMock = new TemplateFunctionProviderMock
            {
                DuplicateFunctionNames = new[] { "GetType" }
            };

            subject = new TemplScaffoldCore(
                fileSystemMock = new FileSystemMock(),
                loggerMock = new LoggerMock(),
                editorUtilityMock = new EditorUtilityMock(),
                templateFunctionProviderMock);
        }

        [OneTimeTearDown]
        public void AfterAll()
        {
            TemplTestUtility.DeleteTestAsset(testScaffoldTemplate);
            TemplTestUtility.DeleteTestAsset(testScaffold);
        }

        [Test]
        public void GivenFunctionConflicts_WhenInstantiatingTemplScaffoldCore_ThenShouldLogError()
        {
            // Verify
            Assert.AreEqual(1, loggerMock.ErrorCount, "Did not log error");
        }

        [Test]
        public void GivenFunctionConflicts_WhenValidating_ThenShouldReturnCorrectError()
        {
            // Act
            var errors = subject.ValidateScaffoldGeneration(testScaffold, TestTargetPath);

            // Verify
            Assert.IsNotEmpty(errors, "Errors expected");
            Assert.AreEqual(TemplScaffoldErrorType.Undefined, errors[0].Type, "Wrong error type");
            Assert.That(errors[0].Message.Contains("Found duplicate template function"),
                "Wrong error");
        }

        [Test]
        public void GivenFunctionConflicts_WhenGenerating_ThenShouldLogError()
        {
            // Act
            subject.GenerateScaffold(testScaffold, TestTargetPath);

            // Verify
            Assert.AreEqual(2, loggerMock.ErrorCount, "Did not log error");
        }
    }
}
