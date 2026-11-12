# Energía nuclear y transporte — Planetbase 1.2.0

Expansión para **Planetbase 1.4.2 de Steam en Windows**, Unity 2022.3.62f2 Mono y **BepInEx 5 x86**. Añade edificios al menú normal de construcción, combustible fabricable y rovers que utilizan los colonos para transportar cajas en exteriores.

## Contenido

| Construcción | Menú | Coste | Función |
|---|---|---|---|
| Reactor nuclear | Exterior | 12 metal + 4 semiconductores | Genera 60 kW constantes mientras tiene combustible, agua y refrigeración. |
| Torre de refrigeración | Exterior | 5 metal + 1 semiconductor | Refrigera un reactor a 80 m, en la misma red. Consume 2 kW y 1 unidad de agua. |
| Garaje de rovers | Exterior | 7 metal + 2 bioplástico + 2 semiconductores | Incluye dos rovers; consume 3 kW. |
| Taller de combustible nuclear | Interior | 6 metal + 6 bioplástico + 2 semiconductores | Sala con centrifugadoras y depósitos; consume 2 kW además de sus máquinas. |

El reactor, el taller y el garaje se desbloquean al construir una fábrica. La torre requiere un extractor de agua. Cada edificio tiene un tamaño fijo en esta versión: 10 m para el taller y 7,5 m para las estructuras exteriores.

## Instalar

1. Cierra Planetbase.
2. Si ya usas los otros mods de esta carpeta, conserva su instalación de BepInEx. En una instalación nueva, instala [BepInEx 5.4.23.6 para Windows x86](https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.6/BepInEx_win_x86_5.4.23.6.zip) junto a `Planetbase.exe`.
3. Extrae el ZIP del mod y copia su carpeta `BepInEx` dentro de la carpeta del juego.
4. Comprueba que la DLL quede en `BepInEx\plugins\PlanetbaseNuclearTransport\PlanetbaseNuclearTransport.dll`.
5. Inicia Planetbase. El registro debe mostrar `Expansión lista: reactor, refrigeración, taller de combustible y garaje de rovers.`

El ZIP contiene únicamente el complemento y su código propio; necesitas una instalación legítima del juego y el cargador indicado.

## Poner en marcha el circuito nuclear

1. Conserva generación auxiliar solar/eólica para arrancar el taller y la torre.
2. Construye el **Taller de combustible nuclear** desde **Construir interior**, conectado a una sala con oxígeno.
3. Coloca dentro una **Centrifugadora de combustible**. Cuesta 2 metal y 1 semiconductor; consume 3 kW.
4. Un ingeniero fabrica una barra usando **1 mineral + 1 metal**, en **180 segundos de trabajo base**. El juego aplica su velocidad de trabajo habitual. Los colonos o robots suministran los ingredientes mediante su IA normal.
5. Puedes añadir un **Depósito de combustible**: cuesta 1 metal y 1 bioplástico y almacena 6 barras. Los transportistas llevan el combustible al depósito automáticamente. El reactor también puede tomar barras terminadas directamente de la centrifugadora.
6. Construye el **Reactor nuclear** y una **Torre de refrigeración**, y conecta todo a la misma red mediante las conexiones normales. La torre debe estar a 80 m o menos del reactor, medidos entre sus centros.
7. Asegura agua suficiente: el reactor demanda 2 unidades y la torre 1. La torre necesita además energía.

Cada barra alimenta el reactor durante **1800 segundos de simulación en funcionamiento**. La recarga desde los depósitos o centrifugadoras de la misma red representa un circuito automático de servicio; no hay un viaje individual de un colono hasta la vasija del reactor. Los recursos se retiran del inventario real una sola vez.

El mineral normal de Planetbase representa aquí la materia prima para el combustible. El mod no genera vetas nuevas de uranio. Las barras tienen su propio tipo, nombre, icono y persistencia; su caja reutiliza un modelo del juego.

El reactor se apaga al faltar combustible, agua o refrigeración, al desactivarlo o al quedar extremadamente dañado. Los cortes preservan la parte no consumida de la barra. Necesita repuestos y mantenimiento mediante el sistema normal del juego. Una torre atiende **un solo reactor**; se asignan primero los reactores construidos más antiguos. Un reactor habilitado reserva su torre aunque esté esperando combustible; desactívalo para liberarla.

