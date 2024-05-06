using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using BepInEx;
using System.Linq;

namespace BaldiEndless
{
    public class ITM_Present : Item
    {
        public static List<WeightedItemObject> potentialObjects = new List<WeightedItemObject>();
        static float[] LuckValues => new float[] { 1f, 1.45f, 2f, 2.37f, 3f, 4f };

        public override bool Use(PlayerManager pm)
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
            pm.itm.AddItem(WeightedItemObject.RandomSelection(objects));
            objects.Do((WeightedItemObject obj) =>
            {
                obj.weight = ogWeights[obj];
            });
            return true;
        }
    }
}
