param (
    [string]$packageVersion = $null,
    [string]$config = "Release"
)
function Die([string]$message, [object[]]$output) {
    if ($output) {
        Write-Output $output
        $message += ". See output above."
    }
    Write-Error $message
    exit 1
}

$rootFolder = Split-Path -parent $script:MyInvocation.MyCommand.Definition

# Myget
$currentVersion = if(Test-Path env:PackageVersion) { Get-Content env:PackageVersion } else { $packageVersion }
if($currentVersion -eq "") {
    Die("Package version cannot be empty")
}

$nugetExe = if(Test-Path Env:NuGet) { Get-Content env:NuGet } else { Join-Path $rootFolder ".nuget\nuget.exe" }
if(-not(Test-Path $nugetExe)){
    Die("Cannot find nuget.exe $nugetExe, either set the nuget env variable or add a .nuget\nuget.exe file.")
}


& "$(Get-Content env:windir)\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" "PowerShell.sln" /p:Configuration=$config 

. $nugetExe pack "Meriworks.PowerShell\Meriworks.PowerShell.nuspec" -version $currentVersion -properties "configuration=$config" #-nopackageanalysis