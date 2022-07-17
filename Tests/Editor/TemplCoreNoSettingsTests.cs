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
using UnityEditor;

namespace Willykc.Templ.Editor.Tests
{
    using Mocks;
    using static TemplCoreTests;

    internal class TemplCoreNoSettingsTests
    {
        private TemplCore subject;
        private AssetDatabaseMock assetDatabaseMock;
        private FileMock fileMock;
        private SessionStateMock sessionStateMock;
        private LoggerMock loggerMock;
        private SettingsProviderMock settingsProviderMock;
        private Type[] typeCache;
        private TemplSettings settings;
        private AssetChanges changes;
        private EntryMock firstEntryMock;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            typeCache = new Type[0];
            settings = AssetDatabase.LoadAssetAtPath<TemplSettings>(TestSettingsPath);
            firstEntryMock = settings.Entries[0] as EntryMock;
        }

        [SetUp]
        public void BeforeEach()
        {
            changes = new AssetChanges(new string[0], new string[0], new string[0], new string[0]);

            subject = new TemplCore(
                assetDatabaseMock = new AssetDatabaseMock(),
                fileMock = new FileMock(),
                sessionStateMock = new SessionStateMock(),
                loggerMock = new LoggerMock(),
                settingsProviderMock = new SettingsProviderMock(),
                typeCache);

            settingsProviderMock.settingsExist = false;
            settingsProviderMock.settings = settings;
        }

        [TearDown]
        public void AfterEach()
        {
            firstEntryMock.Clear();
        }

        [Test]
        public void GivenNoSettings_WhenAssetsChange_ThenEntryShouldNotRender()
        {
            // Setup
            firstEntryMock.templateChanged = true;

            // Act
            subject.OnAssetsChanged(changes);

            // Verify
            Assert.AreEqual(0, fileMock.WriteAllTextCount, "Unexpected render");
        }

        [Test]
        public void GivenNoSettings_WhenAssemblyReloads_ThenEntryShouldNotRender()
        {
            // Setup
            sessionStateMock.value = firstEntryMock.guid;

            // Act
            subject.OnAfterAssemblyReload();

            // Verify
            Assert.AreEqual(0, fileMock.WriteAllTextCount, "Unexpected render");
        }

        [Test]
        public void GivenNoSettings_WhenAssetDeleted_ThenShouldNotFlagEntries()
        {
            // Setup
            firstEntryMock.willDelete = true;

            // Act
            subject.OnWillDeleteAsset(string.Empty);

            // Verify
            Assert.AreNotEqual(firstEntryMock.guid, sessionStateMock.SetValue, "Unexpected flag");
        }

        [Test]
        public void GivenNoSettings_WhenRenderAllValidEntries_ThenEntriesShouldNotRender()
        {
            // Act
            subject.RenderAllValidEntries();

            // Verify
            Assert.AreEqual(0, fileMock.WriteAllTextCount, "Unexpected render");
        }
    }
}