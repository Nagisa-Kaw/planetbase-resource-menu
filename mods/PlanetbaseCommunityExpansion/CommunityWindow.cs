using System;
using System.Linq;
using HarmonyLib;
using Planetbase;
using UnityEngine;
using Module=Planetbase.Module;
namespace PlanetbaseCommunityExpansion
{
 internal sealed class CommunityWindow:GuiWindow
 {
  static int lastPage;readonly GameGui gui;readonly int page;readonly Selectable selected;
  readonly GuiLabelItem summary=new GuiLabelItem("");readonly GuiLabelItem status=new GuiLabelItem("");
  static readonly string[] Names={"Defensa","Logística","Expedición","Rescate","Vida colonial","Comercio"};
  internal CommunityWindow(GameGui gui,int page=-1):base(new GuiLabelItem("COLONIAS EN EXPANSIÓN · ALFA 0.1.0 · F10"))
  {
   this.gui=gui;this.page=page<0?lastPage:Mathf.Clamp(page,0,5);lastPage=this.page;selected=Selection.getSelected();
   var tabs=new GuiRowItem(6);tabs.setHeight(GuiStyles.getIconSizeMedium());for(int i=0;i<6;i++){int p=i;tabs.addChild(Action(Names[i],()=>gui.setWindow(new CommunityWindow(gui,p))));}mRootItem.addChild(tabs);mRootItem.addChild(summary);
   Add("Seleccionado: "+(selected?.getName()??"ninguno")+" · selecciona una unidad o edificio antes de F10.");
   var s=CommunityState.Current;
   if(this.page==0){
    Add("Radar: 140 m · torre: 50 m · 4 s entre disparos · 1 repuesto / 20 disparos.");
    Button("Asignar al guardia seleccionado una patrulla",()=>Defense.AssignPatrol(selected as Character));
    Button("Cancelar la tarea de la unidad seleccionada",CancelSelected);
    Button("Incursiones de Frontera Solar: activar / desactivar",Defense.ToggleRaids);
    Add("Las incursiones son opcionales. Las torres necesitan un centro de seguridad operativo.");
   }else if(this.page==1){
    Add("Origen: "+(World.Module(s.Source)?.getName()??"sin elegir")+" · destino: "+(World.Module(s.Destination)?.getName()??"sin elegir"));
    Button("Usar el almacén seleccionado como origen",()=>SetWarehouse(true));Button("Usar el almacén seleccionado como destino",()=>SetWarehouse(false));
    var types=Orders.HaulTypes();int index=Mathf.Clamp(s.ResourceIndex,0,types.Length-1);
    Button("Material de la ruta: "+types[index].getName()+" (cambiar)",()=>{s.ResourceIndex=(index+1)%types.Length;gui.setWindow(new CommunityWindow(gui,1));return "";});
    Button("Iniciar ruta con el transportador pesado seleccionado",()=>Orders.Route(selected as Character));Button("Cancelar la tarea seleccionada y soltar sus cargas",CancelSelected);
    Button("Prioridad general: "+new[]{"normal","combustible","medicinas","alimentos"}[s.Priority]+" (cambiar)",()=>{s.Priority=(s.Priority+1)%4;gui.setWindow(new CommunityWindow(gui,1));return "";});
    Add("Tres cajas físicas por viaje. El origen y el destino deben ser almacenes de la misma red.");
   }else if(this.page==2){
    Button("Cartografiar señales de esta colonia",()=>{string result=Exploration.Map();gui.setWindow(new CommunityWindow(gui,2));World.Toast(result);return result;});
    foreach(var site in s.Sites){int id=site.Id;Button((site.Done?"RECUPERADA · ":"")+Exploration.Name(site)+" ["+site.Position.x.ToString("0")+", "+site.Position.z.ToString("0")+"]",()=>Exploration.Send(selected as Character,id));}
    if(s.Sites.Count==0)Add("Construye centro y analizador; una baliza operativa amplía el radio al cartografiar.");
    Button("Cancelar expedición o tarea seleccionada",CancelSelected);Add("La unidad recorre el terreno, explora 10 s y trae tres cargas al centro.");
   }else if(this.page==3){
    Button(s.Emergency?"Finalizar emergencia y evacuación":"Declarar emergencia y evacuar civiles",Orders.Emergency);
    Button("Cancelar la tarea de la unidad seleccionada",CancelSelected);
    Add("Las ambulancias fabricadas buscan heridos exteriores a 180 m de un hangar operativo.");
    Add("Requieren un hospital con una cama de trauma disponible. La atención usa médicos y medicinas nativos.");
    Add("Los civiles sanos se evacúan por corredores y esclusas al refugio. Los guardias mantienen su función.");
    Add("Interrumpir un rescate deja al paciente junto al vehículo; no lo traslada instantáneamente.");
   }else if(this.page==4){
    Add("Formación: 2 informes + 20 créditos · 300 s de estudio en una academia operativa.");
    foreach(string p in new[]{"Worker","Biologist","Engineer","Medic","Guard"}){string profession=p;Button("Formar como "+TypeList<Specialization,SpecializationList>.find(p).getName(),()=>Orders.Train(selected as Character,profession));}
    Button("Cancelar formación (se pierde el coste pagado)",CancelSelected);
    Add("Las camas privadas recuperan descanso y moral; la cafetería utiliza comida y bebidas nativas.");
   }else{
    Add(s.Contract<0?"Sin contrato activo · preparación: "+s.TradeCooldown.ToString("0")+" s":Commerce.Contracts[s.Contract].Name+" · "+(s.ExportLoaded?"en expedición: "+s.Shipping.ToString("0")+" s":"plazo: "+s.Deadline.ToString("0")+" s"));
    for(int i=0;i<Commerce.Contracts.Length;i++){int k=i;var c=Commerce.Contracts[i];Button(c.Name+": "+c.Count+" × "+TypeList<ResourceType,ResourceTypeList>.find(c.Resource).getName()+" → "+c.Reward+" créditos",()=>Commerce.Accept(k));}
    Button("Entregar el inventario del contrato a la expedición",Commerce.Export);Button("Cancelar contrato sin entregar (pierde depósito)",Commerce.Cancel);
    var row=new GuiRowItem(3);row.setHeight(GuiStyles.getIconSizeMedium());row.addChild(Action("Importar 4 metales · 100",()=>Result(Commerce.Import(0))));row.addChild(Action("3 medicinas · 120",()=>Result(Commerce.Import(1))));row.addChild(Action("2 piezas + 1 semiconductor · 180",()=>Result(Commerce.Import(2))));mRootItem.addChild(row);
    Add("Depósito: 30 créditos. La entrega consume recursos libres de la misma red; el pago devuelve el depósito.");
   }
   Button("Automatización de "+Names[this.page]+": "+(s.Enabled[this.page]?"activada":"desactivada")+" (cambiar)",()=>{bool next=!s.Enabled[this.page];if(!next){if(this.page==3&&s.Emergency)Orders.Emergency();foreach(var o in s.Orders.Values.Where(o=>o.Feature==this.page).ToArray())Orders.Cancel(o.Unit);}s.Enabled[this.page]=next;gui.setWindow(new CommunityWindow(gui,this.page));return "Se conservan los edificios; las tareas canceladas dejan sus cargas físicamente.";});
   mRootItem.addChild(status);mRootItem.addChild(new GuiButtonItem("Cerrar",_=>gui.setWindow(null)));
  }
  void Add(string text)=>mRootItem.addChild(new GuiLabelItem(text));
  void Button(string text,Func<string> callback)=>mRootItem.addChild(Action(text,()=>Result(callback())));
  void Result(string text){status.setText(text);}
  string CancelSelected(){if(!(selected is Character c))return "Selecciona una unidad.";Orders.Cancel(c.getId());return "Tarea cancelada; las cargas y pacientes permanecen en el mundo.";}
  string SetWarehouse(bool source){if(!(selected is Module m)||!m.hasFlag(ModuleType.FlagStorage)||!m.isBuilt())return "Selecciona un almacén construido.";if(source)CommunityState.Current.Source=m.getId();else CommunityState.Current.Destination=m.getId();return (source?"Origen":"Destino")+" guardado: "+m.getName()+". Cierra F10 para elegir el siguiente objeto.";}
  static GuiLabelItem Action(string text,Action callback){var row=new GuiLabelItem(text,ResourceList.getInstance().Icons.Tech,text);row.setHeight(GuiStyles.getIconSizeMedium());row.addCallback(_=>callback(),null);return row;}
  public override float getWidth()=>Mathf.Min(Screen.width*.94f,Screen.height*1.5f);
  public override Exclusivity getExclusivity()=>Exclusivity.Exclusive;
  public override void update(){var s=CommunityState.Current;summary.setText(Names[page]+" · tareas: "+s.Orders.Count+" · contratos completados: "+s.ContractsDone+(s.Emergency?" · EVACUACIÓN ACTIVA":""));}
  [HarmonyPatch(typeof(GuiMenuSystem),nameof(GuiMenuSystem.init))]
  static class Management{static void Postfix(GameStateGame __0,GuiMenu ___mMenuBaseManagement){var gui=(GameGui)AccessTools.Field(typeof(GameStateGame),"mGameGui").GetValue(__0);___mMenuBaseManagement.addItem(new GuiMenuItem(ResourceList.getInstance().Icons.Tech,"Colonias en expansión (F10)",_=>gui.setWindow(new CommunityWindow(gui)),null,0));}}
  [HarmonyPatch(typeof(GameStateGame),nameof(GameStateGame.update))]
  static class Input{static bool Prefix(GameGui ___mGameGui){if(!UnityEngine.Input.GetKeyDown(KeyCode.F10))return true;if(___mGameGui.getWindow() is CommunityWindow)___mGameGui.setWindow(null);else if(!___mGameGui.isWindowVisible())___mGameGui.setWindow(new CommunityWindow(___mGameGui));return false;}}
 }
 internal sealed class PriorityDelivery:AiRule
 {
  public override bool update(Character c){var s=CommunityState.Current;if(s==null||!s.Enabled[1]||s.Priority==0||World.Working<ModuleTypeLogisticsHub>()==null||Orders.Owns(c)||!hasCarryPriority(c)||!canWork(c))return false;
   string[] types=s.Priority==1?new[]{"NuclearFuelRod","FusionCapsule"}:s.Priority==2?new[]{"MedicalSupplies"}:new[]{"Meal","Vegetables","Vitromeat"};
   foreach(string name in types){var type=TypeList<ResourceType,ResourceTypeList>.find(name);if(type==null)continue;var store=Module.findStorageComponent(c,type);if(store==null)continue;var resource=Resource.findStorable(c,type,true);if(resource!=null&&goTarget(c,resource,store))return true;}return false;}
  [HarmonyPatch(typeof(AiRuleGoLoadResourcesToStore),nameof(AiRuleGoLoadResourcesToStore.update))]
  static class Apply{static readonly PriorityDelivery Rule=new PriorityDelivery();[HarmonyPriority(HarmonyLib.Priority.First)]static bool Prefix(Character character,ref bool __result){if(!Rule.update(character))return true;__result=true;return false;}}
 }
}
