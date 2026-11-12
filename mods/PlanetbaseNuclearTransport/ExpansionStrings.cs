using System.Collections.Generic;
using HarmonyLib;
using Planetbase;

namespace PlanetbaseNuclearTransport
{
    internal static class ExpansionStrings
    {
        private static readonly Dictionary<string, string> Text = new Dictionary<string, string>
        {
            ["nuclear_reactor"] = "Reactor nuclear",
            ["tooltip_nuclear_reactor"] = "Genera 60 kW constantes. Consume una barra de combustible cada 1800 s de funcionamiento y 2 unidades de agua. Necesita una torre de refrigeración disponible a 80 m y combustible en el taller de la misma red. Se detiene automáticamente si falta agua, refrigeración o combustible.",
            ["cooling_tower"] = "Torre de refrigeración",
            ["tooltip_cooling_tower"] = "Refrigera un reactor a 80 m en la misma red. Consume 2 kW y 1 unidad de agua. Construye una torre por reactor; se asignan primero a los reactores más antiguos.",
            ["rover_garage"] = "Garaje de rovers",
            ["tooltip_rover_garage"] = "Incluye dos rovers de carga y consume 3 kW. Los colonos con cajas pesadas despliegan automáticamente un rover a 120 m del garaje y transportan al doble de velocidad en exteriores. Llevan una caja por viaje y lo liberan al descargar o entrar en una esclusa.",
            ["fuel_workshop"] = "Taller de combustible nuclear",
            ["tooltip_fuel_workshop"] = "Sala interior para centrifugadoras y depósitos de combustible. Necesita oxígeno y 2 kW. Sus barras abastecen los reactores conectados a la misma red.",
            ["component_fuel_centrifuge"] = "Centrifugadora de combustible",
            ["tooltip_fuel_centrifuge"] = "Un ingeniero transforma 1 mineral y 1 metal en 1 barra de combustible en 180 s de trabajo base. Consume 3 kW. El mineral de Planetbase representa la materia prima nuclear en esta expansión.",
            ["component_nuclear_fuel_store"] = "Depósito de combustible",
            ["tooltip_nuclear_fuel_store"] = "Almacena 6 barras. Los colonos llevan aquí el combustible automáticamente; los reactores de la misma red lo recargan por el circuito de servicio.",
            ["nuclear_fuel_rod"] = "Barra de combustible nuclear",
            ["nuclear_fuel_rod_plural"] = "Barras de combustible nuclear"
        };

        [HarmonyPatch(typeof(StringList), nameof(StringList.get), new[] { typeof(string), typeof(string), typeof(string), typeof(string) })]
        private static class Translate
        {
            private static bool Prefix(string key, ref string __result)
            {
                if (!Text.TryGetValue(key, out string value)) return true;
                __result = value; return false;
            }
        }
        [HarmonyPatch(typeof(StringList), nameof(StringList.exists))]
        private static class Exists
        {
            private static bool Prefix(string key, ref bool __result)
            {
                if (!Text.ContainsKey(key)) return true;
                __result = true; return false;
            }
        }
        [HarmonyPatch(typeof(ConstructionComponent), nameof(ConstructionComponent.getHelpId))]
        private static class EquipmentHelp
        {
            private static void Postfix(ConstructionComponent __instance, ref string __result)
            {
                if (__instance.getComponentType() is FuelCentrifuge || __instance.getComponentType() is NuclearFuelStore)
                    __result = __instance.getComponentType().GetType().Name;
            }
        }
        [HarmonyPatch(typeof(HelpManager), MethodType.Constructor)]
        private static class RegisterHelp
        {
            private static void Postfix(Dictionary<string, HelpItem> ___mItems)
            {
                Add(___mItems, "NuclearReactor", "nuclear_reactor", "power_collector");
                Add(___mItems, "CoolingTower", "cooling_tower", "water_extractor");
                Add(___mItems, "RoverGarage", "rover_garage", "robotics_facility");
                Add(___mItems, "FuelWorkshop", "fuel_workshop", "factory");
                Add(___mItems, "FuelCentrifuge", "component_fuel_centrifuge", "semiconductor_foundry", "fuel_centrifuge");
                Add(___mItems, "NuclearFuelStore", "component_nuclear_fuel_store", "medical_cabinet", "nuclear_fuel_store");
            }
            private static void Add(Dictionary<string, HelpItem> items, string id, string key, string icon, string tip = null) =>
                items[id] = new HelpItem(id, Text[key], Text["tooltip_" + (tip ?? key)], "icon_" + icon);
        }
    }
}
