using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using BepInEx;
using HarmonyLib;
using Planetbase;
using PlanetbaseSolarFrontier;
using UnityEngine;
using Module=Planetbase.Module;
[BepInPlugin("local.planetbase.solar-frontier.qa","Solar Frontier QA","0.1.0")]
[BepInDependency("local.planetbase.solar-frontier")]
public sealed class SolarFrontierProbe:BaseUnityPlugin
{
 static SolarFrontierProbe probe;static int step,checks,newWorld=4,phase,worldOnly;
 static string save;static float captureAt,expectedProgress,expectedFuel;static int actorId,reportCount,wasteCount;
 static Module lab,assembly,treatment,greenhouse,geo,fusion,drill,pylon;
 static ConstructionComponent desk;static Character engineer;static Vector3 viewCenter;
 void Awake(){probe=this;Application.runInBackground=true;foreach(string arg in Environment.GetCommandLineArgs())if(arg.StartsWith("-solar-world="))int.TryParse(arg.Substring(13),out worldOnly);new Harmony(Info.Metadata.GUID).PatchAll(typeof(SolarFrontierProbe).Assembly);}
 [HarmonyPatch(typeof(ServerInterfaceSteam),nameof(ServerInterfaceSteam.isAvailable))]
 class Offline{static bool Prefix(ref bool __result){__result=false;return false;}}
 [HarmonyPatch(typeof(GameManager),nameof(GameManager.update))]
 class Drive
 {
  static void Postfix(GameManager __instance,ref GameManager.State ___mState)
  {
   try
   {
    var state=__instance.getGameState();
    if(step==0&&state is GameStateTitle&&___mState==GameManager.State.Updating)
    {
     Check(Util.getFilesFolder().Contains("PlanetbaseResourceMenu-qa"),"isolated QA profile");Check(Singleton<ServerInterfaceManager>.getInstance().getInterfaceName().Contains("Offline"),"offline achievements");
     Check(TypeList<Planet,PlanetList>.getCount()==8,"four worlds appended");Check(TypeList<Planet,PlanetList>.get().Take(4).Select(p=>p.GetType().Name).SequenceEqual(new[]{"PlanetClassD","PlanetClassF","PlanetClassM","PlanetClassS"}),"original world indices retained");
     if(worldOnly>=4&&worldOnly<=7){newWorld=worldOnly;step=7;__instance.setGameStateGameNew(71111,newWorld,false,null);return;}
     __instance.setGameStateLocationSelection();step=10;captureAt=Time.realtimeSinceStartup+4;return;
    }
    if(step==10&&state is GameStateLocationSelection&&___mState==GameManager.State.Updating&&Time.realtimeSinceStartup>captureAt)
    {Check(((GameObject[])AccessTools.Field(typeof(GameStateLocationSelection),"mPlanets").GetValue(state)).Length==8,"native selector creates eight planets");step=1;__instance.setGameStateGameLoad(Path.Combine(SaveData.FolderName,"save1.sav"),0,null);return;}
    if(!(state is GameStateGame game)||___mState!=GameManager.State.Waiting)return;
    game.onStartUpdating();___mState=GameManager.State.Updating;Singleton<TimeManager>.getInstance().pause();
    if(step==1){step=2;Exercise(game);}
    else if(step==3){step=4;Reload(game);}
    else if(step==7)
    {
     Check(Singleton<PlanetManager>.getInstance().getCurrentPlanetIndex()==newWorld,"new game index "+newWorld);
     Check(PlanetManager.getCurrentPlanet() is SolarWorld,"native new-world terrain generated "+PlanetManager.getCurrentPlanet().getName());
     Check(NavigationGraph.getExterior().findNearestNode(Singleton<TerrainGenerator>.getInstance().getCenter(),1)!=null,"new-world navigation generated "+newWorld);
     for(int i=0;i<2000&&WorldObjects.Characters.Count<10;i++)Ship.updateAll(.1f,0);
     Check(WorldObjects.Characters.Count(c=>c is Colonist)==8,"new-world landing deploys eight colonists "+newWorld);
     Check(Resource.getCountOfType(ResourceTypeList.MetalInstance)==60,"new-world landing delivers starting materials "+newWorld);
     if(newWorld==5||newWorld==6)Check(PlanetManager.getCurrentPlanet().getAtmosphereDensity()==Planet.Quantity.None,"vacuum world has no wind "+newWorld);
     step=8;probe.Logger.LogInfo("SOLAR WORLD QA PASSED: "+newWorld+"; "+checks+" checks.");
    }
   }
   catch(Exception e){Fail(e);}
  }
 }
 static void Exercise(GameStateGame game)
 {
  Check(TypeList<ResourceType,ResourceTypeList>.find<PlanetbaseNuclearTransport.NuclearFuelRod>().getTypeIndex()==14,"previous fuel index preserved");
  Check(TypeList<Specialization,SpecializationList>.find<ScoutDrone>()!=null&&SpecializationList.getBotSpecializations().Count==(AppDomain.CurrentDomain.GetAssemblies().Any(a=>a.GetName().Name=="PlanetbaseCommunityExpansion")?9:6),"three new native specializations");
  Check(!Research.Has(typeof(TechSolarFusion)),"fusion starts locked");
  TerrainGenerator t=Singleton<TerrainGenerator>.getInstance();t.flatten(new Vector3(1260,0,1150),145,t.getFloorHeight());t.clearArea(new Vector3(1260,0,1150),145);Physics.SyncTransforms();
  Module start=Module.findOfType(TypeList<ModuleType,ModuleTypeList>.find<ModuleTypeCanteen>());
  lab=Make<ModuleTypeSurveyLab>(1185,1123.5f);Link(start,lab);assembly=Make<ModuleTypeAssemblyWorks>(1208,1123.5f);Link(lab,assembly);
  new Grid().calculate(lab);Supply(lab);Supply(assembly);
  desk=Equip<ResearchDesk>(lab);engineer=WorldObjects.Characters.First(c=>c is Colonist&&c.getSpecialization() is Engineer);Prepare(engineer,Location.Interior,desk.getAnchorPoint().position);
  Reports(2);int total=Resource.getCountOfType(TypeList<ResourceType,ResourceTypeList>.find<ResearchPacket>());Check(total==2,"engineer manufactures two physical research reports");
  Research.Start(0);Check(Research.DataStock().Count==0,"local research consumes reports once");Research.Tick(90);Check(Research.Has(typeof(TechSolarSurvey)),"survey research completed");
  Reports(3);Research.Start(1);Research.Tick(179);float held=SolarState.Current.Progress;lab.setPower(false);Research.Tick(5);Check(SolarState.Current.Progress==held,"local research stops on power cut");Supply(lab);Research.Tick(1);Check(Research.Has(typeof(TechSolarGeothermal)),"geothermal branch unlocked");
  Reports(2);Research.Start(2);Research.Tick(120);Check(Research.Has(typeof(TechSolarRecycling)),"recycling branch unlocked");Reports(3);Research.Start(3);Research.Tick(240);Check(Research.Has(typeof(TechSolarRobotics)),"robotics prerequisite branch unlocked");
  int coins=Resource.getCountOfType(ResourceTypeList.CoinsInstance);Resource.addInmaterialResource(new ResourceAmount(ResourceTypeList.CoinsInstance,500-coins));
  Research.Start(4);int paid=Resource.getCountOfType(ResourceTypeList.CoinsInstance);Research.Start(4);Check(paid==410&&Resource.getCountOfType(ResourceTypeList.CoinsInstance)==paid,"Earth blueprints charge once");Research.Tick(300);Check(Research.Has(typeof(TechSolarShield)),"Earth shielding delivered");
  Reports(4);Research.Start(5);Research.Tick(600);Check(Research.Has(typeof(TechSolarFusion)),"fusion requires both completed branches");
  treatment=Make<ModuleTypeTreatmentPlant>(1231,1123.5f);Link(assembly,treatment);greenhouse=Make<ModuleTypeSolarGreenhouse>(1254,1123.5f);Link(treatment,greenhouse);
  Module tunnel=Make<ModuleTypeTunnelGallery>(1277,1123.5f);Link(greenhouse,tunnel);
  fusion=Make<ModuleTypeFusionPlant>(1300,1123.5f);Link(tunnel,fusion);drill=Make<ModuleTypeRegolithDrill>(1323,1123.5f);Link(fusion,drill);pylon=Make<ModuleTypeRadiationPylon>(1346,1123.5f);Link(drill,pylon);
  Vector3 hot=Vector3.zero;
  for(float x=1185;x<=1360&&hot==Vector3.zero;x+=5)foreach(float z in new[]{1098.5f,1148.5f})if(Survey.Heat(new Vector3(x,0,z))>=.55f){hot=new Vector3(x,0,z);break;}
  Check(hot!=Vector3.zero,"geothermal candidate exists");Survey.Complete(hot);geo=Make<ModuleTypeGeothermalPlant>(hot.x,hot.z);
  Module nearest=WorldObjects.Modules.Where(m=>m!=geo&&m.getModuleType() is SolarModule).OrderBy(m=>(m.getPosition()-geo.getPosition()).sqrMagnitude).First();Link(nearest,geo);
  new Grid().calculate(lab);foreach(Module m in WorldObjects.Modules.Where(m=>m.getModuleType() is SolarModule))Supply(m);
  geo.updatePowerGeneration();Check(geo.getPowerGeneration()>=11000&&geo.getPowerGeneration()<=20000,"geothermal output follows surveyed heat");
  var parts=Equip<PartsWorkshop>(assembly);var chassis=Equip<ChassisBench>(assembly);var assembler=Equip<AdvancedAssembler>(assembly);var refinery=Equip<FusionRefinery>(assembly);var rego=Equip<RegolithProcessor>(assembly);
  Stock(parts,ResourceTypeList.MetalInstance,TypeList<ResourceType,ResourceTypeList>.find<Semiconductors>());Produce(parts);Check(parts.getResourceContainer().getCountOf(TypeList<ResourceType,ResourceTypeList>.find<MachineParts>())==1,"parts manufacturing consumes inputs");
  Stock(chassis,ResourceTypeList.MetalInstance,ResourceTypeList.BioplasticInstance);Produce(chassis);Check(chassis.getResourceContainer().getCountOf(TypeList<ResourceType,ResourceTypeList>.find<VehicleChassis>())==1,"chassis manufacturing");
  Move(parts,assembler,TypeList<ResourceType,ResourceTypeList>.find<MachineParts>());Move(chassis,assembler,TypeList<ResourceType,ResourceTypeList>.find<VehicleChassis>());Stock(assembler,TypeList<ResourceType,ResourceTypeList>.find<Semiconductors>());
  int bots=Character.getCountOfSpecialization(TypeList<Specialization,SpecializationList>.find<ScoutDrone>());Produce(assembler);Check(Character.getCountOfSpecialization(TypeList<Specialization,SpecializationList>.find<ScoutDrone>())==bots+1,"Scout Drone assembled from actual manufactured parts");
  Check(assembler.getResourceContainer().getResourceCount()==0,"assembly consumed all three inputs");
  assembler.setProducedItemIndex(2);Stock(assembler,TypeList<ResourceType,ResourceTypeList>.find<MachineParts>(),TypeList<ResourceType,ResourceTypeList>.find<VehicleChassis>(),TypeList<ResourceType,ResourceTypeList>.find<Semiconductors>());Produce(assembler);Check(Character.getCountOfSpecialization(TypeList<Specialization,SpecializationList>.find<CargoTruck>())==1,"cargo truck native assembly alternative");
  assembler.setProducedItemIndex(1);Stock(assembler,TypeList<ResourceType,ResourceTypeList>.find<MachineParts>(),TypeList<ResourceType,ResourceTypeList>.find<VehicleChassis>(),TypeList<ResourceType,ResourceTypeList>.find<Semiconductors>());Produce(assembler);Check(Character.getCountOfSpecialization(TypeList<Specialization,SpecializationList>.find<ScannerBot>())==1,"scanner bot native assembly alternative");
  Stock(refinery,TypeList<ResourceType,ResourceTypeList>.find<MachineParts>(),TypeList<ResourceType,ResourceTypeList>.find<Semiconductors>());Produce(refinery);fusion.update(.1f,0);Check(fusion.getPowerGeneration()==120000&&MachineState.For(fusion).Fuel==1200,"fusion burns manufactured capsule");fusion.update(20,0);Check(MachineState.For(fusion).Fuel==1180,"fusion fuel advances with simulation");fusion.setHasWater(false);fusion.update(10,0);Check(fusion.getPowerGeneration()==0&&MachineState.For(fusion).Fuel==1180,"fusion water interlock preserves remaining fuel");Supply(fusion);fusion.update(.1f,0);
  var warehouseType=TypeList<ModuleType,ModuleTypeList>.find<ModuleTypeStorage>();var warehouse=Module.create(new Vector3(1277,0,1098.5f),warehouseType.getMinSize(),warehouseType);Finish(warehouse);Link(tunnel,warehouse);new Grid().calculate(lab);foreach(Module room in WorldObjects.Modules.Where(m=>m.getModuleType() is SolarModule||m==warehouse))Supply(room);
  Prepare(engineer,Location.Interior,warehouse.getFloorPosition());var storedCapsule=Resource.create(TypeList<ResourceType,ResourceTypeList>.find<FusionCapsule>(),engineer.getPosition(),Location.Interior);engineer.loadResource(storedCapsule);engineer.storeResource(warehouse);Check(storedCapsule.getState()==Resource.State.Stored,"capsule stored through native warehouse slots");fusion.update(2000,0);fusion.update(.1f,0);Check(storedCapsule.isDestroyed()&&MachineState.For(fusion).Fuel==1200,"fusion refuels from connected native warehouse without duplicating capsule");
  int ore=Resource.getCountOfType(TypeList<ResourceType,ResourceTypeList>.find<Regolith>());drill.update(300,0);Check(Resource.getCountOfType(TypeList<ResourceType,ResourceTypeList>.find<Regolith>())==ore+1,"surface regolith extraction");Stock(rego,TypeList<ResourceType,ResourceTypeList>.find<Regolith>());Produce(rego);Check(rego.getResourceContainer().getCountOf(ResourceTypeList.OreInstance)==1,"regolith to native ore");
  var sanitary=Equip<SanitaryUnit>(treatment);var recycler=Equip<WasteReprocessor>(treatment);Prepare(engineer,Location.Interior,sanitary.getPosition());((Indicator[])AccessTools.Field(typeof(Character),"mIndicators").GetValue(engineer))[(int)CharacterIndicator.Hydration].setValue(.1f);
  Interaction visit=Interaction.create<InteractionComponentRestore>(engineer,sanitary);int waste=Resource.getCountOfType(TypeList<ResourceType,ResourceTypeList>.find<OrganicWaste>());visit.update(11);visit.update(1);Check(Resource.getCountOfType(TypeList<ResourceType,ResourceTypeList>.find<OrganicWaste>())==waste+1,"sanitary visit produces waste once");visit.destroy();
  Stock(recycler,TypeList<ResourceType,ResourceTypeList>.find<OrganicWaste>());Produce(recycler);Check(recycler.getResourceContainer().getCountOf(TypeList<ResourceType,ResourceTypeList>.find<Fertilizer>())==1,"waste becomes fertilizer");
  var hopper=Equip<NutrientHopper>(greenhouse);var meat=Equip<TissueSynthesizer>(greenhouse);Move(recycler,hopper,TypeList<ResourceType,ResourceTypeList>.find<Fertilizer>());greenhouse.update(.1f,0);Check(MachineState.For(greenhouse).Nutrients==600,"greenhouse consumes fertilizer into timed buff");Check(hopper.getResourceContainer().getResourceCount()==0,"fertilizer consumed exactly once");
  var progress=(Indicator)AccessTools.Field(typeof(ConstructionComponent),"mProductionProgress").GetValue(meat);MachineState.For(greenhouse).Nutrients=0;progress.setValue(0);AccessTools.Method(typeof(ConstructionComponent),"updateProduction").Invoke(meat,new object[]{20f});float baseline=progress.getValue();MachineState.For(greenhouse).Nutrients=600;progress.setValue(0);AccessTools.Method(typeof(ConstructionComponent),"updateProduction").Invoke(meat,new object[]{20f});Check(baseline>0&&Math.Abs(progress.getValue()/baseline-1.25f)<.001,"fertilizer increases actual native food production by 25 percent");
  NavigationGraph.update();NavigationGraph.update();
  Character driver=WorldObjects.Characters.First(c=>c is Colonist&&c!=engineer);Vector3 p=new Vector3(1260,t.getFloorHeight(),1108);Prepare(driver,Location.Exterior,p);actorId=driver.getId();
  probe.Logger.LogInfo("Manual QA: start color "+NavigationGraph.getExteriorColorIndex(p)+", end color "+NavigationGraph.getExteriorColorIndex(p+Vector3.left*12)+", interactions "+driver.anyInteractions()+", flee "+driver.isFleeing());
  Check(ManualControl.Issue(driver,p+Vector3.left*12),"manual colonist move accepted");for(int i=0;i<150&&UnitState.For(driver).Order!="";i++){driver.update(.1f);if(i%2==0)driver.tick(.2f);}Check((driver.getPosition()-(p+Vector3.left*12)).sqrMagnitude<16,"manual colonist follows real path");
  Check(ManualControl.Issue(driver,driver.getPosition(),true),"manual scan accepted");for(int i=0;i<110;i++)driver.update(.1f);Check(Survey.IsScanned(driver.getPosition()),"scan discovers actual heat and regolith field");
  Character bot=WorldObjects.Characters.First(c=>c is Bot&&c.getSpecialization() is Carrier);Prepare(bot,Location.Exterior,p+Vector3.back*4);Check(ManualControl.Issue(bot,p+Vector3.left*5),"native carrier accepts micro control");ManualControl.Release(bot);
  Character truck=WorldObjects.Characters.First(c=>c.getSpecialization() is CargoTruck);Prepare(truck,Location.Exterior,p+Vector3.right*4);truck.loadResource(Resource.create(ResourceTypeList.MetalInstance,truck.getPosition(),Location.Exterior));Check(ManualControl.Issue(truck,truck.getPosition()+Vector3.back*12),"vehicle accepts micro control with real cargo");for(int i=0;i<50;i++)truck.update(.1f);Check(truck.isLoaded(),"vehicle movement preserves cargo");ManualControl.Hold(truck);
  Character scout=WorldObjects.Characters.First(c=>c.getSpecialization() is ScoutDrone);Prepare(scout,Location.Interior,lab.getFloorPosition());Check(ManualControl.Issue(scout,p,true),"indoor scout routes through native airlock");Singleton<TimeManager>.getInstance().unpause();for(int i=0;i<3000&&UnitState.For(scout).Order.Length>0;i++){scout.update(.1f);if(i%2==0)scout.tick(.2f);Interaction.updateAll(.1f);NavigationGraph.update();}Singleton<TimeManager>.getInstance().pause();Check(scout.getLocation()==Location.Exterior&&UnitState.For(scout).Order.Length==0&&Survey.IsScanned(p),"scout exits airlock and completes exterior survey through native navigation");ManualControl.Hold(scout);
  var pm=Singleton<PlanetManager>.getInstance();pm.setCurrentPlanetIndex(6);Prepare(driver,Location.Exterior,pylon.getPosition()+Vector3.back*15);Supply(pylon);driver.tick(60);Check(UnitState.For(driver).Dose==0,"powered pylon blocks Europa radiation");pylon.setPower(false);driver.tick(90);Check(UnitState.For(driver).Dose>=.49f,"Europa exposure accumulates without shielding");Supply(pylon);driver.tick(30);Check(UnitState.For(driver).Dose<.5f,"shielding reduces accumulated dose");pm.setCurrentPlanetIndex(0);
  pm.setCurrentPlanetIndex(6);ManualControl.Hold(driver);var flare=Singleton<DisasterManager>.getInstance().getSolarFlare();flare.trigger();flare.update(60);Supply(pylon);Check(driver.isProtected(),"pylon integrates with native solar flare protection");driver.update(10);var health=((Indicator[])AccessTools.Field(typeof(Character),"mIndicators").GetValue(driver))[(int)CharacterIndicator.Health];Check(health.getValue()>.99f,"native solar flare causes no damage inside powered shield");pylon.setPower(false);driver.update(10);Check(health.getValue()<.9f,"native solar flare damages unshielded colonist");flare.update(300);Supply(pylon);health.setValue(1);pm.setCurrentPlanetIndex(0);
  SolarState.Current.Raids=true;SolarState.Current.RaidClock=1800;int intruders=WorldObjects.Characters.Count(c=>c.getSpecialization()==SpecializationList.IntruderInstance);Threats.Tick(.1f);Check(WorldObjects.Characters.Count(c=>c.getSpecialization()==SpecializationList.IntruderInstance)==intruders+1,"optional raider spawns native hostile unit on reachable terrain");WorldObjects.Characters.Last(c=>c.getSpecialization()==SpecializationList.IntruderInstance).destroy();
  ManualControl.Hold(driver);Research.Start(6);Research.Tick(100);expectedProgress=SolarState.Current.Progress;expectedFuel=MachineState.For(fusion).Fuel;reportCount=Research.DataStock().Count;
  Prepare(engineer,Location.Interior,sanitary.getPosition());((Indicator[])AccessTools.Field(typeof(Character),"mIndicators").GetValue(engineer))[(int)CharacterIndicator.Hydration].setValue(.1f);var continuingVisit=Interaction.create<InteractionComponentRestore>(engineer,sanitary);continuingVisit.update(11);wasteCount=Resource.getCountOfType(TypeList<ResourceType,ResourceTypeList>.find<OrganicWaste>());
  SolarState.Current.Raids=true;SolarState.Current.RaidClock=12;Selection.select(geo);Survey.Layer=1;Survey.Refresh();viewCenter=assembly.getPosition()+Vector3.right*45;
  save=SaveData.getSavePath();AccessTools.Field(typeof(GameStateGame),"mSavePath").SetValue(game,save);probe.Logger.LogInfo("SOLAR QA mechanics passed. Saving "+save);
 }
 static void Reload(GameStateGame game)
 {
  Check(SolarState.Current.Project==6&&Math.Abs(SolarState.Current.Progress-expectedProgress)<.001,"Earth project survives reload without restart");Check(Research.Has(typeof(TechSolarFusion)),"native acquired techs persist");
  Check(SolarState.Current.Raids&&SolarState.Current.RaidClock==12,"optional raid settings persist");Check(SolarState.Current.Surveys.Count>=2,"survey coverage persists");
  Check(Research.DataStock().Count==reportCount,"no reports duplicated on reload");fusion=WorldObjects.Modules.Single(m=>m.getModuleType() is ModuleTypeFusionPlant);Check(Math.Abs(MachineState.For(fusion).Fuel-expectedFuel)<.001,"fusion remaining fuel persists exactly");
  Character actor=WorldObjects.Characters.Single(c=>c.getId()==actorId);Check(UnitState.For(actor).Order=="hold","manual hold survives reload");Check(UnitState.For(actor).Dose>0,"radiation exposure persists");
  foreach(Module m in WorldObjects.Modules.Where(m=>m.getModuleType() is SolarModule))Supply(m);
  var interactions=(List<Interaction>)AccessTools.Field(typeof(Interaction),"mInteractions").GetValue(null);var visit=interactions.Single(i=>i.getSelectable() is ConstructionComponent component&&component.getComponentType() is SanitaryUnit);visit.update(1);Check(Resource.getCountOfType(TypeList<ResourceType,ResourceTypeList>.find<OrganicWaste>())==wasteCount,"resuming sanitary visit does not duplicate waste");visit.destroy();
  Research.Tick(200);Check(Research.Has(typeof(TechConstructorBot)),"Earth delivery completes after reload");
  Check(WorldObjects.Modules.Count(m=>m.getModuleType() is SolarModule)==9,"all nine structures persist");Check(WorldObjects.Characters.Count(c=>c.getSpecialization() is ScoutDrone)==1&&WorldObjects.Characters.Count(c=>c.getSpecialization() is CargoTruck)==1,"manufactured vehicles persist once");
  var gui=(GameGui)AccessTools.Field(typeof(GameStateGame),"mGameGui").GetValue(game);gui.setWindow(new FrontierWindow(gui));Check(gui.getWindow() is FrontierWindow,"native F9 management window");
  Survey.Layer=1;Survey.Refresh();CameraManager.getInstance().focusOnPosition(viewCenter,85);captureAt=Time.realtimeSinceStartup+3;phase=0;
 }
 [HarmonyPatch(typeof(GameStateGame),nameof(GameStateGame.isEndOfFramePending))]
 class Frames{static void Postfix(ref bool __result){if(step==4)__result=true;}}
 [HarmonyPatch(typeof(GameStateGame),nameof(GameStateGame.onEndOfFrame))]
 class End
 {
  static void Postfix(GameStateGame __instance)
  {
   try
   {
    if(step==2&&File.Exists(save)){var doc=new XmlDocument();doc.Load(save);Check(doc.SelectSingleNode("//solar-frontier")!=null,"custom colony state serialized");step=3;GameManager.getInstance().setGameStateGameLoad(save,0,null);}
    else if(step==4&&Time.realtimeSinceStartup>captureAt)
    {
     string[] names={"research","control","world","overview","vehicles"};File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(SaveData.FolderName),"solar-"+names[phase]+".jpg"),Util.captureScreenshot(720));
     var gui=(GameGui)AccessTools.Field(typeof(GameStateGame),"mGameGui").GetValue(__instance);
     phase++;
     if(phase<3){gui.setWindow(new FrontierWindow(gui,phase));captureAt=Time.realtimeSinceStartup+2;return;}
     gui.setWindow(null);
     if(phase==3){Selection.select(WorldObjects.Modules.Single(m=>m.getModuleType() is ModuleTypeGeothermalPlant));Survey.Refresh();CameraManager.getInstance().focusOnPosition(viewCenter,85);captureAt=Time.realtimeSinceStartup+2;return;}
     if(phase==4)
     {
      Survey.Layer=0;Survey.Refresh();Character truck=WorldObjects.Characters.Single(c=>c.getSpecialization() is CargoTruck);Vector3 p=truck.getPosition();
      foreach(Character bot in WorldObjects.Characters.Where(c=>c.getSpecialization() is ScoutDrone||c.getSpecialization() is ScannerBot)){Prepare(bot,Location.Exterior,p+Vector3.right*(bot.getSpecialization() is ScoutDrone?3:6));ManualControl.Hold(bot);}
      CameraManager.getInstance().focusOnPosition(p+Vector3.right*3,17);captureAt=Time.realtimeSinceStartup+2;return;
     }
     step=8;probe.Logger.LogInfo("SOLAR QA PASSED: "+checks+" checks; research, industry, units, scanning, radiation, save/reload.");
    }
   }catch(Exception e){Fail(e);}
  }
 }
 static Module Make<T>(float x,float z)
 {
  ModuleType t=TypeList<ModuleType,ModuleTypeList>.find<T>();Check(t!=null&&!t.getName().Contains("ModuleType"),"localized type "+typeof(T).Name);Module m=Module.create(new Vector3(x,0,z),t.getMinSize(),t);Physics.SyncTransforms();Check(m.canPlaceModule(m.getPosition(),Vector3.up,Module.ValidSizes[t.getMinSize()]),"native placement "+typeof(T).Name);Finish(m);return m;
 }
 static void Link(Module a,Module b){Check(Connection.canLink(a,b),"native connection "+b.getName());Finish(Connection.create(a,b));}
 static void Finish(Buildable b){b.onUserPlaced();ResourceAmounts cost=b.calculateCost();foreach(ResourceType t in TypeList<ResourceType,ResourceTypeList>.get())for(int i=0;i<cost.getAmount(t);i++)b.addConstructionMaterial(Resource.create(t,b.getPosition(),b.getBuildLocation()));b.startBuilding();Check(b.updateBuild(1000)&&b.isBuilt(),"native construction "+b.getName());}
 static ConstructionComponent Equip<T>(Module m)
 {
  var c=ConstructionComponent.create(m,TypeList<ComponentType,ComponentTypeList>.find<T>());bool valid=false;
  for(int x=-7;x<=7&&!valid;x++)for(int z=-7;z<=7&&!valid;z++){c.setPosition(m.getFloorPosition()+new Vector3(x*.5f,0,z*.5f));c.setRotation(Quaternion.identity);valid=m.canPlaceComponent(c);}
  Check(valid,"native furnishing placement "+typeof(T).Name);m.getComponents().Add(c);Finish(c);return c;
 }
 static void Supply(Module m){m.setPower(true);m.setHasWater(true);m.getOxygenIndicator().setValue(1);}
 static void Prepare(Character c,Location location,Vector3 p){ManualControl.Release(c);AccessTools.Method(typeof(Character),"destroyInteractions").Invoke(c,null);if(c.isLoaded())c.unloadResource(Resource.State.Idle);c.setIdle();AccessTools.Method(typeof(Character),"setLocation").Invoke(c,new object[]{location});c.setPosition(p);foreach(Indicator i in c.getIndicators())if(i!=null)i.setValue(1);}
 static void Stock(ConstructionComponent c,params ResourceType[] types){foreach(ResourceType t in types){Resource r=Resource.create(t,c.getPosition(),Location.Interior);Check(c.getResourceContainer().add(r),"fixture input capacity "+c.getName());}}
 static void Move(ConstructionComponent from,ConstructionComponent to,ResourceType t){Resource r=from.getResourceContainer().extract(t);Check(r!=null&&to.getResourceContainer().add(r),"transfer manufactured "+t.getName());}
 static void Produce(ConstructionComponent c){Prepare(engineer,Location.Interior,c.getAnchorPoint()?.position??c.getPosition());Interaction w=Interaction.create<InteractionWork>(engineer,c);AccessTools.Method(typeof(ConstructionComponent),"updateProduction").Invoke(c,new object[]{1000f});w.destroy();}
 static void Reports(int n){for(int i=0;i<n;i++)Produce(desk);}
 static void Check(bool ok,string label){if(!ok)throw new InvalidOperationException(label);checks++;probe.Logger.LogInfo("PASS "+checks+": "+label);}
 static void Fail(Exception e){if(step==99)return;step=99;probe.Logger.LogError("SOLAR QA FAILED: "+e);}
}
