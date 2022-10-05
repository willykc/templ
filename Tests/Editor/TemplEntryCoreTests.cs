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
using System.IO;
using UnityEditor;

namespace Willykc.Templ.Editor.Tests
{
    using Mocks;

    internal class TemplEntryCoreTests
    {
        internal const string TestSettingsPath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets/TestTemplSettings.asset";
        internal const string TestTextPath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets/TestText.txt";
        internal const string TestTemplatePath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets/TestTemplate.sbn";
        internal const string TestErrorTemplatePath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets/TestErrorTemplate.sbn";
        internal const string TestOutputPathTemplatePath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets/TestOutputPathTemplate.sbn";
        internal const string TestPathFunctionTemplatePath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets/TestPathFunctionTemplate.sbn";
        private const string ExpectedOutput = "Hello world!";

        private TemplEntryCore subject;
        private AssetDatabaseMock assetDatabaseMock;
        private FileSystemMock fileMock;
        private SessionStateMock sessionStateMock;
        private LoggerMock loggerMock;
        private SettingsProviderMock settingsProviderMock;
        private Type[] typeCache;
        private TemplSettings settings;
        private AssetsPaths changes;
        private EntryMock firstEntryMock;
        private EntryMock secondEntryMock;
        private ScribanAsset testErrorTemplate;
        private ScribanAsset testOutputPathTemplate;
        private ScribanAsset testPathFunctionTemplate;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            typeCache = new Type[0];
            settings = AssetDatabase.LoadAssetAtPath<TemplSettings>(TestSettingsPath);
            testErrorTemplate = AssetDatabase.LoadAssetAtPath<ScribanAsset>(TestErrorTemplatePath);
            testOutputPathTemplate =
                AssetDatabase.LoadAssetAtPath<ScribanAsset>(TestOutputPathTemplatePath);
            testPathFunctionTemplate =
                AssetDatabase.LoadAssetAtPath<ScribanAsset>(TestPathFunctionTemplatePath);
            firstEntryMock = settings.Entries[0] as EntryMock;
            secondEntryMock = settings.Entries[1] as EntryMock;
        }

        [SetUp]
        public void BeforeEach()
        {
            changes = new AssetsPaths(
                new[] { string.Empty },
                new string[0],
                new string[0],
                new string[0]);

            subject = new TemplEntryCore(
                assetDatabaseMock = new AssetDatabaseMock(),
                fileMock = new FileSystemMock(),
                sessionStateMock = new SessionStateMock(),
                loggerMock = new LoggerMock(),
                settingsProviderMock = new SettingsProviderMock(),
                typeCache);

            settingsProviderMock.settingsExist = true;
            settingsProviderMock.settings = settings;
        }

        [TearDown]
        public void AfterEach()
        {
            firstEntryMock.Clear();
            secondEntryMock.Clear();
        }

        [Test]
        public void GivenTemplateChange_WhenAssetsChange_ThenEntryShouldRender()
        {
            // Setup
            firstEntryMock.templateChanged = true;

            // Act
            subject.OnAssetsChanged(changes);

            // Verify
            Assert.AreEqual(1, fileMock.WriteAllTextCount, "Did not render");
            Assert.AreEqual(ExpectedOutput, fileMock.Contents[0], "Wrong render result");
        }

        [Test]
        public void GivenInputChange_WhenAssetsChange_ThenEntryShouldRender()
        {
            // Setup
            firstEntryMock.inputChanged = true;

            // Act
            subject.OnAssetsChanged(changes);

            // Verify
            Assert.AreEqual(1, fileMock.WriteAllTextCount, "Did not render");
            Assert.AreEqual(ExpectedOutput, fileMock.Contents[0], "Wrong render result");
        }

        [Test]
        public void GivenInputChange_WhenAssetsChange_ThenShouldLogInfo()
        {
            // Setup
            firstEntryMock.inputChanged = true;

            // Act
            subject.OnAssetsChanged(changes);

            // Verify
            Assert.AreEqual(1, loggerMock.InfoCount, "Did not log info");
        }

        [Test]
        public void GivenInputChangeWithDeferredEntry_WhenAssetsChange_ThenEntryShouldNotRender()
        {
            // Setup
            firstEntryMock.inputChanged = true;
            firstEntryMock.defer = true;

            // Act
            subject.OnAssetsChanged(changes);

            // Verify
            Assert.AreEqual(0, fileMock.WriteAllTextCount, "Unexpected render");
        }

        [Test]
        public void GivenInputChangeWithDeferredEntry_WhenAssetsChange_ThenShouldFlagEntry()
        {
            // Setup
            firstEntryMock.inputChanged = true;
            firstEntryMock.defer = true;

            // Act
            subject.OnAssetsChanged(changes);

            // Verify
            Assert.AreEqual(firstEntryMock.guid, sessionStateMock.SetValue, "Did not flag entry");
        }

