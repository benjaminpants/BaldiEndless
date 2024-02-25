using HarmonyLib;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace BaldiEndless
{

    [HarmonyPatch(typeof(NoLateTeacher))]
    [HarmonyPatch("PlayerCaught")]
    class PompIncreasedTime
    {
        static void Prefix(NoLateTeacher __instance, ref float ___classTime, ref int ___successPoints)
        {
            float timeIncrease = Mathf.Floor((EndlessFloorsPlugin.currentFloorData.classRoomCount * 5f) / 60f)*60f;
            ___classTime = Mathf.Min(120f + (timeIncrease), 540f);
            //___successPoints = 100 - (EndlessFloorsPlugin.currentSave.pompIncreaseMinutes * 15);
        }
    }

    [HarmonyPatch(typeof(Bully))]
    [HarmonyPatch("StealItem")]
    class BullyStealPatch
    {

        static FieldInfo slotsToSteal = AccessTools.Field(typeof(Bully), "slotsToSteal");
        static void CheckForHungry(Bully instance, PlayerManager pm)
        {
            if (!EndlessFloorsPlugin.currentSave.HasUpgrade("hungrybully")) return;
            List<int> toSteal = (List<int>)slotsToSteal.GetValue(instance);
            for (int i = 0; i < toSteal.Count; i++)
            {
                if (pm.itm.items[toSteal[i]].itemType == Items.ZestyBar)
                {
                    slotsToSteal.SetValue(instance, new List<int>()
                    {
                        toSteal[i]
                    });
                    return;
                }
            }
            for (int i = 0; i < toSteal.Count; i++)
            {
                if (pm.itm.items[toSteal[i]].itemType == Items.Bsoda)
                {
                    slotsToSteal.SetValue(instance, new List<int>()
                    {
                        toSteal[i]
                    });
                    return;
                }
            }
            for (int i = 0; i < toSteal.Count; i++)
            {
                if (pm.itm.items[toSteal[i]].itemType == Items.Apple)
                {
                    slotsToSteal.SetValue(instance, new List<int>()
                    {
                        toSteal[i]
                    });
                    return;
                }
            }
            for (int i = 0; i < toSteal.Count; i++)
            {
                if (pm.itm.items[toSteal[i]].GetMeta().tags.Contains("food"))
                {
                    slotsToSteal.SetValue(instance, new List<int>()
                    {
                        toSteal[i]
                    });
                    return;
                }
            }
        }

        static MethodInfo checkF = AccessTools.Method(typeof(BullyStealPatch), "CheckForHungry");
        // stolen from raldi's crackhouse plus, a mod that will never see the light of day because the raldi team sucks
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) // insert calling CheckForEnergy right after the toSteal for loop
        {
            bool didFirstFor = false;
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Blt_S && !didFirstFor) //end of the for loop
                {
                    didFirstFor = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0); //this
                    yield return new CodeInstruction(OpCodes.Ldarg_1); //pm
                    yield return new CodeInstruction(OpCodes.Call, checkF); //BullyStealPatch.CheckForEnergy
                }
            }
            yield break;
        }
    }
}
