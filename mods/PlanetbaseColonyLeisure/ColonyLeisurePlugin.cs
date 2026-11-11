using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Planetbase;
using UnityEngine;

namespace PlanetbaseColonyLeisure
{
    [BepInPlugin("local.planetbase.colony-leisure", "Ocio colonial", "1.0.0")]
    [BepInProcess("Planetbase.exe")]
    public sealed class ColonyLeisurePlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log { get; private set; }

        private void Awake()
        {
            Log = Logger;
            if (Definitions.VersionNumber.ToString() != "1.4.2")
            {
                Logger.LogWarning("Ocio colonial requiere Planetbase 1.4.2.");
                return;
            }

            new Harmony(Info.Metadata.GUID).PatchAll(typeof(ColonyLeisurePlugin).Assembly);
            Logger.LogInfo("Ocio colonial listo. Cuatro edificios nuevos en Construir interior.");
        }

        [HarmonyPatch(typeof(ComponentTypeList), MethodType.Constructor)]
        private static class RegisterEquipment
        {
            private static readonly MethodInfo Add =
                AccessTools.Method(typeof(TypeList<ComponentType, ComponentTypeList>), "add");

            private static void Postfix(ComponentTypeList __instance)
            {
                foreach (ComponentType type in new ComponentType[]
                {
                    new ArcadeCabinet(), new DanceFloor(), new CasinoTerminal(), new MeditationSeat()
                })
                    Add.Invoke(__instance, new object[] { type });
            }
        }

        [HarmonyPatch(typeof(ModuleTypeList), MethodType.Constructor)]
        private static class RegisterBuildings
        {
            private static readonly MethodInfo Add =
                AccessTools.Method(typeof(TypeList<ModuleType, ModuleTypeList>), "add");

            private static void Postfix(ModuleTypeList __instance)
            {
                foreach (ModuleType type in new ModuleType[]
                {
                    new ModuleTypeArcade(), new ModuleTypeNightClub(),
                    new ModuleTypeCasino(), new ModuleTypeWellness()
                })
                    Add.Invoke(__instance, new object[] { type });
            }
        }

        [HarmonyPatch(typeof(ModuleType), "loadPrefab")]
        private static class BuildingModels
        {
            private static readonly MethodInfo Load = AccessTools.Method(typeof(ModuleType), "loadPrefab");

            private static bool Prefix(ModuleType __instance, int sizeIndex, ref GameObject __result)
            {
                if (!(__instance is LeisureDome dome))
                    return true;

                ModuleType template = TypeList<ModuleType, ModuleTypeList>.find(dome.Template);
                __result = (GameObject)Load.Invoke(template, new object[] { sizeIndex });
                LeisureModels.AddBuildingSign(__result, dome, sizeIndex);
                return false;
            }
        }

        [HarmonyPatch(typeof(ComponentType), nameof(ComponentType.createModel))]
        private static class EquipmentModels
        {
            private static void Postfix(ComponentType __instance, GameObject __result)
            {
                if (__instance is LeisureEquipment equipment)
                    LeisureModels.DecorateEquipment(__result, equipment);
            }
        }
    }
}
