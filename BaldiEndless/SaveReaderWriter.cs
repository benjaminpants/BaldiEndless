using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;
using MTM101BaldAPI.SaveSystem;

namespace BaldiEndless
{
    internal class EndlessFloorsSaveHandler : ModdedSaveGameIOBinary
    {
        public override PluginInfo pluginInfo => EndlessFloorsPlugin.Instance.Info;

        public override void Load(BinaryReader reader)
        {
            EndlessFloorsPlugin.mainSave = EndlessSaveData.Load(reader);
        }

        public override void Reset()
        {
            EndlessFloorsPlugin.mainSave = new EndlessSaveData();
        }

        public override void Save(BinaryWriter writer)
        {
            EndlessFloorsPlugin.mainSave.Save(writer);
        }
    }
}