        [Test]
        public void GivenTemplateChangeWithDeferredEntry_WhenAssetsChange_ThenEntryShouldRender()
        {
            // Setup
            firstEntryMock.templateChanged = true;
            firstEntryMock.defer = true;

            // Act
            subject.OnAssetsChanged(changes);

            // Verify
            Assert.AreEqual(1, fileMock.WriteAllTextCount, "Did not render");
            Assert.AreEqual(ExpectedOutput, fileMock.Contents[0], "Wrong render result");
        }

        [Test]
        public void GivenTemplateChangeWithDeferredEntry_WhenAssetsChange_ThenShouldNotFlagEntry()
        {
            // Setup
            firstEntryMock.templateChanged = true;
            firstEntryMock.defer = true;

            // Act
            subject.OnAssetsChanged(changes);

            // Verify
            Assert.AreNotEqual(firstEntryMock.guid, sessionStateMock.SetValue, "Unexpected flag");
        }

        [Test]
        public void GivenSettingsChangeWithFlag_WhenAssetsChange_ThenEntryShouldRender()
        {
            // Setup
            assetDatabaseMock.mockSettingsPath = TestSettingsPath;
            changes.importedAssets = new string[] { TestSettingsPath };
            sessionStateMock.value = firstEntryMock.guid;

            // Act
            subject.OnAssetsChanged(changes);

            // Verify
            Assert.AreEqual(1, fileMock.WriteAllTextCount, "Did not render");
            Assert.AreEqual(ExpectedOutput, fileMock.Contents[0], "Wrong render result");
        }

        [Test]
        public void GivenSettingsChangeWithNoFlag_WhenAssetsChange_ThenEntryShouldNotRender()
        {
            // Setup
            assetDatabaseMock.mockSettingsPath = TestSettingsPath;
            changes.importedAssets = new string[] { TestSettingsPath };

            // Act
            subject.OnAssetsChanged(changes);

            // Verify
            Assert.AreEqual(0, fileMock.WriteAllTextCount, "Unexpected render");
        }

        [Test]
        public void GivenNoEntryFlagged_WhenAssemblyReloads_ThenEntryShouldNotRender()
        {
            // Act
            subject.OnAfterAssemblyReload();

            // Verify
            Assert.AreEqual(0, fileMock.WriteAllTextCount, "Unexpected render");
        }

        [Test]
        public void GivenEntryFlagged_WhenAssemblyReloads_ThenEntryShouldRender()
        {
            // Setup
            sessionStateMock.value = firstEntryMock.guid;

            // Act
            subject.OnAfterAssemblyReload();

            // Verify
            Assert.AreEqual(1, fileMock.WriteAllTextCount, "Did not render");
            Assert.AreEqual(ExpectedOutput, fileMock.Contents[0], "Wrong render result");
        }

        [Test]
        public void GivenEntryFlagged_WhenAssemblyReloads_ThenFlagsShouldClear()
        {
            // Setup
            sessionStateMock.value = firstEntryMock.guid;

            // Act
            subject.OnAfterAssemblyReload();

            // Verify
            Assert.IsNotEmpty(sessionStateMock.EraseKey, "Did not clear flags");
        }

        [Test]
        public void GivenNoDeleteFlagEntry_WhenAssetDeleted_ThenShouldNotFlagEntry()
        {
            // Act
            subject.OnWillDeleteAsset(string.Empty);

            // Verify
            Assert.IsNull(sessionStateMock.SetValue, "Unexpected flag");
        }

        [Test]
        public void GivenDeleteFlagEntry_WhenAssetDeleted_ThenShouldFlagEntry()
        {
            // Setup
            secondEntryMock.inputChanged = true;

            // Act
            subject.OnWillDeleteAsset(string.Empty);

            // Verify
            Assert.AreEqual(secondEntryMock.guid, sessionStateMock.SetValue, "Did not flag entry");
        }

        [Test]
        public void GivenNullEntry_WhenFlaggingChange_ThenShouldNotFlagEntry()
        {
            // Setup
            sessionStateMock.value = string.Empty;

            // Act
            subject.FlagChangedEntry(null);

            // Verify
            Assert.IsNull(sessionStateMock.SetValue, "Unexpected flag");
        }

        [Test]
        public void GivenEntry_WhenFlaggingChange_ThenShouldFlagEntry()
        {
            // Setup
            sessionStateMock.value = string.Empty;

            // Act
            subject.FlagChangedEntry(secondEntryMock);

            // Verify
            Assert.AreEqual(secondEntryMock.guid, sessionStateMock.SetValue, "Did not flag entry");
        }

