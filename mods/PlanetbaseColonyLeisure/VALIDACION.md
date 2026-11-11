# Validación de Ocio colonial 1.0.0

Fecha: 10 de noviembre de 2026.

## Entorno comprobado

- Planetbase 1.4.2 de Steam para Windows, build 20301361.
- Unity 2022.3.62f2 Mono, ejecutable de 32 bits.
- BepInEx 5.4.23.6 x86.
- Compilación Release con SDK .NET 10.0.302: 0 errores y 0 advertencias.
- Menú de recursos y Terreno y velocidad cargados al mismo tiempo.

SHA-256 de la DLL final, igual en la compilación, en las pruebas y en la instalación de Steam:

```text
9F8FF60A3A0E63BEDFAB49149E2F0D336C595AF06A0A1A63CFFEDD209E573DC2
```

## Prueba de ejecución

Se ejecutó el juego real con un complemento de pruebas temporal, en una copia separada de la instalación y de una colonia. La copia utilizó un perfil separado y el modo sin conexión. El complemento de pruebas no forma parte del mod ni se instala en el juego del usuario.

El resultado de la ejecución final fue:

```text
LEISURE QA PASSED: 64 checks; four native buildings, five furnishings, AI, morale, fatigue, betting and save/reload.
```

Se comprobaron los siguientes comportamientos:

1. Registro de los cuatro edificios y su inclusión en el menú interior normal, con nombres en español.
2. Ayuda contextual de los edificios y de los cuatro tipos de mobiliario.
3. Colocación válida de cuatro salas pequeñas mediante las reglas de Planetbase y corredores válidos que las conectan con una base existente.
4. Finalización de construcción con las rutinas y materiales nativos.
5. Colocación válida de cinco muebles: dos recreativas, una pista social, una terminal de casino y un asiento de meditación. Se verificó que todos tengan una posición de interacción.
6. Aceptación de una actividad de ocio por la regla de IA del juego para un colono con moral baja.
7. Recuperación de moral en la recreativa y aumento medido del 50% con un segundo jugador.
8. Recuperación de moral y pérdida de descanso en la pista. Un colono con mucho sueño termina la actividad.
9. Recuperación simultánea de moral y descanso mediante meditación.
10. El mobiliario del casino deja de estar operativo al cortar la energía de su sala.
11. La IA evita el casino sin fondos. Una visita iniciada sin monedas termina sin dar moral gratis.
12. Descuento único de 2 monedas y devolución al interrumpir una ronda pendiente.
13. Guardado nativo con una apuesta en curso. Se inspeccionaron los datos adicionales de la ronda dentro del guardado.
14. Carga del guardado conservando los cuatro edificios, sus identificadores y los cinco muebles.
15. Continuación de la ronda con el saldo original, sin segundo cobro ni reembolso por cargar.
16. Premio válido al completar los 30 segundos acumulados, sin pago repetido al actualizar otra vez ni reembolso después de resolverse.

La ejecución final acreditó un premio de 2 monedas. Otras ejecuciones previas de la misma prueba también produjeron resultados válidos de 0 y 6 monedas. Las probabilidades están definidas en el código, sin una prueba estadística extensa.

## Alcance de las pruebas

La colonia de prueba recibió una ampliación de terreno y suministros controlados para aislar las mecánicas. Las salas y los muebles se construyeron mediante las funciones del juego desde el complemento de pruebas. Esto verifica la integración de construcción e interacción, pero no representa una campaña jugada durante horas ni una medición del balance económico a largo plazo. La modificación de terreno pertenece exclusivamente a la preparación de la prueba y no está incorporada en este mod.

Se inspeccionaron dos capturas de la escena de prueba, con las cuatro salas conectadas y un acercamiento al club. Las salas reutilizan la geometría del juego. La inspección no cubre cada ángulo de cámara, cada tamaño ni todas las situaciones gráficas. El bucle musical está implementado, pero no se efectuó una evaluación auditiva.

El registro final no contiene excepciones del mod. Conserva avisos de shaders del juego y avisos `List is null DescriptionItem` durante la preparación inmediata de los edificios de prueba. No se afirma que el registro completo carezca de advertencias.

## Instalación local

Se copió únicamente `PlanetbaseColonyLeisure.dll` a `BepInEx\plugins\PlanetbaseColonyLeisure` en la instalación habitual de Steam. Se verificó que el contenido coincida con la DLL probada y que las DLL de los dos mods existentes conserven sus hashes. El ensamblado original de Planetbase conserva este SHA-256:

```text
245441E4892D10551823334B7171EC712E88E7218913C20D8B44154CA98AF045
```

No se cerró ni se reinició la sesión habitual desde esta tarea. El mod se cargará allí en el próximo inicio de Planetbase. Las construcciones, las apuestas y los guardados de la prueba se realizaron en el perfil aislado. No se modificó la colonia habitual del usuario desde el complemento de pruebas.
