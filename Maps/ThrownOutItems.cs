using Forgetive.Server.Math;
using System.Collections.Generic;
using Forgetive.Server.Items;

namespace Forgetive.Server.Maps
{
    public class ThrownOutItem
    {
        public int ItemServerId;
        public Vector3 Location;

        public ThrownOutItem(int item, Vector3 location)
        {
            ItemServerId = item;
            Location = location;
        }
    }

    public class ThrownOutItems
    {
        List<ThrownOutItem> Items;
        MapBase Map;

        public ThrownOutItems(MapBase map)
        {
            Items = new List<ThrownOutItem>();
            Map = map;
        }

        public void ThrowNewObject(int stdId, string desc, string ext, Vector3 location)
        {
            int svrId = ItemStorage.NewItem(stdId, desc, ext);
            ThrownOutItem t = new ThrownOutItem(svrId, location);
            Items.Add(t);
            SendAddMessage(t);
        }

        public void ThrowFromPlayer(int serverId, Vector3 location, Player player)
        {
            if (player.DeleteItem(serverId))
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].ItemServerId == serverId)
                    {
                        Logger.WriteLine(LogLevel.Info, "玩家{0}尝试扔出已经扔出的物品ID={1}", player.Name, serverId);
                        return;
                    }
                }
                ThrownOutItem t = new ThrownOutItem(serverId, location);
                Items.Add(t);
                SendAddMessage(t);
                Logger.WriteLine(LogLevel.Info, "玩家{0}在位置{1}扔出了物品ID={2}", player.Name, location, serverId);
            }
        }

        public void ChangeItemLocation(int serverId, Vector3 lctn)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].ItemServerId == serverId)
                {
                    Items[i].Location = lctn;
                }
            }
        }

        public void PickUpFromPlayer(Player player, int serverId)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].ItemServerId == serverId)
                {
                    if (player.PickUpItem(serverId))
                    {
                        Items.RemoveAt(i);
                        Logger.WriteLine(LogLevel.Info, "玩家{0}在位置{1}拾取了物品ID={2}",
                            player.Name, player.Location, serverId);
                        SendRemoveMessage(serverId);
                    }
                    return;
                }
            }
        }

        public void SendAddMessage(ThrownOutItem item)
        {
            List<string> adData = new List<string>();
            Item itemDesc = ItemStorage.GetItemDesc(item.ItemServerId);
            adData.Add("addThrownItem");
            adData.Add(itemDesc.ItemStaticId.ToString());
            adData.Add(itemDesc.ServerId.ToString());
            adData.Add(ForgetiveServer.Base64Encode(itemDesc.DescribeMessage));
            adData.Add(ForgetiveServer.Base64Encode(itemDesc.ExtraInfo));
            adData.Add(item.Location.ToXYZ());
            for (int i = 0; i < Map.players.Count; i++)
            {
                try
                {
                    Map.players[i].SendDataPackage(adData);
                }
                catch { }
            }
        }

        public void SendRemoveMessage(int serverId)
        {
            List<string> rmData = new List<string>();
            rmData.Add("removeThrownItem");
            rmData.Add(serverId.ToString());
            for (int i = 0; i < Map.players.Count; i++)
            {
                try
                {
                    Map.players[i].SendDataPackage(rmData);
                }
                catch { }
            }
        }

        public void SendToNewPlayer(Player player)
        {
            List<string> data = new List<string>();
            data.Add("updateThrownItems");
            for (int i = 0; i < Items.Count; i++)
            {
                int index = Items[i].ItemServerId;
                Item itemdesc = ItemStorage.GetItemDesc(index);
                if (itemdesc == null)
                    continue;
                data.Add(itemdesc.ItemStaticId.ToString());
                data.Add(itemdesc.ServerId.ToString());
                data.Add(ForgetiveServer.Base64Encode(itemdesc.DescribeMessage));
                data.Add(ForgetiveServer.Base64Encode(itemdesc.ExtraInfo));
                data.Add(Items[i].Location.ToXYZ());
                data.Add("next");
                player.SendDataPackage(data);
            }
        }
    }
}
