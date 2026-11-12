param([ValidateSet('Solar','Nuclear','Leisure')][string]$Suite)
$ErrorActionPreference='Stop'
$qa=(Join-Path ([IO.Path]::GetTempPath()) 'PlanetbaseResourceMenu-qa')
$exe=Join-Path $qa 'game/Planetbase.exe'
$root=Split-Path (Split-Path $PSScriptRoot)
if(Test-Path "$qa/architecture-process.txt"){
 $pidValue=[int](Get-Content "$qa/architecture-process.txt")
 $process=Get-Process -Id $pidValue -ErrorAction SilentlyContinue
 if($process -and $process.Path -eq $exe){Stop-Process -Id $process.Id;$process.WaitForExit(5000)|Out-Null}
}
$disabled=Join-Path $qa 'architecture-disabled-probes'
New-Item -ItemType Directory -Path $disabled -Force|Out-Null
foreach($dll in Get-ChildItem "$qa/game/BepInEx/plugins" -Recurse -Filter '*Probe.dll'){
 Move-Item -LiteralPath $dll.FullName -Destination (Join-Path $disabled $dll.Name) -Force
}
$names=@{Solar='SolarFrontierProbe';Nuclear='NuclearTransportProbe';Leisure='LeisureProbe'}
$projects=@{Solar='PlanetbaseSolarFrontier';Nuclear='PlanetbaseNuclearTransport';Leisure='PlanetbaseColonyLeisure'}
$name=$names[$Suite];$project=$projects[$Suite]
dotnet build "$root/$project/qa/$name.csproj" -c Release '-p:PlanetbaseDir=C:/Program Files (x86)/Steam/steamapps/common/Planetbase' --nologo -v quiet
if($LASTEXITCODE -ne 0){throw 'Regression probe compilation failed'}
$pluginDir="$qa/game/BepInEx/plugins/$project"
Copy-Item "$root/$project/qa/bin/Release/netstandard2.1/$name.dll" "$pluginDir/$name.dll"
foreach($mod in $projects.Values){Copy-Item "$root/$mod/bin/Release/netstandard2.1/$mod.dll" "$qa/game/BepInEx/plugins/$mod/$mod.dll"}
$p=Start-Process $exe -WorkingDirectory "$qa/game" -ArgumentList @('-screen-fullscreen','0','-screen-width','1440','-screen-height','900','-logFile',"$qa/architecture-$Suite-player.log") -WindowStyle Hidden -PassThru
$p.Id|Set-Content "$qa/architecture-process.txt"
Write-Output "Regression $Suite PID $($p.Id)"
