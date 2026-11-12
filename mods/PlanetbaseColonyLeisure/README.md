# Ocio colonial para Planetbase

Versión 1.2.0. Cuatro edificios interiores nuevos y cuatro muebles con actividades para los colonos. Compatible con **Planetbase 1.4.2 de Steam para Windows**, Unity Mono de 32 bits y **BepInEx 5.4.23.6 x86**. Los textos añadidos están en español.

## Edificios y actividades

| Edificio | Mueble principal | Mecánica |
| --- | --- | --- |
| Sala de videojuegos | Máquina recreativa | Recupera moral. Con dos máquinas ocupadas en la misma sala, cada jugador recibe un 50% adicional de recuperación. |
| Club nocturno | Pista social | Recupera moral, consume descanso y termina la visita si el colono tiene mucho sueño. Incluye luces de colores y un pequeño bucle musical original que suena al usarse, con la simulación en marcha. |
| Casino | Terminal de casino | Cada visita apuesta 2 monedas de la colonia. A los 30 segundos de actividad se resuelve la ronda. Sin monedas suficientes, los colonos evitan las terminales. |
| Centro de bienestar | Asiento de meditación | Recupera moral y descanso simultáneamente. |

El club y el casino también admiten máquinas de bebidas, mesas de bar y fuentes. Las bebidas y las mesas conservan las reglas y consumos del juego. El club representa ocio nocturno para adultos sin contenido sexual explícito.

Los colonos eligen estas actividades mediante las reglas normales de ocio de Planetbase cuando necesitan recuperar moral. Hace falta construir los muebles dentro de cada edificio. Un edificio vacío no recupera moral por sí solo.

## Cómo usarlo

1. Cierra Planetbase por completo y vuelve a abrirlo tras instalar el mod.
2. Carga una partida normal y abre **Construir interior**. Busca **Sala de videojuegos**, **Club nocturno**, **Casino** o **Centro de bienestar**. Requieren una cantina construida.
3. Coloca el edificio y conecta un corredor a la base. Debe recibir energía y oxígeno y ser accesible.
4. Selecciona la sala construida y abre su menú de componentes para añadir los muebles nuevos. Deja que los trabajadores lleven los materiales y terminen la construcción.
5. Para el bono cooperativo, instala al menos dos máquinas recreativas en la misma sala. Para el casino, mantén monedas en el saldo de la colonia.

El menú de ayuda contextual describe los edificios y los muebles nuevos. Puedes combinar este mod con el Menú de recursos y con Terreno y velocidad.

## Costes y balance inicial

Los edificios admiten dos tamaños, de 10 y 12.5 metros. El pequeño cuesta **4 metales y 4 bioplásticos**, y el grande **6 metales y 6 bioplásticos**, más los corredores y el mobiliario. Necesitan energía de forma continua y oxígeno de la red de la colonia.

| Mueble | Materiales | Consumo eléctrico | Duración máxima | Recuperación máxima de una visita |
| --- | --- | --- | --- | --- |
| Máquina recreativa | 1 metal, 1 bioplástico, 1 semiconductor | 0.5 kW | 60 s | 33.3% de moral en solitario, hasta 50% con compañero |
| Pista social | 1 metal, 1 bioplástico | 0.35 kW | 45 s | 30% de moral, consume hasta 5% de descanso |
| Terminal de casino | 1 metal, 1 bioplástico, 1 semiconductor | 0.4 kW | 35 s | Hasta 14.6% de moral y una ronda por visita |
| Asiento de meditación | 1 metal, 1 bioplástico | 0.1 kW | 60 s | 20% de moral y 10% de descanso |

Cada mueble admite un colono a la vez. Los porcentajes son máximos teóricos antes de las pérdidas normales de necesidades, y la visita puede acabar antes si el colono recupera toda la moral, debe marcharse o el equipo deja de funcionar. Los segundos corresponden al tiempo de simulación. La velocidad del juego también acelera estas actividades.

La potencia base de las salas es 2 kW para videojuegos, 2.5 kW para el club, 2 kW para el casino y 1.5 kW para bienestar. Planetbase aplica su multiplicador por tamaño, además del consumo de los muebles. Los intervalos de reutilización por colono son 240 s para videojuegos y club, 480 s para casino y 180 s para meditación.

