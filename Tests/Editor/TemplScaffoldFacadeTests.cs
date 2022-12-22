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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using UnityObject = UnityEngine.Object;

namespace Willykc.Templ.Editor.Tests
{
    using Mocks;
    using Scaffold;
    using static TemplEntryCoreTests;
    using static TemplScaffoldCoreTests;
    using static TemplSettings;
    using static TemplTestUtility;

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
        private TemplScaffoldWindowManagerMock windowManagerMock;
        private ScriptableObject defaultInput;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            settings = CreateTestAsset<TemplSettings>(TestSettingsPath, out _);
        }

        [SetUp]
        public void BeforeEach()
        {
            subject = new TemplScaffoldFacade(
                loggerMock = new LoggerMock(),
                settingsProviderMock = new SettingsProviderMock(),
                assetDatabaseMock = new AssetDatabaseMock(),
                editorUtilityMock = new EditorUtilityMock(),
                windowManagerMock = new TemplScaffoldWindowManagerMock(),
                templScaffoldCoreMock = new TemplScaffoldCoreMock());

            assetDatabaseMock.mockIsValidFolder = true;
            settingsProviderMock.settingsExist = true;
            settingsProviderMock.settings = UnityObject.Instantiate(settings);
            newScaffoldMock = ScriptableObject.CreateInstance<TemplScaffoldMock>();
            existingScaffoldMock = ScriptableObject.CreateInstance<TemplScaffoldMock>();
            settingsProviderMock.settings.Scaffolds.Add(existingScaffoldMock);
            defaultInput = ScriptableObject.CreateInstance<ScriptableObject>();
            SetDefaultInput(newScaffoldMock, defaultInput);
        }

        [TearDown]
        public void AfterEach()
        {
            UnityObject.DestroyImmediate(settingsProviderMock.settings);
            UnityObject.DestroyImmediate(newScaffoldMock);
            UnityObject.DestroyImmediate(existingScaffoldMock);
            UnityObject.DestroyImmediate(defaultInput);
        }

        [OneTimeTearDown]
        public void AfterAll()
        {
            DeleteTestAsset(settings);
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
        public void GivenAlreadyEnabledScaffold_WhenEnablingScaffold_ThenShouldDoNothing()
        {
            // Setup
            var expectedCount = settingsProviderMock.settings.Scaffolds.Count;

            // Act
            subject.EnableScaffoldForSelection(existingScaffoldMock);

            // Verify
            Assert.AreEqual(0, editorUtilityMock.SetDirtyCount, "Unexpectedly set Settings dirty");
            Assert.AreEqual(0, assetDatabaseMock.SaveAssetsCount, "Unexpectedly saved assets");
            Assert.AreEqual(expectedCount, settingsProviderMock.settings.Scaffolds.Count,
                "Unexpected amount of enabled scaffolds");
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

        [Test]
        public void GivenAlreadyDisabledScaffold_WhenDisablingScaffold_ThenShouldDoNothing()
        {
            // Setup
            var expectedCount = settingsProviderMock.settings.Scaffolds.Count;

            // Act
            subject.DisableScaffoldForSelection(newScaffoldMock);

            // Verify
            Assert.AreEqual(0, editorUtilityMock.SetDirtyCount, "Unexpectedly set Settings dirty");
            Assert.AreEqual(0, assetDatabaseMock.SaveAssetsCount, "Unexpectedly saved assets");
            Assert.AreEqual(expectedCount, settingsProviderMock.settings.Scaffolds.Count,
                "Unexpected amount of enabled scaffolds");
        }

        [UnityTest]
        public IEnumerator GivenCanceledToken_WhenGenerating_ThenShouldReturnNull()
        => ToCoroutine(async () =>
        {
            // Setup
            var token = new CancellationToken(canceled: true);

            // Act
            var result = await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath,
                cancellationToken: token);

            // Verify
            Assert.IsNull(result, "Value is not null");
        });

        [UnityTest]
        public IEnumerator GivenNullScaffold_WhenGenerating_ThenShouldThrowException()
        => ToCoroutine(async () =>
        {
            try
            {
                // Act
                await subject.GenerateScaffoldAsync(null, TestTargetPath);
                Assert.Fail("Expected exception was not trown");
            }
            catch (Exception exception)
            {
                // Verify
                Assert.That(exception, Is.TypeOf<ArgumentNullException>(),
                    "Unexpected exception type");
            }
        });

        [UnityTest]
        public IEnumerator GivenNullTargetPath_WhenGenerating_ThenShouldThrowException()
        => ToCoroutine(async () =>
        {
            try
            {
                // Act
                await subject.GenerateScaffoldAsync(newScaffoldMock, null);
                Assert.Fail("Expected exception was not trown");
            }
            catch (Exception exception)
            {
                // Verify
                Assert.That(exception, Is.TypeOf<ArgumentException>(),
                    "Unexpected exception type");
            }
        });

        [UnityTest]
        public IEnumerator GivenEmptyTargetPath_WhenGenerating_ThenShouldThrowException()
        => ToCoroutine(async () =>
        {
            try
            {
                // Act
                await subject.GenerateScaffoldAsync(newScaffoldMock, "");
                Assert.Fail("Expected exception was not trown");
            }
            catch (Exception exception)
            {
                // Verify
                Assert.That(exception, Is.TypeOf<ArgumentException>(),
                    "Unexpected exception type");
            }
        });

        [UnityTest]
        public IEnumerator GivenInvalidScaffold_WhenGenerating_ThenShouldThrowException()
        => ToCoroutine(async () =>
        {
            // Setup
            newScaffoldMock.isValid = false;

            try
            {
                // Act
                await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath);
                Assert.Fail("Expected exception was not trown");
            }
            catch (Exception exception)
            {
                // Verify
                Assert.That(exception, Is.TypeOf<InvalidOperationException>(),
                    "Unexpected exception type");
            }
        });

        [UnityTest]
        public IEnumerator GivenTargetPathDirNotExist_WhenGenerating_ThenShouldThrowException()
        => ToCoroutine(async () =>
        {
            // Setup
            assetDatabaseMock.mockIsValidFolder = false;

            try
            {
                // Act
                await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath);
                Assert.Fail("Expected exception was not trown");
            }
            catch (Exception exception)
            {
                // Verify
                Assert.That(exception, Is.TypeOf<DirectoryNotFoundException>(),
                    "Unexpected exception type");
            }
        });

        [UnityTest]
        public IEnumerator GivenSimultaneousCalls_WhenGenerating_ThenShouldLogError()
        => ToCoroutine(async () =>
        {
            // Setup
            windowManagerMock.SetShowInputFormDelay(50, null);

            // Act
            var firstTask = subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath);
            var secondTask = subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath);

            await Task.WhenAll(firstTask, secondTask);

            // Verify
            Assert.AreEqual(1, loggerMock.ErrorCount, "Did not show expected error");
        });

        [UnityTest]
        public IEnumerator GivenScaffoldWithDefaultInput_WhenGenerating_ThenIsGeneratingIsTrue()
        => ToCoroutine(async () =>
        {
            // Setup
            windowManagerMock.SetShowInputFormDelay(50, null);

            // Act
            var task = subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath);
            var isGenerating = subject.IsGenerating;
            await task;

            // Verify
            Assert.IsTrue(isGenerating, "IsGenerating was false during generation");
            Assert.IsFalse(subject.IsGenerating, "IsGenerating is true when it should not");
        });

        [UnityTest]
        public IEnumerator GivenScaffoldWithDefaultInput_WhenGenerating_ThenShouldShowInputForm()
        => ToCoroutine(async () =>
        {
            // Act
            await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath);

            // Verify
            Assert.AreEqual(1, windowManagerMock.ShowInputFormCount, "Did not show input form");
        });

        [UnityTest]
        public IEnumerator GivenUserClosingInputForm_WhenGenerating_ThenShouldReturnNull()
        => ToCoroutine(async () =>
        {
            // Setup
            windowManagerMock.SetShowInputFormDelay(50, null);

            // Act
            var resutl = await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath);

            // Verify
            Assert.IsNull(resutl, "Value is not null");
        });

        [UnityTest]
        public IEnumerator GivenOverwritePath_WhenGenerating_ThenShouldShowOverwriteDialog()
        => ToCoroutine(async () =>
        {
            // Setup
            templScaffoldCoreMock.errors = new TemplScaffoldError[]
            {
                new TemplScaffoldError(TemplScaffoldErrorType.Overwrite, $"{TestTargetPath}/file")
            };
            windowManagerMock.ShowInputFormSourceTask = Task.FromResult(defaultInput);
            windowManagerMock.ShowOverwriteDialogTask = Task.FromResult(new string[0]);

            // Act
            await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath);

            // Verify
            Assert.AreEqual(1, windowManagerMock.ShowOverwriteDialogCount,
                "Did not show overwrite dialog");
        });

        [UnityTest]
        public IEnumerator GivenOverwritePath_WhenGenerating_ThenShouldCloseInputFrom()
        => ToCoroutine(async () =>
        {
            // Setup
            templScaffoldCoreMock.errors = new TemplScaffoldError[]
            {
                new TemplScaffoldError(TemplScaffoldErrorType.Overwrite, $"{TestTargetPath}/file")
            };
            windowManagerMock.ShowInputFormSourceTask = Task.FromResult(defaultInput);
            windowManagerMock.ShowOverwriteDialogTask = Task.FromResult(new string[0]);

            // Act
            await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath);

            // Verify
            Assert.AreEqual(1, windowManagerMock.CloseInputFormCount,
                "Did not close input form");
        });

        [UnityTest]
        public IEnumerator GivenOverwritePath_WhenGenerating_ThenShouldNotSkipPaths()
        => ToCoroutine(async () =>
        {
            // Setup
            var overwrite = $"{TestTargetPath}/file";
            templScaffoldCoreMock.errors = new TemplScaffoldError[]
            {
                new TemplScaffoldError(TemplScaffoldErrorType.Overwrite, overwrite)
            };
            windowManagerMock.ShowInputFormSourceTask = Task.FromResult(defaultInput);
            windowManagerMock.ShowOverwriteDialogTask = Task.FromResult(new[] { overwrite });

            // Act
            await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath);

            // Verify
            CollectionAssert.AreEquivalent(new[] { overwrite }, templScaffoldCoreMock.SkipPaths,
                "Unexpected skip paths");
        });

        [UnityTest]
        public IEnumerator GivenClosedInputAfterOverwrite_WhenGenerating_ThenShouldCancelOverwrite()
        => ToCoroutine(async () =>
        {
            // Setup
            templScaffoldCoreMock.errors = new TemplScaffoldError[]
            {
                new TemplScaffoldError(TemplScaffoldErrorType.Overwrite, $"{TestTargetPath}/file")
            };
            windowManagerMock.ShowInputFormSourceTask = Task.FromResult(defaultInput);
            windowManagerMock.SetShowOverwriteDialogDelay(50, null);
            windowManagerMock.CloseInputFormAfter(40);

            // Act
            await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath);

            // Verify
            Assert.IsTrue(windowManagerMock.OverwriteDialogToken.IsCancellationRequested,
                "Did not close overwrite dialog");
        });

        [UnityTest]
        public IEnumerator GivenClosedOverwriteDialog_WhenGenerating_ThenShouldShowInputFormAgain()
        => ToCoroutine(async () =>
        {
            // Setup
            templScaffoldCoreMock.errors = new TemplScaffoldError[]
            {
                new TemplScaffoldError(TemplScaffoldErrorType.Overwrite, $"{TestTargetPath}/file")
            };
            windowManagerMock.ShowInputFormSourceTask = Task.FromResult(defaultInput);
            windowManagerMock.SetShowOverwriteDialogDelay(50, null);
            windowManagerMock.CloseInputFormAfter(100);

            // Act
            await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath);

            // Verify
            Assert.AreEqual(2, windowManagerMock.ShowInputFormCount,
                "Did not show input form twice");
        });

        [UnityTest]
        public IEnumerator GivenManyClosedOverwriteDialogs_WhenGenerating_ThenShouldMaxOutAndExit()
        => ToCoroutine(async () =>
        {
            // Setup
            templScaffoldCoreMock.errors = new TemplScaffoldError[]
            {
                new TemplScaffoldError(TemplScaffoldErrorType.Overwrite, $"{TestTargetPath}/file")
            };
            windowManagerMock.ShowInputFormSourceTask = Task.FromResult(defaultInput);

            // Act
            await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath);

            // Verify
            Assert.AreEqual(20, windowManagerMock.ShowInputFormCount,
                "Did not max out showing input form");
            Assert.AreEqual(1, windowManagerMock.CloseInputFormCount,
                "Did not close input form");
        });

        [UnityTest]
        public IEnumerator GivenInput_WhenGenerating_ThenShouldNotShowInputForm()
        => ToCoroutine(async () =>
        {
            // Act
            await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath, defaultInput);

            // Verify
            Assert.AreEqual(0, windowManagerMock.ShowInputFormCount,
                "Showed input form unexpectedly");
        });

        [UnityTest]
        public IEnumerator GivenTrailingPathSeparators_WhenGenerating_ThenShouldTrimTargetPath()
        => ToCoroutine(async () =>
        {
            // Setup
            var trail = "//";
            var targetPath = trail + TestTargetPath + trail;

            // Act
            await subject.GenerateScaffoldAsync(newScaffoldMock, targetPath);

            // Verify
            Assert.AreEqual(TestTargetPath, assetDatabaseMock.IsValidFolderPath,
                "Did not trim target path");
        });

        [UnityTest]
        public IEnumerator GivenWindowsPathSeparators_WhenGenerating_ThenShouldSanitizeAssetPath()
        => ToCoroutine(async () =>
        {
            // Setup
            var targetPath = TestTargetPath.Replace(AssetPathSeparator, WindowsPathSeparator);

            // Act
            await subject.GenerateScaffoldAsync(newScaffoldMock, targetPath);

            // Verify
            Assert.AreEqual(TestTargetPath, assetDatabaseMock.IsValidFolderPath,
                "Did not sanitize target path");
        });

        [UnityTest]
        public IEnumerator GivenNoDefaultInput_WhenGenerating_ThenShouldNotShowInputForm()
        => ToCoroutine(async () =>
        {
            // Setup
            SetDefaultInput(newScaffoldMock, null);

            // Act
            await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath);

            // Verify
            Assert.AreEqual(0, windowManagerMock.ShowInputFormCount,
                "Showed input form unexpectedly");
        });

        [UnityTest]
        public IEnumerator GivenGenerationErrors_WhenGenerating_ThenShouldLogError()
        => ToCoroutine(async () =>
        {
            // Setup
            templScaffoldCoreMock.errors = new TemplScaffoldError[]
            {
                new TemplScaffoldError(TemplScaffoldErrorType.Undefined, "message")
            };

            // Act
            await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath, defaultInput);

            // Verify
            Assert.AreEqual(1, loggerMock.ErrorCount,
                "Did not log error as expected");
        });

        [UnityTest]
        public IEnumerator GivenOverwriteAllOption_WhenGenerating_ThenShouldNotShowOverwriteDialog()
        => ToCoroutine(async () =>
        {
            // Setup
            templScaffoldCoreMock.errors = new TemplScaffoldError[]
            {
                new TemplScaffoldError(TemplScaffoldErrorType.Overwrite, $"{TestTargetPath}/file")
            };

            // Act
            await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath, defaultInput,
                overwriteOption: OverwriteOptions.OverwriteAll);

            // Verify
            Assert.AreEqual(0, windowManagerMock.ShowOverwriteDialogCount,
                "Showed overwrite dialog unexpectedly");
        });

        [UnityTest]
        public IEnumerator GivenOverwriteAllOption_WhenGenerating_ThenShouldNotSkipPaths()
        => ToCoroutine(async () =>
        {
            // Setup
            templScaffoldCoreMock.errors = new TemplScaffoldError[]
            {
                new TemplScaffoldError(TemplScaffoldErrorType.Overwrite, $"{TestTargetPath}/file")
            };

            // Act
            await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath, defaultInput,
                overwriteOption: OverwriteOptions.OverwriteAll);

            // Verify
            CollectionAssert.IsEmpty(templScaffoldCoreMock.SkipPaths,
                "Unexpected skip paths");
        });

        [UnityTest]
        public IEnumerator GivenSkipAllOption_WhenGenerating_ThenShouldNotShowOverwriteDialog()
        => ToCoroutine(async () =>
        {
            // Setup
            templScaffoldCoreMock.errors = new TemplScaffoldError[]
            {
                new TemplScaffoldError(TemplScaffoldErrorType.Overwrite, $"{TestTargetPath}/file")
            };

            // Act
            await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath, defaultInput,
                overwriteOption: OverwriteOptions.SkipAll);

            // Verify
            Assert.AreEqual(0, windowManagerMock.ShowOverwriteDialogCount,
                "Showed overwrite dialog unexpectedly");
        });

        [UnityTest]
        public IEnumerator GivenSkipAllOption_WhenGenerating_ThenShouldSkipPaths()
        => ToCoroutine(async () =>
        {
            // Setup
            var overwrite = $"{TestTargetPath}/file";
            templScaffoldCoreMock.errors = new TemplScaffoldError[]
            {
                new TemplScaffoldError(TemplScaffoldErrorType.Overwrite, overwrite)
            };

            // Act
            await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath, defaultInput,
                overwriteOption: OverwriteOptions.SkipAll);

            // Verify
            CollectionAssert.AreEquivalent(new[] { overwrite }, templScaffoldCoreMock.SkipPaths,
                "Unexpected skip paths");
        });

        [UnityTest]
        public IEnumerator GivenValidParameters_WhenGenerating_ThenShouldGenerate()
        => ToCoroutine(async () =>
        {
            // Act
            await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath, defaultInput);

            // Verify
            Assert.AreEqual(1, templScaffoldCoreMock.GenerateScaffoldCount,
                "Did not generate scaffold");
        });

        [UnityTest]
        public IEnumerator GivenValidParameters_WhenGenerating_ThenShouldReturnGeneratedPaths()
        => ToCoroutine(async () =>
        {
            // Setup
            templScaffoldCoreMock.paths = new[] { $"{TestTargetPath}/file" };

            // Act
            var result = await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath,
                defaultInput);

            // Verify
            CollectionAssert.AreEquivalent(templScaffoldCoreMock.paths, result,
                "Did not return generated paths");
        });

        [UnityTest]
        public IEnumerator GivenExternalCancelation_WhenGenerating_ThenShouldCloseAll()
        => ToCoroutine(async () =>
        {
            // Setup
            templScaffoldCoreMock.errors = new TemplScaffoldError[]
            {
                new TemplScaffoldError(TemplScaffoldErrorType.Overwrite, $"{TestTargetPath}/file")
            };
            windowManagerMock.ShowInputFormSourceTask = Task.FromResult(defaultInput);
            windowManagerMock.SetShowOverwriteDialogDelay(100, new string[0]);
            var source = new CancellationTokenSource();
            source.CancelAfter(50);

            // Act
            await subject.GenerateScaffoldAsync(newScaffoldMock, TestTargetPath,
                cancellationToken: source.Token);

            // Verify
            Assert.IsTrue(windowManagerMock.OverwriteDialogToken.IsCancellationRequested,
                "Did not close overwrite dialog");
            Assert.AreEqual(1, windowManagerMock.CloseInputFormCount,
                "Did not close input form");
        });

        private static void SetDefaultInput(TemplScaffold scaffold, ScriptableObject input)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase;
            typeof(TemplScaffold)
                .GetField(nameof(TemplScaffold.DefaultInput), flags)
                .SetValue(scaffold, input);
        }
    }
}
