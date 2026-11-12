# Validación de Frontera Solar 0.1.0

Fecha: 11 de noviembre de 2026.

## Entorno y binarios

- Planetbase 1.4.2 Steam, Windows x86, Unity 2022.3.62f2 Mono.
- BepInEx 5.4.23.6 x86 y HarmonyX del cargador instalado.
- .NET SDK 10.0.302, compilaciones Release sin errores ni advertencias.
- Carga simultánea de Ocio colonial, Menú de recursos, Terreno y velocidad y Energía nuclear y transporte.

SHA-256 de Frontera Solar, igual en compilación, copia de QA, paquete e instalación local:

```text
7F955E1EA1EE826BE8FD212908716158D8B9D5C112AC66CDC70770332A1B7F2E
```

La dependencia Energía nuclear y transporte 1.0.0 conserva:

```text
C25F2DFC3756B7D76554A58F38672AE0E1615AC9E2C77DEA5163535198E4A9D8
```

## Aislamiento

Se utilizó una copia del juego con `Util.getFilesFolder` dirigido al perfil temporal de QA, y la interfaz de logros forzada a Offline por el complemento de pruebas. Ambos requisitos se comprobaron durante cada ejecución. Los guardados de preparación, construcción y recarga pertenecen a ese perfil, no a la colonia habitual.

El complemento de pruebas es independiente de las DLL de juego entregadas. Se retira al terminar las pruebas y no se instala en Steam.

## Prueba de la colonia

La ejecución final produjo:

```text
SOLAR QA PASSED: 154 checks; research, industry, units, scanning, radiation, save/reload.
```

Se ejercitaron las funciones reales de Planetbase dentro de su proceso. El complemento preparó terreno, alimentación e ingredientes controlados, aceleró la construcción y dirigió determinadas interacciones. Estas pruebas no representan una campaña manual de principio a fin.

Las comprobaciones incluyen:

1. Ocho mundos registrados, preservación de los cuatro índices originales y creación de ocho objetos en el selector nativo.
2. Conservación del índice 14 de la barra de combustible del mod anterior; registro de las tres especializaciones nuevas en la lista nativa de bots.
3. Colocación y construcción nativas de nueve estructuras nuevas, conexiones válidas y mobiliario. La planta de ensamblaje utiliza el tamaño de 12,5 m.
4. Producción física de informes mediante una interacción de trabajo de un ingeniero.
5. Consumo único de informes, dependencias del árbol y finalización de escaneo, geotermia, reciclaje y robótica.
6. Detención de una investigación local sin energía en el laboratorio.
7. Cobro único de planos terrestres y finalización del blindaje y la fusión.
8. Ubicación geotérmica explorada y producción real de entre 11 y 20 kW según su potencial.
9. Fabricación de piezas y chasis; transferencia de sus productos a un ensamblador y fabricación de un Scout Drone. Consumo de los tres ingredientes.
10. Fabricación alternativa de un camión y un bot escáner mediante el ensamblador nativo.
11. Producción de cápsulas, generación de 120 kW de fusión, descuento de combustible y apagado al perder agua.
12. Almacenamiento de una cápsula en un almacén normal usando ranuras nativas; consumo desde el almacén conectado tras agotar la anterior, sin duplicación.
13. Extracción de regolito y transformación en mineral nativo.
14. Producción de un residuo por visita sanitaria, reciclaje en fertilizante y consumo único en el invernadero.
15. Aumento medido del 25% en el progreso de producción nativa de carne sintética con fertilización.
16. Desplazamiento manual de un colono por rutas reales, escaneo de terreno y control de un bot portador nativo.
17. Orden de movimiento de un camión cargado manteniendo su recurso físico.
18. Salida completa de un Scout Drone desde una sala, paso por la esclusa y finalización del reconocimiento exterior. Se avanzaron sus rutinas, interacciones y navegación nativas en pasos pequeños.
19. Exposición radiológica usando el perfil de Europa sobre la escena controlada; protección por pilón, exposición al cortar energía y reducción posterior de dosis.
20. Erupción solar nativa: sin daño dentro del pilón operativo y con daño al cortar su energía.
21. Aparición de un intruso hostil en terreno alcanzable al activar la incursión opcional. Se retiró la unidad de la escena de pruebas después de comprobar su creación; no se simuló una batalla completa.
22. Guardado nativo con proyecto terrestre pendiente, combustible parcial, órdenes, exposición y visita sanitaria en curso.
23. Recarga de edificios, mobiliario, vehículos y tecnologías; conservación exacta del combustible y del progreso de investigación; ausencia de informes y residuos duplicados.
24. Finalización del proyecto terrestre después de cargar y creación de la ventana de gestión nativa.

El detalle de cada aserción está en `verificacion/colonia-pruebas.log`. `colonia-unity.log` conserva el registro completo de esa ejecución.

## Nuevas partidas en los cuatro destinos

