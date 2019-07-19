using Forgetive.Server.Maps;
using Forgetive.Server.Math;
using System;
using System.Collections.Generic;

namespace Forgetive.Server.Items
{
    /// <summary>
    /// 定义物品用法
    /// </summary>
    public static class ItemUsage
    {
        static List<KeyValuePair<int, Action<Item, Player>>> Usage;

        internal static void Init()
        {
            Usage = new List<KeyValuePair<int, Action<Item, Player>>>();

            AddUsage(104, (item, user) =>
            {
                if (user.DeleteItem(item.ServerId))
                    user.UpHealth(float.Parse(item.ExtraInfo));
            });

            AddUsage(105, (item, user) =>
            {
                if (user.DeleteItem(item.ServerId))
                    user.Damage(float.Parse(item.ExtraInfo));
            });

            AddUsage(106, (item, user) =>
            {
                if (user.MapContext == null) return;
                if (user.DeleteItem(item.ServerId))
                {
                    string[] vec2ls = item.ExtraInfo.Split(';');
                    Vector3 location = new Vector3(vec2ls[0]);
                    Vector3 EulerAngle = new Vector3(vec2ls[1]);
                    user.MapContext.ThrowItems.Create(106, location, EulerAngle);
                }
            });

            AddUsage(107, (item, user) => DoFireworkEvent(item, user));
            AddUsage(108, (item, user) => DoFireworkEvent(item, user));
            AddUsage(109, (item, user) => DoFireworkEvent(item, user));
            AddUsage(110, (item, user) => DoFireworkEvent(item, user));

            AddUsage(111, (item, user) =>
            {

            });

            AddUsage(117, (item, user) => user.DeleteItem(item.ServerId));
        }

        public static bool CanPlayFirework(MapBase map)
        {
            return map.blocks.Count < map.MaxBlockCount;
        }

        public static void DoFireworkEvent(Item item, Player user)
        {
            if (user.MapContext == null) return;
            if (!CanPlayFirework(user.MapContext))
            {
                user.SendChat("放花也要排队，当前地图烟花正在燃放数量已经达到上限。", "Forgetive");
                return;
            }
            if (user.DeleteItem(item.ServerId))
            {
                int id = item.ServerId;
                Vector3 location = new Vector3(item.ExtraInfo);
                Block block = new Block();
                block.blockId = "FireworkID" + id;
                block.controllerName = user.Name;
                block.updateLocation = location;
                block.updateRotation = new Vector3(0f, 0f, 0f);
                block.blockType = item.ItemStaticId;
                user.MapContext.blocks.Add(block);
                List<string> data = new List<string>();
                data.Add("updateBlock");
                data.Add(block.blockId);
                data.Add(block.updateLocation.ToXYZ());
                data.Add(block.updateRotation.ToXYZ());
                data.Add(block.blockType.ToString());
                user.MapContext.SendAll(data);
                List<string> self = new List<string>();
                self.Add("updateBlockController");
                self.Add(block.blockId);
                self.Add(bool.TrueString);
                user.SendDataPackage(self);
            }
        }

        public static bool AddUsage(int staticId, Action<Item, Player> actn)
        {
            for (int i = 0; i < Usage.Count; i++)
            {
                if (Usage[i].Key == staticId)
                {
                    return false;
                }
            }
            Usage.Add(new KeyValuePair<int, Action<Item, Player>>(staticId, actn));
            return true;
        }

        internal static bool Invoke(int serverId, Player user)
        {
            if (user.Owned(serverId))
            {
                Item item = ItemStorage.GetItemDesc(serverId);
                for (int i = 0; i < Usage.Count; i++)
                {
                    if (Usage[i].Key == item.ItemStaticId)
                    {
                        Usage[i].Value(item, user);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
