using AlmostEngine;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Reflection;
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
            __instance.GradeVal = EndlessFloorsPlugin.currentSave.savedGrade; //less bonus YTPs until you truly DESERVE them!!
            //Singleton<BaseGameManager>.Instance.CompleteMapOnReady();
        }


    }

    [HarmonyPatch(typeof(CoreGameManager))]
    [HarmonyPatch("AddPoints")]
    class PointsMultiplier
    {
        static double[] _multipliers = { 1, 1.25, 1.5, 2 };
        static void Prefix(ref int points)
        {
            if (points < 0) return;
            points = (int)Math.Ceiling(points * _multipliers[EndlessFloorsPlugin.currentSave.GetUpgradeCount("ytpsmult")]);
        }


    }

    [HarmonyPatch(typeof(BaseGameManager))]
    [HarmonyPatch("BeginSpoopMode")]
    class OnSpoopMode
    {
        static void Postfix(BaseGameManager __instance)
        {
            if (EndlessFloorsPlugin.currentSave.HasUpgrade("autotag"))
            {
                Item theTag = GameObject.Instantiate(MTM101BaldiDevAPI.itemMetadata.FindByEnum(Items.Nametag).value.item);
                theTag.gameObject.SetActive(true);
                if (EndlessFloorsPlugin.currentSave.GetUpgradeCount("autotag") == 2)
                {
                    ITM_Nametag tag = theTag.GetComponent<ITM_Nametag>();
                    tag.ReflectionSetVariable("setTime", 60f);
                }
                theTag.Use(__instance.Ec.Players[0]);
            }
            __instance.Ec.Players[0].plm.staminaMax += EndlessFloorsPlugin.currentSave.GetUpgradeCount("stamina") * 25;
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
            if (EndlessFloorsPlugin.currentSave.HasUpgrade("freeexit"))
            {
                List<int> possibleElevators = new List<int>();
                for (int i = 0; i < __instance.elevators.Count; i++)
                {
                    possibleElevators.Add(i);
                }
                int elevatorsToClose = EndlessFloorsPlugin.currentSave.GetUpgradeCount("freeexit");
                while (elevatorsToClose > 0)
                {
                    int random = UnityEngine.Random.Range(0,possibleElevators.Count);
                    int selectedId = possibleElevators[random];
                    possibleElevators.RemoveAt(random);
                    Elevator toClose = __instance.elevators[selectedId];
                    hasPlayer.SetValue(toClose.ColliderGroup, true); //there is totally a player here yes absolutely DO NOT QUESTION ANYTHING THERE IS A PLAYER HERE
                    elevatorsToClose--;
                }
            }
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
            int clampedCount = Mathf.Min(unclampedCount, EndlessFloorsPlugin.lastGenMaxNpcs);
            bool isBaldi = npc.Character == Character.Baldi;
            if (Mathf.Abs(clampedCount - unclampedCount) >= 3)
            {
                moveSpeed = Mathf.Min(1f + (((Mathf.Abs(clampedCount - unclampedCount)) - 2f) * (isBaldi ? 0.010f : 0.020f)), isBaldi ? 1.07f : 1.6f);
            }
            if (moveSpeed != 0f)
            {
                MovementModifier mm = new MovementModifier(Vector3.zero, moveSpeed);
                __instance.Npcs.Last().GetComponent<ActivityModifier>().moveMods.Add(mm);
            }
        }
    }

    /*
    [HarmonyPatch(typeof(BaseGameManager))]
    [HarmonyPatch("Initialize")]
    class QuitOnBeyondMap
    {
        static void Postfix(BaseGameManager __instance)
        {
            __instance.Ec.map.CompleteMap();
            //__instance.Ec.Players[0].plm.staminaMax += EndlessFloorsPlugin.currentSave.staminasBought * 100;
            //__instance.Ec.Players[0].plm.stamina = __instance.Ec.Players[0].plm.staminaMax;
            //__instance.Ec.Players[0].itm.AddItem(EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.GrapplingHook));
        }
    }*/


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