Se arrancó un proceso separado por destino desde el título, usando el generador del juego, semilla 71111 y la misma DLL del mod. El aterrizaje se adelantó mediante actualizaciones nativas de 0,1 s; no se creó una población de sustitución desde el complemento.

| Destino | Registro | Comprobaciones |
|---|---|---:|
| Marte, índice 4 | `mundo-4.log` | 9 |
| Luna, índice 5 | `mundo-5.log` | 10 |
| Europa, índice 6 | `mundo-6.log` | 10 |
| Titán, índice 7 | `mundo-7.log` | 9 |

Las cuatro ejecuciones comprobaron aislamiento, registro e índices, generación del terreno, navegación exterior, ocho colonos desplegados por la nave y 60 unidades iniciales de metal. Luna y Europa también comprobaron la ausencia de atmósfera que desactiva la energía eólica.

Las cifras incluyen comprobaciones de entorno repetidas en cada proceso; no se presentan como 192 escenarios de juego independientes. No se completó una campaña ni un ciclo económico entero en cada destino.

## Inspección visual

Se capturaron y revisaron cinco imágenes del juego real:

- `solar-research.jpg`: proyectos, costes, estados completados y las tres secciones.
- `solar-control.jpg`: órdenes y capas.
- `solar-world.jpg`: características y opción de incursiones.
- `solar-overview.jpg`: estructuras conectadas y capa térmica explorada.
- `solar-vehicles.jpg`: modelos iniciales de las unidades fabricadas.

El panel se corrigió después de la primera revisión para evitar el solapamiento de los botones nativos. Las opciones largas utilizan filas con iconos de acción y texto legible. La inspección final cubre esas vistas a 1280 × 720 de captura; no todos los tamaños de pantalla, idiomas o ángulos.

Los modelos geométricos de los edificios exteriores y vehículos son provisionales. Se reutilizan bases, salas, texturas y animaciones del primer juego. El Scout Drone mantiene rutas terrestres aunque su modelo flota visualmente.

## Salida automática del entorno de pruebas

Una ejecución anterior solicitó `Application.Quit` desde el complemento de QA después de aprobar las comprobaciones. El proceso produjo un fallo nativo en `mono_domain_free` durante la salida.

Se repitió una prueba de control **sin cargar Frontera Solar ni su complemento de QA**, con los cuatro mods anteriores y un complemento mínimo que solo solicita salir desde el título. El registro confirma cinco complementos: los cuatro anteriores y `Baseline Exit QA`. También produjo el fallo nativo al salir.

La comparación está en `salida-control-sin-frontera.log` y `salida-control-sin-frontera-unity.log`. Demuestra que el fallo de esta ruta de salida también existe sin la expansión; no identifica por sí sola la causa del problema de Mono/cargador. No se afirma que se haya reparado el cierre normal del juego ni que esta prueba equivalga a salir desde su menú.

Las ejecuciones finales se dejaron en pausa tras completar las comprobaciones y se cerraron como procesos de prueba controlados. La compilación, ejecución, guardado y recarga finales no presentan excepciones administradas de Frontera Solar.

## Límites y adaptaciones

- Las colonias de cada mundo son independientes. No hay transporte entre planetas ni simulación simultánea.
- Las galerías son refugios en superficie; no hay excavación de túneles bajo montañas.
- La capa de calor es potencial geotérmico determinista, no una simulación termodinámica.
- El camión transporta una caja, usa la navegación de bots y conserva desgaste y reparación nativos. La sonda no sobrevuela obstáculos.
- La sanidad se adapta al servicio de hidratación; no se implementó una nueva necesidad fisiológica ni recuperación de aguas residuales.
- Las incursiones usan intrusos nativos; no hay nave pirata nueva, especie alienígena o comercio alienígena.
- No se midieron campañas largas, rendimiento en colonias masivas, balance exhaustivo ni todas las combinaciones de tormentas y terreno.
- Las interacciones de fabricación se prepararon desde QA. La ruta completa de una sonda y el movimiento manual de las unidades sí recorrieron navegación nativa; no se afirma haber probado físicamente cada ruta de cada máquina.
- Se comprobó carga simultánea con los cuatro mods indicados, no una regresión exhaustiva de todas sus funciones.

El registro de Unity contiene avisos de shaders del juego y `List is null DescriptionItem` durante la construcción inmediata del escenario; no se afirma ausencia de advertencias en todo el registro.

## Instalación local

La instalación habitual recibe únicamente la DLL de Frontera Solar. La dependencia nuclear ya instalada coincide con la incluida en el paquete. Se cotejan los hashes de los cuatro mods existentes antes y después de copiar la nueva DLL.

El ensamblado original `Assembly-CSharp.dll` conserva:

```text
245441E4892D10551823334B7171EC712E88E7218913C20D8B44154CA98AF045
```

No se instala ningún complemento de QA ni se agregan edificios a la colonia habitual automáticamente. La expansión se carga en el siguiente inicio de Planetbase. Conserva las dos DLL para abrir guardados que contengan sus tipos nuevos.
