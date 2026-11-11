using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using Planetbase;
using UnityEngine;
using UnityEngine.Rendering;

namespace PlanetbaseTerrainAndSpeed
{
    internal static class TerrainSelection
    {
        private const int Segments = 72;
        private const int TerrainLayer = 1 << 8;
        private static readonly Vector3[] RingPoints = new Vector3[Segments];
        private static LineRenderer ring;
        private static Vector3? selectedCenter;
        private static bool selecting;
        private static bool resumeOnClose;
        private static int radius;
        private static string hint;
        private static string pendingSave;

        [HarmonyPatch(typeof(GameStateGame), nameof(GameStateGame.update))]
        private static class UpdateSelection
        {
            [HarmonyPriority(Priority.High)]
            private static bool Prefix(GameStateGame __instance, GameGui ___mGameGui,
                Module ___mActiveModule, ref string ___mSavePath)
            {
                if (pendingSave != null)
                    return false;

                bool toggle = Input.GetKeyDown(KeyCode.F7) &&
                    (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
                if (!selecting)
                {
                    if (!toggle)
                        return true;
                    if (___mActiveModule != null || ___mGameGui.isWindowVisible() ||
                        !__instance.isSaveAllowed() || Singleton<ChallengeManager>.getInstance().isChallengeEnabled())
                        return false;

                    Open();
                }
                else if (toggle || Input.GetKeyUp(KeyCode.Escape))
                {
                    Close();
                    Resume();
                    return false;
                }

                if (Input.GetKeyDown(KeyCode.PageUp))
                    radius = Mathf.Min(radius + 10, FlattenZone.MaxRadius);
                if (Input.GetKeyDown(KeyCode.PageDown))
                    radius = Mathf.Max(radius - 10, FlattenZone.MinRadius);

                bool hasTarget = TryPickTerrain(out Vector3 target);
                if (hasTarget && Input.GetMouseButtonDown(0))
                    selectedCenter = target;
                if (Input.GetMouseButtonDown(1))
                    selectedCenter = null;

                Vector3? center = selectedCenter ?? (hasTarget ? (Vector3?)target : null);
                if (center.HasValue)
                {
                    var zone = new FlattenZone(center.Value, radius);
                    hint = GetPlacementError(zone);
                    DrawRing(center.Value, zone, hint == null, selectedCenter.HasValue);
                    if (hint == null && selectedCenter.HasValue &&
                        (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
                    {
                        if (___mSavePath == null && __instance.isSaveAllowed())
                            ScheduleSave(zone, ref ___mSavePath);
                        else
                            hint = "Espera a que termine el guardado.";
                    }
                }
                else
                {
                    ring.enabled = false;
                    hint = "Apunta al terreno para ver el círculo.";
                }

                return false;
            }
        }

        private static void Open()
        {
            selecting = true;
            radius = 40;
            selectedCenter = null;
            TimeManager time = Singleton<TimeManager>.getInstance();
            resumeOnClose = !time.isPaused();
            if (resumeOnClose)
                time.pause();

            var marker = new GameObject("Flatten zone preview") { layer = 2 };
            ring = marker.AddComponent<LineRenderer>();
            ring.sharedMaterial = ResourceList.getInstance().Materials.PlainColor;
            ring.shadowCastingMode = ShadowCastingMode.Off;
            ring.receiveShadows = false;
            ring.useWorldSpace = true;
            ring.loop = true;
            ring.positionCount = Segments;
            ring.startWidth = 0.38f;
            ring.endWidth = 0.38f;
            ring.enabled = false;
        }

        private static void Close()
        {
            selecting = false;
            selectedCenter = null;
            hint = null;
            if (ring != null)
                Object.Destroy(ring.gameObject);
            ring = null;
        }

        private static void Resume()
        {
            if (resumeOnClose)
                Singleton<TimeManager>.getInstance().unpause();
            resumeOnClose = false;
        }

        private static bool TryPickTerrain(out Vector3 point)
        {
            Camera camera = Camera.main;
            if (camera != null &&
                Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 300f, TerrainLayer))
            {
                point = hit.point;
                return true;
            }
            point = Vector3.zero;
            return false;
        }

        private static string GetPlacementError(FlattenZone zone)
        {
            if (!zone.IsValid())
                return "El centro debe quedar dentro del área de construcción.";

            List<Module> mines = Module.getCategoryModules(Module.Category.Mine);
            if (mines != null)
            {
                foreach (Module mine in mines)
                {
                    Vector3 p = mine.getPosition();
                    float clearance = zone.Radius + mine.getRadius() + 12f;
                    if (!mine.isDestroyed() &&
                        (zone.Center - new Vector2(p.x, p.z)).sqrMagnitude < clearance * clearance)
                        return "Reduce o mueve el círculo para dejar libre la ladera de la mina.";
                }
            }

            List<FlattenZone> zones = TerrainEdits.For(Singleton<TerrainGenerator>.getInstance());
            if (zones.Count >= TerrainEdits.MaxZones)
                return "Esta partida alcanzó el límite de 64 zonas.";
            foreach (FlattenZone existing in zones)
            {
                if (existing.Radius >= zone.Radius + (existing.Center - zone.Center).magnitude)
                    return "Esa área ya está dentro de una zona aplanada.";
            }
            return null;
        }

        private static Vector3 lastCenter;
        private static int lastRadius;
        private static Color lastColor;
        private static readonly MaterialPropertyBlock RingColor = new MaterialPropertyBlock();

        private static void DrawRing(Vector3 center, FlattenZone zone, bool valid, bool locked)
        {
            bool updateShape = !ring.enabled || lastRadius != radius || (lastCenter - center).sqrMagnitude > 0.8f;
            if (updateShape)
            {
                for (int i = 0; i < Segments; i++)
                {
                    float angle = i * (2f * Mathf.PI / Segments);
                    Vector3 point = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * zone.Radius;
                    if (PhysicsUtil.findFloor(point, out Vector3 ground))
                        point = ground;
                    RingPoints[i] = point + Vector3.up * 0.5f;
                }
                ring.SetPositions(RingPoints);
                lastCenter = center;
                lastRadius = radius;
            }

            Color color = valid ? (locked ? Color.green : Color.cyan) : Color.red;
            if (!ring.enabled || color != lastColor || updateShape)
            {
                RingColor.SetColor("_Color", color);
                ring.SetPropertyBlock(RingColor);
                lastColor = color;
            }
            ring.enabled = true;
        }

        private static void ScheduleSave(FlattenZone zone, ref string savePath)
        {
            pendingSave = SaveData.getSavePath();
            TerrainEdits.For(Singleton<TerrainGenerator>.getInstance()).Add(zone);
            savePath = pendingSave;
            TerrainAndSpeedPlugin.Log.LogInfo($"Zona confirmada en {zone.Center.x:F1}, {zone.Center.y:F1}, radio {zone.Radius:F0} m.");
            Close();
        }

        [HarmonyPatch(typeof(GameStateGame), nameof(GameStateGame.onEndOfFrame))]
        private static class ReloadTerrain
        {
            private static void Postfix()
            {
                string path = pendingSave;
                if (path == null)
                    return;
                pendingSave = null;
                if (File.Exists(path))
                {
                    int planet = Singleton<PlanetManager>.getInstance().getCurrentPlanetIndex();
                    GameManager.getInstance().setGameStateGameLoad(path, planet, null);
                    TerrainAndSpeedPlugin.Log.LogInfo("Recargando una nueva ranura con la zona confirmada.");
                }
                else
                {
                    List<FlattenZone> zones = TerrainEdits.For(Singleton<TerrainGenerator>.getInstance());
                    zones.RemoveAt(zones.Count - 1);
                    Resume();
                    TerrainAndSpeedPlugin.Log.LogWarning("No se pudo guardar la zona. El terreno sigue igual.");
                }
            }
        }

        [HarmonyPatch(typeof(GameStateGame), nameof(GameStateGame.destroy))]
        private static class ClearSelection
        {
            private static void Prefix()
            {
                Close();
                pendingSave = null;
                resumeOnClose = false;
            }
        }

        [HarmonyPatch(typeof(GameStateGame), nameof(GameStateGame.onGui))]
        private static class RenderInstructions
        {
            private static void Postfix()
            {
                if ((!selecting && pendingSave == null) || Event.current.type != EventType.Repaint)
                    return;

                float width = Mathf.Min(585f, Screen.width - 30f);
                float x = (Screen.width - width) * 0.5f;
                float y = Screen.height - 152f;
                GUI.Box(new Rect(x, y, width, 100f), GUIContent.none);
                string action = selectedCenter.HasValue
                    ? "Intro guarda en una ranura nueva y recarga el terreno."
                    : "Haz clic sobre el terreno para fijar el centro.";
                GUI.Label(new Rect(x + 14f, y + 9f, width - 28f, 22f), $"Aplanar zona · Radio {radius} m");
                GUI.Label(new Rect(x + 14f, y + 34f, width - 28f, 22f), pendingSave != null ? "Guardando…" : hint ?? action);
                GUI.Label(new Rect(x + 14f, y + 61f, width - 28f, 22f),
                    "RePág / AvPág ajusta el radio · Clic derecho suelta el centro · Esc cancela");
            }
        }
    }
}
