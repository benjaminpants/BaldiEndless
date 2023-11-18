using System;
using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using UnityEngine;
using MTM101BaldAPI.AssetManager;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MTM101BaldAPI.SaveSystem;
using BepInEx.Bootstrap;
using System.Reflection;
using UnityEngine.Rendering;
using System.Collections;
using System.Xml.Linq;

namespace BaldiEndless
{

    //[BepInIncompatibility(BBTimesID)]
    [BepInPlugin("mtm101.rulerp.baldiplus.endlessfloors", "Endless Floors", "2.0.0.0")]
    public class EndlessFloorsPlugin : BaseUnityPlugin
    {

        public const string BBTimesID = "pixelguy.pixelmodding.baldiplus.bbextracontent";

        public SceneObject[] SceneObjects;

        public Dictionary<string, Sprite> UpgradeIcons = new Dictionary<string, Sprite>();

        public static ItemObject Nametag;

        public static List<ItemObject> ItemObjects;

        public static List<SpecialRoomCreator> SpecialCreators;

        public static List<FieldTripObject> tripObjects;

        public static List<ObjectBuilder> objBuilders;

        public static ClassBuilder classBuilder;

        public static MathMachineRoom mathMachineBuilder;

        public static List<NPC> NpcSpawns = new List<NPC>();

        public static List<Baldi> TheBladis = new List<Baldi>();

        public static List<HappyBaldi> TheHappyBladis = new List<HappyBaldi>();

        public static List<RandomEvent> randomEvents = new List<RandomEvent>();

        public static List<WeightedNPC> weightedNPCs = new List<WeightedNPC>();

        public static SceneObject currentSceneObject;

        public static EndlessSave currentSave = new EndlessSave();

        public static Cubemap[] skyBoxes;

        public static SceneObject victoryScene;

        public static EndlessFloorsPlugin Instance { get; private set; }

        public static Mode NNFloorMode = EnumExtensions.ExtendEnum<Mode>("Floor99");

        public static string F99MusicStart = "";

        public static string F99MusicLoop = "";
        public static FloorData currentFloorData => currentSave.currentFloorData;

        public bool hasSave = false; // UNUSED

        public int highestFloorCount = 1;

        public int selectedFloor = 1;

        public string lastAllocatedPath = "";

        public static Texture2D upgradeTex5;

        public static Items presentEnum = EnumExtensions.ExtendEnum<Items>("Present");

        public static ItemObject presentObject;

        public static WeightedItemObject[] weightedItems;

        public static List<WeightedTexture2D> wallTextures = new List<WeightedTexture2D>();

        public static List<WeightedTexture2D> facultyWallTextures = new List<WeightedTexture2D>();

        public static List<WeightedTexture2D> ceilTextures = new List<WeightedTexture2D>();

        public static List<WeightedTexture2D> floorTextures = new List<WeightedTexture2D>();

        public static List<WeightedTexture2D> profFloorTextures = new List<WeightedTexture2D>();

        public void SaveHighestFloor()
        {
            string allocatedPath = ModdedSaveSystem.GetCurrentSaveFolder(this);
            string highestFloorCountFile = Path.Combine(allocatedPath, "high.txt");
            File.WriteAllText(highestFloorCountFile, highestFloorCount.ToString());
        }

        public void LoadHighestFloor()
        {
            string allocatedPath = ModdedSaveSystem.GetCurrentSaveFolder(this);
            string highestFloorCountFile = Path.Combine(allocatedPath, "high.txt");
            if (!File.Exists(highestFloorCountFile))
            {
                SaveHighestFloor();
            }
            highestFloorCount = int.Parse(File.ReadAllText(highestFloorCountFile));
        }

        public void WriteHasDataAck(bool val)
        {
            if (lastAllocatedPath == "") throw new InvalidOperationException();
            File.WriteAllText(Path.Combine(lastAllocatedPath, "hasdata.txt"), val ? "y" : "n");
        }

