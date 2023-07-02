using HarmonyLib;
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

        public List<GameUpgrade> Upgrades = new List<GameUpgrade>();

        public static WeightedSelection<GameUpgrade>[] WeightedUpgrades = new WeightedSelection<GameUpgrade>[]
        {
            new WeightedSelection<GameUpgrade>()
            {
                selection = new AutoTag(),
                weight = 120
            },
            new WeightedSelection<GameUpgrade>()
            {
                selection = new ExtraLife(),
                weight = 100
            },
            new WeightedSelection<GameUpgrade>()
            {
                selection = new Banking(),
                weight = 32
            },
            new WeightedSelection<GameUpgrade>()
            {
                selection = new PompTimeIncrease(),
                weight = 70
            },
            new WeightedSelection<GameUpgrade>()
            {
                selection = new HungryBully(),
                weight = 78
            },
            new WeightedSelection<GameUpgrade>()
            {
                selection = new GrappleBreakWindows(),
                weight = 82
            },
            new WeightedSelection<GameUpgrade>()
            {
                selection = new HyperWhistle(),
                weight = 80
            },
            new WeightedSelection<GameUpgrade>()
            {
                selection = new StaminaIncrease(),
                weight = 90
            },
            new WeightedSelection<GameUpgrade>()
            {
                selection = new FieldtripRedo(),
                weight = 85
            },
            new WeightedSelection<GameUpgrade>()
            {
                selection = new DrinkEfficient(),
                weight = 90
            },
            new WeightedSelection<GameUpgrade>()
            {
                selection = new SlowBSODA(),
                weight = 70
            },
            new WeightedSelection<GameUpgrade>()
            {
                selection = new Reroll(),
                weight = 30
            },
            new WeightedSelection<GameUpgrade>()
            {
                selection = new SkipExit(),
                weight = 45
            },
            new WeightedSelection<GameUpgrade>()
            {
                selection = new PresentLuck(),
                weight = 35
            }

        };

        public bool[] Purchased = new bool[6];

        public bool alwaysAddReroll = false;

        public void Awake()
        {
            Instance = this;
        }


        public void PopulateShop()
        {
            int randRang = alwaysAddReroll ? 5 : UnityEngine.Random.Range(2,6);
            WeightedSelection<GameUpgrade>[] thegrades = WeightedUpgrades.Where(x => x.selection.ShouldAppear()).ToArray();
            for (int i = 0; i < randRang; i++)
            {
                GameUpgrade gu = WeightedSelection<GameUpgrade>.RandomSelection(thegrades);
                Upgrades.Add(gu);
                if (Upgrades.FindAll(x => x.GetType() == gu.GetType()).Count >= gu.MaxStack)
                {
                    thegrades = thegrades.Where(x => x.selection != gu).ToArray(); //remove the entry that correlates to the thing
                }
            }
            if (alwaysAddReroll)
            {
                Upgrades.Add(new Reroll());
            }
        }
    }

    public static class StorePatchHelpers
    {
        public static bool AllowDebt = false; //FOR DEBUG USE ONLY
        public static void UpdateUpgradeBar(ref Image[] ___inventoryImage, ref ItemObject ___defaultItem)
        {
            for (int i = 0; i < ___inventoryImage.Length; i++)
            {
                if (i >= EndlessFloorsPlugin.currentSave.purchasedUpgrades.Count)
                {
                    ___inventoryImage[i].sprite = ___defaultItem.itemSpriteSmall;
                }
                else
                {
                    GameUpgrade gu = EndlessFloorsPlugin.currentSave.purchasedUpgradesClasses[i];
                    ___inventoryImage[i].sprite = gu.GetSprite(EndlessFloorsPlugin.currentSave.GetUpgradeCount(gu.GetType()) - 1);
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
                    ___forSaleImage[i].sprite = shop.Upgrades[i].GetSprite(EndlessFloorsPlugin.currentSave.GetUpgradeCount(shop.Upgrades[i].GetType()));
                    ___itemPrice[i].text = shop.Upgrades[i].Cost.ToString();
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
            shop.alwaysAddReroll = (Singleton<CoreGameManager>.Instance.currentMode == EndlessFloorsPlugin.NNFloorMode) || ((EndlessFloorsPlugin.currentFloorData.FloorID == EndlessFloorsPlugin.Instance.selectedFloor) && EndlessFloorsPlugin.Instance.selectedFloor != 1);
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

            StorePatchHelpers.UpdateUpgradeBar(ref ___inventoryImage,ref ___defaultItem);
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
                        ___itemDescription.text = Singleton<LocalizationManager>.Instance.GetLocalizedText(shop.Upgrades[val].Description);
                    }
                    return false;
                }
                if (val == 6)
                {
                    ___itemDescription.text = Singleton<LocalizationManager>.Instance.GetLocalizedText("Desc_MapFill");
                    if (!___audMan.IsPlaying)
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
            if (val >= EndlessFloorsPlugin.currentSave.purchasedUpgrades.Count)
            {
                ___itemDescription.text = Singleton<LocalizationManager>.Instance.GetLocalizedText("Upg_None");
            }
            else
            {
                string baseDesc = Singleton<LocalizationManager>.Instance.GetLocalizedText(EndlessFloorsPlugin.currentSave.purchasedUpgradesClasses[val].Description);
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
                    GameUpgrade curUpgrade = shop.Upgrades[val];
                    if (___ytps >= curUpgrade.Cost || StorePatchHelpers.AllowDebt)
                    {
                        if (!curUpgrade.CanPurchase()) return false;
                        ___ytps -= curUpgrade.Cost * (StorePatchHelpers.AllowDebt ? -1 : 1); //do this before anything else, so it can't possibly change
                        ___pointsSpent += curUpgrade.Cost * (StorePatchHelpers.AllowDebt ? -1 : 1);
                        ___totalPoints.text = ___ytps.ToString();
                        ___purchaseMade = true;
                        if (!___audMan.QueuedUp)
                        {
                            ___audMan.QueueRandomAudio(___audBuy);
                        }
                        __instance.StandardDescription();
                        shop.Purchased[val] = true;
                        curUpgrade.OnPurchase();
                        StorePatchHelpers.UpdateShopItems(shop, ref ___forSaleImage, ref ___itemPrice, ref ___defaultItem);
                        StorePatchHelpers.UpdateUpgradeBar(ref ___inventoryImage, ref ___defaultItem);
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
            if (val >= EndlessFloorsPlugin.currentSave.purchasedUpgrades.Count)
            {
                return false;
            }
            else
            {
                GameUpgrade myG = EndlessFloorsPlugin.currentSave.purchasedUpgradesClasses[val];
                myG.OnSell();
                ___pointsSpent -= myG.SellPrice;
                ___ytps += myG.SellPrice;
                EndlessFloorsPlugin.currentSave.RemoveUpgrade(val);
                ___totalPoints.text = ___ytps.ToString();
                StorePatchHelpers.UpdateUpgradeBar(ref ___inventoryImage, ref ___defaultItem);
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
