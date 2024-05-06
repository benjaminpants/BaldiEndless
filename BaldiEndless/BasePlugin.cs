using System;
using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using UnityEngine;
using MTM101BaldAPI.AssetTools;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MTM101BaldAPI.SaveSystem;
using BepInEx.Bootstrap;
using System.Reflection;
using UnityEngine.Rendering;
using System.Collections;
using System.Xml.Linq;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.ObjectCreation;

namespace BaldiEndless
{

    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi")]
    [BepInPlugin("mtm101.rulerp.baldiplus.endlessfloors", "Endless Floors", "2.2.1.0")]
    public class EndlessFloorsPlugin : BaseUnityPlugin
    {
        public AssetManager assetManager = new AssetManager();
        public Dictionary<string, Sprite> UpgradeIcons = new Dictionary<string, Sprite>();
        public SceneObject[] SceneObjects;
        public static SceneObject currentSceneObject;

        public static Dictionary<string, StandardUpgrade> Upgrades = new Dictionary<string, StandardUpgrade>();

        public static EndlessFloorsPlugin Instance { get; private set; }

        public static Mode NNFloorMode = EnumExtensions.ExtendEnum<Mode>("Floor99");

        public static string F99MusicStart = "";
        public static string F99MusicLoop = "";

        internal static Dictionary<PluginInfo, Action<GeneratorData>> genActions = new Dictionary<PluginInfo, Action<GeneratorData>>();

        public static void AddGeneratorAction(PluginInfo info, Action<GeneratorData> data)
        {
            if (genActions.ContainsKey(info))
            {
                throw new Exception("Can't add already existing generator action!");
            }
            genActions.Add(info, data);
        }

        public int highestFloorCount = 1;
        //public int[] highestFloorStreak = new int[2] {1, 1};
        public int selectedFloor = 1;
        public static int lastGenMaxNpcs = 0;

        public string lastAllocatedPath = "";

        public static Items presentEnum = EnumExtensions.ExtendEnum<Items>("Present");
        public static ItemObject presentObject;

        public static EndlessSaveData mainSave = new EndlessSaveData();
        internal static EndlessSaveData freeSave = new EndlessSaveData();
        internal static EndlessSaveData _99Save = new EndlessSaveData();

        public static EndlessSaveData currentSave
        {
            get
            {
                if (Singleton<CoreGameManager>.Instance != null)
                {
                    if (Singleton<CoreGameManager>.Instance.currentMode == NNFloorMode)
                    {
                        return _99Save;
                    }
                    else if (Singleton<CoreGameManager>.Instance.currentMode == Mode.Free)
                    {
                        return freeSave;
                    }
                }
                return mainSave;
            }
            set
            {
                if (Singleton<CoreGameManager>.Instance != null)
                {
                    if (Singleton<CoreGameManager>.Instance.currentMode == NNFloorMode)
                    {
                        _99Save = value;
                        return;
                    }
                    else if (Singleton<CoreGameManager>.Instance.currentMode == Mode.Free)
                    {
                        freeSave = value;
                        return;
                    }
                }
                mainSave = value;
            }
        }

        public static FloorData currentFloorData => currentSave.myFloorData;

        public static Texture2D upgradeTex5 => (Texture2D)Instance.assetManager[typeof(Texture2D), "UpgradeSlot5"];
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

        void AddWeightedTextures(ref List<WeightedTexture2D> tex, string folder)
        {
            string myPath = AssetLoader.GetModPath(this);
            string wallsPath = Path.Combine(myPath, "Textures", folder);
            foreach (string p in Directory.GetFiles(wallsPath))
            {
                string standardName = Path.GetFileNameWithoutExtension(p);
                Texture2D texx = AssetLoader.TextureFromFile(p);
                string[] splitee = standardName.Split('!');
                tex.Add(new WeightedTexture2D()
                {
                    selection = texx,
                    weight = int.Parse(splitee[1])
                });
            }
        }

