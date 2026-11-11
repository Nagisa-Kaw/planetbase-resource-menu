# Terreno y velocidad para Planetbase

Mod para **Planetbase 1.4.2 de Steam en Windows de 32 bits**. Usa BepInEx 5 y se puede instalar junto con el Menú de recursos.

- **Terreno.** Elige con el ratón un círculo en una partida normal. El mod rebaja el terreno elevado dentro de ese círculo y suaviza el borde. La herramienta protege las laderas de minas existentes.
- **Velocidad.** El control normal de velocidad admite ×1, ×2, ×4 y ×8. Usa **+** y **−** del teclado numérico, o abre el botón de velocidad **⏩** en la barra inferior.

## Instalar

1. Cierra Planetbase y abre la carpeta del juego desde Steam con **Administrar → Ver archivos locales**.
2. Si ya tienes BepInEx por el Menú de recursos, usa esa instalación. Si aún no lo tienes, extrae [BepInEx 5.4.23.6 para Windows x86](https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.6/BepInEx_win_x86_5.4.23.6.zip) junto a `Planetbase.exe`.
3. Extrae el ZIP del mod y copia la carpeta `BepInEx` sobre la carpeta de Planetbase. La DLL debe quedar en `BepInEx\plugins\PlanetbaseTerrainAndSpeed\PlanetbaseTerrainAndSpeed.dll`.
4. Inicia Planetbase y carga una partida normal.

## Aplanar una zona

1. Pulsa **Ctrl+F7** para activar la herramienta. La simulación se pausa mientras eliges y puedes mover la cámara.
2. Haz **clic izquierdo** sobre el terreno para fijar el centro. Usa **RePág** y **AvPág** para cambiar el radio entre **20 y 120 m**. Haz **clic derecho** para volver a elegir el centro.
3. Pulsa **Intro** para confirmar un círculo válido. El mod guarda en una **ranura nueva**, recarga la colonia y reconstruye las rutas sobre el terreno aplanado. Tras la pantalla de carga, pulsa una tecla para continuar.
4. Pulsa **Esc** o **Ctrl+F7** para cancelar mientras estás eligiendo. Para volver al relieve anterior después de confirmar, carga el guardado previo.

Los cambios quedan asociados al guardado. El mod conserva el fondo del horizonte y el relieve fuera del círculo. Conserva un guardado anterior si piensas retirar el mod después de construir en una zona aplanada.

## Compilar

Con el SDK de .NET 8 o posterior, abre PowerShell en la carpeta que contiene `PlanetbaseTerrainAndSpeed.csproj`. Dentro del ZIP, se llama `codigo`.

```powershell
$juego = 'C:\Program Files (x86)\Steam\steamapps\common\Planetbase'
dotnet build .\PlanetbaseTerrainAndSpeed.csproj -c Release "-p:PlanetbaseDir=$juego"
```

Ajusta la ruta si el juego está en otra biblioteca. La DLL compilada queda en `bin\Release\netstandard2.1\PlanetbaseTerrainAndSpeed.dll`.
