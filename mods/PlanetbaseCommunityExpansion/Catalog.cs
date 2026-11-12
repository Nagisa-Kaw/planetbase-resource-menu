using System.Collections.Generic;
using HarmonyLib;
using Planetbase;
using UnityEngine;
namespace PlanetbaseCommunityExpansion
{
 internal sealed class Entry
 {
  internal string Name,Tip,Icon,Group;
  internal Entry(string name,string tip,string icon,string group){Name=name;Tip=tip;Icon=icon;Group=group;}
 }
 internal static class Catalog
 {
  internal static readonly Dictionary<string,Entry> All=new Dictionary<string,Entry>{
   ["SecurityCommand"]=new Entry("Centro de seguridad","Instala una consola de seguridad. F10 organiza patrullas y alertas; el guardia de consola conserva su trabajo nativo.","control_center","Defensa"),
   ["SurveillanceRadar"]=new Entry("Radar de vigilancia","Detecta intrusos exteriores a 140 m y avisa una vez por intruso. Necesita energía.","water_extractor","Defensa"),
   ["DefenseTurret"]=new Entry("Torre defensiva","Dispara a intrusos exteriores a 50 m sin obstáculos del terreno. Consume energía y un repuesto por 20 disparos. Requiere centro de seguridad operativo.","water_extractor","Defensa"),
   ["PatrolDepot"]=new Entry("Puesto de patrulla","Despliega un vehículo de patrulla para un guardia asignado desde F10. El guardia usa rutas alcanzables y deja la patrulla ante necesidades críticas.","water_extractor","Defensa"),
   ["LogisticsHub"]=new Entry("Centro de distribución","Consolas, empaquetadoras y ensambladores. F10 configura rutas entre almacenes para transportadores de hasta tres cajas.","factory","Logística"),
   ["FreightWarehouse"]=new Entry("Almacén logístico","Almacén nativo de dos tamaños. Puede ser origen o destino de rutas; admite los mismos recursos físicos que un almacén normal.","storage","Logística"),
   ["ExpeditionCenter"]=new Entry("Centro de expediciones","Localiza señales en terreno alcanzable. Envía una unidad desde F10 y analiza las muestras encontradas.","lab","Exploración"),
   ["ExpeditionBeacon"]=new Entry("Baliza de expedición","Amplía a 300 m el radio de búsqueda de señales al cartografiar por primera vez. Necesita energía.","water_extractor","Exploración"),
   ["EmergencyHospital"]=new Entry("Hospital de emergencia","Camas de trauma y medicinas para la atención nativa por médicos. Las ambulancias llevan aquí a heridos exteriores.","sick_bay","Rescate"),
   ["RescueHangar"]=new Entry("Hangar de rescate","Habilita ambulancias autónomas a 180 m. Fabrícalas en el centro de distribución.","water_extractor","Rescate"),
   ["EmergencyShelter"]=new Entry("Refugio de emergencia","Sala presurizada de evacuación con bancos, agua y camas. La evacuación respeta corredores y esclusas.","multi_dome","Rescate"),
   ["PrivateResidence"]=new Entry("Residencias privadas","Camas individuales que recuperan descanso y moral mediante las reglas normales de sueño.","dorm","Vida colonial"),
   ["Academy"]=new Entry("Academia colonial","Forma al colono seleccionado para otra profesión: 2 informes, 20 créditos y 300 s de estudio. La falta de energía pausa el progreso.","multi_dome","Vida colonial"),
   ["CommunityCafe"]=new Entry("Cafetería comunitaria","Asientos cómodos, comida y bebidas. Los consumos alimentarios conservan las reglas nativas.","bar","Vida colonial"),
   ["TradeTerminal"]=new Entry("Terminal comercial","Contratos e importaciones en F10. Los contratos consumen inventario libre de la misma red y pagan una sola vez tras la expedición.","control_center","Comercio"),
   ["ExportDock"]=new Entry("Plataforma logística","Plataforma exclusiva para expediciones comerciales e importaciones del mod; no admite visitantes nativos.","landing_pad","Comercio"),
   ["SecurityDispatch"]=new Entry("Consola de seguridad avanzada","Puesto de trabajo nativo para un guardia. El centro de seguridad habilita las torres y patrullas.","security_console","equipo"),
   ["LogisticsConsole"]=new Entry("Consola de distribución","Abre F10 para definir origen, destino, material y transportador.","security_console","equipo"),
   ["VehicleFabricator"]=new Entry("Ensamblador de flota","Un ingeniero utiliza 1 juego de piezas, 1 chasis y 1 semiconductor para fabricar un transportador, ambulancia o rover de expedición. Requiere robótica.","bot_workshop","equipo"),
   ["SampleAnalyzer"]=new Entry("Analizador de muestras","Un ingeniero transforma una muestra de campo en un informe científico en 90 s de trabajo base.","semiconductor_foundry","equipo"),
   ["TraumaBed"]=new Entry("Cama de trauma","Cama hospitalaria que usa médicos, medicamentos y animaciones nativos. Las ambulancias entregan aquí al paciente.","sick_bay_bed","equipo"),
   ["PrivateBunk"]=new Entry("Cama privada","Recuperación base de sueño en 180 s y moral en 400 s. Conserva la animación y el espacio de uso nativos.","bed","equipo"),
   ["TrainingConsole"]=new Entry("Puesto de formación","Selecciona un colono y una profesión en F10. El estudio requiere esta consola libre, energía y oxígeno. Cancelar pierde el coste ya pagado.","security_console","equipo"),
   ["CafeSeat"]=new Entry("Asiento de cafetería","Un asiento cómodo para recuperar moral; las máquinas de comida y bebida se colocan por separado.","bench","equipo"),
   ["TradeConsole"]=new Entry("Consola comercial","Permite gestionar encargos e importaciones desde F10 con una terminal y plataforma operativas.","security_console","equipo"),
   ["SupplyPacker"]=new Entry("Empaquetadora de suministros","Un ingeniero transforma 1 metal y 1 bioplástico en un paquete de suministros en 90 s de trabajo base.","spares_workshop","equipo"),
   ["FieldSample"]=new Entry("Muestra de campo","Material obtenido en una expedición; se analiza para producir informes.","spares","recurso"),
   ["AncientRelic"]=new Entry("Vestigio recuperado","Hallazgo ficticio de exploración. Se puede exportar mediante contratos.","spares","recurso"),
   ["SupplyCrate"]=new Entry("Paquete de suministros","Carga fabricada para contratos de exportación.","spares","recurso")};
  internal static bool IsEquipment(string id)=>All.TryGetValue(id,out var e)&&e.Group=="equipo";
  [HarmonyPatch(typeof(ComponentType),"loadIcon",new System.Type[]{})]
  static class ComponentIcon{static bool Prefix(ComponentType __instance,ref Texture2D __result){if(!IsEquipment(__instance.GetType().Name))return true;__result=ResourceUtil.loadIconColor("Components/icon_"+All[__instance.GetType().Name].Icon);return false;}}
  [HarmonyPatch(typeof(ModuleType),"loadIcon")]
  static class ModuleIcon{static bool Prefix(ModuleType __instance,ref Texture2D __result){if(!(__instance is ICommunityModule))return true;__result=ResourceUtil.loadIconColor("Modules/icon_"+All[__instance.GetType().Name.Replace("ModuleType","")].Icon);return false;}}
  static readonly Dictionary<string,string> Text=MakeText();
  static Dictionary<string,string> MakeText()
  {
   var text=new Dictionary<string,string>();
   foreach(var p in All){string key=Util.camelCaseToLowercase(p.Key);text[(p.Value.Group=="equipo"?"component_":"")+key]=p.Value.Name;text["tooltip_"+key]=p.Value.Tip;text[key+"_plural"]=p.Value.Name;}
   foreach(var p in new[]{new[]{"freighthauler","Transportador pesado"},new[]{"rescuerover","Ambulancia rover"},new[]{"expeditionrover","Rover de expedición"}})
    foreach(string suffix in new[]{"","_plural","_female"})text["specialization_"+p[0]+suffix]=p[1];
   return text;
  }
  [HarmonyPatch(typeof(StringList),nameof(StringList.get),new[]{typeof(string),typeof(string),typeof(string),typeof(string)})]
  static class Translate{static bool Prefix(string key,ref string __result){if(!Text.TryGetValue(key,out string value))return true;__result=value;return false;}}
  [HarmonyPatch(typeof(StringList),nameof(StringList.exists))]
  static class Exists{static bool Prefix(string key,ref bool __result){if(!Text.ContainsKey(key))return true;__result=true;return false;}}
  [HarmonyPatch(typeof(HelpManager),MethodType.Constructor)]
  static class Help{static void Postfix(Dictionary<string,HelpItem> ___mItems){foreach(var p in All)___mItems[p.Key]=new HelpItem(p.Key,p.Value.Name,p.Value.Tip,"icon_"+p.Value.Icon);}}
  [HarmonyPatch(typeof(ConstructionComponent),nameof(ConstructionComponent.getHelpId))]
  static class ComponentHelp{static void Postfix(ConstructionComponent __instance,ref string __result){string id=__instance.getComponentType().GetType().Name;if(IsEquipment(id))__result=id;}}
 }
}
