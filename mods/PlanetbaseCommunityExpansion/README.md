# Colonias en expansión — alfa comunitaria 0.1.0

Seis sistemas nuevos para **Planetbase 1.4.2 de Steam, Windows x86, Unity Mono y BepInEx 5.4.23.6 x86**. Defensa, logística, expediciones, rescate, vida colonial y comercio comparten la ventana **F10** y guardan sus datos por colonia.

La colección incluye **16 edificios, 10 equipos, tres robots fabricables, un vehículo de patrulla, tres recursos y una lanzadera comercial de servicio**. Requiere **Nuclear y transporte 1.2.0** y **Frontera Solar 0.3.0**. El paquete conjunto también incluye **Ocio colonial 1.2.0** y los modelos rediseñados de esas versiones.

Es una **alfa pública para probar y enviar comentarios**. La validación funcional y sus límites se describen en `VALIDACION.md`; el balance de campañas prolongadas y el rendimiento en colonias grandes todavía requieren pruebas.

## Instalación

1. Conserva una ranura de guardado anterior a usar la colección y cierra Planetbase.
2. Instala BepInEx 5.4.23.6 **x86** junto a `Planetbase.exe`, o conserva el cargador que ya utilizas con estos mods.
3. Copia la carpeta `BepInEx` del paquete sobre la carpeta del juego. Debe quedar una sola DLL por complemento:

```text
BepInEx/plugins/PlanetbaseNuclearTransport/PlanetbaseNuclearTransport.dll
BepInEx/plugins/PlanetbaseSolarFrontier/PlanetbaseSolarFrontier.dll
BepInEx/plugins/PlanetbaseColonyLeisure/PlanetbaseColonyLeisure.dll
BepInEx/plugins/PlanetbaseCommunityExpansion/PlanetbaseCommunityExpansion.dll
```

4. Inicia el juego. El registro debe mostrar `Colonias en expansión 0.1.0 alfa lista. F10: seis módulos de gestión.`
5. Los edificios se añaden a los menús normales. Los de esta expansión requieren una cantina construida; la flota robotizada necesita investigar **Ensamblaje avanzado** en Frontera Solar.

No necesitas Menú de recursos ni Terreno y velocidad para jugar. Son compatibles con la colección en las pruebas realizadas. El paquete no incluye el juego, sus bibliotecas, BepInEx ni los complementos de pruebas.

## Defensa

| Construcción | Función |
|---|---|
| Centro de seguridad | Admite consola de seguridad avanzada, armería y fuente. Habilita torres y patrullas mientras está operativo. |
| Radar de vigilancia | Detecta intrusos exteriores a 140 m y avisa una vez por intruso. Consume 3 kW. |
| Torre defensiva | Consume 4 kW y dispara a intrusos exteriores a 50 m, sin obstáculos del terreno, cada 4 s. Un repuesto de la red proporciona 20 disparos. |
| Puesto de patrulla | Despliega un vehículo para un guardia asignado desde F10. Consume 1,5 kW. Cada puesto admite una patrulla. |

Selecciona un guardia sano y libre, abre **F10 → Defensa** y asígnale una patrulla. Recorre puntos alcanzables a unos 24 m del puesto. Omite puntos bloqueados, abandona la patrulla al necesitar atención y devuelve el control al combate nativo si un intruso se aproxima.

El interruptor de incursiones utiliza el sistema opcional existente de Frontera Solar: un intruso cada 1800 s. **Permanece desactivado por defecto.** No añade otra fuente paralela de ataques ni una nave pirata.

## Logística

Construye un **Centro de distribución** y coloca una **Consola de distribución**, un **Ensamblador de flota** y, si lo deseas, una **Empaquetadora de suministros**. El **Almacén logístico** tiene dos tamaños y admite los recursos normales; sirve como origen o destino junto con los almacenes del juego.

El ensamblador utiliza un ingeniero, **1 juego de piezas + 1 chasis + 1 semiconductor** y 240 s de trabajo base para fabricar una de estas unidades mediante los controles normales de producción:

| Unidad | Uso |
|---|---|
| Transportador pesado | Rutas entre almacenes con hasta tres cajas físicas. Factor de velocidad 1,45 respecto a la referencia del bot nativo. |
| Ambulancia rover | Recogida y traslado de heridos exteriores. Factor 1,6. |
| Rover de expedición | Exploración y regreso con hallazgos. Factor 1,55. |

Para una ruta:

1. Selecciona el almacén de origen, abre **F10 → Logística** y guarda el origen.
2. Cierra la ventana, selecciona otro almacén y guarda el destino.
3. Elige el material con el botón de cambio.
4. Cierra la ventana, selecciona el transportador pesado y pulsa **Iniciar ruta**.

Ambos almacenes deben pertenecer a la misma red. La unidad carga hasta tres recursos, recorre la ruta nativa y los deposita en espacios reales del destino. Espera si falta energía o espacio. Cancelar la ruta o perder la unidad deja las cargas en el mundo. No transporta monedas ni teletransporta recursos.

El selector de prioridad da preferencia a combustible, medicinas o alimentos al abastecer componentes de almacenamiento compatibles con la IA nativa. No impone cuotas globales ni restringe el contenido del almacén. Para utilizar órdenes manuales de F9 en una unidad asignada, cancela antes su tarea desde F10.

## Expediciones

Construye un **Centro de expediciones** con un **Analizador de muestras**. La **Baliza de expedición** amplía la búsqueda si está operativa antes de cartografiar. En **F10 → Expedición**, cartografía las señales de la colonia y envía una unidad sana y libre.

Se buscan hasta seis lugares sobre nodos alcanzables: sondas averiadas, suministros perdidos y vestigios ficticios de expediciones anteriores. El terreno disponible puede producir menos señales. Se generan una sola vez por colonia y los lugares explorados permanecen agotados al recargar.

La unidad llega al lugar, explora durante 10 s y carga tres hallazgos reales. Regresa físicamente al centro por rutas y esclusas. Los hallazgos incluyen **muestras de campo**, **vestigios recuperados**, suministros y piezas. El analizador convierte una muestra en un informe científico mediante un ingeniero y 90 s de trabajo base.

Los sitios son elementos de exploración en superficie. No generan cuevas, otra colonia, viajes interplanetarios ni una civilización alienígena.

## Rescate y emergencias

El **Hangar de rescate** habilita la búsqueda automática de heridos exteriores a 180 m. Fabrica una ambulancia y construye un **Hospital de emergencia** con una **Cama de trauma** libre. La ambulancia se desplaza hasta un colono herido, lo recoge y atraviesa las esclusas hasta el hospital. El paciente recibe oxígeno y protección durante el traslado; al llegar utiliza la atención **nativa de un médico con medicamentos**.

Cada paciente y cama se reservan para una ambulancia. La falta de servicio pausa el traslado. Cancelarlo deja al paciente junto al vehículo. La ambulancia no cura instantáneamente ni resucita colonos muertos.

Un **Refugio de emergencia** admite bancos, agua y camas privadas. Declarar una emergencia en F10 activa la alerta amarilla si no existía otra alerta y dirige a civiles exteriores sanos hacia un refugio operativo. Utilizan sus rutas y esclusas. Los guardias conservan su función. Finalizar la emergencia restituye el estado anterior sin cancelar una alerta roja que ya existía.

## Vida colonial

| Construcción | Contenido |
|---|---|
| Residencias privadas | Camas individuales y plantas decorativas. Las camas recuperan descanso con tiempo base de 180 s y moral con tiempo base de 400 s. |
| Cafetería comunitaria | Asientos cómodos, máquinas de comida, bebidas y fuentes; consumos y alimentación nativos. |
| Academia colonial | Puestos de formación para cambiar la profesión de un colono seleccionado. |

Selecciona un colono sano, sin carga ni otra tarea, abre **F10 → Vida colonial** y elige obrero, biólogo, ingeniero, médico o guardia. Se requieren **2 informes libres de la misma red, 20 créditos y 300 s de estudio** en un puesto operativo. Se conserva la identidad del colono, se actualizan su profesión y los índices nativos, y cambia su uniforme.

La falta de energía pausa el progreso. El guardado conserva el estudio pendiente. Cancelar la formación, retirar su puesto o desactivar su automatización pierde el coste ya pagado. No introduce niños, familias, envejecimiento ni reproducción.