Selecciona el reactor para consultar sus segundos de combustible, refrigeración y agua. La potencia indicada es bruta: el consumo de la torre, el taller y la maquinaria se descuenta en la red.

## Usar los vehículos

Construye y conecta un **Garaje de rovers** desde **Construir exterior**. Cada garaje incluye dos vehículos, financiados con su coste de construcción.

- Un colono que transporta una caja pesada en el exterior despliega un rover si está a 120 m o menos de un garaje operativo con vehículo disponible.
- Viaja al **doble de su velocidad normal con esa carga**. Se mantienen los efectos habituales de cansancio, salud y tormentas sobre su velocidad de referencia.
- Transporta **una caja real por viaje**. El vehículo muestra al conductor y la carga; sus ruedas giran durante el desplazamiento.
- Utiliza los destinos y rutas del juego: almacenamiento, abastecimiento y materiales de construcción. Los rovers no crean recursos ni los teletransportan a su destino.
- El tercer colono sigue a pie si los dos vehículos están ocupados. El vehículo queda disponible al descargar, entrar en una esclusa, iniciar una interacción, caer inconsciente o morir.
- Un viaje ya iniciado puede terminar fuera del radio de despliegue. Los rovers no circulan por las salas interiores. Al salir de una esclusa, el colono puede desplegar uno si cumple las condiciones.
- Si se corta la energía o se desactiva/recicla el garaje, el colono continúa usando el transporte normal del juego.

Los rovers son vehículos de servicio desplegables: no hay una fase separada en la que el colono camine al aparcamiento, ni carreteras nuevas, ni conducción manual. Las unidades estacionadas representan los vehículos disponibles. Los robots conservan su transporte habitual.

## Guardados y compatibilidad

Los edificios, las máquinas y las barras se guardan usando los tipos nativos registrados por el mod. La duración restante de la barra se guarda dentro del nodo del reactor. La carga de los colonos sigue usando el guardado normal del juego; las asignaciones de rover se reconstruyen al cargar.

Conserva el mod instalado para abrir las colonias que contengan sus edificios, mobiliario o combustible. Para retirarlo, vuelve a un guardado anterior a su uso. Guarda en una ranura nueva antes de comenzar una colonia con esta expansión.

Se comprobó su carga simultánea con **Menú de recursos**, **Terreno y velocidad** y **Ocio colonial**. Consulta `VALIDACION.md` para el entorno, las pruebas realizadas y sus límites. El balance de una campaña de varias horas todavía requiere pruebas de juego.

La actualización visual 1.2.0 sustituye por geometría propia la sala del taller de combustible, la centrifugadora, el armario blindado, las barras transportables y el rover de los colonos. El rover tiene cuatro ruedas detalladas, puesto abierto, volante, arco y caja de carga; el garaje muestra los mismos vehículos aparcados. Se conservan los diseños del reactor, la torre y las dos bahías de la versión anterior. Las piezas se agrupan por material y comparten mallas inmutables cuando corresponde. Las salas mantienen sus bases, suelos, colisiones, puntos de reparación y animaciones nativos; sus cascos propios se abren con el control de techos y con cada conexión. Los costes, recetas, potencias e identificadores de guardado conservan sus valores. `VALIDACION-INTEGRAL.md` enlaza las pruebas de esta actualización.

## Compilar

Necesitas .NET SDK 8 o posterior, el juego y BepInEx 5 x86. Conserva la carpeta hermana `PlanetbaseVisuals`, incluida con el código del paquete:

```powershell
$juego = 'C:\Program Files (x86)\Steam\steamapps\common\Planetbase'
dotnet build .\PlanetbaseNuclearTransport.csproj -c Release "-p:PlanetbaseDir=$juego"
```

La DLL queda en `bin\Release\netstandard2.1\PlanetbaseNuclearTransport.dll`. No se modifica `Assembly-CSharp.dll` ni se distribuyen ensamblados del juego.

La carpeta `qa` contiene el complemento de pruebas de desarrollo. No debe instalarse en una partida normal: prepara construcciones y recursos controlados y solo admite el perfil aislado utilizado en la validación.
