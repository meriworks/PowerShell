# Add powershell statements that should be executed after the build
# Remember that you can access the following variables
#
# $solutionDir		-	refers to the path to the solution folder.
# $projectDir		-	refers to the path to the project folder.
# $targetPath		-	is the path to the resulting output target that the project produces.
# $configuration	-	is the name of the current build configuration

#Rename the script to be triggered <Before|After><Build|Compile|Publish>[Debug|Release|*].ps1

#testing the xml-functions
Write-Host "Xml:$XmlTransformDllPath"
Invoke-XmlTransform -xml (Join-Path $projectDir "app.config") -xdt (Join-Path $projectDir "app.config.merge.pre.xdt") -replaceVariables $true