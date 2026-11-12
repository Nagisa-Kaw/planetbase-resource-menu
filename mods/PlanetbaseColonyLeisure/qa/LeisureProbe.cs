using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using BepInEx;
using HarmonyLib;
using Planetbase;
using PlanetbaseColonyLeisure;
using UnityEngine;
using Module = Planetbase.Module;

[BepInPlugin("local.planetbase.leisure.qa", "Leisure QA", "1.0.0")]
[BepInDependency("local.planetbase.colony-leisure")]
public sealed class LeisureProbe : BaseUnityPlugin
{
    private static LeisureProbe probe;
    private static int step;
    private static int checks;
    private static float captureAt;
    private static string savePath;
    private static int savedBalance;
    private static int[] roomIds;
    private static Vector3 sceneCenter;
    private static readonly FieldInfo Indicators = AccessTools.Field(typeof(Character), "mIndicators");
    private static readonly FieldInfo Characters = AccessTools.Field(typeof(Character), "mCharacters");
    private static readonly FieldInfo Interactions = AccessTools.Field(typeof(Interaction), "mInteractions");

    private void Awake()
    {
        probe = this;
        Application.runInBackground = true;
        new Harmony(Info.Metadata.GUID).PatchAll(typeof(LeisureProbe).Assembly);
    }

