﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BaldiEndless
{
    public static class EndlessUpgradeRegisters
    {
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
                        cost = 600,
                        descLoca = "Upg_Luck"
                    },
                    new UpgradeLevel()
                    {
                        icon = "Luck2",
                        cost = 675,
                        descLoca = "Upg_Luck"
                    },
                    new UpgradeLevel()
                    {
                        icon = "Luck3",
                        cost = 900,
                        descLoca = "Upg_Luck"
                    },
                    new UpgradeLevel()
                    {
                        icon = "Luck4",
                        cost = 1275,
                        descLoca = "Upg_Luck"
                    },
                    new UpgradeLevel()
                    {
                        icon = "Luck5",
                        cost = 1800,
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
                        cost=125,
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
                        cost=325,
                        descLoca="Upg_AutoTag"
                    },
                    new UpgradeLevel()
                    {
                        icon="AutoTag2",
                        cost=650,
                        descLoca="Upg_AutoTag2"
                    }
                }
            });
            // free exit
            Register(new ExitUpgrade()
            {
                id = "freeexit",
                weight = 45,
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel()
                    {
                        icon="FreeExit",
                        cost=650,
                        descLoca="Upg_FreeExit"
                    },
                    new UpgradeLevel()
                    {
                        icon="FreeExit2",
                        cost=1350,
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
                        cost=400,
                        descLoca="Upg_Bank"
                    },
                    new UpgradeLevel()
                    {
                        icon="Bank2",
                        cost=100,
                        descLoca="Upg_Bank"
                    },
                    new UpgradeLevel()
                    {
                        icon="Bank3",
                        cost=400,
                        descLoca="Upg_Bank"
                    },
                    new UpgradeLevel()
                    {
                        icon="Bank5",
                        cost=500,
                        descLoca="Upg_Bank"
                    },
                    new UpgradeLevel()
                    {
                        icon="Bank6",
                        cost=700,
                        descLoca="Upg_Bank"
                    },
                    new UpgradeLevel()
                    {
                        icon="Bank10",
                        cost=999,
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
                        cost=200,
                        descLoca="Upg_Drink"
                    },
                    new UpgradeLevel()
                    {
                        icon="Drink2",
                        cost=275,
                        descLoca="Upg_Drink"
                    },
                    new UpgradeLevel()
                    {
                        icon="Drink3",
                        cost=350,
                        descLoca="Upg_Drink"
                    },
                    new UpgradeLevel()
                    {
                        icon="Drink4",
                        cost=425,
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
                        cost=325,
                        descLoca="Upg_SlowBSODA"
                    },
                    new UpgradeLevel()
                    {
                        icon="SlowSpray2",
                        cost=500,
                        descLoca="Upg_SlowBSODA"
                    },
                    new UpgradeLevel()
                    {
                        icon="SlowSpray3",
                        cost=675,
                        descLoca="Upg_SlowBSODA"
                    },
                    new UpgradeLevel()
                    {
                        icon="SlowSpray4",
                        cost=1225,
                        descLoca="Upg_SlowBSODA"
                    },
                    new UpgradeLevel()
                    {
                        icon="SlowSpray5",
                        cost=1025,
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
                        cost=400,
                        descLoca="Upg_Stamina"
                    },
                    new UpgradeLevel()
                    {
                        icon="Stamina2",
                        cost=650,
                        descLoca="Upg_Stamina"
                    },
                    new UpgradeLevel()
                    {
                        icon="Stamina3",
                        cost=900,
                        descLoca="Upg_Stamina"
                    },
                    new UpgradeLevel()
                    {
                        icon="Stamina4",
                        cost=1150,
                        descLoca="Upg_Stamina"
                    }
                }
            });
            // hungry bully
            Register(new StandardUpgrade()
            {
                id = "hungrybully",
                weight = 78,
                levels = new UpgradeLevel[]
                {
                    new UpgradeLevel()
                    {
                        icon="HungryBully",
                        cost=500,
                        descLoca="Upg_HungryBully"
                    }
                }
            });
            // item slots
            Register(new SlotUpgrade()
            {
                id = "slots",
                weight = 60,
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
                        cost=50,
                        descLoca="Upg_ItemSlot"
                    },
                    new UpgradeLevel()
                    {
                        icon="SlotPlus",
                        cost=150,
                        descLoca="Upg_ItemSlot"
                    },
                    new UpgradeLevel()
                    {
                        icon="SlotPlus",
                        cost=250,
                        descLoca="Upg_ItemSlot"
                    },
                    new UpgradeLevel()
                    {
                        icon="SlotPlus",
                        cost=350,
                        descLoca="Upg_ItemSlot"
                    }
                }
            });
        }
    }
}