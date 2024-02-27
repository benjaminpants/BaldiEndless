using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BaldiEndless.Patches
{
    [HarmonyPatch(typeof(ElevatorScreen))]
    [HarmonyPatch("Start")]
    class ElevatorScreenAddLives
    {
        static void Prefix(ElevatorScreen __instance, ref Sprite[] ___lifeImages)
        {
            if (___lifeImages.Length > 4) return;
            ___lifeImages = ___lifeImages.AddRangeToArray(new Sprite[] { (Sprite)EndlessFloorsPlugin.Instance.assetManager[typeof(Sprite), "LifeTubes4"], (Sprite)EndlessFloorsPlugin.Instance.assetManager[typeof(Sprite), "LifeTubes5"], (Sprite)EndlessFloorsPlugin.Instance.assetManager[typeof(Sprite), "LifeTubes6"] });
        }
    }
}
