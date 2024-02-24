using MTM101BaldAPI;
using MTM101BaldAPI.Reflection;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BaldiEndless
{
    class RerollUpgrade : StandardUpgrade
    {
        public override void OnPurchase()
        {
            base.OnPurchase();
            UpgradeShop.Instance.Purchased = new bool[8];
            UpgradeShop.Instance.Upgrades.Clear();
            UpgradeShop.Instance.PopulateShop();
            UpgradeShop.Instance.GetComponent<StoreScreen>().BuyItem(-1); // this is actual dogshit code why havent i changed this
        }
    }

    class ExitUpgrade : StandardUpgrade
    {
        public override bool ShouldAppear(int currentLevel)
        {
            return base.ShouldAppear(currentLevel) && (EndlessFloorsPlugin.currentFloorData.exitCount > (currentLevel + 1));
        }
    }

    class SlotUpgrade : StandardUpgrade
    {
        public override void OnPurchase()
        {
            base.OnPurchase();
            Singleton<CoreGameManager>.Instance.GetPlayer(0).itm.ReflectionSetVariable("maxItem", EndlessFloorsPlugin.currentSave.itemSlots - 1);
            Singleton<CoreGameManager>.Instance.GetPlayer(0).itm.UpdateItems();
        }
    }

    class BrokenUpgrade : StandardUpgrade
    {
        public override bool ShouldAppear(int currentLevel)
        {
            return false;
        }

        public override int GetCost(int level)
        {
            return Singleton<CoreGameManager>.Instance.GetPoints(0) + 1;
        }

        public override void OnPurchase()
        {
            MTM101BaldiDevAPI.CauseCrash(EndlessFloorsPlugin.Instance.Info, new NotImplementedException("Attempted to buy Error upgrade!"));
        }
    }
}
