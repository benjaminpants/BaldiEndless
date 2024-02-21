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

namespace BaldiEndless
{

    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi")]
    [BepInPlugin("mtm101.rulerp.baldiplus.endlessfloors", "Endless Floors", "2.0.0.0")]
    public class EndlessFloorsPlugin : BaseUnityPlugin
    {
        public AssetManager assetManager = new AssetManager();
        public Dictionary<string, Sprite> UpgradeIcons = new Dictionary<string, Sprite>();
        public SceneObject[] SceneObjects;
        public static SceneObject currentSceneObject;

        public static GeneratorData genData = new GeneratorData();

        public static EndlessFloorsPlugin Instance { get; private set; }

        public static Mode NNFloorMode = EnumExtensions.ExtendEnum<Mode>("Floor99");

        public static string F99MusicStart = "";
        public static string F99MusicLoop = "";

        public int highestFloorCount = 1;
        public int selectedFloor = 1;

        public string lastAllocatedPath = "";

        public static Items presentEnum = EnumExtensions.ExtendEnum<Items>("Present");
        public static ItemObject presentObject;

        public static EndlessSaveData currentSave = new EndlessSaveData();

        public static FloorData currentFloorData => currentSave.myFloorData;

        public static Texture2D upgradeTex5;
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
            assetManager.AddRange<HappyBaldi>(Resources.FindObjectsOfTypeAll<HappyBaldi>(), (bald) =>
            {
                if (bald.name == "HappyBaldi") return "HappyBaldi1";
                return bald.name;
            }); //we need the happy baldis
        }

        void Awake()
        {
            Instance = this;
            Harmony harmony = new Harmony("mtm101.rulerp.baldiplus.endlessfloors");
            MTM101BaldiDevAPI.SavesEnabled = false;
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

            upgradeTex5 = AssetLoader.TextureFromMod(this, "UpgradeSlot5.png");

            Texture2D presentTex = AssetLoader.TextureFromMod(this, "PresentIcon_Large.png");
            Sprite presentSprite = AssetLoader.SpriteFromTexture2D(presentTex, Vector2.one / 2, 50f);
            presentObject = ObjectCreators.CreateItemObject("Itm_Present", "Itm_Present", presentSprite, presentSprite, presentEnum, 9999, 26);
            DontDestroyOnLoad(presentObject.item = new GameObject().AddComponent<ITM_Present>()); // WHAT THE FUCK THIS IS ACTUALLY VALID SYNTAX I WAS FUCKING JOKING
            ModdedSaveSystem.AddSaveLoadAction(this, SaveLoadHighestFloor);
            StartCoroutine(WaitTilAllLoaded(harmony));

            //string myPath = AssetLoader.GetModPath(EndlessFloorsPlugin.Instance);
            string midiPath = Path.Combine(myPath, "Midi");
            EndlessFloorsPlugin.F99MusicStart = AssetLoader.MidiFromFile(Path.Combine(midiPath, "floor_99_finale_beginning.mid"), "99start");
            EndlessFloorsPlugin.F99MusicLoop = AssetLoader.MidiFromFile(Path.Combine(midiPath, "floor_99_finale_loop.mid"), "99loop");

            MTM101BaldAPI.Registers.LoadingEvents.RegisterOnAssetsLoaded(OnResourcesLoaded, true);

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
            //sceneObject.levelNo = currentSave.currentFloorData.FloorID;
            sceneObject.nextLevel = sceneObject;
            //sceneObject.levelTitle = "F" + currentSave.currentFloorData.FloorID;
        }
    }


}
