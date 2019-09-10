using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;

using NUnit.Framework;


namespace Meriworks.Markdown.Tests
{
    [TestFixture]
    public class MarkdownTextTests : BaseTest
    {

        private static readonly Assembly TestAssembly = Assembly.GetAssembly(typeof(BaseTest));
        public IEnumerable<TestCaseData> GetTests()
        {
            var namespacePrefix = $"{TestAssembly.GetName().Name}.testfiles";
            var resourceNames = TestAssembly.GetManifestResourceNames();

            foreach (var resourceName in resourceNames.Where(n => n.StartsWith(namespacePrefix) && n.EndsWith(".text")))
            {
                yield return new TestCaseData(resourceName);
            }
        }

        private static string GetResourceFileContent(string resourceName)
        {
            using (var stream = TestAssembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return null;

                using (var streamReader = new StreamReader(stream))
                    return streamReader.ReadToEnd();
            }
        }
        [TestCaseSource(nameof(GetTests))]
        public void Test(string markdownName)
        {

            var m = new MarkdownParser();
            var markdownContent = GetResourceFileContent(markdownName);
            var actualContent = m.Transform(markdownContent);
            var actualContentWithoutWhitespace = Program.RemoveWhitespace(actualContent);
            var expectedName = Path.ChangeExtension(markdownName, "html");
            var expectedContent = GetResourceFileContent(expectedName);
            if (expectedContent == null)
            {
                Console.Write(actualContent);
                Assert.Fail($"No expected content is defined, please specify a file named '{expectedName}'");
  
            }
            else
            {
                var expectedContentWithoutWhitespace = Program.RemoveWhitespace(expectedContent);

                Assert.That(actualContentWithoutWhitespace,
                    Is.EqualTo(expectedContentWithoutWhitespace),
                    "Mismatch between '{0}' and the transformed '{1}'.",
                    markdownName,
                    expectedName);
            }
        }
    }
}
