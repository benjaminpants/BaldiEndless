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
            /*float moveSpeed = 0f;
            int unclampedCount = EndlessFloorsPlugin.currentFloorData.npcCountUnclamped;
            int clampedCount = Mathf.Min(unclampedCount, EndlessFloorsPlugin.weightedNPCs.Count);
            bool isBaldi = npc.Character == Character.Baldi;
            if (Mathf.Abs(clampedCount - unclampedCount) >= 3)
            {
                moveSpeed = Mathf.Min(1f + (((Mathf.Abs(clampedCount - unclampedCount)) - 2f) * (isBaldi ? 0.010f : 0.020f)), isBaldi ? 1.1f : 1.6f);
            }
            if (moveSpeed != 0f)
            {
                MovementModifier mm = new MovementModifier(Vector3.zero, moveSpeed);
                __instance.Npcs.Last().GetComponent<ActivityModifier>().moveMods.Add(mm);
            }*/
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
                /*if (EndlessFloorsPlugin.currentFloorData.FloorID != EndlessFloorsPlugin.Instance.selectedFloor)
                {
                    UnityEngine.Object.Destroy(Singleton<ElevatorScreen>.Instance.gameObject);
                    Singleton<CoreGameManager>.Instance.Quit();
                }*/
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
                float stretchedTotal = ((float)standardCount + angerAdditive) / (float)__instance.NotebookTotal;
                __instance.AngerBaldi((((float)count) * ___notebookAngerVal) * stretchedTotal);
            }
        }
    }

}
