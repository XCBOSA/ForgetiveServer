#pragma warning disable CS0618

using Forgetive.Server.Math;
using Forgetive.Server.Items;
using System;
using System.Collections.Generic;
using Forgetive.Server.Properties;
using System.Drawing;
using Forgetive.Server.Test;
using Forgetive.Server.Maps.NPCs;
using Forgetive.Server.TreeSystem;

namespace Forgetive.Server.Maps
{
    /// <summary>
    /// 描述玩家信息并提供上层操作的类型，基于底层操作 <see cref="UserSocketState"/>，并且必须已经进入一个明确指定的 <see cref="MapBase"/>
    /// </summary>
    public class Player
    {
        const int MaxPackageSize = 84;
        internal const string Keyword = "[封地] ";

        public bool IsOnline
        {
            get => !(npcDelay == null && animDelay == null & serverUpdateDelay == null);
        }

        /// <summary>
        /// 玩家死亡事件回调
        /// </summary>
        [Obsolete("注册这个回调没有意义，因为玩家死亡时Player对象将被销毁")]
        public CallBack<Action<Player>> OnPlayerDied;

        /// <summary>
        /// 表示上一次伤害的伤害源
        /// </summary>
        public string LastDamageSourceStr { get; set; }

        Vector3 flctn, frotn;

        /// <summary>
        /// 上一次更新的位置
        /// </summary>
        public Vector3 LastLocation;

        /// <summary>
        /// 获取和设置玩家位置，所做的更改将在玩家客户端下次更新时失效
        /// </summary>
        public Vector3 Location
        {
            get
            {
                if (flctn == Vector3.Zero)
                    return new Vector3(0f, 0f, 0f);
                else return flctn;
            }
            set
            {
                if (HasRecvInited)
                {
                    flctn = value;
                    OnClientUpdate();
                }
            }
        }

        /// <summary>
        /// 获取和设置玩家面向角度，所做的更改将在玩家客户端下次更新时失效
        /// </summary>
        public Vector3 Rotation
        {
            get
            {
                if (frotn == Vector3.Zero)
                    return Vector3.Zero;
                else return frotn;
            }
            set
            {
                if (HasRecvInited)
                {
                    frotn = value;
                }
            }
        }

        public List<int> ownedNPCs;

        public bool IsInSinglePlayerMode = false;

        public int XCoin
        {
            get
            {
                return int.Parse(Program.nick_xcoin.GetFirstYWithX(Name, "0"));
            }
            set
            {
                Program.nick_xcoin.SetXToY(Name, value.ToString());
            }
        }

        public int BP
        {
            get
            {
                return int.Parse(Program.nick_bp.GetFirstYWithX(Name, "0"));
            }
            set
            {
                Program.nick_bp.SetXToY(Name, value.ToString());
            }
        }

        /// <summary>
        /// 玩家的客户端是否已经初始化
        /// </summary>
        public bool HasRecvInited = false;

        /// <summary>
        /// 玩家的状态
        /// </summary>
        public int State;

        /// <summary>
        /// 玩家的名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 玩家所在的地图
        /// </summary>
        public MapBase MapContext;

        /// <summary>
        /// 玩家的物品
        /// </summary>
        public List<int> Items;

        /// <summary>
        /// 玩家操作栏上的物品
        /// </summary>
        public List<int> BarItemsId;

        /// <summary>
        /// 玩家是否有权限使用OP指令
        /// </summary>
        public bool IsOp
        {
            get
            {
                return bool.Parse(Program.nick_isop.GetFirstYWithX(Name, "False"));
            }
            set
            {
                Program.nick_isop.SetXToY(Name, value.ToString());
            }
        }

        public int NewPlayerStep
        {
            get
            {
                return int.Parse(Program.nick_newplayerstep.GetFirstYWithX(Name, "0"));
            }
            set
            {
                Program.nick_newplayerstep.SetXToY(Name, value.ToString());
            }
        }

        public bool IsNewPlayer;
        public bool IsInAttackAnimation;

        public Delay animDelay, npcDelay, serverUpdateDelay;

        internal Player()
        {
            OnPlayerDied = new CallBack<Action<Player>>();
            ownedNPCs = new List<int>();
            Location = new Vector3(0f, 0f, 0f);
            Rotation = new Vector3(0f, 0f, 0f);
            State = 0;
        }

