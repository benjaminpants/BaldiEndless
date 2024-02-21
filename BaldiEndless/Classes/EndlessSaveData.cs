using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BaldiEndless
{
    public struct UpgradeData
    {
        public string id { private set; get; } //to ensure people have to create a new upgradedata if they want to change the id
        public byte count;

        public UpgradeData(string id, byte count)
        {
            this.id = id;
            this.count = count;
        }
    }


    public class EndlessSaveData
    {
        public FloorData myFloorData = new FloorData();

        public UpgradeData[] Upgrades = new UpgradeData[5] { new UpgradeData("none", 0), new UpgradeData("none", 0), new UpgradeData("none", 0), new UpgradeData("none", 0), new UpgradeData("none", 0) };

        public int currentFloor {
            get
            {
                return myFloorData.FloorID;
            }
            set
            {
                myFloorData.FloorID = value;
            }
        }

        public void Save(ref BinaryWriter writer)
        {
            writer.Write((byte)0); //format version
            writer.Write(currentFloor);
            writer.Write((byte)Upgrades.Length);
            for (int i = 0; i < Upgrades.Length; i++)
            {
                writer.Write(Upgrades[i].id);
                writer.Write(Upgrades[i].count);
            }
        }
    }
}
