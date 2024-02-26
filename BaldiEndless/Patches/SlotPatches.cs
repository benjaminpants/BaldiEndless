using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace BaldiEndless
{
    [HarmonyPatch(typeof(ItemManager))]
    [HarmonyPatch("Awake")]
    class SlotAwakePatch
    {
        static void Postfix(ItemManager __instance, ref bool[] ___slotLocked)
        {
            __instance.maxItem = EndlessFloorsPlugin.currentSave.itemSlots - 1;
            /*for (int i = 0; i < ___slotLocked.Length; i++)
            {
                ___slotLocked[i] = i > __instance.maxItem;
            }*/
        }
    }

    [HarmonyPatch(typeof(ItemManager))]
    [HarmonyPatch("UpdateSelect")]
    class PreventSelectPatch
    {
        static bool Prefix(ItemManager __instance)
        {
            if (__instance.selectedItem > EndlessFloorsPlugin.currentSave.itemSlots - 1)
            {
                __instance.selectedItem = EndlessFloorsPlugin.currentSave.itemSlots - 1;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(HudManager))]
    [HarmonyPatch("SetItemSelect")]
    class UpdateSetItemGraphics
    {
        static void Postfix(HudManager __instance, ref Image[] ___itemSprites)
        {
            for (int i = EndlessFloorsPlugin.currentSave.itemSlots; i < 5; i++)
            {
                ___itemSprites[i].sprite = (Sprite)EndlessFloorsPlugin.Instance.assetManager[typeof(Sprite), "OutOfOrderSlot"];
            }
        }
    }
}
