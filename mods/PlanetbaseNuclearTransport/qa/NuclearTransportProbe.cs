using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using BepInEx;
using HarmonyLib;
using Planetbase;
using PlanetbaseNuclearTransport;
using UnityEngine;
using Module = Planetbase.Module;

[BepInPlugin("local.planetbase.nuclear-transport.qa", "Nuclear Transport QA", "1.0.0")]
[BepInDependency("local.planetbase.nuclear-transport")]
public sealed class NuclearTransportProbe : BaseUnityPlugin
{
    private static NuclearTransportProbe probe;
    private static int step, checks, reactorId, garageId, actorId, fuelCount;
    private static float remaining, captureAt;
    private static string savePath;
    private static Vector3 center, driverPosition;
    private static readonly FieldInfo Characters = AccessTools.Field(typeof(Character), "mCharacters");
    private static readonly FieldInfo Indicators = AccessTools.Field(typeof(Character), "mIndicators");

    private void Awake()
    {
        probe = this;
        Application.runInBackground = true;
        new Harmony(Info.Metadata.GUID).PatchAll(typeof(NuclearTransportProbe).Assembly);
    }
    [HarmonyPatch(typeof(ServerInterfaceSteam), nameof(ServerInterfaceSteam.isAvailable))]
    private static class Offline
    {
        private static bool Prefix(ref bool __result) { __result = false; return false; }
    }
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.update))]
    private static class Drive
    {
        private static void Postfix(GameManager __instance, ref GameManager.State ___mState)
        {
            try
            {
                var state = __instance.getGameState();
                if (step == 0 && state is GameStateTitle && ___mState == GameManager.State.Updating)
                {
                    Require(Util.getFilesFolder().Contains("PlanetbaseResourceMenu-qa"), "isolated QA profile");
                    Require(Singleton<ServerInterfaceManager>.getInstance().getInterfaceName().Contains("Offline"), "offline achievements");
                    step = 1;
                    __instance.setGameStateGameLoad(Path.Combine(SaveData.FolderName, "save1.sav"), 0, null);
                    return;
                }
                if (!(state is GameStateGame game) || ___mState != GameManager.State.Waiting) return;
                game.onStartUpdating();
                ___mState = GameManager.State.Updating;
                Singleton<TimeManager>.getInstance().pause();
                if (step == 1) { step = 2; Exercise(game); }
                else if (step == 3) { step = 4; ReloadCheck(); }
            }
            catch (Exception error) { Fail(error); }
        }
    }
    private static void Exercise(GameStateGame game)
    {
        Require(HelpManager.getInstance().findItem("NuclearReactor") != null && HelpManager.getInstance().findItem("FuelCentrifuge") != null, "native help");
        Require(TypeList<ResourceType, ResourceTypeList>.find<NuclearFuelRod>().getTypeIndex() == 14, "fuel follows fourteen vanilla resource indexes with Solar also loaded");
        var menus = AccessTools.Field(typeof(GameStateGame), "mMenuSystem").GetValue(game);
        foreach (string name in new[] { "mMenuBuildInterior", "mMenuBuildExterior" })
        {
            var menu = (GuiMenu)AccessTools.Field(typeof(GuiMenuSystem), name).GetValue(menus);
            bool exterior = name.EndsWith("Exterior");
            Require(menu.getItemCount() == TypeList<ModuleType, ModuleTypeList>.get().Count(t => t.isExterior() == exterior) + 1, "native build menu " + name);
        }
        TerrainGenerator terrain = Singleton<TerrainGenerator>.getInstance();
        terrain.flatten(new Vector3(1220, 0, 1130), 110f, terrain.getFloorHeight());
        terrain.clearArea(new Vector3(1220, 0, 1130), 110f);
        Physics.SyncTransforms();
        Module canteen = Module.findOfType(TypeList<ModuleType, ModuleTypeList>.find<ModuleTypeCanteen>());
        Module workshop = Make<ModuleTypeFuelWorkshop>(new Vector3(1185, 0, 1123.5f), 1);
        Link(canteen, workshop);
        Module reactor = Make<ModuleTypeNuclearReactor>(new Vector3(1210, 0, 1123.5f), 0);
        Link(workshop, reactor);
        Module cooler = Make<ModuleTypeCoolingTower>(new Vector3(1235, 0, 1123.5f), 0);
        Link(reactor, cooler);
        Module garage = Make<ModuleTypeRoverGarage>(new Vector3(1260, 0, 1123.5f), 0);
        Link(cooler, garage);
        reactorId = reactor.getId(); garageId = garage.getId(); center = cooler.getPosition();
        new Grid().calculate(workshop);
        foreach (Module m in new[] { workshop, reactor, cooler, garage }) Supply(m);
        Require(reactor.getGrid() == cooler.getGrid() && reactor.getGrid() == workshop.getGrid(), "shared native service grid");
        ConstructionComponent centrifuge = Equip<FuelCentrifuge>(workshop, new Vector3(0, .2f, -2));
        ConstructionComponent store = Equip<NuclearFuelStore>(workshop, new Vector3(-3.5f, .2f, 0));
        Require(centrifuge.getAnchorPoint() != null, "native engineer interaction marker");
        Require(centrifuge.getComponentType().getOperatorSpecialization() == TypeList<Specialization, SpecializationList>.find<Engineer>(), "requires engineer");
        ResourceType fuel = TypeList<ResourceType, ResourceTypeList>.find<NuclearFuelRod>();
        ResourceContainer tank = centrifuge.getResourceContainer();
        tank.add(Resource.create(ResourceTypeList.OreInstance, centrifuge.getPosition(), Location.Interior));
        tank.add(Resource.create(ResourceTypeList.MetalInstance, centrifuge.getPosition(), Location.Interior));
        Character engineer = People().First(c => c.getSpecialization() == TypeList<Specialization, SpecializationList>.find<Engineer>());
        Prepare(engineer, Location.Interior, centrifuge.getAnchorPoint().position);
        Interaction work = Interaction.create<InteractionWork>(engineer, centrifuge);
        int before = Resource.getCountOfType(fuel);
        AccessTools.Method(typeof(ConstructionComponent), "updateProduction").Invoke(centrifuge, new object[] { 600f });
        Require(Resource.getCountOfType(fuel) == before + 1 && tank.getCountOf(ResourceTypeList.OreInstance) == 0 && tank.getCountOf(ResourceTypeList.MetalInstance) == 0, "native production consumes ore and metal, produces exactly one rod");
        work.destroy();
        AccessTools.Method(typeof(ConstructionComponent), "updateProduction").Invoke(centrifuge, new object[] { 600f });
        Require(Resource.getCountOfType(fuel) == before + 1, "no free production without inputs or operator");
        // Run native pickup, routing and delivery AI for the actual produced rod.
        Character hauler = ((List<Character>)Characters.GetValue(null)).First(c => c is Bot && c.hasFlag(16));
        Prepare(hauler, Location.Interior, workshop.getFloorPosition() + new Vector3(2, 0, 0));
        NavigationGraph.update();
        Require(new AiRuleGoLoadResourcesToStore().update(hauler), "AI selects fuel delivery");
        Require(hauler.getIndirectTargetResource()?.getResourceType() == fuel || hauler.getTargetResource()?.getResourceType() == fuel,
            "AI targets the manufactured fuel rod");
        for (int i = 0; i < 1200 && store.getResourceContainer().getCountOf(fuel) == 0; i++)
        {
            hauler.update(.1f);
            if (i % 2 == 0) hauler.tick(.2f);
        }
        Require(store.getResourceContainer().getCountOf(fuel) == 1 && !hauler.isLoaded(), "autonomous pickup, walk and fuel delivery preserve physical inventory");
        hauler.setIdle();
        foreach (Module m in new[] { workshop, reactor, cooler, garage }) Supply(m);
        reactor.updatePowerGeneration();
        Require(reactor.getPowerGeneration() == 0, "reactor starts without a loaded rod");
        reactor.update(0.1f, 0);
        Require(reactor.getPowerGeneration() == 60000 && Math.Abs(NuclearSystem.Remaining(reactor) - 1800f) < .01f, "reactor refuels from buffer and generates 60 kW");
        Require(store.getResourceContainer().getCountOf(fuel) == 0, "rod removed once on refuel");
        reactor.update(15f, 0);
        Require(Math.Abs(NuclearSystem.Remaining(reactor) - 1785f) < .01f, "fuel burn follows simulation time");
        float held = NuclearSystem.Remaining(reactor);
        reactor.update(0f, 0);
        Require(NuclearSystem.Remaining(reactor) == held, "pause preserves fuel");
        cooler.setEnabled(false); reactor.update(10f, 0);
        Require(reactor.getPowerGeneration() == 0 && NuclearSystem.Remaining(reactor) == held, "lost cooling scrams and preserves fuel");
        cooler.setEnabled(true); Supply(cooler); reactor.update(.1f, 0);
        Require(reactor.getPowerGeneration() == 60000, "cooling restoration restarts core");
        reactor.setHasWater(false); reactor.update(10f, 0);
        Require(reactor.getPowerGeneration() == 0 && NuclearSystem.Remaining(reactor) == held, "water loss scrams without burning fuel");
        Supply(reactor); reactor.update(.1f, 0);
        reactor.setEnabled(false); reactor.update(10f, 0);
        Require(reactor.getPowerGeneration() == 0 && NuclearSystem.Remaining(reactor) == held, "manual shutdown preserves fuel");
        reactor.setEnabled(true); Supply(reactor); reactor.update(.1f, 0);
        // A second reactor cannot share the same tower.
        Module second = Make<ModuleTypeNuclearReactor>(new Vector3(1210, 0, 1148.5f), 0);
        Link(reactor, second); new Grid().calculate(reactor);
        foreach (Module m in new[] { workshop, reactor, cooler, garage, second }) Supply(m);
        Require(NuclearSystem.HasCooling(reactor) && !NuclearSystem.HasCooling(second), "one tower serves one reactor deterministically");
        second.setEnabled(false);
        Character[] people = People().Where(c => c != engineer).Take(3).ToArray();
        Require(people.Length == 3, "three transport colonists available");
        driverPosition = new Vector3(1260, terrain.getFloorHeight(), 1108f);
        for (int i = 0; i < 3; i++)
        {
            Prepare(people[i], Location.Exterior, driverPosition + Vector3.right * i * 3);
            people[i].loadResource(Resource.create(ResourceTypeList.MetalInstance, people[i].getPosition(), Location.Exterior));
        }
        garage.setEnabled(false);
        float walking = people[0].getMaxSpeed();
        garage.setEnabled(true); Supply(garage);
        Require(Math.Abs(people[0].getMaxSpeed() - 2f * walking) < .001f, "loaded exterior rover doubles normal speed");
        Require(RoverSystem.HasRover(people[1]) && !RoverSystem.HasRover(people[2]) && RoverSystem.Used(garage) == 2, "two rover capacity, third colono walks");
        people[1].unloadResource(Resource.State.Idle);
        Require(!RoverSystem.HasRover(people[1]) && RoverSystem.HasRover(people[2]), "unloading releases a vehicle for another colonist");
        people[2].unloadResource(Resource.State.Idle);
        people[0].setPosition(garage.getPosition() + Vector3.right * 140f);
        // Existing lease is allowed to finish outside the deployment radius.
        Require(RoverSystem.HasRover(people[0]), "existing lease can finish a long route");
        people[0].unloadResource(Resource.State.Idle); RoverSystem.HasRover(people[0]);
        people[0].loadResource(Resource.create(ResourceTypeList.MetalInstance, people[0].getPosition(), Location.Exterior));
        Require(!RoverSystem.HasRover(people[0]), "no new deployment beyond 120 m");
        people[0].setPosition(driverPosition);
        Require(RoverSystem.HasRover(people[0]), "nearby pickup deploys rover");
        AccessTools.Method(typeof(Character), "setLocation").Invoke(people[0], new object[] { Location.Interior });
        Require(!RoverSystem.HasRover(people[0]), "rover does not enter indoor paths");
        AccessTools.Method(typeof(Character), "setLocation").Invoke(people[0], new object[] { Location.Exterior });
        people[0].setPosition(driverPosition);
        garage.setPower(false);
        Require(!RoverSystem.HasRover(people[0]), "power loss removes rover bonus");
        Supply(garage);
        // Walk along real navigation, carrying the original native crate.
        people[0].startWalking(new Target(driverPosition + Vector3.left * 15f, Location.Exterior));
        Vector3 start = people[0].getPosition();
        for (int i = 0; i < 30; i++) people[0].update(.1f);
        Require(Vector3.Distance(start, people[0].getPosition()) > 5f, "loaded rover advances through native navigation");
        Require(people[0].getGameObject().GetComponent<RoverDriver>() != null && people[0].getLoadedResource() != null, "visible driver keeps real cargo");
        Module site = Module.create(new Vector3(1285, 0, 1123.5f), 0, TypeList<ModuleType, ModuleTypeList>.find<ModuleTypeWaterExtractor>());
        Physics.SyncTransforms();
        Require(site.canPlaceModule(site.getPosition(), Vector3.up, Module.ValidSizes[0]), "native construction delivery destination");
        site.onUserPlaced();
        people[0].startWalking(new Target(site, site.getPosition()));
        for (int i = 0; i < 1000 && people[0].isLoaded(); i++) people[0].update(.1f);
        Require(!people[0].isLoaded() && !site.isAwaitingMaterials(), "rover delivers real construction material through native arrival AI");
        Require(!RoverSystem.HasRover(people[0]), "completed delivery releases rover");
        people[0].loadResource(Resource.create(ResourceTypeList.MetalInstance, people[0].getPosition(), Location.Exterior));
        people[0].setIdle(); people[0].setPosition(driverPosition); people[0].update(0f);
        actorId = people[0].getId();
        remaining = NuclearSystem.Remaining(reactor); fuelCount = Resource.getCountOfType(fuel);
        Require(reactor.getPowerGeneration() > 0 && remaining > 0, "core running before save");
        CameraManager.getInstance().focusOnPosition(center - Vector3.right * 10f, 65f);
        savePath = SaveData.getSavePath();
        AccessTools.Field(typeof(GameStateGame), "mSavePath").SetValue(game, savePath);
        probe.Logger.LogInfo("NUCLEAR QA mechanics passed; saving fueled core and loaded rover: " + savePath);
    }

    private static IEnumerable<Character> People() => ((List<Character>)Characters.GetValue(null)).Where(c => c is Colonist && !c.isDead());
    private static void Prepare(Character c, Location location, Vector3 p)
    {
        AccessTools.Method(typeof(Character), "destroyInteractions").Invoke(c, null);
        if (c.isLoaded()) c.unloadResource(Resource.State.Idle);
        c.setIdle();
        AccessTools.Method(typeof(Character), "setLocation").Invoke(c, new object[] { location });
        c.setPosition(p);
        foreach (Indicator indicator in (Indicator[])Indicators.GetValue(c)) if (indicator != null) indicator.setValue(1f);
    }
    private static Module Make<T>(Vector3 position, int size)
    {
        ModuleType type = TypeList<ModuleType, ModuleTypeList>.find<T>();
        Require(type != null && !type.getName().Contains("ModuleType"), "registered localized " + typeof(T).Name);
        Module m = Module.create(position, size, type);
        Physics.SyncTransforms();
        Require(m.canPlaceModule(position, Vector3.up, Module.ValidSizes[size]), "native placement " + typeof(T).Name);
        Finish(m); return m;
    }
    private static void Link(Module a, Module b)
    {
        Require(Connection.canLink(a, b), "native connection clearance " + b.getName());
        Finish(Connection.create(a, b));
    }
    private static void Finish(Buildable target)
    {
        target.onUserPlaced();
        ResourceAmounts costs = target.calculateCost();
        foreach (ResourceType type in TypeList<ResourceType, ResourceTypeList>.get())
            for (int i = 0; i < costs.getAmount(type); i++)
                target.addConstructionMaterial(Resource.create(type, target.getPosition(), target.getBuildLocation()));
        target.startBuilding();
        Require(target.updateBuild(1000f) && target.isBuilt(), "native materials and construction " + target.getName());
    }
    private static ConstructionComponent Equip<T>(Module room, Vector3 offset)
    {
        var c = ConstructionComponent.create(room, TypeList<ComponentType, ComponentTypeList>.find<T>());
        c.setPosition(room.getPosition() + offset); c.setRotation(Quaternion.identity);
        bool valid = room.canPlaceComponent(c);
        for (int x = -8; x <= 8 && !valid; x++)
            for (int z = -8; z <= 8 && !valid; z++)
            {
                c.setPosition(room.getFloorPosition() + new Vector3(x * .5f, 0, z * .5f));
                valid = room.canPlaceComponent(c);
            }
        Require(valid, "native furniture placement " + typeof(T).Name);
        room.getComponents().Add(c); Finish(c); return c;
    }
    private static void Supply(Module m) { m.setPower(true); m.setHasWater(true); m.getOxygenIndicator().setValue(1f); }

    [HarmonyPatch(typeof(GameStateGame), nameof(GameStateGame.isEndOfFramePending))]
    private static class ForceFrames
    {
        private static void Postfix(ref bool __result) { if (step == 4 || step == 5) __result = true; }
    }
    [HarmonyPatch(typeof(GameStateGame), nameof(GameStateGame.onEndOfFrame))]
    private static class Reload
    {
        private static void Postfix()
        {
            try
            {
                if (step == 2 && File.Exists(savePath))
                {
                    var xml = new XmlDocument(); xml.Load(savePath);
                    Require(xml.SelectNodes("//nuclear-transport-core").Count == 2, "serialized reactor fuel states");
                    step = 3; GameManager.getInstance().setGameStateGameLoad(savePath, 0, null);
                }
                else if ((step == 4 || step == 5) && Time.realtimeSinceStartup >= captureAt)
                {
                    string output = Path.Combine(Path.GetDirectoryName(SaveData.FolderName), step == 4 ? "nuclear-overview.jpg" : "nuclear-rover.jpg");
                    File.WriteAllBytes(output, Util.captureScreenshot(720));
                    if (step == 4)
                    {
                        step = 5; CameraManager.getInstance().focusOnPosition(driverPosition, 13f);
                        captureAt = Time.realtimeSinceStartup + 3f;
                    }
                    else { step = 6; probe.Logger.LogInfo("NUCLEAR QA previews saved: " + output); }
                }
            }
            catch (Exception error) { Fail(error); }
        }
    }
    private static void ReloadCheck()
    {
        Module reactor = NuclearSystem.Modules.Single(m => m.getId() == reactorId);
        Module garage = NuclearSystem.Modules.Single(m => m.getId() == garageId);
        Require(NuclearSystem.Modules.Count(m => m.getModuleType() is ExpansionModule) == 5, "all buildings persist across reload");
        Require(Math.Abs(NuclearSystem.Remaining(reactor) - remaining) < .001f, "remaining fuel persists exactly");
        Require(Resource.getCountOfType(TypeList<ResourceType, ResourceTypeList>.find<NuclearFuelRod>()) == fuelCount, "no duplicated fuel on reload");
        foreach (Module m in NuclearSystem.Modules.Where(m => m.getModuleType() is ExpansionModule)) Supply(m);
        reactor.updatePowerGeneration();
        Require(reactor.getPowerGeneration() == 60000, "reactor restarts after reload with cooling");
        Character actor = People().Single(c => c.getId() == actorId);
        Require(actor.isLoaded() && actor.getLoadedResource().getResourceType() == ResourceTypeList.MetalInstance, "native crate survives reload");
        Require(RoverSystem.HasRover(actor) && RoverSystem.Used(garage) == 1, "rover assignment reconstructs without duplication");
        actor.update(0f);
        Require(actor.getGameObject().GetComponent<RoverDriver>() != null, "rover model recreates after reload");
        Require(NuclearSystem.Modules.Where(m => m.getModuleType() is ModuleTypeFuelWorkshop).Single().getComponents().Count == 2, "fuel equipment persists");
        // Exhaustion and safe shutdown tested after save/reload.
        reactor.update(2000f, 0);
        Require(NuclearSystem.Remaining(reactor) == 0f && reactor.getPowerGeneration() == 0, "exhausted fuel stops generation");
        reactor.update(10f, 0);
        Require(reactor.getPowerGeneration() == 0, "empty buffer cannot refuel for free");
        CameraManager.getInstance().focusOnPosition(center - Vector3.right * 10f, 65f);
        captureAt = Time.realtimeSinceStartup + 3f;
        probe.Logger.LogInfo("NUCLEAR QA PASSED: " + checks + " checks; native construction, fuel production, cooling, transport, navigation, save/reload.");
    }
    private static void Require(bool ok, string label)
    {
        if (!ok) throw new InvalidOperationException(label);
        checks++; probe.Logger.LogInfo("PASS " + checks + ": " + label);
    }
    private static void Fail(Exception error)
    {
        if (step == 99) return;
        step = 99; probe.Logger.LogError("NUCLEAR QA FAILED: " + error);
    }
}
