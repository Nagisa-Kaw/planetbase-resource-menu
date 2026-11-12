# Validación de Energía nuclear y transporte 1.0.0

Fecha: 11 de noviembre de 2026.

## Entorno

- Planetbase 1.4.2 de Steam para Windows; Unity 2022.3.62f2 Mono, ejecutable de 32 bits.
- BepInEx 5.4.23.6 x86 y Harmony incluido en ese cargador.
- Compilación Release con .NET SDK 10.0.302: 0 errores y 0 advertencias.
- Carga simultánea de Menú de recursos, Terreno y velocidad y Ocio colonial.

SHA-256 de la DLL final:

```text
C25F2DFC3756B7D76554A58F38672AE0E1615AC9E2C77DEA5163535198E4A9D8
```

## Método

Se utilizó una copia separada de Planetbase, con una copia de una colonia, perfil de prueba y logros sin conexión. Se verificó desde el propio complemento de pruebas que la carpeta del perfil fuera la de QA y que el servidor de logros fuese Offline.

El complemento de pruebas preparó terreno, materiales, indicadores y alimentación controlados. Colocó los edificios mediante las funciones nativas y comprobó sus restricciones de colocación y conexiones. La construcción utilizó costes, materiales y rutinas del juego; se aceleró su finalización. No se jugó manualmente toda la construcción.

La fabricación de combustible se verificó con ingredientes físicos y una interacción de trabajo nativa iniciada por la prueba. La IA realizó la recogida del producto, la navegación y su entrega al depósito. La prueba de transporte con rover utilizó un colono y la IA nativa de llegada para entregar una caja a una obra.

Se verificaron cortes de servicio mediante las funciones del juego y se guardó la colonia por su sistema normal, con combustible parcialmente utilizado y un colono cargado. Después se cargó el archivo generado y se comprobaron los mismos recursos y estados.

El complemento de QA está separado del mod y no se instala en el juego habitual.

## Resultado

El registro de la ejecución final debe contener:

```text
NUCLEAR QA PASSED: 79 checks; native construction, fuel production, cooling, transport, navigation, save/reload.
```

Las 79 comprobaciones cubren:

1. Aislamiento del perfil y de los logros; registro del combustible al final de la lista para preservar índices originales.
2. Edificios traducidos, ayuda contextual y número de opciones de los menús interior y exterior.
3. Colocación válida, conexiones y finalización nativa de los cuatro edificios nuevos y un segundo reactor de prueba.
4. Colocación de la centrifugadora y el depósito y marcador de interacción de ingeniería.
5. Consumo de una unidad de mineral y una de metal para producir exactamente una barra. Sin entradas u operador no hay producción gratuita.
6. Elección del combustible fabricado por la IA, recogida, desplazamiento y entrega al depósito manteniendo el recurso físico.
7. Arranque sin combustible, recarga desde el depósito, eliminación única de la barra del inventario y generación de 60 kW.
8. Descuento de combustible según tiempo simulado; pausa, apagado manual y cortes de agua o refrigeración preservan el resto de la barra.
9. Reanudación tras recuperar refrigeración y asignación de una torre a un único reactor.
10. Duplicación de la velocidad exterior del colono cargado; dos vehículos por garaje; un tercer colono continúa a pie.
11. Liberación al descargar, reasignación, fin de viajes fuera del radio y rechazo de nuevos despliegues más allá de 120 m.
12. Retirada del vehículo al entrar en interiores o perder energía en el garaje.
13. Desplazamiento con carga por la navegación del juego y existencia del modelo del rover.
14. Entrega física de materiales a una obra mediante la IA de llegada del juego y liberación del vehículo al finalizar.
15. Serialización y recarga de los edificios, mobiliario, segundos de combustible y caja real del colono; reconstrucción de un solo rover y ausencia de combustible duplicado.
16. Agotamiento de la barra y parada sin combustible; ausencia de recarga gratuita.

La evidencia de texto de cada comprobación está en `verificacion/resultado-pruebas.txt`. Las imágenes `nuclear-overview.jpg` y `nuclear-rover.jpg` se capturaron en el juego real y se inspeccionaron visualmente. La primera muestra la ampliación y la segunda un vehículo con conductor y carga.

## Límites

No se ha medido el balance económico de una campaña de varias horas, el rendimiento en colonias masivas ni la compatibilidad con otros mods distintos de los tres indicados. Las rutas probadas son recorridos controlados; no representan todas las combinaciones de terreno, tormentas y congestión. Los eventos de inconsciencia y muerte se filtran en el código, pero no se provocaron en la prueba final.

Los vehículos llevan una caja y se despliegan para el servicio; no hay viaje separado hasta el aparcamiento. La recarga nuclear usa un circuito abstracto desde máquinas y depósitos conectados a la red. El mineral nativo representa la materia prima nuclear.

Los gráficos utilizan geometría propia sencilla y reutilizan modelos, animaciones y bases del juego. No se inspeccionaron todos los ángulos ni condiciones de iluminación. El registro de Unity conserva avisos de shaders del juego y avisos `List is null DescriptionItem` al construir de inmediato en QA; no se afirma que el registro completo esté libre de advertencias.

## Instalación local y guardados

La instalación habitual recibe únicamente `BepInEx/plugins/PlanetbaseNuclearTransport/PlanetbaseNuclearTransport.dll`. Los hashes de los otros complementos se cotejan antes y después de copiarla; el complemento de QA permanece fuera de esa instalación.

El ensamblado original del juego conserva este SHA-256:

```text
245441E4892D10551823334B7171EC712E88E7218913C20D8B44154CA98AF045
```

Las pruebas de construcción, fabricación, transporte y guardado se ejecutaron en la copia aislada. El mod queda disponible en la instalación habitual para el próximo inicio del juego. No se incorpora automáticamente ninguna construcción a una colonia del usuario.

Conserva el mod para cargar colonias que contengan sus tipos nuevos, o vuelve a un guardado anterior a usarlos si deseas retirarlo.