        //once again thank you chat gpt i HATE stupid stuff like this
        public static void SetContentManager(string propertyName, object value, BindingFlags flags = BindingFlags.Public)
        {
            Type contentManagerType = Chainloader.PluginInfos[EndlessFloorsPlugin.BBTimesID].Instance.GetType().Assembly.GetType("BB_MOD.ContentManager");
            object instance = contentManagerType.GetField("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            Debug.Log(instance);
            FieldInfo fo = contentManagerType.GetField(propertyName, flags);
            Debug.Log(fo);
            fo.SetValue(instance, value);
        }

        /*void SaveLoad(bool isSave, string allocatedPath)
        {
            lastAllocatedPath = allocatedPath;
            string playerDataPath = Path.Combine(allocatedPath,"EndlessSave.json");
            string hasDataPath = Path.Combine(allocatedPath, "hasdata.txt");
            if (!File.Exists(playerDataPath))
            {
                File.WriteAllText(playerDataPath, JsonUtility.ToJson(new EndlessSaveSerializable(currentSave), true));
                WriteHasDataAck(false);
                hasSave = false;
            }
            else
            {
                if (File.Exists(hasDataPath))
                {
                    hasSave = File.ReadAllText(hasDataPath) == "y" ? true : false;
                }
                else
                {
                    WriteHasDataAck(false);
                    return;
                }
                if (hasSave)
                {
                    //load the save, only do this if they have a save to be loaded
                    EndlessSaveSerializable serial = JsonUtility.FromJson<EndlessSaveSerializable>(File.ReadAllText(playerDataPath));
                }
            }
        }*/

        void SaveLoadHighestFloor(bool isSave, string allocatedPath)
        {
            if (isSave)
            {
                SaveHighestFloor();
            }
            else
            {
                LoadHighestFloor();
            }
        }

        public static bool TimesInstalled => Chainloader.PluginInfos.ContainsKey(BBTimesID);

        void AddWeightedTextures(ref List<WeightedTexture2D> tex, string folder)
        {
            string myPath = AssetManager.GetModPath(this);
            string wallsPath = Path.Combine(myPath, "Textures", folder);
            foreach (string p in Directory.GetFiles(wallsPath))
            {
                string standardName = Path.GetFileNameWithoutExtension(p);
                Texture2D texx = AssetManager.TextureFromFile(p);
                string[] splitee = standardName.Split('!');
                tex.Add(new WeightedTexture2D()
                {
                    selection = texx,
                    weight = int.Parse(splitee[1])
                });
            }
        }

        void Awake()
        {
            Instance = this;
            Harmony harmony = new Harmony("mtm101.rulerp.baldiplus.endlessfloors");
            MTM101BaldiDevAPI.SavesEnabled = false;
            string myPath = AssetManager.GetModPath(this);
            string iconPath = Path.Combine(myPath, "UpgradeIcons");
            foreach (string p in Directory.GetFiles(iconPath))
            {
                Texture2D tex = AssetManager.TextureFromFile(p);
                Sprite spr = AssetManager.SpriteFromTexture2D(tex, Vector2.one / 2f, 50f);
                UpgradeIcons.Add(Path.GetFileNameWithoutExtension(p), spr);
            }

            string wallsPath = Path.Combine(myPath, "Textures", "Walls");
            foreach (string p in Directory.GetFiles(wallsPath))
            {
                string standardName = Path.GetFileNameWithoutExtension(p);
                if (standardName.StartsWith("F_")) continue; // no.
                Texture2D tex = AssetManager.TextureFromFile(p);
                string[] splitee = standardName.Split('!');
                wallTextures.Add(new WeightedTexture2D()
                {
                    selection = tex,
                    weight = int.Parse(splitee[1])
                });
                string facultyEquiv = Path.Combine(wallsPath, "F_" + splitee[0] + ".png");
                if (File.Exists(facultyEquiv))
                {
                    Texture2D texf = AssetManager.TextureFromFile(facultyEquiv);
                    facultyWallTextures.Add(new WeightedTexture2D()
                    {
                        selection = texf,
                        weight = int.Parse(splitee[1])
                    });
                }
                else
                {
                    facultyWallTextures.Add(new WeightedTexture2D()
                    {
                        selection = tex,
                        weight = int.Parse(splitee[1])
                    });
                }
            }

            AddWeightedTextures(ref ceilTextures, "Ceilings");

            AddWeightedTextures(ref floorTextures, "Floors");

            AddWeightedTextures(ref profFloorTextures, "ProfFloors");

            upgradeTex5 = AssetManager.TextureFromMod(this, "UpgradeSlot5.png");

            Texture2D presentTex = AssetManager.TextureFromMod(this, "PresentIcon_Large.png");

            Sprite presentSprite = AssetManager.SpriteFromTexture2D(presentTex, Vector2.one / 2, 50f);

            presentObject = ObjectCreatorHandlers.CreateItemObject("Itm_Present", "Itm_Present", presentSprite, presentSprite, presentEnum, 9999, 26);

            DontDestroyOnLoad(presentObject.item = new GameObject().AddComponent<ITM_Present>()); // WHAT THE FUCK THIS IS ACTUALLY VALID SYNTAX I WAS FUCKING JOKING

            ModdedSaveSystem.AddSaveLoadAction(this, SaveLoadHighestFloor);

            StartCoroutine(WaitTilAllLoaded(harmony));
        }

        private IEnumerator WaitTilAllLoaded(Harmony harmony)
        {
            FieldInfo loaded = AccessTools.Field(typeof(Chainloader), "_loaded");

            while (!loaded.GetValue<bool>(null))
            {
                yield return null;
            }

            if (EndlessFloorsPlugin.TimesInstalled)
            {
                //harmony.Unpatch(typeof(LevelGenerator).GetMethod("StartGenerate"), HarmonyPatchType.Prefix, BBTimesID);
                //harmony.Unpatch(typeof(LevelGenerator).GetMethod("Generate"), HarmonyPatchType.Transpiler, BBTimesID);
                harmony.Unpatch(typeof(BaseGameManager).GetMethod("ElevatorClosed", BindingFlags.NonPublic | BindingFlags.Instance), HarmonyPatchType.Prefix, BBTimesID);
                harmony.Unpatch(typeof(BaseGameManager).GetMethod("AllNotebooks", BindingFlags.NonPublic | BindingFlags.Instance), HarmonyPatchType.Prefix, BBTimesID);
                //harmony.Unpatch(typeof(CafeteriaCreator).GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance), HarmonyPatchType.Prefix, BBTimesID);
            }
            harmony.PatchAllConditionals();
            //Debug.Log(Chainloader.PluginInfos[EndlessFloorsPlugin.BBTimesID].Instance.GetType().Assembly.GetType("BB_MOD.ContentManager").Assembly.FullName);
        }

        public void UpdateData(ref SceneObject sceneObject)
        {
            sceneObject = currentSceneObject;
            sceneObject.levelNo = currentSave.currentFloorData.FloorID;
            sceneObject.nextLevel = sceneObject;
            sceneObject.levelTitle = "F" + currentSave.currentFloorData.FloorID;
        }
    }

