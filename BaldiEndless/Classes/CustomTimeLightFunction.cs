using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BaldiEndless
{
    public class CustomTimeLightFunction : RoomFunction
    {
        public override void Initialize(RoomController room)
        {
            base.Initialize(room);
            string mapName = Singleton<CoreGameManager>.Instance.sceneObject.skybox.name;
            Color col = Color.white;
            switch (mapName)
            {
                case "Cubemap_Twilight":
                    col = new Color(239f/255f, 188f/255f, 162f/255f);
                    break;
                case "Cubemap_DarkSky":
                    col = new Color(46f / 255f, 52f / 255f, 68f / 255f);
                    break;
            }
            for (int i = 0; i < room.TileCount; i++)
            {
                room.TileAtIndex(i).permanentLight = true;
                room.ec.GenerateLight(room.TileAtIndex(i), col, 1);
            }
        }
    }
}
