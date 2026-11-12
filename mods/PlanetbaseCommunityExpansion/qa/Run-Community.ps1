$ErrorActionPreference='Stop'
$workspace=Split-Path (Split-Path $PSScriptRoot)
$qa=[IO.Path]::GetFullPath((Join-Path ([IO.Path]::GetTempPath()) 'PlanetbaseResourceMenu-qa'))
$exe=[IO.Path]::GetFullPath("$qa/game/Planetbase.exe")
if(Test-Path "$qa/architecture-process.txt"){$idValue=[int](Get-Content "$qa/architecture-process.txt");$p=Get-Process -Id $idValue -ErrorAction SilentlyContinue;if($p -and $p.Path -eq $exe){Stop-Process -Id $p.Id;$p.WaitForExit(5000)|Out-Null}}
$disabled="$qa/architecture-disabled-probes";New-Item -ItemType Directory -Path $disabled -Force|Out-Null
foreach($probe in Get-ChildItem "$qa/game/BepInEx/plugins" -Recurse -Filter '*Probe.dll'){Move-Item -LiteralPath $probe.FullName -Destination (Join-Path $disabled $probe.Name) -Force}
dotnet build "$PSScriptRoot/CommunityProbe.csproj" -c Release '-p:PlanetbaseDir=C:/Program Files (x86)/Steam/steamapps/common/Planetbase' --nologo -v quiet
if($LASTEXITCODE -ne 0){throw 'No se inicia el juego con un complemento de pruebas sin compilar'}
foreach($name in @('PlanetbaseCommunityExpansion','PlanetbaseSolarFrontier','PlanetbaseNuclearTransport','PlanetbaseColonyLeisure')){New-Item -ItemType Directory -Path "$qa/game/BepInEx/plugins/$name" -Force|Out-Null;Copy-Item "$workspace/$name/bin/Release/netstandard2.1/$name.dll" "$qa/game/BepInEx/plugins/$name/$name.dll"}
Copy-Item "$PSScriptRoot/bin/Release/netstandard2.1/CommunityProbe.dll" "$qa/game/BepInEx/plugins/PlanetbaseCommunityExpansion/CommunityProbe.dll"
$output=Join-Path (Split-Path $PSScriptRoot) 'evidencia'
$p=Start-Process $exe -WorkingDirectory "$qa/game" -ArgumentList @('-screen-fullscreen','0','-screen-width','1600','-screen-height','1000',"`"-community-out=$output`"",'-logFile',"$qa/community-player.log") -WindowStyle Hidden -PassThru
$p.Id|Set-Content "$qa/architecture-process.txt"
Write-Output "Community QA PID $($p.Id)"
