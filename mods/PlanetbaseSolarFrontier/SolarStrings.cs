using System.Collections.Generic;
using HarmonyLib;
using Planetbase;
namespace PlanetbaseSolarFrontier
{
 internal static class SolarStrings
 {
  private static readonly Dictionary<string,string> Text=new Dictionary<string,string>();
  private static readonly Dictionary<string,string[]> Help=new Dictionary<string,string[]>();
  static SolarStrings()
  {
   Building("SurveyLab","Laboratorio de investigación","Ingenieros producen informes físicos en las mesas. F9 permite financiar proyectos locales y planos terrestres.","lab");
   Building("AssemblyWorks","Planta de ensamblaje","Fabrica piezas, chasis y cápsulas de fusión. Ensambla robots y vehículos a partir de tres ingredientes reales.","factory");
   Building("TreatmentPlant","Planta de tratamiento","Instala unidades sanitarias y recicladoras. Convierte residuos orgánicos en fertilizante.","processing_plant");
   Building("SolarGreenhouse","Invernadero avanzado","Cultivos y carne de laboratorio. Una unidad de fertilizante en el depósito mejora un 25% la producción durante 600 s.","bio_dome");
   Building("TunnelGallery","Galería blindada","Refugio presurizado en superficie, ampliable mediante corredores normales. No excava montañas ni crea un nivel subterráneo.","multi_dome");
   Building("GeothermalPlant","Planta geotérmica","Coloca sobre terreno explorado con calor mínimo de 55%. Genera entre 11 y 20 kW según el calor. Necesita tecnología geotérmica y conexión a la red.","water_extractor");
   Building("FusionPlant","Central de fusión","Genera 120 kW brutos con agua y cápsulas. Cada cápsula dura 1200 s en funcionamiento; se toma de las refinerías de la misma red.","power_collector");
   Building("RegolithDrill","Extractor de regolito","Extrae regolito superficial consumiendo 3 kW. La concentración local modifica su producción. Se procesa en la planta de ensamblaje para obtener mineral.","mine");
   Building("RadiationPylon","Pilón antirradiación","Protege a colonos en exteriores a 35 m del centro. Consume 6 kW. Sin energía, deja de proteger.","lightning_rod");
   Equipment("ResearchDesk","Mesa de investigación","Un ingeniero produce un informe cada 90 s de trabajo base. Conserva hasta cuatro en su inventario.","semiconductor_foundry");
   Equipment("PartsWorkshop","Taller de piezas","1 metal + 1 semiconductor producen 1 juego de piezas en 120 s de trabajo base.","spares_workshop");
   Equipment("ChassisBench","Banco de chasis","1 metal + 1 bioplástico producen 1 chasis en 120 s de trabajo base.","spares_workshop");
   Equipment("RegolithProcessor","Procesador de regolito","1 regolito produce 1 mineral en 60 s de trabajo base de un obrero.","metal_processor");
   Equipment("FusionRefinery","Refinería de cápsulas","1 juego de piezas + 1 semiconductor producen 1 cápsula de fusión en 240 s de trabajo base. Demanda agua y 6 kW.","semiconductor_foundry");
   Equipment("AdvancedAssembler","Ensamblador avanzado","1 juego de piezas + 1 chasis + 1 semiconductor ensamblan una unidad en 240 s de trabajo base. Cambia el producto con los controles normales de fabricación. Constructor y minero requieren sus planos.","bot_workshop");
   Equipment("SanitaryUnit","Unidad sanitaria","Servicio de hidratación e higiene. Una visita activa de al menos 10 s genera una unidad de residuos orgánicos. Demanda agua.","drinking_fountain");
   Equipment("WasteReprocessor","Recicladora orgánica","Un obrero transforma 1 residuo en 1 fertilizante en 60 s de trabajo base.","bioplastic_processor");
   Equipment("NutrientHopper","Depósito de fertilizante","Almacena 6 fertilizantes para mejorar cultivos y sintetizadores del invernadero. Los transportistas lo abastecen automáticamente.","medical_cabinet");
   Resource("Regolith","Regolito");Resource("MachineParts","Piezas mecánicas");Resource("VehicleChassis","Chasis de vehículo");Resource("OrganicWaste","Residuos orgánicos");Resource("Fertilizer","Fertilizante");Resource("FusionCapsule","Cápsula de fusión");Resource("ResearchPacket","Informe científico");
   foreach(var p in new[]{new[]{"scoutdrone","Scout Drone"},new[]{"scannerbot","Bot escáner"},new[]{"cargotruck","Camión de carga"}})
   {Text["specialization_"+p[0]]=p[1];Text["specialization_"+p[0]+"_plural"]=p[1];Text["specialization_"+p[0]+"_female"]=p[1];}
   Tech("SolarSurvey","Escaneo de recursos","Habilita el escaneo mediante colonos, bots y sondas.");
   Tech("SolarGeothermal","Energía geotérmica","Desbloquea centrales sobre puntos calientes explorados.");
   Tech("SolarRecycling","Reciclaje y cultivos","Desbloquea tratamiento de residuos e invernaderos avanzados.");
   Tech("SolarRobotics","Ensamblaje avanzado","Permite fabricar Scout Drone, bot escáner y camión de carga.");
   Tech("SolarShield","Blindaje de radiación","Planos terrestres para pilones de protección exterior.");
   Tech("SolarFusion","Fusión controlada","Integra geotermia y robótica avanzada para fabricar cápsulas y centrales de fusión.");
  }
  internal static void World(char c,string title,string desc){string k="planet_"+char.ToLowerInvariant(c);Text[k+"_name"]=title;Text[k+"_description"]=desc;Text[k+"_difficulty"]="Frontera Solar";}
  private static void Resource(string id,string name){string k=Util.camelCaseToLowercase(id);Text[k]=name;Text[k+"_plural"]=name;}
  private static void Tech(string id,string name,string tip){string k="tech_"+Util.camelCaseToLowercase(id);Text[k]=name;Text[k+"_description"]=tip;}
  private static void Building(string id,string name,string tip,string icon){string k=Util.camelCaseToLowercase(id);Text[k]=name;Text["tooltip_"+k]=tip;Help[id]=new[]{name,tip,icon};}
  private static void Equipment(string id,string name,string tip,string icon){Text["component_"+Util.camelCaseToLowercase(id)]=name;Text["tooltip_"+Util.camelCaseToLowercase(id)]=tip;Help[id]=new[]{name,tip,icon};}
  [HarmonyPatch(typeof(StringList),nameof(StringList.get),new[]{typeof(string),typeof(string),typeof(string),typeof(string)})]
  private static class Translate{static bool Prefix(string key,ref string __result){if(!Text.TryGetValue(key,out string s))return true;__result=s;return false;}}
  [HarmonyPatch(typeof(StringList),nameof(StringList.exists))]
  private static class Exists{static bool Prefix(string key,ref bool __result){if(!Text.ContainsKey(key))return true;__result=true;return false;}}
  [HarmonyPatch(typeof(HelpManager),MethodType.Constructor)]
  private static class HelpItems{static void Postfix(Dictionary<string,HelpItem> ___mItems){foreach(var p in Help)___mItems[p.Key]=new HelpItem(p.Key,p.Value[0],p.Value[1],"icon_"+p.Value[2]);}}
  [HarmonyPatch(typeof(ConstructionComponent),nameof(ConstructionComponent.getHelpId))]
  private static class HelpId{static void Postfix(ConstructionComponent __instance,ref string __result){string id=__instance.getComponentType().GetType().Name;if(Help.ContainsKey(id))__result=id;}}
 }
}
