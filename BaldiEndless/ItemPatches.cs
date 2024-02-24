using HarmonyLib;
using MTM101BaldAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BaldiEndless
{
    [HarmonyPatch(typeof(ItemManager))]
    [HarmonyPatch("RemoveItem")]
    class RemoveItemPatch
    {
        static float[] Percentages => new float[] { 0f, 1f, 2f, 3f, 5f, 6f, 10f };
        static void Postfix(ItemManager __instance, int val)
        {
            if (EndlessFloorsPlugin.currentSave.HasUpgrade("bank"))
            {
                if (Percentages[EndlessFloorsPlugin.currentSave.GetUpgradeCount("bank")] >= UnityEngine.Random.Range(0f, 100f))
                {
                    __instance.SetItem(MTM101BaldiDevAPI.itemMetadata.FindByEnum(Items.Quarter).value, val);
                }
            }
        }
    }

    [HarmonyPatch(typeof(WaterFountain))]
    [HarmonyPatch("Clicked")]
    class FountainIncreasePatch
    {
        static void Postfix(WaterFountain __instance, int playerNumber)
        {
            float maxStam = Singleton<CoreGameManager>.Instance.GetPlayer(playerNumber).plm.staminaMax;
            if (Singleton<CoreGameManager>.Instance.GetPlayer(playerNumber).plm.stamina != maxStam) return;
            Singleton<CoreGameManager>.Instance.GetPlayer(playerNumber).plm.stamina = maxStam + (EndlessFloorsPlugin.currentSave.GetUpgradeCount("drink") * 10);
        }
    }

    [HarmonyPatch(typeof(ITM_BSODA))]
    [HarmonyPatch("Use")]
    class SlowBSODAPatch
    {
        static void Prefix(ITM_BSODA __instance, ref float ___speed)
        {
            ___speed *= 1f - (EndlessFloorsPlugin.currentSave.GetUpgradeCount("slowsoda") * 0.15f);
        }
    }

    [HarmonyPatch(typeof(ItemManager))]
    [HarmonyPatch("AddItem")]
    [HarmonyPatch(new Type[] { typeof(ItemObject), typeof(Pickup) })]
    class AddItemPatch
    {
        static float[] LuckValues => new float[] { 1f, 1.45f, 2f, 2.37f, 3f, 4f };

        static void Prefix(ItemManager __instance, ref ItemObject item)
        {
            if (item.itemType == EndlessFloorsPlugin.presentEnum)
            {
                WeightedItemObject[] objects = ITM_Present.potentialObjects.ToArray();
                int weightAverage = objects.Sum(x => x.weight) / objects.Length;
                Dictionary<WeightedItemObject, int> ogWeights = new Dictionary<WeightedItemObject, int>();
                objects.Do((WeightedItemObject obj) =>
                {
                    ogWeights.Add(obj, obj.weight);
                    if (obj.weight > weightAverage)
                    {
                        obj.weight = Mathf.FloorToInt(obj.weight / LuckValues[EndlessFloorsPlugin.currentSave.GetUpgradeCount("luck")]);
                    }
                    else
                    {
                        obj.weight = Mathf.CeilToInt(obj.weight * LuckValues[EndlessFloorsPlugin.currentSave.GetUpgradeCount("luck")]);
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
