# Frontera Solar 0.3.0

Expansión experimental para **Planetbase 1.4.2 de Steam, Windows x86 y BepInEx 5.4.23.6**, inspirada en las ideas públicas de Planetbase 2. Incluye el mod **Energía nuclear y transporte 1.2.0** como dependencia. La actualización 0.3.0 sustituye los cuerpos provisionales del dron, camión y bot escáner por vehículos propios; también incorpora siete envases de recursos con iconos distintos, nueve máquinas nuevas y cascos completos para las cinco salas. Conserva la fusión toroidal, geotermia con intercambiador, barrena y pilón de la versión anterior. Los suelos, bases, colisiones, animaciones y puntos de interacción nativos siguen dando soporte a los modelos nuevos. `VALIDACION-INTEGRAL.md` enlaza las pruebas de esta actualización.

## Comenzar

1. Inicia Planetbase después de instalar las dos DLL del paquete.
2. En una partida nueva aparecen **Marte, Luna, Europa y Titán** junto a los cuatro mundos originales.
3. Construye un laboratorio normal y después el **Laboratorio de investigación**. Instala una **Mesa de investigación** y deja trabajar a un ingeniero.
4. Abre **Gestión de la base → Frontera Solar**, o pulsa **F9**. Pulsa el icono junto al proyecto en la pestaña Investigación.
5. Para dar órdenes, selecciona primero un colono o robot, abre F9 y entra en **Unidades y capas**. Puedes moverlo, escanear, mantener posición o devolverlo al trabajo automático.
6. **F6** alterna las capas de calor, concentración de regolito y protección radiológica. Se centran en la selección actual; cambia la selección y vuelve a activar la capa para explorar otra zona.

Para comenzar con menos presión, usa Marte. Conserva una ranura de guardado anterior a la instalación y guarda tu colonia ampliada en una ranura nueva.

## Mundos

| Destino | Atmósfera | Luz solar efectiva | Riesgos y particularidades |
|---|---|---|---|
| Marte | Tenue | Referencia normal | Tormentas de polvo y meteoros; menor rendimiento eólico por atmósfera tenue. |
| Luna | Ninguna | Referencia normal | Meteoros, erupciones solares y radiación exterior continua moderada. |
| Europa | Ninguna | 20% de la referencia de alta luz | Radiación exterior intensa, meteoros, erupciones solares y hielo visual. |
| Titán | Densa | 10% de la referencia de alta luz | Muy poca luz y tormentas; alternativa visual basada en el mundo tormentoso. |

La radiación ambiental de esta expansión afecta a los colonos en exteriores: sin protección, la dosis alcanza el umbral aproximadamente en 180 s en Europa, 514 s en la Luna y 1800 s en Titán. Al llegar al umbral se aplica la enfermedad radiológica nativa. Dentro de salas o cerca de un pilón operativo, la exposición disminuye.

Los cuatro destinos nuevos están disponibles sin hitos. Los índices y requisitos de los mundos originales se conservan. Cada mundo se juega con **una colonia y un guardado independientes**. No hay viajes entre bases ni simulación simultánea de varias colonias.

## Investigación

Los informes científicos son recursos físicos producidos por ingenieros, a razón de uno por cada 90 s de trabajo base. Se pueden consumir desde sus mesas, el suelo o almacenes; un informe reservado por un transportista o en tránsito no cuenta como disponible.

| Proyecto | Requisitos | Coste | Tiempo base |
|---|---|---|---|
| Escaneo de recursos | Ninguno | 2 informes | 90 s |
| Energía geotérmica | Escaneo | 3 informes | 180 s |
| Reciclaje y cultivos | Escaneo | 2 informes | 120 s |
| Ensamblaje avanzado | Reciclaje | 3 informes | 240 s |
| Blindaje de radiación | Escaneo | 90 créditos | 300 s |
| Fusión controlada | Geotermia y ensamblaje avanzado | 4 informes + 120 créditos | 600 s |
| Planos terrestres de constructor | Ensamblaje avanzado | 100 créditos | 300 s |
| Planos terrestres de minero | Ensamblaje avanzado | 100 créditos | 300 s |

Solo se desarrolla un proyecto a la vez. Los costes se pagan al iniciarlo y no se repiten al cargar un guardado. Los proyectos locales necesitan un laboratorio de investigación operativo para avanzar. Los proyectos con créditos representan planos terrestres y tardan ×1 en Marte, ×0,5 en Luna, ×1,8 en Europa y ×2,2 en Titán. Durante una pausa no avanzan.

La disponibilidad de las tecnologías se integra con el sistema nativo. Los componentes y edificios correspondientes permanecen bloqueados hasta investigar sus requisitos. La tecnología de fusión combina dos ramas; no basta con comprar sus créditos.

## Construcciones nuevas

