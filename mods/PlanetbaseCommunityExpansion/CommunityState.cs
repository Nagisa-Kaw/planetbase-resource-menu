using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using HarmonyLib;
using Planetbase;
using UnityEngine;
namespace PlanetbaseCommunityExpansion
{
 internal sealed class Order
 {
  internal int Unit,Source=-1,Destination=-1,Target=-1,Phase;
  internal string Mode="",Kind="";
  internal float Work,Age;
  internal readonly List<int> Cargo=new List<int>();
  internal int Feature=>Mode=="patrol"?0:Mode=="freight"?1:Mode=="explore"?2:Mode=="rescue"||Mode=="evacuate"?3:4;
 }
 internal sealed class Site{internal int Id,Kind;internal Vector3 Position;internal bool Done;}
 internal sealed class ModuleProgress{internal float Clock;internal int Charges,Target=-1;}
 internal sealed class CommunityState
 {
  static readonly ConditionalWeakTable<GameStateGame,CommunityState> States=new ConditionalWeakTable<GameStateGame,CommunityState>();
  internal static CommunityState Current=>World.Game==null?null:States.GetOrCreateValue(World.Game);
  internal readonly bool[] Enabled={true,true,true,true,true,true};
  internal readonly Dictionary<int,Order> Orders=new Dictionary<int,Order>();
  internal readonly Dictionary<int,ModuleProgress> Machines=new Dictionary<int,ModuleProgress>();
  internal readonly HashSet<int> Detected=new HashSet<int>();
  internal readonly List<Site> Sites=new List<Site>();
  internal bool SitesMapped,Emergency;internal int PreviousAlert;
  internal int Source=-1,Destination=-1,ResourceIndex,Contract=-1,ContractsDone,Import=-1,Priority;
  internal float Deadline,Shipping,ImportTime,TradeCooldown;
  internal bool ExportLoaded;
  internal ModuleProgress Machine(int id){if(!Machines.TryGetValue(id,out var p))Machines[id]=p=new ModuleProgress();return p;}
  internal static float F(XmlElement e,string key,float fallback=0)=>float.TryParse(e.GetAttribute(key),NumberStyles.Float,CultureInfo.InvariantCulture,out var n)&&!float.IsNaN(n)&&!float.IsInfinity(n)?n:fallback;
  internal static int I(XmlElement e,string key,int fallback=-1)=>int.TryParse(e.GetAttribute(key),NumberStyles.Integer,CultureInfo.InvariantCulture,out var n)?n:fallback;
  internal static void A(XmlElement e,string key,float v)=>e.SetAttribute(key,XmlConvert.ToString(v));
  static XmlElement Node(XmlNode parent,string name){var e=parent.OwnerDocument.CreateElement(name);parent.AppendChild(e);return e;}
  [HarmonyPatch(typeof(Serialization),nameof(Serialization.endSave))]
  static class Save
  {
   static void Prefix(XmlNode ___mRootNode)
   {
    var s=Current;if(___mRootNode?.Name!="save-game"||s==null)return;
    var e=Node(___mRootNode,"community-expansion");e.SetAttribute("version","1");e.SetAttribute("modules",string.Join("",s.Enabled.Select(x=>x?"1":"0")));e.SetAttribute("mapped",s.SitesMapped?"true":"false");e.SetAttribute("emergency",s.Emergency?"true":"false");
    A(e,"previous-alert",s.PreviousAlert);A(e,"source",s.Source);A(e,"destination",s.Destination);A(e,"resource",s.ResourceIndex);A(e,"priority",s.Priority);A(e,"contract",s.Contract);A(e,"completed",s.ContractsDone);A(e,"deadline",s.Deadline);A(e,"shipping",s.Shipping);e.SetAttribute("export-loaded",s.ExportLoaded?"true":"false");A(e,"import",s.Import);A(e,"import-time",s.ImportTime);A(e,"trade-cooldown",s.TradeCooldown);
    foreach(var o in s.Orders.Values){var n=Node(e,"order");A(n,"unit",o.Unit);A(n,"source",o.Source);A(n,"destination",o.Destination);A(n,"target",o.Target);A(n,"phase",o.Phase);n.SetAttribute("mode",o.Mode);n.SetAttribute("kind",o.Kind);A(n,"work",o.Work);A(n,"age",o.Age);foreach(int id in o.Cargo){var r=Node(n,"cargo");A(r,"id",id);}}
    foreach(var site in s.Sites){var n=Node(e,"site");A(n,"id",site.Id);A(n,"kind",site.Kind);A(n,"x",site.Position.x);A(n,"y",site.Position.y);A(n,"z",site.Position.z);n.SetAttribute("done",site.Done?"true":"false");}
    foreach(var pair in s.Machines){var n=Node(e,"machine");A(n,"id",pair.Key);A(n,"clock",pair.Value.Clock);A(n,"charges",pair.Value.Charges);}
    foreach(int id in s.Detected){var n=Node(e,"detected");A(n,"id",id);}
   }
  }
  [HarmonyPatch(typeof(GameStateGame),nameof(GameStateGame.stageLoadGame2))]
  static class Load
  {
   static void Prefix(GameStateGame __instance,XmlNode ___mRootNode)
   {
    var s=States.GetOrCreateValue(__instance);if(!(___mRootNode?["community-expansion"] is XmlElement e))return;
    string modules=e.GetAttribute("modules");for(int i=0;i<6&&i<modules.Length;i++)s.Enabled[i]=modules[i]!='0';s.SitesMapped=e.GetAttribute("mapped")=="true";s.Emergency=e.GetAttribute("emergency")=="true";
    s.PreviousAlert=Mathf.Clamp(I(e,"previous-alert",0),0,2);s.Source=I(e,"source");s.Destination=I(e,"destination");s.ResourceIndex=Math.Max(0,I(e,"resource",0));s.Priority=Mathf.Clamp(I(e,"priority",0),0,3);s.Contract=I(e,"contract");if(s.Contract<0||s.Contract>=Commerce.Contracts.Length)s.Contract=-1;s.ContractsDone=Math.Max(0,I(e,"completed",0));s.Deadline=Mathf.Clamp(F(e,"deadline"),0,7200);s.Shipping=Mathf.Clamp(F(e,"shipping"),0,45);s.ExportLoaded=s.Contract>=0&&e.GetAttribute("export-loaded")=="true";s.Import=I(e,"import");if(s.Import<0||s.Import>2)s.Import=-1;s.ImportTime=Mathf.Clamp(F(e,"import-time"),0,60);s.TradeCooldown=Mathf.Clamp(F(e,"trade-cooldown"),0,120);
    foreach(XmlNode child in e.ChildNodes)if(child is XmlElement n){
     if(n.Name=="order"&&s.Orders.Count<256){string mode=n.GetAttribute("mode");if(!new[]{"patrol","freight","explore","rescue","evacuate","training"}.Contains(mode))continue;int id=I(n,"unit");if(id<0)continue;var o=new Order{Unit=id,Mode=mode,Kind=n.GetAttribute("kind"),Source=I(n,"source"),Destination=I(n,"destination"),Target=I(n,"target"),Phase=Mathf.Clamp(I(n,"phase",0),0,mode=="patrol"?7:4),Work=Mathf.Clamp(F(n,"work"),0,300),Age=Mathf.Clamp(F(n,"age"),0,1800)};foreach(XmlNode r in n.ChildNodes)if(r is XmlElement c&&c.Name=="cargo"&&o.Cargo.Count<3){int cargo=I(c,"id");if(cargo>=0&&!o.Cargo.Contains(cargo))o.Cargo.Add(cargo);}s.Orders[id]=o;}
     else if(n.Name=="site"&&s.Sites.Count<16){int id=I(n,"id");var p=new Vector3(F(n,"x"),F(n,"y"),F(n,"z"));if(id<0||p.x<0||p.x>2048||p.z<0||p.z>2048||s.Sites.Any(x=>x.Id==id))continue;s.Sites.Add(new Site{Id=id,Kind=Mathf.Clamp(I(n,"kind",0),0,2),Position=p,Done=n.GetAttribute("done")=="true"});}
     else if(n.Name=="machine"&&s.Machines.Count<2048){int id=I(n,"id");if(id>=0)s.Machines[id]=new ModuleProgress{Clock=Mathf.Clamp(F(n,"clock"),0,60),Charges=Mathf.Clamp(I(n,"charges",0),0,20)};}
     else if(n.Name=="detected"&&s.Detected.Count<4096){int id=I(n,"id");if(id>=0)s.Detected.Add(id);}
    }
   }
  }
  [HarmonyPatch(typeof(GameStateGame),nameof(GameStateGame.fixedUpdate))]
  static class Simulation
  {
   static void Postfix(float timeStep){float dt=timeStep*Singleton<TimeManager>.getInstance().getTimeScale();if(dt<=0||Current==null)return;Defense.Tick(dt);PlanetbaseCommunityExpansion.Orders.Tick(dt);Commerce.Tick(dt);}
  }
 }
}
