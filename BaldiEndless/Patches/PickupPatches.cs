using HarmonyLib;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BaldiEndless.Patches
{
    [HarmonyPatch(typeof(Pickup))]
    [HarmonyPatch("AssignItem")]
    class PickupPatch
    {
        internal static bool Prefix(Pickup __instance, ItemObject item, ref bool ___stillHasItem)
        {
            if (item.itemType != EndlessFloorsPlugin.upgradeEnum) return true;
            ___stillHasItem = true;
            __instance.item = item;
            StandardUpgrade upg;
            if (__instance.GetComponent<UpgradePickupMarker>())
            {
                upg = __instance.GetComponent<UpgradePickupMarker>().upgrade;
                if (!upg.ShouldAppear(EndlessFloorsPlugin.currentSave.GetUpgradeCount(upg.id)))
                {
                    GameObject.Destroy(__instance.GetComponent<UpgradePickupMarker>());
                    if (__instance.price != 0) // we are likely in a shop
                    {
                        __instance.AssignItem(ItemMetaStorage.Instance.FindByEnum(Items.None).value);
                        ___stillHasItem = false;
                        return false;
                    }
                    else
                    {
                        __instance.AssignItem(ItemMetaStorage.Instance.GetPointsObject(500, false)); // become the 500 points object(will currently placeholder to 100 points)
                    }
                }
            }
            else
            {
                if (__instance.price != 0)
                {
                    // shop logic goes here!
                    throw new NotImplementedException("shop logic no pre assign not implemented if it even should be idk man im just an error message.");
                }
                else
                {
                    if (EndlessFloorsPlugin.currentSave.claimedFreeUpgradeCurrentFloor) //no infinite upgrades in the halls!
                    {
                        __instance.AssignItem(ItemMetaStorage.Instance.FindByEnum(Items.None).value);
                        ___stillHasItem = false;
                        return false;
                    }
                    upg = UpgradeStoreFunction.GetRandomValidUpgrade((Mathf.RoundToInt(__instance.gameObject.transform.position.x * __instance.gameObject.transform.position.z) * 9163) + Mathf.RoundToInt(__instance.gameObject.transform.position.z * 1.224f));
                }
            }
            if (upg == null)
            {
                __instance.AssignItem(ItemMetaStorage.Instance.GetPointsObject(500, false));
                return false;
            }
            __instance.itemSprite.sprite = upg.GetIcon(EndlessFloorsPlugin.currentSave.GetUpgradeCount(upg.id));
            __instance.transform.name = "Item_Upg" + upg.id;
            __instance.gameObject.AddComponent<UpgradePickupMarker>().upgrade = upg;
            __instance.showDescription = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(Pickup))]
    [HarmonyPatch("Start")]
    class PickupStartHack
    {
        static void Postfix(Pickup __instance, ref bool ___stillHasItem)
        {
            PickupPatch.Prefix(__instance, __instance.item, ref ___stillHasItem);
        }
    }

    [HarmonyPatch(typeof(Pickup))]
    [HarmonyPatch("Collect")]
    class PickupCollectPatch
    {
        static bool Prefix(Pickup __instance, int player, ref bool ___stillHasItem)
        {
            if (__instance.GetComponent<UpgradePickupMarker>() == null) return true;
            StandardUpgrade upg = __instance.GetComponent<UpgradePickupMarker>().upgrade;
            GameObject.Destroy(__instance.GetComponent<UpgradePickupMarker>()); //bye bye marker!
            __instance.AssignItem(Singleton<CoreGameManager>.Instance.NoneItem);
            ___stillHasItem = false;
            __instance.gameObject.SetActive(false);
            if (__instance.icon != null)
            {
                __instance.icon.spriteRenderer.enabled = false;
            }
            if (!EndlessFloorsPlugin.currentSave.PurchaseUpgrade(upg, upg.behavior)) return false;
            upg.OnPurchase();
            EndlessFloorsPlugin.currentSave.claimedFreeUpgradeCurrentFloor = true;
            UpgradePickupMarker.UpdateAllUpgrades();
            return false;
        }
    }

    [HarmonyPatch(typeof(Pickup))]
    [HarmonyPatch("ClickableSighted")]
    class PickupSightedPatch
    {
        static bool Prefix(Pickup __instance, int player)
        {
            if (!__instance.showDescription) return true;
            if (__instance.GetComponent<UpgradePickupMarker>() == null) return true;
            StandardUpgrade upg = __instance.GetComponent<UpgradePickupMarker>().upgrade;
            Singleton<CoreGameManager>.Instance.GetHud(player).SetTooltip(upg.GetLoca(EndlessFloorsPlugin.currentSave.GetUpgradeCount(upg.id)));
            return false;
        }
    }

    public class UpgradePickupMarker : MonoBehaviour
    {
        public StandardUpgrade upgrade;

        public static void UpdateAllUpgrades()
        {
            UpgradePickupMarker[] markers = GameObject.FindObjectsOfType<UpgradePickupMarker>();
            for (int i = 0; i < markers.Length; i++)
            {
                Pickup pickup = markers[i].gameObject.GetComponent<Pickup>();
                pickup.AssignItem(pickup.item); //refresh
            }
            if (UpgradeStoreFunction.Instance)
            {
                UpgradeStoreFunction.Instance.UpdateAllTags();
            }
        }
    }
}
