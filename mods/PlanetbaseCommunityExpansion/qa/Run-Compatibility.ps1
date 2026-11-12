$ErrorActionPreference='Stop'
$workspace=Split-Path (Split-Path $PSScriptRoot)
$qa=(Join-Path ([IO.Path]::GetTempPath()) 'PlanetbaseResourceMenu-qa')
$output=Join-Path (Split-Path $PSScriptRoot) 'evidencia/compatibilidad'
New-Item -ItemType Directory -Path $output -Force|Out-Null
foreach($suite in @('Nuclear','Solar','Leisure')){
 & "$workspace/DisenosEstructuras/qa/Run-Regression.ps1" -Suite $suite
 $deadline=[DateTime]::UtcNow.AddMinutes(5);$passed=$false
 while([DateTime]::UtcNow -lt $deadline){
  Start-Sleep -Seconds 3
  $log=Get-Content "$qa/game/BepInEx/LogOutput.log" -Raw -ErrorAction SilentlyContinue
  if($log -match 'QA FAILED'){throw "$suite regression failed"}
  $marker=if($suite -eq 'Solar'){'SOLAR QA PASSED:'}else{"$($suite.ToUpper()) QA previews saved:"}
  if($log -match [regex]::Escape($marker)){$passed=$true;break}
 }
 if(-not $passed){throw "$suite regression timed out"}
 if($log -notmatch 'Colonias en expansión 0.1.0 alfa lista'){throw 'El mod comunitario no estaba cargado durante la regresión'}
 Copy-Item "$qa/game/BepInEx/LogOutput.log" "$output/$($suite.ToLower()).log"
 Copy-Item "$qa/architecture-$suite-player.log" "$output/$($suite.ToLower())-player.log"
 ($log -split "`n"|Where-Object {$_ -match 'QA PASSED:'})|Write-Output
}
Write-Output 'Las tres suites de mecánicas anteriores pasaron con la expansión comunitaria cargada.'
