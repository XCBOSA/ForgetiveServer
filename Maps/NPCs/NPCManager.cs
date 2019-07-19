using Forgetive.Server.Math;
using System.Collections.Generic;
using static System.Math;
using static Forgetive.Server.Maps.MapBase;
using System;

namespace Forgetive.Server.Maps.NPCs
{
    public static class NPCManager
    {
        public static List<NPC> NPCs;
        public static int MaxNPCCount = 50;
        public static float MaxSwitchDistance = 75f;
        public static readonly int ItemGCoinStdId = 34;
        public static readonly int ItemXCoinStdId = 38;
        public static Delay loop, loopSend;

        static MapBase map;
        static int ids = 0;

        public static void Init()
        {
            NPCs = new List<NPC>();
        }

        public static void InitMap(MapBase mp)
        {
            loop = new Delay(mp);
            loop.SetDelay(10, Update);
            loopSend = new Delay(mp);
            loopSend.SetDelay(50, LoopSendState);
            map = mp;
        }

        static void LoopSendState()
        {
            try
            {
                loopSend.SetDelay(50, LoopSendState);
                List<string> data = new List<string>();
                data.Add("updateNPCState");
                for (int i = 0; i < NPCs.Count; i++)
                {
                    NPCServerToClientData sdata = NPCs[i].GetServerToClientData();
                    data.Add(sdata.ToData());
                }
                map.ReplaceAll(data);
            }
            catch { }
        }

        /// <summary>
        /// 1-25%
        /// 2-25%
        /// 3-20%
        /// 4-9%
        /// 5-6%
        /// 6-5%
        /// 7-4%
        /// 8-3%
        /// 9-2%;
        /// 10-1%;
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static int RandomLevelNPC(Player host)
        {
            int t = Random(0, 100);
            if (t >= 99)
                return RandomNPC(host, 10);
            else if (t >= 97)
                return RandomNPC(host, 9);
            else if (t >= 94)
                return RandomNPC(host, 8);
            else if (t >= 90)
                return RandomNPC(host, 7);
            else if (t >= 85)
                return RandomNPC(host, 6);
            else if (t >= 79)
                return RandomNPC(host, 5);
            else if (t >= 70)
                return RandomNPC(host, 4);
            else if (t >= 50)
                return RandomNPC(host, 3);
            else if (t >= 25)
                return RandomNPC(host, 2);
            else return RandomNPC(host, 1);
        }

