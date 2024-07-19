using BaldiEndless.Patches;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.Reflection;
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
    public class UpgradeStoreFunction : RoomFunction
    {
        public bool alwaysAddReroll = false;
        public static UpgradeStoreFunction Instance;
        private List<Pickup> pickups = new List<Pickup>();
        public List<StandardUpgrade> upgrades = new List<StandardUpgrade>();
        public PriceTag mapTag;
        public Pickup mapPickup;
        public List<PriceTag> priceTags = new List<PriceTag>();
        private float saleChance = 0.05f;
        private float minSaleDiscount = 0.5f;
        private float maxSaleDiscount = 0.9f;
        private int shopItems = 6;
        private Dictionary<int, float> discounts = new Dictionary<int, float>();
        private bool[] purchasedItems = new bool[0];

        private bool hasGreeted = false;

        public Transform roomBase;
        public Transform johnnyBase;


        private void ItemCollected(Pickup pickup, int player)
        {
            //this.MarkItemAsSold(pickup);
            purchasedItems[pickups.IndexOf(pickup)] = true;
            pickup.price = 0;
            pickup.showDescription = false;
            if (pickup == mapPickup)
            {
                //this.BuyMap();
            }
            UpdateAllTags();
        }

        private void ItemPurchased(Pickup pickup, int player)
        {
            if (audMan.QueuedUp) return;
            audMan.QueueRandomAudio(succesfulPurchase);
        }

        private void ItemDenied(Pickup pickup, int player)
        {
            if (audMan.QueuedUp) return;
            audMan.QueueRandomAudio(failedPurchase);
        }

        PropagatedAudioManager audMan;
        CustomSpriteAnimator animator;
        CustomVolumeAnimator volumeAnimator;

        SoundObject initialGreeting;
        SoundObject greeting;
        SoundObject[] succesfulPurchase = new SoundObject[3];
        SoundObject[] failedPurchase = new SoundObject[3];

        public override void Initialize(RoomController room)
        {
            base.Initialize(room);

            initialGreeting = EndlessFloorsPlugin.Instance.assetManager.Get<SoundObject>("AudDanielFirstEnc");
            greeting = EndlessFloorsPlugin.Instance.assetManager.Get<SoundObject>("AudDanielGreeting");
            succesfulPurchase = new SoundObject[3] { EndlessFloorsPlugin.Instance.assetManager.Get<SoundObject>("AudDanielBuy1"), EndlessFloorsPlugin.Instance.assetManager.Get<SoundObject>("AudDanielBuy2"), EndlessFloorsPlugin.Instance.assetManager.Get<SoundObject>("AudDanielBuy3") };
            failedPurchase = new SoundObject[2] { EndlessFloorsPlugin.Instance.assetManager.Get<SoundObject>("AudDanielFail1"), EndlessFloorsPlugin.Instance.assetManager.Get<SoundObject>("AudDanielFail2") };

            roomBase.SetParent(room.objectObject.transform);
            Destroy(johnnyBase.GetComponent<Animator>());
            Destroy(johnnyBase.GetComponent<PropagatedAudioManagerAnimator>());
            Destroy(johnnyBase.Find("Mouth").gameObject);
            audMan = johnnyBase.gameObject.AddComponent<PropagatedAudioManager>();
            audMan.ReflectionSetVariable("maxDistance", 200f); // im lazy i shouldnt be using this
            johnnyBase.GetComponent<SpriteRenderer>().sprite = EndlessFloorsPlugin.Instance.assetManager.Get<Sprite>("DanielIdle");
            volumeAnimator = johnnyBase.gameObject.AddComponent<CustomVolumeAnimator>();
            animator = johnnyBase.gameObject.AddComponent<CustomSpriteAnimator>();
            animator.animations.Add("Idle", new CustomAnimation<Sprite>(new Sprite[] { EndlessFloorsPlugin.Instance.assetManager.Get<Sprite>("DanielIdle") }, 1f));
            animator.animations.Add("Talk1", new CustomAnimation<Sprite>(new Sprite[] { EndlessFloorsPlugin.Instance.assetManager.Get<Sprite>("DanielTalk1") }, 0.25f));
            animator.animations.Add("Talk2", new CustomAnimation<Sprite>(new Sprite[] { EndlessFloorsPlugin.Instance.assetManager.Get<Sprite>("DanielTalk2") }, 0.25f));
            animator.animations.Add("Talk3", new CustomAnimation<Sprite>(new Sprite[] { EndlessFloorsPlugin.Instance.assetManager.Get<Sprite>("DanielTalk3") }, 0.25f));
            animator.spriteRenderer = johnnyBase.GetComponent<SpriteRenderer>();
            volumeAnimator.animator = animator;
            volumeAnimator.audioSource = audMan.audioDevice;
            volumeAnimator.animations = new string[] { "Idle", "Talk1", "Talk2", "Talk3" };
            johnnyBase.gameObject.SetActive(true);
            foreach (Door door in this.room.doors)
            {
                door.Unlock();
            }
            int i = 0;
            while (i < shopItems && i < room.itemSpawnPoints.Count)
            {
                ItemSpawnPoint itemSpawnPoint = room.itemSpawnPoints[i];
                Pickup pickup = room.ec.CreateItem(room, Singleton<CoreGameManager>.Instance.NoneItem, itemSpawnPoint.position);
                pickup.OnItemPurchased += this.ItemPurchased;
                pickup.OnItemDenied += this.ItemDenied;
                pickup.OnItemCollected += this.ItemCollected;
                pickup.showDescription = true;
                pickups.Add(pickup);
                i++;
            }
            Restock();
            Instance = this;
        }

        public void UpdateAllTags()
        {
            for (int i = 0; i < pickups.Count; i++)
            {
                Pickup pickup = pickups[i];
                if (pickup.item.itemType == Items.None)
                {
                    if (purchasedItems[i])
                    {
                        priceTags[i].SetText(Singleton<LocalizationManager>.Instance.GetLocalizedText("TAG_Sold"));
                    }
                    else
                    {
                        priceTags[i].SetText(Singleton<LocalizationManager>.Instance.GetLocalizedText("TAG_Out"));
                    }
                    continue;
                }
                int originalPrice = upgrades[i].GetCost(EndlessFloorsPlugin.currentSave.GetUpgradeCount(upgrades[i].id));
                int finalPrice = originalPrice;
                if (discounts.ContainsKey(i))
                {
                    float discount = discounts[i];
                    float discountedPrice = (float)originalPrice * discount;
                    finalPrice = Mathf.RoundToInt(discountedPrice - discountedPrice % 10f); //what the fuck is this doing
                    priceTags[i].SetSale(originalPrice, finalPrice);
                }
                else
                {
                    priceTags[i].SetText(finalPrice.ToString());
                }
                pickup.price = finalPrice;
                pickup.free = false;
                pickup.gameObject.SetActive(true);
            }
        }

        public void Restock()
        {
            discounts = new Dictionary<int, float>();
            purchasedItems = new bool[shopItems];
            upgrades.Clear();
            PopulateShop();
            for (int i = 0; i < pickups.Count; i++)
            {
                Pickup pickup = pickups[i];
                pickup.gameObject.GetOrAddComponent<UpgradePickupMarker>().upgrade = upgrades[i];
                pickup.AssignItem(EndlessFloorsPlugin.upgradeObject);
                int originalPrice = upgrades[i].GetCost(EndlessFloorsPlugin.currentSave.GetUpgradeCount(upgrades[i].id));
                int finalPrice = originalPrice;

                if (UnityEngine.Random.value < saleChance)
                {
                    float discount = UnityEngine.Random.Range(minSaleDiscount, maxSaleDiscount);
                    discounts.Add(i, discount);
                }
                pickup.price = finalPrice;
                pickup.showDescription = true;
                pickup.free = false;
                pickup.gameObject.SetActive(true);
            }
            UpdateAllTags();
        }

        public static StandardUpgrade GetRandomValidUpgrade(int seed)
        {
            List<WeightedSelection<StandardUpgrade>> upgradesTemp = new List<WeightedSelection<StandardUpgrade>>();
            EndlessFloorsPlugin.Upgrades.Do(x =>
            {
                if (x.Key == "none") return;
                if (x.Value.weight == 0) return;
                if (x.Key == "reroll") return;
                if (!x.Value.ShouldAppear(EndlessFloorsPlugin.currentSave.GetUpgradeCount(x.Value.id))) return;
                upgradesTemp.Add(new WeightedSelection<StandardUpgrade>()
                {
                    selection = x.Value,
                    weight = x.Value.weight
                });
            });
            WeightedSelection<StandardUpgrade>[] weightedUpgrades = upgradesTemp.ToArray();
            if (weightedUpgrades.Length == 0) return null;
            return WeightedSelection<StandardUpgrade>.ControlledRandomSelection(weightedUpgrades, new System.Random(seed));
        }

        public void PopulateShop()
        {
            try
            {
                int randRang = shopItems - (alwaysAddReroll ? 1 : 0);
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
                        upgrades.Add(EndlessFloorsPlugin.Upgrades["error"]);
                        continue;
                    }
                    StandardUpgrade gu = WeightedSelection<StandardUpgrade>.RandomSelection(weightedUpgrades);
                    upgrades.Add(gu);
                    if (!gu.ShouldAppear(EndlessFloorsPlugin.currentSave.GetUpgradeCount(gu.id) + upgrades.Where(x => x == gu).Count()))
                    {
                        weightedUpgrades = weightedUpgrades.Where(x => x.selection.id != gu.id).ToArray();
                    }
                }
                if (alwaysAddReroll)
                {
                    upgrades.Add(EndlessFloorsPlugin.Upgrades["reroll"]);
                }
            }
            catch(Exception ex)
            {
                MTM101BaldiDevAPI.CauseCrash(EndlessFloorsPlugin.Instance.Info, ex);
            }
        }

        public override void OnPlayerEnter(PlayerManager player)
        {
            Singleton<CoreGameManager>.Instance.GetHud(player.playerNumber).PointsAnimator.ShowDisplay(true);
            if (!hasGreeted)
            {
                hasGreeted = true;
                audMan.QueueAudio(initialGreeting);
            }
        }

        public override void OnPlayerExit(PlayerManager player)
        {
            Singleton<CoreGameManager>.Instance.GetHud(player.playerNumber).PointsAnimator.ShowDisplay(false);
        }
    }


}