        void NPCDelay()
        {
            if (npcDelay == null) return;
            if (NewPlayerStep == -1 &&
                MapContext.IsInPVEArea(Location) &&
                ownedNPCs.Count < 4 &&
                !MapContext.sky.IsDay)
                NPCManager.RandomLevelNPC(this);
            npcDelay.SetDelay(
                MapBase.Random(MapContext.MinNPCCreateWaitTime, MapContext.MaxNPCCreateWaitTime),
                NPCDelay);
        }

        void OnClientUpdate()
        {
            if (MapContext == null) return;
            
        }

        /// <summary>
        /// 降低玩家血量，已死亡玩家无法进行此操作
        /// </summary>
        /// <param name="damage">降低大小</param>
        /// <param name="compStr_reason">使用表示法的伤害源字符串</param>
        public void Damage(float damage, string compStr_reason = "HITLAST")
        {
            if (IsDead) return;
            LastDamageSourceStr = compStr_reason;
            Health -= damage;
            List<string> data = new List<string>();
            data.Add("updateSelfHealth");
            data.Add(Health.ToString());
            SendDataPackage(data);
        }

        /// <summary>
        /// 提升玩家血量，已死亡玩家无法进行此操作
        /// </summary>
        /// <param name="damage">提升大小</param>
        /// <param name="compStr_reason">使用表示法的伤害源字符串</param>
        public void UpHealth(float damage, string compStr_reason = "HITLAST")
        {
            if (IsDead) return;
            LastDamageSourceStr = compStr_reason;
            Health += damage;
            List<string> data = new List<string>();
            data.Add("updateSelfHealth");
            data.Add(Health.ToString());
            SendDataPackage(data);
        }

        internal void Init()
        {
            Items = new List<int>();
            BarItemsId = new List<int>();
            IsNewPlayer = false;
            animDelay = new Delay(MapContext);
            npcDelay = new Delay(MapContext);
            serverUpdateDelay = new Delay(MapContext);
            PlayerOwnServerId ser = ItemStorage.ReadPlayer(Name, ref IsNewPlayer);
            if (IsNewPlayer)
            {
                AddItem(102, "Forgetive 欢迎你 !!", Resources.ForgetiveHello);
                AddItem(101, "一张用来写字的纸", "");
                AddItem(104, "使用后生命值迅速提高25点", "25");
                AddItem(104, "使用后生命值迅速提高50点", "50");
                AddItem(104, "使用后生命值迅速提高75点", "75");
                AddItem(104, "使用后生命值迅速提高100点", "100");
            }
            else
            {
                Items.AddRange(ser.Items);
                BarItemsId.AddRange(ser.DockItems);
            }
            if (XCoin == 0)
            {
                XCoin += 2019;
                SendStoryMessage("赠送您2019个X币（=RMB:2019)，仅在此服务器使用", Color.Green, 8);
            }
            flctn = new Vector3(MapContext.location.GetFirstYWithX(Name + ".location", "545:34:586"));
            frotn = new Vector3(MapContext.location.GetFirstYWithX(Name + ".rotation", "0:0:0"));

            List<string> data = new List<string>();
            data.Add("newplayerhelp");
            data.Add(NewPlayerStep.ToString());
            SendDataPackage(data);

            npcDelay.SetDelay(
                MapBase.Random(MapContext.MinNPCCreateWaitTime, MapContext.MaxNPCCreateWaitTime),
                NPCDelay);
            serverUpdateDelay.SetDelay(1000, ServerUpdate);

            MapContext.Trees.UpdateForPlayer(this);

            //SendStoryMessage("目前已知Bug：与服务器断开连接时客户端不会有任何显示", Color.Red, 5);
            //SendStoryMessage("如果客户端右上角CON一直不变或显示0ms则已经断开连接", Color.Red, 5);
            //SendStoryMessage("以上问题将在下一个版本中修复", Color.Red, 5);
        }

