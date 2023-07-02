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
    [HarmonyPatch(typeof(ITM_GrapplingHook))]
    [HarmonyPatch("OnCollisionEnter")]
    class GrappleBreakWindowsPatch
    {
        static bool Prefix(ITM_GrapplingHook __instance, Collision collision)
        {
            if (!EndlessFloorsPlugin.currentSave.HasUpgrade(typeof(GrappleBreakWindows))) return true;
            if (collision.transform.parent.gameObject.CompareTag("Window"))
            {
                collision.transform.parent.gameObject.GetComponent<Window>().Break(true);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ITM_BSODA))]
    [HarmonyPatch("Use")]
    class SlowBSODAPatch
    {
        static void Prefix(ITM_BSODA __instance, ref float ___speed)
        {
            ___speed *= 1f - (EndlessFloorsPlugin.currentSave.GetUpgradeCount(typeof(SlowBSODA)) * 0.15f);
        }
    }

    [HarmonyPatch(typeof(WaterFountain))]
    [HarmonyPatch("Clicked")]
    class FountainIncreasePatch
    {
        static void Postfix(WaterFountain __instance, int playerNumber)
        {
            float maxStam = Singleton<CoreGameManager>.Instance.GetPlayer(playerNumber).plm.staminaMax;
            Singleton<CoreGameManager>.Instance.GetPlayer(playerNumber).plm.stamina = maxStam + (EndlessFloorsPlugin.currentSave.GetUpgradeCount(typeof(DrinkEfficient)) * 10);
        }
    }

    [HarmonyPatch(typeof(ItemManager))]
    [HarmonyPatch("RemoveItem")]
    class RemoveItemPatch
    {
        static void Postfix(ItemManager __instance, int val)
        {
            if (EndlessFloorsPlugin.currentSave.HasUpgrade(typeof(Banking)))
            {
                if (Banking.Percentages[EndlessFloorsPlugin.currentSave.GetUpgradeCount(typeof(Banking))] >= UnityEngine.Random.Range(0f, 100f))
                {
                    __instance.SetItem(EndlessFloorsPlugin.ItemObjects.Find(x => x.itemType == Items.Quarter), val);
                }
            }
        }
    }

    [HarmonyPatch(typeof(ItemManager))]
    [HarmonyPatch("AddItem")]
    [HarmonyPatch(new Type[] { typeof(ItemObject), typeof(Pickup) })]
    class AddItemPatch
    {
        static void Prefix(ItemManager __instance, ref ItemObject item)
        {
            if (item.itemType == EndlessFloorsPlugin.presentEnum)
            {
                WeightedItemObject[] objects = EndlessFloorsPlugin.weightedItems.Where(x => x.selection.itemType != EndlessFloorsPlugin.presentEnum).ToArray();
                int weightAverage = objects.Sum(x => x.weight) / objects.Length;
                Dictionary<WeightedItemObject,int> ogWeights = new Dictionary<WeightedItemObject,int>();
                objects.Do((WeightedItemObject obj) => 
                {
                    ogWeights.Add(obj,obj.weight);
                    if (obj.weight > weightAverage)
                    {
                        obj.weight = Mathf.FloorToInt(obj.weight / EndlessFloorsPlugin.currentSave.luckValue);
                    }
                    else
                    {
                        obj.weight = Mathf.CeilToInt(obj.weight * EndlessFloorsPlugin.currentSave.luckValue);
                    }
                });
                item = WeightedItemObject.RandomSelection(objects);
                objects.Do((WeightedItemObject obj) =>
                {
                    obj.weight = ogWeights[obj];
                });
            }
        }
    }

}
