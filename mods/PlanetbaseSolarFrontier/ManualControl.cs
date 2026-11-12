using System;
using System.Runtime.CompilerServices;
using System.Xml;
using HarmonyLib;
using Planetbase;
using UnityEngine;
namespace PlanetbaseSolarFrontier
{
 internal sealed class UnitState
 {
  internal static readonly ConditionalWeakTable<Character,UnitState> All=new ConditionalWeakTable<Character,UnitState>();
  internal static UnitState For(Character c)=>All.GetOrCreateValue(c);
  internal string Order="";internal Vector3 Target;internal float Work,Age,Dose;internal bool Automatic;
 }
 public static class ManualControl
 {
  internal static Character Pending;internal static string PendingOrder;
  private sealed class Route:AiRule
  {
   public override bool update(Character c)=>false;
   internal static bool Go(Character c,Vector3 p)
   {
    if(c.getLocation()==Location.Interior){var airlock=Planetbase.Module.findClosestAirlock(c,p);return airlock!=null&&goToBestAirlock(c,airlock);}
    return goTarget(c,new Target(p,Location.Exterior));
   }
  }
  public static bool Issue(Character c,Vector3 target,bool scan=false,bool automatic=false)
  {
   if(c==null||!(c is Colonist||c is Bot)||c.isDead()||c.isKo()||c.anyInteractions()||c.isFighting()||c.isFleeing())return false;
   if(scan&&!Research.Has(typeof(TechSolarSurvey)))return false;
   if(!PhysicsUtil.findFloor(target,out Vector3 ground)||!NavigationGraph.isExteriorLocationReachable(ground))return false;
   UnitState s=UnitState.For(c);s.Order=scan?"scan":"move";s.Target=ground;s.Work=s.Age=0;s.Automatic=automatic;c.setIdle();bool ok=Route.Go(c,ground);if(!ok)s.Order="";return ok;
  }
  public static void Release(Character c){if(c==null)return;UnitState.For(c).Order="";if(!c.isDestroyed()&&!c.anyInteractions())c.setIdle();}
  public static void Hold(Character c){if(c==null||c.anyInteractions()||c.isDead())return;c.setIdle();var s=UnitState.For(c);s.Order="hold";s.Age=0;s.Work=0;}
  private static bool Unsafe(Character c)=>c.isDead()||c.isKo()||(c is Human&&(!c.isFitForCriticalWork()||c.isStatusVeryLow(CharacterIndicator.Oxygen)))||(c is Bot&&(c.isIndicatorLow(CharacterIndicator.Condition)||c.isIndicatorLow(CharacterIndicator.Integrity)));
  [HarmonyPatch(typeof(BaseAi),nameof(BaseAi.updateIdle))]
  private static class OverrideIdle
  {
   static bool Prefix(Character character)
   {
    UnitState s=UnitState.For(character);
    if(Unsafe(character)){s.Order="";return true;}
    if(s.Order.Length==0&&(character.getSpecialization() is ScoutDrone||character.getSpecialization() is ScannerBot)&&Research.Has(typeof(TechSolarSurvey)))
    {
     Planetbase.Module first=WorldObjects.Modules.Find(m=>m.isBuilt());if(first!=null)
     {
      for(int i=0;i<16;i++){float a=i*Mathf.PI/8;Vector3 p=first.getPosition()+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*85;if(!Survey.IsScanned(p)&&Issue(character,p,true,true))return false;}
     }
     // Once the nearby ring is mapped, scanners wait for a manual destination.
     // Native survival and repair rules remain available through Unsafe above.
     return false;
    }
    if(s.Order.Length==0)return true;
    if(s.Order=="hold")return false;
    if(character.getLocation()==Location.Exterior&&(character.getPosition()-s.Target).sqrMagnitude<9)
    {if(s.Order=="move")s.Order="";return false;}
    if(!Route.Go(character,s.Target)){s.Order="";WorldObjects.Toast("Orden cancelada: no hay una ruta disponible.");return true;}
    return false;
   }
  }
  [HarmonyPatch(typeof(BaseAi),nameof(BaseAi.onTargetReached))]
  private static class Arrival
  {
   static bool Prefix(Character character)
   {
    if(UnitState.For(character).Order.Length==0)return true;
    // Native airlock rules must remain responsible for crossing between locations.
    if(character.getTargetConstruction()?.hasFlag(ModuleType.FlagAirlock)==true)return true;
    character.setIdle();return false;
   }
  }
  [HarmonyPatch(typeof(Character),nameof(Character.update))]
  private static class Tick
  {
   static void Postfix(Character __instance,float timeStep)
   {
    UnitState s=UnitState.For(__instance);if(timeStep<=0||s.Order.Length==0)return;
    if(Unsafe(__instance)){Release(__instance);return;}
    s.Age+=timeStep;if(s.Age>600){Release(__instance);return;}
    if(s.Order=="scan"&&__instance.getLocation()==Location.Exterior&&!__instance.anyInteractions()&&(__instance.getPosition()-s.Target).sqrMagnitude<9)
    {s.Work+=timeStep;if(s.Work>=10){Survey.Complete(s.Target);s.Order="";}}
   }
  }
  [HarmonyPatch(typeof(Character),"serialize")]
  private static class Save
  {
   static void Postfix(Character __instance,XmlNode parent)
   {
    UnitState s=UnitState.For(__instance);if(s.Order.Length==0&&s.Dose==0)return;
    XmlElement e=parent.OwnerDocument.CreateElement("frontier-unit");e.SetAttribute("order",s.Order);e.SetAttribute("automatic",s.Automatic?"true":"false");SolarState.Float(e,"x",s.Target.x);SolarState.Float(e,"z",s.Target.z);SolarState.Float(e,"y",s.Target.y);SolarState.Float(e,"work",s.Work);SolarState.Float(e,"age",s.Age);SolarState.Float(e,"dose",s.Dose);parent.LastChild.AppendChild(e);
   }
  }
  [HarmonyPatch(typeof(Character),"deserialize")]
  private static class Load
  {
   static void Postfix(Character __instance,XmlNode node)
   {
    if(!(node["frontier-unit"] is XmlElement e))return;UnitState s=UnitState.For(__instance);string order=e.GetAttribute("order");s.Order=order=="scan"||order=="move"||order=="hold"?order:"";s.Target=new Vector3(SolarState.Number(e,"x"),SolarState.Number(e,"y"),SolarState.Number(e,"z"));s.Work=Mathf.Clamp(SolarState.Number(e,"work"),0,10);s.Age=Mathf.Clamp(SolarState.Number(e,"age"),0,600);s.Dose=Mathf.Clamp01(SolarState.Number(e,"dose"));s.Automatic=e.GetAttribute("automatic")=="true";
   }
  }
  [HarmonyPatch(typeof(Character),nameof(Character.getDescription))]
  private static class Description
  {
   static void Postfix(Character __instance,ref string __result){var s=UnitState.For(__instance);if(s.Order.Length>0)__result+="\nOrden manual: "+s.Order;if(s.Dose>0)__result+="\nExposición a radiación: "+s.Dose.ToString("P0");}
  }
 }
}
