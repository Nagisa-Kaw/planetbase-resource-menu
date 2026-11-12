using System.Reflection;
using BepInEx;
using HarmonyLib;
using Planetbase;
using UnityEngine;

namespace PlanetbaseNuclearTransport
{
    [BepInPlugin("local.planetbase.nuclear-transport", "Energía nuclear y transporte", "1.2.0")]
    [BepInProcess("Planetbase.exe")]
    public sealed class NuclearTransportPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            if (Definitions.VersionNumber.ToString() != "1.4.2")
            {
                Logger.LogWarning("Energía nuclear y transporte requiere Planetbase 1.4.2; no se aplicaron cambios.");
                return;
            }
            new Harmony(Info.Metadata.GUID).PatchAll(typeof(NuclearTransportPlugin).Assembly);
            Logger.LogInfo("Expansión lista: reactor, refrigeración, taller de combustible y garaje de rovers.");
        }

        [HarmonyPatch(typeof(ResourceTypeList), MethodType.Constructor)]
        private static class RegisterFuel
        {
            private static void Postfix(ResourceTypeList __instance) =>
                AccessTools.Method(typeof(ResourceTypeList), "addResource").Invoke(__instance, new object[] { new NuclearFuelRod() });
        }

        [HarmonyPatch(typeof(ComponentTypeList), MethodType.Constructor)]
        private static class RegisterEquipment
        {
            private static void Postfix(ComponentTypeList __instance)
            {
                MethodInfo add = AccessTools.Method(typeof(TypeList<ComponentType, ComponentTypeList>), "add");
                add.Invoke(__instance, new object[] { new FuelCentrifuge() });
                add.Invoke(__instance, new object[] { new NuclearFuelStore() });
            }
        }

        [HarmonyPatch(typeof(ModuleTypeList), MethodType.Constructor)]
        private static class RegisterBuildings
        {
            private static void Postfix(ModuleTypeList __instance)
            {
                MethodInfo add = AccessTools.Method(typeof(TypeList<ModuleType, ModuleTypeList>), "add");
                foreach (ModuleType type in new ModuleType[] { new ModuleTypeNuclearReactor(), new ModuleTypeCoolingTower(),
                    new ModuleTypeFuelWorkshop(), new ModuleTypeRoverGarage() })
                    add.Invoke(__instance, new object[] { type });
            }
        }

        [HarmonyPatch(typeof(ModuleType), "loadPrefab")]
        private static class BuildingModels
        {
            private static bool Prefix(ModuleType __instance, int sizeIndex, ref GameObject __result)
            {
                if (!(__instance is ExpansionModule type)) return true;
                __result = (GameObject)AccessTools.Method(typeof(ModuleType), "loadPrefab")
                    .Invoke(TypeList<ModuleType, ModuleTypeList>.find(type.Template), new object[] { sizeIndex });
                ExpansionArchitecture.Build(__result, type);
                return false;
            }
        }

        [HarmonyPatch(typeof(ComponentType), nameof(ComponentType.createModel))]
        private static class EquipmentModels
        {
            private static void Postfix(ComponentType __instance, GameObject __result)
            {
                if (__instance is FuelCentrifuge || __instance is NuclearFuelStore)
                    ExpansionArchitecture.Equipment(__result, __instance);
            }
        }

        [HarmonyPatch(typeof(ResourceType), nameof(ResourceType.createModel))]
        private static class ResourceModels
        {
            private static void Postfix(ResourceType __instance,GameObject __result)
            { if(__instance is NuclearFuelRod)PlanetbaseVisuals.ResourceArchitecture.Attach(__result,"NuclearFuelRod"); }
        }

        [HarmonyPatch(typeof(ResourceType), nameof(ResourceType.getIcon))]
        private static class ResourceIcons
        {
            private static void Postfix(ResourceType __instance,ref Texture2D __result)
            { if(__instance is NuclearFuelRod)__result=PlanetbaseVisuals.ResourceArchitecture.Icon("NuclearFuelRod"); }
        }
    }
}
