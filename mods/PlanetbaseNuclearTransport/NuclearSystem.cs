using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using HarmonyLib;
using Planetbase;
using UnityEngine;
using Module = Planetbase.Module;

namespace PlanetbaseNuclearTransport
{
    internal static class NuclearSystem
    {
        internal const float FuelSeconds = 1800f;
        internal const float CoolingRange = 80f;
        private sealed class Core { internal float Remaining; }
        private static readonly ConditionalWeakTable<Module, Core> Cores = new ConditionalWeakTable<Module, Core>();
        internal static readonly List<Module> Modules = (List<Module>)AccessTools.Field(typeof(Module), "mModules").GetValue(null);
        internal static float Remaining(Module reactor) => Cores.GetOrCreateValue(reactor).Remaining;

        internal static bool HasCooling(Module reactor)
        {
            if (reactor.getGrid() == null) return false;
            var used = new HashSet<Module>();
            foreach (Module candidate in Modules.Where(m => m.getModuleType() is ModuleTypeNuclearReactor &&
                m.isBuilt() && m.isEnabled() && !m.isExtremelyDamaged()).OrderBy(m => m.getId()))
            {
                Module cooler = Modules.Where(m => m.getModuleType() is ModuleTypeCoolingTower && !used.Contains(m) &&
                    m.getGrid() == candidate.getGrid() && m.isOperational() && m.hasWater() &&
                    (m.getPosition() - candidate.getPosition()).sqrMagnitude <= CoolingRange * CoolingRange)
                    .OrderBy(m => (m.getPosition() - candidate.getPosition()).sqrMagnitude).ThenBy(m => m.getId()).FirstOrDefault();
                if (candidate == reactor) return cooler != null;
                if (cooler != null) used.Add(cooler);
            }
            return false;
        }

        private static bool Ready(Module reactor) => reactor.isBuilt() && reactor.isEnabled() &&
            !reactor.isExtremelyDamaged() && reactor.hasWater() && HasCooling(reactor);

        private static bool Refuel(Module reactor)
        {
            foreach (Module room in Modules)
            {
                if (room.getGrid() != reactor.getGrid() || !room.isOperational()) continue;
                foreach (ConstructionComponent component in room.getComponents())
                {
                    if (!(component.getComponentType() is NuclearFuelStore) && !(component.getComponentType() is FuelCentrifuge)) continue;
                    if (!component.isOperational()) continue;
                    Resource fuel = component.getResourceContainer()?.findResource(TypeList<ResourceType, ResourceTypeList>.find<NuclearFuelRod>());
                    if (fuel == null || fuel.getState() == Resource.State.Loaded || fuel.anyTargeters()) continue;
                    fuel.destroy();
                    Cores.GetOrCreateValue(reactor).Remaining = FuelSeconds;
                    return true;
                }
            }
            return false;
        }

        [HarmonyPatch(typeof(Module), nameof(Module.update))]
        private static class BurnFuel
        {
            private static void Prefix(Module __instance, float timeStep)
            {
                if (!(__instance.getModuleType() is ModuleTypeNuclearReactor) || timeStep <= 0f) return;
                Core core = Cores.GetOrCreateValue(__instance);
                if (Ready(__instance))
                {
                    if (core.Remaining <= 0f) Refuel(__instance);
                    // Fuel is spent only while producing power; a shutdown preserves the remaining rod.
                    if (__instance.getPowerGeneration() > 0)
                        core.Remaining = Mathf.Max(0f, core.Remaining - timeStep);
                }
                __instance.updatePowerGeneration();
            }
        }

        [HarmonyPatch(typeof(Module), "calculateCurrentPowerGeneration")]
        private static class SafetyInterlock
        {
            private static bool Prefix(Module __instance, ref int __result)
            {
                if (!(__instance.getModuleType() is ModuleTypeNuclearReactor)) return true;
                if (Remaining(__instance) > 0f && Ready(__instance)) return true;
                __result = 0;
                return false;
            }
        }

        [HarmonyPatch(typeof(Module), nameof(Module.serialize))]
        private static class SaveCore
        {
            private static void Postfix(Module __instance, XmlNode parent)
            {
                if (!(__instance.getModuleType() is ModuleTypeNuclearReactor)) return;
                XmlElement saved = parent.OwnerDocument.CreateElement("nuclear-transport-core");
                saved.SetAttribute("version", "1");
                saved.SetAttribute("remaining", XmlConvert.ToString(Remaining(__instance)));
                parent.LastChild.AppendChild(saved);
            }
        }

        [HarmonyPatch(typeof(Module), nameof(Module.deserialize))]
        private static class LoadCore
        {
            private static void Postfix(Module __instance, XmlNode node)
            {
                if (!(__instance.getModuleType() is ModuleTypeNuclearReactor)) return;
                float remaining = 0f;
                if (node["nuclear-transport-core"] is XmlElement saved &&
                    float.TryParse(saved.GetAttribute("remaining"), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float parsed) && !float.IsNaN(parsed) && !float.IsInfinity(parsed))
                    remaining = Mathf.Clamp(parsed, 0f, FuelSeconds);
                Cores.GetOrCreateValue(__instance).Remaining = remaining;
            }
        }

        [HarmonyPatch(typeof(Construction), nameof(Construction.getDescription))]
        private static class Status
        {
            private static void Postfix(Construction __instance, ref string __result)
            {
                if (!(__instance is Module module)) return;
                if (module.getModuleType() is ModuleTypeNuclearReactor)
                    __result += "\nCombustible restante: " + Remaining(module).ToString("0") + " s" +
                        "\nRefrigeración: " + (HasCooling(module) ? "conectada" : "falta torre disponible a 80 m") +
                        "\nAgua: " + (module.hasWater() ? "disponible" : "insuficiente");
                if (module.getModuleType() is ModuleTypeCoolingTower)
                    __result += "\nRefrigera un reactor a 80 m en la misma red. Necesita energía y agua.";
                if (module.getModuleType() is ModuleTypeRoverGarage)
                    __result += "\nRovers en servicio: " + RoverSystem.Used(module) + "/2\nAlcance de despliegue: 120 m. Transporte exterior ×2.";
            }
        }

        private sealed class FuelDeliveryRule : AiRule
        {
            public override bool update(Character character)
            {
                if (!hasCarryPriority(character) || !canWork(character) || character.isLoaded()) return false;
                ResourceType type = TypeList<ResourceType, ResourceTypeList>.find<NuclearFuelRod>();
                ConstructionComponent store = Module.findStorageComponent(character, type);
                if (store == null) return false;
                Resource resource = Resource.findStorable(character, type, true);
                return resource != null && goTarget(character, resource, store);
            }
        }

        [HarmonyPatch(typeof(AiRuleGoLoadResourcesToStore), nameof(AiRuleGoLoadResourcesToStore.update))]
        private static class DeliverFuel
        {
            private static readonly FuelDeliveryRule Rule = new FuelDeliveryRule();
            private static bool Prefix(Character character, ref bool __result)
            {
                if (!Rule.update(character)) return true;
                __result = true;
                return false;
            }
        }
    }
}
