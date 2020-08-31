using Meriworks.PowerShell.Cmdlets;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.IO;

namespace Meriworks.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Summary description for MarkdownInsertSamplesTaskTest.
    /// </summary>
    /// <remarks>
    /// 2011-11-11 dan: Created
    /// </remarks>
    [TestFixture]
    public class ImportMarkdownSamplesCmdletTest
    {
        /* *******************************************************************
		 *  SetupMethods 
		 * *******************************************************************/
        private MockRepository _mocks;
        private ImportMarkdownSamplesCmdlet _cmdlet;
        #region public void FixtureSetUp()
        /// <summary>
        /// Performs operations before tests are started
        /// </summary>
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            _mocks = new MockRepository();

        }
        #endregion

        [SetUp]
        public void Setup()
        {
            _cmdlet = _mocks.PartialMock<ImportMarkdownSamplesCmdlet>();
        }

        /* *******************************************************************
		 *  Test Methods 
		 * *******************************************************************/
        #region public IEnumerable<TestCaseData> FindRegionInDataTestData
        /// <summary>
        /// Gets the FindRegionInDataTestData of the MarkdownInsertSamplesTaskTest
        /// </summary>
        /// <value></value>
        public static IEnumerable<TestCaseData> FindRegionInDataTestData
        {
            get
            {
                const string sample1 = @"   This is the sample";
                const string text1 = @"
Some text
#region MyTest
" + sample1 + @"
#endregion
Another text
";

                yield return new TestCaseData(text1, "MyTest", sample1, null);
                yield return new TestCaseData(text1, "mytest", sample1, null);
                yield return new TestCaseData(text1, "MYTEST", sample1, null);

                const string sample2 = @"    	Several texts # on multipe rows //#endregion
		With some fake data...
			#¤#define&"" asd      ";
                const string text2 = @"
Q2imklqrw.
qwe,lwqleqwe
			#region      another region    
" + sample2 + @"
        #endregion      ";
                yield return new TestCaseData(text2, "another region", sample2, null);
                yield return new TestCaseData(text2, "another region2", sample2, typeof(ApplicationException));
                const string sample3 = "testing";
                const string text3 = @"
#region reg1
Sample1
#endregion

#region reg2
" + sample3 + @"
#endregion
";

                yield return new TestCaseData(text3, "reg2", sample3, null);
            }
        }
        #endregion
        [TestCaseSource(nameof(FindRegionInDataTestData))]
        public void FindRegionInDataTest(string data, string name, string expected, Type expectedException)
        {
            _mocks.ReplayAll();
            EvaluateExceptedException(() =>
            {
                var actual = _cmdlet.FindRegionInData(data, name);
                Assert.AreEqual(expected, actual, "Mismatch in actual");

            }, expectedException);

            _mocks.VerifyAll();
        }
        #region public IEnumerable<TestCaseData> GetFileContentsData
        /// <summary>
        /// Gets the GetFileContentsData of the MarkdownInsertSamplesTaskTest
        /// </summary>
        /// <value></value>
        public static IEnumerable<TestCaseData> GetFileContentsData
        {
            get
            {
                yield return new TestCaseData(null, "nonexisting file", null, typeof(ApplicationException));
                yield return new TestCaseData(null, "file.xml", "some data", null);
                yield return new TestCaseData("name", "file.xml", "some data", null);
            }
        }
        #endregion
        #region public void GetFileContentsTest(string name, string path,string data)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        [TestCaseSource(nameof(GetFileContentsData))]
        public void GetFileContentsTest(string name, string path, string data, Type expectedException)
        {
            if (!string.IsNullOrEmpty(data))
            {
                var file = new FileInfo(path);
                using (var w = new StreamWriter(file.Open(FileMode.Create)))
                {
                    w.Write(data);
                }
                path = file.FullName;
            }
            if (!string.IsNullOrEmpty(name))
            {
                Expect.Call(_cmdlet.FindRegionInData(null, null)).IgnoreArguments().Return(data);
            }
            _mocks.ReplayAll();
            EvaluateExceptedException(() =>
            {
                var content = _cmdlet.GetFileContents(name, path);
                Assert.AreEqual(data, content, "Mismatch in content");

            }, expectedException);

            _mocks.VerifyAll();
        }
        #endregion
        #region public void CalculateAbsolutePathTest(string sourceFile, string path, string expected)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="path"></param>
        /// <param name="expected"></param>
        [TestCase(@"c:\test\file.markdown", null, @"c:\test\file.cs")]
        [TestCase(@"c:\test\file.markdown", "file.cs", @"c:\test\file.cs")]
        [TestCase(@"c:\test\file.markdown", "test.cs", @"c:\test\test.cs")]
        [TestCase(@"c:\test\file.markdown", "test.xml", @"c:\test\test.xml")]
        [TestCase(@"c:\test\file.markdown", @"..\test.xml", @"c:\test.xml")]
        [TestCase(@"c:\test\file.markdown", @"../test.xml", @"c:\test.xml")]
        [TestCase(@"c:\temp\file.markdown", null, @"c:\temp\file.cs")]
        [TestCase(@"c:\temp\file.markdown", "", @"c:\temp\file.cs")]
        [TestCase(@"c:\temp path\file.markdown", null, @"c:\temp path\file.cs")]
        [TestCase(@"c:\temp path\file.markdown", "", @"c:\temp path\file.cs")]
        [TestCase(@"c:\temp path\file.markdown", "apa.cs", @"c:\temp path\apa.cs")]
        [TestCase(@"c:\vss\tfs\Meriworks - ImageVault\Development4\DeveloperDocumentation\csharp\index.markdown", null, @"c:\vss\tfs\Meriworks - ImageVault\Development4\DeveloperDocumentation\csharp\index.cs")]
        public void CalculateAbsolutePathTest(string sourceFile, string path, string expected)
        {

            _mocks.ReplayAll();
            var actual = _cmdlet.CalculateAbsolutePath(new FileInfo(sourceFile), path);
            Assert.AreEqual(expected, actual, "Mismatch in actual");

            _mocks.VerifyAll();
        }
        #endregion
        public static IEnumerable<TestCaseData> ParseAndInsertSampleDataTestData
        {
            get
            {

                yield return new TestCaseData("{CODE name/}", "DATA", new[] { new Sample("CODE", "name", "") });
                yield return new TestCaseData("{HTML name/}", "DATA", new[] { new Sample("HTML", "name", "") });
                yield return new TestCaseData("{CODE name}", "DATA", new[] { new Sample("CODE", "name", "") });
                yield return new TestCaseData("{CODE name /}", "DATA", new[] { new Sample("CODE", "name", "") });
                yield return new TestCaseData("{CODE name }", "DATA", new[] { new Sample("CODE", "name", "") });
                yield return new TestCaseData("{XML @../file.xml /}", "DATA", new[] { new Sample("XML", "", "../file.xml") });
                yield return new TestCaseData("{XML @../file.xml }", "DATA", new[] { new Sample("XML", "", "../file.xml") });
                yield return new TestCaseData("{CODE name@file.cs/}", "DATA", new[] { new Sample("csharp", "name", "file.cs") });
                yield return new TestCaseData("{CODE name@file.cs}", "DATA", new[] { new Sample("csharp", "name", "file.cs") });
                yield return new TestCaseData("{CODE name @ file.cs/}", "DATA", new[] { new Sample("csharp", "name", "file.cs") });
                yield return new TestCaseData("{CODE name @ file.cs}", "DATA", new[] { new Sample("csharp", "name", "file.cs") });
                yield return new TestCaseData(@"{CODE name/}

Some other data

  {CODE name2 /}

Some other data", @"DATA

Some other data

DATA

Some other data", new[] { new Sample("CODE", "name", ""), new Sample("CODE", "name2", "") });
                yield return new TestCaseData(@"{CODE name}

Some other data

  {CODE name2 }

Some other data", @"DATA

Some other data

DATA

Some other data", new[] { new Sample("CODE", "name", ""), new Sample("CODE", "name2", "") });
                yield return new TestCaseData(@"{$type: ""ImageVault.Common.Data.ThumbnailFormat, ImageVault.Common""}", @"{$type: ""ImageVault.Common.Data.ThumbnailFormat, ImageVault.Common""}", null);
                yield return new TestCaseData("{CODE @mediareference.cshtml}", "DATA", new[] { new Sample("razor", "", "mediareference.cshtml") });

            }
        }
        #region public void ParseAndInsertSampleDataTest(string data, string expected, IEnumerable<Sample> samples)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="expected"></param>
        /// <param name="samples"></param>
        [TestCaseSource(nameof(ParseAndInsertSampleDataTestData))]
        public void ParseAndInsertSampleDataTest(string data, string expected, IEnumerable<Sample> samples)
        {
            const string replacement = "DATA";
            var fileInfo = new FileInfo("text.markdown");
            var errors = new List<string>();
            IEnumerator<Sample> senu = null;
            if (samples != null)
            {
                senu = samples.GetEnumerator();
                SetupResult.For(_cmdlet.FindSampleData(fileInfo, null, null, null)).IgnoreArguments()
                        .WhenCalled(a =>
                        {
                            if (!senu.MoveNext())
                            {
                                errors.Add("Another request to FindSampleDataWas not expected");
                                return;
                            }

                            var sample = senu.Current;
                            var n = a.Arguments[1] as string;
                            var p = a.Arguments[2] as string;
                            var t = a.Arguments[3] as string;
                            if (!string.Equals(sample.Type, t))
                                errors.Add("Error parsing type. Expected:" + sample.Type + " Actual:" + t);
                            if (!string.Equals(sample.Name, n))
                                errors.Add("Error parsing Name. Expected:" + sample.Name + " Actual:" + n);
                            if (!string.Equals(sample.Path, p))
                                errors.Add("Error parsing Path. Expected:" + sample.Path + " Actual:" + p);

                        })
                        .Return(replacement);
            }

            _mocks.ReplayAll();
            var actual = _cmdlet.ParseAndInsertSampleData(data, fileInfo);

            if (samples != null)
            {
                if (senu.MoveNext())
                    errors.Add("Expected atleast one more FindSampleDataCall");
                if (errors.Count > 0)
                {
                    Assert.Fail("Errors occured: " + string.Join(",", errors.ToArray()));
                }
            }
            Assert.AreEqual(expected, actual, "Mismatch in actual");

            _mocks.VerifyAll();
        }
        #endregion
        [TestCase("{#T:ImageVault.Client.IVClient}", "Help/html/ImageVault.Client.IVClient.html", "Help/html")]
        [TestCase("{#ImageVault.Client.IVClient}", "Help/html/ImageVault.Client.IVClient.html", "Help/html")]
        //[TestCase("{#AllMembers:T:ImageVault.Client.IVClient}", "html/ImageVault.Client.IVClient.html", "html")]
        [TestCase("{#P:ImageVault.Client.IVClient.PublishIdentifier}", "ImageVault.Client.IVClient.html#ImageVault_Client_IVClient_PublishIdentifier", "")]
        [TestCase("{#N:ImageVault.Client}", "Help/html/ImageVault.Client.html", "Help/html/")]
        [TestCase("{#ImageVault.Client}", "Help/html/ImageVault.Client.html", "Help/html/")]
        [TestCase("{#M:ImageVault.Client.IIVClient.Query`1}", "Help/html/ImageVault.Client.IIVClient.html#ImageVault_Client_IIVClient_Query__1", "Help/html/")]
        [TestCase(@"{$type: ""ImageVault.Common.Data.ThumbnailFormat, ImageVault.Common"")}", @"{$type: ""ImageVault.Common.Data.ThumbnailFormat, ImageVault.Common"")}", "")]
        public void ParseAndInsertApiReferenceLinksTest(string data, string expected, string relPath)
        {
            _mocks.ReplayAll();
            var actual = _cmdlet.ParseAndInsertApiReferenceLinks(data, relPath);
            Assert.AreEqual(expected, actual, "Mismatch in actual");

            _mocks.VerifyAll();
        }
        [TestCase(@"c:/path/html", @"c:\path\file.xml", @"html", null)]
        [TestCase(@"c:/path/html", @"c:\path\another folder\file.xml", @"../html", null)]
        [TestCase(@"c:/path/html", @"c:\file.xml", @"path/html", null)]
        [TestCase(@"c:/path/html", @"d:\file.xml", @"c:/path/html", null)]
        [TestCase(@"path\html", @"d:\file.xml", @"path/html", typeof(ArgumentException))]
        public void CalculateRelativePathTest(string path, string filePath, string expected, Type expectedException)
        {

            _mocks.ReplayAll();
            EvaluateExceptedException(() =>
            {
                var actual = _cmdlet.CalculateRelativePath(path, new FileInfo(filePath));
                Assert.AreEqual(expected, actual, "Mismatch in actual");

            }, expectedException);

            _mocks.VerifyAll();
        }

        private void EvaluateExceptedException(Action test, Type expectedException)
        {
            try
            {
                test();
                if (expectedException != null)
                {
                    Assert.Fail("Expected an exception of type " + expectedException.Name);
                }
            }
            catch (Exception e)
            {
                if (expectedException == null)
                {
                    throw;
                }
                Assert.AreEqual(expectedException, e.GetType(), "Expected exception of wrong type");
            }

        }
        public struct Sample
        {
            public string Type;
            public string Name;
            public string Path;
            public Sample(string type, string name, string path)
            {
                Type = type;
                Name = name;
                Path = path;
            }
        }
        #region public void ConvertToTypeTest(string type, string content, string expected)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="content"></param>
        /// <param name="expected"></param>
        [TestCase("CODE", @"public string MyMethod(){
	Test
}",
@"<pre class=""brush: csharp"">public string MyMethod(){
	Test
}
</pre>")]
        [TestCase("XML", @"<?xml?>
<root>
	<data attr=""value""/>
</root>",
@"<pre class=""brush: xml"">&lt;?xml?&gt;
&lt;root&gt;
	&lt;data attr=&quot;value&quot;/&gt;
&lt;/root&gt;
</pre>")]
        [TestCase("razor", @"//comment",
@"<pre class=""brush: razor"">//comment
</pre>")]
        public void ConvertToTypeTest(string type, string content, string expected)
        {
            _mocks.ReplayAll();
            var actual = _cmdlet.ConvertToType(type, content);
            Assert.AreEqual(StringUtil.FixLineEndings(expected), StringUtil.FixLineEndings(actual), "Mismatch in actual");
            _mocks.VerifyAll();
        }
        #endregion
        #region public void FixIndentationTest(string data, object expected)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="expected"></param>
        [TestCase(@"
    Line after 5

  Line after 2
", @"
  Line after 5

Line after 2
", Description = "Whitespaces in same document")]
        [TestCase(@"
		Line after 2 tabs

			Line after 3 tabs
", @"
Line after 2 tabs

	Line after 3 tabs
", Description = "Tabs in document")]
        [TestCase(@"
   Line after 3 spaces
	Line after 1 tab
		Line after 2 tabs
", @"
Line after 3 spaces
 Line after 1 tab
 	Line after 2 tabs
", Description = "Mixed tabs and spaces. Keep tabs if possible, change vs 4 spaces if needed.")]
        public void FixIndentationTest(string data, string expected)
        {

            _mocks.ReplayAll();
            var actual = _cmdlet.FixIndentation(data);
            Assert.AreEqual(StringUtil.FixLineEndings(expected), StringUtil.FixLineEndings(actual), "Mismatch in actual");

            _mocks.VerifyAll();
        }
        #endregion

        [TestCase("code", "test.cs", "csharp")]
        [TestCase("code", "test.cshtml", "razor")]
        [TestCase("code", "test.html", "html")]
        [TestCase("code", "test.js", "js")]
        [TestCase("code", "test.xml", "xml")]
        [TestCase("xml", "test.xml", "xml")]
        [TestCase("xml", "test.readme", "xml")]
        [TestCase("html", "test.readme", "html")]
        [TestCase("html", "test.html", "html")]
        public void ExpandCodeTypeFromPathTest(string type, string path, string expectedType)
        {
            var actual = ImportMarkdownSamplesCmdlet.ExpandCodeTypeFromPath(type, path);
            Assert.AreEqual(expectedType, actual, "Mismatch in actual");

        }
    }
}
