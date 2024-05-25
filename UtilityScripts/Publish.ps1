. "$PSScriptRoot/Helpers.ps1"

New-Item $outputDir -Type Directory

$runtime = 'win-x64'
$runtimeName = 'Windows-X64'

Publish -runtime $runtime -runtimeName $runtimeName -projectName 'BeerBattle'
Publish -runtime $runtime -runtimeName $runtimeName -projectName 'Stipstonks'