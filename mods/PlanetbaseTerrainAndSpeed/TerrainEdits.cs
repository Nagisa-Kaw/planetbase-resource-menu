using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using HarmonyLib;
using Planetbase;
using UnityEngine;

namespace PlanetbaseTerrainAndSpeed
{
    internal readonly struct FlattenZone
    {
        internal const int MinRadius = 20;
        internal const int MaxRadius = 120;

        internal FlattenZone(Vector3 center, float radius)
        {
            Center = new Vector2(center.x, center.z);
            Radius = radius;
        }

        internal Vector2 Center { get; }
        internal float Radius { get; }

        internal bool IsValid()
        {
            Vector2 mapCenter = Vector2.one * 1000f;
            return Radius >= MinRadius && Radius <= MaxRadius &&
                (Center - mapCenter).sqrMagnitude <= 375f * 375f;
        }

        internal bool Contains(Vector3 point)
        {
            return (new Vector2(point.x, point.z) - Center).sqrMagnitude < Radius * Radius;
        }
    }

    internal static class TerrainEdits
    {
        internal const int MaxZones = 64;
        private const string EditsTag = "planetbase-terrain-speed";
        private const float WorldSize = 2000f;
        private const float TerrainHeight = 150f;
        private const float BorderWidth = 10f;
        private static readonly ConditionalWeakTable<TerrainGenerator, List<FlattenZone>> Edits =
            new ConditionalWeakTable<TerrainGenerator, List<FlattenZone>>();

        internal static List<FlattenZone> For(TerrainGenerator terrain) => Edits.GetOrCreateValue(terrain);

        [HarmonyPatch(typeof(TerrainGenerator), nameof(TerrainGenerator.serialize))]
        private static class WriteEdits
        {
            private static void Postfix(TerrainGenerator __instance, XmlNode parent, string name)
            {
                List<FlattenZone> zones = For(__instance);
                if (zones.Count == 0)
                    return;

                XmlElement edits = parent.OwnerDocument.CreateElement(EditsTag);
                parent[name].AppendChild(edits);
                foreach (FlattenZone zone in zones)
                {
                    XmlElement item = parent.OwnerDocument.CreateElement("zone");
                    item.SetAttribute("x", XmlConvert.ToString(zone.Center.x));
                    item.SetAttribute("z", XmlConvert.ToString(zone.Center.y));
                    item.SetAttribute("radius", XmlConvert.ToString(zone.Radius));
                    edits.AppendChild(item);
                }
            }
        }

        [HarmonyPatch(typeof(TerrainGenerator), nameof(TerrainGenerator.deserialize))]
        private static class ReadEdits
        {
            private static void Postfix(TerrainGenerator __instance, XmlNode node)
            {
                List<FlattenZone> zones = For(__instance);
                zones.Clear();
                var edits = node?[EditsTag] as XmlElement;
                if (edits == null)
                    return;

                foreach (XmlNode child in edits.ChildNodes)
                {
                    if (zones.Count == MaxZones)
                        break;
                    if (child is XmlElement item && item.Name == "zone" &&
                        TryRead(item, "x", out float x) && TryRead(item, "z", out float z) &&
                        TryRead(item, "radius", out float radius))
                    {
                        var zone = new FlattenZone(new Vector3(x, 0f, z), radius);
                        if (zone.IsValid())
                            zones.Add(zone);
                    }
                }
            }

