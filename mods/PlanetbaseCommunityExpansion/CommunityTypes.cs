using System.Collections.Generic;
using HarmonyLib;
using Planetbase;
using PlanetbaseSolarFrontier;
using UnityEngine;
namespace PlanetbaseCommunityExpansion
{
 internal interface ICommunityModule{string Template{get;}}
 public abstract class CommunityModule:ModuleType,ICommunityModule
 {
  public string Template{get;}
  protected CommunityModule(string template,bool exterior,int power,int metal,int plastic,params ComponentType[] equipment){Template="ModuleType"+template;mIcon=ResourceUtil.loadIconColor("Modules/icon_"+(exterior?"water_extractor":"multi_dome"),Color.cyan);mExterior=exterior;mPowerGeneration=-power;mMinSize=mMaxSize=exterior?0:1;mExteriorNavRadius=4;if(!exterior){mFlags=FlagDome|FlagLightAtNight|FlagWalkable|FlagSnapComponents;mLayoutType=LayoutType.Cross;}mCost=new ResourceAmounts();mCost.add(ResourceTypeList.MetalInstance,metal);if(plastic>0)mCost.add(ResourceTypeList.BioplasticInstance,plastic);mComponentTypes=equipment;mRequiredStructure.set<ModuleTypeCanteen>();initStrings();}
  protected static ComponentType E<T>()=>TypeList<ComponentType,ComponentTypeList>.find<T>();
 }
 public sealed class ModuleTypeSecurityCommand:CommunityModule{public ModuleTypeSecurityCommand():base("ControlCenter",false,2000,5,4,E<SecurityDispatch>(),E<Armory>(),E<DrinkingFountain>()){} }
 public sealed class ModuleTypeSurveillanceRadar:CommunityModule{public ModuleTypeSurveillanceRadar():base("WaterExtractor",true,3000,5,0){mCost.add(TypeList<ResourceType,ResourceTypeList>.find<Semiconductors>(),2);} }
 public sealed class ModuleTypeDefenseTurret:CommunityModule{public ModuleTypeDefenseTurret():base("WaterExtractor",true,4000,8,0){mCost.add(TypeList<ResourceType,ResourceTypeList>.find<MachineParts>(),2);mCondicionDecayTime=5400;mRepairResource=ResourceTypeList.SparesInstance;} }
 public sealed class ModuleTypePatrolDepot:CommunityModule{public ModuleTypePatrolDepot():base("WaterExtractor",true,1500,5,0){mCost.add(TypeList<ResourceType,ResourceTypeList>.find<VehicleChassis>(),1);} }
 public sealed class ModuleTypeLogisticsHub:CommunityModule{public ModuleTypeLogisticsHub():base("Factory",false,2000,6,6,E<LogisticsConsole>(),E<VehicleFabricator>(),E<SupplyPacker>()){mMinSize=mMaxSize=2;} }
 public sealed class ModuleTypeFreightWarehouse:ModuleTypeStorage,ICommunityModule{public string Template=>"ModuleTypeStorage";public ModuleTypeFreightWarehouse(){mIcon=ResourceUtil.loadIconColor("Modules/icon_storage",Color.cyan);mMinSize=1;mMaxSize=2;mExtraSize=2;mPowerGeneration=-1500;initStrings();} }
 public sealed class ModuleTypeExpeditionCenter:CommunityModule{public ModuleTypeExpeditionCenter():base("Lab",false,2000,5,5,E<SampleAnalyzer>(),E<LogisticsConsole>(),E<DrinkingFountain>()){} }
 public sealed class ModuleTypeExpeditionBeacon:CommunityModule{public ModuleTypeExpeditionBeacon():base("WaterExtractor",true,1000,3,0){} }
 public sealed class ModuleTypeEmergencyHospital:CommunityModule{public ModuleTypeEmergencyHospital():base("SickBay",false,2500,6,5,E<TraumaBed>(),E<MedicalCabinet>(),E<DrinkingFountain>()){} }
 public sealed class ModuleTypeRescueHangar:CommunityModule{public ModuleTypeRescueHangar():base("WaterExtractor",true,2000,6,0){mCost.add(TypeList<ResourceType,ResourceTypeList>.find<VehicleChassis>(),1);} }
 public sealed class ModuleTypeEmergencyShelter:CommunityModule{public ModuleTypeEmergencyShelter():base("MultiDome",false,1000,4,4,E<Bench>(),E<DrinkingFountain>(),E<PrivateBunk>()){} }
 public sealed class ModuleTypePrivateResidence:CommunityModule{public ModuleTypePrivateResidence():base("Dorm",false,1200,4,6,E<PrivateBunk>(),E<DecorativePlant>()){mLayoutType=LayoutType.Normal;mFlags|=FlagAutoRotate;} }
 public sealed class ModuleTypeAcademy:CommunityModule{public ModuleTypeAcademy():base("MultiDome",false,1800,5,5,E<TrainingConsole>(),E<DrinkingFountain>()){} }
 public sealed class ModuleTypeCommunityCafe:CommunityModule{public ModuleTypeCommunityCafe():base("Bar",false,1500,4,5,E<CafeSeat>(),E<MealMaker>(),E<DrinksMachine>(),E<DrinkingFountain>()){} }
 public sealed class ModuleTypeTradeTerminal:CommunityModule{public ModuleTypeTradeTerminal():base("ControlCenter",false,2000,5,5,E<TradeConsole>(),E<SupplyPacker>()){} }
 public sealed class ModuleTypeExportDock:CommunityModule{public ModuleTypeExportDock():base("LandingPad",true,2500,10,0){mMinSize=mMaxSize=2;mHeight=0;} }

