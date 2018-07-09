# Meriworks.PowerShell
The sign library adds a PowerShell module for handling various tasks

Is an [Extension Module](https://github.com/meriworks/PowerShell.BuildEvents#Extension_Modules) to the [Meriworks.PowerShell.BuildEvents](https://github.com/meriworks/PowerShell.BuildEvents) .

* [Documentation](#documentation)
* [License](#license)
* [Author](#author)
* [Changelog](#changelog)

<a name="documentation"></a>
## Documentation
The following commands are defined in this module.

* [Invoke-Robocopy](#invoke-robocopy)
* [Invoke-XmlTransform](#invoke-xmltransform)
* [Import-MarkdownSamples](#import-markdownsamples)
* [Convert-MarkdownToHtml](#convert-markdowntohtml)
* [Set-AssemblyInfoVersion](#set-assemblyinfoversion)
* [Set-NuspecVersion](#set-nuspecversion)
* [Get-ProjectVersion](#get-projectversion)
* [Get-ProjectSemanticVersion](#get-projectsemanticversion)
* [Get-VersionData](#get-versiondata)
* [Get-BuildVersion](#get-buildversion)
* [Format-Version](#format-version)

<a name="invoke-robocopy"></a>
### Invoke-Robocopy
When running a robocopy command from a powershell script it can cause havoc when it comes to the exit code set by the robocopy command. Normally an exit code of 0 indicates that a command went well and 1 indicates that an error occurred. Robocopy has redefined this list <http://ss64.com/nt/robocopy-exit.html> and to avoid a successful robocopy command indicating the PowerShell exit code incorrect as a failure, we can use the `invoke-robocopy` powershell command instead.

It has the same parameters as the [Robocopy](http://ss64.com/nt/robocopy.html) command and only handles the exit code. If any error occur, it will throw an error.

#### Example

	invoke-robocopy "bin\$configuration\help" "bin\$configuration\html_old\reference" /s

<a name="invoke-xmltransform"></a>
### Invoke-XmlTransform
Xml transformation is used to apply an xdt template on to an xml file.

The function takes the following parameters

* xml - path to the Xml document that should be merged
* xdt - path to the Xtd document contaning the data to be merged
* replaceVariables - (optional) If true, replaces some variables in the xdt file before merge with their respective values
	Currently supports:
	* $(ProjectDir)
    * $(SolutionDir)
    * $(ConfigBuilderHost)

#### Example

	invoke-XmlTransform "$projectDir\web.config" "$projectDir\myweb.config.xdt"

<a name="import-markdownsamples"></a>
### Import-MarkdownSamples
This cmdlet will expand code samples and api references in the supplied markdown file. The cmdlet takes the following parameters

* Filename - the path to the markdown file to expand samples in
* RootPath - the path to the folder who acts as root for the markdown structure
* HtmlHelpPath - the relative path (from RootPath) to where the API reference resides

		Import-MarkdownSamples -Filename $file.FullName -RootPath $targetDir -HtmlHelpPath "reference/html"

<a name="convert-markdowntohtml"></a>
### Convert-MarkdownToHtml
This cmdlet will convert a markdown file to Html. The cmdlet takes the following parameters.

* InputFile - the path to the markdown file to convert
* OutputFile - the path to the output html file
* TemplateFile - the name of the template file to use. The closest template found will be used (seaching from the current dir and up).

		Convert-MarkdownToHtml -InputFile $file.FullName -OutputFile $outFile -TemplateFile $templateFile

<a name="set-assemblyinfoversion"></a>
### Set-AssemblyInfoVersion
Sets the versions on the AssemblyInfo.cs file according to the supplied $version object.
Looks in the Properties folder of the project for the AssemblyInfo.cs file

* version - object with three properties:
	* assemblyInfoVersion - string to set as assemblyVersion. ie. 1.1.4.0
	* fileVersion - string to set as fileVersion. ie. 1.1.4.252
	* informationalVersion - string to set as informationalVersion. ie 1.1.4-beta
 
	If no version is suppled, the [Get-ProjectVersion](#get-projectversion) method is invoked

Example

	$version = @{ assemblyVersion = "1.1.0.0", fileVersion = "1.1.0.131", informationalVersion = "1.1.0-beta" }
	Set-AssemblyInfoVersion -version $version

<a name="set-nuspecversion"></a>
### Set-NuspecVersion
Sets the version element of the nuspec file to the supplied version string
 
* path - Path to the nuspec file to modify
* nuspecversion - the version string to set. If omitted, the version is calculated using the [Get-ProjectSemanticVersion](#get-projectsemanticversion)

Example

	Set-NuspecVersion -path (join-path $projectDir "myproj.nuspec") -nuspecversion "1.0.1"

<a name="get-projectversion"></a>
### Get-ProjectVersion
Gets the version object for the current project. See [Get-VersionData](#get-versiondata) for more information.

<a name="get-projectsemanticversion"></a>
### Get-ProjectSemanticVersion
Gets the Semantic version for the project (major.minor.patch). Based on the assemblyVersion from the [Get-ProjectVersion](#get-projectversion)

<a name="get-versiondata"></a>
### Get-VersionData
Gets the version object for the supplied directory.
First calls [Get-BuildVersion](#get-buildversion) to get the current version for the build.
Then looks for the nearest version.json file that expects a json object with three properties
assemblyVersion, fileVersion, informationalVersion.
Each of these properties are patterns on how to format the specific version string. 

	Get-VersionData "c:\myFolder\mySubfolder"


Example of a version.json file

	{
		"assemblyInfo"          : "9.*.*.0",
		"fileVersion"           : "9.*.*.*",
		"informationalVersion"  : "9.*.*"
	}

Pattern in version.json file matches the ver entity in [the Format-Version BNF](#format-version-bnf).
<a name="get-buildversion"></a>
### Get-BuildVersion
Looks in the supplied directory for a buildVersion.txt file. If none is found, it continues to search upwards in the supplied path.
The buildversion.txt file is expected to contain a version string on the format major.minor.patch.build
If no buildVersion.txt is found, an exception is thrown.
	
	Get-BuildVersion "c:\myFolder\mySubfolder"

<a name="format-version"></a>
### Format-Version
Combines the supplied baseVersion with a ver format that controls the resulting format.

* baseVersion - version string on the format major.minor.patch.build,
* ver - version pattern string on the format pattern.pattern.pattern[.pattern]

Example usage: 

	ps> Format-Version -baseVersion "1.4.244.2331" -ver "6.*.*"	
	6.4.244

<a name="format-version-bnf"></a>
#### BNF

	<baseVersion>   ::= <number>.<number>.<number>.<number>
	<ver>           ::= <patternNumber>.<patternNumber>.<patternNumber>[.<patternNumber>]
	<number>        ::= 1*<digit>
	<patternNumber> ::= <number>|<wildcard>
	<digit>         ::= "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9"
	<wildcard>      ::= "*"

<a name="license"></a>
## License
Licensed using the [MIT License](LICENSE.md).

<a name="author"></a>
## Author
Developed by [Dan Händevik](mailto:dan@meriworks.se), [Meriworks](http://www.meriworks.se).

Includes a modified version of MarkdownSharp

### Markdown
Markdown is a text-to-HTML conversion tool for web writers

Copyright (c) 2004 John Gruber <http://daringfireball.net/projects/markdown/>

### Markdown.NET
Copyright (c) 2004-2009 Milan Negovan
<http://www.aspnetresources.com>
<http://aspnetresources.com/blog/markdown_announced.aspx>

### MarkdownSharp
Copyright (c) 2009-2010 Jeff Atwood
<http://stackoverflow.com>
<http://www.codinghorror.com/blog/>
<http://code.google.com/p/markdownsharp/>

History: Milan ported the Markdown processor to C#. He granted license to Jeff so he can open source it and let the community contribute to and improve MarkdownSharp.

<a name="changelog"></a>
## Changelog

### v6.3.0 - 2018-07-09
Markdown now support code blocks with three backticks 
``` 
code block 
  Some indented row
```

### v6.2.1 - 2018-01-12
Markdown tables now adds nowrap to a column when a header separator contains of = instead of -

### v6.2.0 - 2018-01-08
A set of commands related to maintaining version numbers added

* [Set-AssemblyInfoVersion](#set-assemblyinfoversion)
* [Set-NuspecVersion](#set-nuspecversion)
* [Get-ProjectVersion](#get-projectversion)
* [Get-ProjectSemanticVersion](#get-projectsemanticversion)
* [Get-VersionData](#get-versiondata)
* [Get-BuildVersion](#get-buildversion)
* [Format-Version](#format-version)

### v6.1.0 - 2018-01-03
* [Invoke-XmlTransform](#invoke-xmltransform) function added

### v6.0.0 - 2016-11-09
* [Invoke-Robocopy](#invoke-robocopy) replaces Run-Robocopy [#1](https://github.com/meriworks/PowerShell/issues/1)

### v5.0.0 - 2016-11-08
* Initial public release

