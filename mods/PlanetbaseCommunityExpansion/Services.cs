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
 internal static class Stock
 {
  internal static List<Resource> Find(Module owner,string kind)
  {
   if(owner?.getGrid()==null)return new List<Resource>();
   var network=World.Modules.Where(m=>m.getGrid()==owner.getGrid()&&m.isOperational()).ToArray();
   return World.Resources.Where(r=>r.getResourceType().GetType().Name==kind&&World.Available(r)&&
    (r.isEmbedded()?r.getContainer()?.getParent() is ConstructionComponent c&&c.isOperational()&&c.getParentConstruction().getGrid()==owner.getGrid():
    (r.getState()==Resource.State.Idle||r.getState()==Resource.State.Stored)&&network.Any(m=>m.getLocation()==r.getLocation()&&(m.getPosition()-r.getPosition()).sqrMagnitude<(m.getRadius()+2)*(m.getRadius()+2)))).OrderBy(r=>r.getId()).ToList();
  }
  internal static bool Consume(Module owner,string kind,int amount){var list=Find(owner,kind);if(list.Count<amount)return false;foreach(var r in list.Take(amount))r.destroy();return true;}
  internal static void Coins(int value){int before=Resource.getCountOfType(ResourceTypeList.CoinsInstance);int add=Math.Min(value,int.MaxValue-before);if(add>0)Resource.addInmaterialResource(new ResourceAmount(ResourceTypeList.CoinsInstance,add));}
  internal static bool Pay(int value)=>Resource.removeInmaterialResource(new ResourceAmount(ResourceTypeList.CoinsInstance,value));
 }
 internal static class Defense
 {
  internal static void Tick(float dt)
  {
   var state=CommunityState.Current;if(!state.Enabled[0])return;
   var enemies=World.Characters.Where(c=>!c.isDead()&&!c.isDestroyed()&&c.getSpecialization()==SpecializationList.IntruderInstance&&c.getLocation()==Location.Exterior).ToArray();
   foreach(var radar in World.Modules.Where(m=>m.getModuleType() is ModuleTypeSurveillanceRadar&&m.isOperational()))foreach(var enemy in enemies)if((enemy.getPosition()-radar.getPosition()).sqrMagnitude<140*140&&state.Detected.Add(enemy.getId()))World.Toast("Radar: intruso detectado cerca de "+radar.getName()+".");
   if(World.Working<ModuleTypeSecurityCommand>()==null)return;
   foreach(var turret in World.Modules.Where(m=>m.getModuleType() is ModuleTypeDefenseTurret&&m.isOperational())){
    var progress=state.Machine(turret.getId());progress.Clock=Mathf.Max(0,progress.Clock-dt);if(progress.Clock>0)continue;
    var target=enemies.Where(c=>(c.getPosition()-turret.getPosition()).sqrMagnitude<50*50&&!Physics.Linecast(turret.getPosition()+Vector3.up*2.4f,c.getPosition()+Vector3.up,1<<8)).OrderBy(c=>(c.getPosition()-turret.getPosition()).sqrMagnitude).FirstOrDefault();
    if(target==null)continue;
    if(progress.Charges==0){if(!Stock.Consume(turret,"Spares",1)){progress.Clock=4;continue;}progress.Charges=20;}
    progress.Charges--;progress.Clock=4;progress.Target=target.getId();target.decayIndicator(CharacterIndicator.Health,.16f);
    var view=turret.getGameObject().GetComponent<TurretView>();if(view==null)view=turret.getGameObject().AddComponent<TurretView>();view.Target=target.getPosition();view.FlashUntil=Time.realtimeSinceStartup+.18f;
   }
  }
  internal static string AssignPatrol(Character c)
  {
   var state=CommunityState.Current;if(!state.Enabled[0]||World.Working<ModuleTypeSecurityCommand>()==null)return "Hace falta un centro de seguridad operativo.";
   if(!(c is Colonist)||!(c.getSpecialization() is Guard)||!World.Healthy(c)||c.isLoaded()||c.anyInteractions())return "Selecciona un guardia sano y disponible.";
   var post=World.Modules.Where(m=>m.getModuleType() is ModuleTypePatrolDepot&&m.isOperational()&&!state.Orders.Values.Any(o=>o.Mode=="patrol"&&o.Source==m.getId())).OrderBy(m=>(m.getPosition()-c.getPosition()).sqrMagnitude).FirstOrDefault();
   if(post==null)return "Construye un puesto de patrulla operativo y libre.";
   Orders.Cancel(c.getId());ManualControl.Release(c);state.Orders[c.getId()]=new Order{Unit=c.getId(),Mode="patrol",Source=post.getId()};return "Patrulla asignada; se conserva la respuesta nativa al combate.";
  }
  internal static string ToggleRaids()
  {
   var type=typeof(ManualControl).Assembly.GetType("PlanetbaseSolarFrontier.SolarState");var current=AccessTools.Property(type,"Current").GetValue(null);var field=AccessTools.Field(type,"Raids");bool enabled=!(bool)field.GetValue(current);field.SetValue(current,enabled);return "Incursiones de Frontera Solar: "+(enabled?"ACTIVADAS (un intruso cada 1800 s).":"desactivadas.");
  }
 }
 internal static class Exploration
 {
  internal static string Map()
  {
   var s=CommunityState.Current;if(!s.Enabled[2]||World.Equipment<SampleAnalyzer>()==null)return "Hace falta un centro de expediciones y analizador operativos.";
   if(s.SitesMapped)return "Las señales de esta colonia ya están cartografiadas.";
   var origin=World.Working<ModuleTypeExpeditionCenter>();if(origin==null)return "No hay centro de expediciones operativo.";
   float radius=World.Working<ModuleTypeExpeditionBeacon>()!=null?250:140;
   for(int i=0;i<64&&s.Sites.Count<6;i++){
    float angle=(i*137.5f+Singleton<PlanetManager>.getInstance().getCurrentPlanetIndex()*11)*Mathf.Deg2Rad;
    var p=origin.getPosition()+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*(radius+(i%3)*15);
    if(!PhysicsUtil.findFloor(p,out var ground)||!NavigationGraph.isExteriorLocationReachable(ground)||s.Sites.Any(x=>(x.Position-ground).sqrMagnitude<30*30)||World.Modules.Any(m=>(m.getPosition()-ground).sqrMagnitude<(m.getRadius()+8)*(m.getRadius()+8)))continue;
    var node=NavigationGraph.getExterior().findNearestNode(ground);if(node==null||(node.getPosition()-ground).sqrMagnitude>25)continue;ground=node.getPosition();
    s.Sites.Add(new Site{Id=s.Sites.Count,Kind=s.Sites.Count%3,Position=ground});
   }
   if(s.Sites.Count==0)return "No se encontraron señales en terreno alcanzable; prueba una baliza.";
   s.SitesMapped=true;SiteScene.Sync();return s.Sites.Count+" señales localizadas. Selecciona una unidad y envíala desde F10.";
  }
  internal static string Send(Character c,int siteId)
  {
   var s=CommunityState.Current;var site=s.Sites.Find(x=>x.Id==siteId&&!x.Done);var home=World.Working<ModuleTypeExpeditionCenter>();
   if(!s.Enabled[2]||site==null||home==null)return "La señal o el centro no están disponibles.";
   if(!World.Healthy(c)||c.isLoaded()||c.anyInteractions()||!(c is Colonist||c is Bot))return "Selecciona un colono o robot sano, libre y sin carga.";
   if(s.Orders.Values.Any(o=>o.Mode=="explore"&&o.Target==siteId))return "Otra unidad ya investiga esa señal.";
   Orders.Cancel(c.getId());ManualControl.Release(c);s.Orders[c.getId()]=new Order{Unit=c.getId(),Mode="explore",Source=home.getId(),Target=siteId};return "Expedición asignada: recorrido, exploración y regreso físico de la carga.";
  }
  internal static string Name(Site site)=>site.Kind==0?"Sonda averiada":site.Kind==1?"Suministros perdidos":"Vestigios de una expedición";
  internal static void Recover(Character c,Order order,Site site)
  {
   string[] rewards=site.Kind==0?new[]{"FieldSample","FieldSample","MachineParts"}:site.Kind==1?new[]{"Metal","Metal","SupplyCrate"}:new[]{"AncientRelic","FieldSample","FieldSample"};
   var made=new List<Resource>();try{
    foreach(string name in rewards){var type=TypeList<ResourceType,ResourceTypeList>.find(name);if(type==null)throw new InvalidOperationException("Recurso no registrado: "+name);made.Add(Resource.create(type,c.getPosition(),c.getLocation()));}
    foreach(var r in made){r.load();order.Cargo.Add(r.getId());}site.Done=true;order.Phase=1;order.Age=0;World.Toast(Name(site)+": carga recuperada; regresando al centro.");
   }catch{foreach(var r in made)if(!r.isDestroyed())r.destroy();order.Cargo.Clear();throw;}
  }
 }
 internal sealed class Contract
 {
  internal string Name,Resource;internal int Count,Reward;internal float Limit;
  internal Contract(string name,string resource,int count,int reward,float limit){Name=name;Resource=resource;Count=count;Reward=reward;Limit=limit;}
 }
 internal static class Commerce
 {
  internal static readonly Contract[] Contracts={new Contract("Suministros para una expedición","SupplyCrate",4,220,900),new Contract("Pedido industrial","MachineParts",5,200,1200),new Contract("Combustible para una estación","NuclearFuelRod",2,180,1500),new Contract("Colección de vestigios","AncientRelic",1,160,1800)};
  internal static bool Ready(out Module terminal,out Module dock){terminal=World.Modules.FirstOrDefault(m=>m.getModuleType() is ModuleTypeTradeTerminal&&m.isOperational()&&m.getGrid()!=null&&m.getComponents().Any(c=>c.getComponentType() is TradeConsole&&c.isOperational())&&World.Modules.Any(d=>d.getModuleType() is ModuleTypeExportDock&&d.isOperational()&&d.getGrid()==m.getGrid()));var found=terminal;dock=found==null?null:World.Modules.FirstOrDefault(d=>d.getModuleType() is ModuleTypeExportDock&&d.isOperational()&&d.getGrid()==found.getGrid());return terminal!=null&&dock!=null;}
  internal static string Accept(int index)
  {
   var s=CommunityState.Current;if(!s.Enabled[5]||!Ready(out _,out _))return "Necesitas terminal, consola y plataforma operativas en la misma red.";
   if(index<0||index>=Contracts.Length||s.Contract>=0||s.Import>=0||s.TradeCooldown>0)return "Ya hay una operación activa o la plataforma está en preparación.";
   if(!Stock.Pay(30))return "Faltan 30 créditos para el depósito.";
   s.Contract=index;s.Deadline=Contracts[index].Limit;s.ExportLoaded=false;s.Shipping=0;return "Contrato aceptado; depósito de 30 créditos. Entrega antes del plazo.";
  }
  internal static string Export()
  {
   var s=CommunityState.Current;if(!s.Enabled[5]||s.Contract<0||s.ExportLoaded||!Ready(out var terminal,out _))return "No hay contrato pendiente de entrega o falta servicio.";
   var c=Contracts[s.Contract];if(!Stock.Consume(terminal,c.Resource,c.Count))return "Faltan "+c.Count+" unidades libres de "+TypeList<ResourceType,ResourceTypeList>.find(c.Resource).getName()+" en la misma red.";
   s.ExportLoaded=true;s.Shipping=45;return "Carga entregada al servicio comercial. Expedición: 45 s con energía; pago al finalizar.";
  }
  internal static string Import(int kind)
  {
   var s=CommunityState.Current;if(kind<0||kind>2||!s.Enabled[5]||!Ready(out _,out _)||s.Contract>=0||s.Import>=0||s.TradeCooldown>0)return "La plataforma no está disponible.";
   int price=kind==0?100:kind==1?120:180;if(!Stock.Pay(price))return "Créditos insuficientes.";s.Import=kind;s.ImportTime=60;return "Importación pagada una sola vez; llegada en 60 s de servicio.";
  }
  internal static string Cancel(){var s=CommunityState.Current;if(s.ExportLoaded)return "La carga está en expedición; espera a la liquidación.";if(s.Contract<0)return "No hay contrato que cancelar.";s.Contract=-1;s.Deadline=0;return "Contrato cancelado; se pierde el depósito.";}
  internal static void Tick(float dt)
  {
   var s=CommunityState.Current;if(!s.Enabled[5])return;s.TradeCooldown=Mathf.Max(0,s.TradeCooldown-dt);
   if(s.Contract>=0){
    if(!s.ExportLoaded){s.Deadline=Mathf.Max(0,s.Deadline-dt);if(s.Deadline==0){s.Contract=-1;World.Toast("Contrato vencido: depósito perdido; tus materiales permanecen en la colonia.");}}
    else if(Ready(out _,out _)){s.Shipping=Mathf.Max(0,s.Shipping-dt);if(s.Shipping==0){int reward=Contracts[s.Contract].Reward+30;s.Contract=-1;s.ExportLoaded=false;s.ContractsDone++;s.TradeCooldown=120;Stock.Coins(reward);World.Toast("Exportación completada: "+reward+" créditos, incluido el depósito.");}}
   }
   if(s.Import>=0&&Ready(out _,out var dock)){s.ImportTime=Mathf.Max(0,s.ImportTime-dt);if(s.ImportTime==0){int kind=s.Import;var types=kind==0?new[]{"Metal","Metal","Metal","Metal"}:kind==1?new[]{"MedicalSupplies","MedicalSupplies","MedicalSupplies"}:new[]{"MachineParts","MachineParts","Semiconductors"};var made=new List<Resource>();try{for(int i=0;i<types.Length;i++){var r=Resource.create(TypeList<ResourceType,ResourceTypeList>.find(types[i]),dock.getPosition()+new Vector3((i%2)*1.4f-1,0,(i/2)*1.4f)+Vector3.right*(dock.getRadius()+1),Location.Exterior);made.Add(r);r.drop(Resource.State.Idle);}s.Import=-1;s.TradeCooldown=120;World.Toast("Importación recibida: recursos físicos junto a la plataforma.");}catch{foreach(var r in made)if(!r.isDestroyed())r.destroy();throw;}}}
  }
 }
}
