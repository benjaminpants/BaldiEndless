using System;
using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using UnityEngine;
using MTM101BaldAPI.AssetManager;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MTM101BaldAPI.SaveSystem;
using BepInEx.Bootstrap;
using System.Reflection;
using UnityEngine.Rendering;
using System.Collections;
using UnityEngine.Events;

namespace BaldiEndless
{
    [HarmonyPatch(typeof(BaseGameManager))]
    [HarmonyPatch("AllNotebooks")]
    static class AllNotebooksMusic
    {
        private static UnityAction<string, MidiPlayerTK.EventEndMidiEnum> action;

        static void Prefix(BaseGameManager __instance, bool ___allNotebooksFound)
        {
            if (!___allNotebooksFound)
            {
                if ((EndlessFloorsPlugin.currentFloorData.FloorID % 99) == 0)
                {
                    //this is how we do the floor 99 music intro properly
                    Singleton<MusicManager>.Instance.PlayMidi(EndlessFloorsPlugin.F99MusicStart, false);
                    action = null;
                    action += Action;
                    Singleton<MusicManager>.Instance.MidiPlayer.OnEventEndPlayMidi.AddListener(action);
                }
                else
                {
                    Singleton<MusicManager>.Instance.PlayMidi("Level_1_End", true);
                }
            }
        }

        static void Action(string st, MidiPlayerTK.EventEndMidiEnum num)
        {
            // make sure the midi actually completed before starting the loop
            if (num == MidiPlayerTK.EventEndMidiEnum.MidiEnd)
            {
                float oldSpeed = Singleton<MusicManager>.Instance.MidiPlayer.MPTK_Speed;
                Singleton<MusicManager>.Instance.PlayMidi(EndlessFloorsPlugin.F99MusicLoop, true);
                Singleton<MusicManager>.Instance.SetSpeed(oldSpeed);
                action -= Action;
            }
        }
    }

    [HarmonyPatch(typeof(BaseGameManager))]
    [HarmonyPatch("ElevatorClosed")]
    static class MusicSlowElevatorShut
    {
        static void Prefix(BaseGameManager __instance, int ___elevatorsToClose)
        {
            if (((EndlessFloorsPlugin.currentFloorData.FloorID % 99) == 0) && (___elevatorsToClose > 0))
            {
                Singleton<MusicManager>.Instance.SetSpeed(Singleton<MusicManager>.Instance.MidiPlayer.MPTK_Speed + 0.035f);
            }
            else
            {
                Singleton<MusicManager>.Instance.SetSpeed(0.12f);
                Singleton<MusicManager>.Instance.SetLoop(false);
            }
        }
    }

    [HarmonyPatch(typeof(BaseGameManager))]
    [HarmonyPatch("BeginSpoopMode")]
    static class AllNotebooksMusicSpoop
    {
        static void Prefix(BaseGameManager __instance, bool ___allNotebooksFound)
        {
            if (___allNotebooksFound)
            {
                Singleton<MusicManager>.Instance.PlayMidi("Level_1_End", true);
            }
        }
    }
}