        internal void CalcLocationChanged()
        {
            Area lastInfo = MapContext.FindFirstArea(LastLocation);
            Area curInfo = MapContext.FindFirstArea(Location);
            if (lastInfo == null)
            {
                if (curInfo != null)
                {
                    UndoStoryMessage(Keyword);
                    SendStoryMessage(curInfo.JoinMessage, Color.Green, 5f);
                }
            }
            else
            {
                if (curInfo == null)
                {
                    UndoStoryMessage(Keyword);
                    SendStoryMessage(lastInfo.LeaveMessage, Color.Orange, 5f);
                    return;
                }
                if (curInfo != lastInfo)
                {
                    UndoStoryMessage(Keyword);
                    SendStoryMessage(curInfo.JoinMessage, Color.Green, 5f);
                }
            }
        }

        bool firstTick = false;
        const float dWaterDamage = 1.01f;
        const float dWaterDamageMax = 20f;
        float waterDamage = dWaterDamage;

        /// <summary>
        /// 保存信息并发送地图天空数据
        /// </summary>
        public void ServerUpdate()
        {
            serverUpdateDelay.SetDelay(1000, ServerUpdate);

            if (!firstTick)
            {
                firstTick = true;
                return;
            }

            if (!HasRecvInited)
            {
                MapContext.WRecv(this, new string[1] { "getUserItems" });
                MapContext.WRecv(this, new string[1] { "getBlocks" });
                MapContext.WRecv(this, new string[1] { "getSelf" });
                MapContext.WRecv(this, new string[1] { "updateThrownItems" });
                MapContext.WRecv(this, new string[1] { "getHealth" });
            }

            if (Items != null && BarItemsId != null)
            {
                PlayerOwnServerId items = new PlayerOwnServerId();
                items.DockItems = BarItemsId.ToArray();
                items.Items = Items.ToArray();
                ItemStorage.WritePlayer(Name, items);
            }

            if (MapContext == null) return;
            List<string> data = new List<string>();
            data.Add("updateSkyRotation");
            data.Add(MapContext.sky.Rotation.ToString());
            data.Add(MapContext.sky.RotateSpeed.ToString());
            data.Add(MapContext.sky.TimeStamp.ToString());
            data.Add(MapContext.sky.FogLevel.ToString());
            SendDataPackage(data);
            data.Clear();

            if (HasRecvInited)
            {
                data.Add("updateFullItems");
                for (int i = 0; i < Items.Count; i++)
                {
                    int index = Items[i];
                    Item itemdesc = ItemStorage.GetItemDesc(index);
                    if (itemdesc == null)
                        continue;
                    data.Add(itemdesc.ItemStaticId.ToString());
                    data.Add(itemdesc.ServerId.ToString());
                    data.Add(ForgetiveServer.Base64Encode(itemdesc.DescribeMessage));
                    data.Add(ForgetiveServer.Base64Encode(itemdesc.ExtraInfo));
                    data.Add("next");
                }
                ReplaceData(data);
                data.Clear();
                List<string> data2 = new List<string>();
                data2.Add("updateDockItems");
                for (int i = 0; i < BarItemsId.Count; i++)
                    data2.Add(BarItemsId[i].ToString());
                ReplaceData(data2);
                data2.Clear();
            }

            if (MapContext != null)
            {
                MapContext.location.SetXToY(Name + ".location", flctn.ToXYZ());
                MapContext.location.SetXToY(Name + ".rotation", frotn.ToXYZ());
            }

            if (!IsDead && Location.Y <= MapContext.BaseWaterHeight)
            {
                waterDamage = waterDamage * waterDamage;
                if (waterDamage <= dWaterDamageMax)
                    Damage(waterDamage, "HITBYWATER");
                else Damage(dWaterDamageMax, "HITBYWATER");
            }
            else waterDamage = dWaterDamage;

            SendXCoinAndBPData();

            List<string> data3 = new List<string>();
            data3.Add("updateSelfHealth");
            data3.Add(Health.ToString());
            ReplaceData(data3);
            data3.Clear();
        }

        public void SendXCoinAndBPData()
        {
            List<string> data3 = new List<string>();
            data3.Add("xcoin");
            data3.Add(XCoin.ToString());
            ReplaceData(data3);
            data3.Clear();

            List<string> data4 = new List<string>();
            data4.Add("bp");
            data4.Add(BP.ToString());
            ReplaceData(data4);
            data4.Clear();
        }

