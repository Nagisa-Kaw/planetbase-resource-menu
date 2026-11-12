# Validación — Colonias en expansión 0.1.0 alfa

## Entorno y resultado

Planetbase 1.4.2 Steam, Unity 2022.3.62f2 Mono x86, Windows y BepInEx 5.4.23.6 x86. Se cargaron simultáneamente Colonias en expansión, Nuclear y transporte 1.2.0, Frontera Solar 0.3.0, Ocio colonial 1.2.0, Menú de recursos y Terreno y velocidad. Las compilaciones Release terminaron con cero errores y advertencias.

| Suite | Comprobaciones aprobadas |
|---|---:|
| Colonias en expansión, seis sistemas | 270 |
| Nuclear y transporte con la expansión cargada | 79 |
| Frontera Solar con la expansión cargada | 154 |
| Ocio colonial con la expansión cargada | 64 |
| **Total de esta entrega** | **567** |

## Evidencia funcional

La prueba comunitaria construye los 16 tipos de edificio con materiales y conexiones nativos, coloca los diez tipos de equipo nuevos y un armario médico nativo, comprueba los puntos de interacción y fabrica las tres unidades con sus ingredientes reales. Verifica la receta de suministros y el análisis de muestras.

El transportador recoge tres recursos almacenados, avanza por navegación nativa y los entrega en espacios reales del almacén de destino. Cancelarlo después de recargar deja las tres cargas sin alterar su número. El radar detecta un intruso físico; la torre lo daña, consume un repuesto para veinte cargas y se detiene sin energía. Un guardia avanza por su ruta de patrulla.

La expedición genera señales en lugares alcanzables, exige acercamiento y tiempo de exploración, recupera tres recursos físicos y guarda el regreso con las cargas. Cartografiar dos veces no duplica las señales. Los sitios agotados permanecen agotados tras cargar.

La ambulancia se acerca físicamente al herido, lo recoge, guarda y recarga con el paciente a bordo, atraviesa una esclusa y lo entrega a la interacción nativa de una cama de trauma. El tratamiento nativo mejora la salud y consume una medicina. Otro civil evacúa por la esclusa y los corredores. Terminar la emergencia recupera la alerta anterior y conserva una alerta roja que ya existiera.

La cafetería recupera moral mediante una interacción nativa. La academia cobra el coste, avanza solo durante el uso del puesto, pausa el progreso al perder energía, conserva el estudio y cambia realmente la profesión y su índice de personajes tras completarlo. Se conserva la identidad del colono.

Los contratos pagan depósito una sola vez, consumen la cantidad exacta de recursos físicos, liquidan su recompensa una vez y conservan la expedición cargada al guardar. La importación se paga al pedirla, espera si la plataforma no tiene energía y entrega tres medicinas físicas sin repetirlas al continuar la simulación.

El guardado conjunto incluye cinco tareas activas —carga, exploración, rescate, formación y patrulla— y un contrato en tránsito. Tras recargar se comprueban identidades de recursos, cantidad total, saldo, progreso, paciente, lugares agotados y estado comercial. Los módulos anteriores vuelven a pasar sus suites con esta DLL cargada. La prueba solar reconoce las tres especializaciones comunitarias adicionales sin cambiar los índices de recursos anteriores.

## Condiciones y límites

Se utilizó exclusivamente una copia local del juego y un perfil QA aislados, con acceso Steam deshabilitado durante las pruebas. El terreno y los suministros se preparan artificialmente. La rejilla exterior de navegación se reconstruye solo en esa fixture de QA después de aplanar; el mod de juego no reconstruye el terreno ni esa rejilla. La prueba no modifica la campaña habitual.

Se generaron 27 capturas de la colonia, incluidas las seis ventanas F10, y 37 vistas de modelos reales con cámara aislada. Se revisaron las hojas de los 37 modelos y capturas seleccionadas de interfaz e interiores. Las animaciones nativas pueden aparecer en transición en imágenes pausadas. No se afirma una inspección detallada a tamaño completo de las 27 capturas, una prueba manual de cada clic ni una campaña prolongada.

No se midieron FPS ni consumo de memoria en colonias masivas. El balance económico, múltiples configuraciones de terreno y otros complementos de terceros necesitan pruebas de la comunidad. Esta es una alfa; no equivale a una validación integral de todas las campañas posibles. Las rutas F10 tienen prioridad mientras estén asignadas; deben cancelarse para volver al control manual F9.

## Archivos probados e instalados

| DLL | Versión | SHA-256 |
|---|---|---|
| PlanetbaseCommunityExpansion | 0.1.0 | `603277F4F8EE41DCCFCFB695D7811C3253F6144A0B3AACB33095E005B74128A9` |
| PlanetbaseNuclearTransport | 1.2.0 | `AABAC8B7CAA60F4CD147BEA6A1ED4A8B97B33FE14FBB657D395D174DA3F2D27A` |
| PlanetbaseSolarFrontier | 0.3.0 | `B0E7BAAF2414E5F41D7B23EEA4C45304578A584A32B79FAC6ECCDC7DC54566EA` |
| PlanetbaseColonyLeisure | 1.2.0 | `06545757792FEC9AD08B95E732C3A120FBEA2B14169281C5CA43EAD41A87177F` |

Las cuatro DLL del ZIP se comparan con estas compilaciones y con la instalación local. El paquete no contiene partidas, bibliotecas del juego, BepInEx ni DLL de QA. Assembly-CSharp, Menú de recursos y Terreno y velocidad conservan sus hashes previos. La activación ocurre al iniciar Planetbase. Los registros publicados sustituyen las rutas personales por marcadores; los originales se conservan localmente.
