using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Planetbase;
using PlanetbaseVisuals;
using UnityEngine;
using Module=Planetbase.Module;
namespace PlanetbaseCommunityExpansion
{
 internal static class CommunityModels
 {
  internal static readonly Color Blue=new Color(.16f,.48f,.8f),Medical=new Color(.2f,.8f,.63f),Purple=new Color(.61f,.36f,.82f);
  internal static void Building(GameObject model,ModuleType type,int size)
  {
   string id=type.GetType().Name.Replace("ModuleType","");
   if(!type.isExterior()){
    RoomStyle style=type is ModuleTypeLogisticsHub||type is ModuleTypeFreightWarehouse?RoomStyle.Assembly:type is ModuleTypeEmergencyHospital?RoomStyle.Treatment:type is ModuleTypeEmergencyShelter?RoomStyle.Shelter:type is ModuleTypePrivateResidence?RoomStyle.Wellness:type is ModuleTypeCommunityCafe?RoomStyle.Arcade:type is ModuleTypeTradeTerminal?RoomStyle.Casino:RoomStyle.Research;
    InteriorArchitecture.Build(model,size,style);
    var roof=model.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Room roof architecture");
    var sign=new Architecture(roof,"Community room identification");float r=Module.ValidSizes[size]*.5f;
    sign.Box(new Vector3(0,1.65f,-r*.91f),new Vector3(2,.27f,.12f),Architecture.Dark,"Service designation");
    sign.Text(Catalog.All[id].Group.ToUpperInvariant(),new Vector3(0,1.65f,-r*.99f),.036f,Architecture.Cyan,Vector3.zero);sign.Finish();roof.GetComponent<ColonyShellGeometry>().Surfaces=roof.GetComponentsInChildren<MeshFilter>(true);roof.GetComponent<ColonyRoofVisibility>().Surfaces=roof.GetComponentsInChildren<Renderer>(true);return;
   }
   Architecture.HideOriginalMeshes(model);var b=new Architecture(model.transform,"Community exterior architecture");float radius=type is ModuleTypeExportDock?6.1f:3.5f;b.Foundation(radius);
   if(type is ModuleTypeSurveillanceRadar){
    b.Cylinder(new Vector3(0,.62f,0),1.5f,.6f,Architecture.Shell,"Radar equipment shelter",8);b.Cylinder(new Vector3(0,2.0f,0),.22f,2.2f,Architecture.Edge,"Radar support",16);
    var pivot=Pivot(model.transform,"Radar array",new Vector3(0,3.1f,0));var p=new Architecture(pivot,"Phased radar array");
    p.Box(Vector3.zero,new Vector3(3.4f,1.2f,.2f),Architecture.Dark,"Radar panel");for(int x=-4;x<=4;x++)for(int y=-1;y<=1;y++)p.Box(new Vector3(x*.34f,y*.32f,.13f),new Vector3(.22f,.2f,.035f),Blue,"Radar emitter");p.Box(new Vector3(0,0,-.19f),new Vector3(3.5f,.16f,.14f),Architecture.Orange);p.Finish();var motion=pivot.gameObject.AddComponent<MachineryMotion>();motion.Spin=12;
    b.Beacon(new Vector3(1.1f,1.0f,0),Blue);
   }else if(type is ModuleTypeDefenseTurret){
    b.Cylinder(new Vector3(0,.62f,0),1.7f,.65f,Architecture.Dark,"Armored turret base",8);b.Cylinder(new Vector3(0,1.35f,0),.68f,.9f,Architecture.Shell,"Turret pedestal",12);b.Ring(new Vector3(0,1.77f,0),.7f,.075f,Architecture.Orange);
    var pivot=Pivot(model.transform,"Turret yaw",new Vector3(0,2.05f,0));var p=new Architecture(pivot,"Defensive turret head");p.Box(Vector3.zero,new Vector3(1.65f,.65f,1.1f),Architecture.Shell,"Turret armor");foreach(int x in new[]{-1,1}){p.Cylinder(new Vector3(x*.42f,0,.85f),.12f,1.6f,Architecture.Dark,"Emitter barrel",16).localRotation=Quaternion.Euler(90,0,0);p.Ring(new Vector3(x*.42f,0,1.58f),.12f,.025f,Architecture.Cyan,"Emitter aperture",true,16).localRotation=Quaternion.Euler(90,0,0);}p.Box(new Vector3(0,.38f,.24f),new Vector3(.4f,.2f,.5f),Blue,"Turret range finder");p.Finish();b.Hazard(new Vector3(0,.65f,1.62f),8,180);
   }else if(type is ModuleTypeExpeditionBeacon){
    b.Box(new Vector3(0,.7f,0),new Vector3(1.4f,.8f,1.4f),Architecture.Shell,"Field beacon base");b.Cylinder(new Vector3(0,2.6f,0),.14f,3.2f,Architecture.Edge,"Beacon mast",12);b.Ring(new Vector3(0,3.5f,0),.72f,.05f,Purple,"Beacon navigation ring",true,32);b.Box(new Vector3(0,4.25f,0),new Vector3(.3f,.25f,.3f),Purple,"Beacon light",true);foreach(int x in new[]{-1,1})b.Box(new Vector3(x*1.22f,.92f,0),new Vector3(1.4f,.08f,1.7f),Blue,"Beacon solar reserve").localRotation=Quaternion.Euler(0,0,x*18);
   }else if(type is ModuleTypeExportDock){
    for(int i=-2;i<=2;i++){b.Box(new Vector3(i*1.7f,.4f,0),new Vector3(.12f,.035f,9),Architecture.Edge,"Deck panel seam");b.Box(new Vector3(0,.4f,i*1.7f),new Vector3(9,.035f,.12f),Architecture.Edge);}
    b.Box(new Vector3(0,.43f,0),new Vector3(3,.035f,.2f),Architecture.Yellow);foreach(int x in new[]{-1,1})b.Box(new Vector3(x*1.4f,.43f,0),new Vector3(.2f,.035f,3.4f),Architecture.Yellow);
    foreach(int x in new[]{-1,1})for(int i=-2;i<=2;i++)b.Box(new Vector3(x*5.3f,.45f,i*1.8f),new Vector3(.25f,.12f,.5f),Medical,"Landing guidance",true);
    b.Box(new Vector3(4.4f,1,4.1f),new Vector3(1.2f,1.4f,1.0f),Architecture.Shell,"Trade service cabinet");b.Vents(new Vector3(4.4f,1,3.56f),.8f,.7f,5);b.Beacon(new Vector3(4.4f,1.8f,4.1f),Medical);
    var ship=Ship(model.transform);ship.name="Trade shuttle";ship.SetActive(false);model.AddComponent<DockView>().Ship=ship.transform;
   }else{
    bool rescue=type is ModuleTypeRescueHangar;Color color=rescue?Medical:Blue;
    b.Box(new Vector3(0,1.35f,-.5f),new Vector3(5.2f,2.0f,3.7f),Architecture.Shell,"Vehicle service shelter");
    b.Box(new Vector3(0,1.3f,1.4f),new Vector3(4.7f,1.7f,.15f),Architecture.Dark,"Hangar entrance");b.Box(new Vector3(0,2.43f,-.5f),new Vector3(5.3f,.22f,3.8f),color,"Hangar roof fascia");
    foreach(int x in new[]{-1,1}){b.Box(new Vector3(x*2.4f,1.2f,1.52f),new Vector3(.12f,1.7f,.08f),color,"Door guidance",true);b.Vents(new Vector3(x*1.4f,1.3f,-2.4f),1,.65f,4);}
    b.Text(rescue?"RESCATE":"PATRULLA",new Vector3(0,2.43f,1.48f),.065f,Architecture.Dark,new Vector3(0,180,0));b.Beacon(new Vector3(1.8f,2.7f,0),color);
    var parked=Vehicle(model.transform,rescue?"RescueRover":"PatrolCar");parked.transform.localPosition=new Vector3(0,.32f,.7f);parked.transform.localScale=Vector3.one*.7f;
   }
   b.Finish();
  }
  static Transform Pivot(Transform parent,string name,Vector3 position){var g=new GameObject(name);g.layer=11;g.transform.SetParent(parent,false);g.transform.localPosition=position;return g.transform;}
  internal static GameObject Vehicle(Transform parent,string kind)
  {
   return VisualAssets.Clone("community vehicle "+kind,parent,t=>{
    string template=kind=="FreightHauler"?"CargoTruck":kind=="ExpeditionRover"?"ScannerBot":"ColonyRover";var baseModel=VehicleArchitecture.Create(t,template);if(template=="ColonyRover"){var motion=baseModel.AddComponent<VehicleMotion>();motion.Wheels=Enumerable.Range(0,4).Select(i=>baseModel.transform.Find("Wheel "+i)).ToArray();}
    var b=new Architecture(t,"Community vehicle equipment");Color color=kind=="RescueRover"?Medical:kind=="ExpeditionRover"?Purple:Blue;
    if(kind=="RescueRover"){
     b.Box(new Vector3(0,1.12f,-.55f),new Vector3(1.48f,.8f,1.7f),Architecture.Shell,"Pressurized rescue compartment");b.Box(new Vector3(0,1.56f,-.55f),new Vector3(1.52f,.14f,1.78f),color,"Rescue compartment roof");
     foreach(int x in new[]{-1,1}){b.Box(new Vector3(x*.76f,1.18f,-.55f),new Vector3(.025f,.12f,.55f),color,"Medical sign",true);b.Box(new Vector3(x*.76f,1.18f,-.55f),new Vector3(.025f,.42f,.12f),color,"Medical sign",true);}b.Box(new Vector3(0,1.74f,-.4f),new Vector3(.65f,.18f,.2f),color,"Rescue light bar",true);
    }else if(kind=="FreightHauler"){
     b.Beam(new Vector3(-.8f,1.1f,-1.45f),new Vector3(-.8f,1.5f,-1.45f),.08f,Blue);b.Beam(new Vector3(.8f,1.1f,-1.45f),new Vector3(.8f,1.5f,-1.45f),.08f,Blue);b.Box(new Vector3(0,1.5f,-1.45f),new Vector3(1.7f,.13f,.12f),Blue,"Freight identification");
    }else if(kind=="ExpeditionRover"){
     b.Box(new Vector3(0,1.1f,-.48f),new Vector3(.9f,.24f,.35f),Purple,"Sample rack");b.Cylinder(new Vector3(.35f,1.65f,-.25f),.035f,.7f,Architecture.Shell,"Survey antenna",8);b.Ring(new Vector3(.35f,1.97f,-.25f),.17f,.025f,Purple,"Survey antenna ring",true,16);
    }else{b.Box(new Vector3(0,1.83f,.02f),new Vector3(1.15f,.13f,.18f),Blue,"Patrol light bar",true);b.Box(new Vector3(0,1.22f,1.12f),new Vector3(.8f,.1f,.07f),Blue,"Security marking");}
    b.Finish();
   });
  }
  internal static void Equipment(GameObject model,string id)
  {
   Architecture.HideOriginalMeshes(model);var anchors=model.GetComponentsInChildren<Transform>(true).Where(t=>t.CompareTag("AnchorPoint")).Select(t=>new SimpleTransform(model.transform.InverseTransformPoint(t.position),Quaternion.Inverse(model.transform.rotation)*t.rotation)).ToArray();
   VisualAssets.Clone("community equipment "+id,model.transform,t=>{
    var b=new Architecture(t,"Community equipment body");Color color=id=="TraumaBed"||id=="SupplyPacker"?Medical:id=="SampleAnalyzer"||id=="TrainingConsole"?Purple:id=="TradeConsole"?Architecture.Yellow:id=="LogisticsConsole"?Architecture.Orange:Blue;
    if(id=="PrivateBunk"||id=="TraumaBed"){
     b.Box(new Vector3(0,.2f,0),new Vector3(2.15f,.32f,1.12f),Architecture.Shell,"Bed frame");b.Box(new Vector3(0,.45f,0),new Vector3(1.95f,.2f,1.02f),id=="TraumaBed"?Medical:Architecture.Edge,"Bed mattress");b.Box(new Vector3(-.7f,.58f,0),new Vector3(.4f,.13f,.85f),Architecture.Shell,"Bed pillow");
     foreach(int z in new[]{-1,1})b.Box(new Vector3(0,.55f,z*.59f),new Vector3(1.9f,.1f,.08f),Architecture.Edge,"Bed side rail");
     b.Box(new Vector3(-1.05f,.72f,0),new Vector3(.12f,.95f,1.2f),Architecture.Shell,"Headboard");
     if(id=="TraumaBed"){b.Screen(new Vector3(-1.1f,1.2f,-.2f),new Vector2(.42f,.3f),Medical,90);b.Cylinder(new Vector3(.8f,.28f,-.5f),.1f,.35f,Architecture.Dark,"Medical bed support",12);}
     else b.Box(new Vector3(0,.8f,-.66f),new Vector3(2.2f,1.3f,.09f),Architecture.Edge,"Privacy partition");
    }else if(id=="CafeSeat"){
     foreach(var a in anchors){var p=a.getPosition();p.y=.43f;b.Box(p,new Vector3(.9f,.17f,.6f),Architecture.Edge,"Cafe cushion").localRotation=a.getRotation();b.Box(p+a.getRotation()*new Vector3(0,.25f,-.3f),new Vector3(.9f,.55f,.12f),Architecture.Shell,"Cafe backrest").localRotation=a.getRotation();b.Cylinder(new Vector3(p.x,.2f,p.z),.12f,.3f,Architecture.Steel,"Cafe seat pedestal",16);}
    }else if(id=="VehicleFabricator"||id=="SupplyPacker"){
     b.Box(new Vector3(0,.5f,0),new Vector3(1.65f,.8f,1.1f),Architecture.Shell,"Fabrication base");b.Box(new Vector3(0,.96f,0),new Vector3(1.8f,.13f,1.25f),Architecture.Dark,"Fabrication work surface");
     foreach(int x in new[]{-1,1})b.Beam(new Vector3(x*.78f,1,-.4f),new Vector3(x*.78f,2.1f,-.4f),.15f,Architecture.Edge);b.Box(new Vector3(0,2.1f,-.4f),new Vector3(1.8f,.22f,.3f),color,"Assembly gantry");
     b.Beam(new Vector3(.3f,2,-.4f),new Vector3(.3f,1.4f,.1f),.1f,Architecture.Shell);b.Box(new Vector3(0,1.15f,.15f),new Vector3(.7f,.25f,.6f),Architecture.Orange,"Assembly fixture");if(id=="VehicleFabricator")b.Cylinder(new Vector3(0,1.44f,.15f),.17f,.27f,Architecture.Shell,"Robot drive assembly",12);else b.Box(new Vector3(0,1.41f,.15f),new Vector3(.55f,.25f,.42f),Medical,"Packed cargo");b.Text(id=="VehicleFabricator"?"FLOTA":"EMPAQUE",new Vector3(0,.6f,.58f),.034f,color,new Vector3(0,180,0));
    }else if(id=="SampleAnalyzer"){
     b.Box(new Vector3(0,.55f,0),new Vector3(1.5f,.95f,1.1f),Architecture.Shell,"Sample analysis bench");for(int i=-1;i<=1;i++){b.Cylinder(new Vector3(i*.35f,1.35f,-.1f),.13f,.6f,Architecture.Edge,"Sample tube",16);b.Ring(new Vector3(i*.35f,1.65f,-.1f),.14f,.025f,Purple,"Sample scan ring",true,16);}b.Screen(new Vector3(0,1.12f,.6f),new Vector2(.65f,.35f),Purple);
    }else{
     b.Box(new Vector3(0,.7f,0),new Vector3(1.55f,1.25f,.75f),Architecture.Shell,"Management console");b.Box(new Vector3(0,1.36f,0),new Vector3(1.65f,.1f,.9f),color,"Console work surface");b.Screen(new Vector3(0,1.65f,-.15f),new Vector2(.95f,.5f),color);b.Box(new Vector3(0,1.44f,.26f),new Vector3(.9f,.055f,.3f),Architecture.Dark,"Console keyboard");
     string label=id=="SecurityDispatch"?"SEGURIDAD":id=="LogisticsConsole"?"LOGÍSTICA":id=="TrainingConsole"?"ACADEMIA":"COMERCIO";b.Text(label,new Vector3(0,1.02f,.4f),.027f,color,new Vector3(0,180,0));
     foreach(var a in anchors){var p=a.getPosition();p.y=.44f;b.Box(p,new Vector3(.65f,.15f,.6f),Architecture.Dark,"Operator seat").localRotation=a.getRotation();b.Box(p+a.getRotation()*new Vector3(0,.25f,-.3f),new Vector3(.65f,.55f,.12f),Architecture.Dark,"Operator backrest").localRotation=a.getRotation();b.Cylinder(new Vector3(p.x,.2f,p.z),.12f,.35f,Architecture.Edge,"Operator chair pedestal",12);}
    }
    b.Finish();
   });
  }
  internal static void Resource(GameObject model,string id)
  {
   Architecture.HideOriginalMeshes(model);VisualAssets.Clone("community cargo "+id,model.transform,t=>{var b=new Architecture(t,"Community cargo geometry");
    if(id=="FieldSample"){b.Box(new Vector3(0,.08f,0),new Vector3(.76f,.14f,.65f),Architecture.Dark);for(int i=-1;i<=1;i++){b.Cylinder(new Vector3(i*.23f,.27f,0),.085f,.3f,Architecture.Edge,"Field sample vessel",16);b.Cylinder(new Vector3(i*.23f,.44f,0),.095f,.06f,Purple,"Sample cap",16);}}
    else if(id=="AncientRelic"){b.Box(new Vector3(0,.07f,0),new Vector3(.68f,.12f,.68f),Architecture.Dark);b.Cylinder(new Vector3(0,.3f,0),.2f,.34f,Purple,"Recovered prism",5);b.Ring(new Vector3(0,.32f,0),.24f,.025f,Architecture.Cyan,"Survey retention ring",true,20);}
    else{b.Box(new Vector3(0,.24f,0),new Vector3(.8f,.46f,.7f),Architecture.Shell,"Supply crate");foreach(int x in new[]{-1,1})b.Box(new Vector3(x*.25f,.25f,0),new Vector3(.1f,.48f,.73f),Medical,"Supply strap");b.Box(new Vector3(0,.49f,0),new Vector3(.35f,.04f,.25f),Architecture.Dark,"Supply label");}b.Finish();});
  }
  static GameObject Ship(Transform parent)
  {
   var root=Pivot(parent,"Trade shuttle",Vector3.up*.8f);var b=new Architecture(root,"Trade shuttle hull");b.Wedge(Vector3.zero,2.7f,new[]{new Vector2(-2.2f,0),new Vector2(-2.2f,1),new Vector2(.8f,1.2f),new Vector2(2.5f,.35f),new Vector2(2.5f,0)},Architecture.Shell,"Freighter hull");b.Box(new Vector3(0,.8f,1.25f),new Vector3(1.55f,.35f,.1f),Architecture.Dark,"Shuttle cockpit").localRotation=Quaternion.Euler(32,0,0);foreach(int x in new[]{-1,1}){b.Box(new Vector3(x*1.9f,.2f,-.4f),new Vector3(1.4f,.15f,2.5f),Blue,"Freighter wing");b.Cylinder(new Vector3(x*.85f,.5f,-2.3f),.38f,.8f,Architecture.Dark,"Freighter engine",20).localRotation=Quaternion.Euler(90,0,0);b.Ring(new Vector3(x*.85f,.5f,-2.72f),.27f,.06f,Architecture.Cyan,"Engine glow",true,20).localRotation=Quaternion.Euler(90,0,0);b.Beam(new Vector3(x*.9f,0,1),new Vector3(x*1.2f,-.6f,1),.15f,Architecture.Edge);}b.Finish();return root.gameObject;
  }
 }
 internal sealed class MachineryMotion:MonoBehaviour{public float Spin;void Update(){if(World.Game!=null&&!Singleton<TimeManager>.getInstance().isPaused())transform.Rotate(Vector3.up,Spin*Time.deltaTime,Space.Self);}}
 internal sealed class TurretView:MonoBehaviour
 {
  internal Vector3 Target;internal float FlashUntil;Transform head;LineRenderer beam;
  void LateUpdate(){if(head==null)head=GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="Turret yaw");if(head!=null&&Target!=Vector3.zero){var d=Target-head.position;d.y=0;if(d.sqrMagnitude>.1f)head.rotation=Quaternion.LookRotation(d);}
   if(beam==null){var g=new GameObject("Defensive pulse");g.transform.SetParent(transform,false);beam=g.AddComponent<LineRenderer>();beam.material=new Material(Shader.Find("Sprites/Default"));beam.startColor=beam.endColor=Color.cyan;beam.startWidth=.055f;beam.endWidth=.025f;beam.positionCount=2;}
   beam.enabled=Time.realtimeSinceStartup<FlashUntil;if(beam.enabled){beam.SetPosition(0,head!=null?head.position+head.forward*1.5f:transform.position+Vector3.up*2);beam.SetPosition(1,Target+Vector3.up);}}
  void OnDestroy(){if(beam!=null&&beam.sharedMaterial!=null)Destroy(beam.sharedMaterial);}
 }
 internal sealed class DockView:MonoBehaviour
 {
  public Transform Ship;
  void LateUpdate(){if(Ship==null||CommunityState.Current==null)return;var owner=transform.parent==null?null:Construction.find(transform.parent.gameObject) as Module;if(owner==null){Ship.gameObject.SetActive(false);return;}var s=CommunityState.Current;bool active=s.Enabled[5]&&(s.ExportLoaded||s.Import>=0)&&World.Working<ModuleTypeExportDock>()==owner;Ship.gameObject.SetActive(active);if(active){float elapsed=s.Import>=0?60-s.ImportTime:45-s.Shipping;float duration=s.Import>=0?60:45;float y=elapsed<8?Mathf.Lerp(18,.8f,elapsed/8):elapsed>duration-8?Mathf.Lerp(.8f,18,(elapsed-duration+8)/8):.8f;Ship.localPosition=Vector3.up*y;}}
 }
 internal sealed class OrderView:MonoBehaviour
 {
  internal Character Owner;GameObject vehicle;Transform body;Vector3 original;
  void LateUpdate(){if(Owner==null||Owner.isDestroyed())return;Orders.SyncCargo(Owner);bool driving=CommunityState.Current?.Orders.TryGetValue(Owner.getId(),out var o)==true&&o.Mode=="patrol"&&Owner.getLocation()==Location.Exterior&&!Owner.isFighting();if(driving&&vehicle==null)vehicle=CommunityModels.Vehicle(transform,"PatrolCar");if(vehicle!=null)vehicle.SetActive(driving);if(Owner is Colonist){var current=Owner.getSelectionModel().transform;if(current!=body){body=current;original=body.localPosition;}body.localPosition=driving?original+new Vector3(0,.38f,.3f):original;}}
 }
 [HarmonyPatch]
 internal static class PatrolPose
 {
  static IEnumerable<System.Reflection.MethodBase> TargetMethods(){yield return AccessTools.Method(typeof(Character),"getWalkAnimationType");yield return AccessTools.Method(typeof(Character),"getIdleAnimationType");}
  static bool Prefix(Character __instance,ref CharacterAnimationType __result){if(CommunityState.Current?.Orders.TryGetValue(__instance.getId(),out var o)!=true||o.Mode!="patrol"||__instance.getLocation()!=Location.Exterior)return true;__result=CharacterAnimationType.SitIdle;return false;}
 }
 internal sealed class SiteScene:MonoBehaviour
 {
  static SiteScene current;GameStateGame game;readonly Dictionary<int,GameObject> objects=new Dictionary<int,GameObject>();
  internal static void Sync(){if(World.Game==null||CommunityState.Current==null)return;if(current==null||current.game!=World.Game){var go=new GameObject("Community expedition signals");current=go.AddComponent<SiteScene>();current.game=World.Game;}current.Refresh();}
  void Refresh(){var s=CommunityState.Current;if(s==null)return;foreach(var site in s.Sites){if(!objects.TryGetValue(site.Id,out var go)||go==null){go=new GameObject("Expedition signal "+site.Id);go.transform.SetParent(transform,false);go.transform.position=site.Position;BuildSite(go.transform,site.Kind);objects[site.Id]=go;}go.SetActive(!site.Done&&s.Enabled[2]);}}
  internal static void BuildSite(Transform parent,int kind){var b=new Architecture(parent,"Expedition discovery");if(kind==0){b.Cylinder(new Vector3(0,.45f,0),.8f,.7f,Architecture.Shell,"Abandoned probe",8);b.Beam(new Vector3(0,.8f,0),new Vector3(.8f,1.8f,0),.08f,Architecture.Edge);foreach(int x in new[]{-1,1})b.Box(new Vector3(x*1.3f,.4f,0),new Vector3(1.5f,.08f,1),CommunityModels.Blue,"Probe panel").localRotation=Quaternion.Euler(0,0,x*16);}else if(kind==1){for(int i=0;i<3;i++)b.Box(new Vector3((i-1)*.9f,.35f,(i%2)*.7f),new Vector3(.8f,.7f,.8f),i==1?Architecture.Orange:Architecture.Shell,"Lost cargo");}else{for(int i=0;i<4;i++){float a=i*Mathf.PI/2;var p=new Vector3(Mathf.Cos(a)*1.1f,.6f,Mathf.Sin(a)*1.1f);b.Cylinder(p,.3f,1.2f,CommunityModels.Purple,"Abandoned survey monument",5);}b.Ring(new Vector3(0,.18f,0),1.25f,.055f,Architecture.Edge,"Old survey marker",false,32);}b.Beacon(new Vector3(1.7f,.2f,0),CommunityModels.Purple);b.Finish();}
  void Update(){if(World.Game!=game){Destroy(gameObject);return;}Refresh();}
 }
}
