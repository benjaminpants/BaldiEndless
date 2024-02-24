using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using BepInEx;

namespace BaldiEndless
{
    public class ITM_Present : Item
    {
        public static List<WeightedItemObject> potentialObjects = new List<WeightedItemObject>();

        public override bool Use(PlayerManager pm)
        {
            //Environment.Exit(0); //crash the game because fuck you
            return false;
            //throw new NotImplementedException();
        }
    }
}