            private static bool TryRead(XmlElement item, string key, out float value) =>
                float.TryParse(item.GetAttribute(key), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        [HarmonyPatch(typeof(TerrainGenerator), "smooth")]
        private static class ApplyEdits
        {
            private static void Postfix(TerrainGenerator __instance, float[,] ___mHeights, float[,,] ___mAlphas)
            {
                List<FlattenZone> zones = For(__instance);
                if (zones.Count == 0)
                    return;

                float cellSize = WorldSize / (___mHeights.GetLength(0) - 1);
                var multipliers = new float[___mHeights.GetLength(0), ___mHeights.GetLength(1)];
                foreach (FlattenZone zone in zones)
                    MarkZone(multipliers, zone, cellSize);

                float floor = __instance.getFloorHeight() / TerrainHeight;
                int lowered = 0;
                float peakRemoved = 0f;
                for (int x = 0; x < ___mHeights.GetLength(0); x++)
                {
                    for (int z = 0; z < ___mHeights.GetLength(1); z++)
                    {
                        float weight = multipliers[x, z];
                        float heightAboveFloor = ___mHeights[x, z] - floor;
                        if (weight <= 0f || heightAboveFloor <= 0f)
                            continue;
                        float removed = heightAboveFloor * weight;
                        ___mHeights[x, z] -= removed;
                        BlendGroundTexture(___mAlphas, x, z, weight);
                        peakRemoved = Math.Max(peakRemoved, removed);
                        lowered++;
                    }
                }

                TerrainAndSpeedPlugin.Log.LogInfo($"Zonas cargadas: {zones.Count}. Puntos rebajados: {lowered}. " +
                    $"Reducción máxima: {peakRemoved * TerrainHeight:F1} m.");
            }

            private static void BlendGroundTexture(float[,,] alphas, int x, int z, float weight)
            {
                if (x >= alphas.GetLength(0) || z >= alphas.GetLength(1))
                    return;

                for (int layer = 1; layer < alphas.GetLength(2); layer++)
                {
                    float shifted = alphas[x, z, layer] * weight;
                    alphas[x, z, layer] -= shifted;
                    alphas[x, z, TerrainGenerator.TextureFlat1] += shifted;
                }
            }

            private static void MarkZone(float[,] multipliers, FlattenZone zone, float cellSize)
            {
                // Leave enough room for the terrain mesh to interpolate at the edge.
                float radius = zone.Radius - cellSize * 2f;
                int minX = Mathf.Max(0, Mathf.CeilToInt((zone.Center.x - radius) / cellSize));
                int minZ = Mathf.Max(0, Mathf.CeilToInt((zone.Center.y - radius) / cellSize));
                int maxX = Mathf.Min(multipliers.GetLength(0) - 1, Mathf.FloorToInt((zone.Center.x + radius) / cellSize));
                int maxZ = Mathf.Min(multipliers.GetLength(1) - 1, Mathf.FloorToInt((zone.Center.y + radius) / cellSize));
                for (int x = minX; x <= maxX; x++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        float distance = (new Vector2(x * cellSize, z * cellSize) - zone.Center).magnitude;
                        float weight = Mathf.SmoothStep(0f, 1f, (radius - distance) / BorderWidth);
                        multipliers[x, z] = Mathf.Max(multipliers[x, z], weight);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(GameStateGame), nameof(GameStateGame.stageLoadGameEnd))]
        private static class SettleExteriorObjects
        {
            private static readonly FieldInfo Characters = AccessTools.Field(typeof(Character), "mCharacters");
            private static readonly FieldInfo Resources = AccessTools.Field(typeof(Resource), "mResources");

            private static void Postfix()
            {
                List<FlattenZone> zones = For(Singleton<TerrainGenerator>.getInstance());
                if (zones.Count == 0)
                    return;

                foreach (Character character in (List<Character>)Characters.GetValue(null))
                {
                    if (character.getLocation() == Location.Exterior &&
                        TryFindLowerGround(character.getPosition(), zones, out Vector3 ground))
                        character.setPosition(ground);
                }
                foreach (Resource resource in (List<Resource>)Resources.GetValue(null))
                {
                    if (!resource.isDestroyed() && !resource.isEmbedded() &&
                        resource.getLocation() == Location.Exterior &&
                        resource.getState() == Resource.State.Idle &&
                        TryFindLowerGround(resource.getPosition(), zones, out Vector3 ground))
                    {
                        resource.setPosition(ground);
                        resource.drop(Resource.State.Idle);
                    }
                }
            }

            private static bool TryFindLowerGround(Vector3 position, List<FlattenZone> zones, out Vector3 ground)
            {
                ground = position;
                foreach (FlattenZone zone in zones)
                {
                    if (zone.Contains(position))
                        return PhysicsUtil.findFloor(position, out ground) && position.y - ground.y > 0.5f;
                }
                return false;
            }
        }
    }
}
