param (
    [string]$packageVersion = $null,
    [string]$config = "Release"
)

#Will abort the build and display the error message
function Die([string]$message, [object[]]$output) {
    if ($output) {
        Write-Output $output
        $message += ". See output above."
    }
    Write-Error $message
    exit 1
}
#Runs the tests in the supplied test assembly
function RunTests([string]$testAssemblyPath) {
	Write-Host "Running test on $testAssemblyPath"
	$testRunner=$env:GallioEcho
	#if($testRunner -eq $null -or -not (Test-Path $testRunner)) {
	#	Die "Cannot find Testrunner. Set env:GallioEcho to the testrunner path. Current '$env:GallioEcho' '$GallioEcho'"
	#}
	Write-Host "$testRunner $testAssemblyPath"
	. $testRunner $testAssemblyPath
}
#Finds the first available path
function findFirstAvailable($paths){
    
    for($i=0;$i -lt $paths.length;$i++){
        if(test-path $paths[$i]) {
            return $paths[$i]
        }
    }
    return $null
}
#setup local paths if not defined (yes I'm lazy)
if($env:GallioEcho -eq $null) {
	Write-Output "env:GallioEcho is not set, setting to local path"
	$env:GallioEcho="D:\usr\bin\NUnit\bin\net-4.0\nunit-console.exe"
}
if($env:nuget-eq $null) {
	$env:nuget="D:\usr\bin\nuget\NuGet.exe"
}

$rootFolder = Split-Path -parent $script:MyInvocation.MyCommand.Definition

# Make sure that package version is supplied
$currentVersion = if(Test-Path env:PackageVersion) { Get-Content env:PackageVersion } else { $packageVersion }
if($currentVersion -eq "") {
    Die("Package version cannot be empty")
}

#make sure that nuget exists
$nugetExe = if(Test-Path Env:NuGet) { Get-Content env:NuGet } else { Join-Path $rootFolder ".nuget\nuget.exe" }
if(-not(Test-Path $nugetExe)){
    Die("Cannot find nuget.exe $nugetExe, either set the nuget env variable or add a .nuget\nuget.exe file.")
}

#restore nuget
#Get-ChildItem . -r packages.config|%{ nuget restore -solutionDir . $_.FullName}
. $nugetExe restore PowerShell.sln

 if($LASTEXITCODE -ne 0) {
	Die "Failed restoring nuget packages"
}

# Detect MSBuild 15.0 path
Write-Output "Finding suitable msbuild"
$defaultMsbuild="$(Get-Content env:windir)\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
$pfdir = (${env:ProgramFiles(x86)}, ${env:ProgramFiles} -ne $null)[0];
$vs2017dir = join-path $pfDir "Microsoft Visual Studio\2017"
$possiblePaths = ($env:MsBuildExe,(join-path $vs2017dir "Community\MSBuild\15.0\Bin\MSBuild.exe"),(join-path $vs2017dir "Professional\MSBuild\15.0\Bin\MSBuild.exe"),(join-path $vs2017dir "Enterprise\MSBuild\15.0\Bin\MSBuild.exe"),$defaultMsbuild)
$msbuild = findFirstAvailable $possiblePaths
Write-Output "Selected msbuild $msbuild"
#build solution
& "$msbuild" "PowerShell.sln" /p:Configuration=$config 

 if($LASTEXITCODE -ne 0) {
	Die "Failed building solution"
}

#run unit tests
$testsFailed=0
Get-ChildItem -Directory "*Tests"|%{
	$p=(join-path $_.FullName "bin\$config\$($_.Name).dll");
	Write-Host "Found test candidate $p";
	if(Test-Path $p) {
		RunTests $p;
		if($LASTEXITCODE -ne 0) {
			$testsFailed=$LASTEXITCODE;
		}
	}
}
if($testsFailed -ne 0) {
	Die "Failed running unit tests"
}

#pack nuget
. $nugetExe pack "Meriworks.PowerShell\Meriworks.PowerShell.nuspec" -version $currentVersion -properties "configuration=$config" #-nopackageanalysis
. $nugetExe pack "Meriworks.Markdown\Meriworks.Markdown.csproj" -version $currentVersion -properties "configuration=$config" #-nopackageanalysis

 if($LASTEXITCODE -ne 0) {
	Die "Failed packaging nuspec"
}
