﻿Write-Host "Information: Documentation for Meriworks.PowerShell can be found @ https://github.com/meriworks/PowerShell"

#Runs the robocopy command and fixes the last exit code so it don't messes up the powershell invoke
function Invoke-RoboCopy {
	robocopy $args
	if($lastexitcode -le 7) {
		#need to set the global lastexitcode to make it work with any calling scripts
		$global:lastexitcode=$null
	} else {
		throw "Robocopy had some failures."
	}
}
$xmlTransformSetup = $false
function setupXmlTransform {
	if($xmlTransformSetup) {
		return
	}
	Add-Type -LiteralPath  $XmlTransformDllPath
	$xmlTransformSetup=$true
}
function Invoke-XmlTransform([string] $xml, [string] $xdt,[bool] $replaceVariables=$false) {
	setupXmlTransform
	 if (!$xml -or !(Test-Path -path $xml -PathType Leaf)) {
        throw "File not found. $xml";
    }
    if (!$xdt -or !(Test-Path -path $xdt -PathType Leaf)) {
        throw "File not found. $xdt";
    }

    $xmldoc = New-Object Microsoft.Web.XmlTransform.XmlTransformableDocument;
    $xmldoc.PreserveWhitespace = $true
    $xmldoc.Load($xml);
	if($replaceVariables) {
	    $transformXml = (Get-Content -raw $xdt) -replace "\$\(ProjectDir\)", "$projectDir" -replace "\$\(SolutionDir\)",  "$solutionDir" -replace "\$\(ConfigBuilderHost\)", $ENV:COMPUTERNAME 
	    $transf = New-Object Microsoft.Web.XmlTransform.XmlTransformation($transformXml,$false,$null);
	} else {
		$transf = New-Object Microsoft.Web.XmlTransform.XmlTransformation($xdt);
	}
    if ($transf.Apply($xmldoc) -eq $false)
    {
        throw "Transformation failed."
    }
    $xmldoc.Save($xml);
}
# Combines the supplied baseVersion with a ver format that controls the resulting format.
# baseVersion is a version string on the format major.minor.patch.build,
# ver is a version pattern string
# BNF
# <baseVersion>		::=	<number>.<number>.<number>.<number>
# <ver>				::=	<patternNumber>.<patternNumber>.<patternNumber>[.<patternNumber>]
# <number>			::=	1*<digit>
# <patternNumber>	::=	<number>|<wildcard>
# <digit>			::=	"0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9"
# <wildcard>		::=	"*"
function Format-Version([string] $baseVersion, [string] $ver) {
	if(!($baseVersion -match "^(?<v1>\d+)\.(?<v2>\d+)\.(?<v3>\d+)\.(?<v4>\d+)$")){
		throw "Incorrect base version format '$baseVersion'. Expected \d+.\d+.\d+.\d+"
	}
	$bv1 = $Matches.v1
	$bv2 = $Matches.v2
	$bv3 = $Matches.v3
	$bv4 = $Matches.v4

	if(!($ver -match "^(?<v1>\d+|\*)\.(?<v2>\d+|\*)\.(?<v3>\d+|\*)(:?\.(?<v4>\d+|\*))?$")){
		throw "Incorrect version format '$ver'. Expected x.x.x.x where x is one or multple numeric characters or a wildcard '*' character"
	}
	$v1 = $Matches.v1
	$v2 = $Matches.v2
	$v3 = $Matches.v3
	$v4 = $Matches.v4
	if($v1 -eq '*') {
		$v1 = $bv1
	}
	if($v2 -eq '*') {
		$v2 = $bv2
	}
	if($v3 -eq '*') {
		$v3 = $bv3
	}
	if($v4 -eq '*') {
		$v4 = $bv4
	}
	$ver = "$v1.$v2.$v3"
	if($v4.length -gt 0) {
		$ver="$ver.$v4"
	}
	return $ver
}
# Looks in the supplied directory for a buildVersion.txt file. If none is found, it continues to search upwards in 
# the supplied path.
# The buildversion.txt file is expected to contain a version string on the format major.minor.patch.build
function Get-BuildVersion([string] $dir) {
	while(Test-Path $dir) {
		$verPath = Join-Path $dir "buildVersion.txt"
		if(Test-Path $verPath) {
			Write-Host "Reading $verPath"
			return (Get-Content $verPath)
		}
		$dir = Split-Path $dir
		if($dir -eq "") {
			break;
		}
	}
	throw "Canot find buildversion.txt file"
}
# Gets the version object for the supplied directory.
# First calls Get-BuildVersion to get the current version for the build.
# Then looks for the nearest version.json file that expects a json object with three properties
# assemblyVersion, fileVersion, informationalVersion.
# Each of these properties are patterns on how to format the specific version string. 
function Get-VersionData([string] $dir){
	$buildVersion = Get-BuildVersion $dir
	while(Test-Path $dir) {
		$verPath = Join-Path $dir "version.json"
		if(Test-Path $verPath) {
			Write-Host "Reading $verPath"
			$v = Get-Content $verPath|Out-String|ConvertFrom-Json
			$v.assemblyVersion = Format-Version $buildVersion $v.assemblyVersion
			$v.fileVersion = Format-Version $buildVersion $v.fileVersion
			$v.informationalVersion = Format-Version $buildVersion $v.informationalVersion
			return $v
		}
		$dir = Split-Path $dir
		if($dir -eq "") {
			break;
		}
	}
	return $null
}
# Gets the version object for the project. See Get-VersionData for more information
function Get-ProjectVersion(){
	$version = Get-VersionData $projectDir
	if($version -eq $null) {
		throw "No version.json file was found in the $projectDir path or any of it's parents"
	} 
	return $version
}
# Gets the Semantic version for the project (major.minor.patch). 
# Based on the assemblyVersion from the Get-ProjectVersion
function Get-ProjectSemanticVersion(){
	$version=Get-ProjectVersion
	return ($version.assemblyVersion) -replace "(\d+\.\d+\.\d+)\.\d+",'$1'
}
# Sets the versions on the AssemblyInfo.cs file according to the supplied $version object.
# Looks in the Properties folder of the project for the AssemblyInfo.cs file
#
# $version - object with three properties:
#	assemblyInfoVersion - string to set as assemblyVersion. ie. 1.1.4.0
#	fileVersion - string to set as fileVersion. ie. 1.1.4.252
#	informationalVersion - string to set as informationalVersion. ie 1.1.4-beta
# If no version is suppled, the Get-ProjectVersion method is invoked
function Set-AssemblyInfoVersion($version){
	if($version -eq $null){
		$version = Get-ProjectVersion
	}
	$aiPath = Join-Path $projectDir "Properties/AssemblyInfo.cs"
	if(!(Test-Path $aiPath)){
		Write-Host "AssemblyInfo file is missing '$aiPath'"
		return
	}

	$ai = Get-Content $aiPath|Out-String
	$ai = $ai -replace "AssemblyVersion\([^)]+\)","AssemblyVersion(""$($version.assemblyVersion)"")"
	$ai = $ai -replace "AssemblyFileVersion\([^)]+\)","AssemblyFileVersion(""$($version.fileVersion)"")"
	$ai = $ai -replace "AssemblyInformationalVersion\([^)]+\)","AssemblyInformationalVersion(""$($version.informationalVersion)"")"
	$ai.TrimEnd("`n`r")|Out-File $aiPath -Encoding utf8
}

