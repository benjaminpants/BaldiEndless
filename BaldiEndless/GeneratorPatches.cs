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
            SceneObject endlessSceneObject = EndlessFloorsPlugin.currentSceneObject;
            EndlessFloorsPlugin.Instance.UpdateData(ref endlessSceneObject);
            return true;
        }
    }

    [HarmonyPatch(typeof(BaseGameManager))]
    [HarmonyPatch("LoadSceneObject")]
    [HarmonyPatch(new Type[] { typeof(SceneObject), typeof(bool)})]
    class EternallyStuckV3
    {
        static void Prefix(SceneObject sceneObject)
        {
            if (sceneObject.levelObject == null) //we have completed the level and we are loading into the pitstop
            {
                EndlessFloorsPlugin.currentSave.currentFloor += 1;
                SceneObject endlessSceneObject = EndlessFloorsPlugin.currentSceneObject;
                EndlessFloorsPlugin.Instance.UpdateData(ref endlessSceneObject);
                if ((Singleton<CoreGameManager>.Instance.currentMode == EndlessFloorsPlugin.NNFloorMode) || (Singleton<CoreGameManager>.Instance.currentMode == Mode.Free))
                {
                    if (EndlessFloorsPlugin.currentFloorData.FloorID != EndlessFloorsPlugin.Instance.selectedFloor)
                    {
                        UnityEngine.Object.Destroy(Singleton<ElevatorScreen>.Instance.gameObject);
                        Singleton<CoreGameManager>.Instance.Quit();
                        return;
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
            }
        }
    }

    [HarmonyPatch(typeof(GameLoader))]
    [HarmonyPatch("LoadLevel")]
    class EternallyStuck
    {
        static void Prefix(ref SceneObject sceneObject)
        {
            if (sceneObject.levelObject == null) return;
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
            CustomLevelObject lvlObj = (CustomLevelObject)__instance.ld;
            lvlObj.potentialItems = genData.items.ToArray();
            lvlObj.forcedItems.Add(EndlessFloorsPlugin.upgradeObject);
            lvlObj.maxItemValue = currentFD.maxItemValue;
            __instance.seedOffset = currentFD.FloorID;
            lvlObj.minSize = new IntVector2(currentFD.minSize, currentFD.minSize);
            lvlObj.maxSize = new IntVector2(currentFD.maxSize, currentFD.maxSize);
            lvlObj.previousLevels = new LevelObject[0];

            lvlObj.timeBonusVal = 15 * currentFD.FloorID;
            lvlObj.timeBonusLimit = 90f * Mathf.Ceil(currentFD.maxSize / 24f);
            //lvlObj.fieldTrip = ((currentFD.FloorID % 4 == 0) && currentFD.FloorID != 4) || currentFD.FloorID % 99 == 0;
            //lvlObj.fieldTrips = genData.fieldTrips.ToArray();
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
            //lvlObj.fieldTripItems = genData.items.Where(x => (x.weight < avgWeight) && !x.selection.GetMeta().flags.HasFlag(ItemFlags.InstantUse)).Select(x => new WeightedItem() { weight = x.weight, selection = x.selection}).ToList();
            //SceneObject floor2 = EndlessFloorsPlugin.Instance.SceneObjects.ToList().Find(x => x.levelTitle == "F2");
            //lvlObj.tripEntrancePre = floor2.levelObject.tripEntrancePre;
            //lvlObj.tripEntranceRoom = floor2.levelObject.tripEntranceRoom;

            lvlObj.hallWallTexs = EndlessFloorsPlugin.wallTextures.ToArray();
            lvlObj.hallFloorTexs = EndlessFloorsPlugin.floorTextures.ToArray();
            lvlObj.hallCeilingTexs = EndlessFloorsPlugin.ceilTextures.ToArray();

            lvlObj.finalLevel = false; // THERE IS NO END

            System.Random stableRng = new System.Random(Singleton<CoreGameManager>.Instance.Seed());

            stableRng.Next();

            System.Random rng = new System.Random(Singleton<CoreGameManager>.Instance.Seed() + __instance.seedOffset);

            Color warmColor = new Color(255f / 255f, 202 / 255f, 133 / 255f);
            Color coldColor = new Color(133f / 255f, 161f / 255f, 255f / 255f);

            float coldLight = Mathf.Max(Mathf.Sin(((currentFD.FloorID / (1f + (float)(rng.NextDouble() * 15f))) + stableRng.Next(-50, 50))), 0f);
            float warmLight = Mathf.Max(Mathf.Sin(((currentFD.FloorID / (1f + (float)(rng.NextDouble() * 15f))) + stableRng.Next(-50, 50))), 0f);

            lvlObj.standardLightColor = Color.Lerp(Color.Lerp(Color.white, coldColor, coldLight), warmColor, warmLight);

            if (currentFD.FloorID % 99 == 0)
            {
                lvlObj.standardLightColor = Color.Lerp(lvlObj.standardLightColor, Color.red, 0.3f);
            }
            // if only i could remember how this fucking code works
            float rgb = Mathf.Max(16f, 255f - (currentFD.FloorID * 5));
            lvlObj.standardDarkLevel = new Color(rgb / 255, rgb / 255, rgb / 255);
            lvlObj.standardLightStrength = Mathf.Max(Mathf.RoundToInt(4f / (currentFD.FloorID / 24f)), 3);
            lvlObj.maxLightDistance = rng.Next(2, Mathf.Clamp(Mathf.FloorToInt(currentFD.FloorID / 2), 2, 12));

            // npc logic
            lvlObj.potentialNPCs = new List<WeightedNPC>();
            lvlObj.additionalNPCs = 0;
            int potNpcs = Mathf.Max(Mathf.Min(currentFD.npcCountUnclamped, genData.npcs.Count),1);
            stableRng = new System.Random(Singleton<CoreGameManager>.Instance.Seed()); //reset stableRng since we are no longer doing light stuff
            stableRng.Next();
            lvlObj.forcedNpcs = CreateWeightedShuffledListWithCount<WeightedNPC, NPC>(genData.npcs, potNpcs, stableRng).ToArray();
            lvlObj.forcedNpcs = lvlObj.forcedNpcs.AddRangeToArray<NPC>(genData.forcedNpcs.ToArray());
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

            RoomGroup classRoomGroup = lvlObj.roomGroup.First(x => x.name == "Class");
            classRoomGroup.potentialRooms = wra.ToArray();
            classRoomGroup.minRooms = currentFD.classRoomCount;
            classRoomGroup.maxRooms = currentFD.classRoomCount;
            classRoomGroup.floorTexture = EndlessFloorsPlugin.profFloorTextures.ToArray();
            classRoomGroup.wallTexture = EndlessFloorsPlugin.wallTextures.ToArray();
            classRoomGroup.ceilingTexture = EndlessFloorsPlugin.ceilTextures.ToArray();

            RoomGroup facultyRoomGroup = lvlObj.roomGroup.First(x => x.name == "Faculty");
            facultyRoomGroup.minRooms = currentFD.minFacultyRoomCount;
            facultyRoomGroup.maxRooms = currentFD.maxFacultyRoomCount;
            facultyRoomGroup.potentialRooms = CreateShuffledListWithCount(genData.facultyRoomAssets, 4 + Mathf.FloorToInt(currentFD.FloorID / 4), rng).ToArray();
            facultyRoomGroup.floorTexture = EndlessFloorsPlugin.profFloorTextures.ToArray();
            facultyRoomGroup.wallTexture = EndlessFloorsPlugin.facultyWallTextures.ToArray();
            facultyRoomGroup.ceilingTexture = EndlessFloorsPlugin.ceilTextures.ToArray();
            facultyRoomGroup.stickToHallChance = currentFD.facultyStickToHall;

            RoomGroup officeRoomGroup = lvlObj.roomGroup.First(x => x.name == "Office");
            officeRoomGroup.maxRooms = Mathf.Max(currentFD.maxOffices, 1);
            officeRoomGroup.minRooms = 1;
            officeRoomGroup.floorTexture = EndlessFloorsPlugin.profFloorTextures.ToArray();
            officeRoomGroup.wallTexture = EndlessFloorsPlugin.facultyWallTextures.ToArray();
            officeRoomGroup.ceilingTexture = EndlessFloorsPlugin.ceilTextures.ToArray();
            officeRoomGroup.potentialRooms = genData.officeRoomAssets.ToArray();

            stableRng = new System.Random(Singleton<CoreGameManager>.Instance.Seed());
            stableRng.Next();
            stableRng.Next();
            stableRng.Next();


            lvlObj.exitCount = currentFD.exitCount;
            lvlObj.additionTurnChance = (int)Mathf.Clamp((currentFD.unclampedScaleVar / 2), 0f, 35f);
            lvlObj.windowChance = Mathf.Max((currentFD.FloorID * -1.2f) + 14, 2);
            lvlObj.mapPrice = (currentFD.FloorID * 25) * Mathf.CeilToInt(currentFD.FloorID / 8f);
            lvlObj.maxPlots = currentFD.maxPlots;
            lvlObj.minPlots = currentFD.minPlots;
            lvlObj.outerEdgeBuffer = 3;
            lvlObj.bridgeTurnChance = Mathf.CeilToInt(Mathf.Clamp(currentFD.exitCount, 1f, 5f) * 3f); //lol
            //this changed... interesting...
            //lvlObj.itemChance = (int)currentFD.itemChance;

            lvlObj.maxSideHallsToRemove = Mathf.FloorToInt(currentFD.classRoomCount / 5);
            lvlObj.minSideHallsToRemove = Mathf.CeilToInt(currentFD.classRoomCount / 7);

            lvlObj.maxSpecialBuilders = Mathf.FloorToInt(currentFD.unclampedScaleVar / 16f);
            lvlObj.minSpecialBuilders = Mathf.FloorToInt((currentFD.unclampedScaleVar / 16f) / 1.5f);

            lvlObj.maxEvents = Mathf.RoundToInt(currentFD.classRoomCount / 2f);
            lvlObj.minEvents = Mathf.FloorToInt(currentFD.classRoomCount / 3);

            lvlObj.randomEvents = genData.randomEvents;
            lvlObj.maxEvents = Mathf.RoundToInt(currentFD.classRoomCount / 2f);
            lvlObj.minEvents = Mathf.FloorToInt(currentFD.classRoomCount / 3f);
            // mystman12 put in some code to remove duplicate events, how sad.
            /*while (lvlObj.maxEvents > (lvlObj.randomEvents.Count + 5))
            {
                lvlObj.randomEvents.AddRange(genData.randomEvents);
                Debug.Log("ADDING EXTRAS(potentially dangerous!)");
                Debug.Log(lvlObj.randomEvents.Count);
            }*/
            lvlObj.maxEventGap = currentFD.classRoomCount <= 19 ? 130f : 120f;
            lvlObj.minEventGap = currentFD.classRoomCount >= 14 ? 30f : 60f;
            //lvlObj.deadEndBuffer

            Baldi myBladi = (Baldi)MTM101BaldiDevAPI.npcMetadata.Get(Character.Baldi).prefabs["Baldi_Main" + currentFD.myFloorBaldi];

            lvlObj.potentialBaldis = new WeightedNPC[1] {
                new WeightedNPC()
                {
                    weight = 420,
                    selection = myBladi
                }
            };

            lvlObj.forcedSpecialHallBuilders = genData.forcedObjectBuilders.ToArray();
            
            lvlObj.specialHallBuilders = genData.objectBuilders.ToArray();

            lvlObj.minSpecialRooms = currentFD.minGiantRooms;
            lvlObj.maxSpecialRooms = currentFD.maxGiantRooms;
            lvlObj.potentialSpecialRooms = genData.specialRoomAssets.ToArray();

            // special hallways, blegh
            lvlObj.minPostPlotSpecialHalls = 0;
            lvlObj.maxPostPlotSpecialHalls = 0;
            lvlObj.minPrePlotSpecialHalls = currentFD.minSpecialHalls;
            lvlObj.maxPrePlotSpecialHalls = currentFD.maxSpecialHalls;
            lvlObj.potentialPostPlotSpecialHalls = new WeightedRoomAsset[0];
            lvlObj.potentialPrePlotSpecialHalls = genData.hallInsertions.ToArray();
            lvlObj.prePlotSpecialHallChance = 0.5f;

            // halls stuff
            lvlObj.maxHallsToRemove = Mathf.Min(currentFD.FloorID / 2, 6);
            lvlObj.minHallsToRemove = Mathf.Max(lvlObj.maxHallsToRemove - 3, 0);

            lvlObj.specialRoomsStickToEdge = ((currentFD.FloorID < 22) || (currentFD.FloorID % 24 == 0)); //random bs for the fun of it lol

            GeneratorManagement.Invoke("INF", currentFD.FloorID, lvlObj);

        }
    }
}
