# Galería de Colonias en expansión

Alfa 0.1.0. Modelos reales renderizados con el motor de Planetbase, en una copia aislada. El encuadre cambia por objeto.

![Seis sistemas de la expansión](hero.jpg)

## Defensa

| Modelo | Vista |
|---|---|
| **Centro de seguridad**<br>Instala una consola de seguridad. F10 organiza patrullas y alertas; el guardia de consola conserva su trabajo nativo. | ![Centro de seguridad](modelos/ModuleTypeSecurityCommand.png) |
| **Radar de vigilancia**<br>Detecta intrusos exteriores a 140 m y avisa una vez por intruso. Necesita energía. | ![Radar de vigilancia](modelos/ModuleTypeSurveillanceRadar.png) |
| **Torre defensiva**<br>Dispara a intrusos exteriores a 50 m sin obstáculos del terreno. Consume energía y un repuesto por 20 disparos. Requiere centro de seguridad operativo. | ![Torre defensiva](modelos/ModuleTypeDefenseTurret.png) |
| **Puesto de patrulla**<br>Despliega un vehículo de patrulla para un guardia asignado desde F10. El guardia usa rutas alcanzables y deja la patrulla ante necesidades críticas. | ![Puesto de patrulla](modelos/ModuleTypePatrolDepot.png) |
| **Consola de seguridad avanzada**<br>Puesto de trabajo nativo para un guardia. El centro de seguridad habilita las torres y patrullas. | ![Consola de seguridad avanzada](modelos/SecurityDispatch.png) |
| **Vehículo de patrulla**<br>Vehículo asociado al guardia asignado a un puesto de patrulla. | ![Vehículo de patrulla](modelos/PatrolCar.png) |

## Logística

| Modelo | Vista |
|---|---|
| **Centro de distribución**<br>Consolas, empaquetadoras y ensambladores. F10 configura rutas entre almacenes para transportadores de hasta tres cajas. | ![Centro de distribución](modelos/ModuleTypeLogisticsHub.png) |
| **Almacén logístico**<br>Almacén nativo de dos tamaños. Puede ser origen o destino de rutas; admite los mismos recursos físicos que un almacén normal. | ![Almacén logístico](modelos/ModuleTypeFreightWarehouse.png) |
| **Consola de distribución**<br>Abre F10 para definir origen, destino, material y transportador. | ![Consola de distribución](modelos/LogisticsConsole.png) |
| **Ensamblador de flota**<br>Un ingeniero utiliza 1 juego de piezas, 1 chasis y 1 semiconductor para fabricar un transportador, ambulancia o rover de expedición. Requiere robótica. | ![Ensamblador de flota](modelos/VehicleFabricator.png) |
| **Empaquetadora de suministros**<br>Un ingeniero transforma 1 metal y 1 bioplástico en un paquete de suministros en 90 s de trabajo base. | ![Empaquetadora de suministros](modelos/SupplyPacker.png) |
| **Transportador pesado**<br>Hasta tres cajas físicas por viaje entre almacenes de una misma red. | ![Transportador pesado](modelos/FreightHauler.png) |

## Exploración

| Modelo | Vista |
|---|---|
| **Centro de expediciones**<br>Localiza señales en terreno alcanzable. Envía una unidad desde F10 y analiza las muestras encontradas. | ![Centro de expediciones](modelos/ModuleTypeExpeditionCenter.png) |
| **Baliza de expedición**<br>Amplía a 300 m el radio de búsqueda de señales al cartografiar por primera vez. Necesita energía. | ![Baliza de expedición](modelos/ModuleTypeExpeditionBeacon.png) |
| **Analizador de muestras**<br>Un ingeniero transforma una muestra de campo en un informe científico en 90 s de trabajo base. | ![Analizador de muestras](modelos/SampleAnalyzer.png) |
| **Muestra de campo**<br>Material obtenido en una expedición; se analiza para producir informes. | ![Muestra de campo](modelos/FieldSample.png) |
| **Vestigio recuperado**<br>Hallazgo ficticio de exploración. Se puede exportar mediante contratos. | ![Vestigio recuperado](modelos/AncientRelic.png) |
| **Rover de expedición**<br>Se fabrica con piezas y lleva hallazgos desde los sitios hasta el centro. | ![Rover de expedición](modelos/ExpeditionRover.png) |
| **Sonda averiada**<br>Señal explorable que entrega muestras y piezas una sola vez. | ![Sonda averiada](modelos/Discovery0.png) |
| **Suministros perdidos**<br>Cargamento recuperable mediante una expedición física. | ![Suministros perdidos](modelos/Discovery1.png) |
| **Vestigios de expedición**<br>Restos ficticios recuperables; no representan contenido oficial de Planetbase 2. | ![Vestigios de expedición](modelos/Discovery2.png) |

