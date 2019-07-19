#pragma warning disable CS0618

using Forgetive.Server.Math;
using Forgetive.Server.Test;
using System;
using System.Collections.Generic;
using Forgetive.Server.Items;
using static Forgetive.Server.ForgetiveServer;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Forgetive.Database;
using System.Timers;
using Forgetive.Server.Extension;
using Forgetive.Server.Maps.NPCs;
using System.Threading;
using Forgetive.Server.TreeSystem;

namespace Forgetive.Server.Maps
{
    /// <summary>
    /// 表示一个可重写的地图基类
    /// </summary>
    public abstract class MapBase
    {
        /// <summary>
        /// 地图上的所有玩家
        /// </summary>
        public List<Player> players;

        /// <summary>
        /// 地图名称
        /// </summary>
        public string MapName;

        /// <summary>
        /// 地图的所有方块
        /// </summary>
        public List<Block> blocks;

        /// <summary>
        /// 地图的天空
        /// </summary>
        public SkySystem sky;

        /// <summary>
        /// 地图的掉落物
        /// </summary>
        public ThrownOutItems thrownOutItems;

        /// <summary>
        /// 复活时的血量
        /// </summary>
        public float RespawnOnDeadHealth = 25f;

        /// <summary>
        /// 复活点
        /// </summary>
        public Vector3 RespawnPoint = new Vector3(573.41f, 33.0f, 592.24f);

        /// <summary>
        /// 复活点视角
        /// </summary>
        public Vector3 RespawnEuler = new Vector3(0.0f, 180.0f, 0.0f);

        /// <summary>
        /// 投掷物管理器
        /// </summary>
        public ThrowOutItems ThrowItems;

        public TreeGen Trees;

        /// <summary>
        /// 默认水源高度
        /// </summary>
        public float BaseWaterHeight = 28.6f;

        public Table HealthData;

        public int MaxBlockCount = 10;
        public int MinNPCCreateWaitTime = 10000;
        public int MaxNPCCreateWaitTime = 60000;

        List<Area> SafeAreas;
        Delay saveDelay;
        Thread loopThread;
        internal List<Delay> delays;

        /// <summary>
        /// 表示玩家名称-玩家位置的表
        /// </summary>
        public Table location;

        /// <summary>
        /// 初始化<see cref="MapBase"/>
        /// </summary>
        public MapBase()
        {
            SafeAreas = new List<Area>();
            HealthData = Data.GetTable("user", "health");

            location = Data.GetTable("user", "location");
            players = new List<Player>();
            blocks = new List<Block>();
            ThrowItems = new ThrowOutItems(this);
            thrownOutItems = new ThrownOutItems(this);
            ReadBlocks();

            loopThread = new Thread(Loop);
            loopThread.IsBackground = true;
            loopThread.Priority = ThreadPriority.BelowNormal;
            loopThread.Start();
            delays = new List<Delay>();

            Trees = new TreeGen(this);
            Trees.Init();
            sky = new SkySystem(this);
            saveDelay = new Delay(this);
            saveDelay.SetDelay(5000, WriteBlocks);
            NPCManager.InitMap(this);

            //Block block3 = new Block();
            //block3.blockId = "TestBlock03";
            //block3.updateLocation = new Vector3(548.14f, 32.61f, 588.11f);
            //block3.updateRotation = new Vector3(0f, 0f, 0f);
            //blocks.Add(block3);

            ForgetiveServer.OnPlayerQuit.Reg(OnPlayerQuit);
            Init();
        }

