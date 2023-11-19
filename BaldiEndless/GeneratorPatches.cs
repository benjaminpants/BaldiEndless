using HarmonyLib;
using MTM101BaldAPI;
using System;
using System.Collections.Generic;
using System.Linq;
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
            EndlessFloorsPlugin.currentSave.currentFloorData.FloorID += 1;
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

    [HarmonyPatch(typeof(MazeGenerator))]
    [HarmonyPatch("StartGenerator")]
    class MazeFix
    {
        static void Prefix(RoomController room, System.Random rng, ref int ___minPatchSize, ref int ___maxPatchSize)
        {
            ___maxPatchSize = Math.Min(___maxPatchSize, Math.Min(room.size.x - 3, room.size.z - 3));
            ___minPatchSize = Math.Min(___maxPatchSize, ___minPatchSize);
        }
    }

    [HarmonyPatch(typeof(GameLoader))]
    [HarmonyPatch("LoadLevel")]
    class EternallyStuck
    {
        static void Prefix(ref SceneObject sceneObject)
        {
            EndlessFloorsPlugin.currentSave = new EndlessSave();
            EndlessFloorsPlugin.currentFloorData.FloorID = EndlessFloorsPlugin.Instance.selectedFloor;
            if (EndlessFloorsPlugin.victoryScene == null)
            {
                EndlessFloorsPlugin.victoryScene = EndlessFloorsPlugin.Instance.SceneObjects.ToList().Find(x => x.levelTitle == "YAY");
            }
            EndlessFloorsPlugin.Instance.UpdateData(ref sceneObject);
        }
    }


    [HarmonyPatch(typeof(LevelGenerator))]
    [HarmonyPatch("StartGenerate")]
    class GenerateBegin
    {
        static void Prefix(LevelGenerator __instance)
        {
            FloorData currentFD = EndlessFloorsPlugin.currentFloorData;
            __instance.seedOffset = currentFD.FloorID;
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

            // switch out the baldi for the one that is closest to our current notebook count
            Baldi myBladi = EndlessFloorsPlugin.TheBladis.FindLast(x => x.name.Contains("Main" + currentFD.myFloorBaldi));

            __instance.ld.potentialBaldis = new WeightedNPC[1] {
                new WeightedNPC()
                {
                    weight = 420,
                    selection = myBladi
                }
            };

            rng = new System.Random(Singleton<CoreGameManager>.Instance.Seed() + __instance.seedOffset);

            EndlessFloorsPlugin.currentSceneObject.skybox = EndlessFloorsPlugin.skyBoxes[rng.Next(0, 1)];

            __instance.ld.forcedNpcs = ForcedNPCS.ToArray();
            __instance.ld.potentialNPCs = new List<WeightedNPC>();
            __instance.Ec.npcsToSpawn = new List<NPC>();
            __instance.ld.minSpecialRooms = currentFD.minGiantRooms;
            __instance.ld.maxSpecialRooms = currentFD.maxGiantRooms;

            __instance.ld.maxEventGap = currentFD.classRoomCount <= 19 ? 130f : 120f;
            __instance.ld.minEventGap = currentFD.classRoomCount >= 14 ? 30f : 60f;

            __instance.ld.maxOffices = Mathf.Max(currentFD.maxOffices, 1);
            __instance.ld.minOffices = 1;

            //use the regular seed without the offset so it stays consistent
            System.Random lightRng = new System.Random(Singleton<CoreGameManager>.Instance.Seed());

            //cause why not
            lightRng.Next();
            lightRng.Next();

            Color warmColor = new Color(255f / 255f, 202 / 255f, 133 / 255f);
            Color coldColor = new Color(133f / 255f, 161f / 255f, 255f / 255f);

            float coldLight = Mathf.Max(Mathf.Sin(((currentFD.FloorID / (1f + (float)(rng.NextDouble() * 15f))) + rng.Next(-50, 50))), 0f);
            float warmLight = Mathf.Max(Mathf.Sin(((currentFD.FloorID / (1f + (float)(rng.NextDouble() * 15f))) + rng.Next(-50, 50))), 0f);

            __instance.ld.standardLightColor = Color.Lerp(Color.Lerp(Color.white, coldColor, coldLight), warmColor, warmLight);

            if (currentFD.FloorID % 99 == 0)
            {
                __instance.ld.standardLightColor = Color.Lerp(__instance.ld.standardLightColor, Color.red, 0.3f);
            }

            __instance.ld.randomEvents = new List<WeightedRandomEvent>
            {
                new WeightedRandomEvent()
                {
                    weight = 150,
                    selection = EndlessFloorsPlugin.randomEvents.FindLast(x => x.Type == RandomEventType.Fog)
                },
                new WeightedRandomEvent()
                {
                    weight = 125,
                    selection = EndlessFloorsPlugin.randomEvents.FindLast(x => x.Type == RandomEventType.Party)
                },
                new WeightedRandomEvent()
                {
                    weight = 70,
                    selection = EndlessFloorsPlugin.randomEvents.FindLast(x => x.Type == RandomEventType.Snap)
                },
                new WeightedRandomEvent()
                {
                    weight = 90,
                    selection = EndlessFloorsPlugin.randomEvents.FindLast(x => x.Type == RandomEventType.Flood)
                }
            };

            if (EndlessFloorsPlugin.TimesInstalled)
            {
                __instance.ld.randomEvents.Add(new WeightedRandomEvent()
                {
                    weight = 20,
                    selection = EndlessFloorsPlugin.randomEvents.FindLast(x => (x.gameObject.name == "CustomEv_PrincipalOut"))
                });
                __instance.ld.randomEvents.Add(new WeightedRandomEvent()
                {
                    weight = 50,
                    selection = EndlessFloorsPlugin.randomEvents.FindLast(x => (x.gameObject.name == "CustomEv_BlackOut"))
                });
                __instance.ld.randomEvents.Add(new WeightedRandomEvent()
                {
                    weight = 70,
                    selection = EndlessFloorsPlugin.randomEvents.FindLast(x => (x.gameObject.name == "CustomEv_CurtainsClosed"))
                });
                __instance.ld.randomEvents.Add(new WeightedRandomEvent()
                {
                    weight = 65,
                    selection = EndlessFloorsPlugin.randomEvents.FindLast(x => (x.gameObject.name == "CustomEv_WindySchool"))
                });
                /*Debug.Log("Select");
                foreach (WeightedRandomEvent wre in __instance.ld.randomEvents)
                {
                    Debug.Log(wre.selection);
                }*/
            }

            if (currentFD.classRoomCount >= 6)
            {
                __instance.ld.randomEvents.Add(new WeightedRandomEvent()
                {
                    weight = 75,
                    selection = EndlessFloorsPlugin.randomEvents.FindLast(x => x.Type == RandomEventType.Gravity)
                });
            }

            if (currentFD.classRoomCount >= 8)
            {
                __instance.ld.randomEvents.Add(new WeightedRandomEvent()
                {
                    weight = 40,
                    selection = EndlessFloorsPlugin.randomEvents.FindLast(x => x.Type == RandomEventType.MysteryRoom)
                });
            }

            __instance.ld.maxEvents = Mathf.RoundToInt(currentFD.classRoomCount / 2f);
            __instance.ld.minEvents = Mathf.FloorToInt(currentFD.classRoomCount / 3);

            if (__instance.ld.maxEvents > (__instance.ld.randomEvents.Count + 5))
            {
                UnityEngine.Debug.Log("Adding extra events!(POTENTIALLY DANGEROUS!)");
                while (__instance.ld.randomEvents.Count < __instance.ld.maxEvents)
                {
                    __instance.ld.randomEvents.Add(__instance.ld.randomEvents[rng.Next(0, __instance.ld.randomEvents.Count - 2)]); //do -2 instead of -1 to exclude some things
                }
            }

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

            __instance.ld.itemChance = (int)currentFD.itemChance;

            __instance.ld.maxSideHallsToRemove = Mathf.FloorToInt(currentFD.classRoomCount / 5);
            __instance.ld.minSideHallsToRemove = Mathf.CeilToInt(currentFD.classRoomCount / 7);

            __instance.ld.maxLightDistance = Mathf.Clamp(Mathf.FloorToInt(currentFD.FloorID / 3), 1, 9);

            float rgb = Mathf.Max(16f, 255f - (currentFD.FloorID * 6));

            __instance.ld.standardDarkLevel = new Color(rgb / 255, rgb / 255, rgb / 255);

            __instance.ld.standardLightStrength = Mathf.Max(Mathf.RoundToInt(4f / (currentFD.FloorID / 24f)), 3);

            __instance.ld.maxFacultyRooms = currentFD.maxFacultyRoomCount;
            __instance.ld.minFacultyRooms = currentFD.minFacultyRoomCount;

            __instance.ld.maxSpecialBuilders = Mathf.RoundToInt(currentFD.unclampedScaleVar / 11f);
            __instance.ld.minSpecialBuilders = Mathf.RoundToInt((currentFD.unclampedScaleVar / 11f) / 1.5f);

            List<ObjectBuilder> extraObjs = new List<ObjectBuilder>();

            WeightedObjectBuilder[] possibleExtraBuilders = new WeightedObjectBuilder[]
            {
                new WeightedObjectBuilder()
                {
                    selection = EndlessFloorsPlugin.objBuilders.Find(x => x.name == "PayphoneBuilder"),
                    weight = 60
                },
                new WeightedObjectBuilder()
                {
                    selection = EndlessFloorsPlugin.objBuilders.Find(x => x.name == "BsodaHallBuilder"),
                    weight = 100
                },
                new WeightedObjectBuilder()
                {
                    selection = EndlessFloorsPlugin.objBuilders.Find(x => x.name == "ZestyHallBuilder"),
                    weight = 100
                },
                new WeightedObjectBuilder()
                {
                    selection = EndlessFloorsPlugin.objBuilders.Find(x => x.obstacle == Obstacle.Fountain),
                    weight = 80
                },
            };

            if (EndlessFloorsPlugin.TimesInstalled)
            {
                possibleExtraBuilders = possibleExtraBuilders.AddItem(new WeightedObjectBuilder()
                {
                    selection = EndlessFloorsPlugin.objBuilders.Find(x => x.obstacle == EnumExtensions.GetFromExtendedName<Obstacle>("Bell Builder")),
                    weight = 50
                }).ToArray();
                possibleExtraBuilders = possibleExtraBuilders.AddItem(new WeightedObjectBuilder()
                {
                    selection = EndlessFloorsPlugin.objBuilders.Find(x => x.obstacle == EnumExtensions.GetFromExtendedName<Obstacle>("Vent Builder")),
                    weight = 70
                }).ToArray();
            }

            int extraBuilders = rng.Next(2, Mathf.Min(2 + Mathf.FloorToInt(currentFD.FloorID / 5), 12));

            for (int i = 0; i < extraBuilders; i++)
            {
                extraObjs.Add(WeightedObjectBuilder.ControlledRandomSelection(possibleExtraBuilders, rng));
            }

            //move this last so the other stuff has room to generate
            extraObjs.Add(EndlessFloorsPlugin.objBuilders.Find(x => x.name == "PlantBuilder"));
            if (EndlessFloorsPlugin.TimesInstalled)
            {
                // only so later on floors get vents
                if (currentFD.scaleVar > 35f)
                {
                    extraObjs.Add(EndlessFloorsPlugin.objBuilders.Find(x => x.obstacle == EnumExtensions.GetFromExtendedName<Obstacle>("Trap Door Builder")));
                }
            }

            __instance.ld.forcedSpecialHallBuilders = extraObjs.ToArray();

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

            /*if (EndlessFloorsPlugin.TimesInstalled)
            {
                __instance.ld.classBuilders = __instance.ld.classBuilders.AddToArray(new WeightedRoomBuilder()
                {
                    selection = Resources.FindObjectsOfTypeAll<ClassBuilder>().ToList().Find(x => x.name == "CustomRoomBuilder_Messy Class Builder"),
                    weight = 90
                });
            }*/

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

            if (EndlessFloorsPlugin.TimesInstalled)
            {
                __instance.ld.specialRooms = __instance.ld.specialRooms.AddItem(new WeightedSpecialRoomCreator()
                {
                    weight = 80,
                    selection = EndlessFloorsPlugin.SpecialCreators.Find(x => x.obstacle == EnumExtensions.GetFromExtendedName<Obstacle>("BasketBallArea"))
                }).ToArray();
                __instance.ld.specialRooms = __instance.ld.specialRooms.AddItem(new WeightedSpecialRoomCreator()
                {
                    weight = 75,
                    selection = EndlessFloorsPlugin.SpecialCreators.Find(x => x.obstacle == EnumExtensions.GetFromExtendedName<Obstacle>("ForestArea"))
                }).ToArray();
            }

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

            __instance.ld.fieldTrip = ((currentFD.FloorID % 4 == 0) && currentFD.FloorID != 4) || currentFD.FloorID % 99 == 0;

            __instance.ld.items = EndlessFloorsPlugin.weightedItems;

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

        }
    }
}
