Write-Host "Information: Documentation for Meriworks.PowerShell can be found @ https://github.com/meriworks/PowerShell"

#Runs the robocopy command and fixes the last exit code so it don't messes up the powershell invoke
function run-robocopy {
	robocopy $args
	if($lastexitcode -le 7) {
		#need to set the global lastexitcode to make it work with any calling scripts
		$global:lastexitcode=$null
	} else {
		throw "Robocopy had some failures."
	}
}