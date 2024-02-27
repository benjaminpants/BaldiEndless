using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
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

    class ExtraLifeUpgrade : StandardUpgrade
    {
        static private FieldInfo _defaultLives = AccessTools.Field(typeof(BaseGameManager), "defaultLives");
        public override void OnPurchase()
        {
            Singleton<CoreGameManager>.Instance.SetLives((int)_defaultLives.GetValue(Singleton<BaseGameManager>.Instance));
            Singleton<ElevatorScreen>.Instance.Invoke("UpdateLives", 0f);
            base.OnPurchase();
        }
        public override bool ShouldAppear(int currentLevel)
        {
            return base.ShouldAppear(currentLevel) && 
                (Singleton<CoreGameManager>.Instance.Lives < (int)_defaultLives.GetValue(Singleton<BaseGameManager>.Instance)) &&
                Singleton<CoreGameManager>.Instance.currentMode != EndlessFloorsPlugin.NNFloorMode;
        }
    }

    class BonusLifeUpgrade : StandardUpgrade
    {
        static internal FieldInfo _defaultLives = AccessTools.Field(typeof(BaseGameManager), "defaultLives");
        public override void OnPurchase()
        {
            if (Singleton<CoreGameManager>.Instance.Lives >= (int)_defaultLives.GetValue(Singleton<BaseGameManager>.Instance))
            {
                Singleton<CoreGameManager>.Instance.SetLives(2 + EndlessFloorsPlugin.currentSave.GetUpgradeCount("bonuslife"));
            }
            _defaultLives.SetValue(Singleton<BaseGameManager>.Instance, 2 + EndlessFloorsPlugin.currentSave.GetUpgradeCount("bonuslife"));
            Singleton<ElevatorScreen>.Instance.Invoke("UpdateLives",0f);
            base.OnPurchase();
        }
        public override bool ShouldAppear(int currentLevel)
        {
            return base.ShouldAppear(currentLevel) && Singleton<CoreGameManager>.Instance.currentMode != EndlessFloorsPlugin.NNFloorMode
                && Singleton<CoreGameManager>.Instance.GetPoints(0) >= Mathf.RoundToInt(GetCost(currentLevel) * 0.75f);
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
