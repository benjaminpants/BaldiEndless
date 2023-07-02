using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BaldiEndless
{
    [HarmonyPatch(typeof(Bully))]
    [HarmonyPatch("StealItem")]
    class BullyPreferCandy
    {
        static MethodInfo hide = AccessTools.Method(typeof(Bully), "Hide");
        static bool Prefix(Bully __instance, PlayerManager pm, ref List<int> ___slotsToSteal, ref List<Items> ___itemsToReject, ref AudioManager ___audMan, ref SoundObject[] ___takeouts, ref SoundObject ___noItems)
        {
            if (EndlessFloorsPlugin.currentSave.HasUpgrade(typeof(BullyPreferCandy)))
            {
                ___slotsToSteal.Clear();
                for (int i = 0; i < 6; i++)
                {
                    if (!___itemsToReject.Contains(pm.itm.items[i].itemType))
                    {
                        ___slotsToSteal.Add(i);
                    }
                }
                List<int> zestyOnly = ___slotsToSteal.Where(x => pm.itm.items[x].itemType == Items.ZestyBar).ToList();
                List<int> sodaOnly = ___slotsToSteal.Where(x => pm.itm.items[x].itemType == Items.Bsoda).ToList();
                List<int> appleOnly = ___slotsToSteal.Where(x => pm.itm.items[x].itemType == Items.Apple).ToList();
                if (zestyOnly.Count > 0)
                {
                    ___slotsToSteal = zestyOnly;
                }
                else if (sodaOnly.Count > 0)
                {
                    ___slotsToSteal = sodaOnly;
                }
                else if (appleOnly.Count > 0)
                {
                    ___slotsToSteal = appleOnly;
                }
                if (___slotsToSteal.Count > 0)
                {
                    pm.itm.RemoveItem(___slotsToSteal[UnityEngine.Random.Range(0, ___slotsToSteal.Count)]);
                    hide.Invoke(__instance, null);
                    ___audMan.PlaySingle(___takeouts[UnityEngine.Random.Range(0, ___takeouts.Length)]);
                    return false;
                }
                ___audMan.PlaySingle(___noItems);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(NoLateTeacher))]
    [HarmonyPatch("OnTriggerEnter")]
    class PompIncreasedTime
    {
        static void Prefix(NoLateTeacher __instance, ref float ___classTime, ref int ___successPoints)
        {
            ___classTime = 120f + (EndlessFloorsPlugin.currentSave.pompIncreaseMinutes * 60f);
            ___successPoints = 100 - (EndlessFloorsPlugin.currentSave.pompIncreaseMinutes * 15);
        }
    }

    [HarmonyPatch(typeof(Principal))]
    [HarmonyPatch("WhistleReact")]
    class PrincipalWhistleSpeedy
    {
        static void Prefix(Principal __instance, ref float ___whistleSpeed)
        {
            if (EndlessFloorsPlugin.currentSave.HasUpgrade(typeof(HyperWhistle)))
            {
                ___whistleSpeed *= 3f; //technically this makes it stack everytime you use the whistle but its so fast... no one should notice?
            }
        }
    }

    [HarmonyPatch(typeof(CoreGameManager))]
    [HarmonyPatch("EndGame")]
    class BaldiKill
    {
        static void Prefix()
        {

            if (!Singleton<CoreGameManager>.Instance.tripPlayed) return;
            if (EndlessFloorsPlugin.currentSave.RemoveOneUpgrade(typeof(FieldtripRedo)))
            {
                Singleton<CoreGameManager>.Instance.tripPlayed = false;
            }
        }
    }
}
