using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using HarmonyLib;
using Planetbase;
using UnityEngine;
using Module = Planetbase.Module;
[assembly: InternalsVisibleTo("SolarFrontierProbe")]
namespace PlanetbaseSolarFrontier
{
 [BepInPlugin("local.planetbase.solar-frontier", "Frontera Solar", "0.3.0")]
 [BepInDependency("local.planetbase.nuclear-transport", "1.0.0")]
 [BepInProcess("Planetbase.exe")]
 public sealed class SolarPlugin : BaseUnityPlugin
 {
  private void Awake()
  {
   if(Definitions.VersionNumber.ToString()!="1.4.2") {Logger.LogWarning("Frontera Solar necesita Planetbase 1.4.2.");return;}
   new Harmony(Info.Metadata.GUID).PatchAll(typeof(SolarPlugin).Assembly);
   Logger.LogInfo("Frontera Solar 0.3.0 lista. F9: gestión; F6: capas de escaneo.");
  }
  internal static void Add<T,L>(L list, T item) where L:TypeList<T,L>,new() => AccessTools.Method(typeof(TypeList<T,L>),"add").Invoke(list,new object[]{item});
  [HarmonyPatch(typeof(PlanetList),MethodType.Constructor)]
  private static class Worlds {static void Postfix(PlanetList __instance){foreach(Planet p in new Planet[]{new SolarMars(),new SolarMoon(),new SolarEuropa(),new SolarTitan()})Add<Planet,PlanetList>(__instance,p);}}
  [HarmonyPatch(typeof(ResourceTypeList),MethodType.Constructor),HarmonyAfter("local.planetbase.nuclear-transport")]
  private static class Resources
  {
   static void Postfix(ResourceTypeList __instance)
   {
    foreach(ResourceType t in new ResourceType[]{new Regolith(),new MachineParts(),new VehicleChassis(),new OrganicWaste(),new Fertilizer(),new FusionCapsule(),new ResearchPacket()})
     AccessTools.Method(typeof(ResourceTypeList),"addResource").Invoke(__instance,new object[]{t});
   }
  }
  [HarmonyPatch(typeof(TechList),MethodType.Constructor)]
  private static class Techs {static void Postfix(TechList __instance){foreach(Tech t in new Tech[]{new TechSolarSurvey(),new TechSolarGeothermal(),new TechSolarRecycling(),new TechSolarRobotics(),new TechSolarShield(),new TechSolarFusion()})Add<Tech,TechList>(__instance,t);}}
  [HarmonyPatch(typeof(SpecializationList),MethodType.Constructor)]
  private static class Bots
  {
   static void Postfix(SpecializationList __instance)
   {
    var list=(List<Specialization>)AccessTools.Field(typeof(TypeList<Specialization,SpecializationList>),"mTypeList").GetValue(__instance);
    var template=list.First(t=>t is Carrier);
    foreach(Specialization s in new Specialization[]{new ScoutDrone(template),new ScannerBot(template),new CargoTruck(template)})
     AccessTools.Method(typeof(SpecializationList),"addBotSpecialization").Invoke(__instance,new object[]{s});
   }
  }
  [HarmonyPatch(typeof(ComponentTypeList),MethodType.Constructor)]
  private static class Equipment
  {
   static void Postfix(ComponentTypeList __instance)
   {
    foreach(ComponentType c in new ComponentType[]{new ResearchDesk(),new PartsWorkshop(),new ChassisBench(),new RegolithProcessor(),new FusionRefinery(),new AdvancedAssembler(),new SanitaryUnit(),new WasteReprocessor(),new NutrientHopper()})Add<ComponentType,ComponentTypeList>(__instance,c);
   }
  }
  [HarmonyPatch(typeof(ModuleTypeList),MethodType.Constructor)]
  private static class Buildings
  {
   static void Postfix(ModuleTypeList __instance)
   {
    foreach(ModuleType m in new ModuleType[]{new ModuleTypeSurveyLab(),new ModuleTypeAssemblyWorks(),new ModuleTypeTreatmentPlant(),new ModuleTypeSolarGreenhouse(),new ModuleTypeTunnelGallery(),new ModuleTypeGeothermalPlant(),new ModuleTypeFusionPlant(),new ModuleTypeRegolithDrill(),new ModuleTypeRadiationPylon()})Add<ModuleType,ModuleTypeList>(__instance,m);
   }
  }
  [HarmonyPatch(typeof(ModuleType),"loadPrefab")]
  private static class Models
  {
   static bool Prefix(ModuleType __instance,int sizeIndex,ref GameObject __result)
   {
    if(!(__instance is SolarModule t))return true;
    __result=(GameObject)AccessTools.Method(typeof(ModuleType),"loadPrefab").Invoke(TypeList<ModuleType,ModuleTypeList>.find(t.Template),new object[]{sizeIndex});
    SolarModels.Build(__result,t);return false;
   }
  }
 }
 internal static class WorldObjects
 {
  internal static List<Module> Modules => (List<Module>)AccessTools.Field(typeof(Module),"mModules").GetValue(null);
  internal static List<Character> Characters => (List<Character>)AccessTools.Field(typeof(Character),"mCharacters").GetValue(null);
  internal static List<Resource> Resources => (List<Resource>)AccessTools.Field(typeof(Resource),"mResources").GetValue(null);
  internal static GameStateGame Game => GameManager.getInstance().getGameState() as GameStateGame;
  internal static void Toast(string text){if(Game!=null)AccessTools.Method(typeof(GameStateGame),"addToast").Invoke(Game,new object[]{text,5f});}
 }
}
