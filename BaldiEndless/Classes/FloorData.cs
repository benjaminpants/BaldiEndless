using System;
using UnityEngine;

namespace BaldiEndless
{
    [Serializable]
    public class FloorData
    {
        public int FloorID = 1;

        public static int GetYTPsAtFloor(int floor)
        {
            FloorData tempD = new FloorData();
            int YTPs = 0;
            tempD.FloorID = 1;
            for (int i = 0; i < floor; i++)
            {
                YTPs += (Mathf.Min(tempD.classRoomCount,50) * 10);
                YTPs += Mathf.FloorToInt(tempD.unclampedScaleVar * 2);
                tempD.FloorID++;
            }
            YTPs *= 6;
            return YTPs;
        }

        public int CapIncrease
        {
            get 
            {
                return Mathf.FloorToInt(FloorID / 99) * 5;
            }
        }

        public float scaleVar
        {
            get
            {
                int mainV = Mathf.Min(FloorID, 42 + CapIncrease);
                return mainV * (mainV / 8f);
            }
        }

        public float unclampedScaleVar
        {
            get
            {
                return FloorID * (FloorID / 8f);
            }
        }

        public float facultyStickToHall
        {
            get
            {
                return Mathf.Clamp(unclampedScaleVar / 16f, 0.05f, 0.95f);
            }
        }

        public int maxItemValue
        {
            get
            {
                return Mathf.FloorToInt(maxFacultyRoomCount * maxFacultyRoomCount) + (25 * Mathf.CeilToInt(FloorID/2f));
            }
        }

        public int classRoomCount
        {
            get
            {
                return Mathf.CeilToInt(Mathf.Max(Mathf.Sqrt(unclampedScaleVar * 3f),3f));
            }
        }

        public int maxPlots
        {
            get
            {
                return Mathf.Clamp(classRoomCount / 2, 4, 12);
            }
        }

        public int npcCountUnclamped
        {
            get
            {
                return Mathf.FloorToInt((FloorID / 3f) - 1);
            }
        }
        public int minPlots
        {
            get
            {
                return Mathf.Max(Mathf.CeilToInt(maxPlots * 0.7f),4);
            }
        }

        public int maxGiantRooms
        {
            get
            {
                return Mathf.Max(Mathf.FloorToInt(maxSize / 30f), minSize >= 37 ? 1 : 0);
            }
        }

        public int maxOffices
        {
            get
            {
                return Mathf.RoundToInt(maxSize / 22f);
            }
        }

        public int minGiantRooms
        {
            get
            {
                return Mathf.Max(maxGiantRooms - 1, maxSize > 43 ? 1 : 0);
            }
        }

        public int maxSpecialHalls
        {
            get
            {
                return Mathf.FloorToInt(maxSize / 28f);
            }
        }

        public int minSpecialHalls
        {
            get
            {
                return Mathf.FloorToInt(maxSpecialHalls / 3f);
            }
        }

        public float itemChance
        {
            get
            {
                return (scaleVar / 3f) + 2f;
            }
        }

        public int maxFacultyRoomCount
        {
            get
            {
                float subCount = Mathf.Clamp(32 - FloorID, 24f, 32f);
                return Mathf.Max(Mathf.CeilToInt((maxSize * 1.2f) - subCount), 3);
            }
        }

        public int minFacultyRoomCount
        {
            get
            {
                return Mathf.CeilToInt(maxFacultyRoomCount * 0.88f);
            }
        }

        public int exitCount
        {
            get
            {
                return (FloorID % 99 == 0) ? 16 : Mathf.Clamp(Mathf.CeilToInt(FloorID / 9) + 1,1,8);
            }
        }

        public int maxSize
        {
            get
            {
                float addend = Mathf.Clamp(FloorID * 8f, 16f, 24f);
                return Mathf.Max(Mathf.CeilToInt((scaleVar / 7f) + addend), 19);
            }
        }

        public int myFloorBaldi
        {
            get
            {
                int mfb = 1;

                if (classRoomCount > 8)
                {
                    mfb = 3;
                }
                else if (classRoomCount > 5)
                {
                    mfb = 2;
                }
                return mfb;
            }
        }

        public int minSize
        {
            get
            {
                return Mathf.Clamp(Mathf.CeilToInt(maxSize / 1.24f),19, maxSize);
            }
        }
    }
}
