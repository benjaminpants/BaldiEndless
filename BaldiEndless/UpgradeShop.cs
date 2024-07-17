using HarmonyLib;
using MTM101BaldAPI;
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

        public List<string> Upgrades = new List<string>();

        public bool[] Purchased = new bool[6];

        public bool alwaysAddReroll = false;

        public void Awake()
        {
            Instance = this;
        }

        public StandardUpgrade GetUpgrade(int index)
        {
            return EndlessFloorsPlugin.Upgrades[Upgrades[index]];
        }

        public void PopulateShop()
        {
            try
            {
                int randRang = alwaysAddReroll ? 5 : UnityEngine.Random.Range(2, 6);
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
                        Upgrades.Add("error");
                        continue;
                    }
                    StandardUpgrade gu = WeightedSelection<StandardUpgrade>.RandomSelection(weightedUpgrades);
                    Upgrades.Add(gu.id);
                    if (!gu.ShouldAppear(EndlessFloorsPlugin.currentSave.GetUpgradeCount(gu.id) + Upgrades.Where(x => x == gu.id).Count()))
                    {
                        weightedUpgrades = weightedUpgrades.Where(x => x.selection.id != gu.id).ToArray();
                    }
                }
                if (alwaysAddReroll)
                {
                    Upgrades.Add("reroll");
                }
            }
            catch(Exception ex)
            {
                MTM101BaldiDevAPI.CauseCrash(EndlessFloorsPlugin.Instance.Info, ex);
            }
        }
    }


}
