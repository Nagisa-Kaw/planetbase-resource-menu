using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Planetbase;
using UnityEngine;
namespace PlanetbaseSolarFrontier
{
 public abstract class SolarTech:Tech
 {
  protected SolarTech(){mIcon=ResourceUtil.loadIconColor("Techs/icon_farm_dome",Color.cyan);mMerchantCategory=MerchantCategory.Count;initStrings();}
 }
 public sealed class TechSolarSurvey:SolarTech{}
 public sealed class TechSolarGeothermal:SolarTech{}
 public sealed class TechSolarRecycling:SolarTech{}
 public sealed class TechSolarRobotics:SolarTech{}
 public sealed class TechSolarShield:SolarTech{}
 public sealed class TechSolarFusion:SolarTech{}
 public sealed class ResearchProject
 {
  public readonly Type Tech;
  public readonly string Name;
  public readonly int Data,Credits;
  public readonly float Seconds;
  public readonly Type[] Parents;
  internal ResearchProject(Type tech,string name,int data,int credits,float seconds,params Type[] parents){Tech=tech;Name=name;Data=data;Credits=credits;Seconds=seconds;Parents=parents;}
 }
 public static class Research
 {
  public static readonly ResearchProject[] Projects={
   new ResearchProject(typeof(TechSolarSurvey),"Escaneo de recursos",2,0,90),
   new ResearchProject(typeof(TechSolarGeothermal),"Energía geotérmica",3,0,180,typeof(TechSolarSurvey)),
   new ResearchProject(typeof(TechSolarRecycling),"Reciclaje y cultivos",2,0,120,typeof(TechSolarSurvey)),
   new ResearchProject(typeof(TechSolarRobotics),"Ensamblaje avanzado",3,0,240,typeof(TechSolarRecycling)),
   new ResearchProject(typeof(TechSolarShield),"Blindaje de radiación",0,90,300,typeof(TechSolarSurvey)),
   new ResearchProject(typeof(TechSolarFusion),"Fusión controlada",4,120,600,typeof(TechSolarGeothermal),typeof(TechSolarRobotics)),
   new ResearchProject(typeof(TechConstructorBot),"Planos terrestres: constructor",0,100,300,typeof(TechSolarRobotics)),
   new ResearchProject(typeof(TechDrillerBot),"Planos terrestres: minero",0,100,300,typeof(TechSolarRobotics))};
  public static bool Has(Type type)=>type==null||Singleton<TechManager>.getInstance().isAcquired(TypeList<Tech,TechList>.find(type.Name));
  internal static List<Resource> DataStock()=>WorldObjects.Resources.Where(r=>r.getResourceType() is ResearchPacket&&!r.isDestroyed()&&!r.isTraded()&&!r.anyTargeters()&&(r.getState()==Resource.State.Idle||r.getState()==Resource.State.Stored)).ToList();
  public static string Start(int index)
  {
   if(index<0||index>=Projects.Length)return "Proyecto inválido.";
   SolarState state=SolarState.Current;ResearchProject p=Projects[index];
   if(state.Project>=0)return "Ya hay un proyecto en curso.";
   if(Has(p.Tech))return "Tecnología ya disponible.";
   if(p.Parents.Any(t=>!Has(t)))return "Primero completa las tecnologías anteriores.";
   var stock=DataStock();if(stock.Count<p.Data)return "Faltan informes disponibles en la colonia: "+stock.Count+"/"+p.Data+".";
   if(Resource.getCountOfType(ResourceTypeList.CoinsInstance)<p.Credits)return "Faltan créditos para los planos terrestres.";
   if(p.Credits>0&&!Resource.removeInmaterialResource(new ResourceAmount(ResourceTypeList.CoinsInstance,p.Credits)))return "No se pudo financiar el proyecto.";
   for(int i=0;i<p.Data;i++)stock[i].destroy();
   state.Project=index;state.Progress=0;
   return p.Name+": iniciado. Costes pagados una sola vez.";
  }
  internal static void Tick(float dt)
  {
   SolarState s=SolarState.Current;if(s.Project<0||s.Project>=Projects.Length||dt<=0)return;
   ResearchProject p=Projects[s.Project];
   // Local projects need an operational research laboratory throughout development.
   if(p.Data>0&&!WorldObjects.Modules.Any(m=>m.getModuleType() is ModuleTypeSurveyLab&&m.isOperational()))return;
   s.Progress+=dt;float duration=p.Seconds*(p.Credits>0?(PlanetManager.getCurrentPlanet() is SolarWorld world?world.EarthDelay:1f):1f);
   if(s.Progress<duration)return;
   Singleton<TechManager>.getInstance().acquire(TypeList<Tech,TechList>.find(p.Tech.Name));
   s.Project=-1;s.Progress=0;WorldObjects.Toast("Investigación terminada: "+p.Name);SolarApi.Researched(p.Tech.Name);
  }
  [HarmonyPatch(typeof(GuiMenuItem),"areRequirementsMet")]
  private static class MenuGate
  {
   static void Postfix(GuiMenuItem __instance,ref bool __result){if(__instance.getModuleType() is SolarModule m&&!Has(m.Research))__result=false;}
  }
  [HarmonyPatch(typeof(GameStateGame),nameof(GameStateGame.onButtonBuildModule))]
  private static class BuildGate
  {
   static bool Prefix(object parameter){if(!(parameter is SolarModule m)||Has(m.Research))return true;WorldObjects.Toast("F9: investiga "+TypeList<Tech,TechList>.find(m.Research.Name).getName());return false;}
  }
  [HarmonyPatch(typeof(Planetbase.Module),nameof(Planetbase.Module.canPlaceModule))]
  private static class PlacementGate
  {
   static void Postfix(Planetbase.Module __instance,ref bool __result)
   {
    if(!(__instance.getModuleType() is SolarModule type))return;
    __result&=Has(type.Research);
    if(type is ModuleTypeGeothermalPlant)__result&=Survey.IsScanned(__instance.getPosition())&&Survey.Heat(__instance.getPosition())>=.55f;
   }
  }
 }
 public static class SolarApi
 {
  public static event Action<Vector3> SurveyCompleted;
  public static event Action<string> ResearchCompleted;
  internal static void Scanned(Vector3 p){if(SurveyCompleted==null)return;foreach(Action<Vector3> action in SurveyCompleted.GetInvocationList())try{action(p);}catch(Exception e){Debug.LogWarning(e);}}
  internal static void Researched(string id){if(ResearchCompleted==null)return;foreach(Action<string> action in ResearchCompleted.GetInvocationList())try{action(id);}catch(Exception e){Debug.LogWarning(e);}}
 }
}
