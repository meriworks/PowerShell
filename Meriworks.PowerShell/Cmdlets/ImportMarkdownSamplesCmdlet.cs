using System;
using System.IO;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Meriworks.PowerShell.Cmdlets {
    /// <summary>
    /// Summary description for ImportMarkdownSamplesCmdlet.
    /// </summary>
    [Cmdlet(VerbsData.Import, "MarkdownSamples", SupportsShouldProcess = true)]
    public class ImportMarkdownSamplesCmdlet : Cmdlet {
        /* *******************************************************************
         *  Properties 
         * *******************************************************************/
        #region public string Filename
        /// <summary>
        /// Get/Sets the File of the ImportMarkdownSamplesCmdlet
        /// </summary>
        /// <value></value>
        [Alias("f")]
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string Filename { get; set; }
        #endregion
        #region public string RootPath
        /// <summary>
        /// Get/Sets the RootPath of the ImportMarkdownSamplesCmdlet
        /// </summary>
        /// <value></value>
        [Alias("r")]
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string RootPath { get; set; }
        #endregion
        #region public string HtmlHelpPath
        /// <summary>
        /// Get/Sets the HtmlHelpPath of the ImportMarkdownSamplesCmdlet
        /// </summary>
        /// <value></value>
        [Alias("h")]
        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true)]
        public string HtmlHelpPath { get; set; }
        #endregion

        /* *******************************************************************
         *  Methods 
         * *******************************************************************/
        protected override void ProcessRecord() {

            WriteVerbose("markdownInsertSamples for file " + Filename + " started.");
            var file = new FileInfo(Filename);
            if (!file.Exists)
                throw new FileNotFoundException(string.Format("File {0} is missing", Filename), Filename);
            string data;
            using (var fs = new StreamReader(file.OpenRead())) {
                data = fs.ReadToEnd();
            }
            var newData = ParseAndInsertSampleData(data, file);
            var relPath = CalculateRelativePath(Path.Combine(RootPath, HtmlHelpPath), file);
            newData = ParseAndInsertApiReferenceLinks(newData, relPath);
            if (string.Equals(data, newData)) {
                WriteVerbose("No replacements where made");
                return;
            }
            using (var fs = new StreamWriter(file.Open(FileMode.Truncate))) {
                fs.Write(newData);
            }
            WriteVerbose("Saved changes");

        }
        #region internal string CalculateRelativePath(string path, FileInfo relFile)
        /// <summary>
        /// Calculates the relative path from the files folder to the supplied path
        /// </summary>
        /// <param name="path">The path to calculate the relative path to</param>
        /// <param name="relFile">The file to calculate the path from.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        internal string CalculateRelativePath(string path, FileInfo relFile) {
            if (!Path.IsPathRooted(path))
                throw new ArgumentException("Cannot calculate relative path on a non rooted path", "path");
            var root = new Uri(path);
            var file = new Uri(relFile.FullName);
            return file.MakeRelativeUri(root).ToString();
        }
        #endregion

        #region internal virtual void Warn(string message)
        /// <summary>
        /// Writes a Warn message to the log
        /// </summary>
        /// <param name="message"></param>
        internal virtual void Warn(string message) {
            WriteWarning(message);
        }
        #endregion
        internal string ParseAndInsertApiReferenceLinks(string data, string htmlRelPath) {

            if (string.IsNullOrEmpty(htmlRelPath)) {
                htmlRelPath = string.Empty;
            } else if (!htmlRelPath.EndsWith("/")) {
                htmlRelPath = htmlRelPath + "/";
            }
            const string pattern = @"
\{\s*
\#\s*
(?<name>[^\}]+?)
\s*
/?}";
            return Regex.Replace(data, pattern, m => {
                try {
                    var name = m.Groups["name"].Value.Trim();
                    name = Regex.Replace(name, @"[\:\.\+]", "_");
                    name = Regex.Replace(name, @"[\`]", "__");
                    return htmlRelPath + name + ".htm";
                } catch (Exception e) {
                    var message = string.Format(@"<pre style=""color:red;weight:bold;"">Cannot calculate APiReferenceLink with name {0}: {1}</pre>", m.Groups["name"].Value, e.Message);
                    Warn(message);
                    return message;
                }
            }, RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        }

        #region internal string ParseAndInsertSampleData(string data, FileInfo source)
        /// <summary>
        /// Parses the data and replaces sample references with their sample data (or an error message)
        /// </summary>
        /// <param name="data">The data to parse</param>
        /// <param name="source">The <see cref="FileInfo"/> that contained the data</param>
        /// <returns>The data with replacements</returns>
        internal string ParseAndInsertSampleData(string data, FileInfo source) {
            //pattern to search for {CODE|XML [name][@path] /}
            const string pattern = @"
^[^\S\012\015]*\{
(:?(?<type>CODE|XML|HTML)
\s+)
(?<name>[^@]+?)?
(:?@
(?<path>.+?))?
\s*
/?}[^\S\012\015]*";
            return Regex.Replace(data, pattern, m => {
                try {
                    var type = m.Groups["type"].Value;
                    var path = m.Groups["path"].Value.Trim();
                    var name = m.Groups["name"].Value.Trim();
                    type = ExpandCodeTypeFromPath(type, path);
                    return FindSampleData(source, name, path, type);
                } catch (Exception e) {
                    var message = string.Format(@"<pre style=""color:red;weight:bold;"">Cannot get file contents while parsing markdown file {0}: {1}</pre>", source.FullName, e.Message);
                    Warn(message);
                    return message;
                }
            }, RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        }
        internal static string ExpandCodeTypeFromPath(string type, string path) {
            if (string.IsNullOrEmpty(path))
                return type;
            //only expand type on code types
            if (!string.Equals(type, "code", StringComparison.CurrentCultureIgnoreCase)) return type;
            var lastIndexOf = path.LastIndexOf('.');
            //not found or last character
            if (lastIndexOf < 0 || lastIndexOf == path.Length - 1)
                return null;
            var extension = path.Substring(lastIndexOf + 1).ToLower();
            switch (extension) {
                case "cs": return "csharp";
                case "cshtml": return "razor";
                default: return extension;
            }
        }
        #endregion
        #region internal virtual string FindSampleData(FileInfo referee, string name, string path, string type)
        /// <summary>
        /// Finds the requested sample data 
        /// </summary>
        /// <param name="referee">The file that contained the reference</param>
        /// <param name="name">The name of the reference</param>
        /// <param name="path">The path of the reference</param>
        /// <param name="type">The type of the reference</param>
        /// <returns></returns>
        internal virtual string FindSampleData(FileInfo referee, string name, string path, string type) {
            path = CalculateAbsolutePath(referee, path);
            var content = GetFileContents(name, path);
            return ConvertToType(type, content);
        }
        #endregion
        #region internal virtual string ConvertToType(string type, string data)
        /// <summary>
        /// Converts the sample to a valid html of the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">If type .</exception>
        internal virtual string ConvertToType(string type, string data) {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentNullException("type");

            var brushType = type.ToLower();
            switch (brushType) {
                case "code":
                    brushType = "csharp";
                    break;
            }
            data = FixIndentation(data);
            data = HttpUtility.HtmlEncode(data);

            return string.Format(@"<pre class=""brush: " + brushType + @""">{0}</pre>", data);
        }
        #endregion
        #region internal virtual string FixIndentation(string data)
        /// <summary>
        /// Fixes the indentation of the sample so the paragraph is deindented to the left border.
        /// </summary>
        /// <param name="data">The data to fix indentation for</param>
        /// <returns>The indented data</returns>
        internal virtual string FixIndentation(string data) {
            //count min no of leading spaces, skip blank lines, count tab as 4 spaces
            var leadingSpaces = CountMinNoOfLeadingSpaces(data);
            //Remove the number of blanks from each row
            return RemoveNoOfLeadingSpacesFromEachLine(data, leadingSpaces);
        }
        #endregion
        #region private static string RemoveNoOfLeadingSpacesFromEachLine(string data, int count)
        /// <summary>
        /// Removes the number of leading spaces/tabs (counts as 4) from each line
        /// </summary>
        /// <param name="data">The text where to remove leading whitespaces from</param>
        /// <param name="count">The number of spaces to remove</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException">If cannot remove char .</exception>
        private static string RemoveNoOfLeadingSpacesFromEachLine(string data, int count) {
            var sb = new StringBuilder();
            using (var r = new StringReader(data)) {
                string line;
                while ((line = r.ReadLine()) != null) {
                    var pos = 0;
                    var c = 0;
                    while (c < count && pos < line.Length) {
                        if (line[pos] == ' ') {
                            c++;
                        } else if (line[pos] == '\t') {
                            c += 4;
                        } else {
                            throw new ApplicationException("Cannot remove char " + pos + " (" + line[pos] + ") in line " + line);
                        }
                        pos++;
                    }
                    line = line.Substring(pos);
                    if (c > count) {
                        line = line.PadLeft(line.Length + c - count);
                    }
                    sb.AppendLine(line);
                }
            }
            return sb.ToString();
        }
        #endregion
        #region private static int CountMinNoOfLeadingSpaces(string data)
        /// <summary>
        /// Counts the minimum number of leading spaces (ignoring blank lines)
        /// </summary>
        /// <param name="data">The text to parse</param>
        /// <returns></returns>
        private static int CountMinNoOfLeadingSpaces(string data) {
            var minSpaces = int.MaxValue;
            using (var r = new StringReader(data)) {
                string line;
                while ((line = r.ReadLine()) != null) {
                    //skip blanks
                    if (line.Length == 0 || Regex.IsMatch(line, @"^\s*$")) {
                        continue;
                    }
                    var spacesFound = 0;
                    var pos = 0;
                    var done = false;
                    do {
                        switch (line[pos]) {
                            case ' ':
                                spacesFound++;
                                break;
                            case '\t': //count tabs as 4 spaces
                                spacesFound += 4;
                                break;
                            default:
                                done = true;
                                break;
                        }
                        if (done)
                            break;
                        pos++;
                    } while (pos < line.Length);

                    if (minSpaces < spacesFound) {
                        continue;
                    }
                    minSpaces = spacesFound;
                }
            }
            return minSpaces;
        }
        #endregion
        #region internal virtual string CalculateAbsolutePath(FileInfo sourceFile, string path)
        /// <summary>
        /// Calculates the Absolute path of the requested parameters
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        internal virtual string CalculateAbsolutePath(FileInfo sourceFile, string path) {
            if (sourceFile == null) {
                throw new ArgumentNullException("sourceFile");
            }

            if (string.IsNullOrEmpty(path)) {
                path = Path.ChangeExtension(sourceFile.FullName, ".cs");
            } else {
                if (sourceFile.Directory == null)
                    throw new ApplicationException("sourceFile.Directory is null, " + sourceFile.FullName);
                if (!Path.IsPathRooted(path)) {
                    path = Path.GetFullPath(Path.Combine(sourceFile.Directory.FullName, path));
                }
            }
            return path;
        }
        #endregion
        #region internal virtual string GetFileContents(string name, string path)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException">If cannot find file .</exception>
        /// <exception cref="Exception">If cannot find region .</exception>
        internal virtual string GetFileContents(string name, string path) {
            if (!File.Exists(path)) {
                throw new ApplicationException("Cannot find file " + path + " to get contents from");
            }
            using (var fs = new StreamReader(File.OpenRead(path))) {
                var data = fs.ReadToEnd();
                if (!string.IsNullOrEmpty(name)) {
                    try {
                        return FindRegionInData(data, name);
                    } catch (Exception e) {
                        throw new Exception("Cannot find region " + name + " in file " + path, e);
                    }
                }
                return data;
            }
        }
        #endregion
        #region internal virtual string FindRegionInData(string data, string name)
        /// <summary>
        /// Finds a named region in the data
        /// </summary>
        /// <param name="data">The data to search for the region in </param>
        /// <param name="name">The name of the region</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException">If cannot find region .</exception>
        internal virtual string FindRegionInData(string data, string name) {
            var pattern = @"
^\s*\#region\s*" + Regex.Escape(name) + @"\s*$
(.*?)$
\s*\#endregion
";
            var match = Regex.Match(data, pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Singleline);
            if (!match.Success) {
                throw new ApplicationException("Cannot find region " + name + " in data");
            }

            var value = match.Groups[1].Value;
            value = value.Trim('\n', '\r');
            return value;
        }
        #endregion

    }
}