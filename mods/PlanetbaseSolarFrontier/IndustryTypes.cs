using System.Collections.Generic;
using HarmonyLib;
using Planetbase;
using UnityEngine;
namespace PlanetbaseSolarFrontier
{
 public abstract class SolarResource:ResourceType
 {
  protected SolarResource(string prefab,string icon,Color color,int value)
  {mModel=ResourceUtil.loadPrefab("Prefabs/Resources/Prefab"+prefab);mIcon=ResourceUtil.loadIconColor("Resources/icon_"+icon,color);mStatsColor=color;mSize=SmallResourceSize;mValue=value;mMerchantCategory=MerchantCategory.Count;}
 }
 public sealed class Regolith:SolarResource{public Regolith():base("Ore","ore",new Color(.65f,.5f,.3f),4){}}
 public sealed class MachineParts:SolarResource{public MachineParts():base("Spares","spares",Color.cyan,30){}}
 public sealed class VehicleChassis:SolarResource{public VehicleChassis():base("Metal","metal",new Color(1,.5f,.15f),40){}}
 public sealed class OrganicWaste:SolarResource{public OrganicWaste():base("Starch","starch",new Color(.5f,.4f,.2f),1){}}
 public sealed class Fertilizer:SolarResource{public Fertilizer():base("Starch","starch",Color.green,8){}}
 public sealed class FusionCapsule:SolarResource{public FusionCapsule():base("Semiconductors","semiconductors",Color.magenta,100){}}
 public sealed class ResearchPacket:SolarResource{public ResearchPacket():base("Semiconductors","semiconductors",Color.cyan,15){}}
 public abstract class SolarMachine:ComponentType
 {
  protected SolarMachine(string prefab,string icon,int power,int metal=2,int plastic=1)
  {
   mPrefabName="Prefab"+prefab;mIcon=ResourceUtil.loadIconColor("Components/icon_"+icon,Color.cyan);
   mConstructionCosts=new ResourceAmounts();mConstructionCosts.add(ResourceTypeList.MetalInstance,metal);mConstructionCosts.add(ResourceTypeList.BioplasticInstance,plastic);
   mPowerGeneration=-power;mEmbeddedResourceCount=4;mResourceProductionPeriod=120;
   mFlags=FlagRequiresOperator|FlagCanBeDisabled|FlagStoreProduction;
   mOperatorSpecialization=TypeList<Specialization,SpecializationList>.find<Engineer>();addUsageAnimation(CharacterAnimationType.WorkStanding);
  }
  protected void Inputs(params ResourceType[] inputs){mResourceConsumption=new List<ResourceType>(inputs);}
 }
 public sealed class ResearchDesk:SolarMachine
 {
  public ResearchDesk():base("SemiconductorFoundry","semiconductor_foundry",1500)
  {mResourceProductionPeriod=90;addResourceProduction<ResearchPacket>();initStrings();}
 }
 public sealed class PartsWorkshop:SolarMachine
 {
  public PartsWorkshop():base("SparesWorkshop","spares_workshop",2000)
  {Inputs(ResourceTypeList.MetalInstance,TypeList<ResourceType,ResourceTypeList>.find<Semiconductors>());addResourceProduction<MachineParts>();initStrings();}
 }
 public sealed class ChassisBench:SolarMachine
 {
  public ChassisBench():base("SparesWorkshop","spares_workshop",2000)
  {Inputs(ResourceTypeList.MetalInstance,ResourceTypeList.BioplasticInstance);addResourceProduction<VehicleChassis>();initStrings();}
 }
 public sealed class RegolithProcessor:SolarMachine
 {
  public RegolithProcessor():base("MetalProcessor","metal_processor",1500)
  {Inputs(TypeList<ResourceType,ResourceTypeList>.find<Regolith>());addResourceProduction<Ore>();mOperatorSpecialization=TypeList<Specialization,SpecializationList>.find<Worker>();mResourceProductionPeriod=60;initStrings();}
 }
 public sealed class FusionRefinery:SolarMachine
 {
  public FusionRefinery():base("SemiconductorFoundry","semiconductor_foundry",6000)
  {Inputs(TypeList<ResourceType,ResourceTypeList>.find<MachineParts>(),TypeList<ResourceType,ResourceTypeList>.find<Semiconductors>());addResourceProduction<FusionCapsule>();mRequiredTech=TypeList<Tech,TechList>.find<TechSolarFusion>();mWaterGeneration=-1000;mResourceProductionPeriod=240;initStrings();}
 }
 public sealed class AdvancedAssembler:SolarMachine
 {
  public AdvancedAssembler():base("BotWorkshop","bot_workshop",5000,4,2)
  {
   Inputs(TypeList<ResourceType,ResourceTypeList>.find<MachineParts>(),TypeList<ResourceType,ResourceTypeList>.find<VehicleChassis>(),TypeList<ResourceType,ResourceTypeList>.find<Semiconductors>());
   addBotProduction<ScoutDrone>();addBotProduction<ScannerBot>();addBotProduction<CargoTruck>();addBotProduction<Carrier>();addBotProduction<Constructor>();addBotProduction<Driller>();
   mFlags=FlagRequiresOperator|FlagCanBeDisabled|FlagAlternativeProduction;mRequiredTech=TypeList<Tech,TechList>.find<TechSolarRobotics>();mResourceProductionPeriod=240;
   mUsageAnimations=null;addUsageAnimation(CharacterAnimationType.WorkBotWorkshop);initStrings();
  }
 }
 public sealed class WasteReprocessor:SolarMachine
 {
  public WasteReprocessor():base("BioplasticProcessor","bioplastic_processor",1500)
  {Inputs(TypeList<ResourceType,ResourceTypeList>.find<OrganicWaste>());addResourceProduction<Fertilizer>();mOperatorSpecialization=TypeList<Specialization,SpecializationList>.find<Worker>();mResourceProductionPeriod=60;initStrings();}
 }
 public sealed class SanitaryUnit:ComponentType
 {
  public SanitaryUnit()
  {
   mPrefabName="PrefabDrinkingFountain";mIcon=ResourceUtil.loadIconColor("Components/icon_drinking_fountain",Color.green);
   mConstructionCosts=new ResourceAmounts();mConstructionCosts.add(ResourceTypeList.BioplasticInstance,1);mConstructionCosts.add(ResourceTypeList.MetalInstance,1);
   mPowerGeneration=-150;mWaterGeneration=-250;mFlags=FlagHydrating|FlagCanBeDisabled|FlagRequiresUncoveredHead;
   mStatusRecoveryTimes=new float[8];mStatusRecoveryTimes[(int)CharacterIndicator.Hydration]=30;mPrimaryStatusRecovery=CharacterIndicator.Hydration;mRadius=.5f;
   addUsageAnimation(CharacterAnimationType.UseFountain);initStrings();
  }
 }
 public sealed class NutrientHopper:ComponentType
 {
  public NutrientHopper()
  {
   mPrefabName="PrefabMedicalCabinet";mIcon=ResourceUtil.loadIconColor("Components/icon_medical_cabinet",Color.green);mConstructionCosts=new ResourceAmounts();mConstructionCosts.add(ResourceTypeList.BioplasticInstance,1);
   mEmbeddedResourceCount=6;mStoredResources=new List<ResourceType>{TypeList<ResourceType,ResourceTypeList>.find<Fertilizer>()};mFlags=FlagWallComponent;mWallSeparation=.9f;mRadius=.8f;initStrings();
  }
 }
 public abstract class SolarBot:Specialization
 {
  protected SolarBot(Specialization template,string prefix,int flags,float speed)
  {
   mModel=(GameObject)AccessTools.Field(typeof(Specialization),"mModel").GetValue(template);mIcon=template.getIcon();mNamePrefix=prefix;mAi=BotAi.getInstance();mFlags=flags;mCharacterType=typeof(Bot);mValue=250;mSpeedFactor=speed;mColor=Color.cyan;mRequiredTech=TypeList<Tech,TechList>.find<TechSolarRobotics>();
  }
 }
 public sealed class ScoutDrone:SolarBot{public ScoutDrone(Specialization t):base(t,"SCOUT",0,1.6f){}}
 public sealed class ScannerBot:SolarBot{public ScannerBot(Specialization t):base(t,"SCAN",0,1f){}}
 public sealed class CargoTruck:SolarBot{public CargoTruck(Specialization t):base(t,"TRUCK",FlagCarrier,1.8f){}}
}