### Casino

- Se descuentan **2 monedas al comenzar** una visita válida, una sola vez.
- A los **30 segundos**: 60% de probabilidad de premio 0, 30% de premio 2 y 10% de premio 6. El premio es el total acreditado, no una ganancia adicional a la apuesta.
- Si la visita se interrumpe antes de resolver la ronda, se devuelve la apuesta. Una ronda resuelta no vuelve a pagarse ni se reembolsa al terminar.
- El saldo, el tiempo de la ronda y su estado se conservan al guardar y cargar. Una ronda en curso continúa sin cobrar de nuevo.
- El rendimiento medio es **1.2 monedas de premio por cada 2 apostadas**. Es una actividad de ocio con coste para la colonia.

## Instalar o actualizar

1. Abre la carpeta de Planetbase desde Steam con **Administrar → Ver archivos locales**.
2. Si ya tienes BepInEx por los otros mods, conserva esa instalación. Para una instalación nueva, extrae [BepInEx 5.4.23.6 para Windows x86](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.6) junto a `Planetbase.exe`.
3. Extrae el ZIP del mod y copia su carpeta `BepInEx` sobre la carpeta del juego. Debe quedar una sola copia de la DLL:

   `BepInEx\plugins\PlanetbaseColonyLeisure\PlanetbaseColonyLeisure.dll`

4. Inicia el juego. El registro `BepInEx\LogOutput.log` debe mostrar **Ocio colonial listo. Cuatro edificios nuevos en Construir interior.**

La DLL se carga al iniciar Planetbase. Copiarla mientras el juego está abierto requiere cerrar el juego y volver a abrirlo para activarla.

## Guardados y desinstalación

**Conserva un guardado anterior a la construcción del primer edificio de este mod.** Un guardado que contiene estos edificios o muebles necesita el mod para cargarse. Para retirarlo, carga una partida anterior a su uso, o elimina primero todos sus muebles y edificios desde el juego, guarda en una ranura nueva y comprueba ese guardado antes de quitar la DLL. Guarda con las visitas al casino terminadas.

El mod utiliza el formato de guardado del propio juego y añade los datos de la ronda del casino. No sobrescribe ni sustituye los archivos originales de Planetbase. La compatibilidad con otras versiones del juego u otros mods no está garantizada.

## Modelos y sonido

La actualización visual 1.2.0 sustituye las cuatro cúpulas visibles por cascos propios y cubiertas diferenciadas: cian en videojuegos, magenta en club, ámbar en casino y verde en bienestar, en ambos tamaños. Mantiene las bases, los suelos y las colisiones nativos. Los cascos se recortan en las conexiones y se ocultan con el control normal de techos. Las recreativas, terminales de casino y asientos de meditación tienen cuerpos propios; la pista conserva las baldosas luminosas y altavoces de la versión anterior. Los colonos conservan sus posiciones y animaciones originales de sentarse, trabajar e idle. Esta versión no incluye una animación de baile nueva. `VALIDACION-INTEGRAL.md` enlaza las pruebas de esta actualización.

La música del club se sintetiza en memoria con una secuencia original y sigue el volumen de música del juego. El paquete no contiene modelos, música, DLL ni otros recursos extraídos de Planetbase.

## Compilar

Necesitas el SDK de .NET 8 o posterior, el juego instalado y BepInEx 5. Conserva la carpeta hermana `PlanetbaseVisuals`, que contiene el código compartido de geometría. Desde la carpeta del proyecto (también incluida dentro de `codigo` en el paquete):

```powershell
$juego = 'C:\Program Files (x86)\Steam\steamapps\common\Planetbase'
dotnet build .\PlanetbaseColonyLeisure.csproj -c Release "-p:PlanetbaseDir=$juego"
```

Si BepInEx está en otra carpeta, añade `-p:BepInExCoreDir=RUTA\BepInEx\core`. La salida está en `bin\Release\netstandard2.1\PlanetbaseColonyLeisure.dll`. El proyecto usa las bibliotecas locales como referencias y no las copia al paquete.

## Validación

Consulta `VALIDACION.md` para conocer las comprobaciones efectuadas y sus límites.
