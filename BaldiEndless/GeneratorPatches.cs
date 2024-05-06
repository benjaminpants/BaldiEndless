using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BaldiEndless
{
    [HarmonyPatch(typeof(BaseGameManager))]
    [HarmonyPatch("LoadNextLevel")]
    class EternallyStuckV2
    {
        static bool Prefix()
        {
            BonusLifeUpgrade._defaultLives.SetValue(Singleton<BaseGameManager>.Instance, 2 + EndlessFloorsPlugin.currentSave.GetUpgradeCount("bonuslife"));
            SceneObject sceneObject = EndlessFloorsPlugin.currentSceneObject;
            EndlessFloorsPlugin.currentSave.currentFloor += 1;
            if ((Singleton<CoreGameManager>.Instance.currentMode == EndlessFloorsPlugin.NNFloorMode) || (Singleton<CoreGameManager>.Instance.currentMode == Mode.Free))
            {
                if (EndlessFloorsPlugin.currentFloorData.FloorID != EndlessFloorsPlugin.Instance.selectedFloor)
                {
                    UnityEngine.Object.Destroy(Singleton<ElevatorScreen>.Instance.gameObject);
                    Singleton<CoreGameManager>.Instance.Quit();
                    return false;
                }
            }
            if (!((Singleton<CoreGameManager>.Instance.currentMode == Mode.Free) || (Singleton<CoreGameManager>.Instance.currentMode == EndlessFloorsPlugin.NNFloorMode)))
            {
                if (EndlessFloorsPlugin.currentFloorData.FloorID > EndlessFloorsPlugin.Instance.highestFloorCount)
                {
                    EndlessFloorsPlugin.Instance.highestFloorCount = EndlessFloorsPlugin.currentFloorData.FloorID;
                    EndlessFloorsPlugin.Instance.SaveHighestFloor();
                }
            }
            /*if (sceneObject.levelTitle.Contains("LAP"))
            {
                EndlessFloorsPlugin.currentSave.currentFloorData.FloorID -= 1;
                EndlessFloorsPlugin.Instance.UpdateData(ref sceneObject);
                return;
            }*/
            EndlessFloorsPlugin.Instance.UpdateData(ref sceneObject);
            /*if (EndlessFloorsPlugin.currentSave.currentFloorData.FloorID % 2 == 0)
            {
                EndlessFloorsPlugin.victoryScene.levelTitle = "LAP1";
                sceneObject.nextLevel = EndlessFloorsPlugin.victoryScene;
            }*/
            return true;
        }
    }

    [HarmonyPatch(typeof(GameLoader))]
    [HarmonyPatch("LoadLevel")]
    class EternallyStuck
    {
        static void Prefix(ref SceneObject sceneObject)
        {
            EndlessFloorsPlugin.Instance.UpdateData(ref sceneObject);
        }
    }


    [HarmonyPatch(typeof(LevelGenerator))]
    [HarmonyPatch("StartGenerate")]
    class GenerateBegin
    {

        static List<T2> CreateWeightedShuffledListWithCount<T, T2>(List<T> list, int count, System.Random rng) where T : WeightedSelection<T2>
        {
            List<T2> newL = new List<T2>();
            List<T> selections = list.ToList();
            for (int i = 0; i < count; i++)
            {
                // this is literally the worst fucking thing that i think anyone has written
                // AND I LITERALLY ONLY END UP USING THIS FUCKING ONCE MEANING MAKING IT GENERICALLY TYPED SERVERS LITERALLY NO PURPOSE. FUCK.
                T2 selectedValue = (T2)AccessTools.Method(typeof(T), "ControlledRandomSelection").Invoke(null, new object[] { selections.ToArray(), rng  });//.ControlledRandomSelectionList(selections, rng);
                selections.RemoveAll(x => object.Equals(x.selection, selectedValue)); //thank you stack overflow for saving my ass
                newL.Add(selectedValue);
            }

            return newL;
        }

        static List<T> CreateShuffledListWithCount<T>(List<T> list, int count, System.Random rng)
        {
            count = Math.Min(list.Count,count);
            List<T> newList = new List<T>();
            List<T> copiedList = list.ToList(); // create a duplicate list
            for (int i = 0; i < count; i++)
            {
                int selectedIndex = rng.Next(0, copiedList.Count);
                newList.Add(copiedList[selectedIndex]);
                copiedList.RemoveAt(selectedIndex);
            }
            return newList;
        }

        static void Prefix(LevelGenerator __instance)
        {
            FloorData currentFD = EndlessFloorsPlugin.currentFloorData;
            GeneratorData genData = new GeneratorData();
            EndlessFloorsPlugin.ExtendGenData(genData);
            EndlessFloorsPlugin.lastGenMaxNpcs = genData.npcs.Count;
            __instance.ld.potentialItems = genData.items.ToArray();
            __instance.ld.maxItemValue = currentFD.maxItemValue;
            __instance.seedOffset = currentFD.FloorID;
            __instance.ld.minSize = new IntVector2(currentFD.minSize, currentFD.minSize);
            __instance.ld.maxSize = new IntVector2(currentFD.maxSize, currentFD.maxSize);
            __instance.ld.previousLevels = new LevelObject[0];

            __instance.ld.timeBonusVal = 1 * currentFD.FloorID;
            __instance.ld.fieldTrip = ((currentFD.FloorID % 4 == 0) && currentFD.FloorID != 4) || currentFD.FloorID % 99 == 0;
            __instance.ld.fieldTrips = genData.fieldTrips.ToArray();
            int avgWeight = 0;
            int heighestWeight = 0;
            for (int i = 0; i < genData.items.Count; i++)
            {
                avgWeight += genData.items[i].weight;
                if (genData.items[i].weight > heighestWeight)
                {
                    heighestWeight = genData.items[i].weight;
                }
            }
            avgWeight /= genData.items.Count;
            //avgWeight = Mathf.CeilToInt(Mathf.Clamp(avgWeight * 2f, heighestWeight / 2f, heighestWeight - 25));
            __instance.ld.fieldTripItems = genData.items.Where(x => (x.weight < avgWeight) && !x.selection.GetMeta().flags.HasFlag(ItemFlags.InstantUse)).Select(x => new WeightedItem() { weight = x.weight, selection = x.selection}).ToList();
            SceneObject floor2 = EndlessFloorsPlugin.Instance.SceneObjects.ToList().Find(x => x.levelTitle == "F2");
            __instance.ld.tripEntrancePre = floor2.levelObject.tripEntrancePre;
            __instance.ld.tripEntranceRoom = floor2.levelObject.tripEntranceRoom;

            __instance.ld.hallWallTexs = EndlessFloorsPlugin.wallTextures.ToArray();
            __instance.ld.classWallTexs = EndlessFloorsPlugin.wallTextures.ToArray();
            __instance.ld.facultyWallTexs = EndlessFloorsPlugin.facultyWallTextures.ToArray();

            __instance.ld.classCeilingTexs = EndlessFloorsPlugin.ceilTextures.ToArray();
            __instance.ld.hallCeilingTexs = EndlessFloorsPlugin.ceilTextures.ToArray();
            __instance.ld.facultyCeilingTexs = EndlessFloorsPlugin.ceilTextures.ToArray();

            __instance.ld.hallFloorTexs = EndlessFloorsPlugin.floorTextures.ToArray();
            __instance.ld.facultyFloorTexs = EndlessFloorsPlugin.profFloorTextures.ToArray();
            __instance.ld.classFloorTexs = EndlessFloorsPlugin.profFloorTextures.ToArray();

            __instance.ld.finalLevel = false; // THERE IS NO END

            System.Random stableRng = new System.Random(Singleton<CoreGameManager>.Instance.Seed());

            stableRng.Next();

            System.Random rng = new System.Random(Singleton<CoreGameManager>.Instance.Seed() + __instance.seedOffset);

            Color warmColor = new Color(255f / 255f, 202 / 255f, 133 / 255f);
            Color coldColor = new Color(133f / 255f, 161f / 255f, 255f / 255f);

            float coldLight = Mathf.Max(Mathf.Sin(((currentFD.FloorID / (1f + (float)(rng.NextDouble() * 15f))) + stableRng.Next(-50, 50))), 0f);
            float warmLight = Mathf.Max(Mathf.Sin(((currentFD.FloorID / (1f + (float)(rng.NextDouble() * 15f))) + stableRng.Next(-50, 50))), 0f);

            __instance.ld.standardLightColor = Color.Lerp(Color.Lerp(Color.white, coldColor, coldLight), warmColor, warmLight);

            if (currentFD.FloorID % 99 == 0)
            {
                __instance.ld.standardLightColor = Color.Lerp(__instance.ld.standardLightColor, Color.red, 0.3f);
            }
            // if only i could remember how this fucking code works
            float rgb = Mathf.Max(16f, 255f - (currentFD.FloorID * 5));
            __instance.ld.standardDarkLevel = new Color(rgb / 255, rgb / 255, rgb / 255);
            __instance.ld.standardLightStrength = Mathf.Max(Mathf.RoundToInt(4f / (currentFD.FloorID / 24f)), 3);
            __instance.ld.maxLightDistance = rng.Next(2, Mathf.Clamp(Mathf.FloorToInt(currentFD.FloorID / 2), 2, 12));

            // npc logic
            __instance.ld.potentialNPCs = new List<WeightedNPC>();
            __instance.ld.additionalNPCs = 0;
            int potNpcs = Mathf.Max(Mathf.Min(currentFD.npcCountUnclamped, genData.npcs.Count),1);
            stableRng = new System.Random(Singleton<CoreGameManager>.Instance.Seed()); //reset stableRng since we are no longer doing light stuff
            stableRng.Next();
            __instance.ld.forcedNpcs = CreateWeightedShuffledListWithCount<WeightedNPC, NPC>(genData.npcs, potNpcs, stableRng).ToArray();
            __instance.ld.forcedNpcs = __instance.ld.forcedNpcs.AddRangeToArray<NPC>(genData.forcedNpcs.ToArray());
            stableRng = new System.Random(Singleton<CoreGameManager>.Instance.Seed());
            stableRng.Next();
            stableRng.Next();

            List<WeightedRoomAsset> wra = CreateShuffledListWithCount(genData.classRoomAssets, 3 + Mathf.FloorToInt(currentFD.FloorID / 3), rng);

            wra.Do((x) =>
            {
                if (x.selection.hasActivity)
                {
                    x.weight = (int)Math.Ceiling(x.weight * (currentFD.FloorID * 0.1));
                }
            });

            __instance.ld.potentialClassRooms = wra.ToArray();

            stableRng = new System.Random(Singleton<CoreGameManager>.Instance.Seed());
            stableRng.Next();
            stableRng.Next();
            stableRng.Next();

            __instance.ld.potentialFacultyRooms = CreateShuffledListWithCount(genData.facultyRoomAssets, 4 + Mathf.FloorToInt(currentFD.FloorID / 4), rng).ToArray();


            __instance.ld.exitCount = currentFD.exitCount;
            __instance.ld.additionTurnChance = (int)Mathf.Clamp((currentFD.unclampedScaleVar / 2), 0f, 35f);
            __instance.ld.minClassRooms = currentFD.classRoomCount;
            __instance.ld.maxClassRooms = currentFD.classRoomCount;
            __instance.ld.windowChance = Mathf.Max((currentFD.FloorID * -1.2f) + 14, 2);
            __instance.ld.mapPrice = currentFD.FloorID * 25;
            __instance.ld.maxPlots = currentFD.maxPlots;
            __instance.ld.minPlots = currentFD.minPlots;
            __instance.ld.outerEdgeBuffer = 3;
            __instance.ld.bridgeTurnChance = Mathf.CeilToInt(Mathf.Clamp(currentFD.exitCount, 1f, 5f) * 3f); //lol
            //this changed... interesting...
            //__instance.ld.itemChance = (int)currentFD.itemChance;

            __instance.ld.maxSideHallsToRemove = Mathf.FloorToInt(currentFD.classRoomCount / 5);
            __instance.ld.minSideHallsToRemove = Mathf.CeilToInt(currentFD.classRoomCount / 7);


            __instance.ld.maxFacultyRooms = currentFD.maxFacultyRoomCount;
            __instance.ld.minFacultyRooms = currentFD.minFacultyRoomCount;

            __instance.ld.maxSpecialBuilders = Mathf.CeilToInt(currentFD.unclampedScaleVar / 11f);
            __instance.ld.minSpecialBuilders = Mathf.CeilToInt((currentFD.unclampedScaleVar / 11f) / 1.5f);

            __instance.ld.maxEvents = Mathf.RoundToInt(currentFD.classRoomCount / 2f);
            __instance.ld.minEvents = Mathf.FloorToInt(currentFD.classRoomCount / 3);

            __instance.ld.randomEvents = genData.randomEvents;
            __instance.ld.maxEvents = Mathf.RoundToInt(currentFD.classRoomCount / 2f);
            __instance.ld.minEvents = Mathf.FloorToInt(currentFD.classRoomCount / 3f);
            // mystman12 put in some code to remove duplicate events, how sad.
            /*while (__instance.ld.maxEvents > (__instance.ld.randomEvents.Count + 5))
            {
                __instance.ld.randomEvents.AddRange(genData.randomEvents);
                Debug.Log("ADDING EXTRAS(potentially dangerous!)");
                Debug.Log(__instance.ld.randomEvents.Count);
            }*/
            __instance.ld.maxEventGap = currentFD.classRoomCount <= 19 ? 130f : 120f;
            __instance.ld.minEventGap = currentFD.classRoomCount >= 14 ? 30f : 60f;
            __instance.ld.maxOffices = Mathf.Max(currentFD.maxOffices, 1);
            __instance.ld.minOffices = 1;

            //__instance.ld.deadEndBuffer

            Baldi myBladi = (Baldi)MTM101BaldiDevAPI.npcMetadata.Get(Character.Baldi).prefabs["Baldi_Main" + currentFD.myFloorBaldi];

            __instance.ld.potentialBaldis = new WeightedNPC[1] {
                new WeightedNPC()
                {
                    weight = 420,
                    selection = myBladi
                }
            };

            __instance.ld.forcedSpecialHallBuilders = genData.forcedObjectBuilders.ToArray();
            
            __instance.ld.specialHallBuilders = genData.objectBuilders.ToArray();

            __instance.ld.minSpecialRooms = currentFD.minGiantRooms;
            __instance.ld.maxSpecialRooms = currentFD.maxGiantRooms;
            __instance.ld.potentialSpecialRooms = genData.specialRoomAssets.ToArray();

            __instance.ld.potentialOffices = genData.officeRoomAssets.ToArray();

            // special hallways, blegh
            __instance.ld.minPostPlotSpecialHalls = 0;
            __instance.ld.maxPostPlotSpecialHalls = 0;
            __instance.ld.minPrePlotSpecialHalls = currentFD.minSpecialHalls;
            __instance.ld.maxPrePlotSpecialHalls = currentFD.maxSpecialHalls;
            __instance.ld.potentialPostPlotSpecialHalls = new WeightedRoomAsset[0];
            __instance.ld.potentialPrePlotSpecialHalls = genData.hallInsertions.ToArray();
            __instance.ld.prePlotSpecialHallChance = 0.5f;

            // halls stuff
            __instance.ld.maxHallsToRemove = Mathf.Min(currentFD.FloorID / 2, 6);
            __instance.ld.minHallsToRemove = Mathf.Max(__instance.ld.maxHallsToRemove - 3, 0);

            __instance.ld.facultyStickToHallChance = currentFD.facultyStickToHall;

            __instance.ld.specialRoomsStickToEdge = currentFD.FloorID < 15;

            GeneratorManagement.Invoke("INF", currentFD.FloorID, (CustomLevelObject)__instance.ld);

        }
    }
}
