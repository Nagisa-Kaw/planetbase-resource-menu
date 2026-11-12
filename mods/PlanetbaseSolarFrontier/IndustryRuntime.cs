using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using HarmonyLib;
using Planetbase;
using UnityEngine;
using Module=Planetbase.Module;
namespace PlanetbaseSolarFrontier
{
 internal static class IndustryRuntime
 {
  private static bool FusionReady(Module m)=>m.isBuilt()&&m.isEnabled()&&!m.isExtremelyDamaged()&&m.hasWater()&&Research.Has(typeof(TechSolarFusion));
  internal static Resource Buffered<T>(Module owner)
  {
   if(owner.getGrid()==null)return null;
   Resource buffered=WorldObjects.Modules.Where(m=>m.getGrid()==owner.getGrid()&&m.isOperational()).SelectMany(m=>m.getComponents()).Where(c=>c.isOperational()&&c.getResourceContainer()!=null)
    .SelectMany(c=>c.getResourceContainer().getResources()).FirstOrDefault(r=>r.getResourceType() is T&&!r.anyTargeters()&&!r.isTraded()&&r.getState()!=Resource.State.Loaded);
   if(buffered!=null)return buffered;
   // Carriers may have moved the capsule to a normal warehouse. Consume it there
   // only when that warehouse belongs to the reactor's connected service grid.
   return WorldObjects.Resources.FirstOrDefault(r=>r.getResourceType() is T&&r.getState()==Resource.State.Stored&&!r.isEmbedded()&&!r.anyTargeters()&&!r.isTraded()&&
    WorldObjects.Modules.Any(m=>m.hasFlag(ModuleType.FlagStorage)&&m.isOperational()&&m.getGrid()==owner.getGrid()&&(m.getPosition()-r.getPosition()).sqrMagnitude<=m.getRadius()*m.getRadius()));
  }
  [HarmonyPatch(typeof(Module),nameof(Module.update))]
  private static class Machines
  {
   static void Prefix(Module __instance,float timeStep)
   {
    if(timeStep<=0||!(__instance.getModuleType() is SolarModule)||!__instance.isBuilt())return;
    MachineState s=MachineState.For(__instance);
    if(__instance.getModuleType() is ModuleTypeFusionPlant)
    {
     if(FusionReady(__instance))
     {
      if(s.Fuel<=0){Resource r=Buffered<FusionCapsule>(__instance);if(r!=null){r.destroy();s.Fuel=1200;}}
      if(__instance.getPowerGeneration()>0)s.Fuel=Mathf.Max(0,s.Fuel-timeStep);
     }
     __instance.updatePowerGeneration();
    }
    if(!__instance.isOperational())return;
    if(__instance.getModuleType() is ModuleTypeRegolithDrill)
    {
     s.Cycle+=timeStep;float period=120/(.5f+Survey.Richness(__instance.getPosition()));
     if(s.Cycle>=period)
     {
      s.Cycle=Mathf.Min(s.Cycle-period,period);
      if(WorldObjects.Resources.Count(r=>r.getResourceType() is Regolith&&!r.isEmbedded()&&(r.getPosition()-__instance.getPosition()).sqrMagnitude<100)<12)
       Resource.create(TypeList<ResourceType,ResourceTypeList>.find<Regolith>(),__instance.getPosition()+Vector3.right*5,Location.Exterior).drop(Resource.State.Idle);
     }
    }
    if(__instance.getModuleType() is ModuleTypeSolarGreenhouse)
    {
     s.Nutrients=Mathf.Max(0,s.Nutrients-timeStep);
     if(s.Nutrients<=0)
     {
      Resource fertilizer=__instance.getComponents().Where(c=>c.getComponentType() is NutrientHopper&&c.isOperational()).SelectMany(c=>c.getResourceContainer().getResources()).FirstOrDefault(r=>r.getResourceType() is Fertilizer&&!r.anyTargeters());
      if(fertilizer!=null){fertilizer.destroy();s.Nutrients=600;}
     }
    }
   }
  }
  [HarmonyPatch(typeof(Module),"calculateCurrentPowerGeneration")]
  private static class EnergyInterlock
  {
   static bool Prefix(Module __instance,ref int __result)
   {
    if(__instance.getModuleType() is ModuleTypeFusionPlant&&(!FusionReady(__instance)||MachineState.For(__instance).Fuel<=0)){__result=0;return false;}
    if(__instance.getModuleType() is ModuleTypeGeothermalPlant&&(!Research.Has(typeof(TechSolarGeothermal))||!Survey.IsScanned(__instance.getPosition())||Survey.Heat(__instance.getPosition())<.55f)){__result=0;return false;}
    return true;
   }
  }
  [HarmonyPatch(typeof(Module),nameof(Module.getMaxPowerGeneration))]
  private static class GeothermalOutput
  {
   static void Postfix(Module __instance,ref int __result)
   {
    if(__instance.getModuleType() is ModuleTypeGeothermalPlant)__result=Mathf.RoundToInt(20000*Survey.Heat(__instance.getPosition())/1000)*1000;
    if(__instance.getModuleType() is ModuleTypeSolarPanel&&PlanetManager.getCurrentPlanet() is SolarWorld world)__result=Mathf.RoundToInt(__result*world.SolarFactor);
   }
  }
  [HarmonyPatch(typeof(ConstructionComponent),"updateProduction")]
  private static class Nutrients
  {
   static void Prefix(ConstructionComponent __instance,ref float timeStep)
   {
    Module m=__instance.getParentConstruction() as Module;
    if(m!=null&&m.getModuleType() is ModuleTypeSolarGreenhouse&&MachineState.For(m).Nutrients>0&&(__instance.getComponentType() is VegetablePad||__instance.getComponentType() is TissueSynthesizer))timeStep*=1.25f;
   }
  }
  private sealed class Hygiene {internal float Time;internal bool Produced;}
  private static readonly ConditionalWeakTable<Interaction,Hygiene> Visits=new ConditionalWeakTable<Interaction,Hygiene>();
  [HarmonyPatch(typeof(InteractionComponentRestore),nameof(InteractionComponentRestore.update))]
  private static class Waste
  {
   static void Postfix(InteractionComponentRestore __instance,float timeStep,bool __result)
   {
    if(__result||!(__instance.getSelectable() is ConstructionComponent c)||!(c.getComponentType() is SanitaryUnit)||!c.isOperational())return;
    Hygiene s=Visits.GetOrCreateValue(__instance);s.Time+=timeStep;
    if(s.Time>=10&&!s.Produced){s.Produced=true;Resource.create(TypeList<ResourceType,ResourceTypeList>.find<OrganicWaste>(),c.getPosition()+c.getTransform().forward,Location.Interior).drop(Resource.State.Idle);}
   }
  }
  [HarmonyPatch(typeof(InteractionComponent),"serialize")]
  private static class SaveHygiene
  {
   static void Postfix(InteractionComponent __instance,XmlNode parent)
   {
    if(!(__instance.getSelectable() is ConstructionComponent c)||!(c.getComponentType() is SanitaryUnit))return;
    Hygiene s=Visits.GetOrCreateValue(__instance);XmlElement e=parent.OwnerDocument.CreateElement("frontier-hygiene");SolarState.Float(e,"elapsed",s.Time);e.SetAttribute("produced",s.Produced?"true":"false");parent.LastChild.AppendChild(e);
   }
  }
  [HarmonyPatch(typeof(InteractionComponent),"deserialize")]
  private static class LoadHygiene
  {
   static void Postfix(InteractionComponent __instance,XmlNode node)
   {
    if(!(node["frontier-hygiene"] is XmlElement e))return;Hygiene s=Visits.GetOrCreateValue(__instance);s.Time=Mathf.Clamp(SolarState.Number(e,"elapsed"),0,600);s.Produced=e.GetAttribute("produced")=="true";
   }
  }
  private sealed class DeliverNutrients:AiRule
  {
   public override bool update(Character c)
   {
    if(!hasCarryPriority(c)||!canWork(c))return false;
    ResourceType t=TypeList<ResourceType,ResourceTypeList>.find<Fertilizer>();ConstructionComponent store=Module.findStorageComponent(c,t);if(store==null)return false;
    Resource r=Resource.findStorable(c,t,true);return r!=null&&goTarget(c,r,store);
   }
  }
  [HarmonyPatch(typeof(AiRuleGoLoadResourcesToStore),nameof(AiRuleGoLoadResourcesToStore.update))]
  private static class Deliver
  {
   static readonly DeliverNutrients Rule=new DeliverNutrients();
   static bool Prefix(Character character,ref bool __result){if(!Rule.update(character))return true;__result=true;return false;}
  }
  [HarmonyPatch(typeof(Human),nameof(Human.tick))]
  private static class Radiation
  {
   static void Postfix(Human __instance,float timeStep)
   {
    if(!(__instance is Colonist)||__instance.isDead()||timeStep<=0)return;
    UnitState s=UnitState.For(__instance);float risk=(PlanetManager.getCurrentPlanet() as SolarWorld)?.Radiation??0;
    if(__instance.getLocation()!=Location.Exterior||Survey.Protected(__instance.getPosition()))s.Dose=Mathf.Max(0,s.Dose-timeStep/180);
    else s.Dose=Mathf.Clamp01(s.Dose+timeStep*risk/180);
    if(s.Dose>=1&&__instance.getCondition()==null)__instance.setCondition(TypeList<ConditionType,ConditionTypeList>.find<ConditionRadiation>());
   }
  }
  [HarmonyPatch(typeof(Character),nameof(Character.isProtected))]
  private static class FlareShield
  {
   static void Postfix(Character __instance,ref bool __result)
   {
    if(!__result&&__instance.getLocation()==Location.Exterior&&Singleton<DisasterManager>.getInstance().getSolarFlare().isInProgress()&&Survey.Protected(__instance.getPosition()))__result=true;
   }
  }
  [HarmonyPatch(typeof(Construction),nameof(Construction.getDescription))]
  private static class Status
  {
   static void Postfix(Construction __instance,ref string __result)
   {
    if(!(__instance is Module m))return;
    if(m.getModuleType() is ModuleTypeGeothermalPlant)__result+="\nCalor del terreno: "+Survey.Heat(m.getPosition()).ToString("P0")+"\nExplorado: "+Survey.IsScanned(m.getPosition());
    if(m.getModuleType() is ModuleTypeFusionPlant)__result+="\nCápsula de fusión: "+MachineState.For(m).Fuel.ToString("0")+" s";
    if(m.getModuleType() is ModuleTypeSolarGreenhouse)__result+="\nFertilización +25%: "+MachineState.For(m).Nutrients.ToString("0")+" s";
    if(m.getModuleType() is ModuleTypeRadiationPylon)__result+="\nProtección radiológica exterior: 35 m. Requiere energía.";
   }
  }
 }
}
