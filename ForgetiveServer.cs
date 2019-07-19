using Forgetive.Database;
using Forgetive.Server.Maps;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Forgetive.Server
{
    public static class ForgetiveServer
    {
        static Socket socket;
        static Table config;

        static IPAddress serverip;
        static int port;

        public static List<UserSocketState> Users;
        public static int UserCount { get => Users.Count; }

        public static CallBack<Action<UserSocketState>> OnPlayerQuit;

        public static string DefaultMapName;

        public static MapBase DefaultMap
        {
            get
            {
                return GlobalServer.FindMap(DefaultMapName);
            }
            set
            {
                DefaultMapName = value.MapName;
            }
        }

        public static void Init()
        {
            OnPlayerQuit = new CallBack<Action<UserSocketState>>();
            config = Data.GetTable("server", "config", false);
            if (config.IsNewCreated)
            {
                config.SetXToY("ip-addr", "127.0.0.1");
                config.SetXToY("ip-port", "32767");
                config.Save();
                Program.ShutDown("请在 ./server-config 设置完成服务器信息然后重新启动服务器。");
                return;
            }

            string addrstr = config.GetFirstYWithX("ip-addr");

            serverip = IPAddress.Parse(addrstr);
            port = int.Parse(config.GetFirstYWithX("ip-port"));

            Users = new List<UserSocketState>();

            socket = new Socket(serverip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(serverip, port));
            socket.Listen(1024);
            
            Thread listener = new Thread(AcceptLoop);
            listener.Start(socket);

            Logger.WriteLine("已在" + addrstr + "上初始化成功。");
        }

        static void AcceptLoop(object server)
        {
            Socket serv = (Socket)server;
            while (true)
            {
                Socket client = serv.Accept();
                Logger.WriteLine("{0}:{1} 连接到服务器。", ((IPEndPoint)client.RemoteEndPoint).Address.ToString(),
                    ((IPEndPoint)client.RemoteEndPoint).Port.ToString());
                Thread thread = new Thread(Accept);
                thread.Start(client);
            }
        }

        public const int fixedSize = 256;

        static void Accept(object user)
        {
            Socket client = (Socket)user;
            UserSocketState state = new UserSocketState(client);
            Users.Add(state);
            state.ClientSocket = client;
            Thread dataPushThread = new Thread(UserDataRecv);
            dataPushThread.Start(state);
        }

        static void UserDataRecv(object State)
        {
            UserSocketState state = (UserSocketState)State;
            Socket client = state.ClientSocket;
            string recvDataTemp = "";
            while (true)
            {
                try
                {
                    byte[] recvByte = new byte[fixedSize];
                    int rc = client.Receive(recvByte);
                    state.LastRecv = DateTime.Now;
                    if (rc == 0)
                    {
                        Close(state);
                        break;
                    }
                    recvDataTemp += Encoding.UTF8.GetString(recvByte).Trim().Replace("\0", "");
                    if (recvDataTemp[recvDataTemp.Length - 1] == '`')
                    {
                        state.RecvString = UserSocketState.Decrypt(recvDataTemp.Substring(0, recvDataTemp.Length - 1));
                        recvDataTemp = "";
                        state.RecvData();
                    }
                }
                catch (Exception e)
                {
                    Logger.WriteLine(e.StackTrace);
                    Logger.WriteLine(e.ToString());
                    if (e is SocketException)
                    {
                        Logger.WriteLine("------需要终止连接------");
                        Close(state);
                    }
                    break;
                }
            }
        }

        public static void Close(UserSocketState state, string reason = null, bool needRemByThisFunc = true)
        {
            try
            {
                Logger.WriteLine("{0}:{1} 离开了服务器。", ((IPEndPoint)state.ClientSocket.RemoteEndPoint).Address.ToString(),
                    ((IPEndPoint)state.ClientSocket.RemoteEndPoint).Port.ToString());
                if (reason != null) Logger.WriteLine("原因: " + reason);
                if (state != null)
                {
                    if (needRemByThisFunc)
                        Users.Remove(state);
                    state.Close();
                }
            }
            catch { }
            Exception[] excs = OnPlayerQuit.Call(state);
            for (int i = 0; i < excs.Length; i++)
                Logger.WriteLine(excs.ToString());
        }

        public static void WaitExecute(int time, Action code)
        {
            System.Timers.Timer t = new System.Timers.Timer();
            t.Interval = time;
            t.AutoReset = false;
            t.Elapsed += (m, e) =>
            {
                t.Enabled = false;
                t.Close();
                t.Dispose();
                code?.Invoke();
            };
            t.Enabled = true;
        }

        public static string Base64Decode(string str)
        {
            byte[] bytes = Convert.FromBase64String(str);
            bytes = Convert.FromBase64String(Encoding.UTF8.GetString(bytes));
            return Encoding.UTF8.GetString(bytes);
        }

        public static string Base64Encode(string str)
        {
            string go = Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(go));
        }

        /// <summary>
        /// 此方法不会返回控制，让线程指令比率达到100%来标记线程无效以便上级线程顺利的执行 <see cref="Thread.Abort"/>
        /// </summary>
        public static void FillThread()
        {
            while (true) { }
        }
    }
}
