# Menú de recursos para Planetbase

Mod para **Planetbase 1.4.2 en Windows**, probado con la edición de Steam, compilación 20301361. El ejecutable es de 32 bits y usa Unity 2022.3.62f2 con Mono. El mod está escrito en C# y utiliza BepInEx 5 con Harmony para integrar su ventana en la interfaz del juego. [Anuncio de Planetbase 1.4.2](https://steamcommunity.com/app/403190/announcements/) · [BepInEx 5.4.23.6](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.6)

## Instalar

Descarga el [ZIP del mod en la sección de versiones](https://github.com/Nagisa-Kaw/planetbase-resource-menu/releases/latest). Incluye la DLL compilada y el código fuente.

1. Cierra Planetbase. En Steam, abre **Biblioteca → Planetbase → Administrar → Ver archivos locales**. La carpeta que se abre contiene `Planetbase.exe`.
2. Descarga el archivo oficial [BepInEx_win_x86_5.4.23.6.zip](https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.6/BepInEx_win_x86_5.4.23.6.zip) y extrae su contenido junto a `Planetbase.exe`. Si esa instalación ya tiene BepInEx 5, puedes usar su carpeta existente. [Guía de instalación de BepInEx](https://docs.bepinex.dev/v5.4.21/articles/user_guide/installation/index.html)
3. Extrae el ZIP del mod y copia su carpeta `BepInEx` sobre la carpeta de Planetbase. El archivo del mod debe quedar en `BepInEx\plugins\PlanetbaseResourceMenu\PlanetbaseResourceMenu.dll`.
4. Abre Planetbase desde Steam y entra a una partida. Pulsa **F8** una vez que tengas el control de la colonia.

Para retirar el mod, cierra el juego y saca `PlanetbaseResourceMenu.dll` de la carpeta `BepInEx\plugins`. Los recursos de las partidas que guardaste siguen siendo objetos y monedas normales de Planetbase.

## Usar el menú

1. Haz clic en el **icono** de un recurso. La ventana muestra el recurso seleccionado y cuántas unidades hay en la base.
2. Ajusta la cantidad con **−10**, **−1**, **+1** y **+10**. Cada entrega admite de **1 a 100** unidades.
3. Pulsa **Agregar a la base**. El mensaje al pie muestra cuántas unidades se agregaron. El mod usa primero los almacenes operativos y deposita el excedente en terreno accesible cercano. Si se agota el espacio, indica cuántas unidades pudo entregar. Las monedas se suman al saldo.
4. Cierra con **F8**, **Esc** o la **X**. La simulación se pausa mientras el menú está abierto. Guarda desde el menú del juego o con **F9** para conservar la entrega.

## Compilar

Abre PowerShell en la carpeta que contiene `PlanetbaseResourceMenu.csproj`. En el ZIP se llama `codigo`. Con el SDK de .NET 8 o posterior y Planetbase instalado, ejecuta lo siguiente. Ajusta la ruta si Steam usa otra biblioteca.

```powershell
$juego = 'C:\Program Files (x86)\Steam\steamapps\common\Planetbase'
dotnet build .\PlanetbaseResourceMenu.csproj -c Release "-p:PlanetbaseDir=$juego"
```

Si tienes BepInEx extraído en otra carpeta, agrega `"-p:BepInExCoreDir=RUTA\BepInEx\core"` al comando. La DLL queda en `bin\Release\netstandard2.1\PlanetbaseResourceMenu.dll`. El proyecto usa las bibliotecas de tu instalación como referencias.

## Comprobar una instalación

Carga una partida, pulsa **F8**, selecciona **Metal**, cambia la cantidad a **11** y comprueba que el total aumente en esa cantidad después de pulsar **Agregar a la base**. Repite con **Monedas** para verificar el saldo. Cierra la ventana, guarda la partida y vuelve a cargarla para verificar la persistencia. BepInEx registra cada entrega en `BepInEx\LogOutput.log`.
