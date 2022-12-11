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
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Willykc.Templ.Editor.Tests
{
    using Entry;
    using Mocks;
    using static TemplEntryCoreTests;

    internal class TemplEntryFacadeTests
    {
        private const string TestOutputPath = "Packages/com.willykc.templ/Tests/Editor/out3.txt";
        private ITemplEntryFacade subject;
        private SettingsProviderMock settingsProviderMock;
        private AssetDatabaseMock assetDatabaseMock;
        private EditorUtilityMock editorUtilityMock;
        private TemplEntryCoreMock templEntryCoreMock;
        private TemplSettings settings;
        private TextAsset testText;
        private ScribanAsset testTemplate;
        private DefaultAsset testDirectory;
        private string firstEntryId;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            settings = TemplTestUtility.CreateTestAsset<TemplSettings>(TestSettingsPath, out _);
            testText = TemplTestUtility.CreateTestAsset<TextAsset>(TestTextPath, out _);
            testTemplate = TemplTestUtility.CreateTestAsset<ScribanAsset>(TestTemplatePath, out _);
            testDirectory =
                AssetDatabase.LoadAssetAtPath<DefaultAsset>(Path.GetDirectoryName(TestOutputPath));
        }

        [SetUp]
        public void BeforeEach()
        {
            settingsProviderMock = new SettingsProviderMock();
            assetDatabaseMock = new AssetDatabaseMock();
            editorUtilityMock = new EditorUtilityMock();
            templEntryCoreMock = new TemplEntryCoreMock();
            subject = new TemplEntryFacade(
                settingsProviderMock,
                assetDatabaseMock,
                editorUtilityMock,
                templEntryCoreMock);

            settingsProviderMock.settingsExist = true;
            settingsProviderMock.settings = UnityEngine.Object.Instantiate(settings);
            assetDatabaseMock.mockLoadAsset = testDirectory;
            assetDatabaseMock.mockIsValidFolder = true;
            firstEntryId = settingsProviderMock.settings.Entries[0].Id;
        }

        [TearDown]
        public void AfterEach()
        {
            UnityEngine.Object.DestroyImmediate(settingsProviderMock.settings);
        }

        [OneTimeTearDown]
        public void AfterAll()
        {
            TemplTestUtility.DeleteTestAsset(settings);
            TemplTestUtility.DeleteTestAsset(testText);
            TemplTestUtility.DeleteTestAsset(testTemplate);
        }

        [Test]
        public void GivenNoSettings_WhenGettingAllEntries_ThenShouldThrowException()
        {
            // Setup
            settingsProviderMock.settingsExist = false;

            void Act()
            {
                // Act
                subject.GetEntries();
            }

            // Verify
            Assert.Throws<InvalidOperationException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenSettingsWithEntries_WhenGettingAllEntries_ThenShouldReturnAllEntries()
        {
            // Act
            var restult = subject.GetEntries();

            // Verify
            CollectionAssert.IsNotEmpty(restult, "No entries were returned");
            CollectionAssert.AreEquivalent(settingsProviderMock.settings.Entries, restult,
                "Returned entries did not match entries in settings");
        }

        [Test]
        public void GivenNoSettings_WhenAddingEntry_ThenShouldThrowException()
        {
            // Setup
            settingsProviderMock.settingsExist = false;

            void Act()
            {
                // Act
                subject.AddEntry<EntryMock>(testText, testTemplate, TestOutputPath);
            }

            // Verify
            Assert.Throws<InvalidOperationException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenNullInputAsset_WhenAddingEntry_ThenShouldThrowException()
        {
            void Act()
            {
                // Act
                subject.AddEntry<EntryMock>(null, testTemplate, TestOutputPath);
            }

            // Verify
            Assert.Throws<ArgumentNullException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenNullTemplate_WhenAddingEntry_ThenShouldThrowException()
        {
            void Act()
            {
                // Act
                subject.AddEntry<EntryMock>(testText, null, TestOutputPath);
            }

            // Verify
            Assert.Throws<ArgumentNullException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenNullOutputAssetPath_WhenAddingEntry_ThenShouldThrowException()
        {
            void Act()
            {
                // Act
                subject.AddEntry<EntryMock>(testText, testTemplate, null);
            }

            // Verify
            Assert.Throws<ArgumentNullException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenSettingsAsInputAsset_WhenAddingEntry_ThenShouldThrowException()
        {
            // Setup
            var settings = settingsProviderMock.settings;

            void Act()
            {
                // Act
                subject.AddEntry<EntryMock>(settings, testTemplate, TestOutputPath);
            }

            // Verify
            Assert.Throws<ArgumentException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenTemplateAsInputAsset_WhenAddingEntry_ThenShouldThrowException()
        {
            void Act()
            {
                // Act
                subject.AddEntry<EntryMock>(testTemplate, testTemplate, TestOutputPath);
            }

            // Verify
            Assert.Throws<ArgumentException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenInvalidTemplate_WhenAddingEntry_ThenShouldThrowException()
        {
            // Setup
            var template = UnityEngine.Object.Instantiate(testTemplate);
            SetTemplateAsInvalid(template);

            void Act()
            {
                // Act
                subject.AddEntry<EntryMock>(testText, template, TestOutputPath);
            }

            // Verify
            Assert.Throws<ArgumentException>(Act, "Did not throw expected exception");

            // Clean
            UnityEngine.Object.DestroyImmediate(template);
        }

        [Test]
        public void GivenNonExistingDirectory_WhenAddingEntry_ThenShouldThrowException()
        {
            // Setup
            assetDatabaseMock.mockIsValidFolder = false;

            void Act()
            {
                // Act
                subject.AddEntry<EntryMock>(testText, testTemplate, TestOutputPath);
            }

            // Verify
            Assert.Throws<DirectoryNotFoundException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenIllegalCharactersInPath_WhenAddingEntry_ThenShouldThrowException()
        {
            // Setup
            var invalidOutputPath = "Packages/com.willykc.templ/Tests/Editor/out3.txt\"";

            void Act()
            {
                // Act
                subject.AddEntry<EntryMock>(testText, testTemplate, invalidOutputPath);
            }

            // Verify
            Assert.Throws<ArgumentException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenInvalidCharactersInPath_WhenAddingEntry_ThenShouldThrowException()
        {
            // Setup
            var invalidOutputPath = "Packages/com.willykc.templ/Tests/Editor/out3.txt?";

            void Act()
            {
                // Act
                subject.AddEntry<EntryMock>(testText, testTemplate, invalidOutputPath);
            }

            // Verify
            Assert.Throws<ArgumentException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenInvalidEntryType_WhenAddingEntry_ThenShouldThrowException()
        {
            void Act()
            {
                // Act
                subject.AddEntry<InvalidEntryMock>(testText, testTemplate, TestOutputPath);
            }

            // Verify
            Assert.Throws<InvalidOperationException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenExistingEntryWithOutputPath_WhenAddingEntry_ThenShouldThrowException()
        {
            // Setup
            var existingOutputPath = settingsProviderMock.settings.Entries[0].OutputPath;

            void Act()
            {
                // Act
                subject.AddEntry<EntryMock>(testText, testTemplate, existingOutputPath);
            }

            // Verify
            Assert.Throws<InvalidOperationException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenIncorrectInputType_WhenAddingEntry_ThenShouldThrowException()
        {
            // Setup
            var input = ScriptableObject.CreateInstance<ScriptableObject>();

            void Act()
            {
                // Act
                subject.AddEntry<EntryMock>(input, testTemplate, TestOutputPath);
            }

            // Verify
            Assert.Throws<ArgumentException>(Act, "Did not throw expected exception");

            // Clean
            UnityEngine.Object.DestroyImmediate(input);
        }

        [Test]
        public void GivenValidParameters_WhenAddingEntry_ThenShouldAddEntryToSettings()
        {
            // Setup
            var numberOfEntriesBeforeTest = settingsProviderMock.settings.Entries.Count;

            // Act
            var id = subject.AddEntry<EntryMock>(testText, testTemplate, TestOutputPath);

            // Verify
            Assert.That(settingsProviderMock.settings.Entries,
                Has.Exactly(1).Matches<TemplEntry>(e =>
                e.Id == id &&
                e.InputAsset == testText &&
                e.Template == testTemplate &&
                e.OutputPath == TestOutputPath),
                "Did not add expected entry");
            Assert.AreEqual(numberOfEntriesBeforeTest + 1,
                settingsProviderMock.settings.Entries.Count,
                "Did not increase number of entries by one");
        }

        [Test]
        public void GivenValidParameters_WhenAddingEntry_ThenShouldSetSettingsDirty()
        {
            // Act
            subject.AddEntry<EntryMock>(testText, testTemplate, TestOutputPath);

            // Verify
            Assert.AreEqual(1, editorUtilityMock.SetDirtyCount, "Did not set Settings dirty");
        }

        [Test]
        public void GivenValidParameters_WhenAddingEntry_ThenShouldSaveAllAssets()
        {
            // Act
            subject.AddEntry<EntryMock>(testText, testTemplate, TestOutputPath);

            // Verify
            Assert.AreEqual(1, assetDatabaseMock.SaveAssetsCount, "Did not set save assets");
        }

        [Test]
        public void GivenNoSettings_WhenUpdatingEntry_ThenShouldThrowException()
        {
            // Setup
            settingsProviderMock.settingsExist = false;
            var input = UnityEngine.Object.Instantiate(testText);

            void Act()
            {
                // Act
                subject.UpdateEntry(firstEntryId, inputAsset: input);
            }

            // Verify
            Assert.Throws<InvalidOperationException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenNullId_WhenUpdatingEntry_ThenShouldThrowException()
        {
            void Act()
            {
                // Act
                subject.UpdateEntry(null);
            }

            // Verify
            Assert.Throws<ArgumentNullException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenSettingsAsInputAsset_WhenUpdatingEntry_ThenShouldThrowException()
        {
            // Setup
            var settings = settingsProviderMock.settings;

            void Act()
            {
                // Act
                subject.UpdateEntry(firstEntryId, inputAsset: settings);
            }

            // Verify
            Assert.Throws<ArgumentException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenTemplateAsInputAsset_WhenUpdatingEntry_ThenShouldThrowException()
        {
            void Act()
            {
                // Act
                subject.UpdateEntry(firstEntryId, inputAsset: testTemplate);
            }

            // Verify
            Assert.Throws<ArgumentException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenInvalidTemplate_WhenUpdatingEntry_ThenShouldThrowException()
        {
            // Setup
            var template = UnityEngine.Object.Instantiate(testTemplate);
            SetTemplateAsInvalid(template);

            void Act()
            {
                // Act
                subject.UpdateEntry(firstEntryId, template: template);
            }

            // Verify
            Assert.Throws<ArgumentException>(Act, "Did not throw expected exception");

            // Clean
            UnityEngine.Object.DestroyImmediate(template);
        }

        [Test]
        public void GivenNonExistingDirectory_WhenUpdatingEntry_ThenShouldThrowException()
        {
            // Setup
            assetDatabaseMock.mockIsValidFolder = false;

            void Act()
            {
                // Act
                subject.UpdateEntry(firstEntryId, outputAssetPath: TestOutputPath);
            }

            // Verify
            Assert.Throws<DirectoryNotFoundException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenIllegalCharactersInPath_WhenUpdatingEntry_ThenShouldThrowException()
        {
            // Setup
            var invalidOutputPath = "Packages/com.willykc.templ/Tests/Editor/out3.txt\"";

            void Act()
            {
                // Act
                subject.UpdateEntry(firstEntryId, outputAssetPath: invalidOutputPath);
            }

            // Verify
            Assert.Throws<ArgumentException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenInvalidCharactersInPath_WhenUpdatingEntry_ThenShouldThrowException()
        {
            // Setup
            var invalidOutputPath = "Packages/com.willykc.templ/Tests/Editor/out3.txt?";

            void Act()
            {
                // Act
                subject.UpdateEntry(firstEntryId, outputAssetPath: invalidOutputPath);
            }

            // Verify
            Assert.Throws<ArgumentException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenWrongId_WhenUpdatingEntry_ThenShouldThrowException()
        {
            // Setup
            var wrongId = "not-entry-id";

            void Act()
            {
                // Act
                subject.UpdateEntry(wrongId, outputAssetPath: TestOutputPath);
            }

            // Verify
            Assert.Throws<InvalidOperationException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenExistingEntryWithOutputPath_WhenUpdatingEntry_ThenShouldThrowException()
        {
            // Setup
            var existingOutputPath = settingsProviderMock.settings.Entries[1].OutputPath;

            void Act()
            {
                // Act
                subject.UpdateEntry(firstEntryId, outputAssetPath: existingOutputPath);
            }

            // Verify
            Assert.Throws<InvalidOperationException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenIncorrectInputType_WhenUpdatingEntry_ThenShouldThrowException()
        {
            // Setup
            var input = ScriptableObject.CreateInstance<ScriptableObject>();

            void Act()
            {
                // Act
                subject.UpdateEntry(firstEntryId, inputAsset: input);
            }

            // Verify
            Assert.Throws<ArgumentException>(Act, "Did not throw expected exception");

            // Clean
            UnityEngine.Object.DestroyImmediate(input);
        }

        [Test]
        public void GivenValidParameters_WhenUpdatingEntry_ThenShouldUpdateEntryInSettings()
        {
            // Setup
            var input = UnityEngine.Object.Instantiate(testText);
            var template = UnityEngine.Object.Instantiate(testTemplate);
            var outputPath = "Packages/com.willykc.templ/Tests/Editor/out4.txt";

            // Act
            subject.UpdateEntry(firstEntryId, input, template, outputPath);

            // Verify
            Assert.AreEqual(settingsProviderMock.settings.Entries[0].InputAsset, input,
                "Did not update input asset");
            Assert.AreEqual(settingsProviderMock.settings.Entries[0].Template, template,
                "Did not update template");
            Assert.AreEqual(settingsProviderMock.settings.Entries[0].OutputPath, outputPath,
                "Did not update output asset path");
        }

        [Test]
        public void GivenValidParameters_WhenUpdatingEntry_ThenShouldSetSettingsDirty()
        {
            // Setup
            var input = UnityEngine.Object.Instantiate(testText);
            var template = UnityEngine.Object.Instantiate(testTemplate);
            var outputPath = "Packages/com.willykc.templ/Tests/Editor/out4.txt";

            // Act
            subject.UpdateEntry(firstEntryId, input, template, outputPath);

            // Verify
            Assert.AreEqual(1, editorUtilityMock.SetDirtyCount, "Did not set Settings dirty");
        }

        [Test]
        public void GivenValidParameters_WhenUpdatingEntry_ThenShouldSaveAllAssets()
        {
            // Setup
            var input = UnityEngine.Object.Instantiate(testText);
            var template = UnityEngine.Object.Instantiate(testTemplate);
            var outputPath = "Packages/com.willykc.templ/Tests/Editor/out4.txt";

            // Act
            subject.UpdateEntry(firstEntryId, input, template, outputPath);

            // Verify
            Assert.AreEqual(1, assetDatabaseMock.SaveAssetsCount, "Did not set save assets");
        }

        [Test]
        public void GivenNoSettings_WhenRemovingEntry_ThenShouldThrowException()
        {
            // Setup
            settingsProviderMock.settingsExist = false;

            void Act()
            {
                // Act
                subject.RemoveEntry(firstEntryId);
            }

            // Verify
            Assert.Throws<InvalidOperationException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenNullId_WhenRemovingEntry_ThenShouldThrowException()
        {
            void Act()
            {
                // Act
                subject.RemoveEntry(null);
            }

            // Verify
            Assert.Throws<ArgumentNullException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenWrongId_WhenRemovingEntry_ThenShouldThrowException()
        {
            // Setup
            var wrongId = "not-entry-id";

            void Act()
            {
                // Act
                subject.RemoveEntry(wrongId);
            }

            // Verify
            Assert.Throws<InvalidOperationException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenExistingEntry_WhenRemovingEntry_ThenShouldRemoveEntryFromSettings()
        {
            // Setup
            var numberOfEntriesBeforeTest = settingsProviderMock.settings.Entries.Count;

            // Act
            subject.RemoveEntry(firstEntryId);

            // Verify
            Assert.That(settingsProviderMock.settings.Entries,
                Has.Exactly(0).Matches<TemplEntry>(e => e.Id == firstEntryId),
                "Did not remove expected entry");
            Assert.AreEqual(numberOfEntriesBeforeTest - 1,
                settingsProviderMock.settings.Entries.Count,
                "Did not decrease number of entries by one");
        }

        [Test]
        public void GivenValidParameters_WhenRemovingEntry_ThenShouldSetSettingsDirty()
        {
            // Act
            subject.RemoveEntry(firstEntryId);

            // Verify
            Assert.AreEqual(1, editorUtilityMock.SetDirtyCount, "Did not set Settings dirty");
        }

        [Test]
        public void GivenValidParameters_WhenRemovingEntry_ThenShouldSaveAllAssets()
        {
            // Act
            subject.RemoveEntry(firstEntryId);

            // Verify
            Assert.AreEqual(1, assetDatabaseMock.SaveAssetsCount, "Did not set save assets");
        }

        [Test]
        public void GivenNoSettings_WhenRenderingEntry_ThenShouldThrowException()
        {
            // Setup
            settingsProviderMock.settingsExist = false;

            void Act()
            {
                // Act
                subject.ForceRenderEntry(firstEntryId);
            }

            // Verify
            Assert.Throws<InvalidOperationException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenNullId_WhenRenderingEntry_ThenShouldThrowException()
        {
            void Act()
            {
                // Act
                subject.ForceRenderEntry(null);
            }

            // Verify
            Assert.Throws<ArgumentNullException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenWrongId_WhenRenderingEntry_ThenShouldThrowException()
        {
            // Setup
            var wrongId = "not-entry-id";

            void Act()
            {
                // Act
                subject.ForceRenderEntry(wrongId);
            }

            // Verify
            Assert.Throws<InvalidOperationException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenInvalidEntry_WhenRenderingEntry_ThenShouldThrowException()
        {
            // Setup
            var firstEntryMock = settingsProviderMock.settings.Entries[0] as EntryMock;
            firstEntryMock.valid = false;

            void Act()
            {
                // Act
                subject.ForceRenderEntry(firstEntryId);
            }

            // Verify
            Assert.Throws<InvalidOperationException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenValidEntry_WhenRenderingEntry_ThenShouldRenderEntry()
        {
            // Act
            subject.ForceRenderEntry(firstEntryId);

            // Verify
            Assert.AreEqual(1, templEntryCoreMock.RenderEntryCount, "Did not render entry");
        }

        [Test]
        public void GivenNoSettings_WhenRenderingAllValidEntry_ThenShouldThrowException()
        {
            // Setup
            settingsProviderMock.settingsExist = false;

            void Act()
            {
                // Act
                subject.ForceRenderAllValidEntries();
            }

            // Verify
            Assert.Throws<InvalidOperationException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenValidEntries_WhenRenderingAllValidEntry_ThenShouldRenderAllValidEntries()
        {
            // Act
            subject.ForceRenderAllValidEntries();

            // Verify
            Assert.AreEqual(1, templEntryCoreMock.RenderAllValidEntriesCount,
                "Did not render all valid entries");
        }

        private static void SetTemplateAsInvalid(ScribanAsset template)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase;
            typeof(ScribanAsset)
                .GetField(nameof(ScribanAsset.HasErrors), flags)
                .SetValue(template, true);
        }
    }
}
