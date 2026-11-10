using System;
using System.Collections.Generic;
using System.Linq;
using Planetbase;
using UnityEngine;

namespace PlanetbaseResourceMenu
{
    internal static class ResourceDelivery
    {
        internal const int MaxAmount = 100;

        internal static string Add(ResourceType type, int amount)
        {
            if (amount < 1 || amount > MaxAmount)
                throw new ArgumentOutOfRangeException(nameof(amount));

            int before = Resource.getCountOfType(type);
            if (type.isInmaterial())
            {
                if (before > int.MaxValue - amount)
                    return "El saldo alcanzaría su límite.";

                Resource.addInmaterialResource(new ResourceAmount(type, amount));
                LogResult(type, before, $"saldo +{amount}");
                return $"Agregaste {amount} al saldo.";
            }

            int stored = 0;
            int outside = 0;
            bool deliveryError = false;
            try
            {
                var storages = Module.getCategoryModules(Module.Category.Storage);
                if (storages != null)
                {
                    foreach (Module storage in storages)
                    {
                        if (stored == amount)
                            break;
                        if (storage.isDestroyed() || !storage.isOperational())
                            continue;

                        Vector3 origin = storage.getPosition();
                        while (stored < amount)
                        {
                            StorageSlot slot = storage.findStorageSlot(origin);
                            if (slot == null)
                                break;

                            Resource resource = Resource.create(type, slot.getPosition(), Location.Interior);
                            slot.addResource(resource);
                            stored++;
                        }
                    }
                }

                if (stored < amount)
                    DropOutside(type, amount - stored, ref outside);
            }
            catch (Exception error)
            {
                deliveryError = true;
                ResourceMenuPlugin.Log.LogError(error);
            }

            int added = Resource.getCountOfType(type) - before;
            LogResult(type, before, $"almacén {stored}, exterior {outside}");
            if (deliveryError)
                return $"La entrega se interrumpió. El inventario aumentó en {added}.";
            if (added == 0)
                return "No hay espacio en almacenes ni terreno accesible cerca.";
            if (added < amount)
                return $"Agregaste {added} de {amount}. Ya no hay espacio.";
            return $"Agregaste {added}. Almacén {stored} · Exterior {outside}.";
        }

        private static void DropOutside(ResourceType type, int amount, ref int delivered)
        {
            var airlocks = Module.getCategoryModules(Module.Category.Airlock);
            Module airlock = airlocks?.Find(module => !module.isDestroyed() && module.isOperational());
            ColonyShip ship = Ship.getFirstOfType<ColonyShip>();
            Character colonist = Character.getFirstCharacter();
            Vector3 anchor;
            if (airlock != null)
                anchor = airlock.getPosition();
            else if (ship != null && ship.isDone())
                anchor = ship.getPosition();
            else if (Construction.getCount() > 0)
                anchor = Construction.getAveragePosition();
            else if (colonist != null)
                anchor = colonist.getPosition();
            else
                return;

            const int resourceLayer = 1 << 10;
            foreach (NavigationNode node in FindExteriorNodes(anchor))
            {
                if (delivered >= amount)
                    break;
                if (!PhysicsUtil.findFloor(node.getPosition(), out Vector3 ground))
                    continue;
                if (Physics.CheckSphere(ground + Vector3.up * 0.45f, 0.65f, resourceLayer))
                    continue;

                Resource resource = Resource.create(type, ground, Location.Exterior);
                resource.drop(Resource.State.Idle);
                delivered++;
            }
        }

        private static IEnumerable<NavigationNode> FindExteriorNodes(Vector3 anchor)
        {
            const float radius = 28f;
            NavigationGraph graph = NavigationGraph.getExterior();
            var extent = new Vector3(radius, 0f, radius);
            Vector2i min = graph.getCellIndices(anchor - extent);
            Vector2i max = graph.getCellIndices(anchor + extent);
            var candidates = new List<NavigationNode>();
            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    foreach (NavigationNode node in graph.getCell(x, y).getNodes())
                    {
                        if (node.isActive() && node.getColorIndex() >= 0 &&
                            (node.getPosition() - anchor).sqrMagnitude <= radius * radius)
                            candidates.Add(node);
                    }
                }
            }

            return candidates.OrderBy(node => (node.getPosition() - anchor).sqrMagnitude);
        }

        private static void LogResult(ResourceType type, int before, string destinations)
        {
            int after = Resource.getCountOfType(type);
            ResourceMenuPlugin.Log.LogInfo($"Entrega {type.GetType().Name}: {before} -> {after} ({destinations}).");
        }
    }
}