## Comercio

Construye una **Terminal comercial** con consola y una **Plataforma logística** en la misma red. La empaquetadora transforma **1 metal + 1 bioplástico** en un paquete de suministros mediante un ingeniero y 90 s de trabajo base.

| Contrato | Entrega | Recompensa | Plazo |
|---|---|---:|---:|
| Suministros para una expedición | 4 paquetes de suministros | 220 créditos | 900 s |
| Pedido industrial | 5 juegos de piezas | 200 créditos | 1200 s |
| Combustible para una estación | 2 barras nucleares | 180 créditos | 1500 s |
| Colección de vestigios | 1 vestigio recuperado | 160 créditos | 1800 s |

Aceptar cobra un depósito de 30 créditos. **Entregar** consume las unidades libres del contrato en la misma red; excluye recursos cargados, negociados o reservados por otra unidad. La expedición dura 45 s con servicio operativo. Después paga la recompensa y devuelve el depósito una sola vez. El plazo avanza mientras esperas entregar; vencer o cancelar antes de entregar pierde el depósito y conserva tus materiales. Una expedición cargada debe terminar y pausa su tiempo si falta servicio.

Puedes importar 4 metales por 100 créditos, 3 medicinas por 120, o 2 juegos de piezas y 1 semiconductor por 180. Se pagan al pedir y llegan físicamente junto a la plataforma tras 60 s de servicio. Solo puede haber una operación comercial activa y hay 120 s de preparación entre operaciones.

La lanzadera es una representación visual del servicio. La liquidación de contratos contra el inventario de red no representa un viaje individual del transportador hasta la nave. La plataforma es exclusiva de este sistema y no admite visitantes nativos ni transporta colonos entre mundos.

## Guardados y controles

- **F10:** gestión de los seis módulos; también aparece en Gestión de la base.
- **F9/F6:** conservan los controles y capas de Frontera Solar.
- Cada tema permite activar o desactivar su automatización. Los edificios y tipos permanecen registrados para cargar la colonia. Las tareas canceladas dejan sus cargas físicamente; el comercio desactivado conserva sus operaciones pendientes hasta reactivarse.
- Guarda la colonia ampliada en una ranura nueva. Para cargar guardados que contengan este contenido necesitas mantener instaladas sus DLL y dependencias. Desactivar un tema no equivale a poder desinstalar la DLL.

## Código, imágenes y comentarios

El paquete incluye las fuentes propias en `codigo`, manteniendo juntas las carpetas `PlanetbaseCommunityExpansion`, `PlanetbaseSolarFrontier`, `PlanetbaseNuclearTransport`, `PlanetbaseColonyLeisure` y `PlanetbaseVisuals`.

```powershell
$juego='C:\Program Files (x86)\Steam\steamapps\common\Planetbase'
dotnet build codigo/PlanetbaseCommunityExpansion/PlanetbaseCommunityExpansion.csproj -c Release "-p:PlanetbaseDir=$juego"
dotnet build codigo/PlanetbaseColonyLeisure/PlanetbaseColonyLeisure.csproj -c Release "-p:PlanetbaseDir=$juego"
```

Requiere .NET SDK 8 o posterior y referencias de tu instalación legítima. Los archivos de `qa` solo se usan en una copia y perfil aislados, con construcciones y suministros preparados; no se instalan como complementos de juego.

La [galería pública](https://github.com/Nagisa-Kaw/planetbase-resource-menu/blob/main/docs/ColoniasEnExpansion/GALERIA.md) muestra los modelos reales y capturas de la colonia de pruebas. El visor `Galeria.html` del paquete consulta las imágenes públicas y requiere conexión; el visor del espacio de trabajo conserva sus imágenes locales. Para comunicar un fallo, abre un issue en el repositorio e indica versión del juego y del paquete, complementos instalados, pasos para reproducirlo y el fragmento relevante de `BepInEx/LogOutput.log`.

Mod comunitario no oficial. Los nuevos diseños siguen el estilo de la colección y conservan las bases, colisiones, marcadores y animaciones nativos que utiliza Planetbase.
