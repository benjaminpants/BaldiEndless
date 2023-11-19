using System;
using System.Collections.Generic;
using System.Text;

namespace BaldiEndless
{
    public class EndlessSave
    {
        public FloorData currentFloorData = new FloorData();

        // lol
        public int livesBought = 0;
        // stupid workaround
        public bool hasClaimedFreeYTP = false;
        public int staminasBought => GetUpgradeCount(typeof(StaminaIncrease));
        public int pompIncreaseMinutes => GetUpgradeCount(typeof(PompTimeIncrease));
        public float luckValue => GetUpgradeCount(typeof(PresentLuck)) == 0 ? 1 : PresentLuck.LuckValues[GetUpgradeCount(typeof(PresentLuck)) - 1];


        // TODO: redo how this shop system works I did not realize everything I would've needed for this but I don't want to rewrite it.
        public List<PurchasedUpgradeData> purchasedUpgrades = new List<PurchasedUpgradeData>();

        [NonSerialized]
        public List<GameUpgrade> purchasedUpgradesClasses = new List<GameUpgrade>();

        public void AddUpgrade(GameUpgrade upg)
        {
            string theType = upg.GetType().ToString();
            PurchasedUpgradeData alUpg = purchasedUpgrades.Find(x => x.myUpgrade == theType);
            if (alUpg == null)
            {
                purchasedUpgrades.Add(new PurchasedUpgradeData()
                {
                    stack = 1,
                    myUpgrade = theType,
                });
                purchasedUpgradesClasses.Add(upg);
            }
            else
            {
                alUpg.stack += 1;
            }
        }

        public bool RemoveOneUpgrade(Type upg)
        {
            string theType = upg.ToString();
            PurchasedUpgradeData alUpg = purchasedUpgrades.Find(x => x.myUpgrade == theType);
            if (alUpg != null)
            {
                if (alUpg.stack != 0)
                {
                    alUpg.stack -= 1;
                }
                return true;
            }
            return false;
        }

        public void RemoveUpgrade(int slot)
        {
            purchasedUpgrades.RemoveAt(slot);
            purchasedUpgradesClasses.RemoveAt(slot);
        }

        public int GetUpgradeCount(Type upg)
        {
            PurchasedUpgradeData mupg = purchasedUpgrades.Find(x => x.myUpgrade == upg.ToString());
            if (mupg == null) return 0;
            return mupg.stack;
        }

        public bool HasUpgrade(Type upg)
        {
            return GetUpgradeCount(upg) > 0;
        }
    }
}