 public class ConsoleEquipment:ComponentType
 {
  protected ConsoleEquipment(){mPrefabName="PrefabSecurityConsole";mIcon=ResourceUtil.loadIconColor("Components/icon_security_console",Color.cyan);mConstructionCosts=new ResourceAmounts();mConstructionCosts.add(ResourceTypeList.MetalInstance,1);mConstructionCosts.add(ResourceTypeList.BioplasticInstance,1);mPowerGeneration=-250;mRadius=1.25f;mFlags=FlagCanBeDisabled;addUsageAnimation(CharacterAnimationType.WorkSeated);initStrings();}
 }
 public sealed class SecurityDispatch:SecurityConsole{public SecurityDispatch(){mPrefabName="PrefabSecurityConsole";mPowerGeneration=-500;initStrings();} }
 public sealed class LogisticsConsole:ConsoleEquipment{}
 public sealed class TradeConsole:ConsoleEquipment{}
 public sealed class TrainingConsole:ConsoleEquipment{public TrainingConsole(){mPrimaryStatusRecovery=CharacterIndicator.Morale;mStatusRecoveryTimes=new float[8];mStatusRecoveryTimes[(int)CharacterIndicator.Morale]=600;} }
 public sealed class VehicleFabricator:SolarMachine
 {
  public VehicleFabricator():base("BotWorkshop","bot_workshop",5000,4,2){Inputs(TypeList<ResourceType,ResourceTypeList>.find<MachineParts>(),TypeList<ResourceType,ResourceTypeList>.find<VehicleChassis>(),TypeList<ResourceType,ResourceTypeList>.find<Semiconductors>());addBotProduction<FreightHauler>();addBotProduction<RescueRover>();addBotProduction<ExpeditionRover>();mFlags=FlagRequiresOperator|FlagCanBeDisabled|FlagAlternativeProduction;mRequiredTech=TypeList<Tech,TechList>.find<TechSolarRobotics>();mResourceProductionPeriod=240;mUsageAnimations=null;addUsageAnimation(CharacterAnimationType.WorkBotWorkshop);initStrings();}
 }
 public sealed class SampleAnalyzer:SolarMachine{public SampleAnalyzer():base("SemiconductorFoundry","semiconductor_foundry",1500){Inputs(TypeList<ResourceType,ResourceTypeList>.find<FieldSample>());addResourceProduction<ResearchPacket>();mResourceProductionPeriod=90;initStrings();} }
 public sealed class SupplyPacker:SolarMachine{public SupplyPacker():base("SparesWorkshop","spares_workshop",1000){Inputs(ResourceTypeList.MetalInstance,ResourceTypeList.BioplasticInstance);addResourceProduction<SupplyCrate>();mResourceProductionPeriod=90;initStrings();} }
 public sealed class TraumaBed:SickBayBed{public TraumaBed(){mPrefabName="PrefabSickBayBed";mPowerGeneration=-200;initStrings();} }
 public sealed class PrivateBunk:Bed{public PrivateBunk(){mPrefabName="PrefabBed";mStatusRecoveryTimes[(int)CharacterIndicator.Sleep]=180;mStatusRecoveryTimes[(int)CharacterIndicator.Morale]=400;initStrings();} }
 public sealed class CafeSeat:Bench{public CafeSeat(){mPrefabName="PrefabBench";mStatusRecoveryTimes[(int)CharacterIndicator.Morale]=240;mPowerGeneration=-100;initStrings();} }

 public abstract class CommunityResource:ResourceType
 {protected CommunityResource(int value,Color color){mModel=ResourceUtil.loadPrefab("Prefabs/Resources/PrefabSpares");mIcon=ResourceUtil.loadIconColor("Resources/icon_spares",color);mSize=SmallResourceSize;mStatsColor=color;mValue=value;mMerchantCategory=MerchantCategory.Count;} }
 public sealed class FieldSample:CommunityResource{public FieldSample():base(12,Color.cyan){} }
 public sealed class AncientRelic:CommunityResource{public AncientRelic():base(120,new Color(.8f,.6f,1)){} }
 public sealed class SupplyCrate:CommunityResource{public SupplyCrate():base(35,Color.green){} }
 public abstract class CommunityBot:Specialization
 {protected CommunityBot(Specialization template,string prefix,float speed){mModel=(GameObject)AccessTools.Field(typeof(Specialization),"mModel").GetValue(template);mIcon=template.getIcon();mNamePrefix=prefix;mAi=BotAi.getInstance();mFlags=0;mCharacterType=typeof(Bot);mValue=300;mSpeedFactor=speed;mColor=Color.cyan;mRequiredTech=TypeList<Tech,TechList>.find<TechSolarRobotics>();} }
 public sealed class FreightHauler:CommunityBot{public FreightHauler(Specialization t):base(t,"FREIGHT",1.45f){} }
 public sealed class RescueRover:CommunityBot{public RescueRover(Specialization t):base(t,"RESCUE",1.6f){} }
 public sealed class ExpeditionRover:CommunityBot{public ExpeditionRover(Specialization t):base(t,"EXPEDITION",1.55f){} }
}
