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
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Willykc.Templ.Editor.Tests
{
    using Entry;
    using Mocks;
    using static TemplEntryCoreTests;
    using static TemplSettings;

    internal class TemplEntryCoreFunctionConflictsTests
    {
        private ITemplEntryCore subject;
        private AssetDatabaseMock assetDatabaseMock;
        private FileSystemMock fileSystemMock;
        private SessionStateMock sessionStateMock;
        private LoggerMock loggerMock;
        private SettingsProviderMock settingsProviderMock;
        private TemplateFunctionProviderMock templateFunctionProviderMock;
        private EditorUtilityMock editorUtilityMock;
        private TemplSettings settings;
        private ScribanAsset testTemplate;
        private TextAsset testText;
        private AssetsPaths changes;
        private EntryMock firstEntryMock;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            settings = TemplTestUtility.CreateTestAsset<TemplSettings>(TestSettingsPath, out _);
            testTemplate = TemplTestUtility.CreateTestAsset<ScribanAsset>(TestTemplatePath, out _);
            testText = TemplTestUtility.CreateTestAsset<TextAsset>(TestTextPath, out _);
        }

        [SetUp]
        public void BeforeEach()
        {
            changes = new AssetsPaths(new string[0], new string[0], new string[0], new string[0]);
            templateFunctionProviderMock = new TemplateFunctionProviderMock
            {
                DuplicateFunctionNames = new[] { "GetType" }
            };

            subject = new TemplEntryCore(
                assetDatabaseMock = new AssetDatabaseMock(),
                fileSystemMock = new FileSystemMock(),
                sessionStateMock = new SessionStateMock(),
                loggerMock = new LoggerMock(),
                settingsProviderMock = new SettingsProviderMock(),
                templateFunctionProviderMock,
                editorUtilityMock = new EditorUtilityMock());

            settingsProviderMock.settingsExist = true;
            settingsProviderMock.settings = UnityObject.Instantiate(settings);
            firstEntryMock = settingsProviderMock.settings.Entries[0] as EntryMock;
        }

        [TearDown]
        public void AfterEach()
        {
            UnityObject.DestroyImmediate(settingsProviderMock.settings);
        }

        [OneTimeTearDown]
        public void AfterAll()
        {
            TemplTestUtility.DeleteTestAsset(settings);
            TemplTestUtility.DeleteTestAsset(testTemplate);
            TemplTestUtility.DeleteTestAsset(testText);
        }

        [Test]
        public void GivenFunctionConflicts_WhenInstantiatingTemplCore_ThenShouldLogError()
        {
            // Verify
            Assert.AreEqual(1, loggerMock.ErrorCount, "Did not log error");
        }

        [Test]
        public void GivenFunctionConflicts_WhenAssetsChange_ThenShouldLogError()
        {
            // Act
            subject.OnAssetsChanged(changes);

            // Verify
            Assert.AreEqual(2, loggerMock.ErrorCount, "Did not log error");
        }

        [Test]
        public void GivenFunctionConflicts_WhenAssetsChange_ThenEntriesShouldNotRender()
        {
            // Setup
            firstEntryMock.templateChanged = true;

            // Act
            subject.OnAssetsChanged(changes);

            // Verify
            Assert.AreEqual(0, fileSystemMock.WriteAllTextCount, "Unexpected render");
        }

        [Test]
        public void GivenFunctionConflicts_WhenAssemblyReloads_ThenShouldLogError()
        {
            // Setup
            sessionStateMock.value = firstEntryMock.Id;

            // Act
            subject.OnAfterAssemblyReload();

            // Verify
            Assert.AreEqual(2, loggerMock.ErrorCount, "Did not log error");
        }

        [Test]
        public void GivenFunctionConflicts_WhenAssemblyReloads_ThenEntriesShouldNotRender()
        {
            // Setup
            sessionStateMock.value = firstEntryMock.Id;

            // Act
            subject.OnAfterAssemblyReload();

            // Verify
            Assert.AreEqual(0, fileSystemMock.WriteAllTextCount, "Unexpected render");
        }

        [Test]
        public void GivenFunctionConflicts_WhenAssetDeleted_ThenShouldNotFlagEntries()
        {
            // Setup
            firstEntryMock.inputChanged = true;

            // Act
            subject.OnWillDeleteAsset(string.Empty);

            // Verify
            Assert.AreNotEqual(firstEntryMock.Id, sessionStateMock.SetValue, "Unexpected flag");
        }

        [Test]
        public void GivenFunctionConflicts_WhenRenderAllValidEntries_ThenShouldLogError()
        {
            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(2, loggerMock.ErrorCount, "Did not log error");
        }

        [Test]
        public void GivenFunctionConflicts_WhenRenderAllValidEntries_ThenEntriesShouldNotRender()
        {
            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(0, fileSystemMock.WriteAllTextCount, "Unexpected render");
        }

        [Test]
        public void GivenFunctionConflicts_WhenRenderSingleEntry_ThenShouldLogError()
        {
            // Setup
            var id = firstEntryMock.Id;

            // Act
            subject.RenderEntry(id);

            // Verify
            Assert.AreEqual(2, loggerMock.ErrorCount, "Did not log error");
        }

        [Test]
        public void GivenFunctionConflicts_WhenRenderSingleEntry_ThenEntriesShouldNotRender()
        {
            // Setup
            var id = firstEntryMock.Id;

            // Act
            subject.RenderEntry(id);

            // Verify
            Assert.AreEqual(0, fileSystemMock.WriteAllTextCount, "Unexpected render");
        }

        [Test]
        public void GivenFunctionNamedWithReservedKeyword_WhenAssetsChange_ThenShouldLogError()
        {
            // Setup
            templateFunctionProviderMock = new TemplateFunctionProviderMock
            {
                FunctionNames = new[] { NameOfOutputAssetPath }
            };

            // Act
            subject.OnAssetsChanged(changes);

            // Verify
            Assert.AreEqual(2, loggerMock.ErrorCount, "Did not log error");
        }
    }
}
