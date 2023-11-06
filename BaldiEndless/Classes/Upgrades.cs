using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BaldiEndless
{
    [Serializable]
    public class PurchasedUpgradeData
    {
        public int stack;
        public string myUpgrade;
    }

    public abstract class GameUpgrade
    {

        public abstract int Cost { get; }
        public abstract int SellPrice { get; }
        public abstract string Description { get; }
        public abstract string[] Icons { get; }
        public Sprite GetSprite(int stack)
        {
            return EndlessFloorsPlugin.Instance.UpgradeIcons[Icons[Mathf.Clamp(stack,0,Icons.Length - 1)]];
        }

        public virtual int MaxStack { 
            get 
            {
                return 0;
            } 
        }

        public virtual void OnSell()
        {

        }

        public virtual void OnPurchase()
        {
            if (EndlessFloorsPlugin.currentSave.GetUpgradeCount(GetType()) < MaxStack)
            {
                EndlessFloorsPlugin.currentSave.AddUpgrade(this);
            }
        }

        public virtual bool CanPurchase()
        {
            return !(EndlessFloorsPlugin.currentSave.purchasedUpgrades.Count > 4 && !EndlessFloorsPlugin.currentSave.HasUpgrade(GetType()));
        }

        public virtual bool ShouldAppear()
        {
            return (EndlessFloorsPlugin.currentSave.GetUpgradeCount(GetType()) < MaxStack);
        }
    }

    // Grants the player an extra life
    public class ExtraLife : GameUpgrade
    {
        public override int Cost => 300 + (1200 * (EndlessFloorsPlugin.currentSave.livesBought));

        public override int SellPrice => throw new NotImplementedException();

        public override string Description => "Upg_ExtraLife";

        public override string[] Icons => new string[] { "ExtraLife" };

        public override int MaxStack => 999; //so more then one can appear in the shop, doesn't matter since both OnPurchase and MaxStack are bye byed.
        public override void OnPurchase()
        {
            Singleton<CoreGameManager>.Instance.AddLives(1);
            EndlessFloorsPlugin.currentSave.livesBought++;
        }

        public override bool CanPurchase()
        {
            return true;
        }

        public override bool ShouldAppear()
        {
            return Singleton<CoreGameManager>.Instance.currentMode != EndlessFloorsPlugin.NNFloorMode;
        }
    }

    // Autocompletes an exit for the player
    public class SkipExit : GameUpgrade
    {
        public override int Cost => 650;

        public override int SellPrice => 100;

        public override string Description => "Upg_FreeExit";

        public override string[] Icons => new string[] { "FreeExit" };

        public override int MaxStack => 1;

        public override bool ShouldAppear()
        {
            return base.ShouldAppear() && (EndlessFloorsPlugin.currentFloorData.exitCount != 1); //no instant completion in the halls.
        }
    }

    // Increases the chances for a good item from a present
    public class PresentLuck : GameUpgrade
    {
        public override int Cost 
        { 
            get
            {
                int amount = EndlessFloorsPlugin.currentSave.GetUpgradeCount(GetType());
                return 600 + ((amount * amount) * 75);
            }
        }

        public override int SellPrice => Cost / 10;

        public override string Description => "Upg_Luck";

        public static float[] LuckValues => new float[] { 1.45f, 2f, 2.37f, 3f, 4f };

        public override string[] Icons => new string[] { "Luck1", "Luck2", "Luck3", "Luck4", "Luck5" };

        public override int MaxStack => 5;

    }

    // Rerolls the shop
    public class Reroll : GameUpgrade
    {
        public override int Cost => 125;

        public override int SellPrice => throw new NotImplementedException();

        public override string Description => "Upg_Reroll";

        public override string[] Icons => new string[] { "Reroll" };

        public override int MaxStack => 999; //so more then one can appear in the shop, doesn't matter since both OnPurchase and MaxStack are bye byed.
        public override void OnPurchase()
        {
            // reroll shop
            UpgradeShop.Instance.Purchased = new bool[8];
            UpgradeShop.Instance.Upgrades.Clear();
            UpgradeShop.Instance.PopulateShop();
            UpgradeShop.Instance.GetComponent<StoreScreen>().BuyItem(-1); // this is actual dogshit code
        }

        public override bool CanPurchase()
        {
            return true;
        }
    }

    // When Baldi is done counting, if the player has this upgrade, they'll get a faculty name tag automatically applied
    public class AutoTag : GameUpgrade
    {
        public override int Cost => 325;

        public override int SellPrice => 35;

        public override string Description => "Upg_AutoTag";

        public override string[] Icons => new string[] { "AutoTag" };

        public override int MaxStack => 1;
    }

    // If the player has failed a fieldtrip, this instantly unsets the flag, if they haven't, they'll be able to redo the fieldtrip the next time they die
    public class FieldtripRedo : GameUpgrade
    {
        public override int Cost => 200;

        public override int SellPrice => 75;

        public override string Description => "Upg_FieldtripRedo";

        public override string[] Icons => new string[] { "FieldtripRedo" };

        public override int MaxStack => 1;

        public override bool CanPurchase()
        {
            if (Singleton<CoreGameManager>.Instance.tripPlayed)
            {
                return true;
            }
            return base.CanPurchase();
        }

        public override bool ShouldAppear()
        {
            return base.ShouldAppear() && Singleton<CoreGameManager>.Instance.currentMode != EndlessFloorsPlugin.NNFloorMode;
        }

        public override void OnPurchase()
        {
            if (Singleton<CoreGameManager>.Instance.tripPlayed)
            {
                Singleton<CoreGameManager>.Instance.tripPlayed = false;
            }
            else
            {
                base.OnPurchase();
            }
        }
    }

    // Increases the amount of stamina by drinking the fountain by 10%
    public class DrinkEfficient : GameUpgrade
    {
        public override int Cost => 200 + (EndlessFloorsPlugin.currentSave.GetUpgradeCount(GetType()) * 75);

        public override int SellPrice => 75;

        public override string Description => "Upg_Drink";

        public override string[] Icons => new string[] { "Drink1", "Drink2", "Drink3", "Drink4" };

        public override int MaxStack => 4;

    }

    // Slows down the bsoda by 10%
    public class SlowBSODA : GameUpgrade
    {
        public override int Cost => 325 + (EndlessFloorsPlugin.currentSave.GetUpgradeCount(GetType()) * 175);

        public override int SellPrice => 100;

        public override string Description => "Upg_SlowBSODA";

        public override string[] Icons => new string[] { "SlowSpray1", "SlowSpray2", "SlowSpray3"};

        public override int MaxStack => 3;

    }

    // If the player uses an item it has a rare chance to become a quarter instead.
    public class Banking : GameUpgrade
    {
        public override int Cost => Costs[EndlessFloorsPlugin.currentSave.GetUpgradeCount(GetType())];

        public static float[] Percentages => new float[] { 0f, 1f, 2f, 3f, 5f, 6f, 10f };

        public override int SellPrice => Cost / 15;

        public override string Description => "Upg_Bank";

        public int[] Costs => new int[] { 400, 100, 400, 350, 700, 999, 9999 };

        public override string[] Icons => new string[] { "Bank1", "Bank2", "Bank3", "Bank5", "Bank6", "Bank10" };

        public override int MaxStack => 6;
    }

    // Increase the players max stamina by how many times this is stacked x25
    public class StaminaIncrease : GameUpgrade
    {
        public override int Cost => 325 + (EndlessFloorsPlugin.currentSave.staminasBought * 175);

        public override int SellPrice => 35 * EndlessFloorsPlugin.currentSave.staminasBought;

        public override string Description => "Upg_Stamina";

        public override string[] Icons => new string[] { "Stamina1", "Stamina2", "Stamina3", "Stamina4" };

        public override int MaxStack => 4;
    }

    // Increase the time you have to get to Mrs Pomps Classroom
    public class PompTimeIncrease : GameUpgrade
    {
        public override int Cost => 400 + (EndlessFloorsPlugin.currentSave.pompIncreaseMinutes * 250);

        public override int SellPrice => 40 + (25 * EndlessFloorsPlugin.currentSave.staminasBought);

        public override string Description => "Upg_PompIncrease";

        public override string[] Icons => new string[] { "PompReduce", "PompReduce2", "PompReduce3", "PompReduce4" };

        public override int MaxStack => 4;

    }

    // Make bully prioritize stealing zesty bars, bsoda, and apples.
    // Steal Order: Zesty Bars > BSODA > Apple
    public class HungryBully : GameUpgrade
    {
        public override int Cost => 500;

        public override int SellPrice => 50;

        public override string Description => "Upg_HungryBully";

        public override string[] Icons => new string[] { "HungryBully" };

        public override int MaxStack => 1;
    }

    // Make Grappling hooks able to break windows(or doors if times is installed)
    public class GrappleBreakWindows : GameUpgrade
    {
        public override int Cost => EndlessFloorsPlugin.TimesInstalled ? 350 : 250;

        public override int SellPrice => 75;

        public override string Description => EndlessFloorsPlugin.TimesInstalled ? "Upg_GrappleBreakDoor" : "Upg_GrappleBreak";

        public override string[] Icons => EndlessFloorsPlugin.TimesInstalled ? new string[] { "GrappleBreakDoor" } : new string[] { "GrappleBreak" };

        public override int MaxStack => 1;
    }

    // Make Whistles make principal even faster
    public class HyperWhistle : GameUpgrade
    {
        public override int Cost => 200;

        public override int SellPrice => 50;

        public override string Description => "Upg_SpeedyWhistle";

        public override string[] Icons => new string[] { "SpeedyWhistle" };

        public override int MaxStack => 1;
    }
}
