using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Planetbase;
using UnityEngine;
using Module=Planetbase.Module;
[assembly:InternalsVisibleTo("CommunityProbe")]
namespace PlanetbaseCommunityExpansion
{
 [BepInPlugin(Id,"Colonias en expansión · alfa","0.1.0")]
 [BepInDependency("local.planetbase.solar-frontier","0.3.0")]
 [BepInDependency("local.planetbase.nuclear-transport","1.2.0")]
 [BepInProcess("Planetbase.exe")]
 public sealed class CommunityPlugin:BaseUnityPlugin
 {
  internal const string Id="local.planetbase.community-expansion";
  internal static ManualLogSource Log;
  void Awake(){Log=Logger;if(Definitions.VersionNumber.ToString()!="1.4.2"){Logger.LogWarning("Requiere Planetbase 1.4.2; complemento desactivado.");return;}new Harmony(Id).PatchAll(typeof(CommunityPlugin).Assembly);Logger.LogInfo("Colonias en expansión 0.1.0 alfa lista. F10: seis módulos de gestión.");}
  static void Add<T,L>(object list,T value) where L:TypeList<T,L>,new()=>AccessTools.Method(typeof(TypeList<T,L>),"add").Invoke(list,new object[]{value});
  [HarmonyPatch(typeof(ResourceTypeList),MethodType.Constructor)]
  [HarmonyAfter("local.planetbase.solar-frontier")]
  static class Resources{static void Postfix(ResourceTypeList __instance){foreach(ResourceType t in new ResourceType[]{new FieldSample(),new AncientRelic(),new SupplyCrate()})AccessTools.Method(typeof(ResourceTypeList),"addResource").Invoke(__instance,new object[]{t});}}
  [HarmonyPatch(typeof(SpecializationList),MethodType.Constructor)]
  [HarmonyAfter("local.planetbase.solar-frontier")]
  static class Units{static void Postfix(SpecializationList __instance){var list=(List<Specialization>)AccessTools.Field(typeof(TypeList<Specialization,SpecializationList>),"mTypeList").GetValue(__instance);var template=list.Find(t=>t is Carrier);foreach(Specialization t in new Specialization[]{new FreightHauler(template),new RescueRover(template),new ExpeditionRover(template)})AccessTools.Method(typeof(SpecializationList),"addBotSpecialization").Invoke(__instance,new object[]{t});}}
  [HarmonyPatch(typeof(ComponentTypeList),MethodType.Constructor)]
  [HarmonyAfter("local.planetbase.solar-frontier")]
  static class Equipment{static void Postfix(ComponentTypeList __instance){foreach(ComponentType t in new ComponentType[]{new SecurityDispatch(),new LogisticsConsole(),new VehicleFabricator(),new SampleAnalyzer(),new TraumaBed(),new PrivateBunk(),new TrainingConsole(),new CafeSeat(),new TradeConsole(),new SupplyPacker()})Add<ComponentType,ComponentTypeList>(__instance,t);}}
  [HarmonyPatch(typeof(ModuleTypeList),MethodType.Constructor)]
  static class Buildings{static void Postfix(ModuleTypeList __instance){foreach(ModuleType t in new ModuleType[]{new ModuleTypeSecurityCommand(),new ModuleTypeSurveillanceRadar(),new ModuleTypeDefenseTurret(),new ModuleTypePatrolDepot(),new ModuleTypeLogisticsHub(),new ModuleTypeFreightWarehouse(),new ModuleTypeExpeditionCenter(),new ModuleTypeExpeditionBeacon(),new ModuleTypeEmergencyHospital(),new ModuleTypeRescueHangar(),new ModuleTypeEmergencyShelter(),new ModuleTypePrivateResidence(),new ModuleTypeAcademy(),new ModuleTypeCommunityCafe(),new ModuleTypeTradeTerminal(),new ModuleTypeExportDock()})Add<ModuleType,ModuleTypeList>(__instance,t);}}
  [HarmonyPatch(typeof(ModuleType),"loadPrefab")]
  static class Models{static bool Prefix(ModuleType __instance,int sizeIndex,ref GameObject __result){if(!(__instance is ICommunityModule t))return true;__result=(GameObject)AccessTools.Method(typeof(ModuleType),"loadPrefab").Invoke(TypeList<ModuleType,ModuleTypeList>.find(t.Template),new object[]{sizeIndex});CommunityModels.Building(__result,__instance,sizeIndex);return false;}}
  [HarmonyPatch(typeof(ComponentType),nameof(ComponentType.createModel))]
  static class Furnishings{static void Postfix(ComponentType __instance,GameObject __result){if(Catalog.IsEquipment(__instance.GetType().Name))CommunityModels.Equipment(__result,__instance.GetType().Name);}}
  [HarmonyPatch]
  static class Vehicles
  {
   static IEnumerable<MethodBase> TargetMethods(){yield return AccessTools.Method(typeof(Specialization),nameof(Specialization.createModel),new[]{typeof(ObjectList<Material>),typeof(Human.Gender),typeof(int),typeof(int),typeof(int)});yield return AccessTools.Method(typeof(Specialization),nameof(Specialization.createModelExterior));}
   static void Postfix(Specialization __instance,GameObject __result){if(__instance is CommunityBot){PlanetbaseVisuals.Architecture.HideOriginalMeshes(__result);CommunityModels.Vehicle(__result.transform,__instance.GetType().Name);}}
  }
  [HarmonyPatch(typeof(ResourceType),nameof(ResourceType.createModel))]
  static class CargoModels{static void Postfix(ResourceType __instance,GameObject __result){if(__instance is CommunityResource)CommunityModels.Resource(__result,__instance.GetType().Name);}}
 }
 internal static class World
 {
  internal static List<Module> Modules=>(List<Module>)AccessTools.Field(typeof(Module),"mModules").GetValue(null);
  internal static List<Character> Characters=>(List<Character>)AccessTools.Field(typeof(Character),"mCharacters").GetValue(null);
  internal static List<Resource> Resources=>(List<Resource>)AccessTools.Field(typeof(Resource),"mResources").GetValue(null);
  internal static GameStateGame Game=>GameManager.getInstance()?.getGameState() as GameStateGame;
  internal static Module Module(int id)=>Modules.Find(m=>m.getId()==id&&!m.isDestroyed());
  internal static Character Unit(int id)=>Characters.Find(c=>c.getId()==id&&!c.isDestroyed());
  internal static Resource Cargo(int id)=>Resources.Find(r=>r.getId()==id&&!r.isDestroyed());
  internal static Module Working<T>()=>Modules.Find(m=>m.getModuleType() is T&&m.isOperational());
  internal static ConstructionComponent Equipment<T>(){foreach(var m in Modules)if(m.isOperational())foreach(var c in m.getComponents())if(c.getComponentType() is T&&c.isOperational())return c;return null;}
  internal static void Toast(string message){if(Game!=null)AccessTools.Method(typeof(GameStateGame),"addToast").Invoke(Game,new object[]{message,5f});}
  internal static bool Healthy(Character c)=>c!=null&&!c.isDestroyed()&&!c.isDead()&&!c.isKo()&&!c.isFighting()&&!c.isFleeing()&&c.isFitForCriticalWork();
  internal static void Location(Character c,Location location)=>AccessTools.Method(typeof(Character),"setLocation").Invoke(c,new object[]{location});
  internal static bool Available(Resource r)=>!r.isDestroyed()&&!r.isTraded()&&!r.anyTargeters()&&r.getState()!=Resource.State.Loaded;
 }
}