| Edificio | Función | Requisito adicional |
|---|---|---|
| Laboratorio de investigación | Mesas de investigación y fuente | Laboratorio normal |
| Planta de ensamblaje | Piezas, chasis, procesado de regolito, ensamblador y refinería de fusión | Fábrica normal |
| Planta de tratamiento | Unidades sanitarias y recicladoras | Tecnología de reciclaje |
| Invernadero avanzado | Tomates, trigo, cebolla, carne de laboratorio y depósito de fertilizante | Tecnología de reciclaje |
| Galería blindada | Refugio presurizado conectable mediante corredores | Esclusa |
| Planta geotérmica | Energía dependiente del calor descubierto | Tecnología geotérmica; calor ≥55% |
| Central de fusión | Generación de 120 kW brutos | Tecnología de fusión |
| Extractor de regolito | Extracción superficial automatizada | Planta de procesamiento |
| Pilón antirradiación | Cobertura radiológica exterior de 35 m | Planos de blindaje |

La planta de ensamblaje mide 12,5 m para admitir su maquinaria. Los demás interiores nuevos miden 10 m y los exteriores 7,5 m. Necesitan materiales y conexiones normales; los edificios interiores necesitan oxígeno. La galería utiliza una sala y corredores nativos en superficie: **no excava montañas ni añade un piso subterráneo**.

### Geotermia y escaneo

Los colonos y bots pueden recibir una orden de escaneo desde F9. Al llegar al punto deben permanecer allí durante 10 s de simulación. Se descubre un radio de 45 m, que queda guardado.

El Scout Drone y el bot escáner reconocen automáticamente puntos sin explorar de un anillo cercano a la primera estructura de la base. Después esperan nuevas órdenes. Puedes enviarlos más lejos desde F9.

La capa térmica muestra potencial geotérmico, desde azul a rojo; el terreno sin explorar aparece gris. La planta geotérmica solo admite ubicaciones exploradas con calor mínimo de 55%, y produce de 11 a 20 kW según ese valor. El campo de calor se genera de forma determinista por coordenadas y mundo; representa un recurso de esta expansión, no una simulación física de temperaturas.

### Fabricación por partes

| Máquina | Entrada | Salida | Trabajo base |
|---|---|---|---|
| Taller de piezas | 1 metal + 1 semiconductor | 1 juego de piezas | 120 s |
| Banco de chasis | 1 metal + 1 bioplástico | 1 chasis | 120 s |
| Ensamblador avanzado | 1 juego de piezas + 1 chasis + 1 semiconductor | 1 unidad elegida | 240 s |
| Procesador de regolito | 1 regolito | 1 mineral normal | 60 s |
| Refinería de cápsulas | 1 juego de piezas + 1 semiconductor; agua y energía | 1 cápsula de fusión | 240 s |

El ensamblador fabrica **Scout Drone, bot escáner, camión de carga, Carrier, Constructor y Driller**. Cambia el producto con los controles normales del componente. Constructor y Driller requieren sus planos respectivos. Los robots ensamblados respetan los límites de fabricación del juego.

Los transportistas nativos abastecen las máquinas y mueven sus productos. El camión conserva la IA de transporte y mantenimiento de los bots, transporta **una caja por viaje** y tiene un factor de velocidad de 1,8 respecto al bot portador normal. Las reparaciones y el desgaste siguen usando los sistemas nativos.

El Scout Drone tiene un modelo visual flotante, pero utiliza la navegación terrestre de Planetbase para conservar rutas, esclusas y guardados. No vuela por encima de montañas. Es una sonda adaptada al primer juego, utilizable también en mundos sin atmósfera.

### Fusión y energía nuclear

Cada cápsula alimenta una central de fusión durante **1200 s de funcionamiento**. Se toma de contenedores de máquinas operativas o de almacenes normales conectados a la misma red. Consume 3 unidades de agua. Si falta agua o combustible, se detiene sin perder el resto de la cápsula. Necesita repuestos.

La geotermia no usa cápsulas. El extractor de regolito consume 3 kW y produce según la concentración del terreno, con un límite de doce unidades sueltas próximas para evitar acumulación ilimitada sin transporte.

El mod nuclear incluido conserva reactor de 60 kW, torre de refrigeración, taller de combustible y garaje de dos rovers para colonos. Consulta su guía adjunta para esa cadena. Las potencias indicadas son brutas: se descuenta el consumo de salas y maquinaria.

### Reciclaje y alimentación

La unidad sanitaria adapta el servicio de hidratación del juego. Una visita activa de al menos 10 s produce una unidad de residuos orgánicos; cada visita produce una sola, también al guardar y cargar.

