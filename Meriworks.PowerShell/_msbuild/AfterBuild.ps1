
#Add powershell statements that should be executed after the build

Write-Host "Signing scripts  $projectDir"
SignScriptsInFolder (join-path $projectDir "nuspec/tools")
SignScript (join-path $projectDir "nuspec/tools/Meriworks.PowerShell.psd1")


#copy dlls to tools folder so the test project can use the files from its project folder
copy $targetPath (Join-Path $projectDir "nuspec/tools/")
copy (join-path (split-path $targetPath -Parent) "MarkdownSharp.dll") (Join-Path $projectDir "nuspec/tools/")