using AlmostEngine;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BaldiEndless
{
    [HarmonyPatch(typeof(CoreGameManager))]
    [HarmonyPatch("Start")]
    class SetGradeToF
    {
        static void Postfix(CoreGameManager __instance)
        {
            __instance.GradeVal = 16; //less bonus YTPs until you truly DESERVE them!!
            //Singleton<BaseGameManager>.Instance.CompleteMapOnReady();
        }


    }

    [HarmonyPatch(typeof(EnvironmentController))]
    [HarmonyPatch("SpawnNPC")]
    public class SpawnNPCPatch
    {
        public static void Postfix(EnvironmentController __instance, NPC npc)
        {
            float moveSpeed = 0f;
            int unclampedCount = EndlessFloorsPlugin.currentFloorData.npcCountUnclamped;
            int clampedCount = Mathf.Min(unclampedCount, EndlessFloorsPlugin.weightedNPCs.Count);
            bool isBaldi = npc.Character == Character.Baldi;
            if (Mathf.Abs(clampedCount - unclampedCount) >= 3)
            {
                moveSpeed = Mathf.Min(1f + (((Mathf.Abs(clampedCount - unclampedCount)) - 2f) * (isBaldi ? 0.010f : 0.020f)), isBaldi ? 1.4f : 1.6f);
            }
            if (moveSpeed != 0f)
            {
                Debug.Log(moveSpeed);
                MovementModifier mm = new MovementModifier(Vector3.zero, moveSpeed);
                __instance.Npcs.Last().GetComponent<ActivityModifier>().moveMods.Add(mm);
            }
        }
    }

    [HarmonyPatch(typeof(BaseGameManager))]
    [HarmonyPatch("Initialize")]
    class QuitOnBeyondMap
    {
        static void Postfix(BaseGameManager __instance)
        {
            //__instance.Ec.map.CompleteMap();
            //__instance.Ec.Players[0].plm.staminaMax += EndlessFloorsPlugin.currentSave.staminasBought * 100;
            //__instance.Ec.Players[0].plm.stamina = __instance.Ec.Players[0].plm.staminaMax;
            //__instance.Ec.Players[0].itm.AddItem(EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.GrapplingHook));
            if ((Singleton<CoreGameManager>.Instance.currentMode == EndlessFloorsPlugin.NNFloorMode) || (Singleton<CoreGameManager>.Instance.currentMode == Mode.Free))
            {
                if (EndlessFloorsPlugin.currentFloorData.FloorID != EndlessFloorsPlugin.Instance.selectedFloor)
                {
                    UnityEngine.Object.Destroy(Singleton<ElevatorScreen>.Instance.gameObject);
                    Singleton<CoreGameManager>.Instance.Quit();
                }
            }
        }
    }

    [HarmonyPatch(typeof(BaseGameManager))]
    [HarmonyPatch("BeginPlay")]
    class ResetTrip
    {
        static FieldInfo theFo = AccessTools.Field(typeof(TripEntrance), "entered");
        static void Postfix(BaseGameManager __instance, ref TripEntrance ___currentTripEntrance)
        {
            if (___currentTripEntrance == null) return;
            if (!Singleton<CoreGameManager>.Instance.tripPlayed)
            {
                ___currentTripEntrance.BaldiSprite.enabled = true;
                theFo.SetValue(___currentTripEntrance, false);
            }
        }
    }

    [HarmonyPatch(typeof(BaseGameManager))]
    [HarmonyPatch("BeginSpoopMode")]
    class OnSpoopMode
    {
        static void Postfix(BaseGameManager __instance)
        {
            if (EndlessFloorsPlugin.currentSave.HasUpgrade(typeof(AutoTag)))
            {
                Item theTag = GameObject.Instantiate(EndlessFloorsPlugin.Nametag.item);
                theTag.gameObject.SetActive(true);
                theTag.Use(__instance.Ec.Players[0]);
            }
            __instance.Ec.Players[0].plm.staminaMax += EndlessFloorsPlugin.currentSave.staminasBought * 25;
            // Singleton<CoreGameManager>.Instance.AddPoints(10000,0,true);
            //__instance.Ec.Players[0].plm.stamina = __instance.Ec.Players[0].plm.staminaMax;
        }
    }

    [HarmonyPatch(typeof(EnvironmentController))]
    [HarmonyPatch("SetElevators")]
    class OnElevator
    {
        static FieldInfo hasPlayer = AccessTools.Field(typeof(ColliderGroup), "hasPlayer");
        static void Postfix(EnvironmentController __instance, bool enable)
        {
            if (!enable) return;
            if (EndlessFloorsPlugin.currentSave.HasUpgrade(typeof(SkipExit)))
            {
                Elevator toClose = __instance.elevators[UnityEngine.Random.Range(0, __instance.elevators.Count)];
                hasPlayer.SetValue(toClose.ColliderGroup, true); //there is totally a player here yes absolutely DO NOT QUESTION ANYTHING THERE IS A PLAYER HERE
            }
        }
    }

    [HarmonyPatch(typeof(BaseGameManager))]
    [HarmonyPatch("CollectNotebooks")]
    class TweakBaldiAnger
    {
        static void Postfix(BaseGameManager __instance, int count, float ___notebookAngerVal)
        {
            int standardCount = (EndlessFloorsPlugin.currentFloorData.myFloorBaldi == 1) ? 4 : (EndlessFloorsPlugin.currentFloorData.myFloorBaldi == 2) ? 7 : 9;
            if (__instance.NotebookTotal > standardCount)
            {
                __instance.AngerBaldi(-(((float)count) * ___notebookAngerVal)); // undo the anger done by the game (makes the math easier)
                float angerAdditive = 0f;
                float stretchedTotal = ((float)standardCount + angerAdditive) / __instance.NotebookTotal;
                __instance.AngerBaldi((((float)count) * ___notebookAngerVal) * stretchedTotal);
            }
        }
    }

    public class AutoEndLevel : MonoBehaviour
    {
        public IEnumerator Wait()
        {
            yield return new WaitForSeconds(25f);
            Singleton<BaseGameManager>.Instance.LoadNextLevel();
            yield break;
        }
    }

    [HarmonyPatch(typeof(PlaceholderWinManager))]
    [HarmonyPatch("BeginPlay")]
    class MakeNormal
    {
        static bool Prefix(PlaceholderWinManager __instance, ref MovementModifier ___moveMod, ref BaldiDance ___dancingBaldi, ref Balloon[] ___balloonPre)
        {
            //___moveMod.movementMultiplier = 0.5f;
            AutoEndLevel ael = __instance.gameObject.AddComponent<AutoEndLevel>();
            ael.StartCoroutine("Wait");
            Time.timeScale = 1f;
            AudioListener.pause = false;
            Singleton<MusicManager>.Instance.PlayMidi("DanceV0_5", false);
            ___dancingBaldi.gameObject.SetActive(true);
            Singleton<CoreGameManager>.Instance.GetPlayer(0).Am.moveMods.Add(___moveMod);
            Singleton<CoreGameManager>.Instance.GetPlayer(0).itm.enabled = false;
            //base.StartCoroutine(this.FreakOut());
            for (int i = 0; i < __instance.balloonCount; i++)
            {
                UnityEngine.Object.Instantiate<Balloon>(___balloonPre[UnityEngine.Random.Range(0, ___balloonPre.Length)], __instance.Ec.transform).Initialize(__instance.Ec.rooms[0]);
            }
            return false;
        }
    }
}
