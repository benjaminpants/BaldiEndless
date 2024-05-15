using System;
using System.Collections.Generic;
using System.Text;

namespace BaldiEndless
{
    public static class EndlessUpgradeRegisters
    {
        // NOTE TO MODDERS: IF YOU WANT TO CREATE YOUR OWN UPGRADES AS A SEPERATE MOD, INHERIT FROM STANDARDUPGRADE AND OVERRIDE THE GETICON FUNCTION!!
        public static void Register(StandardUpgrade upgrade)
        {
            EndlessFloorsPlugin.Upgrades.Add(upgrade.id, upgrade);
        }
        internal static void RegisterDefaults()
        {
            // luck
            Register(new StandardUpgrade()
            {
                id="luck",
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel()
                    {
                        icon = "Luck1",
                        cost = 1000,
                        descLoca = "Upg_Luck"
                    },
                    new UpgradeLevel()
                    {
                        icon = "Luck2",
                        cost = 1750,
                        descLoca = "Upg_Luck"
                    },
                    new UpgradeLevel()
                    {
                        icon = "Luck3",
                        cost = 2000,
                        descLoca = "Upg_Luck"
                    },
                    new UpgradeLevel()
                    {
                        icon = "Luck4",
                        cost = 3500,
                        descLoca = "Upg_Luck"
                    },
                    new UpgradeLevel()
                    {
                        icon = "Luck5",
                        cost = 6000,
                        descLoca = "Upg_Luck"
                    }
                },
                weight = 35
            });
            // reroll
            Register(new RerollUpgrade()
            {
                id = "reroll",
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel()
                    {
                        icon="Reroll",
                        cost=500,
                        descLoca="Upg_Reroll"
                    }
                },
                weight = 30,
                behavior = UpgradePurchaseBehavior.Nothing
            });
            // autotag
            Register(new StandardUpgrade()
            {
                id="autotag",
                weight = 120,
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel()
                    {
                        icon="AutoTag",
                        cost=800,
                        descLoca="Upg_AutoTag"
                    },
                    new UpgradeLevel()
                    {
                        icon="AutoTag2",
                        cost=1250,
                        descLoca="Upg_AutoTag2"
                    }
                }
            });
            // free exit
            Register(new ExitUpgrade()
            {
                id = "freeexit",
                weight = 40,
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel()
                    {
                        icon="FreeExit",
                        cost=2000,
                        descLoca="Upg_FreeExit"
                    },
                    new UpgradeLevel()
                    {
                        icon="FreeExit2",
                        cost=2500,
                        descLoca="Upg_FreeExit2"
                    }
                }
            });
            // piggy bank
            Register(new StandardUpgrade()
            {
                id = "bank",
                weight = 32,
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel()
                    {
                        icon="Bank1",
                        cost=1000,
                        descLoca="Upg_Bank"
                    },
                    new UpgradeLevel()
                    {
                        icon="Bank2",
                        cost=500,
                        descLoca="Upg_Bank"
                    },
                    new UpgradeLevel()
                    {
                        icon="Bank3",
                        cost=1000,
                        descLoca="Upg_Bank"
                    },
                    new UpgradeLevel()
                    {
                        icon="Bank5",
                        cost=1500,
                        descLoca="Upg_Bank"
                    },
                    new UpgradeLevel()
                    {
                        icon="Bank6",
                        cost=1500,
                        descLoca="Upg_Bank"
                    },
                    new UpgradeLevel()
                    {
                        icon="Bank10",
                        cost=5000,
                        descLoca="Upg_Bank"
                    },
                }
            });
            // drink efficiency
            Register(new StandardUpgrade()
            {
                id = "drink",
                weight = 90,
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel()
                    {
                        icon="Drink1",
                        cost=1000,
                        descLoca="Upg_Drink"
                    },
                    new UpgradeLevel()
                    {
                        icon="Drink2",
                        cost=1250,
                        descLoca="Upg_Drink"
                    },
                    new UpgradeLevel()
                    {
                        icon="Drink3",
                        cost=1500,
                        descLoca="Upg_Drink"
                    },
                    new UpgradeLevel()
                    {
                        icon="Drink4",
                        cost=1500,
                        descLoca="Upg_Drink"
                    }
                }
            });
            // slow bsoda
            Register(new StandardUpgrade()
            {
                id = "slowsoda",
                weight = 70,
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel()
                    {
                        icon="SlowSpray1",
                        cost=1000,
                        descLoca="Upg_SlowBSODA"
                    },
                    new UpgradeLevel()
                    {
                        icon="SlowSpray2",
                        cost=2000,
                        descLoca="Upg_SlowBSODA"
                    },
                    new UpgradeLevel()
                    {
                        icon="SlowSpray3",
                        cost=2250,
                        descLoca="Upg_SlowBSODA"
                    },
                    new UpgradeLevel()
                    {
                        icon="SlowSpray4",
                        cost=3000,
                        descLoca="Upg_SlowBSODA"
                    },
                    new UpgradeLevel()
                    {
                        icon="SlowSpray5",
                        cost=4000,
                        descLoca="Upg_SlowBSODAMax"
                    }
                }
            });
            // stamina gain
            Register(new StandardUpgrade()
            {
                id = "stamina",
                weight = 90,
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel()
                    {
                        icon="Stamina1",
                        cost=1000,
                        descLoca="Upg_Stamina"
                    },
                    new UpgradeLevel()
                    {
                        icon="Stamina2",
                        cost=1250,
                        descLoca="Upg_Stamina"
                    },
                    new UpgradeLevel()
                    {
                        icon="Stamina3",
                        cost=2000,
                        descLoca="Upg_Stamina"
                    },
                    new UpgradeLevel()
                    {
                        icon="Stamina4",
                        cost=2500,
                        descLoca="Upg_Stamina"
                    }
                }
            });
            // hungry bully
            Register(new StandardUpgrade()
            {
                id = "hungrybully",
                weight = 90,
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel()
                    {
                        icon="HungryBully",
                        cost=700,
                        descLoca="Upg_HungryBully"
                    }
                }
            });
            // item slots
            Register(new SlotUpgrade()
            {
                id = "slots",
                weight = 92,
                behavior=UpgradePurchaseBehavior.IncrementCounter,
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel() //you wont ever see this one
                    {
                        icon="SlotPlus",
                        cost=0,
                        descLoca="Upg_Error"
                    },
                    new UpgradeLevel()
                    {
                        icon="SlotPlus",
                        cost=500,
                        descLoca="Upg_ItemSlot"
                    },
                    new UpgradeLevel()
                    {
                        icon="SlotPlus",
                        cost=1000,
                        descLoca="Upg_ItemSlot"
                    },
                    new UpgradeLevel()
                    {
                        icon="SlotPlus",
                        cost=800,
                        descLoca="Upg_ItemSlot"
                    },
                    new UpgradeLevel()
                    {
                        icon="SlotPlus",
                        cost=700,
                        descLoca="Upg_ItemSlot"
                    }
                }
            });
            // life restore
            Register(new ExtraLifeUpgrade()
            {
                id="life",
                behavior=UpgradePurchaseBehavior.IncrementCounter,
                weight=80,
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel()
                    {
                        icon="ExtraLife",
                        cost=1000,
                        descLoca="Upg_ExtraLife"
                    },
                    new UpgradeLevel()
                    {
                        icon="ExtraLife",
                        cost=2000,
                        descLoca="Upg_ExtraLife"
                    },
                    new UpgradeLevel()
                    {
                        icon="ExtraLife",
                        cost=4000,
                        descLoca="Upg_ExtraLife"
                    },
                    new UpgradeLevel()
                    {
                        icon="ExtraLife",
                        cost=8000,
                        descLoca="Upg_ExtraLife"
                    },
                    new UpgradeLevel()
                    {
                        icon="ExtraLife",
                        cost=9000,
                        descLoca="Upg_ExtraLife"
                    },
                    new UpgradeLevel()
                    {
                        icon="ExtraLife",
                        cost=9300,
                        descLoca="Upg_ExtraLife"
                    },
                    new UpgradeLevel()
                    {
                        icon="ExtraLife",
                        cost=9600,
                        descLoca="Upg_ExtraLife"
                    },
                    new UpgradeLevel()
                    {
                        icon="ExtraLife",
                        cost=9900,
                        descLoca="Upg_ExtraLife"
                    },
                    new UpgradeLevel()
                    {
                        icon="ExtraLife",
                        cost=9990,
                        descLoca="Upg_ExtraLife"
                    },
                    new UpgradeLevel()
                    {
                        icon="ExtraLife",
                        cost=9998,
                        descLoca="Upg_ExtraLife"
                    },
                    new UpgradeLevel()
                    {
                        icon="ExtraLife",
                        cost=9999,
                        descLoca="Upg_ExtraLifeLast"
                    },
                }
            });
            // favoritism
            Register(new StandardUpgrade()
            {
                id = "favor",
                weight = 80,
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel()
                    {
                        icon="Favoritism",
                        cost=1500,
                        descLoca="Upg_Favoritism"
                    }
                }
            });
            // timeslow clock
            Register(new StandardUpgrade()
            {
                id = "timeclock",
                weight = 80,
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel()
                    {
                        icon="ClockSlow",
                        cost=3000,
                        descLoca="Upg_Timeslow"
                    },
                    new UpgradeLevel()
                    {
                        icon="ClockSlow2",
                        cost=5000,
                        descLoca="Upg_Timeslow2"
                    },
                    new UpgradeLevel()
                    {
                        icon="ClockSlow3",
                        cost=8000,
                        descLoca="Upg_Timeslow3"
                    }
                }
            });
            // bonus life
            Register(new BonusLifeUpgrade()
            {
                id = "bonuslife",
                weight = 80,
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel()
                    {
                        icon="ExtraPermaLife",
                        cost=5000,
                        descLoca="Upg_BonusLife"
                    },
                    new UpgradeLevel()
                    {
                        icon="ExtraPermaLife",
                        cost=8000,
                        descLoca="Upg_BonusLife"
                    },
                    new UpgradeLevel()
                    {
                        icon="ExtraPermaLife",
                        cost=10000,
                        descLoca="Upg_BonusLife"
                    }
                },
                behavior = UpgradePurchaseBehavior.IncrementCounter
            });
            // ytps upgrade
            /*
            Register(new StandardUpgrade()
            {
                id = "ytpsmult",
                weight = 60,
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel()
                    {
                        icon="Multiply1",
                        cost=444,
                        descLoca="Upg_MultiplyYTP"
                    },
                    new UpgradeLevel()
                    {
                        icon="Multiply2",
                        cost=888,
                        descLoca="Upg_MultiplyYTP"
                    },
                    new UpgradeLevel()
                    {
                        icon="Multiply3",
                        cost=999,
                        descLoca="Upg_MultiplyYTP"
                    }
                }
            });*/
            // speedy boots
            Register(new StandardUpgrade()
            {
                id = "speedyboots",
                weight = 80,
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel()
                    {
                        icon="SpeedyBoots",
                        cost=1000,
                        descLoca="Upg_SpeedyBoots"
                    },
                    new UpgradeLevel()
                    {
                        icon="SpeedyBoots2",
                        cost=1500,
                        descLoca="Upg_SpeedyBoots2"
                    },
                    /*new UpgradeLevel() // why so expensive? because this is fucking overpowered.
                    {
                        icon="SpeedyBoots3",
                        cost=2500,
                        descLoca="Upg_SpeedyBoots3"
                    }*/
                }
            });
        }
    }
}
