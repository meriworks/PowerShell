param([string] $solutionDir, [string] $projectDir, [string] $targetPath)

#Add powershell statements that should be executed after the build

Write-Host "Signing scripts"
SignScriptsInFolder (join-path $projectDir "nuspec/tools")
SignScript (join-path $projectDir "nuspec/tools/Meriworks.PowerShell.psd1")