# Mods para Planetbase

Colección de tres mods para **Planetbase 1.4.2 de Steam en Windows de 32 bits**. El repositorio conserva el nombre `planetbase-resource-menu` del primer mod y reúne ahora los tres proyectos.

| Mod | Funciones | Descargar versión 1.0.0 | Instrucciones y código |
| --- | --- | --- | --- |
| Menú de recursos | Pulsa F8 para elegir recursos y añadir cantidades a la colonia. | [ZIP](descargas/PlanetbaseResourceMenu-1.0.0.zip?raw=true) | [Guía](docs/MenuDeRecursos.md) · [Proyecto](PlanetbaseResourceMenu.csproj) |
| Terreno y velocidad | Selecciona un área circular para rebajar montañas. Añade velocidades ×1, ×2, ×4 y ×8. | [ZIP](descargas/PlanetbaseTerrainAndSpeed-1.0.0.zip?raw=true) | [Guía y código](mods/PlanetbaseTerrainAndSpeed) |
| Ocio colonial | Sala de videojuegos, club nocturno, casino y centro de bienestar, con muebles y actividades para los colonos. | [ZIP](descargas/PlanetbaseColonyLeisure-1.0.0.zip?raw=true) | [Guía y código](mods/PlanetbaseColonyLeisure) · [Validación](mods/PlanetbaseColonyLeisure/VALIDACION.md) |

Cada ZIP contiene la DLL compilada y su código fuente. Los tres pueden instalarse juntos. La [versión original del Menú de recursos](https://github.com/Nagisa-Kaw/planetbase-resource-menu/releases/tag/v1.0.0) también sigue disponible.

## Instalación

1. Cierra Planetbase y abre su carpeta desde Steam con **Administrar → Ver archivos locales**.
2. Instala [BepInEx 5.4.23.6 para Windows x86](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.6) junto a `Planetbase.exe`. Si ya lo tienes por uno de estos mods, conserva esa instalación.
3. Descarga el ZIP del mod que quieras y copia su carpeta `BepInEx` sobre la carpeta del juego. Puedes repetirlo para los otros mods.
4. Vuelve a abrir Planetbase. Consulta la guía de cada mod para sus controles y requisitos.

Los paquetes no incluyen archivos del juego, partidas personales ni complementos de pruebas. Los hashes de las descargas están en [SHA256SUMS.txt](descargas/SHA256SUMS.txt).

## Uso rápido

- **Menú de recursos:** F8, selecciona un recurso, ajusta la cantidad y pulsa **Agregar a la base**.
- **Terreno:** Ctrl+F7, elige el centro, ajusta el radio con RePág/AvPág y confirma con Intro. Guarda en una ranura nueva y recarga para aplicar el terreno. Esc cancela la selección.
- **Velocidad:** usa los botones del juego o +/− del teclado numérico, hasta ×8.
- **Ocio colonial:** abre **Construir interior**, construye una sala, conéctala a la base e instala sus muebles. Requiere una cantina construida. Los colonos usan el ocio automáticamente cuando necesitan recuperar moral.

Conserva un guardado previo a usar las herramientas de terreno o construir edificios de Ocio colonial. Las partidas que contienen los edificios o muebles nuevos necesitan mantener instalado Ocio colonial. El casino utiliza monedas de la colonia y tiene un coste medio por visita. Las guías explican estos comportamientos.

## Compilar

Necesitas el SDK de .NET 8 o posterior, Planetbase instalado y BepInEx 5. Ejecuta desde la raíz del repositorio:

```powershell
$juego = 'C:\Program Files (x86)\Steam\steamapps\common\Planetbase'
dotnet build .\PlanetbaseResourceMenu.csproj -c Release "-p:PlanetbaseDir=$juego"
dotnet build .\mods\PlanetbaseTerrainAndSpeed\PlanetbaseTerrainAndSpeed.csproj -c Release "-p:PlanetbaseDir=$juego"
dotnet build .\mods\PlanetbaseColonyLeisure\PlanetbaseColonyLeisure.csproj -c Release "-p:PlanetbaseDir=$juego"
```

Cada proyecto genera su DLL en su propia carpeta `bin/Release/netstandard2.1`. Las bibliotecas del juego se usan como referencias locales.

## Alcance

Mods para el juego individual. Ocio colonial reutiliza cúpulas, modelos base y animaciones de Planetbase, con mobiliario y adornos generados por el mod. Su versión 1.0.0 pasó 64 comprobaciones automáticas dentro del juego en un perfil aislado. El detalle y los límites están en su documento de validación. No se garantiza compatibilidad con otras versiones del juego.
