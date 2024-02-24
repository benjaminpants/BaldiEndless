using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BaldiEndless
{
    public enum UpgradePurchaseBehavior
    {
        Nothing,
        FillUpgradeSlot,
        IncrementCounter
    }

    public struct UpgradeLevel
    {
        public string icon;
        public int cost;
        public string descLoca;
    }


    public class StandardUpgrade
    {
        public string id { internal set; get; }
        public UpgradeLevel[] levels = new UpgradeLevel[0];
        public int weight { internal set; get; } = 100;
        public UpgradePurchaseBehavior behavior = UpgradePurchaseBehavior.FillUpgradeSlot;

        protected int ClampLvl(int level) => Mathf.Clamp(level, 0, levels.Length - 1);
        public virtual Sprite GetIcon(int level) => EndlessFloorsPlugin.Instance.UpgradeIcons[levels[ClampLvl(level)].icon];
        public virtual int GetCost(int level) => levels[ClampLvl(level)].cost;
        public virtual string GetLoca(int level) => levels[ClampLvl(level)].descLoca;
        public virtual int CalculateSellPrice(int level) => GetCost(ClampLvl(level)) / 8;
        public virtual bool ShouldAppear(int currentLevel)
        {
            return currentLevel < levels.Length;
        }
        public virtual void OnPurchase()
        {

        }
    }
}
