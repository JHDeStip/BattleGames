function ThrowOnError() {
	if ($LASTEXITCODE -ne 0) {
		throw 'Build failed.'
	}
}

function Publish(
	[string]$runtime,
	[string]$runtimeName,
	[string]$projectName) {
		$publishDir = "$tempDir\$projectName\$runtime"
		New-Item $publishDir -Type Directory

		Write-Host "`n`n`n`n----PUBLISHING $($projectName.ToUpper()) FOR $($runtime.ToUpper())"
		Invoke-Expression "& dotnet publish `"$PSScriptRoot\..\$projectName\$projectName.csproj`" --configuration `"$configuration`" -p:PublishDir=`"$publishDir`" -p:Runtime=`"$runtime`" -p:DebugType=None -p:DebugSymbols=false"
		ThrowOnError
		
		Write-Host "`n`n`n`n----CREATING $($projectName.ToUpper()) $($runtime.ToUpper()) ZIP FILE"
		Push-Location $publishDir
		$versionInfo = (Get-Item "$projectName.exe").VersionInfo
		Rename-Item "$projectName.exe" "$($versionInfo.ProductName).exe"
		Invoke-Expression "& tar.exe -caf `"$outputDir\$projectName-$($versionInfo.ProductVersion)-$runtime.zip`" *"
		ThrowOnError
		Pop-Location
}

$tempDir = "$PSScriptRoot/../Temp"
$outputDir = "$PSScriptRoot/../Output"
$configuration = 'Release'