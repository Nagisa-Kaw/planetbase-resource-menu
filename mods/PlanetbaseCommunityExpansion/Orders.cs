using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Planetbase;
using PlanetbaseSolarFrontier;
using UnityEngine;
using Module=Planetbase.Module;
namespace PlanetbaseCommunityExpansion
{
 internal static class Orders
 {
  sealed class Router:AiRule
  {
   public override bool update(Character c)=>false;
   internal static bool Go(Character c,Vector3 p,Location location,Selectable selectable=null)
   {
    if(c.getLocation()!=location){var airlock=Module.findClosestAirlock(c,location==Location.Exterior?p:c.getPosition());return airlock!=null&&goToBestAirlock(c,airlock);}
    return goTarget(c,selectable==null?new Target(p,location):new Target(selectable,p));
   }
  }
  static float automaticClock;
  internal static bool Owns(Character c)=>c!=null&&CommunityState.Current?.Orders.ContainsKey(c.getId())==true;
  internal static Order Passenger(Character c)=>CommunityState.Current?.Orders.Values.FirstOrDefault(o=>o.Mode=="rescue"&&o.Phase==1&&o.Target==c.getId());
  static bool Near(Character c,Vector3 p,Location location,float radius=2)=>c.getLocation()==location&&(c.getPosition()-p).sqrMagnitude<radius*radius;
  static bool Go(Character c,Vector3 p,Location location,Selectable selectable=null){if(c.anyInteractions()||c.getState()==Character.State.Walking)return true;return Router.Go(c,p,location,selectable);}
  static ConstructionComponent Component(int id)=>World.Modules.SelectMany(m=>m.getComponents()).FirstOrDefault(c=>!c.isDestroyed()&&c.getId()==id);
  internal static void Drop(Order o,Character c)
  {
   Vector3 p=c?.getPosition()??World.Module(o.Source)?.getFloorPosition()??Singleton<TerrainGenerator>.getInstance().getCenter();Location location=c?.getLocation()??Location.Exterior;
   foreach(int id in o.Cargo){var r=World.Cargo(id);if(r==null)continue;r.detach();r.setPosition(p+Vector3.right*((id%3)-1));r.setLocation(location);r.drop(Resource.State.Idle);}o.Cargo.Clear();
  }
  internal static void Cancel(int id)
  {
   var state=CommunityState.Current;if(state==null||!state.Orders.TryGetValue(id,out var o))return;var c=World.Unit(id);
   Drop(o,c);
   if(o.Mode=="rescue"&&o.Phase==1){var patient=World.Unit(o.Target);if(patient!=null&&!patient.isDead()){if(c!=null){patient.setPosition(c.getPosition()+Vector3.right);World.Location(patient,c.getLocation());}if(!patient.isKo())patient.setIdle();}}
   if(o.Mode=="training"&&c!=null&&c.anyInteractions()&&c.getFirstInteraction().getSelectable() is ConstructionComponent desk&&desk.getId()==o.Destination)c.getFirstInteraction().destroy();
   state.Orders.Remove(id);if(c!=null&&!c.isDead()&&!c.isKo()&&!c.anyInteractions()&&!c.isFighting()&&!c.isFleeing())c.setIdle();
  }
  internal static string Route(Character c)
  {
   var s=CommunityState.Current;var source=World.Module(s.Source);var destination=World.Module(s.Destination);
   if(!s.Enabled[1]||World.Working<ModuleTypeLogisticsHub>()==null)return "Hace falta un centro de distribución operativo.";
   if(c==null||!(c.getSpecialization() is FreightHauler)||!World.Healthy(c)||c.isLoaded()||c.anyInteractions())return "Selecciona un transportador pesado fabricado, sano y libre.";
   if(source==null||destination==null||source==destination||!source.hasFlag(ModuleType.FlagStorage)||!destination.hasFlag(ModuleType.FlagStorage)||source.getGrid()==null||source.getGrid()!=destination.getGrid())return "Elige dos almacenes diferentes de la misma red.";
   var resources=HaulTypes();if(s.ResourceIndex<0||s.ResourceIndex>=resources.Length)s.ResourceIndex=0;
   Cancel(c.getId());ManualControl.Release(c);s.Orders[c.getId()]=new Order{Unit=c.getId(),Mode="freight",Source=source.getId(),Destination=destination.getId(),Kind=resources[s.ResourceIndex].GetType().Name};return "Ruta creada: hasta tres cajas por viaje. F10 permite cancelarla.";
  }
  internal static ResourceType[] HaulTypes()=>TypeList<ResourceType,ResourceTypeList>.get().Where(r=>!r.hasFlag(ResourceType.FlagCurrency)).ToArray();
  internal static string Train(Character c,string profession)
  {
   var s=CommunityState.Current;var desk=World.Modules.Where(m=>m.getModuleType() is ModuleTypeAcademy&&m.isOperational()).SelectMany(m=>m.getComponents()).FirstOrDefault(d=>d.getComponentType() is TrainingConsole&&d.isOperational()&&!d.anyInteractions()&&!s.Orders.Values.Any(o=>o.Mode=="training"&&o.Destination==d.getId()));
   if(!s.Enabled[4]||desk==null)return "Hace falta una academia y un puesto de formación libre y operativo.";
   if(!(c is Colonist)||!World.Healthy(c)||c.isLoaded()||c.anyInteractions()||Owns(c))return "Selecciona un colono sano, libre y sin otra tarea asignada.";
   if(!new[]{"Worker","Biologist","Engineer","Medic","Guard"}.Contains(profession)||c.getSpecialization().GetType().Name==profession)return "Elige una profesión diferente.";
   var academy=(Module)desk.getParentConstruction();if(Stock.Find(academy,"ResearchPacket").Count<2||Resource.getCountOfType(ResourceTypeList.CoinsInstance)<20)return "Se requieren 2 informes disponibles en la red y 20 créditos.";
   if(!Stock.Pay(20))return "No se pudo pagar la formación.";if(!Stock.Consume(academy,"ResearchPacket",2)){Stock.Coins(20);return "Los informes dejaron de estar disponibles.";}
   ManualControl.Release(c);s.Orders[c.getId()]=new Order{Unit=c.getId(),Mode="training",Destination=desk.getId(),Kind=profession};return "Formación iniciada: 300 s de estudio. Cancelarla pierde el coste pagado.";
  }
  internal static void Profession(Character c,string name)
  {
   Specialization old=c.getSpecialization(),next=TypeList<Specialization,SpecializationList>.find(name);if(!(c is Colonist)||next==null)return;
   var groups=(Dictionary<Specialization,List<Character>>)AccessTools.Field(typeof(Character),"mSpecializationCharacters").GetValue(null);groups[old].Remove(c);if(!groups.ContainsKey(next))groups[next]=new List<Character>();groups[next].Add(c);
   AccessTools.Field(typeof(Character),"mSpecialization").SetValue(c,next);AccessTools.Field(typeof(Character),"mAi").SetValue(c,next.getAi());
   var gender=(Human.Gender)AccessTools.Field(typeof(Human),"mGender").GetValue(c);int head=(int)AccessTools.Field(typeof(Human),"mHeadIndex").GetValue(c);head=Mathf.Clamp(head,0,Math.Max(0,next.getHeadCount(gender)-1));AccessTools.Field(typeof(Human),"mHeadIndex").SetValue(c,head);var ethnic=next.getEthnicParameters(gender,head);
   if(ethnic!=null){foreach(var p in new[]{new[]{"mSkinColorIndex",ethnic.SkinColors.Length.ToString()},new[]{"mHairColorIndex",ethnic.HairColors.Length.ToString()}}){var field=AccessTools.Field(typeof(Human),p[0]);field.SetValue(c,Mathf.Clamp((int)field.GetValue(c),0,Math.Max(0,int.Parse(p[1])-1)));}}
   c.createModel();c.setIdle();c.playIdleAnimation(CharacterAnimation.PlayMode.Immediate);World.Toast(c.getName()+": formación terminada, nueva profesión "+next.getName()+".");
  }
  internal static string Emergency()
  {
   var s=CommunityState.Current;if(!s.Enabled[3]&&!s.Emergency)return "Activa el módulo de rescate.";s.Emergency=!s.Emergency;
   var security=Singleton<SecurityManager>.getInstance();if(s.Emergency){s.PreviousAlert=(int)security.getAlertState();if(security.getAlertState()==AlertState.NoAlert)security.setAlertState(AlertState.YellowAlert);}else if(s.PreviousAlert==0&&security.getAlertState()==AlertState.YellowAlert)security.setAlertState(AlertState.NoAlert);
   if(!s.Emergency)foreach(var o in s.Orders.Values.Where(o=>o.Mode=="evacuate").ToArray())Cancel(o.Unit);
   return s.Emergency?"Evacuación activa: los civiles exteriores buscarán un refugio por las esclusas.":"Emergencia terminada; se restablece el trabajo normal.";
  }
  static void Auto()
  {
   var s=CommunityState.Current;
   if(s.Enabled[3]&&s.Emergency){var shelter=World.Working<ModuleTypeEmergencyShelter>();if(shelter!=null)foreach(var c in World.Characters.Where(c=>c is Colonist&&!(c.getSpecialization() is Guard)&&World.Healthy(c)&&!c.isLoaded()&&!c.anyInteractions()&&c.getLocation()==Location.Exterior&&!Owns(c)&&Passenger(c)==null).ToArray()){ManualControl.Release(c);s.Orders[c.getId()]=new Order{Unit=c.getId(),Mode="evacuate",Destination=shelter.getId()};}}
   if(!s.Enabled[3]||World.Working<ModuleTypeRescueHangar>()==null||World.Working<ModuleTypeEmergencyHospital>()==null)return;
   foreach(var rover in World.Characters.Where(c=>c.getSpecialization() is RescueRover&&World.Healthy(c)&&!c.isLoaded()&&!c.anyInteractions()&&!Owns(c)).ToArray()){
    var patient=World.Characters.Where(c=>c is Colonist&&!c.isDead()&&c.getLocation()==Location.Exterior&&!c.anyInteractions()&&(c.isKo()||c.isIndicatorLow(CharacterIndicator.Health))&&!s.Orders.Values.Any(o=>o.Mode=="rescue"&&o.Target==c.getId())&&World.Modules.Any(m=>m.getModuleType() is ModuleTypeRescueHangar&&m.isOperational()&&(m.getPosition()-c.getPosition()).sqrMagnitude<180*180)).OrderBy(c=>(c.getPosition()-rover.getPosition()).sqrMagnitude).FirstOrDefault();
    var bed=World.Modules.Where(m=>m.getModuleType() is ModuleTypeEmergencyHospital&&m.isOperational()).SelectMany(m=>m.getComponents()).FirstOrDefault(c=>c.getComponentType() is TraumaBed&&c.isOperational()&&!c.anyInteractions()&&!s.Orders.Values.Any(o=>o.Mode=="rescue"&&o.Destination==c.getId()));
    if(patient==null||bed==null)break;if(Owns(patient))Cancel(patient.getId());ManualControl.Release(rover);s.Orders[rover.getId()]=new Order{Unit=rover.getId(),Mode="rescue",Target=patient.getId(),Destination=bed.getId()};
   }
  }
  internal static void Tick(float dt)
  {
   var s=CommunityState.Current;automaticClock+=dt;if(automaticClock>=1){automaticClock=0;Auto();}
   foreach(var o in s.Orders.Values.ToArray()){
    var c=World.Unit(o.Unit);if(c==null||!s.Enabled[o.Feature]||!World.Healthy(c)||c.isLoaded()){Cancel(o.Unit);continue;}
    o.Age+=dt;if(o.Age>1800&&o.Mode!="patrol"){Cancel(o.Unit);World.Toast("Tarea cancelada: tiempo de recorrido agotado.");continue;}
    if(o.Mode=="freight")Freight(c,o);
    else if(o.Mode=="patrol")Patrol(c,o);
    else if(o.Mode=="explore")Explore(c,o,dt);
    else if(o.Mode=="training")Training(c,o,dt);
    else if(o.Mode=="rescue")Rescue(c,o,dt);
    else if(o.Mode=="evacuate"){var m=World.Module(o.Destination);if(m==null||!m.isOperational()){Cancel(o.Unit);continue;}if(Near(c,m.getFloorPosition(),Location.Interior)){Cancel(o.Unit);continue;}Go(c,m.getFloorPosition(),Location.Interior,m);}
   }
  }
  static void Freight(Character c,Order o)
  {
   var from=World.Module(o.Source);var to=World.Module(o.Destination);if(from==null||to==null||from.getGrid()==null||from.getGrid()!=to.getGrid()){Cancel(o.Unit);return;}if(!from.isOperational()||!to.isOperational()){o.Age=0;return;}
   var target=o.Cargo.Count==0?from:to;
   if(!Near(c,target.getFloorPosition(),target.getLocation(),3)){Go(c,target.getFloorPosition(),target.getLocation(),target);return;}
   o.Age=0;
   if(o.Cargo.Count==0){if(to.findStorageSlot(to.getFloorPosition())==null)return;foreach(var r in World.Resources.Where(r=>r.getResourceType().GetType().Name==o.Kind&&r.getState()==Resource.State.Stored&&World.Available(r)&&!r.isEmbedded()&&(r.getPosition()-from.getPosition()).sqrMagnitude<from.getRadius()*from.getRadius()).OrderBy(r=>r.getId()).Take(3).ToArray()){r.load();o.Cargo.Add(r.getId());}}
   else{foreach(int id in o.Cargo.ToArray()){var r=World.Cargo(id);if(r==null){o.Cargo.Remove(id);continue;}if(to.findStorageSlot(c.getPosition())==null)break;r.setLocation(c.getLocation());c.loadResource(r);c.storeResource(to);o.Cargo.Remove(id);}}
  }
  static void Patrol(Character c,Order o)
  {
   var post=World.Module(o.Source);if(post==null||!post.isOperational()||World.Working<ModuleTypeSecurityCommand>()==null){Cancel(o.Unit);return;}
   if(World.Characters.Any(x=>x.getSpecialization()==SpecializationList.IntruderInstance&&!x.isDead()&&(x.getPosition()-c.getPosition()).sqrMagnitude<50*50)){Cancel(o.Unit);return;}
   for(int i=0;i<8;i++){float a=o.Phase*Mathf.PI/4;var p=post.getPosition()+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*24;if(!PhysicsUtil.findFloor(p,out var ground)||!NavigationGraph.isExteriorLocationReachable(ground)||World.Modules.Any(m=>(m.getPosition()-ground).sqrMagnitude<(m.getRadius()+3)*(m.getRadius()+3))){o.Phase=(o.Phase+1)%8;continue;}if(Near(c,ground,Location.Exterior,3)){o.Phase=(o.Phase+1)%8;o.Age=0;continue;}if(Go(c,ground,Location.Exterior))break;o.Phase=(o.Phase+1)%8;}
  }
  static void Explore(Character c,Order o,float dt)
  {
   var s=CommunityState.Current;var site=s.Sites.Find(x=>x.Id==o.Target);var home=World.Module(o.Source);if(site==null||home==null||!home.isOperational()){if(home==null)Cancel(o.Unit);return;}
   if(o.Phase==0){if(site.Done){Cancel(o.Unit);return;}if(!Near(c,site.Position,Location.Exterior,3)){Go(c,site.Position,Location.Exterior);return;}o.Work+=dt;if(o.Work>=10)Exploration.Recover(c,o,site);}
   else{if(Near(c,home.getFloorPosition(),Location.Interior,3)){Drop(o,c);Cancel(o.Unit);World.Toast("Expedición recibida: las muestras y cargas están en el centro.");}else Go(c,home.getFloorPosition(),Location.Interior,home);}
  }
  static void Training(Character c,Order o,float dt)
  {
   var desk=Component(o.Destination);if(desk==null){Cancel(o.Unit);return;}if(!desk.isOperational()){if(c.anyInteractions()&&c.getFirstInteraction().getSelectable()==desk)c.getFirstInteraction().destroy();o.Age=0;return;}
   var anchor=desk.getAnchorPoint();if(anchor==null){Cancel(o.Unit);return;}
   if(c.anyInteractions()){if(c.getFirstInteraction().getSelectable()==desk){o.Work+=dt;o.Age=0;if(o.Work>=300){string kind=o.Kind;Cancel(o.Unit);Profession(c,kind);}}return;}
   if(Near(c,anchor.position,Location.Interior,1)){Interaction.create<InteractionComponentRestore>(c,desk);return;}Go(c,anchor.position,Location.Interior,desk);
  }
  static void Rescue(Character c,Order o,float dt)
  {
   var patient=World.Unit(o.Target);var bed=Component(o.Destination);if(patient==null||patient.isDead()||bed==null){Cancel(o.Unit);return;}if(!bed.isOperational()){o.Age=0;return;}
   if(o.Phase==0){if(patient.getLocation()!=Location.Exterior||patient.anyInteractions()){Cancel(o.Unit);return;}if(Near(c,patient.getPosition(),Location.Exterior,2.5f)){if(Owns(patient))Cancel(patient.getId());AccessTools.Method(typeof(Character),"destroyInteractions").Invoke(patient,null);o.Phase=1;o.Age=0;World.Toast("Ambulancia: paciente recogido; en ruta al hospital.");}else Go(c,patient.getPosition(),Location.Exterior,patient);}
   else{
    patient.setPosition(c.getGameObject().transform.TransformPoint(new Vector3(0,.72f,-.35f)));World.Location(patient,c.getLocation());patient.restoreStatus(CharacterIndicator.Oxygen,dt/15);
    var anchor=bed.getAnchorPoint();if(anchor==null){Cancel(o.Unit);return;}
    if(Near(c,anchor.position,Location.Interior,2.5f)&&!bed.anyInteractions()){o.Phase=2;patient.setIdle();patient.setPosition(anchor.position);World.Location(patient,Location.Interior);Interaction.create<InteractionWaitForHealing>(patient,bed);Cancel(o.Unit);World.Toast("Paciente entregado al hospital; requiere atención de un médico.");}
    else Go(c,anchor.position,Location.Interior,bed);
   }
  }
  internal static void SyncCargo(Character c)
  {
   if(CommunityState.Current?.Orders.TryGetValue(c.getId(),out var o)!=true)return;
   for(int i=0;i<o.Cargo.Count;i++){var r=World.Cargo(o.Cargo[i]);if(r==null)continue;r.setPosition(c.getGameObject().transform.TransformPoint(new Vector3(i==2?0:i==0?-.43f:.43f,.83f,i==2?-1.25f:-.45f)));r.setRotation(c.getGameObject().transform.rotation);r.setLocation(c.getLocation());}
  }
  [HarmonyPatch(typeof(BaseAi),nameof(BaseAi.updateIdle))]
  static class Idle{[HarmonyPriority(Priority.First)]static bool Prefix(Character character){if(Passenger(character)!=null)return false;if(!Owns(character))return true;if(!World.Healthy(character)){Cancel(character.getId());return true;}return false;}}
  [HarmonyPatch(typeof(BaseAi),nameof(BaseAi.onTargetReached))]
  static class Arrival{[HarmonyPriority(Priority.First)]static bool Prefix(Character character){if(!Owns(character)||character.getTargetConstruction()?.hasFlag(ModuleType.FlagAirlock)==true)return true;character.setIdle();return false;}}
  [HarmonyPatch(typeof(Character),nameof(Character.update))]
  static class Riding{static bool Prefix(Character __instance)=>Passenger(__instance)==null;static void Postfix(Character __instance){if(Owns(__instance)){var view=__instance.getGameObject().GetComponent<OrderView>();if(view==null)view=__instance.getGameObject().AddComponent<OrderView>();view.Owner=__instance;}}}
  [HarmonyPatch(typeof(Character),nameof(Character.destroy))]
  static class Destroyed{static void Prefix(Character __instance){var passenger=Passenger(__instance);if(passenger!=null)Cancel(passenger.Unit);Cancel(__instance.getId());}}
  [HarmonyPatch(typeof(Character),nameof(Character.isProtected))]
  static class Protection{static void Postfix(Character __instance,ref bool __result){if(Passenger(__instance)!=null)__result=true;}}
  [HarmonyPatch(typeof(Character),nameof(Character.getDescription))]
  static class Status{static void Postfix(Character __instance,ref string __result){if(CommunityState.Current?.Orders.TryGetValue(__instance.getId(),out var o)==true){string name=o.Mode=="freight"?"transporte":o.Mode=="patrol"?"patrulla":o.Mode=="explore"?"expedición":o.Mode=="rescue"?"rescate":o.Mode=="evacuate"?"evacuación":"formación";__result+="\nColonias en expansión: "+name+(o.Mode=="training"?" · "+o.Work.ToString("0")+"/300 s":"")+(o.Cargo.Count>0?" · "+o.Cargo.Count+" cargas":"");}}}
 }
}
