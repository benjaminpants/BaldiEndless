using HarmonyLib;
using MTM101BaldAPI;
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
        static void Prefix()
        {
            SceneObject sceneObject = EndlessFloorsPlugin.currentSceneObject;
            EndlessFloorsPlugin.currentSave.currentFloor += 1;
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
        }
    }

    [HarmonyPatch(typeof(GameLoader))]
    [HarmonyPatch("LoadLevel")]
    class EternallyStuck
    {
        static void Prefix(ref SceneObject sceneObject)
        {
            EndlessFloorsPlugin.currentSave = new EndlessSaveData();
            EndlessFloorsPlugin.currentSave.startingFloor = EndlessFloorsPlugin.Instance.selectedFloor;
            EndlessFloorsPlugin.currentFloorData.FloorID = EndlessFloorsPlugin.Instance.selectedFloor;
            if ((EndlessFloorsPlugin.Instance.selectedFloor != 1) && (Singleton<CoreGameManager>.Instance.currentMode != EndlessFloorsPlugin.NNFloorMode))
            {
                Singleton<CoreGameManager>.Instance.AddPoints(FloorData.GetYTPsAtFloor(EndlessFloorsPlugin.Instance.selectedFloor), 0, false);
            }
            if (EndlessFloorsPlugin.Instance.selectedFloor >= 16)
            {
                EndlessFloorsPlugin.currentSave.Counters["slots"] = 5;
            }
            else if (EndlessFloorsPlugin.Instance.selectedFloor >= 12)
            {
                EndlessFloorsPlugin.currentSave.Counters["slots"] = 4;
            }
            else if (EndlessFloorsPlugin.Instance.selectedFloor >= 9)
            {
                EndlessFloorsPlugin.currentSave.Counters["slots"] = 3;
            }
            else if (EndlessFloorsPlugin.Instance.selectedFloor >= 6)
            {
                EndlessFloorsPlugin.currentSave.Counters["slots"] = 2;
            }
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
            __instance.ld.items = genData.items.ToArray();
            __instance.seedOffset = currentFD.FloorID;
            __instance.ld.minSize = new IntVector2(currentFD.minSize, currentFD.minSize);
            __instance.ld.maxSize = new IntVector2(currentFD.maxSize, currentFD.maxSize);
            __instance.ld.previousLevels = new LevelObject[0];

            __instance.ld.timeBonusVal = 1 * currentFD.FloorID;
            __instance.ld.fieldTrip = false;//((currentFD.FloorID % 4 == 0) && currentFD.FloorID != 4) || currentFD.FloorID % 99 == 0;

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

            // npc logic
            __instance.ld.potentialNPCs = new List<WeightedNPC>();
            __instance.ld.additionalNPCs = Mathf.Max(Mathf.Min(currentFD.npcCountUnclamped, genData.npcs.Count),1);
            stableRng = new System.Random(Singleton<CoreGameManager>.Instance.Seed());
            stableRng.Next();
            __instance.ld.forcedNpcs = CreateWeightedShuffledListWithCount<WeightedNPC, NPC>(genData.npcs, __instance.ld.additionalNPCs, stableRng).ToArray();
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
            __instance.ld.additionTurnChance = (int)(currentFD.unclampedScaleVar / 2);
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

            __instance.ld.maxLightDistance = Mathf.Clamp(Mathf.FloorToInt(currentFD.FloorID / 3), 1, 9);

            float rgb = Mathf.Max(16f, 255f - (currentFD.FloorID * 5));

            __instance.ld.standardDarkLevel = new Color(rgb / 255, rgb / 255, rgb / 255);

            __instance.ld.standardLightStrength = Mathf.Max(Mathf.RoundToInt(4f / (currentFD.FloorID / 24f)), 3);

            __instance.ld.maxFacultyRooms = currentFD.maxFacultyRoomCount;
            __instance.ld.minFacultyRooms = currentFD.minFacultyRoomCount;

            __instance.ld.maxSpecialBuilders = Mathf.RoundToInt(currentFD.unclampedScaleVar / 11f);
            __instance.ld.minSpecialBuilders = Mathf.RoundToInt((currentFD.unclampedScaleVar / 11f) / 1.5f);

            __instance.ld.maxEvents = Mathf.RoundToInt(currentFD.classRoomCount / 2f);
            __instance.ld.minEvents = Mathf.FloorToInt(currentFD.classRoomCount / 3);

            __instance.ld.randomEvents = genData.randomEvents;
            while (__instance.ld.maxEvents > (__instance.ld.randomEvents.Count + 5))
            {
                __instance.ld.randomEvents.AddRange(__instance.ld.randomEvents);
            }
            __instance.ld.maxEventGap = currentFD.classRoomCount <= 19 ? 130f : 120f;
            __instance.ld.minEventGap = currentFD.classRoomCount >= 14 ? 30f : 60f;
            __instance.ld.maxOffices = Mathf.Max(currentFD.maxOffices, 1);
            __instance.ld.minOffices = 1;
            __instance.ld.maxEvents = Mathf.RoundToInt(currentFD.classRoomCount / 2f);
            __instance.ld.minEvents = Mathf.FloorToInt(currentFD.classRoomCount / 3);

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

            /*
            __instance.ld.additionalNPCs = Mathf.Min(currentFD.npcCountUnclamped, EndlessFloorsPlugin.weightedNPCs.Count);
            System.Random rng = new System.Random(Singleton<CoreGameManager>.Instance.Seed());

            //custom npc handling because. honestly i don't think there is any reason for it anymore but im too lazy to undo all this code so
            List<NPC> ForcedNPCS = new List<NPC>
            {
                EndlessFloorsPlugin.NpcSpawns.Find(x => x.Character == Character.Principal)
            };
            WeightedSelection<NPC>[] weightedNpcsT = EndlessFloorsPlugin.weightedNPCs.ToArray();
            List<WeightedSelection<NPC>> weightedNpcs = weightedNpcsT.ToList();
            for (int i = 0; i < __instance.ld.additionalNPCs; i++)
            {
                NPC selectedNpc = WeightedNPC.ControlledRandomSelectionList(weightedNpcs, rng);
                weightedNpcs.RemoveAll(x => x.selection == selectedNpc);
                ForcedNPCS.Add(selectedNpc);
            }

            rng = new System.Random(Singleton<CoreGameManager>.Instance.Seed() + __instance.seedOffset);

            EndlessFloorsPlugin.currentSceneObject.skybox = EndlessFloorsPlugin.skyBoxes[rng.Next(0, 1)];

            __instance.ld.forcedNpcs = ForcedNPCS.ToArray();
            __instance.ld.potentialNPCs = new List<WeightedNPC>();
            __instance.Ec.npcsToSpawn = new List<NPC>();

            if (__instance.ld.maxEvents > (__instance.ld.randomEvents.Count + 5))
            {
                UnityEngine.Debug.Log("Adding extra events!(POTENTIALLY DANGEROUS!)");
                while (__instance.ld.randomEvents.Count < __instance.ld.maxEvents)
                {
                    __instance.ld.randomEvents.Add(__instance.ld.randomEvents[rng.Next(0, __instance.ld.randomEvents.Count - 2)]); //do -2 instead of -1 to exclude some things
                }
            }

            __instance.ld.specialHallBuilders = new WeightedObjectBuilder[5]
            {
                new WeightedObjectBuilder()
                {
                    selection = EndlessFloorsPlugin.objBuilders.Find(x => x.obstacle == Obstacle.Conveyor),
                    weight = 110
                },
                new WeightedObjectBuilder()
                {
                    selection = EndlessFloorsPlugin.objBuilders.Find(x => x.obstacle == Obstacle.CoinDoor),
                    weight = 90
                },
                new WeightedObjectBuilder()
                {
                    selection = EndlessFloorsPlugin.objBuilders.Find(x => x.obstacle == Obstacle.OneWaySwing),
                    weight = 80
                },
                new WeightedObjectBuilder()
                {
                    selection = EndlessFloorsPlugin.objBuilders.Find(x => x.obstacle == Obstacle.LockdownDoor),
                    weight = 90
                },
                new WeightedObjectBuilder()
                {
                    selection = EndlessFloorsPlugin.objBuilders.Find(x => x.name == "RotoHallBuilder"),
                    weight = 90
                }
            };

            // select class builders

            __instance.ld.classBuilders = new WeightedRoomBuilder[2]
            {
                new WeightedRoomBuilder()
                {
                    selection = EndlessFloorsPlugin.classBuilder,
                    weight = 100
                },
                new WeightedRoomBuilder()
                {
                    selection = EndlessFloorsPlugin.mathMachineBuilder,
                    weight = (int)(10 * currentFD.unclampedScaleVar)
                }
            };

            __instance.ld.specialRooms = new WeightedSpecialRoomCreator[]
            {
                new WeightedSpecialRoomCreator()
                {
                    weight = 90,
                    selection=EndlessFloorsPlugin.SpecialCreators.Find(x => x.obstacle == Obstacle.Cafe)
                },
                new WeightedSpecialRoomCreator()
                {
                    weight = 90,
                    selection=EndlessFloorsPlugin.SpecialCreators.Find(x => x.obstacle == Obstacle.Playground)
                },
                new WeightedSpecialRoomCreator()
                {
                    weight = 70,
                    selection=EndlessFloorsPlugin.SpecialCreators.Find(x => x.obstacle == Obstacle.Library)
                },

            };

            __instance.ld.totalShopItems = 2;

            __instance.ld.minSize = new IntVector2(currentFD.minSize, currentFD.minSize);
            __instance.ld.maxSize = new IntVector2(currentFD.maxSize, currentFD.maxSize);
            __instance.ld.previousLevels = new LevelObject[0];

            __instance.ld.timeBonusVal = 1 * currentFD.FloorID;

            __instance.ld.fieldTripItems = EndlessFloorsPlugin.Instance.SceneObjects.ToList().Find(x => x.levelTitle == "F2").levelObject.fieldTripItems; //TODO: DONT FUCKING DO THIS

            __instance.ld.fieldTrips = new WeightedFieldTrip[2]
            {
                new WeightedFieldTrip()
                {
                    selection = EndlessFloorsPlugin.tripObjects.Find(x => x.trip == FieldTrips.Camping),
                    weight = 49
                },
                new WeightedFieldTrip()
                {
                    selection = EndlessFloorsPlugin.tripObjects.Find(x => x.trip == FieldTrips.Farm),
                    weight = 50
                }
            };

            */

        }
    }
}
