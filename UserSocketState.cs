using Forgetive.Server.Maps;
using Forgetive.Server.Test;
using Forgetive.Server.VersionControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Timers;

namespace Forgetive.Server
{
    public class UserSocketState
    {
        public Socket ClientSocket;
        
        KeyGen genKey;
        public MapBase Map;
        public Player PlayerContext;

        public DateTime LastRecv;
        
        public string RecvString { get; set; }
        public string[] RecvParams { get; private set; }
        public bool IsMatched { get; set; }
        public string UsedKey { get; private set; }
        public string NickName { get; private set; }

        static byte[] PKGALIVE;

        public UserSocketState(Socket inputSocket)
        {
            LastRecv = DateTime.Now;
            genKey = new KeyGen();
            if (PKGALIVE == null)
                PKGALIVE = new byte[ForgetiveServer.fixedSize];
            IsMatched = false;
            ClientSocket = inputSocket;
            ReadedData = new List_String();
            OtherData = new List_String();
        }

        /// <summary>
        /// 仅从ForgetiveServer中调用，用于释放资源
        /// </summary>
        public void Close()
        {
            if (OtherData != null)
            {
                OtherData.Clear();
                OtherData = null;
            }
            if (ReadedData != null)
            {
                ReadedData.Clear();
                ReadedData = null;
            }
            if (PlayerContext != null)
            {
                if (PlayerContext.MapContext != null)
                {
                    try
                    {
                        PlayerContext.MapContext.QuitMap(PlayerContext);
                    }
                    catch { }
                    PlayerContext.Dispose();
                }
            }
            genKey.Dispose();
            try
            {
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Close();
            }
            catch { }
            try
            {
                if (Map != null)
                {
                    for (int i = 0; i < Map.players.Count; i++)
                    {
                        if (Map.players[i].SocketState == this)
                        {
                            Map.players.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }
                }
            }
            catch { }
            if (PlayerContext != null)
                PlayerContext.Dispose();
        }

        public void Send(ServerResult sr)
        {
            Send(sr.Value());
        }

        public void Send(params string[] msg)
        {
            string combd = "";
            for (int i = 0; i < msg.Length; i++)
            {
                combd += msg[i];
                if (i != msg.Length - 1)
                    combd += "\n";
            }
            Send(combd);
        }

        public void Send(string msg)
        {
            byte[] data = Encoding.UTF8.GetBytes(Encrypt(msg) + "`");
            WaitForSocket();
            __SOCKETLOCK = true;
            ClientSocket.BeginSend(data, 0, data.Length, SocketFlags.None, SendSuccess, ClientSocket);
        }

        static bool slock = false;
        
        public static bool __SOCKETLOCK
        {
            get { return slock; }
            set
            {
                if (!IsSocketInSingleThreadMode) return;
                slock = value;
                if (slock)
                {
                    Console.Title = "Forgetive Server - 网络单线程模式 - Socket 活动中";
                }
                else
                {
                    Console.Title = "Forgetive Server - 网络单线程模式";
                }
            }
        }

        /// <summary>
        /// 是否强制Socket单线程，开启会显著降低网络带宽占用，会降低服务器性能
        /// </summary>
        public static bool IsSocketInSingleThreadMode = false;

        public static void WaitForSocket()
        {
            if (!IsSocketInSingleThreadMode) return;
            while (__SOCKETLOCK)
                Thread.Sleep(10);
        }

        [Obsolete("消息体结尾需要约定终止符，建议使用Send(string)方法", true)]
        public void Send(byte[] data)
        {
            WaitForSocket();
            __SOCKETLOCK = true;
            ClientSocket.BeginSend(data, 0, data.Length, SocketFlags.None, SendSuccess, ClientSocket);
        }

        void SendSuccess(IAsyncResult result)
        {
            try
            {
                __SOCKETLOCK = false;
                ((Socket)result.AsyncState).EndSend(result);
            }
            catch
            {
                ForgetiveServer.Close(this);
            }
        }

        /// <summary>
        /// 已读回执内容
        /// 通常是其他玩家的信息
        /// </summary>
        public List_String ReadedData;

        /// <summary>
        /// 已读回执中的附加项目
        /// </summary>
        public List_String OtherData;

        bool IsMatchVersion = false;
        public VersionControl.Version version;

        public void RecvData()
        {
            RecvParams = RecvString.Split('\n');
            if (!IsMatched)
            {
                ServerResult ret;
                if (IsMatchVersion)
                {
                    if (RecvParams.Length == 3)
                    {
                        if (RecvParams[0] == "reg")
                        {
                            ret = Program.RegDevice(RecvParams[1], RecvParams[2]);
                            if (ret.IsSuccess)
                            {
                                IsMatched = true;
                                UsedKey = RecvParams[1];
                                NickName = Program.key_nick.GetFirstYWithX(UsedKey, GetMD5(UsedKey));
                            }
                            Send(ret.Value());
                            return;
                        }
                        else return;
                    }
                }
                if (RecvString.Contains("PUBLICTIMESTAMP"))
                {
                    if (RecvString.Split(';').Length == 2)
                    {
                        string s = RecvString.Split(';')[1];
                        if (int.TryParse(s, out int v))
                        {
                            version = new VersionControl.Version(v);
                            if (ServerVersion.CheckVersion(v))
                                IsMatchVersion = true;
                        }
                    }
                    Send(DateTime.Now.ToBinary().ToString() + ";" + IsMatchVersion);
                    return;
                }
                if (IsMatchVersion)
                {
                    //登陆取到的设备
                    ret = Program.LogDevice(RecvString);
                    if (ret.IsSuccess)
                    {
                        IsMatched = true;
                        UsedKey = ret.InternalEx;
                        //在key:nick表中获取玩家昵称，若为空用Key的MD5值临时代替
                        NickName = Program.key_nick.GetFirstYWithX(UsedKey, GetMD5(UsedKey));
                        Send(ret.Value());
                    }
                    else
                    {
                        if (ret.Note == "nomac")
                        {
                            if (genKey.Gen())
                                ret.Note = genKey.Result;
                        }
                        Send(ret.Value());
                    }
                }
                return;
            }
            if (RecvParams.Length > 1)
            {
                if (RecvParams[0] == "map")
                {
                    if (PlayerContext != null)
                        PlayerContext.EvalCommand(RecvParams);
                    return;
                }
            }
            try
            {
                ServerResult sresult = OnRecvServerData();
                if (sresult != null)
                    Send(sresult);
            }
            catch
            {
                ConsoleColor color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("在处理来自");
                SayPlayerInfo();
                Logger.WriteLine("的数据时，遇到了错误。");
                Logger.WriteLine("这通常说明此玩家有异常的游戏行为或使用了错误版本的客户端。");
                ForgetiveServer.Close(this);
                Console.ForegroundColor = color;
            }
        }

        public void SayPlayerInfo()
        {
            Logger.WriteLine("玩家 {0}:{1} :",
                ((IPEndPoint)ClientSocket.RemoteEndPoint).Address.ToString(),
                ((IPEndPoint)ClientSocket.RemoteEndPoint).Port.ToString());
            Logger.WriteLine("    已激活 : {0}", IsMatched.ToString());
            Logger.WriteLine("    登陆密匙 : {0}", UsedKey);
            Logger.WriteLine("    玩家名称 : {0}", NickName);
        }

        ServerResult OnRecvServerData()
        {
            string cmd = RecvParams[0];
            ServerResult ret;
            switch (cmd)
            {
                case "rename":
                    ret = new ServerResult();
                    lock (Program.key_nick)
                    {
                        string rec = ForgetiveServer.Base64Decode(RecvParams[1]);
                        if (CheckNickName(rec))
                        {
                            Program.key_nick.SetXToY(UsedKey, rec);
                            NickName = rec;
                            ret.IsSuccess = true;
                        }
                        else
                        {
                            ret.IsSuccess = false;
                        }
                    }
                    return ret;
                case "minf":
                    return new ServerResult(true, XCoin + ";" + BP);
                case "join":
                    if (RecvParams.Length == 2)
                        GlobalServer.JoinMap(this, RecvParams[1]);
                    break;
            }
            return null;
        }

        bool CheckNickName(string name)
        {
            string[][] list = Program.key_nick.List();
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i][1] == name)
                    return false;
            }
            return true;
        }

        public int XCoin
        {
            get
            {
                if (int.TryParse(Program.nick_xcoin.GetFirstYWithX(UsedKey), out int d))
                    return d;
                else
                {
                    Program.nick_xcoin.SetXToY(UsedKey, "0");
                    return 0;
                }
            }
            set
            {
                Program.nick_xcoin.SetXToY(UsedKey, value.ToString());
            }
        }

        public int BP
        {
            get
            {
                if (int.TryParse(Program.nick_bp.GetFirstYWithX(UsedKey), out int d))
                    return d;
                else
                {
                    Program.nick_bp.SetXToY(UsedKey, "0");
                    return 0;
                }
            }
            set
            {
                Program.nick_bp.SetXToY(UsedKey, value.ToString());
            }
        }

        static string GetMD5(string sDataIn)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytValue, bytHash;
            bytValue = Encoding.UTF8.GetBytes(sDataIn);
            bytHash = md5.ComputeHash(bytValue);
            md5.Clear();
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            }
            return sTemp.ToLower();
        }

