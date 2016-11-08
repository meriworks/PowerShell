using System;
using System.Configuration;
using System.IO;
using NUnit.Framework;

namespace Meriworks.PowerShell.Tests.Cmdlets {
    /// <summary>
    /// Summary description for ConvertMarkdownToHtmlCmdletTest
    /// </summary>
    /// <remarks>
    /// 2011-12-09 dan: Created
    /// </remarks>
    public partial class ConvertMarkdownToHtmlCmdletTest {
		#region public void CreateDocListTest(string file, string startPath, string doclist, string parentDocList, string expected)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="file"></param>
		/// <param name="startPath"></param>
		/// <param name="doclist"></param>
		/// <param name="parentDocList"></param>
		/// <param name="expected"></param>
		[TestCase("myPath.html", null, @"
myPath.html My Path
;Commented row
Folder/ folder
Folder%202/item.html item", null,
@"<ul>
	<li class=""selected""><a href=""myPath.html"">My Path</a></li>
	<li><a href=""Folder/"">folder</a></li>
	<li><a href=""Folder%202/item.html"">item</a></li>
</ul>
")]
		[TestCase("mypath.html", null, @"
myPath.html My Path
;Commented row
Folder/ folder
Folder%202/item.html item", null,
@"<ul>
	<li class=""selected""><a href=""myPath.html"">My Path</a></li>
	<li><a href=""Folder/"">folder</a></li>
	<li><a href=""Folder%202/item.html"">item</a></li>
</ul>
")]
		[TestCase("wtf.html", null, @"
myPath.html My Path
;Commented row
Folder/ folder
Folder%202/item.html item", null,
@"<ul>
	<li><a href=""myPath.html"">My Path</a></li>
	<li><a href=""Folder/"">folder</a></li>
	<li><a href=""Folder%202/item.html"">item</a></li>
</ul>
")]
		[TestCase("Folder 2/item.html", null, @"
myPath.html My Path
;Commented row
Folder/ folder
Folder%202/item.html item", null,
@"<ul>
	<li><a href=""../myPath.html"">My Path</a></li>
	<li><a href=""../Folder/"">folder</a></li>
	<li class=""selected""><a href=""../Folder%202/item.html"">item</a></li>
</ul>
")]
		[TestCase("myPath.html", "Folder", @"
myPath.html My Path
;Commented row
Folder/ folder
Folder%202/item.html item", @"
Folder/ folder
Another%20Folder/myPath.html item
",
@"<ul>
	<li><a href=""../Folder/"">folder</a>
		<ul>
			<li class=""selected""><a href=""myPath.html"">My Path</a></li>
			<li><a href=""Folder/"">folder</a></li>
			<li><a href=""Folder%202/item.html"">item</a></li>
		</ul>
	</li>
	<li><a href=""../Another%20Folder/myPath.html"">item</a></li>
</ul>
")]
		[TestCase("Folder/index.html", null, @"
Folder/ item", null,
@"<ul>
	<li class=""selected""><a href=""../Folder/"">item</a></li>
</ul>
")]
		[TestCase("setup.html", "CSharp", @"
setup.html Requirements and setup
Configuration/Configuration.html Configuration
services.html Services
linqQueryProvider.html Linq query provider
Examples/ Examples
", @"
;overview.html Overview
CSharp/ C# client API
;REST/ REST client API
Reference/ APi Reference
", @"<ul>
	<li><a href=""../CSharp/"">C# client API</a>
		<ul>
			<li class=""selected""><a href=""setup.html"">Requirements and setup</a></li>
			<li><a href=""Configuration/Configuration.html"">Configuration</a></li>
			<li><a href=""services.html"">Services</a></li>
			<li><a href=""linqQueryProvider.html"">Linq query provider</a></li>
			<li><a href=""Examples/"">Examples</a></li>
		</ul>
	</li>
	<li><a href=""../Reference/"">APi Reference</a></li>
</ul>
")]
		[TestCase("QueryMedia.html", "Querying MediaItems", @"
Conversions/ Conversions
Metadata/ Metadata
", @"
Querying%20MediaItems/QueryMedia.html Query media
Upload.html Upload media
QueryVault.html Query Vault
QueryMetadataDefinition.html Query Metadata definitions", @"<ul>
	<li class=""selected""><a href=""../Querying%20MediaItems/QueryMedia.html"">Query media</a>
		<ul>
			<li><a href=""Conversions/"">Conversions</a></li>
			<li><a href=""Metadata/"">Metadata</a></li>
		</ul>
	</li>
	<li><a href=""../Upload.html"">Upload media</a></li>
	<li><a href=""../QueryVault.html"">Query Vault</a></li>
	<li><a href=""../QueryMetadataDefinition.html"">Query Metadata definitions</a></li>
</ul>
")]
		[TestCase("myPath.html", "Folder", null, null, null)]
		public void CreateDocListTest(string file, string startPath, string doclist, string parentDocList, string expected) {

			var fileInfo = new FileInfo(Path.Combine(startPath ?? ".", file));
			var dir = new DirectoryInfo(startPath ?? ".");
			if (!dir.Exists)
				dir.Create();
			//if the files directory exists, make sure that it don't contain a doclist file
			if (fileInfo.Directory.Exists)
				WriteDocListFile(null, fileInfo.Directory);
			WriteDocListFile(doclist, dir);
			WriteDocListFile(parentDocList, dir.Parent);
			//remove parent parent doclist
			if (dir.Parent != null) {
				WriteDocListFile(null, dir.Parent.Parent);
			}

			_mocks.ReplayAll();
			var actual = task.CreateDocList(fileInfo);
			Assert.AreEqual(expected, actual, "Mismatch in actual");

			_mocks.VerifyAll();
		}
		#endregion
		[TestCase("Metadata/", "Metadata/", true, true)]
		[TestCase("Metadata/", "Metadata\\index.html", true, true)]
		[TestCase("Metadata/", "Metadata\\another.html", true, true)]
		[TestCase("Metadata/another.html", "Metadata\\another.html", true, true)]
		[TestCase("Metadata/another.html", "Metadata\\another2.html", true, true)]
		[TestCase("Metadata/another.html", "Metadata2\\another2.html", true, false)]
		[TestCase("Metadata/another.html", "Metadata\\another.html", false, true)]
		[TestCase("Metadata/another.html", "Metadata\\another2.html", false, false)]
		[TestCase("Metadata/another.html", "Metadata2\\another2.html", false, false)]
		[TestCase("Metadata/", "Metadata\\index.html", false, true)]
		[TestCase("Metadata/MyFile.html", "Metadata\\anotherFolder\\index.html", true, true)]
		public void PathMatchesFileTest(string path, string filepath, bool ignoreFile, bool expected) {

			_mocks.ReplayAll();
			Assert.AreEqual(expected,task.PathMatchesFile(path,filepath,ignoreFile));
			_mocks.VerifyAll();
		}
        [TestCase("doclist/_.doclist")]
		public void RealDocListTest(string name)
        {
            var assetsDirectory = new DirectoryInfo(ConfigurationManager.AppSettings["assetsPath"]);
            
			var file = new FileInfo(Path.Combine(assetsDirectory.FullName,name));

			_mocks.ReplayAll();
			Assert.IsTrue(file.Exists);
			var actual = task.CreateDocList(file);
			Console.WriteLine(actual);
			_mocks.VerifyAll();
		}
		#region private static void WriteDocListFile(string doclist, DirectoryInfo dir)
		/// <summary>
		/// Writes a .doclist to a specific directory 
		/// </summary>
		/// <param name="doclist">The content of the .doclist file or null if any existing .doclist file should be removed</param>
		/// <param name="dir">The <see cref="DirectoryInfo"/> to create/remove the .doclist file in.</param>
		private static void WriteDocListFile(string doclist, DirectoryInfo dir) {
            var doclistFile = new FileInfo(Path.Combine(dir.FullName, ".doclist"));
            //delete both docklists
            if (doclistFile.Exists) {
                doclistFile.Delete();
            }
            doclistFile = new FileInfo(Path.Combine(dir.FullName, "_.doclist"));
            if (doclistFile.Exists) {
                doclistFile.Delete();
            }
            doclistFile.Refresh();

			if (doclist != null) {
				using (var w = new StreamWriter(doclistFile.Open(FileMode.Create))) {
					w.Write(doclist);
				}
				//delete parent doclist
				WriteDocListFile(null, dir.Parent);
            }
        }
		#endregion
	}
}