#Sets the version element of the nuspec file to the supplied version string
# $path - Path to the nuspec file to modify
# $nuspecversion - the version string to set. If omitted, the version is calculated using the Get-ProjectSemanticVersion
function Set-NuspecVersion([string] $path, [string] $nuspecversion){
	if($nuspecversion -eq $null -or $nuspecversion.Length -eq 0) {
		$nuspecversion = Get-ProjectSemanticVersion
	}
	Write-Host "SetNuspecVersion '$path' '$nuspecversion'"
	if(!(Test-Path $path)){
		throw "File is missing $path"
	}

	$x =New-Object System.Xml.XmlDocument
	$x.Load($path)
	$x.PreserveWhitespace=$true
	$x =[Xml](Get-Content $path)
	$x.Package.metadata.version=$nuspecversion

	$settings = New-Object System.Xml.XmlWriterSettings
	$settings.Indent = $true
	#$settings.NewLineChars ="`r`n"
	$settings.Encoding = New-Object System.Text.UTF8Encoding( $false )
	
	$w = [System.Xml.XmlWriter]::Create($path,$settings)
	try{
		$x.Save( $w )
	} finally{
		$w.Dispose()
	}
}

# SIG # Begin signature block
# MIIbPQYJKoZIhvcNAQcCoIIbLjCCGyoCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUknsDdptHe02dESy/Bmw/yfxC
# NkegghaNMIID7jCCA1egAwIBAgIQfpPr+3zGTlnqS5p31Ab8OzANBgkqhkiG9w0B
# AQUFADCBizELMAkGA1UEBhMCWkExFTATBgNVBAgTDFdlc3Rlcm4gQ2FwZTEUMBIG
# A1UEBxMLRHVyYmFudmlsbGUxDzANBgNVBAoTBlRoYXd0ZTEdMBsGA1UECxMUVGhh
# d3RlIENlcnRpZmljYXRpb24xHzAdBgNVBAMTFlRoYXd0ZSBUaW1lc3RhbXBpbmcg
# Q0EwHhcNMTIxMjIxMDAwMDAwWhcNMjAxMjMwMjM1OTU5WjBeMQswCQYDVQQGEwJV
# UzEdMBsGA1UEChMUU3ltYW50ZWMgQ29ycG9yYXRpb24xMDAuBgNVBAMTJ1N5bWFu
# dGVjIFRpbWUgU3RhbXBpbmcgU2VydmljZXMgQ0EgLSBHMjCCASIwDQYJKoZIhvcN
# AQEBBQADggEPADCCAQoCggEBALGss0lUS5ccEgrYJXmRIlcqb9y4JsRDc2vCvy5Q
# WvsUwnaOQwElQ7Sh4kX06Ld7w3TMIte0lAAC903tv7S3RCRrzV9FO9FEzkMScxeC
# i2m0K8uZHqxyGyZNcR+xMd37UWECU6aq9UksBXhFpS+JzueZ5/6M4lc/PcaS3Er4
# ezPkeQr78HWIQZz/xQNRmarXbJ+TaYdlKYOFwmAUxMjJOxTawIHwHw103pIiq8r3
# +3R8J+b3Sht/p8OeLa6K6qbmqicWfWH3mHERvOJQoUvlXfrlDqcsn6plINPYlujI
# fKVOSET/GeJEB5IL12iEgF1qeGRFzWBGflTBE3zFefHJwXECAwEAAaOB+jCB9zAd
# BgNVHQ4EFgQUX5r1blzMzHSa1N197z/b7EyALt0wMgYIKwYBBQUHAQEEJjAkMCIG
# CCsGAQUFBzABhhZodHRwOi8vb2NzcC50aGF3dGUuY29tMBIGA1UdEwEB/wQIMAYB
# Af8CAQAwPwYDVR0fBDgwNjA0oDKgMIYuaHR0cDovL2NybC50aGF3dGUuY29tL1Ro
# YXd0ZVRpbWVzdGFtcGluZ0NBLmNybDATBgNVHSUEDDAKBggrBgEFBQcDCDAOBgNV
# HQ8BAf8EBAMCAQYwKAYDVR0RBCEwH6QdMBsxGTAXBgNVBAMTEFRpbWVTdGFtcC0y
# MDQ4LTEwDQYJKoZIhvcNAQEFBQADgYEAAwmbj3nvf1kwqu9otfrjCR27T4IGXTdf
# plKfFo3qHJIJRG71betYfDDo+WmNI3MLEm9Hqa45EfgqsZuwGsOO61mWAK3ODE2y
# 0DGmCFwqevzieh1XTKhlGOl5QGIllm7HxzdqgyEIjkHq3dlXPx13SYcqFgZepjhq
# IhKjURmDfrYwggRdMIIDRaADAgECAgsEAAAAAAElBx35rzANBgkqhkiG9w0BAQsF
# ADBXMQswCQYDVQQGEwJCRTEZMBcGA1UEChMQR2xvYmFsU2lnbiBudi1zYTEQMA4G
# A1UECxMHUm9vdCBDQTEbMBkGA1UEAxMSR2xvYmFsU2lnbiBSb290IENBMB4XDTA5
# MTExODEwMDAwMFoXDTE5MDMxODEwMDAwMFowTDEgMB4GA1UECxMXR2xvYmFsU2ln
# biBSb290IENBIC0gUjMxEzARBgNVBAoTCkdsb2JhbFNpZ24xEzARBgNVBAMTCkds
# b2JhbFNpZ24wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDMJXaQeQZ4
# Ihb1wIO2hMoonv0FdhHFrYhy/EYCQ8eyip0EXyTLLkvhYIJG4VKrDIFHcGzdZNHr
# 9SyjD4I9DCuul9e2FIYQebs7E4B3jAjhSdJqYi8fXvqWaN+JJ5U4nwbXPsnLJlkN
# c96wyOkmDoMVxu9bi9IEYMpJpij2aTv2y8gokeWdimFXN6x0FNx04Druci8unPvQ
# u7/1PQDhBjPogiuuU6Y6FnOM3UEOIDrAtKeh6bJPkC4yYOlXy7kEkmho5TgmYHWy
# n3f/kRTvriBJ/K1AFUjRAjFhGV64l++td7dkmnq/X8ET75ti+w1s4FRpFqkD2m7p
# g5NxdsZphYIXAgMBAAGjggEzMIIBLzAOBgNVHQ8BAf8EBAMCAQYwDwYDVR0TAQH/
# BAUwAwEB/zAdBgNVHQ4EFgQUj/BLf6guRSSuTVD6Y5qL3uLdG7wwRgYDVR0gBD8w
# PTA7BgRVHSAAMDMwMQYIKwYBBQUHAgEWJWh0dHA6Ly93d3cuZ2xvYmFsc2lnbi5u
# ZXQvcmVwb3NpdG9yeS8wMwYDVR0fBCwwKjAooCagJIYiaHR0cDovL2NybC5nbG9i
# YWxzaWduLm5ldC9yb290LmNybDBPBggrBgEFBQcBAQRDMEEwPwYIKwYBBQUHMAGG
# M2h0dHA6Ly9vY3NwLmdsb2JhbHNpZ24uY29tL0V4dGVuZGVkU1NMU0hBMjU2Q0FD
# cm9zczAfBgNVHSMEGDAWgBRge2YaRQ2XyolQL30EzTSo//z9SzANBgkqhkiG9w0B
# AQsFAAOCAQEAQlKpfqLPWzvLS926+FdZ0ySkd3LvYkQ3gu0G7gTVFl8koxTcbFQF
# arCbPdqBOdqtKNuVb4GD9c1isUUksd0p5QhUlZWM8B0GXxrWRj8TQBdIERabR03R
# OrUPVxySMND4siU7Cs32h/nHslfTP32ljBTOnKjHn0aT2ln6eV1lIDVEWk/BkJ3B
# VJJW3DTI9cED0F3AWUicAPyVoPHRdvcWNsgTkn8tK8C4gPEmJh9BTVK/Hpe7AYII
# 5xX2wdU0Ksz15MOHeleB4dbXQoZiAXfiqcR6hvQEOHoHan0A7HP3qAs0eMWes++4
# OEAOjDNTyHXsXz7qdV7/gg50FdwZBfO6MTCCBJQwggN8oAMCAQICDkgbagcm0ug/
# JgLUglrNMA0GCSqGSIb3DQEBCwUAMEwxIDAeBgNVBAsTF0dsb2JhbFNpZ24gUm9v
# dCBDQSAtIFIzMRMwEQYDVQQKEwpHbG9iYWxTaWduMRMwEQYDVQQDEwpHbG9iYWxT
# aWduMB4XDTE2MDYxNTAwMDAwMFoXDTI0MDYxNTAwMDAwMFowWjELMAkGA1UEBhMC
# QkUxGTAXBgNVBAoTEEdsb2JhbFNpZ24gbnYtc2ExMDAuBgNVBAMTJ0dsb2JhbFNp
# Z24gQ29kZVNpZ25pbmcgQ0EgLSBTSEEyNTYgLSBHMzCCASIwDQYJKoZIhvcNAQEB
# BQADggEPADCCAQoCggEBAI2FVSOpH1Ovye02wim1btj5QvUhi4fwQ6EvwD+X4xhH
# 1JZOiKvGY+m2SRWF8dmbexv92ErMbU6GQBfNe7pHKLXWFEAMhu4eSzPzLFXuIYza
# tsH+sE46rlGfpRTjEOaTyvr5XbGQh+/4NS38olwm7nsVix/Zw6GXNUGzMP1yE62R
# lzrOcp0GCnU8H5jjyxgvjJ8ISpEK2dk12YOSdmJe34Ou5NYBTszzPHBhWDcfFONq
# oK9r9NnSbCwZMF2gb1Lf0ZzJ4A3ZdRFlltlDtKeQOa5HHFTLsZVuJd6O8RIc8Ndu
# 5xRNsxpl1oRKV+4Er2I4xRCbQ45SuD62rp4sl/+OdO8CAwEAAaOCAWQwggFgMA4G
# A1UdDwEB/wQEAwIBBjAdBgNVHSUEFjAUBggrBgEFBQcDAwYIKwYBBQUHAwkwEgYD
# VR0TAQH/BAgwBgEB/wIBADAdBgNVHQ4EFgQUDzrnrJSRdC2WAnODrZwuST8ZqlQw
# HwYDVR0jBBgwFoAUj/BLf6guRSSuTVD6Y5qL3uLdG7wwPgYIKwYBBQUHAQEEMjAw
# MC4GCCsGAQUFBzABhiJodHRwOi8vb2NzcDIuZ2xvYmFsc2lnbi5jb20vcm9vdHIz
# MDYGA1UdHwQvMC0wK6ApoCeGJWh0dHA6Ly9jcmwuZ2xvYmFsc2lnbi5jb20vcm9v
# dC1yMy5jcmwwYwYDVR0gBFwwWjALBgkrBgEEAaAyATIwCAYGZ4EMAQQBMEEGCSsG
# AQQBoDIBXzA0MDIGCCsGAQUFBwIBFiZodHRwczovL3d3dy5nbG9iYWxzaWduLmNv
# bS9yZXBvc2l0b3J5LzANBgkqhkiG9w0BAQsFAAOCAQEAFYQoDO2hwxmC22MnQdfM
# Y33WvM82m8/SXUwCi1gaFgh6sZiZf9gzilyeYgqa6pDCxWMEC+iHWAYAB4JZw6iU
# MtngFE2g1SPgx9268GniSkVlLU1JkHhmMg6waMCwiLCO7VwG+FwQSD8jc7o7U4RS
# gM3wsxXIEQqLBXhGWQjUvrf/S/WcakycdqIRhUWM1DfuUN3hMzSpoR86C/FWRIpv
# szPQsY7RDGAH7jLC8CRvv6/7gQF2Z8Sj6OCr3DNVKONnRzzraqTflOn0psCB0VKY
# UabWArc86krv0Lpl0jeDU0t+Z3yMQPYHHLCvBAbc+GxGiHKc606dw7C5VKO5TpfK
# FDCCBKMwggOLoAMCAQICEA7P9DjI/r81bgTYapgbGlAwDQYJKoZIhvcNAQEFBQAw
# XjELMAkGA1UEBhMCVVMxHTAbBgNVBAoTFFN5bWFudGVjIENvcnBvcmF0aW9uMTAw
# LgYDVQQDEydTeW1hbnRlYyBUaW1lIFN0YW1waW5nIFNlcnZpY2VzIENBIC0gRzIw
# HhcNMTIxMDE4MDAwMDAwWhcNMjAxMjI5MjM1OTU5WjBiMQswCQYDVQQGEwJVUzEd
# MBsGA1UEChMUU3ltYW50ZWMgQ29ycG9yYXRpb24xNDAyBgNVBAMTK1N5bWFudGVj
# IFRpbWUgU3RhbXBpbmcgU2VydmljZXMgU2lnbmVyIC0gRzQwggEiMA0GCSqGSIb3
# DQEBAQUAA4IBDwAwggEKAoIBAQCiYws5RLi7I6dESbsO/6HwYQpTk7CY260sD0rF
# bv+GPFNVDxXOBD8r/amWltm+YXkLW8lMhnbl4ENLIpXuwitDwZ/YaLSOQE/uhTi5
# EcUj8mRY8BUyb05Xoa6IpALXKh7NS+HdY9UXiTJbsF6ZWqidKFAOF+6W22E7RVEd
# zxJWC5JH/Kuu9mY9R6xwcueS51/NELnEg2SUGb0lgOHo0iKl0LoCeqF3k1tlw+4X
# dLxBhircCEyMkoyRLZ53RB9o1qh0d9sOWzKLVoszvdljyEmdOsXF6jML0vGjG/SL
# vtmzV4s73gSneiKyJK4ux3DFvk6DJgj7C72pT5kI4RAocqrNAgMBAAGjggFXMIIB
# UzAMBgNVHRMBAf8EAjAAMBYGA1UdJQEB/wQMMAoGCCsGAQUFBwMIMA4GA1UdDwEB
# /wQEAwIHgDBzBggrBgEFBQcBAQRnMGUwKgYIKwYBBQUHMAGGHmh0dHA6Ly90cy1v
# Y3NwLndzLnN5bWFudGVjLmNvbTA3BggrBgEFBQcwAoYraHR0cDovL3RzLWFpYS53
# cy5zeW1hbnRlYy5jb20vdHNzLWNhLWcyLmNlcjA8BgNVHR8ENTAzMDGgL6Athito
# dHRwOi8vdHMtY3JsLndzLnN5bWFudGVjLmNvbS90c3MtY2EtZzIuY3JsMCgGA1Ud
# EQQhMB+kHTAbMRkwFwYDVQQDExBUaW1lU3RhbXAtMjA0OC0yMB0GA1UdDgQWBBRG
# xmmjDkoUHtVM2lJjFz9eNrwN5jAfBgNVHSMEGDAWgBRfmvVuXMzMdJrU3X3vP9vs
# TIAu3TANBgkqhkiG9w0BAQUFAAOCAQEAeDu0kSoATPCPYjA3eKOEJwdvGLLeJdyg
# 1JQDqoZOJZ+aQAMc3c7jecshaAbatjK0bb/0LCZjM+RJZG0N5sNnDvcFpDVsfIkW
# xumy37Lp3SDGcQ/NlXTctlzevTcfQ3jmeLXNKAQgo6rxS8SIKZEOgNER/N1cdm5P
# Xg5FRkFuDbDqOJqxOtoJcRD8HHm0gHusafT9nLYMFivxf1sJPZtb4hbKE4FtAC44
# DagpjyzhsvRaqQGvFZwsL0kb2yK7w/54lFHDhrGCiF3wPbRRoXkzKy57udwgCRNx
# 62oZW8/opTBXLIlJP7nPf8m/PiJoY1OavWl0rMUdPH+S4MO8HNgEdTCCBPcwggPf
# oAMCAQICDFtqoyuvu9cLWwsQOzANBgkqhkiG9w0BAQsFADBaMQswCQYDVQQGEwJC
# RTEZMBcGA1UEChMQR2xvYmFsU2lnbiBudi1zYTEwMC4GA1UEAxMnR2xvYmFsU2ln
# biBDb2RlU2lnbmluZyBDQSAtIFNIQTI1NiAtIEczMB4XDTE3MDkxMTExMDYwNFoX
# DTE4MTAyMjA5NDM0M1owcTELMAkGA1UEBhMCU0UxDzANBgNVBAcTBktBTE1BUjEV
# MBMGA1UEChMMTWVyaXdvcmtzIEFCMRUwEwYDVQQDEwxNZXJpd29ya3MgQUIxIzAh
# BgkqhkiG9w0BCQEWFHN1cHBvcnRAbWVyaXdvcmtzLnNlMIIBIjANBgkqhkiG9w0B
# AQEFAAOCAQ8AMIIBCgKCAQEAtJpQVhbhwCzvH/A1yTdGjDxp2qwp17oRX2+ExG3J
# SJp+s+IWO02zvADTB3dRyUlSkJClDy3bKoL3LQxPN+Y+ZSN2slL0rIGIVWy0mIX4
# PV9pqiFG2oL+//1JrpVJzISObHGLVe4rFUd6u69y4CIl+DOxIrm9BTwj14kvxiuc
# ygsTvf5gFFAMgCwk6NK+yYUo5LFDsMAKBHbXWMYhZf2FsVm6z11H3SncOuHkHwJH
# gsQr6vQYgGHvVjxA4/lOBap5F7w0oke17xkhSTYUdmQjctwN5WVx1Xe1ATEhIkw5
# tTUDiDT6/N3irvq3EKJHkOLQsxr4cRcx91d8Xt1r3xgZdwIDAQABo4IBpDCCAaAw
# DgYDVR0PAQH/BAQDAgeAMIGUBggrBgEFBQcBAQSBhzCBhDBIBggrBgEFBQcwAoY8
# aHR0cDovL3NlY3VyZS5nbG9iYWxzaWduLmNvbS9jYWNlcnQvZ3Njb2Rlc2lnbnNo
# YTJnM29jc3AuY3J0MDgGCCsGAQUFBzABhixodHRwOi8vb2NzcDIuZ2xvYmFsc2ln
# bi5jb20vZ3Njb2Rlc2lnbnNoYTJnMzBWBgNVHSAETzBNMEEGCSsGAQQBoDIBMjA0
# MDIGCCsGAQUFBwIBFiZodHRwczovL3d3dy5nbG9iYWxzaWduLmNvbS9yZXBvc2l0
# b3J5LzAIBgZngQwBBAEwCQYDVR0TBAIwADA/BgNVHR8EODA2MDSgMqAwhi5odHRw
# Oi8vY3JsLmdsb2JhbHNpZ24uY29tL2dzY29kZXNpZ25zaGEyZzMuY3JsMBMGA1Ud
# JQQMMAoGCCsGAQUFBwMDMB0GA1UdDgQWBBRTrUxOp1uwprF29a68MBeLBidrbTAf
# BgNVHSMEGDAWgBQPOueslJF0LZYCc4OtnC5JPxmqVDANBgkqhkiG9w0BAQsFAAOC
# AQEAR6ugJ6OmL2/50LPBxm3eqspwL3K4tXaDja28roRiqKkr6sAcnf3uokyBbqxf
# fqi2+uYTHiR7TyxmlHtrOAXkglNCW7ojPhBjOJqEmtIRl5DpYbf6vAdTnQWshSeD
# cR1Y412JWUxwyLgN2rv0UITgkHfQHJ7rHaWHwOlfJAIXIaqSZLeOcd2WtMESYKrA
# OPIHoNXJth8eot/krlPp33MpIgF6vXnWJD7VomrR2oS6TXS5zckLv33YEzANE+D+
# mtBFNf1lLBZSUR0jron7ayVkn8p07Zq65bZNoQCB+4O7NKGAdascRJSvU5zXrWuK
# 2cpVXEDZTg1HN3gZMpgBfHYpBzGCBBowggQWAgEBMGowWjELMAkGA1UEBhMCQkUx
# GTAXBgNVBAoTEEdsb2JhbFNpZ24gbnYtc2ExMDAuBgNVBAMTJ0dsb2JhbFNpZ24g
# Q29kZVNpZ25pbmcgQ0EgLSBTSEEyNTYgLSBHMwIMW2qjK6+71wtbCxA7MAkGBSsO
# AwIaBQCgeDAYBgorBgEEAYI3AgEMMQowCKACgAChAoAAMBkGCSqGSIb3DQEJAzEM
# BgorBgEEAYI3AgEEMBwGCisGAQQBgjcCAQsxDjAMBgorBgEEAYI3AgEVMCMGCSqG
# SIb3DQEJBDEWBBQblsa+gLiJua1wQbRsKm3KthTbFzANBgkqhkiG9w0BAQEFAASC
# AQAUChO2X4Yjxk2xysrLwSR6d8iactPIrtl+mpYQl+OHQ/k8nvrPMHaSgTkhNk7+
# C2hsIbvkQA6MVkayHhIHZPB8AfiNHRoUYqUbdSaLlsN1ayZfx+71+SHQ/3SrOT4R
# 0uia1hJwActaBOufhod+SXl+YHBkkvPOuLw6diBNSDUvXR/40/PIblkC8JA049r+
# JpFN9l8yis2S4IMUhNInhh5vm4v5HjWu9UbnIMXFfjQNF2lY3uwnrT8RInQwVE+n
# 3m074VDZqp2gbF+JuX77PfsObKnoE5JBjlvi/XoS6TlM7hBh3bbK66jk/AXZLF2U
# dw8sjiDSjfwGxKfhowfX9gJEoYICCzCCAgcGCSqGSIb3DQEJBjGCAfgwggH0AgEB
# MHIwXjELMAkGA1UEBhMCVVMxHTAbBgNVBAoTFFN5bWFudGVjIENvcnBvcmF0aW9u
# MTAwLgYDVQQDEydTeW1hbnRlYyBUaW1lIFN0YW1waW5nIFNlcnZpY2VzIENBIC0g
# RzICEA7P9DjI/r81bgTYapgbGlAwCQYFKw4DAhoFAKBdMBgGCSqGSIb3DQEJAzEL
# BgkqhkiG9w0BBwEwHAYJKoZIhvcNAQkFMQ8XDTE4MDEwODEyNDk1NlowIwYJKoZI
# hvcNAQkEMRYEFIXwvzHSbYwy4RXGXcIEs5hvw9Y9MA0GCSqGSIb3DQEBAQUABIIB
# AD5QLur3kLHESRI9RkvvJnBv7r8Ltc9Yhe6A96iw5LR0PugMqMxRtVKoW4Ygd44O
# i3Wp/yNZiTGV/Ee7nhBNpeTxPcVuXGuGCSAuzfEHRrbh84bXYTTh4hH32x9ZCx38
# YhewPXaZEjTPm40XCqm4fCI9w5TSEgMRaf4gl2nvRgKbegfa/YkZat/NUmebn2fs
# tiTMtI/rDILrtZulusO21gMwwdVCQ6zvArJtXtdtFvBN3VKd2BrqYVyQGTCYy2U+
# KoLhAxZGom+kT/M0j/atoxxYOb24zywIJ6JZo7TfeQsdtko0sOLB8MKA6KSKDMWJ
# Gy1C19F7SynaKFi1woKSIog=
# SIG # End signature block
