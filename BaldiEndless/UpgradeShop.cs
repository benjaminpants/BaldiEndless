using HarmonyLib;
using MTM101BaldAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BaldiEndless
{
    public class UpgradeShop : MonoBehaviour
    {

        public static UpgradeShop Instance;

        public List<string> Upgrades = new List<string>();

        public bool[] Purchased = new bool[6];

        public bool alwaysAddReroll = false;

        public void Awake()
        {
            Instance = this;
        }

        public StandardUpgrade GetUpgrade(int index)
        {
            return EndlessFloorsPlugin.Upgrades[Upgrades[index]];
        }

        public void PopulateShop()
        {
            try
            {
                int randRang = alwaysAddReroll ? 5 : UnityEngine.Random.Range(2, 6);
                List<WeightedSelection<StandardUpgrade>> upgradesTemp = new List<WeightedSelection<StandardUpgrade>>();
                EndlessFloorsPlugin.Upgrades.Do(x =>
                {
                    if (x.Key == "none") return;
                    if (x.Value.weight == 0) return;
                    if (x.Key == "reroll" && alwaysAddReroll) return;
                    if (!x.Value.ShouldAppear(EndlessFloorsPlugin.currentSave.GetUpgradeCount(x.Value.id))) return;
                    upgradesTemp.Add(new WeightedSelection<StandardUpgrade>()
                    {
                        selection = x.Value,
                        weight = x.Value.weight
                    });
                });
                WeightedSelection<StandardUpgrade>[] weightedUpgrades = upgradesTemp.ToArray();
                upgradesTemp = null;
                for (int i = 0; i < randRang; i++)
                {
                    if (weightedUpgrades.Length == 0)
                    {
                        Upgrades.Add("error");
                        continue;
                    }
                    StandardUpgrade gu = WeightedSelection<StandardUpgrade>.RandomSelection(weightedUpgrades);
                    Upgrades.Add(gu.id);
                    if (!gu.ShouldAppear(EndlessFloorsPlugin.currentSave.GetUpgradeCount(gu.id) + Upgrades.Where(x => x == gu.id).Count()))
                    {
                        weightedUpgrades = weightedUpgrades.Where(x => x.selection.id != gu.id).ToArray();
                    }
                }
                if (alwaysAddReroll)
                {
                    Upgrades.Add("reroll");
                }
            }
            catch(Exception ex)
            {
                MTM101BaldiDevAPI.CauseCrash(EndlessFloorsPlugin.Instance.Info, ex);
            }
        }
    }

    public static class StorePatchHelpers
    {
        public static bool AllowDebt = false; //FOR DEBUG USE ONLY
        public static void UpdateUpgradeBar(ref Image[] ___inventoryImage, ref ItemObject ___defaultItem)
        {
            for (int i = 0; i < 5; i++)
            {
                UpgradeSaveData saveData = EndlessFloorsPlugin.currentSave.Upgrades[i];
                if (saveData.id == "none")
                {
                    ___inventoryImage[i].sprite = ___defaultItem.itemSpriteSmall;
                }
                else
                {
                    ___inventoryImage[i].sprite = EndlessFloorsPlugin.Upgrades[saveData.id].GetIcon(saveData.count - 1);
                }
            }
        }

        public static void UpdateShopItems(UpgradeShop shop, ref Image[] ___forSaleImage, ref TMP_Text[] ___itemPrice, ref ItemObject ___defaultItem)
        {
            for (int i = 0; i < ___forSaleImage.Length; i++)
            {
                ___itemPrice[i].color = Color.black;
                if (i < shop.Upgrades.Count)
                {
                    StandardUpgrade upg = shop.GetUpgrade(i);
                    int level = EndlessFloorsPlugin.currentSave.GetUpgradeCount(shop.Upgrades[i]);
                    ___forSaleImage[i].sprite = upg.GetIcon(level);
                    ___itemPrice[i].text = upg.GetCost(level).ToString();
                    if (shop.Purchased[i])
                    {
                        ___itemPrice[i].text = "SOLD";
                        ___itemPrice[i].color = Color.red;
                        ___forSaleImage[i].sprite = ___defaultItem.itemSpriteSmall;
                    }
                }
                else
                {
                    ___itemPrice[i].text = "";
                    ___forSaleImage[i].sprite = ___defaultItem.itemSpriteSmall;
                }
            }
        }
    }


    [HarmonyPatch(typeof(StoreScreen))]
    [HarmonyPatch("Start")]
    public class StoreOverhaul
    {
        static bool Prefix(StoreScreen __instance, ref Image[] ___inventoryImage, ref ItemObject ___defaultItem, ref int ___mapPrice, ref Image[] ___forSaleImage, ref TMP_Text[] ___itemPrice, ref TMP_Text ___mapPriceText, ref int ___ytps, ref TMP_Text ___totalPoints, ref AudioManager ___audMan, ref SoundObject ___audJonIntro, ref SoundObject[] ___audIntroP2)
        {
            UpgradeShop shop = __instance.gameObject.AddComponent<UpgradeShop>(); //add the upgrade shop
            shop.alwaysAddReroll = ((Singleton<CoreGameManager>.Instance.currentMode == EndlessFloorsPlugin.NNFloorMode) || ((EndlessFloorsPlugin.currentFloorData.FloorID == EndlessFloorsPlugin.Instance.selectedFloor) && EndlessFloorsPlugin.Instance.selectedFloor != 1) && !EndlessFloorsPlugin.currentSave.claimedFreePoints);
            if (shop.alwaysAddReroll)
            {
                EndlessFloorsPlugin.currentSave.claimedFreePoints = true;
            }
            shop.PopulateShop(); //populate the shop

            //standard map code here
            if (Singleton<CoreGameManager>.Instance != null)
            {
                ___mapPrice = Singleton<BaseGameManager>.Instance.levelObject.mapPrice;
                ___mapPriceText.text = ___mapPrice.ToString();
                ___ytps = Singleton<CoreGameManager>.Instance.GetPoints(0);
                ___totalPoints.text = ___ytps.ToString();
            }
            else
            {
                ___mapPrice = 300;
                ___mapPriceText.text = ___mapPrice.ToString();
                ___ytps = 500;
                ___totalPoints.text = ___ytps.ToString();
            }

            ___audMan.QueueAudio(___audJonIntro);
            ___audMan.QueueRandomAudio(___audIntroP2);
            __instance.StandardDescription();
            ___audMan.audioDevice.ignoreListenerPause = true;

            StorePatchHelpers.UpdateShopItems(shop, ref ___forSaleImage, ref ___itemPrice, ref ___defaultItem);

            GameObject obj = ___inventoryImage[0].transform.parent.parent.Find("ItemsCover").gameObject;
            obj.GetComponent<RawImage>().texture = EndlessFloorsPlugin.upgradeTex5;

            StorePatchHelpers.UpdateUpgradeBar(ref ___inventoryImage, ref ___defaultItem);
            return false;
        }
    }

    [HarmonyPatch(typeof(StoreScreen))]
    [HarmonyPatch("UpdateDescription")]
    public class DescriptionUpdates
    {
        static bool Prefix(StoreScreen __instance, int val, ref bool ___dragging, ref TMP_Text ___itemDescription, ref AudioManager ___audMan, ref SoundObject ___audMapInfo)
        {
            if (!___dragging)
            {
                if (val <= 5)
                {
                    UpgradeShop shop = __instance.GetComponent<UpgradeShop>();
                    if (val >= shop.Upgrades.Count || shop.Purchased[val])
                    {
                        __instance.StandardDescription();
                    }
                    else
                    {
                        ___itemDescription.text = Singleton<LocalizationManager>.Instance.GetLocalizedText(shop.GetUpgrade(val).GetLoca(EndlessFloorsPlugin.currentSave.GetUpgradeCount(shop.Upgrades[val])));
                    }
                    return false;
                }
                if (val == 6)
                {
                    ___itemDescription.text = Singleton<LocalizationManager>.Instance.GetLocalizedText("Desc_MapFill");
                    if (!___audMan.QueuedAudioIsPlaying)
                    {
                        ___audMan.QueueAudio(___audMapInfo);
                        return false;
                    }
                }
                else if (val == 7)
                {
                    ___itemDescription.text = Singleton<LocalizationManager>.Instance.GetLocalizedText("Desc_Suspend");
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(StoreScreen))]
    [HarmonyPatch("InventoryDescription")]
    public class InventoryDescriptionUpdates
    {
        static bool Prefix(StoreScreen __instance, int val, ref bool ___dragging, ref TMP_Text ___itemDescription, ref AudioManager ___audMan, ref SoundObject ___audMapInfo)
        {
            UpgradeSaveData data = EndlessFloorsPlugin.currentSave.Upgrades[val];
            StandardUpgrade upg = EndlessFloorsPlugin.Upgrades[data.id];
            if (data.count == 0)
            {
                ___itemDescription.text = Singleton<LocalizationManager>.Instance.GetLocalizedText("Upg_None");
            }
            else
            {
                string baseDesc = Singleton<LocalizationManager>.Instance.GetLocalizedText(upg.GetLoca(data.count - 1));
                string removeDesc = Singleton<LocalizationManager>.Instance.GetLocalizedText("Upg_Remove");
                ___itemDescription.text = baseDesc + removeDesc;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(StoreScreen))]
    [HarmonyPatch("BuyItem")]
    public class BuyItemReplace
    {
        static bool Prefix(StoreScreen __instance, int val, ref Image[] ___inventoryImage, ref AudioManager ___audMan, ref bool ___purchaseMade, ref SoundObject[] ___audUnafforable, ref SoundObject[] ___audBuy, ref int ___ytps, ref int ___pointsSpent, ref Image[] ___forSaleImage, ref TMP_Text[] ___itemPrice, ref ItemObject ___defaultItem, ref TMP_Text ___totalPoints)
        {
            if (val == -1)
            {
                UpgradeShop shop = __instance.GetComponent<UpgradeShop>();
                StorePatchHelpers.UpdateShopItems(shop, ref ___forSaleImage, ref ___itemPrice, ref ___defaultItem);
                return false;
            }
            if (val <= 5)
            {
                UpgradeShop shop = __instance.GetComponent<UpgradeShop>();
                if (shop.Purchased[val]) return false;
                if (val <= shop.Upgrades.Count)
                {
                    StandardUpgrade curUpgrade = shop.GetUpgrade(val);
                    int level = EndlessFloorsPlugin.currentSave.GetUpgradeCount(curUpgrade.id);
                    if (___ytps >= curUpgrade.GetCost(level) || StorePatchHelpers.AllowDebt)
                    {
                        if (!EndlessFloorsPlugin.currentSave.PurchaseUpgrade(curUpgrade, curUpgrade.behavior)) return false;
                        // since level is gotten before the purchase, it should still be correct.
                        ___ytps -= curUpgrade.GetCost(level) * (StorePatchHelpers.AllowDebt ? -1 : 1); //do this before anything else, so it can't possibly change
                        ___pointsSpent += curUpgrade.GetCost(level) * (StorePatchHelpers.AllowDebt ? -1 : 1);
                        ___totalPoints.text = ___ytps.ToString();
                        ___purchaseMade = true;
                        if (!___audMan.QueuedUp)
                        {
                            ___audMan.QueueRandomAudio(___audBuy);
                        }
                        __instance.StandardDescription();
                        shop.Purchased[val] = true;
                        StorePatchHelpers.UpdateShopItems(shop, ref ___forSaleImage, ref ___itemPrice, ref ___defaultItem);
                        StorePatchHelpers.UpdateUpgradeBar(ref ___inventoryImage, ref ___defaultItem);
                        curUpgrade.OnPurchase();
                        return false;
                    }
                }
                if (!___audMan.QueuedUp)
                {
                    ___audMan.QueueRandomAudio(___audUnafforable);
                    return false;
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(StoreScreen))]
    [HarmonyPatch("ClickInventory")]
    public class ClickInventoryReplace
    {
        static bool Prefix(StoreScreen __instance, int val, ref AudioManager ___audMan, ref int ___pointsSpent, ref int ___ytps, ref Image[] ___inventoryImage, ref ItemObject ___defaultItem, ref TMP_Text ___totalPoints)
        {
            UpgradeSaveData data = EndlessFloorsPlugin.currentSave.Upgrades[val];
            if (data.id == "none")
            {
                return false;
            }
            else
            {
                StandardUpgrade myG = EndlessFloorsPlugin.Upgrades[data.id];
                int level = EndlessFloorsPlugin.currentSave.GetUpgradeCount(myG.id);
                int sellPrice = myG.CalculateSellPrice(level);
                ___pointsSpent -= sellPrice;
                ___ytps += sellPrice;
                EndlessFloorsPlugin.currentSave.SellUpgrade(data.id);
                ___totalPoints.text = ___ytps.ToString();
                StorePatchHelpers.UpdateUpgradeBar(ref ___inventoryImage, ref ___defaultItem);
                UpgradeShop.Instance.GetComponent<StoreScreen>().InventoryDescription(val);
                UpgradeShop.Instance.GetComponent<StoreScreen>().BuyItem(-1);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(StoreScreen))]
    [HarmonyPatch("TryExit")]
    public class TryExitAlways
    {
        static bool Prefix(StoreScreen __instance)
        {
            __instance.Exit();
            return false;
        }
    }

    [HarmonyPatch(typeof(StoreScreen))]
    [HarmonyPatch("Exit")]
    public class ExitAlways
    {
        static bool Prefix(StoreScreen __instance, ref bool ___purchaseMade, ref AudioManager ___audMan, ref int ___pointsSpent, ref SoundObject[] ___audLeaveHappy, ref SoundObject[] ___audLeaveSad)
        {
            if (___purchaseMade)
            {
                ___audMan.QueueRandomAudio(___audLeaveHappy);
            }
            else
            {
                ___audMan.QueueRandomAudio(___audLeaveSad);
            }
            if (Singleton<CoreGameManager>.Instance.GetPlayer(0) != null)
            {
                Singleton<CoreGameManager>.Instance.BackupPlayers();
                Singleton<CoreGameManager>.Instance.AddPoints(___pointsSpent * -1, 0, false);
            }
            UnityEngine.Object.Destroy(CursorController.Instance.gameObject);
            __instance.StartCoroutine("WaitForAudio");
            return false;
        }
    }
}
