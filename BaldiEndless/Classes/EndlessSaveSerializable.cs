using System;
using System.Collections.Generic;

//currently unused as i've given up on attempting to save endless games.
//if anyone wants to take a stab at it here is a starting point i guess?
namespace BaldiEndless
{
    [Serializable]
    public class EndlessSaveSerializable
    {
        public static Dictionary<string, GameUpgrade> classThings = new Dictionary<string, GameUpgrade>();


        public int livesBought = 0;
        public int currentFloorNumber = 0;
        public int[] purchasedUpgradesStacks;
        public string[] purchasedUpgrades;
        public SavedGameData sdg;
        public EndlessSaveSerializable(EndlessSave toSave, SavedGameData savedgamedat = null)
        {
            livesBought = toSave.livesBought;
            currentFloorNumber = toSave.currentFloorData.FloorID;
            List<int> pus = new List<int>();
            List<string> kitty = new List<string>(); //i couldn't help myself
            foreach (PurchasedUpgradeData pud in toSave.purchasedUpgrades)
            {
                pus.Add(pud.stack);
                kitty.Add(pud.myUpgrade);
            }
            purchasedUpgradesStacks = pus.ToArray();
            purchasedUpgrades = kitty.ToArray();
            sdg = savedgamedat;
        }

        public EndlessSave ReturnToSave()
        {
            EndlessSave enS = new EndlessSave();
            enS.livesBought = livesBought;
            enS.currentFloorData.FloorID = currentFloorNumber;
            for (int i = 0; i < purchasedUpgrades.Length; i++)
            {
                enS.purchasedUpgrades.Add(new PurchasedUpgradeData() 
                { 
                    myUpgrade = purchasedUpgrades[i],
                    stack = purchasedUpgradesStacks[i],
                });
            }
            return enS;
        }
    }
}
