Get-ChildItem "$PSScriptRoot\..\" -inc bin, obj -rec | Remove-Item -rec -force
if (Test-Path "$PSScriptRoot\..\TestResults") {
	Remove-Item "$PSScriptRoot\..\TestResults" -rec -force
}
if (Test-Path "$PSScriptRoot\..\Output") {
	Remove-Item "$PSScriptRoot\..\Output" -Recurse -Force
}
if (Test-Path "$PSScriptRoot\..\.vs") {
	Remove-Item "$PSScriptRoot\..\.vs" -Recurse -Force
}
