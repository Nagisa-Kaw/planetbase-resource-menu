using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;
using HarmonyLib;
using Planetbase;
using UnityEngine;
using Module=Planetbase.Module;
namespace PlanetbaseSolarFrontier
{
 internal sealed class SolarState
 {
  private static readonly ConditionalWeakTable<GameStateGame,SolarState> States=new ConditionalWeakTable<GameStateGame,SolarState>();
  internal static SolarState Current=>States.GetOrCreateValue(WorldObjects.Game);
  internal int Project=-1;internal float Progress,RaidClock;internal bool Raids;
  internal readonly List<Vector3> Surveys=new List<Vector3>();
  internal static float Number(XmlElement e,string key,float fallback=0){return float.TryParse(e.GetAttribute(key),System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out float f)&&!float.IsNaN(f)&&!float.IsInfinity(f)?f:fallback;}
  internal static void Float(XmlElement e,string key,float value)=>e.SetAttribute(key,XmlConvert.ToString(value));
  [HarmonyPatch(typeof(Serialization),nameof(Serialization.endSave))]
  private static class Save
  {
   static void Prefix(XmlNode ___mRootNode)
   {
    if(___mRootNode?.Name!="save-game"||WorldObjects.Game==null)return;
    SolarState s=Current;XmlElement e=___mRootNode.OwnerDocument.CreateElement("solar-frontier");e.SetAttribute("version","1");Float(e,"project",s.Project);Float(e,"progress",s.Progress);Float(e,"raid-clock",s.RaidClock);e.SetAttribute("raids",s.Raids?"true":"false");
    foreach(Vector3 p in s.Surveys){XmlElement n=e.OwnerDocument.CreateElement("survey");Float(n,"x",p.x);Float(n,"z",p.z);e.AppendChild(n);}___mRootNode.AppendChild(e);
   }
  }
  [HarmonyPatch(typeof(GameStateGame),nameof(GameStateGame.stageLoadGame2))]
  private static class Load
  {
   static void Prefix(GameStateGame __instance,XmlNode ___mRootNode)
   {
    SolarState s=States.GetOrCreateValue(__instance);s.Surveys.Clear();s.Project=-1;s.Progress=0;s.Raids=false;s.RaidClock=0;
    if(!(___mRootNode?["solar-frontier"] is XmlElement e))return;
    int project=(int)Number(e,"project",-1);s.Project=project>=0&&project<Research.Projects.Length?project:-1;s.Progress=Mathf.Max(0,Number(e,"progress"));s.Raids=e.GetAttribute("raids")=="true";s.RaidClock=Mathf.Clamp(Number(e,"raid-clock"),0,1800);
    foreach(XmlNode n in e.ChildNodes)if(n is XmlElement v&&v.Name=="survey"&&s.Surveys.Count<4096){Vector3 p=new Vector3(Number(v,"x"),0,Number(v,"z"));if(p.x>=0&&p.x<=2048&&p.z>=0&&p.z<=2048)s.Surveys.Add(p);}
   }
  }
  [HarmonyPatch(typeof(GameStateGame),nameof(GameStateGame.fixedUpdate))]
  private static class Tick
  {
   static void Postfix(float timeStep){float dt=timeStep*Singleton<TimeManager>.getInstance().getTimeScale();if(dt<=0)return;Research.Tick(dt);Threats.Tick(dt);}
  }
 }
 internal sealed class MachineState
 {
  private static readonly ConditionalWeakTable<Module,MachineState> All=new ConditionalWeakTable<Module,MachineState>();
  internal static MachineState For(Module m)=>All.GetOrCreateValue(m);
  internal float Fuel,Cycle,Nutrients;
  [HarmonyPatch(typeof(Module),nameof(Module.serialize))]
  private static class Save
  {
   static void Postfix(Module __instance,XmlNode parent)
   {
    if(!(__instance.getModuleType() is SolarModule))return;MachineState s=For(__instance);XmlElement e=parent.OwnerDocument.CreateElement("frontier-machine");SolarState.Float(e,"fuel",s.Fuel);SolarState.Float(e,"cycle",s.Cycle);SolarState.Float(e,"nutrients",s.Nutrients);parent.LastChild.AppendChild(e);
   }
  }
  [HarmonyPatch(typeof(Module),nameof(Module.deserialize))]
  private static class Load
  {
   static void Postfix(Module __instance,XmlNode node)
   {
    if(!(node["frontier-machine"] is XmlElement e))return;MachineState s=For(__instance);s.Fuel=Mathf.Clamp(SolarState.Number(e,"fuel"),0,1200);s.Cycle=Mathf.Clamp(SolarState.Number(e,"cycle"),0,600);s.Nutrients=Mathf.Clamp(SolarState.Number(e,"nutrients"),0,600);
   }
  }
 }
 internal static class Threats
 {
  internal static void Tick(float dt)
  {
   SolarState s=SolarState.Current;if(!s.Raids)return;s.RaidClock+=dt;if(s.RaidClock<1800)return;s.RaidClock=0;
   Module origin=WorldObjects.Modules.Find(m=>m.isBuilt());if(origin==null)return;
   for(int i=0;i<16;i++)
   {
    float a=i*Mathf.PI/8;Vector3 p=origin.getPosition()+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*160;
    if(!PhysicsUtil.findFloor(p,out Vector3 ground)||!NavigationGraph.isExteriorLocationReachable(ground))continue;
    Character.create(SpecializationList.IntruderInstance,ground,Location.Exterior);WorldObjects.Toast("Incursión de saqueadores: un intruso se aproxima a la colonia.");break;
   }
  }
 }
}
