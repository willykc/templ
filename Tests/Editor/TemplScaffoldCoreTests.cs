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
using UnityEngine;

namespace Willykc.Templ.Editor.Tests
{
    using Mocks;
    using Scaffold;

    internal class TemplScaffoldCoreTests
    {
        internal const string TestScaffoldPath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets~/TestScaffold.asset";
        internal const string TestScaffoldTemplatePath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets~/TestScaffoldTemplate.sbn";
        internal const string TestTargetPath = "Assets/Some/Path";

        private const string TestDynamicScaffoldPath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets~/TestDynamicScaffold.asset";
        private const string TestTreeTemplatePath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets~/TestTreeTemplate.sbn";
        private const string TestDynamicScaffoldWithEmptyTemplatePath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets~/" +
            "TestDynamicScaffoldWithEmptyTemplate.asset";
        private const string TestEmptyTreeTemplatePath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets~/TestEmptyTreeTemplate.sbn";
        private const string TestEmptyDirectoryScaffoldPath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets~/TestEmptyDirectoryScaffold.asset";
        private const string TestNullTemplateScaffoldPath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets~/TestNullTemplateScaffold.asset";
        private const string InputName = "roach";

        private static readonly string[] Elements = new[] { "one", "two" };

        private TemplScaffoldCore subject;
        private InputType testInput;
        private Object testSelection;
        private string expectedDirectoryPath;
        private string expectedFilePath;
        private string expectedContent;
        private FileSystemMock fileSystemMock;
        private LoggerMock loggerMock;
        private EditorUtilityMock editorUtilityMock;
        private TemplateFunctionProviderMock templateFunctionProviderMock;
        private TemplScaffold testScaffold;
        private TemplScaffold testDynamicScaffold;
        private TemplScaffold testDynamicScaffoldWithEmptyTemplate;
        private TemplScaffold testEmptyDirectoryScaffold;
        private TemplScaffold testNullTemplateScaffold;
        private ScribanAsset testTreeTemplate;
        private ScribanAsset testEmptyTreeTemplate;
        private ScribanAsset testScaffoldTemplate;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            testScaffoldTemplate = TemplTestUtility
                .CreateTestAsset<ScribanAsset>(TestScaffoldTemplatePath, out _);
            testScaffold = TemplTestUtility.CreateTestAsset<TemplScaffold>(TestScaffoldPath, out _);
            testTreeTemplate = TemplTestUtility
                .CreateTestAsset<ScribanAsset>(TestTreeTemplatePath, out _);
            testDynamicScaffold = TemplTestUtility
                .CreateTestAsset<TemplScaffold>(TestDynamicScaffoldPath, out _);
            testEmptyTreeTemplate = TemplTestUtility
                .CreateTestAsset<ScribanAsset>(TestEmptyTreeTemplatePath, out _);
            testDynamicScaffoldWithEmptyTemplate = TemplTestUtility
                .CreateTestAsset<TemplScaffold>(TestDynamicScaffoldWithEmptyTemplatePath, out _);
            testEmptyDirectoryScaffold = TemplTestUtility
                .CreateTestAsset<TemplScaffold>(TestEmptyDirectoryScaffoldPath, out _);
            testNullTemplateScaffold = TemplTestUtility
                .CreateTestAsset<TemplScaffold>(TestNullTemplateScaffoldPath, out _);

            testSelection = testScaffold;
            expectedDirectoryPath = $"{TestTargetPath}/NewDirectory{InputName}";
            expectedFilePath = $"{expectedDirectoryPath}/NewFile{testScaffold.name}";
            expectedContent = $"Hello {InputName} {testScaffold.name}: {expectedFilePath}";
        }

        [SetUp]
        public void BeforeEach()
        {
            subject = new TemplScaffoldCore(
                fileSystemMock = new FileSystemMock(),
                loggerMock = new LoggerMock(),
                editorUtilityMock = new EditorUtilityMock(),
                templateFunctionProviderMock = new TemplateFunctionProviderMock());

            testInput = new InputType()
            {
                name = InputName,
                elements = Elements
            };
        }

        [OneTimeTearDown]
        public void AfterAll()
        {
            TemplTestUtility.DeleteTestAsset(testScaffoldTemplate);
            TemplTestUtility.DeleteTestAsset(testScaffold);
            TemplTestUtility.DeleteTestAsset(testTreeTemplate);
            TemplTestUtility.DeleteTestAsset(testDynamicScaffold);
            TemplTestUtility.DeleteTestAsset(testEmptyTreeTemplate);
            TemplTestUtility.DeleteTestAsset(testDynamicScaffoldWithEmptyTemplate);
            TemplTestUtility.DeleteTestAsset(testEmptyDirectoryScaffold);
            TemplTestUtility.DeleteTestAsset(testNullTemplateScaffold);
        }

