using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.Registers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BaldiEndless
{
    [HarmonyPatch(typeof(ItemManager))]
    [HarmonyPatch("RemoveItem")]
    class RemoveItemPatch
    {
        static float[] Percentages => new float[] { 0f, 1f, 2f, 3f, 5f, 6f, 10f };
        static void Postfix(ItemManager __instance, int val)
        {
            if (EndlessFloorsPlugin.currentSave.HasUpgrade("bank"))
            {
                if (Percentages[EndlessFloorsPlugin.currentSave.GetUpgradeCount("bank")] >= UnityEngine.Random.Range(0f, 100f))
                {
                    __instance.SetItem(MTM101BaldiDevAPI.itemMetadata.FindByEnum(Items.Quarter).value, val);
                }
            }
        }
    }

    [HarmonyPatch(typeof(WaterFountain))]
    [HarmonyPatch("Clicked")]
    class FountainIncreasePatch
    {
        static void Postfix(WaterFountain __instance, int playerNumber)
        {
            float maxStam = Singleton<CoreGameManager>.Instance.GetPlayer(playerNumber).plm.staminaMax;
            if (Singleton<CoreGameManager>.Instance.GetPlayer(playerNumber).plm.stamina != maxStam) return;
            Singleton<CoreGameManager>.Instance.GetPlayer(playerNumber).plm.stamina = maxStam + (EndlessFloorsPlugin.currentSave.GetUpgradeCount("drink") * 10);
        }
    }

    [HarmonyPatch(typeof(ITM_BSODA))]
    [HarmonyPatch("Use")]
    class SlowBSODAPatch
    {
        static void Prefix(ITM_BSODA __instance, ref float ___speed)
        {
            ___speed *= 1f - (EndlessFloorsPlugin.currentSave.GetUpgradeCount("slowsoda") * 0.15f);
        }
    }

    [HarmonyPatch(typeof(ITM_AlarmClock))]
    [HarmonyPatch("Use")]
    class AlarmClockPatch
    {
        static FieldInfo _finished = AccessTools.Field(typeof(ITM_AlarmClock), "finished");
        static FieldInfo _audMan = AccessTools.Field(typeof(EnvironmentController), "audMan");
        static MethodInfo _Timer = AccessTools.Method(typeof(ITM_AlarmClock), "Timer");
        static bool Prefix(ITM_AlarmClock __instance, ref EnvironmentController ___ec, PlayerManager pm, Entity ___entity, float[] ___setTime, int ___initSetTime, ref bool __result)
        {
            if (!EndlessFloorsPlugin.currentSave.HasUpgrade("timeclock")) return true;
            // i hate doing this.
            __instance.StartCoroutine(WaitForCompletion(__instance));
            ___ec = pm.ec;
            __instance.transform.position = pm.transform.position;
            ___entity.Initialize(___ec, __instance.transform.position);
            __instance.StartCoroutine("Timer", ___setTime[___initSetTime]);
            __result = true;
            return false;
        }

        static IEnumerator WaitForCompletion(ITM_AlarmClock clock)
        {
            while (!(bool)_finished.GetValue(clock))
            {
                yield return null;
            }
            clock.StopCoroutine("Timer"); //dont die until WE are ready!
            TimeScaleModifier timeMod = new TimeScaleModifier() 
            {
                environmentTimeScale = 0f,
                npcTimeScale = 0f
            };
            AudioManager audMan = (AudioManager)_audMan.GetValue(Singleton<BaseGameManager>.Instance.Ec);
            audMan.PlaySingle((SoundObject)EndlessFloorsPlugin.Instance.assetManager[typeof(SoundObject), "TimeSlow"]);
            Singleton<BaseGameManager>.Instance.Ec.AddTimeScale(timeMod);
            float timeToWait = (EndlessFloorsPlugin.currentSave.GetUpgradeCount("timeclock") * 10f);
            while (timeToWait > 0f)
            {
                timeToWait -= Time.deltaTime;
                yield return null;
            }
            Singleton<BaseGameManager>.Instance.Ec.RemoveTimeScale(timeMod);
            audMan.PlaySingle((SoundObject)EndlessFloorsPlugin.Instance.assetManager[typeof(SoundObject), "TimeFast"]);
            UnityEngine.Object.Destroy(clock.gameObject);
            yield break;
        }
    }

    [HarmonyPatch(typeof(ITM_Boots))]
    [HarmonyPatch("Use")]
    class BootsPatch
    {
        static void Postfix(ITM_Boots __instance, PlayerManager pm, float ___setTime)
        {
            if (EndlessFloorsPlugin.currentSave.HasUpgrade("speedyboots"))
            {
                __instance.StartCoroutine(BootsNumerator(pm, ___setTime));
            }
        }

        static IEnumerator BootsNumerator(PlayerManager pm, float startTime)
        {
            float time = startTime;
            BootsSpeedManager myChecker = pm.gameObject.GetComponent<BootsSpeedManager>();
            if (myChecker == null)
            {
                myChecker = pm.gameObject.AddComponent<BootsSpeedManager>();
            }
            myChecker.AddBoot();
            while (time > 0f)
            {
                time -= Time.deltaTime * pm.PlayerTimeScale;
                yield return null;
            }
            myChecker.RemoveBoot();
            yield break;
        }

        class BootsSpeedManager : MonoBehaviour
        {
            private int bootsActive = 0;
            MovementModifier speedMod = new MovementModifier(Vector3.zero, 1f + EndlessFloorsPlugin.currentSave.GetUpgradeCount("speedyboots") * 0.15f);

            void Start()
            {
                this.GetComponent<Entity>().ExternalActivity.moveMods.Add(speedMod);
            }

            public void AddBoot()
            {
                bootsActive++;
            }

            public void RemoveBoot()
            {
                bootsActive--;
                if (bootsActive <= 0)
                {
                    this.GetComponent<Entity>().ExternalActivity.moveMods.Remove(speedMod);
                    Destroy(this);
                }
            }
        }
    }


    [HarmonyPatch(typeof(ITM_PrincipalWhistle))]
    [HarmonyPatch("Use")]
    class WhistlePatch
    {
        private static MethodInfo _setGuilt = AccessTools.Method(typeof(NPC), "SetGuilt");
        static void Prefix(ITM_PrincipalWhistle __instance, PlayerManager pm)
        {
            if (!EndlessFloorsPlugin.currentSave.HasUpgrade("favor")) return;
            List<NPC> elligableNPCs = new List<NPC>();
            foreach (NPC npc in pm.ec.Npcs)
            {
                if (Vector3.Distance(npc.transform.position,pm.transform.position) <= pm.pc.reach * 5)
                {
                    if (!elligableNPCs.Contains(npc))
                    {
                        elligableNPCs.Add(npc);
                    }
                }
            }
            RaycastHit hit;
            LayerMask clickMask = new LayerMask() {value=131073}; //copied from ITM_Scissors
            if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out hit, pm.pc.reach*7, clickMask))
            {
                NPC hitNPC = hit.transform.GetComponent<NPC>();
                if (hitNPC)
                {
                    if (!elligableNPCs.Contains(hitNPC))
                    {
                        elligableNPCs.Add(hitNPC);
                    }
                }
            }
            elligableNPCs.Do(x =>
            {
                if (x.Character == Character.Principal) return;
                if (x.Character == Character.Chalkles) return;
                if (!x.GetMeta().flags.HasFlag(NPCFlags.HasTrigger)) return;
                _setGuilt.Invoke(x, new object[] { 10f, "Bullying" });
            });
        }
    }
}