    public class EndlessSave
    {
        public FloorData currentFloorData = new FloorData();

        // lol
        public int livesBought = 0;
        // stupid workaround
        public bool hasClaimedFreeYTP = false;
        public int staminasBought => GetUpgradeCount(typeof(StaminaIncrease));
        public int pompIncreaseMinutes => GetUpgradeCount(typeof(PompTimeIncrease));
        public float luckValue => GetUpgradeCount(typeof(PresentLuck)) == 0 ? 1 : PresentLuck.LuckValues[GetUpgradeCount(typeof(PresentLuck)) - 1];


        // TODO: redo how this shop system works I did not realize everything I would've needed for this but I don't want to rewrite it.
        public List<PurchasedUpgradeData> purchasedUpgrades = new List<PurchasedUpgradeData>();

        [NonSerialized]
        public List<GameUpgrade> purchasedUpgradesClasses = new List<GameUpgrade>();

        public void AddUpgrade(GameUpgrade upg)
        {
            string theType = upg.GetType().ToString();
            PurchasedUpgradeData alUpg = purchasedUpgrades.Find(x => x.myUpgrade == theType);
            if (alUpg == null)
            {
                purchasedUpgrades.Add(new PurchasedUpgradeData()
                {
                    stack = 1,
                    myUpgrade = theType,
                });
                purchasedUpgradesClasses.Add(upg);
            }
            else
            {
                alUpg.stack += 1;
            }
        }

        public bool RemoveOneUpgrade(Type upg)
        {
            string theType = upg.ToString();
            PurchasedUpgradeData alUpg = purchasedUpgrades.Find(x => x.myUpgrade == theType);
            if (alUpg != null)
            {
                if (alUpg.stack != 0)
                {
                    alUpg.stack -= 1;
                }
                return true;
            }
            return false;
        }

        public void RemoveUpgrade(int slot)
        {
            purchasedUpgrades.RemoveAt(slot);
            purchasedUpgradesClasses.RemoveAt(slot);
        }