Un obrero transforma un residuo en un fertilizante en 60 s de trabajo base. Los transportistas abastecen el depósito del invernadero automáticamente. Cada fertilizante se consume para dar **+25% a la velocidad de producción** de cultivos y sintetizadores de carne de esa sala durante 600 s. La recuperación de agua no forma parte de esta versión.

Se conservan los cultivos, recetas y carne sintética del juego original. El invernadero los amplía con el circuito de fertilización.

## Microgestión

Selecciona una unidad antes de abrir F9. Las órdenes de movimiento y escaneo se asignan con un clic en terreno exterior. Desde un interior, la unidad busca una esclusa normal; si no existe una ruta, rechaza o cancela la orden.

Mantener posición suspende su trabajo automático. Volver a trabajo automático libera la orden. Las órdenes tienen un límite de 600 s y se interrumpen cuando salud, oxígeno, condición o integridad requieren atención. Las interacciones en curso y el combate no se interrumpen para asignar un desplazamiento.

Puedes dirigir colonos y bots nativos, las unidades nuevas y al colono que utiliza un rover del mod nuclear. El camión es una unidad robotizada independiente. No hay control manual de armas, minería con el ratón o una cola de órdenes múltiples.

## Amenazas

Las tormentas, meteoros y erupciones solares son los eventos nativos, configurados por mundo. Los pilones operativos protegen contra la radiación ambiental y se integran con la protección nativa durante erupciones solares.

Las **incursiones de saqueadores están desactivadas por defecto**. Puedes activarlas en F9 → Mundo y amenazas. Cada 1800 s se intenta introducir un intruso hostil nativo en terreno alcanzable, a unos 160 m de la primera estructura. Requieren las defensas y guardias del juego. Si no se encuentra un lugar válido, se omite ese intento.

Estas incursiones no tienen una nave pirata nueva. No se incluye una especie alienígena ni comercio extraterrestre; esos puntos especulativos de la lista no se presentan como contenido confirmado del tráiler.

## Instalación y guardados

Con Planetbase cerrado, copia la carpeta `BepInEx` del paquete sobre la carpeta del juego. Deben quedar estas DLL:

```text
BepInEx/plugins/PlanetbaseNuclearTransport/PlanetbaseNuclearTransport.dll
BepInEx/plugins/PlanetbaseSolarFrontier/PlanetbaseSolarFrontier.dll
```

Actualiza también la DLL nuclear a la 1.2.0 incluida en el paquete para recibir el taller, la maquinaria, el combustible y los rovers rediseñados. Conserva una sola DLL por mod. No necesitas volver a instalar BepInEx.

Conserva ambas DLL para abrir colonias que contengan sus mundos, recursos, tecnologías o unidades. Para retirar la expansión, vuelve a un guardado anterior a utilizarla. Se guardan progreso de investigación, tiempos de combustible, fertilización, áreas exploradas, órdenes, dosis de radiación y opciones de incursiones.

Es una versión inicial: el balance de campañas largas, colonias masivas y todos los recorridos posibles requieren más pruebas. `VALIDACION.md` documenta lo comprobado y sus límites.

## Código y extensiones

El paquete incluye el código propio completo de los dos mods y el complemento de QA por separado. No distribuye ensamblados, prefabs, texturas ni código del juego.

Desde `codigo/PlanetbaseSolarFrontier`:

```powershell
$juego='C:\Program Files (x86)\Steam\steamapps\common\Planetbase'
dotnet build .\PlanetbaseSolarFrontier.csproj -c Release "-p:PlanetbaseDir=$juego"
```

Necesitas .NET SDK 8 o posterior, Planetbase y BepInEx 5 x86. Conserva las carpetas hermanas `codigo/PlanetbaseNuclearTransport` y `codigo/PlanetbaseVisuals`: contienen la dependencia y el código compartido de geometría.

Otros complementos pueden consultar `Research.Has`, `Survey.Heat`, `Survey.Richness` e `IsScanned`, asignar órdenes mediante `ManualControl.Issue/Hold/Release` y suscribirse a `SolarApi.SurveyCompleted` y `ResearchCompleted`. Ejecuta las llamadas de Unity en su hilo principal. Los suscriptores de esos eventos se aíslan si lanzan una excepción. Esto es una API de esta expansión; el primer juego sigue necesitando BepInEx.

## Referencias de diseño

- [Presentación oficial de Planetbase 2](https://store.steampowered.com/app/3509420/Planetbase_2/).
- [Teaser publicado por Madruga Works](https://www.youtube.com/watch?v=eL_VGHLvpzg).
- [Diarios oficiales: geotermia, residuos, fusión, ensamblaje y Europa](https://steamcommunity.com/app/3509420/allnews/).

Las reglas y cifras de Frontera Solar son decisiones de esta adaptación. No representan una reproducción exacta de Planetbase 2 ni una expansión oficial de Madruga Works.
