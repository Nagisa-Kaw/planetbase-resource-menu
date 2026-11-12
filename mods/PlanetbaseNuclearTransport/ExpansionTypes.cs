using System.Collections.Generic;
using Planetbase;
using UnityEngine;

namespace PlanetbaseNuclearTransport
{
    public abstract class ExpansionModule : ModuleType
    {
        internal string Template { get; }
        protected ExpansionModule(string template, string icon, bool exterior, Color color)
        {
            Template = template;
            mIcon = ResourceUtil.loadIconColor("Modules/icon_" + icon, color);
            mExterior = exterior;
            mMinSize = mMaxSize = 0;
            mExteriorNavRadius = 4f;
        }
        protected void Cost(int metal, int plastic, int chips)
        {
            mCost = new ResourceAmounts();
            mCost.add(ResourceTypeList.MetalInstance, metal);
            if (plastic > 0) mCost.add(ResourceTypeList.BioplasticInstance, plastic);
            if (chips > 0) mCost.add(TypeList<ResourceType, ResourceTypeList>.find<Semiconductors>(), chips);
        }
    }

    public sealed class ModuleTypeNuclearReactor : ExpansionModule
    {
        public ModuleTypeNuclearReactor() : base("ModuleTypeWaterExtractor", "power_collector", true, ExpansionModels.Yellow)
        {
            mPowerGeneration = 60000;
            mWaterGeneration = -2000;
            mCondicionDecayTime = 5400f;
            mRepairResource = ResourceTypeList.SparesInstance;
            mRequiredStructure.set<ModuleTypeFactory>();
            Cost(12, 0, 4);
            initStrings();
        }
    }
    public sealed class ModuleTypeCoolingTower : ExpansionModule
    {
        public ModuleTypeCoolingTower() : base("ModuleTypeWaterExtractor", "water_extractor", true, ExpansionModels.Cyan)
        {
            mPowerGeneration = -2000;
            mWaterGeneration = -1000;
            mRequiredStructure.set<ModuleTypeWaterExtractor>();
            Cost(5, 0, 1);
            initStrings();
        }
    }
    public sealed class ModuleTypeRoverGarage : ExpansionModule
    {
        public ModuleTypeRoverGarage() : base("ModuleTypeWaterExtractor", "robotics_facility", true, ExpansionModels.Orange)
        {
            mPowerGeneration = -3000;
            mRequiredStructure.set<ModuleTypeFactory>();
            Cost(7, 2, 2);
            initStrings();
        }
    }
    public sealed class ModuleTypeFuelWorkshop : ExpansionModule
    {
        public ModuleTypeFuelWorkshop() : base("ModuleTypeFactory", "factory", false, ExpansionModels.Yellow)
        {
            mMinSize = mMaxSize = 1;
            mPowerGeneration = -2000;
            mFlags = FlagDome | FlagWalkable | FlagLightAtNight | FlagSnapComponents;
            mLayoutType = LayoutType.Cross;
            mComponentTypes = new[] { TypeList<ComponentType, ComponentTypeList>.find<FuelCentrifuge>(),
                TypeList<ComponentType, ComponentTypeList>.find<NuclearFuelStore>() };
            mRequiredStructure.set<ModuleTypeFactory>();
            Cost(6, 6, 2);
            initStrings();
        }
    }
    public sealed class NuclearFuelRod : ResourceType
    {
        public NuclearFuelRod()
        {
            mModel = ResourceUtil.loadPrefab("Prefabs/Resources/PrefabSemiconductors");
            mStatsColor = ExpansionModels.Yellow;
            mIcon = ResourceUtil.loadIconColor("Resources/icon_semiconductors", mStatsColor);
            mSize = SmallResourceSize;
            mValue = 60;
            // Only manufactured locally: no random merchant supply of nuclear fuel.
            mMerchantCategory = MerchantCategory.Count;
        }
    }
    public sealed class FuelCentrifuge : ComponentType
    {
        public FuelCentrifuge()
        {
            mPrefabName = "PrefabSemiconductorFoundry";
            mIcon = ResourceUtil.loadIconColor("Components/icon_semiconductor_foundry", ExpansionModels.Yellow);
            mConstructionCosts = new ResourceAmounts();
            mConstructionCosts.add(ResourceTypeList.MetalInstance, 2);
            mConstructionCosts.add(TypeList<ResourceType, ResourceTypeList>.find<Semiconductors>(), 1);
            mResourceConsumption = new List<ResourceType> { ResourceTypeList.OreInstance, ResourceTypeList.MetalInstance };
            addResourceProduction<NuclearFuelRod>();
            mEmbeddedResourceCount = 4;
            mResourceProductionPeriod = 180f;
            mPowerGeneration = -3000;
            mFlags = FlagRequiresOperator | FlagCanBeDisabled | FlagStoreProduction;
            mOperatorSpecialization = TypeList<Specialization, SpecializationList>.find<Engineer>();
            addUsageAnimation(CharacterAnimationType.WorkStanding);
            initStrings();
        }
    }
    public sealed class NuclearFuelStore : ComponentType
    {
        public NuclearFuelStore()
        {
            mPrefabName = "PrefabMedicalCabinet";
            mIcon = ResourceUtil.loadIconColor("Components/icon_medical_cabinet", ExpansionModels.Yellow);
            mConstructionCosts = new ResourceAmounts();
            mConstructionCosts.add(ResourceTypeList.MetalInstance, 1);
            mConstructionCosts.add(ResourceTypeList.BioplasticInstance, 1);
            mEmbeddedResourceCount = 6;
            mStoredResources = new List<ResourceType> { TypeList<ResourceType, ResourceTypeList>.find<NuclearFuelRod>() };
            mFlags = FlagWallComponent;
            mWallSeparation = 0.9f;
            mRadius = 0.8f;
            initStrings();
        }
    }
}
