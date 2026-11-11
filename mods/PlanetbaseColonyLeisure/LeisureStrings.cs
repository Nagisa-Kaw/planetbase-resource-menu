using System.Collections.Generic;
using HarmonyLib;
using Planetbase;

namespace PlanetbaseColonyLeisure
{
    internal static class LeisureStrings
    {
        private static readonly Dictionary<string, string> Text = new Dictionary<string, string>
        {
            ["arcade"] = "Sala de videojuegos",
            ["tooltip_arcade"] = "Máquinas recreativas para recuperar moral. Dos jugadores en esta sala reciben un bono cooperativo del 50%.",
            ["night_club"] = "Club nocturno",
            ["tooltip_night_club"] = "Ocio para adultos. Pista social, bar y bebidas. La pista recupera moral y consume descanso.",
            ["casino"] = "Casino",
            ["tooltip_casino"] = "Rondas de 30 segundos con una apuesta de 2 monedas. Premios de 0, 2 o 6 monedas.",
            ["wellness"] = "Centro de bienestar",
            ["tooltip_wellness"] = "Sesiones de meditación que recuperan moral y descanso.",
            ["component_arcade_cabinet"] = "Máquina recreativa",
            ["tooltip_arcade_cabinet"] = "Recupera hasta un tercio de moral en 60 s. Con dos jugadores activos, el bono cooperativo suma un 50%.",
            ["component_dance_floor"] = "Pista social",
            ["tooltip_dance_floor"] = "Recupera moral durante 45 s y consume descanso. El colono se retira si tiene sueño.",
            ["component_casino_terminal"] = "Terminal de casino",
            ["tooltip_casino_terminal"] = "Apuesta 2 monedas por ronda. 60% sin premio, 30% devuelve 2 y 10% paga 6. Si se interrumpe antes del resultado, devuelve la apuesta.",
            ["component_meditation_seat"] = "Asiento de meditación",
            ["tooltip_meditation_seat"] = "Una sesión de 60 s recupera hasta 20% de moral y 10% de descanso."
        };

        [HarmonyPatch(typeof(ConstructionComponent), nameof(ConstructionComponent.getHelpId))]
        private static class EquipmentHelp
        {
            private static void Postfix(ConstructionComponent __instance, ref string __result)
            {
                if (__instance.getComponentType() is LeisureEquipment equipment)
                    __result = equipment.GetType().Name;
            }
        }

        [HarmonyPatch(typeof(HelpManager), MethodType.Constructor)]
        private static class RegisterHelp
        {
            private static void Postfix(Dictionary<string, HelpItem> ___mItems)
            {
                Add(___mItems, "Arcade", "arcade", "multi_dome");
                Add(___mItems, "NightClub", "night_club", "bar");
                Add(___mItems, "Casino", "casino", "bar");
                Add(___mItems, "Wellness", "wellness", "multi_dome");
                Add(___mItems, "ArcadeCabinet", "component_arcade_cabinet", "security_console", "arcade_cabinet");
                Add(___mItems, "DanceFloor", "component_dance_floor", "decorative_plant", "dance_floor");
                Add(___mItems, "CasinoTerminal", "component_casino_terminal", "security_console", "casino_terminal");
                Add(___mItems, "MeditationSeat", "component_meditation_seat", "bench", "meditation_seat");
            }

            private static void Add(Dictionary<string, HelpItem> items, string id, string key,
                string icon, string tooltip = null)
            {
                string body = Text["tooltip_" + (tooltip ?? key)] +
                    "\nNecesita construcción, energía y una conexión con oxígeno. Los colonos lo usan automáticamente cuando necesitan ocio.";
                items[id] = new HelpItem(id, Text[key], body, "icon_" + icon);
            }
        }

        [HarmonyPatch(typeof(StringList), nameof(StringList.get),
            new[] { typeof(string), typeof(string), typeof(string), typeof(string) })]
        private static class Translate
        {
            private static bool Prefix(string key, ref string __result)
            {
                if (!Text.TryGetValue(key, out string value))
                    return true;
                __result = value;
                return false;
            }
        }

        [HarmonyPatch(typeof(StringList), nameof(StringList.exists))]
        private static class HasTranslation
        {
            private static bool Prefix(string key, ref bool __result)
            {
                if (!Text.ContainsKey(key))
                    return true;
                __result = true;
                return false;
            }
        }
    }
}
