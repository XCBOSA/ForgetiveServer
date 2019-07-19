using Forgetive.Server.Math;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Forgetive.Server.Maps
{
    [Serializable]
    public class Block
    {
        static Timer timer;
        DateTime creationTime;
        static readonly int[] FireworkIds = { 107, 108, 109, 110 };
        const int FireworkKeepSeconds = 35;

        static void StaticUpdate()
        {
            for (int i = 0; i < GlobalServer.Maps.Count; i++)
            {
                MapBase currentMap = GlobalServer.Maps[i];
                for (int j = 0; j < currentMap.blocks.Count; j++)
                {
                    Block currentBlock = currentMap.blocks[j];
                    if (currentBlock.IsFirework())
                    {
                        if (DateTime.Now - currentBlock.creationTime >= new TimeSpan(0, 0, FireworkKeepSeconds))
                        {
                            List<string> data = new List<string>();
                            data.Add("removeBlock");
                            data.Add(currentBlock.blockId);
                            currentMap.blocks.RemoveAt(j);
                            j--;
                            currentMap.SendAll(data);
                        }
                    }
                }
            }
        }

        public Block()
        {
            creationTime = DateTime.Now;
            if (timer == null)
            {
                timer = new Timer();
                timer.Interval = 500d;
                timer.AutoReset = true;
                timer.Elapsed += (a, b) => StaticUpdate();
                timer.Enabled = true;
            }
        }

        public string blockId;
        public int blockType;
        public Vector3 updateLocation;
        public Vector3 updateRotation;
        public string controllerName;

        public bool IsFirework()
        {
            for (int i = 0; i < FireworkIds.Length; i++)
            {
                if (FireworkIds[i] == blockType)
                    return true;
            }
            return false;
        }
    }

    [Serializable]
    public class List_Block
    {
        public Block[] blocks;
    }
}
