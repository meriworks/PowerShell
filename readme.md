# Meriworks.PowerShell
The sign library adds a PowerShell module for handling various tasks

Is an [Extension Module](https://github.com/meriworks/PowerShell.BuildEvents#Extension_Modules) to the [Meriworks.PowerShell.BuildEvents](https://github.com/meriworks/PowerShell.BuildEvents) .

* [License](#license)
* [Author](#author)
* [Changelog](#changelog)
* [Documentation](#documentation)

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

### v5.0.0 - 2016-11-08
* Initial public release

<a name="documentation"></a>
## Documentation

## Import-MarkdownSamples
This cmdlet will expand code samples and api references in the supplied markdown file. The cmdlet takes the following parameters

* Filename - the path to the markdown file to expand samples in
* RootPath - the path to the folder who acts as root for the markdown structure
* HtmlHelpPath - the relative path (from RootPath) to where the API reference resides

		Import-MarkdownSamples -Filename $file.FullName -RootPath $targetDir -HtmlHelpPath "reference/html"

## Convert-MarkdownToHtml
This cmdlet will convert a markdown file to Html. The cmdlet takes the following parameters.

* InputFile - the path to the markdown file to convert
* OutputFile - the path to the output html file
* TemplateFile - the name of the template file to use. The closest template found will be used (seaching from the current dir and up).

		Convert-MarkdownToHtml -InputFile $file.FullName -OutputFile $outFile -TemplateFile $templateFile

