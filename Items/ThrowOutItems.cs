using Forgetive.Server.Maps;
using Forgetive.Server.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forgetive.Server.Items
{
    public class ThrowOutItems
    {
        List<ThrowOutItemMetaInfo> Items;
        MapBase map;
        int svrIdMax = 0;

        public static Dictionary<int, int> ItemStaticIdWithEffectId;

        internal ThrowOutItems(MapBase themap)
        {
            Items = new List<ThrowOutItemMetaInfo>();
            map = themap;
            if (ItemStaticIdWithEffectId == null)
            {
                ItemStaticIdWithEffectId = new Dictionary<int, int>();
                ItemStaticIdWithEffectId.Add(106, 10000);
            }
        }

        public void Create(int staticId, Vector3 location, Vector3 eulerAngle)
        {
            ThrowOutItemMetaInfo info = new ThrowOutItemMetaInfo();
            info.Id = staticId;
            info.ServerId = svrIdMax;
            svrIdMax++;
            info.Location = location;
            info.EulerAngle = eulerAngle;
            Items.Add(info);
            List<string> data = new List<string>();
            data.Add("throwFlyingItem");
            data.Add(staticId.ToString());
            data.Add(info.ServerId.ToString());
            data.Add(location.ToXYZ());
            data.Add(eulerAngle.ToXYZ());
            map.SendAll(data);
            Logger.WriteLine(LogLevel.Info, "新增投掷物品ID={0}，位置{1}，角度{2}", info.ServerId, location, eulerAngle);
        }

        internal void RecvBombData(int svrId, Vector3 location)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].ServerId == svrId)
                {
                    Logger.WriteLine(LogLevel.Default, "投掷物 ServerId:{0} 已爆炸", svrId);
                    map.PlayEffect(ItemStaticIdWithEffectId[Items[i].Id], location);
                    Items.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public struct ThrowOutItemMetaInfo
    {
        public int Id;
        public int ServerId;
        public Vector3 Location;
        public Vector3 EulerAngle;
    }
}