        [Test]
        public void GivenValidScaffold_WhenValidating_ThenShouldNotReturnErrors()
        {
            // Act
            var errors = subject.ValidateScaffoldGeneration(testScaffold, TestTargetPath,
                testInput, testSelection);

            // Verify
            Assert.IsEmpty(errors, "Unexpected errors");
        }

        [Test]
        public void GivenExistingFiles_WhenValidating_ThenShouldReturnOverwriteErrors()
        {
            // Setup
            fileSystemMock.FileExists.Add(expectedFilePath);

            // Act
            var errors = subject.ValidateScaffoldGeneration(testScaffold, TestTargetPath,
                testInput, testSelection);

            // Verify
            Assert.IsNotEmpty(errors, "Errors expected");
            Assert.AreEqual(TemplScaffoldErrorType.Overwrite, errors[0].Type, "Wrong error type");
            Assert.That(errors[0].Message == expectedFilePath, "Wrong error");
        }

        [Test]
        public void GivenEmptyScaffold_WhenValidating_ThenShouldReturnErrors()
        {
            // Setup
            var emptyScaffold = ScriptableObject.CreateInstance<TemplScaffold>();

            // Act
            var errors = subject.ValidateScaffoldGeneration(emptyScaffold, TestTargetPath,
                testInput, testSelection);

            // Verify
            Assert.IsNotEmpty(errors, "Errors expected");
            Assert.AreEqual(TemplScaffoldErrorType.Undefined, errors[0].Type, "Wrong error type");
            Assert.That(errors[0].Message.Contains("Found empty tree"), "Wrong error");
        }

        [Test]
        public void GivenValidDynamicScaffold_WhenValidating_ThenShouldNotReturnErrors()
        {
            // Act
            var errors = subject.ValidateScaffoldGeneration(testDynamicScaffold, TestTargetPath,
                testInput, testSelection);

            // Verify
            Assert.IsEmpty(errors, "Unexpected errors");
        }

        [Test]
        public void GivenDynamicScaffoldWithNoTemplate_WhenValidating_ThenShouldReturnErrors()
        {
            // Setup
            var emptyScaffold = ScriptableObject.CreateInstance<TemplDynamicScaffold>();

            // Act
            var errors = subject.ValidateScaffoldGeneration(emptyScaffold, TestTargetPath,
                testInput, testSelection);

            // Verify
            Assert.IsNotEmpty(errors, "Errors expected");
            Assert.AreEqual(TemplScaffoldErrorType.Template, errors[0].Type, "Wrong error type");
            Assert.That(errors[0].Message.Contains("Null or invalid tree template"), "Wrong error");
        }

        [Test]
        public void GivenDynamicScaffoldWithEmptyTemplate_WhenValidating_ThenShouldReturnErrors()
        {
            // Act
            var errors = subject.ValidateScaffoldGeneration(testDynamicScaffoldWithEmptyTemplate,
                TestTargetPath, testInput, testSelection);

            // Verify
            Assert.IsNotEmpty(errors, "Errors expected");
            Assert.AreEqual(TemplScaffoldErrorType.Template, errors[0].Type, "Wrong error type");
            Assert.That(errors[0].Message.Contains("Empty tree template"), "Wrong error");
        }

        [Test]
        public void GivenDynamicScaffoldWithErrors_WhenValidating_ThenShouldReturnErrors()
        {
            // Setup
            testInput.induce_runtime_error = true;

            // Act
            var errors = subject.ValidateScaffoldGeneration(testDynamicScaffold,
                TestTargetPath, testInput, testSelection);

            // Verify
            Assert.IsNotEmpty(errors, "Errors expected");
            Assert.AreEqual(TemplScaffoldErrorType.Template, errors[0].Type, "Wrong error type");
            Assert.That(errors[0].Message.Contains("Error parsing tree"), "Wrong error");
        }

        [Test]
        public void GivenDynamicScaffoldWithDuplicateNodes_WhenValidating_ThenShouldReturnErrors()
        {
            // Setup
            testInput.induce_duplicate_error = true;

            // Act
            var errors = subject.ValidateScaffoldGeneration(testDynamicScaffold,
                TestTargetPath, testInput, testSelection);

            // Verify
            Assert.IsNotEmpty(errors, "Errors expected");
            Assert.AreEqual(TemplScaffoldErrorType.Filename, errors[0].Type, "Wrong error type");
            Assert.That(errors[0].Message.Contains("Different sister node"), "Wrong error");
        }

