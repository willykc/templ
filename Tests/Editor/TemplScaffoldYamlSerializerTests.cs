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

namespace Willykc.Templ.Editor.Tests
{
    using Scaffold;
    using static TemplScaffoldCoreTests;
    using subject = Scaffold.TemplScaffoldYamlSerializer;

    internal class TemplScaffoldYamlSerializerTests
    {
        private TemplScaffold testScaffold;
        private ScribanAsset testScaffoldTemplate;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            testScaffoldTemplate = TemplTestUtility
                .CreateTestAsset<ScribanAsset>(TestScaffoldTemplatePath, out _);
            testScaffold = TemplTestUtility.CreateTestAsset<TemplScaffold>(TestScaffoldPath, out _);
        }

        [OneTimeTearDown]
        public void AfterAll()
        {
            TemplTestUtility.DeleteTestAsset(testScaffoldTemplate);
            TemplTestUtility.DeleteTestAsset(testScaffold);
        }

        [Test]
        public void GivenValidScaffold_WhenSerializing_ThenShouldProduceCorrectTextRepresentation()
        {
            // Setup
            var expectedOutput = "- NewDirectory{{Input.name}}:\r\n    " +
                "- NewFile{{Selection.name}}: 123163d8e239dda4993b73291555da3c\r\n";

            // Act
            var result = subject.SerializeTree(testScaffold.Root, useGuids: true);

            // Verify
            Assert.AreEqual(expectedOutput, result, "Unexpected serialized text");
        }

        [Test]
        public void GivenValidScaffold_WhenSerializingWithoutGUIDs_ThenShouldUsePaths()
        {
            // Setup
            var expectedEnd = "TestScaffoldTemplate.sbn\r\n";

            // Act
            var result = subject.SerializeTree(testScaffold.Root);

            // Verify
            Assert.That(result, Does.EndWith(expectedEnd), "Unexpected end of serialized text");
        }

        [Test]
        public void GivenValidYAML_WhenDeserializing_ThenShouldProduceCorrectTree()
        {
            // Setup
            var directoryName = "NewDirectory";
            var fileName = "NewFile";
            var serializedTree = $"- {directoryName}:\r\n    " +
                $"- {fileName}: 123163d8e239dda4993b73291555da3c\r\n";

            // Act
            var result = subject.DeserializeTree(serializedTree);

            // Verify
            Assert.AreEqual(3, result.NodeCount, "Unexpected total number of nodes");
            Assert.AreEqual(directoryName, result.Children[0].name, "Unexpected node name");
            Assert.AreEqual(fileName, result.Children[0].Children[0].name, "Unexpected node name");
        }
    }
}
