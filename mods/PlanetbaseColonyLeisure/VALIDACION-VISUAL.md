# Validación de la actualización de arquitectura

Fecha: 11 de noviembre de 2026. Planetbase 1.4.2 de Steam para Windows, Unity 2022.3.62f2 Mono x86 y BepInEx 5.4.23.6 x86. Nuclear y transporte 1.1.0; Ocio colonial 1.1.0; Frontera Solar 0.2.0.

## Resultado

Las tres DLL compilaron en Release sin errores ni advertencias. Se ejecutó el juego real en la copia y el perfil separados `PlanetbaseResourceMenu-qa`, con la interfaz de logros sin conexión. Las cuatro suites terminaron sin excepciones gestionadas en sus registros finales:

| Suite | Comprobaciones | Registro |
| --- | ---: | --- |
| Arquitectura | 207 | `evidencia/arquitectura-pruebas.log` |
| Nuclear y transporte | 79 | `evidencia/nuclear-pruebas.log` |
| Frontera Solar | 154 | `evidencia/solar-pruebas.log` |
| Ocio colonial | 64 | `evidencia/ocio-pruebas.log` |
| Total | 504 | Cuatro ejecuciones independientes. |

## Integración visual

- Carga del guardado de prueba anterior de Frontera Solar 0.1.0 con sus nueve edificios, maquinaria, recursos y vehículos.
- Colocación y construcción mediante las rutinas, costes y materiales nativos de los edificios que faltaban; conexiones con la colonia y comprobación de los tamaños grandes de las cuatro salas de ocio.
- Nuevo guardado de la galería y recarga. Los 17 tipos de edificio y las cuatro variantes grandes conservan sus modelos.
- Materiales propios usando el shader iluminado nativo `Planetbase/OpaqueEmission`. Entre tres y nueve grupos de material en el grupo principal de arquitectura de cada edificio de esta escena, dentro del límite comprobado de doce. Las decoraciones no añaden colliders.
- Comparación de los marcadores de anclaje, transición y recursos de los equipos frente a sus prefabs nativos. Se conservan sus nombres y posiciones locales.
- Comprobación de que los refuerzos añadidos siguen el estado consultado de las cubiertas nativas y la regla de visibilidad por distancia. Las cúpulas de vidrio conservan el comportamiento del juego; no se sustituye todo su sistema de cubiertas.
- 24 capturas reales a 1600 × 1000, tomadas al final del fotograma. La galería usa los JPEG originales, sin retoque ni sustitución por arte conceptual. Se revisaron las instalaciones exteriores, las salas y el equipo interior.

La geometría del extractor que sirve como plantilla se oculta conservando sus mallas y límites: el juego usa esos límites para calcular el alcance de reparación. Las piezas temporales se separan antes de clonar el prefab, por lo que solo se clonan los grupos terminados. El garaje conserva los dos objetos de rover que usa su controlador.

## Regresión funcional

La suite nuclear comprobó producción de combustible, refrigeración, generación, agotamiento de la barra, transporte real de una caja, navegación del conductor, reconstrucción del rover y guardado y carga sin duplicar combustible. Su verificación del índice de combustible comprueba el índice 14 original, permitiendo los recursos solares que se registran después.

La suite solar comprobó investigación y entregas, cadenas de producción, ensamblaje de unidades, fertilización, consumo de fusión, órdenes individuales, esclusas y navegación, escaneo, radiación, protección y persistencia. El complemento solar conservó sus 154 comprobaciones.

La suite de ocio comprobó elección por la IA, uso de las recreativas, bono cooperativo, moral, fatiga del club, recuperación de bienestar, cobro y devolución de apuestas y guardado y carga de una ronda sin duplicar cargos. La DLL de ocio probada coincide con la entregada; los últimos ajustes de posición de maquinaria se limitaron a los modelos nuclear y solar.

`evidencia/comparacion-fuentes.json` compara las fuentes con la copia anterior: los tipos, recetas, costes, control, investigación, industria, actividades, guardado y demás archivos de lógica permanecen idénticos. Los cambios se concentran en modelos, sus llamadas de creación, metadatos de versión y documentación. El bucle musical anterior del club se conserva.

## Binarios y archivos preservados

Los SHA-256 finales son:

```text
1A61781FEA206B4C0713504594EE42F2AB26472B841DEC167D9857CD5BA18E7C  PlanetbaseColonyLeisure.dll
C76E814B858BAD63A7FD28621235C56D651EA3109634F110432E3BFB8A906CBC  PlanetbaseNuclearTransport.dll
E28506C4247D3D8199A14D307A3898FEF7B389D8B63F6C3612D0D810316B547A  PlanetbaseSolarFrontier.dll
```

Se comparan las DLL de compilación, copia de pruebas, instalación y ZIP. El paquete contiene exactamente tres DLL de mods, el código propio y las pruebas; excluye bibliotecas del juego, partidas y DLL de QA.

Las siguientes bibliotecas de la instalación normal mantienen sus SHA-256 anteriores:

```text
245441E4892D10551823334B7171EC712E88E7218913C20D8B44154CA98AF045  Assembly-CSharp.dll
C7DFC75B0BCA0005B7C6FD884C861B020094511DA5ECECA58A054C6395AC5AAC  PlanetbaseResourceMenu.dll
0FB109699F6C4D7E13BF76231FE560ECE2EB09B895957EBB723A0093221C2D00  PlanetbaseTerrainAndSpeed.dll
```

## Límites

Las pruebas utilizan una colonia preparada, suministros controlados y terreno aplanado en el perfil aislado. La preparación del terreno se repite después de cargar para la sesión de fotografías, porque esas llamadas de preparación no se serializan como parte de la partida. No se modificó una colonia habitual del usuario.

Esto valida las rutas descritas y el guardado utilizado; no garantiza todas las campañas, tamaños de colonia, modificaciones de terceros o configuraciones gráficas. No es una medición de FPS ni una evaluación de balance a largo plazo. El acabado es una adaptación al motor de Planetbase, no una reproducción exacta del render conceptual.

Los complementos de prueba se retiraron de la carpeta de plugins de QA al terminar. La copia de pruebas se cerró desde fuera del proceso; no se usó `Application.Quit()` en los complementos. La instalación normal carga las DLL nuevas en su siguiente inicio.
