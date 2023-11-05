using System.Collections;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace BaldiEndless
{
    public static class BepInUtils
    {
        //Thanks Fasguy
        public static T GetValue<T>(this FieldInfo fieldInfo, object obj)
        {
            return (T)fieldInfo.GetValue(obj);
        }
    }
}