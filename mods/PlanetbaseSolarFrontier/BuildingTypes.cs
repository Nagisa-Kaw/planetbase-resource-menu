using Planetbase;
using UnityEngine;
namespace PlanetbaseSolarFrontier
{
 public abstract class SolarModule:ModuleType
 {
  public readonly string Template,Label;
  public readonly System.Type Research;
  protected SolarModule(string template,string icon,string label,bool exterior,int power,int metal,int plastic,System.Type research=null)
  {
   Template="ModuleType"+template;Label=label;Research=research;mIcon=ResourceUtil.loadIconColor("Modules/icon_"+icon,Color.cyan);mExterior=exterior;mPowerGeneration=power;
   mMinSize=mMaxSize=exterior?0:1;mExteriorNavRadius=4;
   if(!exterior){mFlags=FlagDome|FlagWalkable|FlagLightAtNight|FlagSnapComponents;mLayoutType=LayoutType.Cross;}
   mCost=new ResourceAmounts();mCost.add(ResourceTypeList.MetalInstance,metal);if(plastic>0)mCost.add(ResourceTypeList.BioplasticInstance,plastic);
  }
  protected ComponentType Find<T>()=>TypeList<ComponentType,ComponentTypeList>.find<T>();
 }
 public sealed class ModuleTypeSurveyLab:SolarModule
 {
  public ModuleTypeSurveyLab():base("Lab","lab","RESEARCH",false,-2000,5,5){mComponentTypes=new[]{Find<ResearchDesk>(),Find<DrinkingFountain>()};mRequiredStructure.set<ModuleTypeLab>();initStrings();}
 }
 public sealed class ModuleTypeAssemblyWorks:SolarModule
 {
  public ModuleTypeAssemblyWorks():base("Factory","factory","ASSEMBLY",false,-3000,7,7)
  {mMinSize=mMaxSize=2;mComponentTypes=new[]{Find<PartsWorkshop>(),Find<ChassisBench>(),Find<AdvancedAssembler>(),Find<RegolithProcessor>(),Find<FusionRefinery>()};mRequiredStructure.set<ModuleTypeFactory>();initStrings();}
 }
 public sealed class ModuleTypeTreatmentPlant:SolarModule
 {
  public ModuleTypeTreatmentPlant():base("ProcessingPlant","processing_plant","RECYCLE",false,-1500,4,4,typeof(TechSolarRecycling))
  {mComponentTypes=new[]{Find<SanitaryUnit>(),Find<WasteReprocessor>()};mRequiredStructure.set<ModuleTypeProcessingPlant>();initStrings();}
 }
 public sealed class ModuleTypeSolarGreenhouse:SolarModule
 {
  public ModuleTypeSolarGreenhouse():base("BioDome","bio_dome","GREENHOUSE",false,-2000,5,5,typeof(TechSolarRecycling))
  {mComponentTypes=new[]{Find<TomatoPad>(),Find<WheatPad>(),Find<OnionPad>(),Find<TissueSynthesizer>(),Find<NutrientHopper>()};mRequiredStructure.set<ModuleTypeBioDome>();initStrings();}
 }
 public sealed class ModuleTypeTunnelGallery:SolarModule
 {
  public ModuleTypeTunnelGallery():base("MultiDome","multi_dome","SHELTER",false,-500,4,2)
  {mComponentTypes=new[]{Find<Bench>()};mRequiredStructure.set<ModuleTypeAirlock>();initStrings();}
 }
 public sealed class ModuleTypeGeothermalPlant:SolarModule
 {
  public ModuleTypeGeothermalPlant():base("WaterExtractor","water_extractor","GEOTHERMAL",true,20000,7,0,typeof(TechSolarGeothermal)){mCondicionDecayTime=5400;mRepairResource=ResourceTypeList.SparesInstance;mRequiredStructure.set<ModuleTypeWaterExtractor>();initStrings();}
 }
 public sealed class ModuleTypeFusionPlant:SolarModule
 {
  public ModuleTypeFusionPlant():base("WaterExtractor","power_collector","FUSION",true,120000,18,0,typeof(TechSolarFusion)){mCost.add(TypeList<ResourceType,ResourceTypeList>.find<MachineParts>(),4);mWaterGeneration=-3000;mCondicionDecayTime=6000;mRepairResource=ResourceTypeList.SparesInstance;mRequiredStructure.set<ModuleTypeFactory>();initStrings();}
 }
 public sealed class ModuleTypeRegolithDrill:SolarModule
 {
  public ModuleTypeRegolithDrill():base("WaterExtractor","mine","REGOLITH",true,-3000,5,0){mRequiredStructure.set<ModuleTypeProcessingPlant>();initStrings();}
 }
 public sealed class ModuleTypeRadiationPylon:SolarModule
 {
  public ModuleTypeRadiationPylon():base("WaterExtractor","lightning_rod","SHIELD",true,-6000,5,0,typeof(TechSolarShield)){mRequiredStructure.set<ModuleTypeSolarPanel>();initStrings();}
 }
}
