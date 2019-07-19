using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forgetive.Server.Items
{
    [Serializable]
    public class Item
    {
        public int ItemStaticId;
        public int ServerId;
        public string DescribeMessage;
        public string ExtraInfo;
    }

    [Serializable]
    public class ItemPage
    {
        public Item[] Items;
    }

    [Serializable]
    public class PlayerOwnServerId
    {
        public int[] Items;
        public int[] DockItems;
    }
}