        // once all resources have been loaded
        void OnResourcesLoaded()
        {
            Texture2D presentTex = AssetLoader.TextureFromMod(this, "PresentIcon_Large.png");
            Sprite presentSprite = AssetLoader.SpriteFromTexture2D(presentTex, Vector2.one / 2, 50f);
            presentObject = new ItemBuilder(Info)
                .SetNameAndDescription("Itm_Present", "Itm_Present")
                .SetSprites(presentSprite, presentSprite)
                .SetAsInstantUse()
                .SetItemComponent<ITM_Present>()
                .SetGeneratorCost(26)
                .SetShopPrice(int.MaxValue)
                .SetEnum(presentEnum)
                .Build();
            assetManager.AddRange<HappyBaldi>(Resources.FindObjectsOfTypeAll<HappyBaldi>(), (bald) =>
            {
                if (bald.name == "HappyBaldi") return "HappyBaldi1";
                return bald.name;
            }); //we need the happy baldis
            assetManager.AddRange<Cubemap>(Resources.FindObjectsOfTypeAll<Cubemap>().Where(x => (x.name == "Cubemap_DayStandard" || x.name == "Cubemap_Twilight")).ToArray());
            Cubemap darkCubemap = AssetLoader.CubemapFromTexture(AssetLoader.TextureFromMod(this, "DarkSky_OneImage.png"));
            /*Cubemap templateMap = (Cubemap)assetManager[typeof(Cubemap), "Cubemap_Default"];
            Cubemap newCubemap = new Cubemap(256, templateMap.format, false);
            newCubemap.filterMode = FilterMode.Point;
            Texture2D tex = AssetLoader.TextureFromMod(this, "DarkSky.png");
            newCubemap.SetPixels(tex.GetPixels(), CubemapFace.PositiveY);
            newCubemap.SetPixels(tex.GetPixels(), CubemapFace.PositiveX);
            newCubemap.SetPixels(tex.GetPixels(), CubemapFace.PositiveZ);
            newCubemap.SetPixels(tex.GetPixels(), CubemapFace.NegativeZ);
            newCubemap.SetPixels(tex.GetPixels(), CubemapFace.NegativeX);
            newCubemap.SetPixels(tex.GetPixels(), CubemapFace.NegativeY);
            newCubemap.name = "Cubemap_DarkSky";
            newCubemap.Apply();*/
            darkCubemap.name = "Cubemap_DarkSky";
            SkyboxMetaStorage.Instance.Add(new SkyboxMetadata(Info, darkCubemap, new Color(46f / 255f, 52f / 255f, 68f / 255f)));
            SkyboxMetaStorage.Instance.Find(x => x.value.name == "Cubemap_Twilight").lightColor = new Color(239f / 255f, 188f / 255f, 162f / 255f);
            assetManager.Add<Cubemap>("Cubemap_DarkSky", darkCubemap);
            SceneObjects = Resources.FindObjectsOfTypeAll<SceneObject>();
            currentSceneObject = SceneObjects.Where(x => x.levelTitle == "F3").First();
            float baseGameMultiplier = 1.5f;
            // add back in the base game textures
            EndlessFloorsPlugin.wallTextures.AddRange(currentSceneObject.levelObject.classWallTexs.Select(x => new WeightedTexture2D() { weight = Mathf.RoundToInt(x.weight * baseGameMultiplier), selection = x.selection }));
            EndlessFloorsPlugin.wallTextures.AddRange(currentSceneObject.levelObject.hallWallTexs.Select(x => new WeightedTexture2D() { weight = Mathf.RoundToInt(x.weight * baseGameMultiplier), selection = x.selection }));
            EndlessFloorsPlugin.facultyWallTextures.AddRange(currentSceneObject.levelObject.facultyWallTexs.Select(x => new WeightedTexture2D() { weight = Mathf.RoundToInt(x.weight * baseGameMultiplier), selection = x.selection }));
            EndlessFloorsPlugin.ceilTextures.AddRange(currentSceneObject.levelObject.hallCeilingTexs.Select(x => new WeightedTexture2D() { weight = Mathf.RoundToInt(x.weight * baseGameMultiplier), selection = x.selection }));
            EndlessFloorsPlugin.ceilTextures.AddRange(currentSceneObject.levelObject.classCeilingTexs.Select(x => new WeightedTexture2D() { weight = Mathf.RoundToInt(x.weight * baseGameMultiplier), selection = x.selection }));
            EndlessFloorsPlugin.ceilTextures.AddRange(currentSceneObject.levelObject.facultyCeilingTexs.Select(x => new WeightedTexture2D() { weight = Mathf.RoundToInt(x.weight * baseGameMultiplier), selection = x.selection }));
            EndlessFloorsPlugin.floorTextures.AddRange(currentSceneObject.levelObject.hallFloorTexs.Select(x => new WeightedTexture2D() { weight = Mathf.RoundToInt(x.weight * baseGameMultiplier), selection = x.selection }));
            EndlessFloorsPlugin.profFloorTextures.AddRange(currentSceneObject.levelObject.facultyFloorTexs.Select(x => new WeightedTexture2D() { weight = Mathf.RoundToInt(x.weight * baseGameMultiplier), selection = x.selection }));
            EndlessFloorsPlugin.profFloorTextures.AddRange(currentSceneObject.levelObject.classFloorTexs.Select(x => new WeightedTexture2D() { weight = Mathf.RoundToInt(x.weight * baseGameMultiplier), selection = x.selection }));
            // obliterate CONVEYOR BELT SUBTITLES
            Resources.FindObjectsOfTypeAll<SoundObject>().Where(x => x.name == "ConveyorBeltLoop").First().subtitle = false; //MYSTMAN12!!!!!!
            SoundObject allNotebooks = Resources.FindObjectsOfTypeAll<SoundObject>().Where(x => x.name == "BAL_AllNotebooks_9").First();
            allNotebooks.subtitle = false;
            allNotebooks.soundClip = AssetLoader.AudioClipFromMod(this, "BAL_Notebooks.wav");
            allNotebooks.MarkAsNeverUnload();
            ItemMetaStorage items = MTM101BaldiDevAPI.itemMetadata;
            // TODO: make this not hard coded anymore.
            ITM_Present.potentialObjects.AddRange(new WeightedItemObject[]
            {
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.Quarter).value,
                    weight = 60
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.AlarmClock).value,
                    weight = 55
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.Apple).value,
                    weight = 10
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.Boots).value,
                    weight = 55
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.ChalkEraser).value,
                    weight = 80
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.DetentionKey).value,
                    weight = 40
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.GrapplingHook).value,
                    weight = 25
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.Nametag).value,
                    weight = 45
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.Wd40).value,
                    weight = 60
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.PortalPoster).value,
                    weight = 20
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.PrincipalWhistle).value,
                    weight = 50
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.Scissors).value,
                    weight = 80
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.DoorLock).value,
                    weight = 42
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.Tape).value,
                    weight = 40
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.Teleporter).value,
                    weight = 25
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.ZestyBar).value,
                    weight = 70
                },
            });
        }

        internal static void ExtendGenData(GeneratorData genData)
        {
            NPCMetaStorage npcs = MTM101BaldiDevAPI.npcMetadata;
            RoomAssetMetaStorage rooms = MTM101BaldiDevAPI.roomAssetMeta;
            ObjectBuilderMetaStorage objs = MTM101BaldiDevAPI.objBuilderMeta;
            RandomEventMetaStorage rngs = MTM101BaldiDevAPI.randomEventStorage;
            ItemMetaStorage items = MTM101BaldiDevAPI.itemMetadata;
            genData.fieldTrips.Add(new WeightedFieldTrip()
            {
                weight=150,
                selection=Resources.FindObjectsOfTypeAll<FieldTripObject>().Where(x => x.trip == FieldTrips.Camping).First()
            });
            genData.npcs.AddRange(new WeightedNPC[]
            {
                new WeightedNPC() {
                    weight = 90,
                    selection = npcs.Get(Character.Playtime).value
                },
                new WeightedNPC() {
                    weight = 100,
                    selection = npcs.Get(Character.Sweep).value
                },
                new WeightedNPC() {
                    weight = 110,
                    selection = npcs.Get(Character.Beans).value
                },
                new WeightedNPC() {
                    weight = 85,
                    selection = npcs.Get(Character.Bully).value
                },
                new WeightedNPC() {
                    weight = 80,
                    selection = npcs.Get(Character.Crafters).value
                },
                new WeightedNPC() { //chalkles is actual hell to deal with since 0.4
                    weight = 80,
                    selection = npcs.Get(Character.Chalkles).value
                },
                new WeightedNPC() {
                    weight = 10,
                    selection = npcs.Get(Character.LookAt).value
                },
                new WeightedNPC() {
                    weight = 90,
                    selection = npcs.Get(Character.Pomp).value
                },
                new WeightedNPC() {
                    weight = 95,
                    selection = npcs.Get(Character.Cumulo).value
                },
                new WeightedNPC() {
                    weight = 70,
                    selection = npcs.Get(Character.Prize).value
                },
                new WeightedNPC() {
                    weight = 90,
                    selection = npcs.Get(Character.DrReflex).value
                },
            });
            genData.forcedNpcs.Add(npcs.Get(Character.Principal).value);
            genData.classRoomAssets.AddRange(new WeightedRoomAsset[]
            {
                new WeightedRoomAsset() {
                    weight = 75,
                    selection = rooms.Get(RoomCategory.Class, "Room_Class_NoActivity_0").value
                },
                new WeightedRoomAsset() {
                    weight = 75,
                    selection = rooms.Get(RoomCategory.Class, "Room_Class_NoActivity_1").value
                },
                new WeightedRoomAsset() {
                    weight = 100,
                    selection = rooms.Get(RoomCategory.Class, "Room_Class_NoActivity_3").value
                },
                new WeightedRoomAsset() {
                    weight = 100,
                    selection = rooms.Get(RoomCategory.Class, "Room_Class_NoActivity_4").value
                },
                new WeightedRoomAsset() {
                    weight = 75,
                    selection = rooms.Get(RoomCategory.Class, "Room_Class_MathMachine_0").value
                },
                new WeightedRoomAsset() {
                    weight = 75,
                    selection = rooms.Get(RoomCategory.Class, "Room_Class_MathMachine_1").value
                },
                new WeightedRoomAsset() {
                    weight = 100,
                    selection = rooms.Get(RoomCategory.Class, "Room_Class_MathMachine_2").value
                },
                new WeightedRoomAsset() {
                    weight = 100,
                    selection = rooms.Get(RoomCategory.Class, "Room_Class_MathMachine_3").value
                },
                new WeightedRoomAsset() {
                    weight = 50,
                    selection = rooms.Get(RoomCategory.Class, "Room_Class_MathMachine_4").value
                }
            });
            genData.facultyRoomAssets.AddRange(new WeightedRoomAsset[]
            {
                new WeightedRoomAsset() {
                    weight = 100,
                    selection = rooms.Get(RoomCategory.Faculty, "Room_Faculty_School_2").value
                },
                new WeightedRoomAsset() {
                    weight = 50,
                    selection = rooms.Get(RoomCategory.Faculty, "Room_Faculty_School_6").value
                },
                new WeightedRoomAsset() {
                    weight = 100,
                    selection = rooms.Get(RoomCategory.Faculty, "Room_Faculty_School_7").value
                },
                new WeightedRoomAsset() {
                    weight = 50,
                    selection = rooms.Get(RoomCategory.Faculty, "Room_Faculty_School_9").value
                },
                new WeightedRoomAsset() {
                    weight = 50,
                    selection = rooms.Get(RoomCategory.Faculty, "Room_Faculty_School_11").value
                },
                new WeightedRoomAsset() {
                    weight = 25,
                    selection = rooms.Get(RoomCategory.Faculty, "Room_Faculty_School_0").value
                },
                new WeightedRoomAsset() {
                    weight = 50,
                    selection = rooms.Get(RoomCategory.Faculty, "Room_Faculty_School_3").value
                },
                new WeightedRoomAsset() {
                    weight = 50,
                    selection = rooms.Get(RoomCategory.Faculty, "Room_Faculty_School_4").value
                },
                new WeightedRoomAsset() {
                    weight = 25,
                    selection = rooms.Get(RoomCategory.Faculty, "Room_Faculty_School_5").value
                },
                new WeightedRoomAsset() {
                    weight = 75,
                    selection = rooms.Get(RoomCategory.Faculty, "Room_Faculty_School_6").value
                },
                new WeightedRoomAsset() {
                    weight = 10,
                    selection = rooms.Get(RoomCategory.Faculty, "Room_Faculty_School_8").value
                },
                new WeightedRoomAsset() {
                    weight = 75,
                    selection = rooms.Get(RoomCategory.Faculty, "Room_Faculty_School_10").value
                },
                new WeightedRoomAsset() {
                    weight = 10,
                    selection = rooms.Get(RoomCategory.Faculty, "Room_Faculty_School_12").value
                },
                new WeightedRoomAsset() {
                    weight = 10,
                    selection = rooms.Get(RoomCategory.Faculty, "Room_Faculty_School_1").value
                },
            });
            genData.forcedObjectBuilders.AddRange(new ObjectBuilder[]
            {
                objs.Get(Obstacle.Null, "SwingDoorBuilder").value,
                objs.Get(Obstacle.Null, "PlantBuilder").value,
            });
            genData.objectBuilders.AddRange(new WeightedObjectBuilder[]
            {
                new WeightedObjectBuilder()
                {
                    selection = objs.Get(Obstacle.Null, "PayphoneBuilder").value,
                    weight = 60
                },
                new WeightedObjectBuilder()
                {
                    selection = objs.Get(Obstacle.Null, "BsodaHallBuilder").value,
                    weight = 100
                },
                new WeightedObjectBuilder()
                {
                    selection = objs.Get(Obstacle.Null, "ZestyHallBuilder").value,
                    weight = 100
                },
                new WeightedObjectBuilder()
                {
                    selection = objs.Get(Obstacle.Fountain, "WaterFountainHallBuilder").value,
                    weight = 80
                }
            });
            genData.objectBuilders.AddRange(new WeightedObjectBuilder[]
            {
                new WeightedObjectBuilder()
                {
                    selection = objs.Get(Obstacle.Conveyor, "ConveyorBeltBuilder").value,
                    weight = 110
                },
                new WeightedObjectBuilder()
                {
                    selection = objs.Get(Obstacle.CoinDoor, "CoinDoorBuilder").value,
                    weight = 90
                },
                new WeightedObjectBuilder()
                {
                    selection = objs.Get(Obstacle.OneWaySwing, "OneWayDoorBuilder").value,
                    weight = 80
                },
                new WeightedObjectBuilder()
                {
                    selection = objs.Get(Obstacle.LockdownDoor, "LockdownDoorBuilder").value,
                    weight = 85
                },
                new WeightedObjectBuilder()
                {
                    selection = objs.Get(Obstacle.Null, "RotoHallBuilder").value,
                    weight = 90
                }
            });
            genData.randomEvents.AddRange(new WeightedRandomEvent[]
            {
                new WeightedRandomEvent()
                {
                    selection = rngs.Get(RandomEventType.Fog).value,
                    weight = 150
                },
                new WeightedRandomEvent()
                {
                    selection = rngs.Get(RandomEventType.Party).value,
                    weight = 125
                },
                new WeightedRandomEvent()
                {
                    selection = rngs.Get(RandomEventType.Snap).value,
                    weight = 70
                },
                new WeightedRandomEvent()
                {
                    selection = rngs.Get(RandomEventType.Flood).value,
                    weight = 90
                },
                new WeightedRandomEvent()
                {
                    selection = rngs.Get(RandomEventType.Lockdown).value,
                    weight = 65
                },
                new WeightedRandomEvent()
                {
                    selection = rngs.Get(RandomEventType.Gravity).value,
                    weight = 55
                },
                new WeightedRandomEvent()
                {
                    selection = rngs.Get(RandomEventType.MysteryRoom).value,
                    weight = 50
                }
            });
            genData.items.AddRange(new WeightedItemObject[]
            {
                new WeightedItemObject() 
                {
                    selection = items.FindByEnum(Items.Quarter).value,
                    weight = 100
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.AlarmClock).value,
                    weight = 76
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.Apple).value,
                    weight = 1
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.Boots).value,
                    weight = 75
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.ChalkEraser).value,
                    weight = 100
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.DetentionKey).value,
                    weight = 60
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.GrapplingHook).value,
                    weight = 45
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.Nametag).value,
                    weight = 65
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.Wd40).value,
                    weight = 80
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.PortalPoster).value,
                    weight = 40
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.PrincipalWhistle).value,
                    weight = 70
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.Scissors).value,
                    weight = 100
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.DoorLock).value,
                    weight = 62
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.Tape).value,
                    weight = 60
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.Teleporter).value,
                    weight = 45
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.ZestyBar).value,
                    weight = 90
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.Bsoda).value,
                    weight = 40
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.NanaPeel).value,
                    weight = 85
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.Points).value, //add 100 point items
                    weight = 5
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.Points).itemObjects[1], //add 50 point item
                    weight = 10
                },
                new WeightedItemObject()
                {
                    selection = items.FindByEnum(Items.Points).itemObjects[0], //add 25 point items
                    weight = 15
                },
                new WeightedItemObject()
                {
                    selection = EndlessFloorsPlugin.presentObject,
                    weight = 80
                }
            });
            genData.specialRoomAssets.AddRange(new WeightedRoomAsset[] { 
                new WeightedRoomAsset()
                {
                    weight = 190,
                    selection = rooms.Get(RoomCategory.Special, "Room_Cafeteria_1").value
                },
                new WeightedRoomAsset()
                {
                    weight = 190,
                    selection = rooms.Get(RoomCategory.Special, "Room_Cafeteria_2").value
                },
                new WeightedRoomAsset()
                {
                    weight = 190,
                    selection = rooms.Get(RoomCategory.Special, "Room_Cafeteria_3").value
                },
                new WeightedRoomAsset()
                {
                    weight = 115,
                    selection = rooms.Get(RoomCategory.Special, "Room_Cafeteria_Hard_1").value
                },
                new WeightedRoomAsset()
                {
                    weight = 115,
                    selection = rooms.Get(RoomCategory.Special, "Room_Cafeteria_Hard_2").value
                },
                new WeightedRoomAsset()
                {
                    weight = 200,
                    selection = rooms.Get(RoomCategory.Special, "Room_Library_1").value
                },
                new WeightedRoomAsset()
                {
                    weight = 200,
                    selection = rooms.Get(RoomCategory.Special, "Room_Library_2").value
                },
                new WeightedRoomAsset()
                {
                    weight = 200,
                    selection = rooms.Get(RoomCategory.Special, "Room_Library_3").value
                },
                new WeightedRoomAsset()
                {
                    weight = 200,
                    selection = rooms.Get(RoomCategory.Special, "Room_Playground_1").value
                },
                new WeightedRoomAsset()
                {
                    weight = 200,
                    selection = rooms.Get(RoomCategory.Special, "Room_Playground_2").value
                },
                new WeightedRoomAsset()
                {
                    weight = 200,
                    selection = rooms.Get(RoomCategory.Special, "Room_Playground_3").value
                }
            });
            genData.officeRoomAssets.AddRange(new WeightedRoomAsset[] {
                new WeightedRoomAsset()
                {
                    weight = 100,
                    selection = rooms.Get(RoomCategory.Office, "Room_Office_0").value
                }
            });
            genData.hallInsertions.AddRange(new WeightedRoomAsset[] {
                new WeightedRoomAsset()
                {
                    weight = 100,
                    selection = rooms.Get(RoomCategory.Hall, "Room_HallFormation_0").value
                },
                new WeightedRoomAsset()
                {
                    weight = 100,
                    selection = rooms.Get(RoomCategory.Hall, "Room_HallFormation_1").value
                },
                new WeightedRoomAsset()
                {
                    weight = 100,
                    selection = rooms.Get(RoomCategory.Hall, "Room_HallFormation_2").value
                }
            });
            foreach (KeyValuePair<PluginInfo, Action<GeneratorData>> kvp in genActions)
            {
                try
                {
                    kvp.Value.Invoke(genData);
                }
                catch (Exception e)
                {
                    MTM101BaldiDevAPI.CauseCrash(kvp.Key, e);
                }
            }
        }

        void Awake()
        {
            Instance = this;
            Harmony harmony = new Harmony("mtm101.rulerp.baldiplus.endlessfloors");
            string myPath = AssetLoader.GetModPath(this);
            string iconPath = Path.Combine(myPath, "UpgradeIcons");
            foreach (string p in Directory.GetFiles(iconPath))
            {
                Texture2D tex = AssetLoader.TextureFromFile(p);
                Sprite spr = AssetLoader.SpriteFromTexture2D(tex, Vector2.one / 2f, 50f);
                UpgradeIcons.Add(Path.GetFileNameWithoutExtension(p), spr);
            }

            string wallsPath = Path.Combine(myPath, "Textures", "Walls");
            foreach (string p in Directory.GetFiles(wallsPath))
            {
                string standardName = Path.GetFileNameWithoutExtension(p);
                if (standardName.StartsWith("F_")) continue; // no.
                Texture2D tex = AssetLoader.TextureFromFile(p);
                string[] splitee = standardName.Split('!');
                wallTextures.Add(new WeightedTexture2D()
                {
                    selection = tex,
                    weight = int.Parse(splitee[1])
                });
                string facultyEquiv = Path.Combine(wallsPath, "F_" + splitee[0] + ".png");
                if (File.Exists(facultyEquiv))
                {
                    Texture2D texf = AssetLoader.TextureFromFile(facultyEquiv);
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

            assetManager.Add("UpgradeSlot5", AssetLoader.TextureFromMod(this, "UpgradeSlot5.png"));
            assetManager.Add("OutOfOrderSlot", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "MissingSlot.png"), 1f));
            assetManager.Add("TimeSlow", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "Effects", "TimeSlow.wav"), "Sfx_TimeSlow", SoundType.Effect, Color.blue));
            assetManager.Add("TimeFast", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "Effects", "TimeFastFast.wav"), "Sfx_TimeFast", SoundType.Effect, Color.blue));
            ModdedSaveSystem.AddSaveLoadAction(this, SaveLoadHighestFloor);
            StartCoroutine(WaitTilAllLoaded(harmony));

            //string myPath = AssetLoader.GetModPath(EndlessFloorsPlugin.Instance);
            string midiPath = Path.Combine(myPath, "Midi");
            EndlessFloorsPlugin.F99MusicStart = AssetLoader.MidiFromFile(Path.Combine(midiPath, "floor_99_finale_beginning.mid"), "99start");
            EndlessFloorsPlugin.F99MusicLoop = AssetLoader.MidiFromFile(Path.Combine(midiPath, "floor_99_finale_loop.mid"), "99loop");
            EndlessUpgradeRegisters.Register(new StandardUpgrade()
            {
                id = "none",
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel()
                    {
                        icon="NO",
                        cost=0,
                        descLoca="Upg_None"
                    }
                },
                weight = 0
            });
            EndlessUpgradeRegisters.Register(new BrokenUpgrade()
            {
                id = "error",
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel()
                    {
                        icon="Error",
                        cost=0,
                        descLoca="Upg_Error"
                    }
                },
                weight = 0,
                behavior=UpgradePurchaseBehavior.Nothing
            });
            EndlessUpgradeRegisters.RegisterDefaults();
            MTM101BaldAPI.SaveSystem.ModdedSaveGame.AddSaveHandler(new EndlessFloorsSaveHandler());
            MTM101BaldAPI.Registers.LoadingEvents.RegisterOnAssetsLoaded(Info, OnResourcesLoaded, true);

            assetManager.AddRange<Sprite>(new Sprite[] { AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Tubes4.png"), 1f), AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Tubes5.png"), 1f), AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Tubes6.png"), 1f) }, new string[] { "LifeTubes4", "LifeTubes5", "LifeTubes6"});

        }

        private IEnumerator WaitTilAllLoaded(Harmony harmony)
        {
            FieldInfo loaded = AccessTools.Field(typeof(Chainloader), "_loaded");

            while (!loaded.GetValue<bool>(null))
            {
                yield return null;
            }
            harmony.PatchAllConditionals();
        }

        public void UpdateData(ref SceneObject sceneObject)
        {
            sceneObject = currentSceneObject;
            sceneObject.levelNo = currentSave.currentFloor;
            sceneObject.nextLevel = sceneObject;
            sceneObject.levelTitle = "F" + currentSave.currentFloor;
            System.Random random = new System.Random(currentSave.currentFloor + Singleton<CoreGameManager>.Instance.Seed());
            random.Next();
            sceneObject.manager.ReflectionSetVariable("happyBaldiPre", assetManager[typeof(HappyBaldi), "HappyBaldi" + random.Next(1, 3)]);
            random = new System.Random((currentSave.currentFloor * 2) + Singleton<CoreGameManager>.Instance.Seed());
            random.Next();
            random.Next();
            float timeOffset = (float)(random.NextDouble() * 256f) - 128f;
            Cubemap[] boxses = new Cubemap[3] {
                (Cubemap)EndlessFloorsPlugin.Instance.assetManager[typeof(Cubemap), "Cubemap_DayStandard"],
                (Cubemap)EndlessFloorsPlugin.Instance.assetManager[typeof(Cubemap), "Cubemap_Twilight"],
                (Cubemap)EndlessFloorsPlugin.Instance.assetManager[typeof(Cubemap), "Cubemap_DarkSky"]
            };
            int skyboxIndex = Mathf.RoundToInt(Mathf.Cos(((float)(currentSave.currentFloor) / 6f) + timeOffset)) + 1;
            sceneObject.skybox = boxses[skyboxIndex];
        }
    }


}