## Rescate

| Modelo | Vista |
|---|---|
| **Hospital de emergencia**<br>Camas de trauma y medicinas para la atención nativa por médicos. Las ambulancias llevan aquí a heridos exteriores. | ![Hospital de emergencia](modelos/ModuleTypeEmergencyHospital.png) |
| **Hangar de rescate**<br>Habilita ambulancias autónomas a 180 m. Fabrícalas en el centro de distribución. | ![Hangar de rescate](modelos/ModuleTypeRescueHangar.png) |
| **Refugio de emergencia**<br>Sala presurizada de evacuación con bancos, agua y camas. La evacuación respeta corredores y esclusas. | ![Refugio de emergencia](modelos/ModuleTypeEmergencyShelter.png) |
| **Cama de trauma**<br>Cama hospitalaria que usa médicos, medicamentos y animaciones nativos. Las ambulancias entregan aquí al paciente. | ![Cama de trauma](modelos/TraumaBed.png) |
| **Ambulancia rover**<br>Recoge heridos exteriores y los lleva a una cama de trauma disponible. | ![Ambulancia rover](modelos/RescueRover.png) |

## Vida colonial

| Modelo | Vista |
|---|---|
| **Residencias privadas**<br>Camas individuales que recuperan descanso y moral mediante las reglas normales de sueño. | ![Residencias privadas](modelos/ModuleTypePrivateResidence.png) |
| **Academia colonial**<br>Forma al colono seleccionado para otra profesión: 2 informes, 20 créditos y 300 s de estudio. La falta de energía pausa el progreso. | ![Academia colonial](modelos/ModuleTypeAcademy.png) |
| **Cafetería comunitaria**<br>Asientos cómodos, comida y bebidas. Los consumos alimentarios conservan las reglas nativas. | ![Cafetería comunitaria](modelos/ModuleTypeCommunityCafe.png) |
| **Cama privada**<br>Recuperación base de sueño en 180 s y moral en 400 s. Conserva la animación y el espacio de uso nativos. | ![Cama privada](modelos/PrivateBunk.png) |
| **Puesto de formación**<br>Selecciona un colono y una profesión en F10. El estudio requiere esta consola libre, energía y oxígeno. Cancelar pierde el coste ya pagado. | ![Puesto de formación](modelos/TrainingConsole.png) |
| **Asiento de cafetería**<br>Un asiento cómodo para recuperar moral; las máquinas de comida y bebida se colocan por separado. | ![Asiento de cafetería](modelos/CafeSeat.png) |

## Comercio

| Modelo | Vista |
|---|---|
| **Terminal comercial**<br>Contratos e importaciones en F10. Los contratos consumen inventario libre de la misma red y pagan una sola vez tras la expedición. | ![Terminal comercial](modelos/ModuleTypeTradeTerminal.png) |
| **Plataforma logística**<br>Plataforma exclusiva para expediciones comerciales e importaciones del mod; no admite visitantes nativos. | ![Plataforma logística](modelos/ModuleTypeExportDock.png) |
| **Consola comercial**<br>Permite gestionar encargos e importaciones desde F10 con una terminal y plataforma operativas. | ![Consola comercial](modelos/TradeConsole.png) |
| **Paquete de suministros**<br>Carga fabricada para contratos de exportación. | ![Paquete de suministros](modelos/SupplyCrate.png) |
| **Lanzadera de servicio**<br>Representación visual de la expedición comercial; no transporta colonos entre mundos. | ![Lanzadera de servicio](modelos/TradeShuttle.png) |