        /// <summary>
        /// level=1~10
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static int RandomNPC(Player host, int level)
        {
            Vector3 rotation = new Vector3(0f, 0f, 0f);
            float ax = host.Location.X + Random(-30, 30);
            float az = Abs((float)Sqrt(Abs(50f - (float)Pow(Abs(host.Location.X) - ax, 2))) - host.Location.Z);
            Vector3 location = new Vector3(ax, 0f, az);
            switch (level)
            {
                case 1:
                    return CreateNPC(host, true, Random(3, 5), "1级强盗",
                    Random(1, 3), 5.5f, location, rotation, ItemGCoinStdId, "",
                    Random(30, 80).ToString());
                case 2:
                    return CreateNPC(host, true, Random(5, 8), "2级强盗",
                    Random(3, 5), 5.5f, location, rotation, ItemGCoinStdId, "",
                    Random(80, 120).ToString());
                case 3:
                    return CreateNPC(host, true, Random(8, 20), "3级强盗",
                    Random(5, 12), 5f, location, rotation, 113, "掉落的寒风剑",
                    Random(20, 30).ToString());
                case 4:
                    return CreateNPC(host, true, Random(20, 50), "4级强盗",
                    Random(12, 30), 4f, location, rotation, Random(113, 115), "掉落的寒风剑",
                    Random(20, 30).ToString());
                case 5:
                    return CreateNPC(host, true, Random(50, 100), "5级盗贼",
                    Random(30, 40), 3f, location, rotation, ItemXCoinStdId, "",
                    Random(120, 160).ToString());
                case 6:
                    return CreateNPC(host, true, Random(100, 150), "6级盗贼",
                    Random(40, 80), 3f, location, rotation, 111, "在背包中受到死亡攻击时将消耗自身抵挡攻击并获得100点血量",
                    Random(160, 300).ToString());
                case 7:
                    return CreateNPC(host, true, Random(200, 300), "7级盗贼",
                    Random(80, 150), 3f, location, rotation, ItemXCoinStdId, "",
                    Random(400, 600).ToString());
                case 8:
                    return CreateNPC(host, true, Random(300, 500), "8级盗贼",
                    Random(200, 240), 3f, location, rotation, ItemXCoinStdId, "",
                    Random(800, 1400).ToString());
                case 9:
                    {
                        map.SendAllChat("9级盗墓贼出没，击败它将获得伤害超高的极品武器！", "Forgetive");
                        return CreateNPC(host, true, Random(500, 800), "9级盗墓贼",
                        Random(500, 600), 2.5f, location, rotation, 114, "掉落的寒风剑，伤害极高，用于攻击10级盗墓贼有奇效",
                        Random(380, 520).ToString());
                    }
                case 10:
                    {
                        map.SendAllChat("10级盗墓贼出没，击败它将获得大量X币奖励！", "Forgetive");
                        return CreateNPC(host, true, Random(2000, 2400), "10级盗墓贼",
                        Random(1200, 3000), 2.5f, location, rotation, ItemXCoinStdId, "",
                        Random(11000, 16000).ToString());
                    }
            }
            return -1;
        }

        public static int CreateNPC(Player host, bool canAttackPlayer, int npcHealth, string name,
            int damage, float speed, Vector3 stdLocation, Vector3 stdRotation,
            int npcTakeItemId, string npcTakeItemDesc, string npcTakeItemExtra)
        {
            if (NPCs.Count >= MaxNPCCount)
                return -1;
            NPC npc = new NPC();
            npc.CanAttackPlayer = canAttackPlayer;
            npc.Location = stdLocation;
            npc.Rotation = stdRotation;
            npc.NPCId = ids;
            npc.NPCHealth = npcHealth;
            npc.Host = host;
            npc.Name = name;
            npc.NPCTakeItemId = npcTakeItemId;
            npc.NPCTakeItemDesc = npcTakeItemDesc;
            npc.NPCTakeItemExtra = npcTakeItemExtra;
            npc.Damage = damage;
            npc.NPCMoveSpeed = speed;
            NPCs.Add(npc);
            host.ownedNPCs.Add(npc.NPCId);
            All_NewNPCMessage(npc.NPCId);
            host.RecvNPCHost(npc.NPCId);
            ids++;
            return npc.NPCId;
        }

        public static void OnPlayerDied(Player oldPlayer)
        {
            MapBase map = oldPlayer.MapContext;
            for (int i = 0; i < oldPlayer.ownedNPCs.Count; i++)
            {
                NPC npc = GetNPC(oldPlayer.ownedNPCs[i]);
                Player p = map.FindNearPlayer(npc.Location, MaxSwitchDistance, oldPlayer);
                if (p == null)
                {
                    npc.Dispose(false);
                }
                else
                {
                    npc.Host = p;
                    p.ownedNPCs.Add(npc.NPCId);
                    p.RecvNPCHost(npc.NPCId);
                }
                oldPlayer.ownedNPCs.RemoveAt(i);
                i--;
            }
        }

        public static void SwitchPlayer(int npcId, Player newPlayer)
        {
            NPC npc = GetNPC(npcId);
            Player oldPlayer = npc.Host;
            oldPlayer.DisposeNPCHost(npcId);
            if (newPlayer.Location.DistanceOf2D(npc.Location) <= MaxSwitchDistance)
            {
                npc.Host = newPlayer;
                newPlayer.ownedNPCs.Add(npc.NPCId);
                newPlayer.RecvNPCHost(npc.NPCId);
            }
            else
            {
                npc.Dispose(false);
            }
            oldPlayer.ownedNPCs.Remove(npcId);
        }

