using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Planetbase;

namespace PlanetbaseTerrainAndSpeed
{
    [BepInPlugin("local.planetbase.terrain-speed", "Terreno y velocidad", "1.0.0")]
    [BepInProcess("Planetbase.exe")]
    public sealed class TerrainAndSpeedPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log { get; private set; }

        private void Awake()
        {
            Log = Logger;
            string version = Definitions.VersionNumber.ToString();
            if (version != "1.4.2")
            {
                Logger.LogWarning($"Versión {version} sin validar. Se requiere Planetbase 1.4.2.");
                return;
            }

            new Harmony(Info.Metadata.GUID).PatchAll(typeof(TerrainAndSpeedPlugin).Assembly);
            Logger.LogInfo("Terreno y velocidad listo. Ctrl+F7 selecciona una zona. Velocidad máxima x8.");
        }

        [HarmonyPatch(typeof(TimeManager), MethodType.Constructor)]
        private static class ExtendGameSpeed
        {
            private static readonly FieldInfo SpeedOptions = AccessTools.Field(typeof(TimeManager), "TimeScales");

            private static void Postfix(TimeManager __instance, float[] ___TimeScales)
            {
                if (Array.IndexOf(___TimeScales, 8f) >= 0)
                    return;

                var extended = new float[___TimeScales.Length + 1];
                Array.Copy(___TimeScales, extended, ___TimeScales.Length);
                extended[extended.Length - 1] = 8f;
                SpeedOptions.SetValue(__instance, extended);
            }
        }
    }
}