    [HarmonyPatch(typeof(ServerInterfaceSteam), nameof(ServerInterfaceSteam.isAvailable))]
    private static class Offline
    {
        private static bool Prefix(ref bool __result) { __result=false; return false; }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.update))]
    private static class Drive
    {
        private static void Postfix(GameManager __instance, ref GameManager.State ___mState)
        {
            try
            {
                GameState state = __instance.getGameState();
                if (step == 0 && state is GameStateTitle && ___mState == GameManager.State.Updating)
                {
                    step = 1;
                    __instance.setGameStateGameLoad(Path.Combine(SaveData.FolderName, "save1.sav"), 0, null);
                    return;
                }
                if (state is GameStateGame game && ___mState == GameManager.State.Waiting)
                {
                    game.onStartUpdating();
                    ___mState = GameManager.State.Updating;
                    Singleton<TimeManager>.getInstance().pause();
                    if (step == 1)
                    {
                        step = 2;
                        BuildAndExercise(game);
                    }
                    else if (step == 3)
                    {
                        step = 4;
                        VerifyReload();
                    }
                }
            }
            catch (Exception error)
            {
                step = 99;
                probe.Logger.LogError("LEISURE QA FAILED: " + error);
            }
        }
    }

    private static void BuildAndExercise(GameStateGame game)
    {
        var menuSystem = (GuiMenuSystem)AccessTools.Field(typeof(GameStateGame), "mMenuSystem").GetValue(game);
        var menu = (GuiMenu)AccessTools.Field(typeof(GuiMenuSystem), "mMenuBuildInterior").GetValue(menuSystem);
        Require(menu.getItemCount() == TypeList<ModuleType, ModuleTypeList>.get().Count(t => !t.isExterior()) + 1, "native build menu");
        ModuleType[] types = {
            TypeList<ModuleType, ModuleTypeList>.find<ModuleTypeArcade>(),
            TypeList<ModuleType, ModuleTypeList>.find<ModuleTypeNightClub>(),
            TypeList<ModuleType, ModuleTypeList>.find<ModuleTypeCasino>(),
            TypeList<ModuleType, ModuleTypeList>.find<ModuleTypeWellness>()
        };
        Require(new[] { "Arcade", "NightClub", "Casino", "Wellness", "ArcadeCabinet", "DanceFloor", "CasinoTerminal", "MeditationSeat" }.All(id => HelpManager.getInstance().findItem(id) != null), "native contextual help");
        Require(types.All(t => t != null && !t.getName().Contains("ModuleType")), "localized building registration");

        TerrainGenerator terrain = Singleton<TerrainGenerator>.getInstance();
        terrain.flatten(new Vector3(1215f, 0f, 1125f), 98f, terrain.getFloorHeight());
        terrain.clearArea(new Vector3(1215f, 0f, 1125f), 98f);
        Physics.SyncTransforms();
        var rooms = new List<Module>();
        Module previous = Module.findOfType(TypeList<ModuleType, ModuleTypeList>.find<ModuleTypeCanteen>());
        Require(previous != null, "starting base");
        for (int i = 0; i < types.Length; i++)
        {
            Vector3 p = new Vector3(1185f + 25f * i, 0f, 1123.5f);
            Module room = Module.create(p, 1, types[i]);
            Physics.SyncTransforms();
            Require(room.canPlaceModule(p, Vector3.up, Module.ValidSizes[1]), "valid native placement " + types[i].GetType().Name);
            room.onUserPlaced();
            Finish(room, true);
            Require(Connection.canLink(previous, room), "native corridor clearance " + i);
            Connection link = Connection.create(previous, room);
            link.onUserPlaced();
            Finish(link, true);
            room.setRenderTop(false);
            Supply(room);
            Require(room.isOperational(), "powered room " + i);
            rooms.Add(room);
            previous = room;
        }
        roomIds = rooms.Select(r => r.getId()).ToArray();
        sceneCenter = rooms[1].getPosition();

        ConstructionComponent arcade = Equip<ArcadeCabinet>(rooms[0], new Vector3(-2f,0.2f,-2f));
        ConstructionComponent secondArcade = Equip<ArcadeCabinet>(rooms[0], new Vector3(2f,0.2f,-2f));
        ConstructionComponent dance = Equip<DanceFloor>(rooms[1], new Vector3(0f,0.2f,-2f));
        ConstructionComponent casino = Equip<CasinoTerminal>(rooms[2], new Vector3(0f,0.2f,-2f));
        ConstructionComponent wellness = Equip<MeditationSeat>(rooms[3], new Vector3(0f,0.2f,-2f));
        foreach (Module room in rooms)
            Supply(room);

        List<Character> people = ((List<Character>)Characters.GetValue(null))
            .Where(c => c is Human && !c.isDead() && !c.isLoaded()).Take(3).ToList();
        Require(people.Count == 3, "available adult colonists");
        foreach (Character person in people)
        {
            AccessTools.Method(typeof(Character), "destroyInteractions").Invoke(person, null);
            person.setIdle();
            AccessTools.Method(typeof(Character), "setLocation").Invoke(person, new object[] { Location.Interior });
        }
        Character actor = people[0];
        Character partner = people[1];

        PutAt(actor, arcade, 0.2f);
        Require(new AiRuleGoRelax(IndicatorLevel.Low).update(actor), "AI chooses leisure without a custom command");
        actor.setIdle();
        float start = Value(actor, CharacterIndicator.Morale);
        Interaction solo = Interaction.create<InteractionComponentRestore>(actor, arcade);
        Require(!solo.update(10f), "arcade session");
        float soloGain = Value(actor, CharacterIndicator.Morale) - start;
        PutAt(partner, secondArcade, 0.2f);
        Interaction coPlayer = Interaction.create<InteractionComponentRestore>(partner, secondArcade);
        float beforeCoop = Value(actor, CharacterIndicator.Morale);
        solo.update(10f);
        float coopGain = Value(actor, CharacterIndicator.Morale) - beforeCoop;
        Require(Mathf.Abs(coopGain / soloGain - 1.5f) < 0.01f, "cooperative morale bonus");
        solo.destroy();
        coPlayer.destroy();

        PutAt(actor, dance, 0.2f);
        Set(actor, CharacterIndicator.Sleep, 0.8f);
        Interaction party = Interaction.create<InteractionComponentRestore>(actor, dance);
        party.update(10f);
        Require(Value(actor, CharacterIndicator.Morale) > 0.26f && Value(actor, CharacterIndicator.Sleep) < 0.8f, "club morale and fatigue");
        Set(actor, CharacterIndicator.Sleep, 0.05f);
        Require(party.update(1f), "sleepy guest leaves club");
        party.destroy();

        PutAt(actor, wellness, 0.2f);
        Set(actor, CharacterIndicator.Sleep, 0.3f);
        Interaction meditation = Interaction.create<InteractionComponentRestore>(actor, wellness);
        meditation.update(30f);
        Require(Value(actor, CharacterIndicator.Morale) > 0.29f && Value(actor, CharacterIndicator.Sleep) > 0.34f, "wellness restores two needs");
        meditation.destroy();

        rooms[2].setPower(false);
        Require(!casino.isOperational(), "equipment needs power");
        Supply(rooms[2]);
        int balance = Resource.getCountOfType(ResourceTypeList.CoinsInstance);
        if (balance > 0)
            Resource.removeInmaterialResource(new ResourceAmount(ResourceTypeList.CoinsInstance, balance));
        Require(casino.getPotentialUserCount(actor) == int.MaxValue, "AI avoids casino without funds");
        PutAt(actor, casino, 0.2f);
        Interaction unfunded = Interaction.create<InteractionComponentRestore>(actor, casino);
        Require(unfunded.update(1f) && Mathf.Abs(Value(actor, CharacterIndicator.Morale) - 0.2f) < 0.001f, "no free casino session");
        unfunded.destroy();

        Resource.addInmaterialResource(new ResourceAmount(ResourceTypeList.CoinsInstance, 100));
        PutAt(actor, casino, 0.2f);
        Interaction cancelled = Interaction.create<InteractionComponentRestore>(actor, casino);
        cancelled.update(10f);
        Require(Resource.getCountOfType(ResourceTypeList.CoinsInstance) == 98, "casino charges exactly once");
        cancelled.destroy();
        Require(Resource.getCountOfType(ResourceTypeList.CoinsInstance) == 100, "interrupted bet refunded");
        PutAt(actor, casino, 0.2f);
        Interaction round = Interaction.create<InteractionComponentRestore>(actor, casino);
        round.update(10f);
        Require(Resource.getCountOfType(ResourceTypeList.CoinsInstance) == 98, "mid-round balance");
        savedBalance = 98;

        CameraManager.getInstance().focusOnPosition(sceneCenter, 35f);
        savePath = SaveData.getSavePath();
        AccessTools.Field(typeof(GameStateGame), "mSavePath").SetValue(game, savePath);
        probe.Logger.LogInfo("LEISURE QA native build, AI and mechanics passed. Saving mid-round.");
    }

    private static void Finish(Buildable target, bool alreadyPlaced = false)
    {
        if (!alreadyPlaced)
            target.onUserPlaced();
        ResourceAmounts cost = target.calculateCost();
        foreach (ResourceType type in TypeList<ResourceType, ResourceTypeList>.get())
        {
            for (int i = 0; i < cost.getAmount(type); i++)
                target.addConstructionMaterial(Resource.create(type, target.getPosition(), target.getBuildLocation()));
        }
        target.startBuilding();
        Require(target.updateBuild(1000f) && target.isBuilt(), "native construction completed");
    }

    private static ConstructionComponent Equip<T>(Module room, Vector3 offset)
    {
        ComponentType type = TypeList<ComponentType, ComponentTypeList>.find<T>();
        var c = ConstructionComponent.create(room, type);
        c.setPosition(room.getPosition() + offset);
        c.setRotation(Quaternion.identity);
        bool valid = room.canPlaceComponent(c);
        for(int x=-4; x<=4 && !valid; x++)
        {
            for(int z=-4; z<=4 && !valid; z++)
            {
                c.setPosition(room.getFloorPosition() + new Vector3(x*0.5f,0f,z*0.5f));
                valid=room.canPlaceComponent(c);
            }
        }
        Require(valid, "native equipment placement " + typeof(T).Name);
        room.getComponents().Add(c);
        Finish(c);
        Require(c.getHelpId() == typeof(T).Name && HelpManager.getInstance().findItem(c.getHelpId()) != null, "equipment contextual help");
        Require(c.getAnchorPoint() != null, "usable equipment anchor " + typeof(T).Name);
        return c;
    }

    private static void Supply(Module room)
    {
        room.setPower(true);
        room.setHasWater(true);
        room.getOxygenIndicator().setValue(1f);
    }

    private static void PutAt(Character person, ConstructionComponent c, float morale)
    {
        AccessTools.Method(typeof(Character), "destroyInteractions").Invoke(person, null);
            person.setIdle();
        person.setPosition(c.getAnchorPoint().position);
        Set(person, CharacterIndicator.Morale, morale);
        Set(person, CharacterIndicator.Nutrition, 1f);
        Set(person, CharacterIndicator.Hydration, 1f);
        Set(person, CharacterIndicator.Oxygen, 1f);
        Set(person, CharacterIndicator.Sleep, 1f);
    }

    private static void Set(Character person, CharacterIndicator index, float value) =>
        ((Indicator[])Indicators.GetValue(person))[(int)index].setValue(value);
    private static float Value(Character person, CharacterIndicator index) =>
        ((Indicator[])Indicators.GetValue(person))[(int)index].getValue();

    [HarmonyPatch(typeof(GameStateGame), nameof(GameStateGame.onEndOfFrame))]
    private static class Reload
    {
        private static void Postfix()
        {
            try
            {
                if ((step == 4 || step == 5) && Time.realtimeSinceStartup >= captureAt)
                {
                    string output = Path.Combine(Path.GetDirectoryName(SaveData.FolderName), step == 4 ? "leisure-overview.jpg" : "leisure-club.jpg");
                    File.WriteAllBytes(output, Util.captureScreenshot(720));
                    if (step == 4)
                    {
                        step = 5;
                        CameraManager.getInstance().focusOnPosition(sceneCenter, 17f);
                        captureAt = Time.realtimeSinceStartup + 3f;
                    }
                    else { step = 6; probe.Logger.LogInfo("LEISURE QA previews saved: " + output); }
                    return;
                }
                if (step != 2 || !File.Exists(savePath))
                    return;
                var xml = new XmlDocument();
                xml.Load(savePath);
                Require(xml.SelectNodes("//colony-leisure-casino[@paid='true'][@settled='false']").Count == 1, "persisted wager state");
                step = 3;
                GameManager.getInstance().setGameStateGameLoad(savePath, 0, null);
            }
            catch(Exception e)
            {
                step=99;
                probe.Logger.LogError("LEISURE QA FAILED: " + e);
            }
        }
    }

    [HarmonyPatch(typeof(GameStateGame), nameof(GameStateGame.isEndOfFramePending))]
    private static class PreviewFrame
    {
        private static void Postfix(ref bool __result)
        {
            if (step == 4 || step == 5) __result = true;
        }
    }

    [HarmonyPatch(typeof(Construction), nameof(Construction.setRenderTop))]
    private static class PreviewInterior
    {
        private static void Prefix(Construction __instance, ref bool __0)
        {
            if(__instance is Module m && m.getModuleType() is LeisureDome) __0 = false;
        }
    }

    private static void VerifyReload()
    {
        List<Module> rooms = ((List<Construction>)AccessTools.Field(typeof(Construction), "mConstructions").GetValue(null)).OfType<Module>()
            .Where(m => m.getModuleType() is LeisureDome).ToList();
        Require(rooms.Count == 4 && rooms.Select(r => r.getId()).OrderBy(id=>id).SequenceEqual(roomIds.OrderBy(id=>id)), "buildings survive reload");
        foreach (Module room in rooms)
            Supply(room);
        Require(rooms.Sum(m=>m.getComponents().Count) == 5, "equipment survives reload");
        Require(Resource.getCountOfType(ResourceTypeList.CoinsInstance) == savedBalance, "reload does not duplicate or refund wager");
        Interaction round = ((List<Interaction>)Interactions.GetValue(null)).Single(i =>
            i.getSelectable() is ConstructionComponent c && c.getComponentType() is CasinoTerminal);
        Set(round.getCharacter(), CharacterIndicator.Morale, 0.2f);
        round.update(20f);
        int settledBalance = Resource.getCountOfType(ResourceTypeList.CoinsInstance);
        int prize = settledBalance - savedBalance;
        Require(prize == 0 || prize == 2 || prize == 6, "resumed round pays valid prize");
        round.update(0.1f);
        Require(Resource.getCountOfType(ResourceTypeList.CoinsInstance) == settledBalance, "prize paid once");
        round.destroy();
        Require(Resource.getCountOfType(ResourceTypeList.CoinsInstance) == settledBalance, "settled round is not refunded");
        foreach (Module room in rooms) room.setRenderTop(false);
        CameraManager.getInstance().focusOnPosition((rooms[0].getPosition() + rooms[3].getPosition()) * 0.5f, 95f);
        captureAt = Time.realtimeSinceStartup + 3f;
        probe.Logger.LogInfo($"LEISURE QA PASSED: {checks} checks; four native buildings, five furnishings, AI, morale, fatigue, betting and save/reload.");
    }

    private static void Require(bool ok, string label)
    {
        if(!ok) throw new InvalidOperationException(label);
        checks++;
    }
}