        public static void Attack(int npcId, float damage, Player source)
        {
            NPC npc = GetNPC(npcId);
            if (npc == null) return;
            if (source.IsDead) return;
            npc.IsAttackByOther = true;
            npc.NotAttackByOtherTime = DateTime.Now + new TimeSpan(0, 0, 0, 0, 500);
            if (npc == null) return;
            if (npc.NPCHealth <= 0) return;
            npc.NPCHealth -= damage;
            if (npc.NPCHealth <= 0)
            {
                npc.NPCHealth = 0;
                if (npc.NPCTakeItemId == ItemGCoinStdId)
                {
                    source.BP += int.Parse(npc.NPCTakeItemExtra);
                    source.SendChat("你获得了" + int.Parse(npc.NPCTakeItemExtra) + "金币", "Forgetive");
                }
                else if (npc.NPCTakeItemId == ItemXCoinStdId)
                {
                    source.XCoin += int.Parse(npc.NPCTakeItemExtra);
                    source.SendChat("你获得了" + int.Parse(npc.NPCTakeItemExtra) + "X币", "Forgetive");
                }
                else if (npc.NPCTakeItemId == 0)
                    source.SendChat("我什么都没有...", source.Name);
                else source.AddItem(npc.NPCTakeItemId, npc.NPCTakeItemDesc, npc.NPCTakeItemExtra);
                for (int i = 0; i < source.MapContext.players.Count; i++)
                    source.MapContext.players[i].OnNPCDied(npcId);
                npc.Dispose(true);
            }
            else
            {
                if (npc.Name.Contains("-"))
                    npc.Name = npc.Name.Split('-')[0] + "-" + npc.NPCHealth + "点生命";
                else npc.Name += "-" + npc.NPCHealth + "点生命";
                if (npc.Host.Name != source.Name)
                {
                    if (Random(0, 2) == 1)
                    {
                        SwitchPlayer(npcId, source);
                    }
                }
            }
        }

        public static NPC GetNPC(int id)
        {
            for (int i = 0; i < NPCs.Count; i++)
            {
                if (NPCs[i].NPCId == id)
                    return NPCs[i];
            }
            return null;
        }

        public static void Update()
        {
            try
            {
                loop.SetDelay(10, Update);
                for (int i = 0; i < NPCs.Count; i++)
                {
                    NPC npc = NPCs[i];
                    if (npc.IsAttacking && npc.NotAttackTime < DateTime.Now)
                        npc.IsAttacking = false;
                    if (npc.IsAttackByOther && npc.NotAttackByOtherTime < DateTime.Now)
                        npc.IsAttackByOther = false;
                    Player hostSrc = npc.Host;
                    MapBase map = hostSrc.MapContext;
                    float distance = npc.Location.DistanceOf2D(hostSrc.Location);
                    if (distance > MaxSwitchDistance || !hostSrc.IsOnline)
                    {
                        Player hostNew = map.FindNearPlayer(npc.Location, MaxSwitchDistance, hostSrc);
                        if (hostNew != null)
                            SwitchPlayer(npc.NPCId, hostNew);
                        else
                        {
                            npc.Dispose(true);
                            i--;
                        }
                    }
                }
            }
            catch { }
        }

        public static void All_DisposeNPCMessage(int npcId)
        {

        }

        public static void All_NewNPCMessage(int npcId)
        {

        }

        public static void RecvDataFromHost(NPCHostToServerData data, Player host)
        {
            NPC npc = GetNPC(data.NPCId);
            if (npc == null) return;
            if (npc.Host.Name != host.Name) return;
            npc.ApplyHostData(data);
        }
    }
}
