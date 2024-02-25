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
        public List<WeightedItemObject> items = new List<WeightedItemObject>();
        public List<WeightedObjectBuilder> objectBuilders = new List<WeightedObjectBuilder>();
        public List<ObjectBuilder> forcedObjectBuilders = new List<ObjectBuilder>();
        public Dictionary<RoomCategory, List<WeightedRoomAsset>> roomAssets = new Dictionary<RoomCategory, List<WeightedRoomAsset>>();
        public List<WeightedRoomAsset> classRoomAssets => roomAssets[RoomCategory.Class];
        public List<WeightedRoomAsset> facultyRoomAssets => roomAssets[RoomCategory.Faculty];
        public List<WeightedRoomAsset> specialRoomAssets => roomAssets[RoomCategory.Special];
        public List<WeightedRoomAsset> officeRoomAssets => roomAssets[RoomCategory.Office];
        public List<WeightedRoomAsset> hallInsertions => roomAssets[RoomCategory.Hall];
        public List<WeightedFieldTrip> fieldTrips = new List<WeightedFieldTrip>();

        public GeneratorData()
        {
            roomAssets.Add(RoomCategory.Class, new List<WeightedRoomAsset>());
            roomAssets.Add(RoomCategory.Faculty, new List<WeightedRoomAsset>());
            roomAssets.Add(RoomCategory.Hall, new List<WeightedRoomAsset>());
            roomAssets.Add(RoomCategory.Office, new List<WeightedRoomAsset>());
            roomAssets.Add(RoomCategory.Special, new List<WeightedRoomAsset>());
        }
    }
}
