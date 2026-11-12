param([int]$WorldIndex=0)
$ErrorActionPreference='Stop'
$qaRoot=(Join-Path ([IO.Path]::GetTempPath()) 'PlanetbaseResourceMenu-qa')
$gameDir='C:\Program Files (x86)\Steam\steamapps\common\Planetbase'
$qaExe=Join-Path $qaRoot 'game\Planetbase.exe'
if(-not (Test-Path -LiteralPath $qaExe)){throw 'Falta la copia aislada de QA.'}
if(Test-Path -LiteralPath "$qaRoot\solar-process.txt"){
 $qaProcess=Get-Process -Id (Get-Content "$qaRoot\solar-process.txt") -ErrorAction SilentlyContinue
 if($qaProcess -and $qaProcess.Path -eq $qaExe){Stop-Process -Id $qaProcess.Id; $qaProcess.WaitForExit(5000)|Out-Null}
}
dotnet build "$PSScriptRoot\SolarFrontierProbe.csproj" -c Release "-p:PlanetbaseDir=$gameDir"
if($LASTEXITCODE -ne 0){throw 'No se inicia QA con una compilación fallida.'}
$destination=Join-Path $qaRoot 'game\BepInEx\plugins\PlanetbaseSolarFrontier'
New-Item -ItemType Directory -Force -Path $destination|Out-Null
Copy-Item -LiteralPath "$PSScriptRoot\..\bin\Release\netstandard2.1\PlanetbaseSolarFrontier.dll","$PSScriptRoot\bin\Release\netstandard2.1\SolarFrontierProbe.dll" -Destination $destination
$process=Start-Process -FilePath $qaExe -WorkingDirectory "$qaRoot\game" -ArgumentList @('-screen-fullscreen','0','-screen-width','1440','-screen-height','900',"-solar-world=$WorldIndex",'-logFile',"$qaRoot\solar-player.log") -WindowStyle Hidden -PassThru
$process.Id|Set-Content "$qaRoot\solar-process.txt"
Write-Output "QA iniciado: $($process.Id)"