        [Test]
        public void GivenDynamicScaffoldWithBrokenReference_WhenValidating_ThenShouldReturnErrors()
        {
            // Setup
            testInput.induce_broken_reference_error = true;

            // Act
            var errors = subject.ValidateScaffoldGeneration(testDynamicScaffold,
                TestTargetPath, testInput, testSelection);

            // Verify
            Assert.IsNotEmpty(errors, "Errors expected");
            Assert.AreEqual(TemplScaffoldErrorType.Template, errors[0].Type, "Wrong error type");
            Assert.That(errors[0].Message.Contains("Could not find template"), "Wrong error");
        }

        [Test]
        public void GivenScaffoldWithEmptyDirectory_WhenValidating_ThenShouldReturnErrors()
        {
            // Act
            var errors = subject.ValidateScaffoldGeneration(testEmptyDirectoryScaffold,
                TestTargetPath, testInput, testSelection);

            // Verify
            Assert.IsNotEmpty(errors, "Errors expected");
            Assert.AreEqual(TemplScaffoldErrorType.Undefined, errors[0].Type, "Wrong error type");
            Assert.That(errors[0].Message.Contains("Empty directory node"), "Wrong error");
        }

        [Test]
        public void GivenDynamicScaffoldWithReservedNames_WhenValidating_ThenShouldReturnErrors()
        {
            // Setup
            testInput.induce_context_error = true;

            // Act
            var errors = subject.ValidateScaffoldGeneration(testDynamicScaffold,
                TestTargetPath, testInput, testSelection);

            // Verify
            Assert.IsNotEmpty(errors, "Errors expected");
            Assert.AreEqual(TemplScaffoldErrorType.Undefined, errors[0].Type, "Wrong error type");
            Assert.That(errors[0].Message.Contains("Error preparing context"), "Wrong error");
        }

        [Test]
        public void GivenScaffoldWithEmptyFilename_WhenValidating_ThenShouldReturnErrors()
        {
            // Setup
            testInput.induce_empty_filename_error = true;

            // Act
            var errors = subject.ValidateScaffoldGeneration(testDynamicScaffold,
                TestTargetPath, testInput, testSelection);

            // Verify
            Assert.IsNotEmpty(errors, "Errors expected");
            Assert.AreEqual(TemplScaffoldErrorType.Filename, errors[0].Type, "Wrong error type");
            Assert.That(errors[0].Message.Contains("Empty Filename found for"), "Wrong error");
        }

        [Test]
        public void GivenScaffoldWithEmptyNodeName_WhenValidating_ThenShouldReturnErrors()
        {
            // Setup
            testInput.induce_empty_nodename_error = true;

            // Act
            var errors = subject.ValidateScaffoldGeneration(testDynamicScaffold,
                TestTargetPath, testInput, testSelection);

            // Verify
            Assert.IsNotEmpty(errors, "Errors expected");
            Assert.AreEqual(TemplScaffoldErrorType.Filename, errors[0].Type, "Wrong error type");
            Assert.That(errors[0].Message.Contains("Empty Filename found in"), "Wrong error");
        }

        [Test]
        public void GivenScaffoldWithInvalidFilename_WhenValidating_ThenShouldReturnErrors()
        {
            // Setup
            testInput.induce_invalid_filename_error = true;

            // Act
            var errors = subject.ValidateScaffoldGeneration(testDynamicScaffold,
                TestTargetPath, testInput, testSelection);

            // Verify
            Assert.IsNotEmpty(errors, "Errors expected");
            Assert.AreEqual(TemplScaffoldErrorType.Filename, errors[0].Type, "Wrong error type");
            Assert.That(errors[0].Message.Contains("Invalid characters found"), "Wrong error");
        }

        [Test]
        public void GivenScaffoldWithParseErrors_WhenValidating_ThenShouldReturnErrors()
        {
            // Setup
            testInput.induce_parse_error = true;

            // Act
            var errors = subject.ValidateScaffoldGeneration(testDynamicScaffold,
                TestTargetPath, testInput, testSelection);

            // Verify
            Assert.IsNotEmpty(errors, "Errors expected");
            Assert.AreEqual(TemplScaffoldErrorType.Filename, errors[0].Type, "Wrong error type");
            Assert.That(errors[0].Message.Contains("Error rendering Filename"), "Wrong error");
        }

