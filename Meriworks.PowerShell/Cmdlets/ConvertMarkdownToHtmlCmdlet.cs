using Meriworks.Markdown;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Meriworks.PowerShell.Cmdlets {
    /// <summary>
    /// Summary description for ConvertMarkdownToHtmlCmdlet.
    /// </summary>
    [Cmdlet(VerbsData.Convert, "MarkdownToHtml", SupportsShouldProcess = true)]
    public class ConvertMarkdownToHtmlCmdlet : Cmdlet {
        /* *******************************************************************
         *  Properties 
         * *******************************************************************/
        internal const string DefaultTemplateFile = "markdownTemplate.html";

        #region public string InputFile
        /// <summary>
        /// Get/Sets the InputFile of the ConvertMarkdownToHtmlCmdlet
        /// </summary>
        /// <value></value>
        [Alias("i")]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string InputFile { get; set; }
        #endregion
        #region public string OutputFile
        /// <summary>
        /// Get/Sets the OutputFile of the ConvertMarkdownToHtmlCmdlet
        /// </summary>
        /// <value></value>
        [Alias("o")]
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string OutputFile { get; set; }
        #endregion
        #region public string TemplateFile
        /// <summary>
        /// Get/Sets the TemplateFile of the ConvertMarkdownToHtmlCmdlet
        /// </summary>
        /// <value></value>
        [Parameter(Mandatory = false, Position = 2, ValueFromPipelineByPropertyName = true)]
        public string TemplateFile { get; set; }
        #endregion
        /* *******************************************************************
         *  Constructors 
         * *******************************************************************/


        /* *******************************************************************
         *  Methods 
         * *******************************************************************/
        protected override void ProcessRecord() {
            WriteVerbose("markdownConvertToHtml for file " + InputFile + " started.");
            var file = new FileInfo(InputFile);
            var outFile = new FileInfo(OutputFile);
            if (!file.Exists)
                throw new FileNotFoundException($"File {InputFile} is missing", InputFile);
            if (outFile.Exists)
                throw new ApplicationException("Outfile " + OutputFile + " exists, cannot continue");

            //read markdown file
            var data = ReadFile(file);
            //convert to html
            var options = new MarkdownOptions { EmptyElementSuffix = "/>" };
            var m = new Markdown.MarkdownParser(options);
            data = m.Transform(data);

            //apply template
            var templateData = FindTemplateData(file.Directory);
            var doclist = CreateDocList(file);
            if (!string.IsNullOrEmpty(templateData)) {
                var dict = new Dictionary<string, string> {
					{"doclist",doclist},
				    {"title", Path.GetFileNameWithoutExtension(InputFile)},
					{"content", data}
				};
                data = FillTemplate(templateData, dict);
            }

            //save html file
            using (var fs = new StreamWriter(outFile.Create())) {
                fs.Write(data);
            }
            WriteVerbose("Saved changes");
        }
        internal string CreateDocList(FileInfo file) {

            var directoryInfo = file.Directory;
            var currentItemRelativePath = file.Name;
            var linkRelativePath = "";
            var selectedPath = file.FullName;
            string navigation = null;
            while (directoryInfo != null) {
                var doclistFile = new FileInfo(Path.Combine(directoryInfo.FullName, ".doclist"));
                if(!doclistFile.Exists)
                    doclistFile = new FileInfo(Path.Combine(directoryInfo.FullName, "_.doclist"));

                //if we have a navigation and the parent doclistfile don't exist, then we are done
                if (navigation != null && !doclistFile.Exists)
                    break;
                if (doclistFile.Exists) {
                    using (var r = new StreamReader(doclistFile.OpenRead())) {
                        var sb = new StringBuilder();
                        sb.AppendLine("<ul>");
                        while (!r.EndOfStream) {
                            var l = r.ReadLine();
                            //skip blanks and commented lines
                            if (string.IsNullOrEmpty(l) || l.StartsWith(";"))
                                continue;
                            var match = Regex.Match(l, @"^(?<path>\S+)\s+(?<name>.+)\s*$", RegexOptions.Compiled);
                            if (!match.Success)
                                throw new ApplicationException("doclist line " + l + " is incorrect");
                            var path = match.Groups["path"].Value;
                            var name = match.Groups["name"].Value;
                            //check if this is the selected item
                            var selected = false;
                            if (selectedPath != null) {
                                selected = PathMatchesFile(path, selectedPath, false);
                                //clear the selected path if found (there can only be one)
                                if (selected)
                                    selectedPath = null;
                            }
                            sb.AppendFormat("\t<li{0}><a href=\"{1}\">{2}</a>",
                                            (selected ? @" class=""selected""" : ""),
                                            linkRelativePath + path,
                                            HttpUtility.HtmlEncode(name));
                            if (navigation != null) {
                                var insertChildList = PathMatchesFile(path, currentItemRelativePath, true);
                                if (insertChildList) {
                                    navigation = Indent(navigation, "\t\t");
                                    sb.Append(Environment.NewLine + navigation + "\t");
                                    navigation = null;
                                }
                            }
                            sb.AppendLine("</li>");
                        }
                        sb.AppendLine("</ul>");
                        navigation = sb.ToString();
                    }
                }
                //if we don't have a parent directory, then we cannot go any further...
                if (directoryInfo.Parent == null)
                    break;
                if (selectedPath != null) {
                    currentItemRelativePath = selectedPath.Substring(directoryInfo.Parent.FullName.Length + 1);
                } else {
                    currentItemRelativePath = directoryInfo.Name + "/";
                }
                linkRelativePath = "../" + linkRelativePath;
                directoryInfo = directoryInfo.Parent;
            }
            return navigation;
        }
        private string Indent(string childList, string indent) {
            return Regex.Replace(childList, "^(.+)$", indent + "$1", RegexOptions.Multiline);
        }
        internal bool PathMatchesFile(string path, string filePath, bool ignoreFile) {
            path = HttpUtility.UrlDecode(path);
            path = path.Replace('/', '\\');
            filePath = filePath.Replace('/', '\\');
            if (ignoreFile) {
                //strip any document
                var p = filePath.LastIndexOf('\\');
                if (p > 0)
                    filePath = filePath.Substring(0, p + 1);
                p = path.LastIndexOf('\\');
                if (p > 0)
                    path = path.Substring(0, p + 1);
            }
            //path can be virtual and start with the same as the filePath
            if (filePath.ToLower().StartsWith(path.ToLower()))
                return true;
            if (filePath.ToLower().EndsWith(path.ToLower()))
                return true;
            if (!ignoreFile) {
                if (path.EndsWith("\\"))
                    path = Path.Combine(path, "index.html");
                if (filePath.ToLower().EndsWith(path.ToLower()))
                    return true;
            }
            return false;
        }
        #region internal string FillTemplate(string templateData, Dictionary<string, string> dict)
        /// <summary>
        /// Fills a template using the given dictionary
        /// </summary>
        /// <param name="templateData"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        internal string FillTemplate(string templateData, Dictionary<string, string> dict) {
            return Regex.Replace(templateData, @"\$\{(?<name>[^\}]+)\}", m => {
                var name = m.Groups[1].Value;
                return dict.ContainsKey(name) ? dict[name] : $@"Cannot find value for token '{name}'";
            });
        }
        #endregion
        #region private static string ReadFile(FileInfo file)
        /// <summary>
        /// Reads a file from disk
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static string ReadFile(FileInfo file) {
            string data;
            using (var fs = new StreamReader(file.OpenRead())) {
                data = fs.ReadToEnd();
            }
            return data;
        }
        #endregion


        #region private string FindTemplateData()
        /// <summary>
        /// Finds the closest markdowntemplate.html in the path
        /// </summary>
        /// <param name="directory">The <see cref="DirectoryInfo"/> to search in</param>
        /// <returns>The template data to use or null if no template was found.</returns>
        internal string FindTemplateData(DirectoryInfo directory) {
            TemplateInfo info = FindTemplateDataInner(directory, 0);
            return info.GetTemplateData();
        }
        private TemplateInfo FindTemplateDataInner(DirectoryInfo directory, int level) {
            try {
                var fileInfo = new FileInfo(Path.Combine(directory.FullName, TemplateFile ?? DefaultTemplateFile));
                if (fileInfo.Exists) {
                    //Log(Level.Info, "Found template " + fileInfo.FullName);
                    return new TemplateInfo(ReadFile(fileInfo), level);
                }
                if (directory.Parent != null) {
                    return FindTemplateDataInner(directory.Parent, level + 1);
                }
            } catch (Exception e) {
                WriteError(new ErrorRecord(e, "Error searching for template data. ", ErrorCategory.ObjectNotFound, TemplateFile ?? DefaultTemplateFile));
            }
            return new TemplateInfo(null, 0);
        }
        #endregion
        #region private struct TemplateInfo
        /// <summary>
        /// Template information
        /// </summary>
        private struct TemplateInfo {
            private readonly string _template;
            private readonly int _level;
            /* *******************************************************************
             *  Constructors
             * *******************************************************************/
            #region public TemplateInfo(string template, int level)
            /// <summary>
            /// Initializes a new instance of the <b>TemplateInfo</b> class.
            /// </summary>
            /// <param name="template"></param>
            /// <param name="level"></param>
            public TemplateInfo(string template, int level) {
                _template = template;
                _level = level;
            }
            #endregion
            /* *******************************************************************
			 *  Methods
			 * *******************************************************************/
            #region public string GetTemplateData()
            /// <summary>
            /// Gets the template data 
            /// </summary>
            /// <returns></returns>
            public string GetTemplateData() {
                //same level, return correct template
                if (_level == 0)
                    return _template;
                var tmp = _template;
                //otherwize, fix links....
                //script src
                tmp = Regex.Replace(tmp, @"<script\b[^>]*\bsrc=(['""])(?<url>[^\1]*?)\1", FixUrl, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                //link href
                tmp = Regex.Replace(tmp, @"<link\b[^>]*\bhref=(['""])(?<url>[^\1]*?)\1", FixUrl, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                //a href
                tmp = Regex.Replace(tmp, @"<a\b[^>]*\bhref=(['""])(?<url>[^\1]*?)\1", FixUrl, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                //img src
                tmp = Regex.Replace(tmp, @"<img\b[^>]*\bsrc=(['""])(?<url>[^\1]*?)\1", FixUrl, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                return tmp;
            }
            #endregion
            #region private string FixUrl(Match m)
            /// <summary>
            /// Fix the matched url according to the level
            /// </summary>
            /// <param name="m"></param>
            /// <returns></returns>
            private string FixUrl(Match m) {
                var g = m.Groups["url"];
                var pre = m.Value.Substring(0, g.Index - m.Index);
                var post = m.Value.Substring(g.Index + g.Length - m.Index);
                var url = g.Value;
                //don't add ../ for absolute url:s
                if (Uri.IsWellFormedUriString(url, UriKind.Relative) && !url.StartsWith("/")) {
                    url = string.Concat(Enumerable.Repeat("../", _level).ToArray()) + url;
                }
                return pre + url + post;
            }
            #endregion
        }
        #endregion
    }
}