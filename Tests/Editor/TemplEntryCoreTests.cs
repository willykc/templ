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
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Willykc.Templ.Editor.Tests
{
    using Entry;
    using Mocks;

    internal class TemplEntryCoreTests
    {
        internal const string TestSettingsPath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets~/TestTemplSettings.asset";
        internal const string TestEdgeCasesSettingsPath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets~/TestEdgeCasesTemplSettings.asset";
        internal const string TestTextPath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets~/TestText.txt";
        internal const string TestTemplatePath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets~/TestTemplate.sbn";
        internal const string TestErrorTemplatePath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets~/TestErrorTemplate.sbn";
        internal const string TestOutputPathTemplatePath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets~/TestOutputPathTemplate.sbn";
        internal const string TestPathFunctionTemplatePath =
            "Packages/com.willykc.templ/Tests/Editor/TestAssets~/TestPathFunctionTemplate.sbn";
        private const string ExpectedOutput = "Hello world!";

        private ITemplEntryCore subject;
        private AssetDatabaseMock assetDatabaseMock;
        private FileSystemMock fileMock;
        private SessionStateMock sessionStateMock;
        private LoggerMock loggerMock;
        private SettingsProviderMock settingsProviderMock;
        private TemplateFunctionProviderMock templateFunctionProviderMock;
        private EditorUtilityMock editorUtilityMock;
        private TemplSettings settings;
        private TemplSettings edgeCasesSettings;
        private AssetsPaths changes;
        private EntryMock firstEntryMock;
        private EntryMock secondEntryMock;
        private ScribanAsset testErrorTemplate;
        private ScribanAsset testOutputPathTemplate;
        private ScribanAsset testPathFunctionTemplate;
        private ScribanAsset testTemplate;
        private TextAsset testText;
        private string testTemplatePath;
        private string testTextPath;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            settings = TemplTestUtility.CreateTestAsset<TemplSettings>(TestSettingsPath, out _);
            edgeCasesSettings =
                TemplTestUtility.CreateTestAsset<TemplSettings>(TestEdgeCasesSettingsPath, out _);
            testErrorTemplate =
                TemplTestUtility.CreateTestAsset<ScribanAsset>(TestErrorTemplatePath, out _);
            testOutputPathTemplate =
                TemplTestUtility.CreateTestAsset<ScribanAsset>(TestOutputPathTemplatePath, out _);
            testPathFunctionTemplate =
                TemplTestUtility.CreateTestAsset<ScribanAsset>(TestPathFunctionTemplatePath, out _);
            testTemplate =
                TemplTestUtility.CreateTestAsset<ScribanAsset>(TestTemplatePath,
                out testTemplatePath);
            testText =
                TemplTestUtility.CreateTestAsset<TextAsset>(TestTextPath, out testTextPath);
            firstEntryMock = settings.Entries[0] as EntryMock;
            secondEntryMock = settings.Entries[1] as EntryMock;
        }

        [SetUp]
        public void BeforeEach()
        {
            changes = new AssetsPaths(
                new[] { string.Empty },
                new[] { string.Empty },
                new[] { string.Empty },
                new[] { string.Empty });

            subject = new TemplEntryCore(
                assetDatabaseMock = new AssetDatabaseMock(),
                fileMock = new FileSystemMock(),
                sessionStateMock = new SessionStateMock(),
                loggerMock = new LoggerMock(),
                settingsProviderMock = new SettingsProviderMock(),
                templateFunctionProviderMock = new TemplateFunctionProviderMock(),
                editorUtilityMock = new EditorUtilityMock());

            settingsProviderMock.settingsExist = true;
            settingsProviderMock.settings = settings;
        }

        [TearDown]
        public void AfterEach()
        {
            firstEntryMock.Clear();
            secondEntryMock.Clear();
        }

        [OneTimeTearDown]
        public void AfterAll()
        {
            TemplTestUtility.DeleteTestAsset(settings);
            TemplTestUtility.DeleteTestAsset(edgeCasesSettings);
            TemplTestUtility.DeleteTestAsset(testErrorTemplate);
            TemplTestUtility.DeleteTestAsset(testOutputPathTemplate);
            TemplTestUtility.DeleteTestAsset(testPathFunctionTemplate);
            TemplTestUtility.DeleteTestAsset(testTemplate);
            TemplTestUtility.DeleteTestAsset(testText);
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
            Assert.AreEqual(firstEntryMock.Id, sessionStateMock.SetValue, "Did not flag entry");
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
            Assert.AreNotEqual(firstEntryMock.Id, sessionStateMock.SetValue, "Unexpected flag");
        }

        [Test]
        public void GivenSettingsChangeWithFlag_WhenAssetsChange_ThenEntryShouldRender()
        {
            // Setup
            assetDatabaseMock.mockSettingsPath = TestSettingsPath;
            changes.importedAssets = new string[] { TestSettingsPath };
            sessionStateMock.value = firstEntryMock.Id;

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
            sessionStateMock.value = firstEntryMock.Id;

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
            sessionStateMock.value = firstEntryMock.Id;

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
            Assert.AreEqual(secondEntryMock.Id, sessionStateMock.SetValue, "Did not flag entry");
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
            Assert.AreEqual(secondEntryMock.Id, sessionStateMock.SetValue, "Did not flag entry");
        }

        [Test]
        public void GivenFlaggedEntry_WhenFlaggingChangeTwice_ThenShouldFlagEntryOnlyOnce()
        {
            // Setup
            sessionStateMock.value = secondEntryMock.Id;

            // Act
            subject.FlagChangedEntry(secondEntryMock);

            // Verify
            Assert.AreNotEqual(secondEntryMock.Id, sessionStateMock.SetValue,
                "Flagged change twice");
        }

        [Test]
        public void GivenDifferentFlaggedEntry_WhenFlaggingChanges_ThenShouldFlagBothEntries()
        {
            // Setup
            sessionStateMock.value = firstEntryMock.Id;

            // Act
            subject.FlagChangedEntry(secondEntryMock);

            // Verify
            Assert.IsTrue(sessionStateMock.SetValue.Contains(firstEntryMock.Id),
                "Did not flag first entry");
            Assert.IsTrue(sessionStateMock.SetValue.Contains(secondEntryMock.Id),
                "Did not flag second entry");
        }

        [Test]
        public void GivenValidEntries_WhenRenderAllValidEntries_ThenShouldRenderAllEntries()
        {
            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(2, fileMock.WriteAllTextCount, "Unexpected number of entries rendered");
        }

        [Test]
        public void GivenInvalidEntry_WhenRenderAllValidEntries_ThenShouldNotRenderEntry()
        {
            // Setup
            firstEntryMock.valid = false;

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(1, fileMock.WriteAllTextCount, "Unexpected number of entries rendered");
        }

        [Test]
        public void GivenInvalidEntry_WhenRenderAllValidEntries_ThenLogWarning()
        {
            // Setup
            firstEntryMock.valid = false;

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(1, loggerMock.WarnCount, "Unexpected number of warnings logged");
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
            Assert.AreEqual(1, fileMock.WriteAllTextCount, "Unexpected number of entries rendered");
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
            Assert.AreEqual(1, loggerMock.ErrorCount, "Unexpected number of errors logged");
        }

        [Test]
        public void GivenOverwritingInput_WhenRenderAllValidEntries_ThenShouldNotRenderEntry()
        {
            // Setup
            firstEntryMock.outputAssetPath = testTextPath;
            assetDatabaseMock.mockInputPath = testTextPath;

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(1, fileMock.WriteAllTextCount, "Unexpected number of entries rendered");
        }

        [Test]
        public void GivenOverwritingInput_WhenRenderAllValidEntries_ThenShouldLogError()
        {
            // Setup
            firstEntryMock.outputAssetPath = testTextPath;
            assetDatabaseMock.mockInputPath = testTextPath;

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(1, loggerMock.ErrorCount, "Unexpected number of errors logged");
        }

        [Test]
        public void GivenOverwritingTemplate_WhenRenderAllValidEntries_ThenShouldNotRenderEntry()
        {
            // Setup
            firstEntryMock.outputAssetPath = testTemplatePath;
            assetDatabaseMock.mockTemplatePath = testTemplatePath;

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(1, fileMock.WriteAllTextCount, "Unexpected number of entries rendered");
        }

        [Test]
        public void GivenOverwritingTemplate_WhenRenderAllValidEntries_ThenShouldLogError()
        {
            // Setup
            firstEntryMock.outputAssetPath = testTemplatePath;
            assetDatabaseMock.mockTemplatePath = testTemplatePath;

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(1, loggerMock.ErrorCount, "Unexpected number of errors logged");
        }

        [Test]
        public void GivenTemplateWithError_WhenRenderAllValidEntries_ThenShouldLogError()
        {
            // Setup
            settingsProviderMock.settings = edgeCasesSettings;

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(1, loggerMock.ErrorCount, "Unexpected number of errors logged");
        }

        [Test]
        public void GivenTemplateWithOutputPath_WhenRenderAllValidEntries_ThenShouldRenderPath()
        {
            // Setup
            settingsProviderMock.settings = edgeCasesSettings;
            var targetEntry = edgeCasesSettings.Entries[1];

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(targetEntry.OutputAssetPath, fileMock.Contents[0],
                "Unexpected content rendered");
        }

        [Test]
        public void GivenTemplateWithPathFunction_WhenRenderAllValidEntries_ThenShouldRenderResult()
        {
            // Setup
            settingsProviderMock.settings = edgeCasesSettings;
            var targetEntry = edgeCasesSettings.Entries[2];

            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(Path.GetFileNameWithoutExtension(targetEntry.OutputAssetPath),
                fileMock.Contents[1], "Unexpected content rendered");
        }

        [Test]
        public void GivenInputChange_WhenAssetsChange_ThenProgressBarShouldBeDisplayed()
        {
            // Setup
            firstEntryMock.inputChanged = true;
            secondEntryMock.inputChanged = true;

            // Act
            subject.OnAssetsChanged(changes);

            // Verify
            Assert.AreEqual(2, editorUtilityMock.DisplayProgressBarCount,
                "Did not display progress bar");
        }

        [Test]
        public void GivenInputChange_WhenAssetsChange_ThenProgressBarShouldBeCleared()
        {
            // Setup
            firstEntryMock.inputChanged = true;
            secondEntryMock.inputChanged = true;

            // Act
            subject.OnAssetsChanged(changes);

            // Verify
            Assert.AreEqual(1, editorUtilityMock.ClearProgressBarCount,
                "Did not clear progress bar");
        }

        [Test]
        public void GivenReferencedInput_WhenInputDeleted_ThenShowNotAllowDelete()
        {
            // Setup
            assetDatabaseMock.mockInputPath = testTextPath;

            // Act
            var deleteAllowed = subject.OnWillDeleteAsset(testTextPath);

            // Verify
            Assert.IsFalse(deleteAllowed, "Should not allow delete");
        }

        [Test]
        public void GivenReferencedTemplate_WhenTemplateDeleted_ThenShowNotAllowDelete()
        {
            // Setup
            assetDatabaseMock.mockTemplatePath = TestTemplatePath;

            // Act
            var deleteAllowed = subject.OnWillDeleteAsset(TestTemplatePath);

            // Verify
            Assert.IsFalse(deleteAllowed, "Should not allow delete");
        }

        [Test]
        public void GivenReferencedDirectory_WhenDirectoryDeleted_ThenShowNotAllowDelete()
        {
            // Setup
            var directoryPath = AssetDatabase.GetAssetPath(firstEntryMock.Directory);
            assetDatabaseMock.mockDirectoryPath = directoryPath;

            // Act
            var deleteAllowed = subject.OnWillDeleteAsset(directoryPath);

            // Verify
            Assert.IsFalse(deleteAllowed, "Should not allow delete");
        }

        [Test]
        public void GivenReferencedInput_WhenParentDirectoryDeleted_ThenShowNotAllowDelete()
        {
            // Setup
            var parentDirectoryPath = "Assets/Directory";
            assetDatabaseMock.mockInputPath = $"{parentDirectoryPath}/Test.txt";

            // Act
            var deleteAllowed = subject.OnWillDeleteAsset(parentDirectoryPath);

            // Verify
            Assert.IsFalse(deleteAllowed, "Should not allow delete");
        }

        [Test]
        public void GivenReferencedInput_WhenSimilarNamedAssetDeleted_ThenShowAllowDelete()
        {
            // Setup
            assetDatabaseMock.mockInputPath = testTextPath;
            var similarPath = testTextPath.Substring(0, testTextPath.Length - 2);

            // Act
            var deleteAllowed = subject.OnWillDeleteAsset(similarPath);

            // Verify
            Assert.IsTrue(deleteAllowed, "Should allow delete");
        }

        [Test]
        public void GivenReferencedInput_WhenNonReferencedAssetDeleted_ThenShowAllowDelete()
        {
            // Setup
            assetDatabaseMock.mockInputPath = testTextPath;
            var nonReferencedPath = "Assets/Any.asset";

            // Act
            var deleteAllowed = subject.OnWillDeleteAsset(nonReferencedPath);

            // Verify
            Assert.IsTrue(deleteAllowed, "Should allow delete");
        }

        [Test]
        public void GivenExistingEntryId_WhenRenderSingleEntry_ThenShouldRenderEntry()
        {
            // Setup
            var id = firstEntryMock.Id;

            // Act
            subject.RenderEntry(id);

            // Verify
            Assert.AreEqual(1, fileMock.WriteAllTextCount, "Unexpected number of entries rendered");
            Assert.AreEqual(ExpectedOutput, fileMock.Contents[0], "Wrong render result");
        }

        [Test]
        public void GivenNonExistingEntryId_WhenRenderSingleEntry_ThenShouldNotRenderEntry()
        {
            // Setup
            var id = "wrong-id";

            // Act
            subject.RenderEntry(id);

            // Verify
            Assert.AreEqual(0, fileMock.WriteAllTextCount, "Unexpected number of entries rendered");
        }

        [Test]
        public void GivenNonExistingEntryId_WhenRenderSingleEntry_ThenShouldLogError()
        {
            // Setup
            var id = "wrong-id";

            // Act
            subject.RenderEntry(id);

            // Verify
            Assert.AreEqual(1, loggerMock.ErrorCount, "Unexpected number of errors logged");
        }

        [Test]
        public void GivenInvalidEntry_WhenRenderSingleEntry_ThenShouldNotRenderEntry()
        {
            // Setup
            firstEntryMock.valid = false;
            var id = firstEntryMock.Id;

            // Act
            subject.RenderEntry(id);

            // Verify
            Assert.AreEqual(0, fileMock.WriteAllTextCount, "Unexpected number of entries rendered");
        }

        [Test]
        public void GivenInvalidEntry_WhenRenderSingleEntry_ThenShouldLogError()
        {
            // Setup
            firstEntryMock.valid = false;
            var id = firstEntryMock.Id;

            // Act
            subject.RenderEntry(id);

            // Verify
            Assert.AreEqual(1, loggerMock.ErrorCount, "Unexpected number of errors logged");
        }
    }
}