        [Test]
        public void GivenScaffoldWithNullTemplate_WhenValidating_ThenShouldReturnErrors()
        {
            // Act
            var errors = subject.ValidateScaffoldGeneration(testNullTemplateScaffold,
                TestTargetPath, testInput, testSelection);

            // Verify
            Assert.IsNotEmpty(errors, "Errors expected");
            Assert.AreEqual(TemplScaffoldErrorType.Template, errors[0].Type, "Wrong error type");
            Assert.That(errors[0].Message.Contains("Null or invalid template"), "Wrong error");
        }

        [Test]
        public void GivenValidScaffold_WhenGenerating_ThenShouldReturnCorrectPaths()
        {
            // Act
            var paths = subject.GenerateScaffold(testScaffold,
                TestTargetPath, testInput, testSelection);

            // Verify
            Assert.IsNotEmpty(paths, "Paths expected");
            Assert.AreEqual(expectedDirectoryPath, paths[0], "Wrong path");
            Assert.AreEqual(expectedFilePath, paths[1], "Wrong path");
        }

        [Test]
        public void GivenValidScaffold_WhenGenerating_ThenShouldCreateCorrectDirectories()
        {
            // Act
            subject.GenerateScaffold(testScaffold,
                TestTargetPath, testInput, testSelection);

            // Verify
            Assert.AreEqual(1, fileSystemMock.CreateDirectoryCount,
                "Unexpected number of created directories");
            Assert.AreEqual(expectedDirectoryPath, fileSystemMock.DirectoryPath[0],
                "Wrong directory created");
        }

        [Test]
        public void GivenValidScaffold_WhenGenerating_ThenShouldCreateCorrectFiles()
        {
            // Act
            subject.GenerateScaffold(testScaffold,
                TestTargetPath, testInput, testSelection);

            // Verify
            Assert.AreEqual(1, fileSystemMock.WriteAllTextCount,
                "Unexpected number of created files");
            Assert.AreEqual(expectedFilePath, fileSystemMock.Path[0], "Wrong file created");
            Assert.AreEqual(expectedContent, fileSystemMock.Contents[0], "Wrong file contents");
        }

        [Test]
        public void GivenIOError_WhenGenerating_ThenShouldLogError()
        {
            // Setup
            fileSystemMock.ThrowException = true;

            // Act
            subject.GenerateScaffold(testScaffold,
                TestTargetPath, testInput, testSelection);

            // Verify
            Assert.AreEqual(1, loggerMock.ErrorCount, "Unexpected number of errors");
        }

        [Test]
        public void GivenScaffoldValidationErrors_WhenGenerating_ThenShouldLogErrors()
        {
            // Act
            subject.GenerateScaffold(testEmptyDirectoryScaffold,
                TestTargetPath, testInput, testSelection);

            // Verify
            Assert.AreEqual(2, loggerMock.ErrorCount, "Unexpected number of errors");
        }

        [Test]
        public void GivenSkipPaths_WhenGenerating_ThenShouldNotCreateFiles()
        {
            // Act
            subject.GenerateScaffold(testEmptyDirectoryScaffold,
                TestTargetPath, testInput, testSelection, new[] { expectedFilePath });

            // Verify
            Assert.AreEqual(0, fileSystemMock.WriteAllTextCount, "Should not create file");
        }

        [Test]
        public void GivenValidScaffold_WhenValidating_ThenShouldShowProgress()
        {
            // Act
            subject.ValidateScaffoldGeneration(testScaffold, TestTargetPath,
                testInput, testSelection);

            // Verify
            Assert.AreEqual(3, editorUtilityMock.DisplayProgressBarCount,
                "Unexpected number of calls to display progress bar");
            Assert.AreEqual(1, editorUtilityMock.ClearProgressBarCount,
                "Unexpected number of calls to clear progress bar");
        }

        [Test]
        public void GivenValidScaffold_WhenGenerating_ThenShouldShowProgress()
        {
            // Act
            subject.GenerateScaffold(testScaffold,
                TestTargetPath, testInput, testSelection);

            // Verify
            Assert.AreEqual(6, editorUtilityMock.DisplayProgressBarCount,
                "Unexpected number of calls to display progress bar");
            Assert.AreEqual(2, editorUtilityMock.ClearProgressBarCount,
                "Unexpected number of calls to clear progress bar");
        }

        internal struct InputType
        {
            public string name;
            public string[] elements;
            public bool induce_runtime_error;
            public bool induce_duplicate_error;
            public bool induce_broken_reference_error;
            public bool induce_context_error;
            public bool induce_empty_filename_error;
            public bool induce_empty_nodename_error;
            public bool induce_invalid_filename_error;
            public bool induce_parse_error;
        }
    }
}
