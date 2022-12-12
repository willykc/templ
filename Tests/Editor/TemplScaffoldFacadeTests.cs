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
    using Mocks;
    using Scaffold;
    using static TemplEntryCoreTests;

    internal class TemplScaffoldFacadeTests
    {
        private ITemplScaffoldFacade subject;
        private SettingsProviderMock settingsProviderMock;
        private AssetDatabaseMock assetDatabaseMock;
        private EditorUtilityMock editorUtilityMock;
        private TemplScaffoldCoreMock templScaffoldCoreMock;
        private TemplSettings settings;
        private TemplScaffoldMock newScaffoldMock;
        private TemplScaffoldMock existingScaffoldMock;
        private LoggerMock loggerMock;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            settings = TemplTestUtility.CreateTestAsset<TemplSettings>(TestSettingsPath, out _);
        }

        [SetUp]
        public void BeforeEach()
        {
            subject = new TemplScaffoldFacade(
                loggerMock = new LoggerMock(),
                settingsProviderMock = new SettingsProviderMock(),
                assetDatabaseMock = new AssetDatabaseMock(),
                editorUtilityMock = new EditorUtilityMock(),
                templScaffoldCoreMock = new TemplScaffoldCoreMock());

            settingsProviderMock.settingsExist = true;
            settingsProviderMock.settings = UnityEngine.Object.Instantiate(settings);
            newScaffoldMock = ScriptableObject.CreateInstance<TemplScaffoldMock>();
            existingScaffoldMock = ScriptableObject.CreateInstance<TemplScaffoldMock>();
            settingsProviderMock.settings.Scaffolds.Add(existingScaffoldMock);
        }

        [TearDown]
        public void AfterEach()
        {
            UnityEngine.Object.DestroyImmediate(settingsProviderMock.settings);
            UnityEngine.Object.DestroyImmediate(newScaffoldMock);
            UnityEngine.Object.DestroyImmediate(existingScaffoldMock);
        }

        [OneTimeTearDown]
        public void AfterAll()
        {
            TemplTestUtility.DeleteTestAsset(settings);
        }

        [Test]
        public void GivenNoSettings_WhenEnablingScaffoldForSelection_ThenShouldThrowException()
        {
            // Setup
            settingsProviderMock.settingsExist = false;

            void Act()
            {
                // Act
                subject.EnableScaffoldForSelection(newScaffoldMock);
            }

            // Verify
            Assert.Throws<InvalidOperationException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenNullScaffold_WhenEnablingScaffoldForSelection_ThenShouldThrowException()
        {
            void Act()
            {
                // Act
                subject.EnableScaffoldForSelection(null);
            }

            // Verify
            Assert.Throws<ArgumentNullException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenInvalidScaffold_WhenEnablingScaffoldForSelection_ThenShouldThrowException()
        {
            // Setup
            newScaffoldMock.isValid = false;

            void Act()
            {
                // Act
                subject.EnableScaffoldForSelection(newScaffoldMock);
            }

            // Verify
            Assert.Throws<ArgumentException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenValidScaffold_WhenEnablingScaffoldForSelection_ThenShouldEnableScaffold()
        {
            // Act
            subject.EnableScaffoldForSelection(newScaffoldMock);

            // Verify
            Assert.That(settingsProviderMock.settings.Scaffolds, Does.Contain(newScaffoldMock),
                "Did not enable scaffold");
        }

        [Test]
        public void GivenValidScaffold_WhenEnablingScaffoldForSelection_ThenShouldSetSettingsDirty()
        {
            // Act
            subject.EnableScaffoldForSelection(newScaffoldMock);

            // Verify
            Assert.AreEqual(1, editorUtilityMock.SetDirtyCount, "Did not set Settings dirty");
        }

        [Test]
        public void GivenValidScaffold_WhenEnablingScaffoldForSelection_ThenShouldSaveAllAssets()
        {
            // Act
            subject.EnableScaffoldForSelection(newScaffoldMock);

            // Verify
            Assert.AreEqual(1, assetDatabaseMock.SaveAssetsCount, "Did not set save assets");
        }

        [Test]
        public void GivenNoSettings_WhenDisablingScaffoldForSelection_ThenShouldThrowException()
        {
            // Setup
            settingsProviderMock.settingsExist = false;

            void Act()
            {
                // Act
                subject.DisableScaffoldForSelection(existingScaffoldMock);
            }

            // Verify
            Assert.Throws<InvalidOperationException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenNullScaffold_WhenDisablingScaffoldForSelection_ThenShouldThrowException()
        {
            void Act()
            {
                // Act
                subject.DisableScaffoldForSelection(null);
            }

            // Verify
            Assert.Throws<ArgumentNullException>(Act, "Did not throw expected exception");
        }

        [Test]
        public void GivenInvalidScaffold_WhenDisablingScaffoldForSelection_ThenShouldDisableIt()
        {
            // Setup
            newScaffoldMock.isValid = false;

            // Act
            subject.DisableScaffoldForSelection(existingScaffoldMock);

            // Verify
            Assert.That(settingsProviderMock.settings.Scaffolds,
                Does.Not.Contains(existingScaffoldMock),
                "Did not disable scaffold");
        }

        [Test]
        public void GivenValidScaffold_WhenDisablingScaffoldForSelection_ThenShouldDisableIt()
        {
            // Act
            subject.DisableScaffoldForSelection(existingScaffoldMock);

            // Verify
            Assert.That(settingsProviderMock.settings.Scaffolds,
                Does.Not.Contains(existingScaffoldMock),
                "Did not disable scaffold");
        }

        [Test]
        public void GivenValidScaffold_WhenDisablingScaffoldForSelection_ThenShouldSetDirty()
        {
            // Act
            subject.DisableScaffoldForSelection(existingScaffoldMock);

            // Verify
            Assert.AreEqual(1, editorUtilityMock.SetDirtyCount, "Did not set Settings dirty");
        }

        [Test]
        public void GivenValidScaffold_WhenDisablingScaffoldForSelection_ThenShouldSaveAllAssets()
        {
            // Act
            subject.DisableScaffoldForSelection(existingScaffoldMock);

            // Verify
            Assert.AreEqual(1, assetDatabaseMock.SaveAssetsCount, "Did not set save assets");
        }
    }
}
