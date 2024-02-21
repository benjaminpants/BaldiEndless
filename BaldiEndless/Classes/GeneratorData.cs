using System;
using System.Collections.Generic;
using System.Text;

namespace BaldiEndless
{
    public class GeneratorData
    {
        public List<WeightedRandomEvent> randomEvents = new List<WeightedRandomEvent>();
        public List<WeightedNPC> npcs = new List<WeightedNPC>();
        public List<NPC> forcedNpcs = new List<NPC>();
        public List<WeightedItem> items = new List<WeightedItem>();
        public List<WeightedObjectBuilder> objectBuilders = new List<WeightedObjectBuilder>();
        public List<WeightedObjectBuilder> randomObjectBuilders = new List<WeightedObjectBuilder>();
        public Dictionary<RoomCategory, List<WeightedRoomAsset>> roomAssets = new Dictionary<RoomCategory, List<WeightedRoomAsset>>();
        public List<WeightedRoomAsset> classRoomAssets => roomAssets[RoomCategory.Class];
        public List<WeightedRoomAsset> facultyRoomAssets => roomAssets[RoomCategory.Faculty];
    }
}
