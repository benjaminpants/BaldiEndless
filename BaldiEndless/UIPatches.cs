using HarmonyLib;
using MTM101BaldAPI.SaveSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace BaldiEndless
{

    public class EndlessTitleUI : MonoBehaviour
    {
        FloorChanger fc;
        TextLocalizer f99Local; //i dont know why i have to fucking do this but i do so. GRAHGHGHGHGUHSFJITEFDNSHIJBRJIENJHIE JCEDFMBHO *I($Y*(#(*WEJBGFDKNMZXKNJMDXZCVIOHS(*Y(*EG W

        bool has_incre = false;

        void OnEnable()
        {
            if (fc == null) return;
            fc.floorNumber = 0;
            fc.Increment();
            f99Local.GetLocalizedText("But_Floor99");
        }

        void Start()
        {
            GameObject.Destroy(gameObject.transform.Find("Endless").gameObject);
            GameObject.Destroy(gameObject.transform.Find("Challenge").gameObject);
            GameObject.Destroy(gameObject.transform.Find("FieldTrips").gameObject);
            GameObject.Destroy(gameObject.transform.Find("Endless").gameObject);
            Transform theFree = gameObject.transform.Find("Free");
            Transform theMain = gameObject.transform.Find("MainNew");
            theMain.gameObject.SetActive(true);
            Transform modeText = gameObject.transform.Find("ModeText");
            Transform seedInput = gameObject.transform.Find("SeedInput");
            seedInput.gameObject.SetActive(true);
            theFree.localPosition = new Vector3(0f, theFree.localPosition.y - 48f, theFree.localPosition.z);
            theMain.localPosition -= new Vector3(0f, 48f,0f);
            StandardMenuButton clone = GameObject.Instantiate(theFree.gameObject).GetComponent<StandardMenuButton>();
            clone.transform.parent = theMain.transform.parent;
            clone.transform.localScale = theMain.transform.localScale; //what the fuck
            clone.OnPress = new UnityEvent();
            clone.OnHighlight = new UnityEvent();
            clone.transform.localPosition = theMain.transform.localPosition + new Vector3(0f, 48f, 0f); //go back
            clone.transform.SetSiblingIndex(1);
            clone.name = "FloorChanger";
            fc = clone.gameObject.AddComponent<FloorChanger>();
            clone.OnPress.AddListener(() =>
            {
                fc.Increment();
            });
            StandardMenuButton mainB = theMain.GetComponent<StandardMenuButton>();
            mainB.OnPress = new UnityEvent();
            mainB.OnPress.AddListener(() =>
            {
                bool saveAvailable = Singleton<ModdedFileManager>.Instance.saveData.saveAvailable;
                GameLoader gl = Resources.FindObjectsOfTypeAll<GameLoader>().First();
                gl.gameObject.SetActive(true);
                if (!saveAvailable)
                {
                    gl.CheckSeed();
                    gl.Initialize(2);
                    gl.SetMode((int)Mode.Main);
                    // reset everything save related
                    EndlessFloorsPlugin.mainSave = new EndlessSaveData();
                    EndlessFloorsPlugin.mainSave.startingFloor = EndlessFloorsPlugin.Instance.selectedFloor;
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
                }
                else
                {
                    EndlessFloorsPlugin.Instance.selectedFloor = 1; //so all my old hacky code presumes we are starting an entirely new game?
                    Singleton<ModdedFileManager>.Instance.CreateSavedGameCoreManager(gl);
                    gl.SetMode((int)Mode.Main);
                    Singleton<CursorManager>.Instance.LockCursor();
                }
                ElevatorScreen evl = SceneManager.GetActiveScene().GetRootGameObjects().Where(x => x.name == "ElevatorScreen").First().GetComponent<ElevatorScreen>();
                gl.AssignElevatorScreen(evl);
                evl.gameObject.SetActive(true);
                gl.LoadLevel(EndlessFloorsPlugin.currentSceneObject);
                evl.Initialize();
                if (!saveAvailable)
                {
                    if (EndlessFloorsPlugin.Instance.selectedFloor != 1)
                    {
                        evl.QueueShop();
                    }
                }
                gl.SetSave(true);
                if (saveAvailable)
                {
                    Singleton<ModdedFileManager>.Instance.DeleteSavedGame();
                }
            });
            mainB.OnHighlight = new UnityEvent();
            mainB.OnHighlight.AddListener(() =>
            {
                if (Singleton<ModdedFileManager>.Instance.saveData.saveAvailable)
                {
                    modeText.gameObject.GetComponent<TextLocalizer>().GetLocalizedText("Men_ContDesc");
                    TextMeshProUGUI ugui = modeText.gameObject.GetComponent<TextMeshProUGUI>();
                    ugui.text = String.Format(ugui.text, EndlessFloorsPlugin.mainSave.currentFloor, EndlessFloorsPlugin.mainSave.startingFloor);
                }
                else
                {
                    modeText.gameObject.GetComponent<TextLocalizer>().GetLocalizedText("Men_MainDesc");
                }
            });

            StandardMenuButton mText = GameObject.Instantiate(theFree.gameObject).GetComponent<StandardMenuButton>();
            mText.transform.parent = theMain.transform.parent;
            mText.transform.localScale = theMain.transform.localScale; //what the fuck
            mText.OnPress = new UnityEvent();
            mText.OnHighlight = new UnityEvent();
            mText.transform.localPosition = theFree.localPosition - new Vector3(0f, 48f, 0f); //go down
            mText.transform.SetSiblingIndex(1);
            mText.name = "99Challenge";
            f99Local = mText.GetComponent<TextLocalizer>();
            mText.OnPress.AddListener(() =>
            {
                GameLoader gl = Resources.FindObjectsOfTypeAll<GameLoader>().First();
                gl.gameObject.SetActive(true);
                gl.CheckSeed();
                gl.Initialize(0);
                gl.SetMode((int)EndlessFloorsPlugin.NNFloorMode);
                ElevatorScreen evl = SceneManager.GetActiveScene().GetRootGameObjects().Where(x => x.name == "ElevatorScreen").First().GetComponent<ElevatorScreen>();
                gl.AssignElevatorScreen(evl);
                evl.gameObject.SetActive(true);
                EndlessFloorsPlugin.Instance.selectedFloor = 99;
                // reset everything save related
                EndlessFloorsPlugin._99Save = new EndlessSaveData();
                EndlessFloorsPlugin._99Save.startingFloor = EndlessFloorsPlugin.Instance.selectedFloor;
                EndlessFloorsPlugin.currentFloorData.FloorID = EndlessFloorsPlugin.Instance.selectedFloor;
                gl.LoadLevel(EndlessFloorsPlugin.currentSceneObject);
                evl.Initialize();
                Singleton<CoreGameManager>.Instance.AddPoints(9999 * 2, 0, false);
                EndlessFloorsPlugin.currentSave.Counters["slots"] = 5;
                evl.QueueShop();
                gl.SetSave(false);
            });
            mText.OnHighlight.AddListener(() =>
            {
                modeText.gameObject.GetComponent<TextLocalizer>().GetLocalizedText("Men_Floor99Desc");
            });

            StandardMenuButton freeB = theFree.GetComponent<StandardMenuButton>();
            freeB.OnPress = new UnityEvent();
            freeB.OnPress.AddListener(() =>
            {
                GameLoader gl = Resources.FindObjectsOfTypeAll<GameLoader>().First();
                gl.gameObject.SetActive(true);
                gl.CheckSeed();
                gl.Initialize(2);
                gl.SetMode((int)Mode.Free);
                // reset everything save related
                EndlessFloorsPlugin.freeSave = new EndlessSaveData();
                EndlessFloorsPlugin.freeSave.startingFloor = EndlessFloorsPlugin.Instance.selectedFloor;
                EndlessFloorsPlugin.currentFloorData.FloorID = EndlessFloorsPlugin.Instance.selectedFloor;
                Singleton<CoreGameManager>.Instance.AddPoints(999999, 0, false);
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
                ElevatorScreen evl = SceneManager.GetActiveScene().GetRootGameObjects().Where(x => x.name == "ElevatorScreen").First().GetComponent<ElevatorScreen>();
                gl.AssignElevatorScreen(evl);
                evl.gameObject.SetActive(true);
                gl.LoadLevel(EndlessFloorsPlugin.currentSceneObject);
                evl.Initialize();
                evl.QueueShop();
                gl.SetSave(false);
            });

            modeText.localPosition -= new Vector3(0f,48f,0f);
        }
        void Update()
        {
            if (!has_incre)
            {
                fc.Increment();
                f99Local.GetLocalizedText("But_Floor99");
                has_incre = true;
            }
        }
    }

    public class FloorChanger : MonoBehaviour
    {
        TextLocalizer textLocal;
        TMP_Text myText;
        public int floorNumber = 0;
        static FieldInfo textFo = AccessTools.Field(typeof(TextLocalizer), "textBox");
        void Awake()
        {
            textLocal = gameObject.GetComponent<TextLocalizer>();
            myText = (TMP_Text)textFo.GetValue(textLocal);
        }

        public void UpdateText()
        {
            textLocal.GetLocalizedText("Men_Floor");
            myText.text += floorNumber.ToString();
        }

        public void Increment()
        {
            floorNumber++;
            if (floorNumber > EndlessFloorsPlugin.Instance.highestFloorCount)
            {
                floorNumber = 1;
            }
            EndlessFloorsPlugin.Instance.selectedFloor = floorNumber;
            UpdateText();
        }
    }


    [HarmonyPatch(typeof(MainModeButtonController))]
    [HarmonyPatch("OnEnable")]
    public class UIPatches
    {
        public static void Finalizer(MainModeButtonController __instance) //first time ever using a finalizer
        {
            if (__instance.gameObject.GetComponent<EndlessTitleUI>() == null)
            {
                __instance.gameObject.AddComponent<EndlessTitleUI>();
            }
            __instance.mainNew.SetActive(true);
            __instance.mainContinue.SetActive(false);
        }
    }

    // todo: replace this with. i dont know, something that makes MORE SENSE??? like a transpiler instead of whatever the hell this is.
    [HarmonyPatch(typeof(EnvironmentController))]
    [HarmonyPatch("SpawnNPCs")]
    public class TemporarySwitchMode
    {
        public static bool DoingTempSwitch = false;
        public static void Finalizer()
        {
            if (Singleton<CoreGameManager>.Instance.currentMode == EndlessFloorsPlugin.NNFloorMode)
            {
                DoingTempSwitch = true;
                Singleton<CoreGameManager>.Instance.currentMode = Mode.Main;
            }
        }
    }

    [HarmonyPatch(typeof(EnvironmentController))]
    [HarmonyPatch("StartEventTimers")]
    public class TemporarySwitchMode2
    {
        public static void Finalizer()
        {
            if (TemporarySwitchMode.DoingTempSwitch)
            {
                Singleton<CoreGameManager>.Instance.currentMode = EndlessFloorsPlugin.NNFloorMode;
                TemporarySwitchMode.DoingTempSwitch = false;
            }
        }
    }
}
