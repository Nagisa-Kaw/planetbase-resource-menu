using Planetbase;
using UnityEngine;

namespace PlanetbaseColonyLeisure
{
    public abstract class LeisureDome : ModuleType
    {
        internal string Template { get; }
        internal string Sign { get; }
        internal Color Accent { get; }

        protected LeisureDome(string template, string icon, string sign, Color accent, int power)
        {
            Template = template;
            Sign = sign;
            Accent = accent;
            mIcon = ResourceUtil.loadIconColor("Modules/icon_" + icon, accent);
            mMinSize = 1;
            mMaxSize = 2;
            mPowerGeneration = -power;
            mFlags = FlagDome | FlagLightAtNight | FlagWalkable | FlagSnapComponents;
            mLayoutType = LayoutType.Cross;
            mBaseCosts = new[] { 1, 4, 6, 7, 10 };
            mRequiredStructure.set<ModuleTypeCanteen>();
            initStrings();
        }
    }

    public sealed class ModuleTypeArcade : LeisureDome
    {
        public ModuleTypeArcade() : base("ModuleTypeMultiDome", "multi_dome", "ARCADE",
            new Color(0.1f, 0.9f, 1f), 2000)
        {
            mComponentTypes = new[]
            {
                TypeList<ComponentType, ComponentTypeList>.find<ArcadeCabinet>(),
                TypeList<ComponentType, ComponentTypeList>.find<Bench>(),
                TypeList<ComponentType, ComponentTypeList>.find<DrinkingFountain>()
            };
        }
    }

    public sealed class ModuleTypeNightClub : LeisureDome
    {
        public ModuleTypeNightClub() : base("ModuleTypeBar", "bar", "CLUB",
            new Color(1f, 0.2f, 0.7f), 2500)
        {
            mFlags |= FlagCylindricalBase;
            mComponentTypes = new[]
            {
                TypeList<ComponentType, ComponentTypeList>.find<DanceFloor>(),
                TypeList<ComponentType, ComponentTypeList>.find<DrinksMachine>(),
                TypeList<ComponentType, ComponentTypeList>.find<BarTable>(),
                TypeList<ComponentType, ComponentTypeList>.find<DrinkingFountain>()
            };
        }
    }

    public sealed class ModuleTypeCasino : LeisureDome
    {
        public ModuleTypeCasino() : base("ModuleTypeBar", "bar", "CASINO",
            new Color(1f, 0.75f, 0.15f), 2000)
        {
            mFlags |= FlagCylindricalBase;
            mComponentTypes = new[]
            {
                TypeList<ComponentType, ComponentTypeList>.find<CasinoTerminal>(),
                TypeList<ComponentType, ComponentTypeList>.find<BarTable>(),
                TypeList<ComponentType, ComponentTypeList>.find<DrinksMachine>(),
                TypeList<ComponentType, ComponentTypeList>.find<DrinkingFountain>()
            };
        }
    }

    public sealed class ModuleTypeWellness : LeisureDome
    {
        public ModuleTypeWellness() : base("ModuleTypeMultiDome", "multi_dome", "ZEN",
            new Color(0.4f, 1f, 0.5f), 1500)
        {
            mComponentTypes = new[]
            {
                TypeList<ComponentType, ComponentTypeList>.find<MeditationSeat>(),
                TypeList<ComponentType, ComponentTypeList>.find<DecorativePlant>(),
                TypeList<ComponentType, ComponentTypeList>.find<DrinkingFountain>()
            };
        }
    }

    public abstract class LeisureEquipment : ComponentType
    {
        internal Color Accent { get; }

        protected LeisureEquipment(string prefab, string icon, Color accent,
            CharacterAnimationType animation, float recovery, float duration, float cooldown)
        {
            Accent = accent;
            mPrefabName = prefab;
            mIcon = ResourceUtil.loadIconColor("Components/icon_" + icon, accent);
            mPrimaryStatusRecovery = CharacterIndicator.Morale;
            mStatusRecoveryTimes = new float[(int)CharacterIndicator.Count];
            mStatusRecoveryTimes[(int)CharacterIndicator.Morale] = recovery;
            mMaxUsageTime = duration;
            mUsageCooldown = cooldown;
            mFlags = FlagRelaxing | FlagRequiresUncoveredHead;
            mRadius = 1.25f;
            mMaxUsers = 1;
            mConstructionCosts = new ResourceAmounts();
            mConstructionCosts.add(ResourceTypeList.BioplasticInstance, 1);
            mConstructionCosts.add(ResourceTypeList.MetalInstance, 1);
            addUsageAnimation(animation);
            initStrings();
        }
    }

    public sealed class ArcadeCabinet : LeisureEquipment
    {
        public ArcadeCabinet() : base("PrefabSecurityConsole", "security_console",
            new Color(0.1f, 0.9f, 1f), CharacterAnimationType.WorkSeated, 180f, 60f, 240f)
        {
            mPowerGeneration = -500;
            mConstructionCosts.add(TypeList<ResourceType, ResourceTypeList>.find<Semiconductors>(), 1);
        }

        public override string getUsageDescription() => "Jugando videojuegos";
    }

    public sealed class DanceFloor : LeisureEquipment
    {
        public DanceFloor() : base("PrefabDecorativePlant", "decorative_plant",
            new Color(1f, 0.2f, 0.7f), CharacterAnimationType.Idle, 150f, 45f, 240f)
        {
            mPowerGeneration = -350;
            mRadius = 1.25f;
        }

        public override string getUsageDescription() => "Disfrutando del club";
    }

    public sealed class CasinoTerminal : LeisureEquipment
    {
        public CasinoTerminal() : base("PrefabSecurityConsole", "security_console",
            new Color(1f, 0.75f, 0.15f), CharacterAnimationType.WorkSeated, 240f, 35f, 480f)
        {
            mPowerGeneration = -400;
            mConstructionCosts.add(TypeList<ResourceType, ResourceTypeList>.find<Semiconductors>(), 1);
        }

        public override string getUsageDescription() => "Jugando una ronda de casino";
    }

    public sealed class MeditationSeat : LeisureEquipment
    {
        public MeditationSeat() : base("PrefabBench", "bench",
            new Color(0.4f, 1f, 0.5f), CharacterAnimationType.SitIdle, 300f, 60f, 180f)
        {
            mPowerGeneration = -100;
            mStatusRecoveryTimes[(int)CharacterIndicator.Sleep] = 600f;
            mTransitionInAnimations = new[] { new CharacterAnimation(CharacterAnimationType.Sit) };
            mTransitionOutAnimations = new[] { new CharacterAnimation(CharacterAnimationType.StandUp) };
            mRadius = 1f;
        }

        public override string getUsageDescription() => "Meditando y descansando";
    }
}