        [Test]
        public void GivenFlaggedEntry_WhenFlaggingChangeTwice_ThenShouldFlagEntryOnlyOnce()
        {
            // Setup
            sessionStateMock.value = secondEntryMock.guid;

            // Act
            subject.FlagChangedEntry(secondEntryMock);

            // Verify
            Assert.AreNotEqual(secondEntryMock.guid, sessionStateMock.SetValue,
                "Flagged change twice");
        }

        [Test]
        public void GivenDifferentFlaggedEntry_WhenFlaggingChanges_ThenShouldFlagBothEntries()
        {
            // Setup
            sessionStateMock.value = firstEntryMock.guid;

            // Act
            subject.FlagChangedEntry(secondEntryMock);

            // Verify
            Assert.IsTrue(sessionStateMock.SetValue.Contains(firstEntryMock.guid),
                "Did not flag first entry");
            Assert.IsTrue(sessionStateMock.SetValue.Contains(secondEntryMock.guid),
                "Did not flag second entry");
        }

        [Test]
        public void GivenValidEntries_WhenRenderAllValidEntries_ThenShouldRenderAllEntries()
        {
            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(2, fileMock.WriteAllTextCount);
        }

        [Test]
        public void GivenInvalidEntry_WhenRenderAllValidEntries_ThenShouldNotRenderEntry()
        {
            // Setup
            firstEntryMock.valid = false;

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(1, fileMock.WriteAllTextCount);
        }

        [Test]
        public void GivenInvalidEntry_WhenRenderAllValidEntries_ThenLogWarning()
        {
            // Setup
            firstEntryMock.valid = false;

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(1, loggerMock.WarnCount);
        }

        [Test]
        public void GivenOverwritingSettings_WhenRenderAllValidEntries_ThenShouldNotRenderEntry()
        {
            // Setup
            firstEntryMock.outputAssetPath = TestSettingsPath;
            assetDatabaseMock.mockSettingsPath = TestSettingsPath;

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(1, fileMock.WriteAllTextCount);
        }

        [Test]
        public void GivenOverwritingSettings_WhenRenderAllValidEntries_ThenShouldLogError()
        {
            // Setup
            firstEntryMock.outputAssetPath = TestSettingsPath;
            assetDatabaseMock.mockSettingsPath = TestSettingsPath;

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(1, loggerMock.ErrorCount);
        }

        [Test]
        public void GivenOverwritingInput_WhenRenderAllValidEntries_ThenShouldNotRenderEntry()
        {
            // Setup
            firstEntryMock.outputAssetPath = TestTextPath;
            assetDatabaseMock.mockInputPath = TestTextPath;

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(1, fileMock.WriteAllTextCount);
        }

        [Test]
        public void GivenOverwritingInput_WhenRenderAllValidEntries_ThenShouldLogError()
        {
            // Setup
            firstEntryMock.outputAssetPath = TestTextPath;
            assetDatabaseMock.mockInputPath = TestTextPath;

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(1, loggerMock.ErrorCount);
        }

        [Test]
        public void GivenOverwritingTemplate_WhenRenderAllValidEntries_ThenShouldNotRenderEntry()
        {
            // Setup
            firstEntryMock.outputAssetPath = TestTemplatePath;
            assetDatabaseMock.mockTemplatePath = TestTemplatePath;

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(1, fileMock.WriteAllTextCount);
        }

        [Test]
        public void GivenOverwritingTemplate_WhenRenderAllValidEntries_ThenShouldLogError()
        {
            // Setup
            firstEntryMock.outputAssetPath = TestTemplatePath;
            assetDatabaseMock.mockTemplatePath = TestTemplatePath;

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(1, loggerMock.ErrorCount);
        }

        [Test]
        public void GivenTemplateWithError_WhenRenderAllValidEntries_ThenShouldLogError()
        {
            // Setup
            var originalTemplate = firstEntryMock.template;
            firstEntryMock.template = testErrorTemplate;

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(1, loggerMock.ErrorCount);

            // Cleanup
            firstEntryMock.template = originalTemplate;
        }

        [Test]
        public void GivenTemplateWithOutputPath_WhenRenderAllValidEntries_ThenShouldRenderPath()
        {
            // Setup
            var originalTemplate = firstEntryMock.template;
            firstEntryMock.template = testOutputPathTemplate;

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(firstEntryMock.OutputAssetPath, fileMock.Contents[0]);

            // Cleanup
            firstEntryMock.template = originalTemplate;
        }

        [Test]
        public void GivenTemplateWithPathFunction_WhenRenderAllValidEntries_ThenShouldRenderResult()
        {
            // Setup
            var originalTemplate = firstEntryMock.template;
            firstEntryMock.template = testPathFunctionTemplate;

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(Path.GetFileNameWithoutExtension(firstEntryMock.OutputAssetPath),
                fileMock.Contents[0]);

            // Cleanup
            firstEntryMock.template = originalTemplate;
        }
    }
}