        public int BuyItem(int staticId, string desc, string extra, int price, bool isPayInXCoin)
        {
            if (isPayInXCoin)
            {
                if (price > XCoin)
                    return 1;
                if (!CanAddItem())
                    return 2;
                XCoin -= price;
                AddItem(staticId, desc, extra);
                return 0;
            }
            else
            {
                if (price > BP)
                    return 3;
                if (!CanAddItem())
                    return 2;
                BP -= price;
                AddItem(staticId, desc, extra);
                return 0;
            }
        }

        public int XCoinToBP(int xcoin, int bp)
        {
            if (xcoin > XCoin)
                return 1;
            XCoin -= xcoin;
            BP += bp;
            return 0;
        }

        public bool CanAddItem()
        {
            return Items.Count < MaxPackageSize;
        }

        /// <summary>
        /// 玩家是否拥有指定物品
        /// </summary>
        /// <param name="svrId">物品的ServerId</param>
        /// <returns>是否拥有</returns>
        public bool Owned(int svrId)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] == svrId)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 从服务器删除玩家使用的物品
        /// </summary>
        /// <param name="svrId">物品的ServerId</param>
        /// <param name="callBackToClient">是否向客户端发送删除信号</param>
        /// <returns>是否成功</returns>
        public bool DeleteItem(int svrId, bool callBackToClient = true)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] == svrId)
                {
                    Items.RemoveAt(i);
                    if (callBackToClient)
                    {
                        List<string> data = new List<string>();
                        data.Add("removeItem");
                        data.Add(svrId.ToString());
                        SendDataPackage(data);
                    }
                    return true;
                }
            }
            Logger.WriteLine(LogLevel.Info, "玩家{0}尝试使用该玩家没有的物品ID={1}", Name, svrId);
            return false;
        }

        /// <summary>
        /// 提供底层操作的 <see cref="UserSocketState"/>
        /// </summary>
        public UserSocketState SocketState;

        public bool IsDead
        {
            get => Health <= 0;
        }
        
        /// <summary>
        /// 玩家的生命值
        /// </summary>
        public float Health
        {
            get
            {
                string source = MapContext.HealthData.GetFirstYWithX(Name, "100.0");
                float f = 0f;
                if (float.TryParse(source, out f))
                    return f;
                else
                {
                    Logger.WriteLine(LogLevel.Warning, "玩家{0}的生命值记录已失效，原始值为{1}，已更改为100.0f。", Name, source);
                    MapContext.HealthData.SetXToY("Name", (100.0f).ToString());
                    return 100.0f;
                }
            }
            set
            {
                float health;
                if (value <= 0f)
                    health = OnPreDied();
                else if (value > 100f)
                    health = 100f;
                else health = value;
                MapContext.HealthData.SetXToY(Name, health.ToString());
                if (health <= 0f)
                    OnPlayerDied.Call(this);
            }
        }

        const int NoDiedItemStdid = 111;

        float OnPreDied()
        {
            int svrId = -1;
            for (int i = 0; i < Items.Count; i++)
            {
                Item desc = ItemStorage.GetItemDesc(Items[i]);
                if (desc.ItemStaticId == NoDiedItemStdid)
                {
                    svrId = Items[i];
                    goto NoDied;
                }
            }
            return 0f;
        NoDied:
            DeleteItem(svrId);
            SendChat("已经使用一个续命丸", "续命丸");
            return 100f;
        }

        /// <summary>
        /// 为玩家添加物品
        /// </summary>
        /// <param name="staticId">物品类型ID</param>
        /// <param name="desc">物品描述</param>
        /// <param name="extra">物品附加信息</param>
        /// <returns></returns>
        public Item AddItem(int staticId, string desc, string extra)
        {
            if (Items.Count >= MaxPackageSize)
                return null;
            int id = ItemStorage.NewItem(staticId, desc, extra);
            Items.Add(id);
            Item item = ItemStorage.GetItemDesc(id);
            List<string> data = new List<string>();
            data.Add("addItem");
            data.Add(item.ItemStaticId.ToString());
            data.Add(item.ServerId.ToString());
            data.Add(ForgetiveServer.Base64Encode(item.DescribeMessage));
            data.Add(ForgetiveServer.Base64Encode(item.ExtraInfo));
            SendDataPackage(data);
            return item;
        }

        /// <summary>
        /// 拾取物品
        /// </summary>
        /// <param name="serverId">物品的服务器唯一ID</param>
        /// <returns></returns>
        public bool PickUpItem(int serverId)
        {
            if (Items.Count >= MaxPackageSize)
                return false;
            Items.Add(serverId);
            Item item = ItemStorage.GetItemDesc(serverId);
            List<string> data = new List<string>();
            data.Add("addItem");
            data.Add(item.ItemStaticId.ToString());
            data.Add(item.ServerId.ToString());
            data.Add(ForgetiveServer.Base64Encode(item.DescribeMessage));
            data.Add(ForgetiveServer.Base64Encode(item.ExtraInfo));
            SendDataPackage(data);
            return true;
        }

        internal void EvalCommand(string[] recv)
        {
            List<string> param = new List<string>();
            for (int i = 1; i < recv.Length; i++)
                param.Add(recv[i]);
            bool end = false;
            List<string> cmdline = new List<string>();
            for (int i = 0; i < param.Count; i++)
            {
                if (end)
                {
                    end = false;
                    MapContext.WRecv(this, cmdline.ToArray());
                    cmdline = new List<string>();
                    cmdline.Add(param[i]);
                }
                else
                {
                    if (param[i] == "end")
                    {
                        end = true;
                        continue;
                    }
                    cmdline.Add(param[i]);
                }
            }
            if (end)
            {
                end = false;
                MapContext.WRecv(this, cmdline.ToArray());
                cmdline = new List<string>();
            }

            while (SocketState.ReadedData.Count == 0)
                MapContext.EvalReadedData(this);

            List<string> preSend = new List<string>();

            List<string> destcpy_o;
            List_String destcpy_c;

            lock (SocketState.OtherData)
            {
                destcpy_o = new List<string>();
                destcpy_o = FixData(SocketState.OtherData.GetList());
                destcpy_c = new List_String();
                destcpy_c = SocketState.ReadedData;
                SocketState.ReadedData = new List_String();
                SocketState.OtherData.Clear();
                //这一步是指针复制，如果Clear()指针指向的内容也清空了
            }

            preSend.AddRange(destcpy_c.GetList());
            preSend.Add("end");
            preSend.AddRange(destcpy_o);

            destcpy_o.Clear();
            destcpy_c.Clear();

            SocketState.Send(preSend.ToArray());

            preSend.Clear();
        }

        List<string> FixData(List<string> input)
        {
            List<string> result = new List<string>();
            bool isNewStart = true;
            for (int i = 0; i < input.Count; i++)
            {
                if (isNewStart)
                {
                    isNewStart = false;
                    if (input[i] == "end")
                    {
                        isNewStart = true;
                        continue;
                    }
                    result.Add(input[i]);
                }
                else
                {
                    result.Add(input[i]);
                    if (input[i] == "end")
                        isNewStart = true;
                }
            }
            return result;
        }

        /// <summary>
        /// 在玩家客户端上发送文本
        /// </summary>
        /// <param name="msg">文本内容</param>
        /// <param name="source">发送源</param>
        public void SendChat(string msg, string source)
        {
            List<string> data = new List<string>();
            data.Add("chatmsg");
            data.Add(ForgetiveServer.Base64Encode(msg));
            data.Add(ForgetiveServer.Base64Encode(source));
            SendDataPackage(data);
        }

        public void RecvNPCHost(int npcId)
        {

        }

        public void DisposeNPCHost(int npcId)
        {

        }

        public void OnNPCDied(int npcId)
        {

        }

        /// <summary>
        /// 添加在玩家客户端字幕上要显示文本，这些文本需要逐条按发送顺序展示给玩家
        /// </summary>
        /// <param name="msg">文本内容</param>
        /// <param name="color">文本颜色</param>
        /// <param name="showTime">文本停留时间（指文本完全可见的时间，单位为秒，不包括文本逐渐消失需要的时间，逐渐消失时间约为1秒）</param>
        public void SendStoryMessage(string msg, Color color, float showTime)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;
            List<string> data = new List<string>();
            data.Add("storymsg");
            data.Add(ForgetiveServer.Base64Encode(msg));
            data.Add(r.ToString());
            data.Add(g.ToString());
            data.Add(b.ToString());
            data.Add(showTime.ToString());
            SendDataPackage(data);
        }

        /// <summary>
        /// 在客户端字幕缓冲池中撤回包含指定关键字的条目，如果已经在展示的文本包含指定关键字，它将在客户端的下一帧直接进入逐渐消失阶段
        /// </summary>
        /// <param name="msgKeyword">关键字</param>
        public void UndoStoryMessage(string msgKeyword)
        {
            List<string> data = new List<string>();
            data.Add("delstory");
            data.Add(ForgetiveServer.Base64Encode(msgKeyword));
            SendDataPackage(data);
        }

        /// <summary>
        /// 锁定玩家游戏界面并让玩家近似黑屏
        /// </summary>
        /// <param name="needLock">True锁定;False解锁</param>
        public void LockView(bool needLock)
        {
            List<string> data = new List<string>();
            data.Add("lock");
            data.Add(needLock.ToString());
            SendDataPackage(data);
        }

        public static int MaxPoolSize = 102400;

        /// <summary>
        /// 发送客户端操作指令，它将在此玩家-服务器连接的下一帧与其它发送缓冲池的操作指令一起发送给客户端（一帧=客户端中1个CON）。请注意此函数的发送延时是不可忽略的，延时的大小视客户端的连接速度而定，可以利用连接标准数据的时间差准确计算延时
        /// </summary>
        /// <param name="data"></param>
        public void SendDataPackage(List<string> data)
        {
            try
            {
                if (SocketState == null)
                {
                    if (serverUpdateDelay != null)
                        serverUpdateDelay.Dispose();
                }
                lock (SocketState)
                {
                    for (int i = 0; i < data.Count; i++)
                        SocketState.OtherData.Add(data[i]);
                    SocketState.OtherData.Add("end");
                }
                if (SocketState.OtherData.Count >= MaxPoolSize)
                {
                    GlobalServer.Logout(SocketState);
                    Dispose();
                }
            }
            catch
            {
                if (serverUpdateDelay != null)
                    serverUpdateDelay.Dispose();
            }
        }

        /// <summary>
        /// 删除消息池里未发送的指定命令名称的消息段落
        /// </summary>
        /// <param name="commandName"></param>
        public void DeleteDataWithCommandName(string commandName)
        {
            if (SocketState == null) return;
            if (SocketState.OtherData == null) return;
            bool nextRead = true;
            int delIndex = -1, delSize = 0;
            for (int i = 0; i < SocketState.OtherData.Count; i++)
            {
                if (delIndex != -1)
                    delSize++;
                if (nextRead)
                {
                    if (SocketState.OtherData.GetId(i) == commandName)
                    {
                        delIndex = i;
                        delSize = 1;
                    }
                    nextRead = false;
                }
                if (SocketState.OtherData.GetId(i) == "end")
                {
                    nextRead = true;
                    if (delIndex != -1)
                    {
                        SocketState.OtherData.RemoveRange(delIndex, delSize);
                        delIndex = -1;
                        delSize = 0;
                        DeleteDataWithCommandName(commandName);
                    }
                }
            }
        }

        /// <summary>
        /// 在空闲时发送消息，如果已有未发送的与此指令名相同的指令段落则删除旧版本指令段落
        /// </summary>
        /// <param name="data"></param>
        public void ReplaceData(List<string> data)
        {
            if (SocketState == null)
            {
                if (serverUpdateDelay != null)
                    serverUpdateDelay.Dispose();
            }
            DeleteDataWithCommandName(data[0]);
            SendDataPackage(data);
        }

        internal void OnInited()
        {
            if (IsNewPlayer)
            {
                UndoStoryMessage("已进入");
                UndoStoryMessage("已离开");
                SendStoryMessage("欢迎来到Forgetive！", Color.LightCyan, 5);
            }
        }

        internal void Dispose()
        {
            if (animDelay != null)
            {
                animDelay.Dispose();
                animDelay = null;
            }
            if (npcDelay != null)
            {
                npcDelay.Dispose();
                npcDelay = null;
            }
            if (serverUpdateDelay != null)
            {
                serverUpdateDelay.Dispose();
                serverUpdateDelay = null;
            }
        }

        ~Player() => Dispose();
    }
}
