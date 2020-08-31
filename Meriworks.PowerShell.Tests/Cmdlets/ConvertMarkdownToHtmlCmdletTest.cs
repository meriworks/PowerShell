using System.Collections.Generic;
using System.IO;
using Meriworks.PowerShell.Cmdlets;
using NUnit.Framework;
using Rhino.Mocks;

namespace Meriworks.PowerShell.Tests.Cmdlets {
	/// <summary>
	/// Summary description for MarkdownConvertToHtmlTest.
	/// </summary>
	/// <remarks>
	/// 2011-11-18 dan: Created
	/// </remarks>
	/// <example></example>
	[TestFixture]
    public partial class ConvertMarkdownToHtmlCmdletTest {
		/* *******************************************************************
		 *  SetupMethods 
		 * *******************************************************************/
		private MockRepository _mocks;
        private ConvertMarkdownToHtmlCmdlet task;
		#region public void SetUp()
		/// <summary>
		/// Performs setup before each test
		/// </summary>
		[SetUp]
		public void SetUp() {
			_mocks = new MockRepository();
            task = _mocks.PartialMock<ConvertMarkdownToHtmlCmdlet>();
		}
		#endregion

		/* *******************************************************************
		 *  Test Methods 
		 * *******************************************************************/
		#region public IEnumerable<TestCaseData> FillTemplateTestData
		/// <summary>
		/// Gets the FillTemplateTestData of the MarkdownConvertToHtmlTest
		/// </summary>
		/// <value></value>
		public static IEnumerable<TestCaseData> FillTemplateTestData {
			get {
				yield return new TestCaseData(@"<html><head><title>${title}</title></head><body>${content}</body></html>", new Dictionary<string, string> { { "title", "MyTitle" }, { "content", "MyContent" } }, @"<html><head><title>MyTitle</title></head><body>MyContent</body></html>");
				yield return new TestCaseData(@"<html>
  <head>
    <title>${title}</title>
  </head>
  <body>
    ${content}
  </body>
</html>", new Dictionary<string, string> { { "title", "MyTitle" }, { "content", "MyContent" } }, @"<html>
  <head>
    <title>MyTitle</title>
  </head>
  <body>
    MyContent
  </body>
</html>");
				yield return new TestCaseData(@"some data ${title} ${nonExisting}", new Dictionary<string, string> { { "title", "MyTitle" }, { "content", "MyContent" } },
@"some data MyTitle Cannot find value for token 'nonExisting'");
                yield return new TestCaseData(@"{$type: ""ImageVault.Common.Data.ThumbnailFormat, ImageVault.Common""}", null, @"{$type: ""ImageVault.Common.Data.ThumbnailFormat, ImageVault.Common""}");

				yield break;
			}
		}
		#endregion
		#region public void FillTemplateTest(string templateData, Dictionary<string, string> dictionary, string expected)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="templateData"></param>
		/// <param name="dictionary"></param>
		/// <param name="expected"></param>
		[TestCaseSource(nameof(FillTemplateTestData))]
		public void FillTemplateTest(string templateData, Dictionary<string, string> dictionary, string expected) {

			_mocks.ReplayAll();
			var actual = task.FillTemplate(templateData, dictionary);
			Assert.AreEqual(expected, actual, "Mismatch in actual");

			_mocks.VerifyAll();
		}
		#endregion
		#region public void FindTemplateDataTest(string templateData, string childTemplateData, string grandChildTemplateData)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="templateData"></param>
		/// <param name="childTemplateData"></param>
		/// <param name="grandChildTemplateData"></param>
		[TestCase(@"<html>
<head>
	<title>${title}</title>
	<script type=""text/javascript"" src=""js/shCore.js""></script>
	<script type=""text/javascript"" src=""http://server/js/shCore.js""></script>
	<script type=""text/javascript"" src='/js/shCore.js'></script>
	<link href=""css/shCore.css"" rel=""stylesheet"" type=""text/css"" />
	<link href=""http://server/css/shCore.css"" rel=""stylesheet"" type=""text/css"" />
	<link href=""/css/shCore.css"" rel=""stylesheet"" type=""text/css"" />
</head>
<body>
<a href=""muPage.html""><img src=""myImg.jpg""/></a>
<a href=""http://server/muPage.html""><img src=""file://server/myImg.jpg""/></a>
<a href=""/muPage.html""><img src=""/absolute/path/to/img.png""/></a>
</body>
</html>",
		@"<html>
<head>
	<title>${title}</title>
	<script type=""text/javascript"" src=""../js/shCore.js""></script>
	<script type=""text/javascript"" src=""http://server/js/shCore.js""></script>
	<script type=""text/javascript"" src='/js/shCore.js'></script>
	<link href=""../css/shCore.css"" rel=""stylesheet"" type=""text/css"" />
	<link href=""http://server/css/shCore.css"" rel=""stylesheet"" type=""text/css"" />
	<link href=""/css/shCore.css"" rel=""stylesheet"" type=""text/css"" />
</head>
<body>
<a href=""../muPage.html""><img src=""../myImg.jpg""/></a>
<a href=""http://server/muPage.html""><img src=""file://server/myImg.jpg""/></a>
<a href=""/muPage.html""><img src=""/absolute/path/to/img.png""/></a>
</body>
</html>", @"<html>
<head>
	<title>${title}</title>
	<script type=""text/javascript"" src=""../../js/shCore.js""></script>
	<script type=""text/javascript"" src=""http://server/js/shCore.js""></script>
	<script type=""text/javascript"" src='/js/shCore.js'></script>
	<link href=""../../css/shCore.css"" rel=""stylesheet"" type=""text/css"" />
	<link href=""http://server/css/shCore.css"" rel=""stylesheet"" type=""text/css"" />
	<link href=""/css/shCore.css"" rel=""stylesheet"" type=""text/css"" />
</head>
<body>
<a href=""../../muPage.html""><img src=""../../myImg.jpg""/></a>
<a href=""http://server/muPage.html""><img src=""file://server/myImg.jpg""/></a>
<a href=""/muPage.html""><img src=""/absolute/path/to/img.png""/></a>
</body>
</html>"
			)]
		public void FindTemplateDataTest(string templateData, string childTemplateData, string grandChildTemplateData) {
			var basePath = Directory.CreateDirectory("path1");
            var template = new FileInfo(Path.Combine(basePath.FullName, ConvertMarkdownToHtmlCmdlet.DefaultTemplateFile));
			using (var w = new StreamWriter(template.OpenWrite())) {
				w.Write(templateData);
			}
			var childPath = Directory.CreateDirectory("path1/path2");
			var grandChildPath = Directory.CreateDirectory("path1/path2/paht3");
			_mocks.ReplayAll();
			var actual = task.FindTemplateData(basePath);
			Assert.AreEqual(templateData, actual, "Mismatch in rootPath templateData");

			actual = task.FindTemplateData(childPath);
			Assert.AreEqual(childTemplateData, actual, "Mismatch in rootPath templateData");
			actual = task.FindTemplateData(grandChildPath);
			Assert.AreEqual(grandChildTemplateData, actual, "Mismatch in rootPath templateData");

			_mocks.VerifyAll();

		}
		#endregion

	}
}
