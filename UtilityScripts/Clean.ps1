. "$PSScriptRoot/Helpers.ps1"

Get-ChildItem "$PSScriptRoot\..\" -inc bin, obj -rec | Remove-Item -rec -force
if (Test-Path "$PSScriptRoot\..\TestResults") {
	Remove-Item "$PSScriptRoot\..\TestResults" -rec -force
}
if (Test-Path $tempDir) {
	Remove-Item $tempDir -Recurse -Force
}
if (Test-Path $outputDir) {
	Remove-Item $outputDir -Recurse -Force
}
if (Test-Path "$PSScriptRoot\..\.vs") {
	Remove-Item "$PSScriptRoot\..\.vs" -Recurse -Force
}