        public Player[] GetOldPlayers()
        {
            List<Player> oldPlayers = new List<Player>();
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].NewPlayerStep == -1)
                    oldPlayers.Add(players[i]);
            }
            return oldPlayers.ToArray();
        }

        void Loop()
        {
            while (true)
            {
                if (delays != null)
                {
                    for (int i = 0; i < delays.Count; i++)
                    {
                        if (delays[i].Do())
                        {
                            delays.RemoveAt(i);
                            i--;
                        }
                    }
                }
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 寻找符合坐标条件的第一个<see cref="Area"/>
        /// </summary>
        /// <param name="location">指定坐标</param>
        /// <returns></returns>
        public Area FindFirstArea(Vector3 location)
        {
            if (location == Vector3.Zero) return null;
            Vector2 vec2 = location.GetNormalVectorXZ();
            for (int i = 0; i < SafeAreas.Count; i++)
            {
                if (SafeAreas[i].Contains(vec2))
                    return SafeAreas[i];
            }
            return null;
        }

        /// <summary>
        /// 添加安全区
        /// </summary>
        /// <param name="safeArea"></param>
        protected void AddSafeArea(Area safeArea)
        {
            SafeAreas.Add(safeArea);
        }
        
        /// <summary>
        /// 强制读取Blocks
        /// </summary>
        public void ReadBlocks()
        {
            string root = Data.RootPath + MapName + "/blocks.bin";
            List_Block lisblock;
            if (!File.Exists(root))
            {
                lisblock = new List_Block();
                lisblock.blocks = new Block[0];
            }
            else
            {
                Stream file = new FileStream(root, FileMode.Open, FileAccess.Read);
                BinaryFormatter binFormat = new BinaryFormatter();
                lisblock = (List_Block)binFormat.Deserialize(file);
                file.Close();
                file.Dispose();
            }
            blocks = new List<Block>(lisblock.blocks);
        }

        /// <summary>
        /// 强制写入Blocks
        /// </summary>
        public void WriteBlocks()
        {
            saveDelay.SetDelay(5000, WriteBlocks);
            try
            {
                string root = Data.RootPath + "/" + MapName + "/blocks.bin";
                List_Block lisblock = new List_Block();
                lisblock.blocks = blocks.ToArray();
                if (File.Exists(root))
                    File.Delete(root);
                Stream file = new FileStream(root, FileMode.Create, FileAccess.Write);
                BinaryFormatter binFormat = new BinaryFormatter();
                binFormat.Serialize(file, lisblock);
                file.Close();
                file.Dispose();
            }
            catch { }
        }

        /// <summary>
        /// 强制释放地图，这将导致异常
        /// </summary>
        public void Dispose()
        {
            ForgetiveServer.OnPlayerQuit.Del(OnPlayerQuit);
        }

        void OnPlayerQuit(UserSocketState sock)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].SocketState == sock)
                    QuitMap(players[i]);
            }
        }

        /// <summary>
        /// 初始化地图
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// 使玩家进入地图
        /// </summary>
        /// <param name="player">玩家的 <see cref="UserSocketState"/></param>
        /// <returns></returns>
        public Player JoinMap(UserSocketState player)
        {
            Player p = null;
            try
            {
                player.ReadedData = new List_String(new List<string>()
                {
                    "map", "noneUpdate"
                });
                p = new Player();
                p.SocketState = player;
                p.Name = player.NickName;
                p.OnPlayerDied.Reg(OnPlayerDied);
                p.MapContext = this;
                players.Add(p);
                player.PlayerContext = p;
                p.Init();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
            Logger.WriteLine("{0}加入了地图{1}", player.NickName, MapName);
            return p;
        }

        /// <summary>
        /// 强制玩家退出地图
        /// </summary>
        /// <param name="player"></param>
        public void QuitMap(Player player)
        {
            NPCManager.OnPlayerDied(player);
            player.OnPlayerDied.Del(OnPlayerDied);
            players.Remove(player);
            player.SocketState.PlayerContext = null;
            player.Dispose();
            Logger.WriteLine("{0}已离开地图{1}", player.SocketState.NickName, MapName);
        }

        public Player FindNearPlayer(Vector3 location, float maxDistance, Player excludePlayer = null)
        {
            float nearest = float.MaxValue;
            Player nearPlayer = null;
            for (int i = 0; i < players.Count; i++)
            {
                if (excludePlayer != null)
                    if (players[i].Name == excludePlayer.Name)
                        continue;
                float distance = players[i].Location.DistanceOf(location);
                if (nearest > distance)
                {
                    nearest = distance;
                    nearPlayer = players[i];
                }
            }
            if (nearest <= maxDistance)
                return nearPlayer;
            else return null;
        }

        //玩家死亡
        void OnPlayerDied(Player diedPlayer)
        {
            NPCManager.OnPlayerDied(diedPlayer);
            Logger.WriteLine(
                LogLevel.Default,
                "{0}: 玩家 {1} 由于 {2} 死亡",
                MapName,
                diedPlayer.Name,
                diedPlayer.LastDamageSourceStr);
            List<string> list = new List<string>();
            list.Add("playerDied");
            list.Add(diedPlayer.SocketState.NickName);
            list.Add(diedPlayer.LastDamageSourceStr);
            SendAll(list);
        }
        
        public void SendAll(List<string> command)
        {
            for (int i = 0; i < players.Count; i++)
                players[i].SendDataPackage(command);
        }

        public void ReplaceAll(List<string> command)
        {
            for (int i = 0; i < players.Count; i++)
                players[i].ReplaceData(command);
        }

        Player SocketToPlayer(UserSocketState socket)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].SocketState == socket)
                    return players[i];
            }
            return null;
        }

        /// <summary>
        /// 在地图中寻找玩家
        /// </summary>
        /// <param name="name">玩家名称</param>
        /// <returns></returns>
        public Player FindPlayer(string name)
        {
            for (int i = 0; i < players.Count; i++)
                if (players[i].SocketState.NickName == name)
                    return players[i];
            return null;
        }

        /// <summary>
        /// 获取地图中的所有玩家
        /// </summary>
        /// <returns></returns>
        public Player[] GetPlayers()
        {
            return players.ToArray();
        }

        bool CheckPam(string[] list, int count, int numIndex = -1)
        {
            if (numIndex >= count)
                return false;
            if (count == list.Length)
            {
                if (numIndex == -1)
                    return true;
                return float.TryParse(list[numIndex], out float tmp);
            }
            else return false;
        }

        /// <summary>
        /// 玩家是否在安全区内
        /// </summary>
        /// <param name="lctn">玩家位置</param>
        /// <param name="isSelfDamaged">是否自己伤害自己</param>
        /// <returns></returns>
        public bool IsInSafeArea(Vector3 lctn, bool isSelfDamaged)
        {
            if (SafeAreas == null) return true;
            Vector2 vec = lctn.GetNormalVectorXZ();
            for (int i = 0; i < SafeAreas.Count; i++)
            {
                if (SafeAreas[i].Contains(vec))
                {
                    if (isSelfDamaged && SafeAreas[i].EnableSelfDamage)
                        return false;
                    else if ((!isSelfDamaged) && SafeAreas[i].EnablePVP)
                        return false;
                    else return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 是否刷怪
        /// </summary>
        /// <param name="lctn">位置</param>
        /// <returns></returns>
        public bool IsInPVEArea(Vector3 lctn)
        {
            if (SafeAreas == null) return false;
            Vector2 vec = lctn.GetNormalVectorXZ();
            for (int i = 0; i < SafeAreas.Count; i++)
            {
                if (SafeAreas[i].Contains(vec))
                {
                    return SafeAreas[i].EnablePVE;
                }
            }
            return false;
        }

        static Random rd;

        public static int Random(int min, int max)
        {
            if (rd == null) rd = new Random();
            return rd.Next(min, max);
        }

        internal void WRecv(Player selectPlayer, string[] msgs)
        {
            List<string> data = new List<string>();
            switch (msgs[0])
            {
                case "damageSelf":
                    if (selectPlayer.IsDead) return;
                    if (!CheckPam(msgs, 3, 2)) return;

                    if (IsInSafeArea(selectPlayer.Location, true))
                        break;

                    selectPlayer.LastDamageSourceStr = msgs[1];
                    selectPlayer.Health -= float.Parse(msgs[2]);

                    if (selectPlayer.Health > 0)
                    {
                        selectPlayer.IsInAttackAnimation = true;
                        selectPlayer.animDelay.SetDelay(500, () => selectPlayer.IsInAttackAnimation = false);
                    }

                    data.Add("damage");
                    data.Add(msgs[1]);
                    data.Add(selectPlayer.Health.ToString());

                    selectPlayer.SendDataPackage(data);
                    break;
                case "damageOther":
                    string killerName = selectPlayer.Name;
                    Player killer = selectPlayer;

                    if (selectPlayer.IsDead) return;
                    if (!CheckPam(msgs, 4, 2)) return;

                    if (IsInSafeArea(selectPlayer.Location, false))
                        break;

                    string attackObj = msgs[3];
                    if (msgs[3].Contains("onlineplayer_"))
                        attackObj = msgs[3].Substring(13);
                        
                    selectPlayer = FindPlayer(attackObj);
                    if (selectPlayer == null) return;

                    if (IsInSafeArea(selectPlayer.Location, false))
                        break;
                    if (selectPlayer.IsDead)
                        break;

                    selectPlayer.LastDamageSourceStr = msgs[1];
                    selectPlayer.Health -= float.Parse(msgs[2]);

                    if (selectPlayer.Health <= 0)
                    {
                        if (selectPlayer.XCoin >= 500)
                        {
                            int xcoin = selectPlayer.XCoin / 40;
                            if (xcoin > 1000) xcoin = 1000;
                            selectPlayer.XCoin -= xcoin;
                            selectPlayer.SendChat("你失去了" + xcoin + "X币", "Forgetive");
                            killer.XCoin += xcoin;
                            killer.SendChat("你得到了" + xcoin + "X币", "Forgetive");
                            SendAllChat(killerName + " 击杀了 " + selectPlayer.Name + " 并抢夺了财产 " + xcoin + "X币", "Forgetive");
                        }
                        else SendAllChat(killerName + " 击杀了 " + selectPlayer.Name, "Forgetive");
                    }
                    else
                    {
                        selectPlayer.IsInAttackAnimation = true;
                        selectPlayer.animDelay.SetDelay(500, () => selectPlayer.IsInAttackAnimation = false);
                    }
                    
                    data.Add("damage");
                    data.Add(msgs[1]);
                    data.Add(selectPlayer.Health.ToString());

                    selectPlayer.SendDataPackage(data);
                    break;
                case "updateSelf":
                    {
                        if (selectPlayer.IsDead) return;
                        if (selectPlayer.LastLocation == Vector3.Zero)
                            selectPlayer.LastLocation = new Vector3(-1f, -1f, -1f);
                        else selectPlayer.LastLocation = selectPlayer.Location;
                        selectPlayer.Location = new Vector3(msgs[1]);
                        selectPlayer.Rotation = new Vector3(msgs[2]);
                        int.TryParse(msgs[3], out selectPlayer.State);
                        selectPlayer.CalcLocationChanged();
                    }
                    break;
                case "getUserItems":
                    data.Add("updateFullItems");
                    for (int i = 0; i < selectPlayer.Items.Count; i++)
                    {
                        int index = selectPlayer.Items[i];
                        Item itemdesc = ItemStorage.GetItemDesc(index);
                        if (itemdesc == null)
                            continue;
                        data.Add(itemdesc.ItemStaticId.ToString());
                        data.Add(itemdesc.ServerId.ToString());
                        data.Add(Base64Encode(itemdesc.DescribeMessage));
                        data.Add(Base64Encode(itemdesc.ExtraInfo));
                        data.Add("next");
                    }
                    selectPlayer.SendDataPackage(data);
                    data = new List<string>();
                    data.Add("updateDockItems");
                    for (int i = 0; i < selectPlayer.BarItemsId.Count; i++)
                        data.Add(selectPlayer.BarItemsId[i].ToString());
                    selectPlayer.SendDataPackage(data);

                    break;
                case "setUserDocks":
                    if (selectPlayer.IsDead) return;
                    List<int> dockIds = new List<int>();
                    for (int i = 0; i < msgs.Length; i++)
                    {
                        if (i == 0)
                            continue;
                        int id = int.Parse(msgs[i]);
                        dockIds.Add(id);
                    }
                    selectPlayer.BarItemsId = dockIds;
                    break;
                case "useItem":
                    {
                        if (selectPlayer.IsDead) return;
                        int itemStaticId = int.Parse(msgs[1]);
                        int serverId = int.Parse(msgs[2]);
                        string desc = Base64Decode(msgs[3]);
                        string extra = Base64Decode(msgs[4]);
                        for (int i = 0; i < selectPlayer.Items.Count; i++)
                        {
                            if (selectPlayer.Items[i] == serverId)
                            {
                                ItemStorage.SetChanged(serverId, itemStaticId, desc, extra);
                            }
                        }
                    }
                    break;
                case "tryControlBlock":
                    {
                        if (selectPlayer.IsDead) return;
                        string blockName = msgs[1];
                        Player srcPlayer = null;
                        bool findBlock = false;
                        for (int i = 0; i < blocks.Count; i++)
                        {
                            if (blocks[i].blockId == blockName)
                            {
                                srcPlayer = FindPlayer(blocks[i].controllerName);
                                blocks[i].controllerName = selectPlayer.Name;
                                findBlock = true;
                                break;
                            }
                        }
                        if (!findBlock)
                        {
                            Block block = new Block();
                            block.blockId = blockName;
                            block.controllerName = selectPlayer.Name;
                            blocks.Add(block);
                        }
                        if (srcPlayer != null)
                            if (selectPlayer.Name == srcPlayer.Name) return;
                        data.Add("updateBlockController");
                        data.Add(blockName);
                        data.Add(bool.TrueString);
                        selectPlayer.SendDataPackage(data);
                        Logger.WriteLine(selectPlayer.Name + "已获得对" + blockName + "的控制权");
                        if (srcPlayer == null) return;
                        data = new List<string>();
                        data.Add("updateBlockController");
                        data.Add(blockName);
                        data.Add(bool.FalseString);
                        srcPlayer.SendDataPackage(data);
                        Logger.WriteLine(selectPlayer.Name + "已失去对" + blockName + "的控制权");
                    }
                    break;
                case "sendBlockState":
                    {
                        if (selectPlayer.IsDead) return;
                        Block selectBlock = null;
                        string id = msgs[1];
                        Vector3 lctn = new Vector3(msgs[2]);
                        Vector3 rotn = new Vector3(msgs[3]);
                        for (int i = 0; i < blocks.Count; i++)
                        {
                            if (blocks[i].blockId == id)
                            {
                                selectBlock = blocks[i];
                                break;
                            }
                        }
                        if (selectBlock == null)
                            return;
                        if (selectBlock.controllerName != selectPlayer.Name)
                            return;
                        selectBlock.updateLocation = lctn;
                        selectBlock.updateRotation = rotn;

                        data.Add("updateBlock");
                        data.Add(id);
                        data.Add(lctn.ToXYZ());
                        data.Add(rotn.ToXYZ());
                        data.Add(selectBlock.blockType.ToString());

                        SendAll(data);

                        //for (int i = 0; i < players.Count; i++)
                        //{
                        //    Player user = players[i];
                        //    if (user != null)
                        //    {
                        //        if (user.SocketState != null)
                        //        {
                        //            if (user.SocketState.OtherData != null)
                        //            {
                        //                user.SocketState.OtherData.Add(new List_String(data));
                        //            }
                        //        }
                        //    }
                        //}
                    }
                    break;
                case "getBlocks":
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        data = new List<string>();
                        data.Add("updateBlock");
                        data.Add(blocks[i].blockId);
                        data.Add(blocks[i].updateLocation.ToXYZ());
                        data.Add(blocks[i].updateRotation.ToXYZ());
                        data.Add(blocks[i].blockType.ToString());
                        selectPlayer.SendDataPackage(data);
                    }
                    break;
                case "getSelf":
                    data.Add("updateSelfLocation");
                    data.Add(selectPlayer.Location.ToXYZ());
                    data.Add(selectPlayer.Rotation.ToCameraRotation().ToXYZ());
                    selectPlayer.SendDataPackage(data);
                    break;
                case "inited":
                    selectPlayer.HasRecvInited = true;
                    Logger.WriteLine(selectPlayer.Name + "已经完成初始化");
                    selectPlayer.OnInited();
                    break;
                case "throwItem":
                    if (selectPlayer.IsDead) return;
                    thrownOutItems.ThrowFromPlayer(int.Parse(msgs[1]), new Vector3(msgs[2]), selectPlayer);
                    break;
                case "pickupItem":
                    if (selectPlayer.IsDead) return;
                    thrownOutItems.PickUpFromPlayer(selectPlayer, int.Parse(msgs[1]));
                    break;
                case "updateThrownItems":
                    thrownOutItems.SendToNewPlayer(selectPlayer);
                    break;
                case "getHealth":
                    data.Add("updateSelfHealth");
                    data.Add(selectPlayer.Health.ToString());
                    selectPlayer.SendDataPackage(data);
                    break;
                case "respawn":
                    if (selectPlayer.IsDead)
                    {
                        selectPlayer.Health = RespawnOnDeadHealth;
                        data.Add("respawnCallback");
                        data.Add(true.ToString());
                        data.Add(RespawnOnDeadHealth.ToString());
                        data.Add(RespawnPoint.ToXYZ());
                        data.Add(RespawnEuler.ToXYZ());
                        selectPlayer.SendDataPackage(data);
                        List<string> dataToAll = new List<string>();
                        dataToAll.Add("playerRespawn");
                        dataToAll.Add(selectPlayer.Name);
                        SendAll(dataToAll);
                    }
                    else
                    {
                        data.Add("respawnCallback");
                        data.Add(true.ToString());
                        data.Add(RespawnOnDeadHealth.ToString());
                        data.Add(RespawnPoint.ToXYZ());
                        data.Add(RespawnEuler.ToXYZ());
                        selectPlayer.SendDataPackage(data);
                    }
                    break;
                case "useItemEx":
                    if (selectPlayer.IsDead) return;
                    ItemUsage.Invoke(int.Parse(msgs[1]), selectPlayer);
                    break;
                case "startBomb":
                    if (selectPlayer.MapContext == null) return;
                    selectPlayer.MapContext.ThrowItems.RecvBombData(int.Parse(msgs[1]), new Vector3(msgs[2]));
                    break;
                case "changeThrownItemLctn":
                    thrownOutItems.ChangeItemLocation(int.Parse(msgs[1]), new Vector3(msgs[2]));
                    break;
                case "removeBlock":
                    {
                        for (int i = 0; i < blocks.Count; i++)
                        {
                            if (blocks[i].blockId == msgs[1])
                            {
                                data.Add("removeBlock");
                                data.Add(blocks[i].blockId);
                                blocks.RemoveAt(i);
                                SendAll(data);
                            }
                        }
                    }
                    break;
                case "buyItem":
                    {
                        int staticId = int.Parse(msgs[1]);
                        string desc = Base64Decode(msgs[2]);
                        string extra = Base64Decode(msgs[3]);
                        int cost = int.Parse(msgs[4]);
                        bool isPayInXCoin = bool.Parse(msgs[5]);
                        int idc = selectPlayer.BuyItem(staticId, desc, extra, cost, isPayInXCoin);
                        data.Add("buyItemCallback");
                        data.Add(idc.ToString());
                        selectPlayer.ReplaceData(data);
                        selectPlayer.SendXCoinAndBPData();
                    }
                    break;
                case "morebp":
                    {
                        int xcoin = int.Parse(msgs[1]);
                        int bp = int.Parse(msgs[2]);
                        int idc = selectPlayer.XCoinToBP(xcoin, bp);
                        data.Add("morebpCallback");
                        data.Add(idc.ToString());
                        selectPlayer.ReplaceData(data);
                        selectPlayer.SendXCoinAndBPData();
                    }
                    break;
                case "updateNewPlayerHelpStep":
                    {
                        int m = int.Parse(msgs[1]);
                        if (selectPlayer.NewPlayerStep != -1)
                        {
                            if (m == -1)
                            {
                                selectPlayer.BP += 1000;
                                selectPlayer.SendChat("你完成了新手引导任务，获得1000金币", "Forgetive");
                            }
                        }
                        selectPlayer.NewPlayerStep = m;
                    }
                    break;
                case "updateNPCState":
                    {
                        for (int i = 1; i < msgs.Length; i++)
                            NPCManager.RecvDataFromHost(new NPCHostToServerData(msgs[i]), selectPlayer);
                    }
                    break;
                case "damageNPC":
                    {
                        NPCManager.Attack(int.Parse(msgs[1]), float.Parse(msgs[2]), selectPlayer);
                    }
                    break;
                default:
                    ExecutionContent.clicmd.Call(selectPlayer, msgs);
                    break;
            }
            data.Clear();
        }

        public void PlayEffect(int id, Vector3 lctn)
        {
            List<string> data = new List<string>();
            data.Add("unsafeArea");
            data.Add(id.ToString());
            data.Add(lctn.ToXYZ());
            SendAll(data);
        }

        public void SendAllChat(string info, string title)
        {
            for (int i = 0; i < players.Count; i++)
            {
                players[i].SendChat(info, title);
            }
        }

        /// <summary>
        /// 计算回执内容
        /// 报文结构:
        /// map
        /// updateOther
        /// {PlayerName}
        /// {Location}
        /// {Rotation}
        /// {State}
        /// next
        /// {PlayerName2}
        /// ...
        /// </summary>
        /// <param name="currentPlayer">排除的玩家</param>
        public void EvalReadedData(Player currentPlayer)
        {
            List<string> tempData = new List<string>();
            tempData.Add("map");
            tempData.Add("updateOther");
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Name == currentPlayer.Name)
                    continue;
                if (!players[i].HasRecvInited)
                    continue;
                tempData.Add(Base64Encode(players[i].Name));
                tempData.Add(players[i].Location.ToXYZ());
                tempData.Add(players[i].Rotation.ToXYZ());
                if (players[i].IsInAttackAnimation && !players[i].IsDead)
                    tempData.Add("4");
                else tempData.Add(players[i].State.ToString());
                tempData.Add("next");
            }
            currentPlayer.SocketState.ReadedData = new List_String(tempData);
        }
    }
}
