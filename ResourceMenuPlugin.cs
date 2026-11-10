using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Planetbase;
using UnityEngine;

namespace PlanetbaseResourceMenu
{
    [BepInPlugin("local.planetbase.resource-menu", "Menú de recursos", "1.0.0")]
    [BepInProcess("Planetbase.exe")]
    public sealed class ResourceMenuPlugin : BaseUnityPlugin
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

            new Harmony(Info.Metadata.GUID).PatchAll(typeof(ResourceMenuPlugin).Assembly);
            Logger.LogInfo("Menú de recursos listo. Pulsa F8 durante una partida.");
        }

        [HarmonyPatch(typeof(GameStateGame), nameof(GameStateGame.update))]
        private static class OpenResourceMenu
        {
            private static bool Prefix(GameGui ___mGameGui)
            {
                if (!Input.GetKeyDown(KeyCode.F8))
                    return true;

                if (___mGameGui.getWindow() is ResourceWindow)
                {
                    ___mGameGui.setWindow(null);
                    return false;
                }

                if (___mGameGui.isWindowVisible() || CameraManager.getInstance().isCinematic())
                    return true;

                ___mGameGui.setWindow(new ResourceWindow());
                return false;
            }
        }
    }
}