#if !DEBUG
        readonly static byte[] btKey = new byte[8] { 23, 189, 213, 173, 34, 82, 137, 38 };
        readonly static byte[] btIv = new byte[8] { 32, 55, 133, 138, 23, 94, 230, 21 };
#else
        readonly static byte[] btKey = new byte[8] { 23, 189, 213, 173, 34, 82, 137, 38 };
        readonly static byte[] btIv = new byte[8] { 32, 55, 133, 138, 23, 94, 230, 21 };
#endif

        internal static string Encrypt(string sourceString)
        {
            var des = new DESCryptoServiceProvider();
            using (var ms = new MemoryStream())
            {
                byte[] inData = Encoding.Default.GetBytes(sourceString);
                try
                {
                    using (var cs = new CryptoStream(ms, des.CreateEncryptor(btKey, btIv), CryptoStreamMode.Write))
                    {
                        cs.Write(inData, 0, inData.Length);
                        cs.FlushFinalBlock();
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        internal static string Decrypt(string encryptedString)
        {
            var des = new DESCryptoServiceProvider();
            using (var ms = new MemoryStream())
            {
                try
                {
                    byte[] inData = Convert.FromBase64String(encryptedString);
                    using (var cs = new CryptoStream(ms, des.CreateDecryptor(btKey, btIv), CryptoStreamMode.Write))
                    {
                        cs.Write(inData, 0, inData.Length);
                        cs.FlushFinalBlock();
                    }
                    return Encoding.Default.GetString(ms.ToArray());
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