        public int GetUpgradeCount(Type upg)
        {
            PurchasedUpgradeData mupg = purchasedUpgrades.Find(x => x.myUpgrade == upg.ToString());
            if (mupg == null) return 0;
            return mupg.stack;
        }

        public bool HasUpgrade(Type upg)
        {
            return GetUpgradeCount(upg) > 0;
        }
    }

    [HarmonyPatch(typeof(NameManager))]
    [HarmonyPatch("Awake")]
    class GetAllAssets
    {

        static void Postfix(NameManager __instance)
        {
            string myPath = AssetManager.GetModPath(EndlessFloorsPlugin.Instance);
            string midiPath = Path.Combine(myPath, "Midi");
            EndlessFloorsPlugin.F99MusicStart = AssetManager.MidiFromFile(Path.Combine(midiPath, "floor_99_finale_beginning.mid"), "99start");
            EndlessFloorsPlugin.F99MusicLoop = AssetManager.MidiFromFile(Path.Combine(midiPath, "floor_99_finale_loop.mid"), "99loop");


            EndlessFloorsPlugin.Instance.SceneObjects = Resources.FindObjectsOfTypeAll<SceneObject>();
            EndlessFloorsPlugin.tripObjects = Resources.FindObjectsOfTypeAll<FieldTripObject>().Where(x => x.tripPre != null).ToList();
            EndlessFloorsPlugin.TheBladis = Resources.FindObjectsOfTypeAll<Baldi>().ToList();
            EndlessFloorsPlugin.TheHappyBladis = Resources.FindObjectsOfTypeAll<HappyBaldi>().ToList();
            EndlessFloorsPlugin.SpecialCreators = Resources.FindObjectsOfTypeAll<SpecialRoomCreator>().Where(x => !x.name.Contains("Speedy")).Where(x => x.obstacle != Obstacle.Cafe || x.gameObject.name == "Cafeteria").ToList();
            EndlessFloorsPlugin.objBuilders = Resources.FindObjectsOfTypeAll<ObjectBuilder>().ToList();
            EndlessFloorsPlugin.classBuilder = Resources.FindObjectsOfTypeAll<ClassBuilder>().First();
            EndlessFloorsPlugin.mathMachineBuilder = Resources.FindObjectsOfTypeAll<MathMachineRoom>().First();
            EndlessFloorsPlugin.randomEvents = Resources.FindObjectsOfTypeAll<RandomEvent>().ToList();
            EndlessFloorsPlugin.ItemObjects = Resources.FindObjectsOfTypeAll<ItemObject>().Where(x => x.itemType != Items.GrapplingHook || x.nameKey == "Itm_GrapplingHook").ToList();
            EndlessFloorsPlugin.skyBoxes = Resources.FindObjectsOfTypeAll<Cubemap>().Where(x => x.name.Contains("Cubemap_")).ToArray();
            EndlessFloorsPlugin.Nametag = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.Nametag);
            EndlessFloorsPlugin.NpcSpawns = Resources.FindObjectsOfTypeAll<NPC>().Where(
                x => x.Character != Character.Baldi && !x.name.Contains("Grapple") && !(x.name == "Principal_AllKnowing") && (x.Character != Character.Beans || x.gameObject.name == "Beans")
                ).ToList();
            var npcs = EndlessFloorsPlugin.NpcSpawns;
            EndlessFloorsPlugin.currentSceneObject = EndlessFloorsPlugin.Instance.SceneObjects.ToList().Find(x => x.levelTitle == "F3");
            EndlessFloorsPlugin.wallTextures.AddRange(EndlessFloorsPlugin.currentSceneObject.levelObject.hallWallTexs);
            EndlessFloorsPlugin.ceilTextures.AddRange(EndlessFloorsPlugin.currentSceneObject.levelObject.hallCeilingTexs);
            EndlessFloorsPlugin.floorTextures.AddRange(EndlessFloorsPlugin.currentSceneObject.levelObject.hallFloorTexs);
            EndlessFloorsPlugin.facultyWallTextures.AddRange(EndlessFloorsPlugin.currentSceneObject.levelObject.facultyWallTexs);
            EndlessFloorsPlugin.profFloorTextures.AddRange(EndlessFloorsPlugin.currentSceneObject.levelObject.facultyFloorTexs);
            EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
            {
                weight = 100,
                selection = npcs.Find(x => x.Character == Character.Playtime)
            });
            EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
            {
                weight = 100,
                selection = npcs.Find(x => x.Character == Character.Sweep)
            });
            EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
            {
                weight = 110,
                selection = npcs.Find(x => x.Character == Character.Beans)
            });
            EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
            {
                weight = 100,
                selection = npcs.Find(x => x.Character == Character.Bully)
            });
            EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
            {
                weight = 80,
                selection = npcs.Find(x => x.Character == Character.Crafters)
            });
            EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
            {
                weight = 125,
                selection = npcs.Find(x => x.Character == Character.Chalkles)
            });
            EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
            {
                weight = 10,
                selection = npcs.Find(x => x.Character == Character.LookAt)
            });
            EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
            {
                weight = 90,
                selection = npcs.Find(x => x.Character == Character.Pomp)
            });
            EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
            {
                weight = 95,
                selection = npcs.Find(x => x.Character == Character.Cumulo)
            });
            EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
            {
                weight = 70,
                selection = npcs.Find(x => x.Character == Character.Prize)
            });
            if (EndlessFloorsPlugin.TimesInstalled)
            {
                EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
                {
                    weight = 25,
                    selection = npcs.Find(x => x.Character == EnumExtensions.GetFromExtendedName<Character>("Office Chair"))
                });
                EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
                {
                    weight = 30,
                    selection = npcs.Find(x => x.Character == EnumExtensions.GetFromExtendedName<Character>("Happy Holidays"))
                });

                // This character is actually the worst Baldi character ever to be created, I've never even encountered him but
                // his fundamental mechanic is just so god awful that he almost always spawns last.
                EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
                {
                    weight = 1,
                    selection = npcs.Find(x => x.Character == EnumExtensions.GetFromExtendedName<Character>("Super Intendent"))
                });
                EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
                {
                    weight = 30,
                    selection = npcs.Find(x => x.Character == EnumExtensions.GetFromExtendedName<Character>("Crazy Clock"))
                });
                EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
                {
                    weight = 34,
                    selection = npcs.Find(x => x.Character == EnumExtensions.GetFromExtendedName<Character>("Forgotten"))
                });
                EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
                {
                    weight = 60,
                    selection = npcs.Find(x => x.Character == EnumExtensions.GetFromExtendedName<Character>("Let's Drum"))
                });
                EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
                {
                    weight = 18,
                    selection = npcs.Find(x => x.Character == EnumExtensions.GetFromExtendedName<Character>("Robocam"))
                });
                EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
                {
                    weight = 60,
                    selection = npcs.Find(x => x.Character == EnumExtensions.GetFromExtendedName<Character>("Pencil Boy"))
                });
                EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
                {
                    weight = 20,
                    selection = npcs.Find(x => x.Character == EnumExtensions.GetFromExtendedName<Character>("Stunly"))
                });
                EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
                {
                    weight = 70,
                    selection = npcs.Find(x => x.Character == EnumExtensions.GetFromExtendedName<Character>("Leapy"))
                });
                EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
                {
                    weight = 60,
                    selection = npcs.Find(x => x.Character == EnumExtensions.GetFromExtendedName<Character>("Watcher"))
                });
                EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
                {
                    weight = 70,
                    selection = npcs.Find(x => x.Character == EnumExtensions.GetFromExtendedName<Character>("Super Intendent Jr"))
                });
                EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC()
                {
                    weight = 80,
                    selection = npcs.Find(x => x.Character == EnumExtensions.GetFromExtendedName<Character>("Glue Boy"))
                });
                EndlessFloorsPlugin.weightedNPCs.Add(new WeightedNPC() //principal replacement that barely acts like the principal
                {
                    weight = 20,
                    selection = npcs.Find(x => x.Character == EnumExtensions.GetFromExtendedName<Character>("Magical Student"))
                });
                //i was also tempted to include 0th prize but if i recall two gotta sweep's that collide with eachother can result in infinite velocity and. nuh uh not dealing with that
            }

            EndlessFloorsPlugin.weightedItems = new WeightedItemObject[]
            {
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.Quarter),
                    weight = 60
                },
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.AlarmClock),
                    weight = 55
                },
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.Apple),
                    weight = 1
                },
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.Boots),
                    weight = 55
                },
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.Bsoda),
                    weight = 75
                },
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.ChalkEraser),
                    weight = 80
                },
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.DetentionKey),
                    weight = 40
                },
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.GrapplingHook),
                    weight = 25
                },
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.Nametag),
                    weight = 45
                },
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.Wd40),
                    weight = 60
                },
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.PortalPoster),
                    weight = 20
                },
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.PrincipalWhistle),
                    weight = 50
                },
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.Scissors),
                    weight = 80
                },
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.DoorLock),
                    weight = 42
                },
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.Tape),
                    weight = 40
                },
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.Teleporter),
                    weight = 25
                },
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.ZestyBar),
                    weight = 70
                },
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.presentObject,
                    weight = 74
                }
            };
            if (EndlessFloorsPlugin.TimesInstalled)
            {
                float moddedWeightReduction = 0.8f;
                EndlessFloorsPlugin.weightedItems = EndlessFloorsPlugin.weightedItems.AddToArray(new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == EnumExtensions.GetFromExtendedName<Items>("Hammer")),
                    weight = (int)(30 * moddedWeightReduction)
                });
                EndlessFloorsPlugin.weightedItems = EndlessFloorsPlugin.weightedItems.AddToArray(new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == EnumExtensions.GetFromExtendedName<Items>("GPS")),
                    weight = (int)(30 * moddedWeightReduction)
                });
                EndlessFloorsPlugin.weightedItems = EndlessFloorsPlugin.weightedItems.AddToArray(new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == EnumExtensions.GetFromExtendedName<Items>("Pencil")),
                    weight = (int)(40 * moddedWeightReduction)
                });
                EndlessFloorsPlugin.weightedItems = EndlessFloorsPlugin.weightedItems.AddToArray(new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == EnumExtensions.GetFromExtendedName<Items>("ScrewDriver")),
                    weight = (int)(50 * moddedWeightReduction)
                });
                EndlessFloorsPlugin.weightedItems = EndlessFloorsPlugin.weightedItems.AddToArray(new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == EnumExtensions.GetFromExtendedName<Items>("BearTrap")),
                    weight = (int)(20 * moddedWeightReduction)
                });
                EndlessFloorsPlugin.weightedItems = EndlessFloorsPlugin.weightedItems.AddToArray(new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == EnumExtensions.GetFromExtendedName<Items>("Banana")),
                    weight = (int)(65 * moddedWeightReduction)
                });
                EndlessFloorsPlugin.weightedItems = EndlessFloorsPlugin.weightedItems.AddToArray(new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == EnumExtensions.GetFromExtendedName<Items>("Gum")),
                    weight = (int)(60 * moddedWeightReduction)
                });
                EndlessFloorsPlugin.weightedItems = EndlessFloorsPlugin.weightedItems.AddToArray(new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == EnumExtensions.GetFromExtendedName<Items>("Lockpick")),
                    weight = (int)(37 * moddedWeightReduction)
                });
                EndlessFloorsPlugin.weightedItems = EndlessFloorsPlugin.weightedItems.AddToArray(new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == EnumExtensions.GetFromExtendedName<Items>("Speedpotion")),
                    weight = (int)(22 * moddedWeightReduction)
                });
                EndlessFloorsPlugin.weightedItems = EndlessFloorsPlugin.weightedItems.AddToArray(new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == EnumExtensions.GetFromExtendedName<Items>("Bsed")),
                    weight = (int)(30 * moddedWeightReduction)
                });
                EndlessFloorsPlugin.weightedItems = EndlessFloorsPlugin.weightedItems.AddToArray(new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == EnumExtensions.GetFromExtendedName<Items>("Gquarter")),
                    weight = (int)(15 * moddedWeightReduction)
                });
                EndlessFloorsPlugin.weightedItems = EndlessFloorsPlugin.weightedItems.AddToArray(new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == EnumExtensions.GetFromExtendedName<Items>("Emptybottle")),
                    weight = (int)(40 * moddedWeightReduction)
                });
                EndlessFloorsPlugin.weightedItems = EndlessFloorsPlugin.weightedItems.AddToArray(new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == EnumExtensions.GetFromExtendedName<Items>("Waterbottle")),
                    weight = (int)(35 * moddedWeightReduction)
                });
                EndlessFloorsPlugin.weightedItems = EndlessFloorsPlugin.weightedItems.AddToArray(new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == EnumExtensions.GetFromExtendedName<Items>("Hardhat")),
                    weight = (int)(30 * moddedWeightReduction)
                });
                EndlessFloorsPlugin.weightedItems = EndlessFloorsPlugin.weightedItems.AddToArray(new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == EnumExtensions.GetFromExtendedName<Items>("Headachepill")),
                    weight = (int)(30 * moddedWeightReduction)
                });
                EndlessFloorsPlugin.weightedItems = EndlessFloorsPlugin.weightedItems.AddToArray(new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == EnumExtensions.GetFromExtendedName<Items>("Basketball")),
                    weight = (int)(25 * moddedWeightReduction)
                });
            }
            /*EndlessFloorsPlugin.weightedItems = new WeightedItemObject[]
            {
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.GrapplingHook),
                    weight = 60
                },
            };*/
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

    [HarmonyPatch(typeof(ElevatorScreen))]
    [HarmonyPatch("Initialize")]
    class GetFreeUpgrades
    {
        static void Postfix(ElevatorScreen __instance)
        {
            if (Singleton<CoreGameManager>.Instance.currentMode != Mode.Main) return;
            if (EndlessFloorsPlugin.currentFloorData.FloorID == 1) return;
            if (EndlessFloorsPlugin.currentFloorData.FloorID == EndlessFloorsPlugin.Instance.selectedFloor)
            {
                if (EndlessFloorsPlugin.currentSave.hasClaimedFreeYTP) return;
                EndlessFloorsPlugin.currentSave.hasClaimedFreeYTP = true;
                Singleton<CoreGameManager>.Instance.AddPoints(FloorData.GetYTPsAtFloor(EndlessFloorsPlugin.Instance.selectedFloor - 1), 0, false);
                __instance.QueueShop();
            }
        }
    }

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
            List <NPC> ForcedNPCS = new List<NPC>
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

            __instance.ld.maxOffices = Mathf.Max(currentFD.maxOffices,1);
            __instance.ld.minOffices = 1;

            //use the regular seed without the offset so it stays consistent
            System.Random lightRng = new System.Random(Singleton<CoreGameManager>.Instance.Seed());

            Color warmColor = new Color(255f / 255f, 202 / 255f, 133 / 255f);
            Color coldColor = new Color(133f / 255f, 161f / 255f, 255f / 255f);

            float coldLight = Mathf.Max(Mathf.Sin(((currentFD.FloorID / (1f + (float)(rng.NextDouble() * 15f))) + rng.Next(-50,50))),0f);
            float warmLight = Mathf.Max(Mathf.Sin(((currentFD.FloorID / (1f + (float)(rng.NextDouble() * 15f))) + rng.Next(-50, 50))), 0f);

            __instance.ld.standardLightColor = Color.Lerp(Color.Lerp(Color.white, coldColor, coldLight), warmColor, warmLight);

            if (currentFD.FloorID % 99 == 0)
            {
                __instance.ld.standardLightColor = Color.Lerp(__instance.ld.standardLightColor,Color.red,0.3f);
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

            __instance.ld.windowChance = Mathf.Max((currentFD.FloorID * -1.2f) + 14,2);

            __instance.ld.mapPrice = currentFD.FloorID * 25;

            __instance.ld.maxPlots = currentFD.maxPlots;

            __instance.ld.minPlots = currentFD.minPlots;

            __instance.ld.outerEdgeBuffer = 3;
            
            __instance.ld.bridgeTurnChance = Mathf.CeilToInt(Mathf.Clamp(currentFD.exitCount,1f,5f) * 3f); //lol

            __instance.ld.itemChance = (int)currentFD.itemChance;

            __instance.ld.maxSideHallsToRemove = Mathf.FloorToInt(currentFD.classRoomCount / 5);
            __instance.ld.minSideHallsToRemove = Mathf.CeilToInt(currentFD.classRoomCount / 7);

            __instance.ld.maxLightDistance = Mathf.Clamp(Mathf.FloorToInt(currentFD.FloorID / 3),1,9);

            float rgb = Mathf.Max(16f, 255f - (currentFD.FloorID * 6));

            __instance.ld.standardDarkLevel = new Color(rgb / 255, rgb / 255, rgb / 255);

            __instance.ld.standardLightStrength = Mathf.Max(Mathf.RoundToInt(4f / (currentFD.FloorID / 24f)),3);

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

            int extraBuilders = rng.Next(2, Mathf.Min(2 + Mathf.FloorToInt(currentFD.FloorID / 5),12));

            for (int i = 0; i < extraBuilders; i++)
            {
                extraObjs.Add(WeightedObjectBuilder.ControlledRandomSelection(possibleExtraBuilders,rng));
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
