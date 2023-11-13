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
    [HarmonyPatch("BB_MOD.FeatureData, Baldi's Basics Times", "SetVariables")]
    class SetFeatureVariables
    {
        static void Postfix()
        {
            Type typ = Chainloader.PluginInfos[EndlessFloorsPlugin.BBTimesID].Instance.GetType().Assembly.GetType("BB_MOD.FeatureData");
            AccessTools.Field(typ, "GrapplingHookBreaksWindow").SetValue(null, EndlessFloorsPlugin.currentSave.HasUpgrade(typeof(GrappleBreakWindows)));

            FieldInfo AmountOfExtraNumBalls = AccessTools.Field(typ, "AmountOfExtraNumBalls");
            FieldInfo MaxConveyorSpeedOffset = AccessTools.Field(typ, "MaxConveyorSpeedOffset");
            FieldInfo MaxNewProblems = AccessTools.Field(typ, "MaxNewProblems");
            AmountOfExtraNumBalls.SetValue(null, Math.Min(Mathf.FloorToInt((EndlessFloorsPlugin.currentFloorData.unclampedScaleVar / 140f) + 0.1f), 9));
            MaxConveyorSpeedOffset.SetValue(null, Mathf.FloorToInt(Mathf.Abs(Mathf.Sin(EndlessFloorsPlugin.currentFloorData.FloorID / 10f)) * 3f));

            List<WeightedSelection<int>> weightedProblemCount = new List<WeightedSelection<int>>();
            int amountOfProblems = Mathf.Max(Mathf.RoundToInt(Mathf.Min((EndlessFloorsPlugin.currentFloorData.unclampedScaleVar / ((EndlessFloorsPlugin.currentFloorData.FloorID * 5f) + 80f)), 2)), 1);
            int weight = 100;
            for (int i = 0; i < amountOfProblems; i++)
            {
                weightedProblemCount.Add(new WeightedSelection<int>
                {
                    selection = i + 1,
                    weight = weight
                });
                weight = Mathf.RoundToInt(weight / 1.65f);
            }

            MaxNewProblems.SetValue(null, weightedProblemCount.ToArray());
        }
    }

    //tell times that everything is available always
    [ConditionalPatchMod(EndlessFloorsPlugin.BBTimesID)]
    [HarmonyPatch("BB_MOD.Builders.TrapDoorBuilder, Baldi's Basics Times", "Build")]
    class TimesTrapPatch
    {
        private static void Prefix(ObjectBuilder __instance, ref System.Random cRng)
        {
            Type trapDoorType = Chainloader.PluginInfos[EndlessFloorsPlugin.BBTimesID].Instance.GetType().Assembly.GetType("BB_MOD.Builders.TrapDoorBuilder");
            int maxAllowed = Mathf.Max(Mathf.CeilToInt(EndlessFloorsPlugin.currentFloorData.scaleVar / 45f), 1);
            int maxAllowedLinked = Mathf.Min(maxAllowed - cRng.Next(1, Mathf.Max(maxAllowed / 4, 2)), maxAllowed);
            int maxAllowedRandom = Mathf.Max(maxAllowed - maxAllowedLinked, cRng.Next(1, Mathf.Max(maxAllowed / 8, 2)));

            trapDoorType.GetField("minAmount", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, 1);
            trapDoorType.GetField("maxAmount", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, maxAllowed);
            trapDoorType.GetField("amountOfLinkedTrapDoors", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, maxAllowedLinked);
            trapDoorType.GetField("amountOfRngTrapDoors", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, maxAllowedRandom);

            //MethodInfo configFunc = trapDoorType.GetMethod("SetMyConfigurations", BindingFlags.Public | BindingFlags.Instance);
            //Debug.Log(configFunc);
            //configFunc.Invoke(__instance, new object[] { 1, maxAllowed, maxAllowedRandom, maxAllowedLinked});
        }

        private static void Finalizer(Exception __exception)
        {
            Debug.Log(__exception);
        }
    }

    [ConditionalPatchMod(EndlessFloorsPlugin.BBTimesID)]
    [HarmonyPatch("BB_MOD.ContentManager, Baldi's Basics Times", "RoomCount")]
    class TimesRoomCountPatch
    {
        private static void Prefix(object __instance, ref int __result, ref Dictionary<RoomCategory, int[]> ___basicRoomPairs)
        {
            ___basicRoomPairs.Clear();
            ___basicRoomPairs.Add(EnumExtensions.GetFromExtendedName<RoomCategory>("bathroom"),
                new int[] {
                    Mathf.FloorToInt(EndlessFloorsPlugin.currentFloorData.classRoomCount / 30),
                    Mathf.RoundToInt(EndlessFloorsPlugin.currentFloorData.classRoomCount / 10)
                });
            ___basicRoomPairs.Add(EnumExtensions.GetFromExtendedName<RoomCategory>("abandoned"),
                new int[] {
                    Math.Min(Mathf.FloorToInt(EndlessFloorsPlugin.currentFloorData.exitCount / 3),3),
                    Math.Min(Mathf.FloorToInt(EndlessFloorsPlugin.currentFloorData.exitCount / 4),5)
                });
            ___basicRoomPairs.Add(EnumExtensions.GetFromExtendedName<RoomCategory>("computerRoom"),
                new int[] {
                    Math.Min(Mathf.FloorToInt(EndlessFloorsPlugin.currentFloorData.classRoomCount / 5),1),
                    Math.Min(Mathf.FloorToInt(EndlessFloorsPlugin.currentFloorData.classRoomCount / 5),1),
                });
        }

        private static void Finalizer(Exception __exception)
        {
            Debug.Log(__exception);
        }
    }

    //tell times that we are always on floor 1
    [ConditionalPatchMod(EndlessFloorsPlugin.BBTimesID)]
    [HarmonyPatch("BB_MOD.GenericExtensions, Baldi's Basics Times", "ToFloorIdentifier")]
    class TimesFloor3Patch
    {
        private static void Prefix(ref string name)
        {
            name = "F1";
        }
    }
}
