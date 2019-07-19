using Forgetive.Server.Command;
using Forgetive.Database;
using Forgetive.Server.Maps;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;
using Forgetive.Server.Extension;

namespace Forgetive.Server
{
    public static class GlobalServer
    {
        public static List<MapBase> Maps;
        public static Island IslandMap;

        public static void Init(Assembly[] assemblies)
        {
            MaxWaitingTime = new TimeSpan(6, 0, 0);

            Maps = new List<MapBase>();
            IslandMap = new Island();

            Maps.Add(IslandMap);

            ForgetiveCommandCenter.cmds = new List<IForgetiveCommand>();
            ForgetiveCommandCenter.FindCommands(assemblies);

            Thread thread = new Thread(() => { while (Loop()) ; });
            thread.Start();

            if (UserSocketState.IsSocketInSingleThreadMode)
                Logger.WriteLine("--网络单线程模式会降低服务器性能，仅测试使用");
        }

        public static void JoinMap(UserSocketState player, string mapName)
        {
            for (int i = 0; i < Maps.Count; i++)
            {
                if (Maps[i].MapName == mapName)
                {
                    player.Map = Maps[i];
                    Maps[i].JoinMap(player);
                }
            }
        }

        public static MapBase FindMap(string mapName)
        {
            for (int i = 0; i < Maps.Count; i++)
            {
                if (Maps[i].MapName == mapName)
                    return Maps[i];
            }
            return null;
        }

        public static Key<Player, MapBase> ServerFindPlayer(string name)
        {
            for (int i = 0; i < Maps.Count; i++)
            {
                for (int j = 0; j < Maps[i].players.Count; j++)
                {
                    if (Maps[i].players[j].Name == name)
                    {
                        return new Key<Player, MapBase>(Maps[i].players[j], Maps[i]);
                    }
                }
            }
            return null;
        }

        public static Key<Player, MapBase>[] ServerGetPlayers()
        {
            List<Key<Player, MapBase>> lkpm = new List<Key<Player, MapBase>>();
            for (int i = 0; i < Maps.Count; i++)
            {
                for (int j = 0; j < Maps[i].players.Count; j++)
                {
                    lkpm.Add(new Key<Player, MapBase>(Maps[i].players[j], Maps[i]));
                }
            }
            return lkpm.ToArray();
        }

        static TimeSpan MaxWaitingTime;

        static bool Loop()
        {
            Thread.Sleep(10000);
            for (int i = 0; i < ForgetiveServer.UserCount; i++)
            {
                if ((DateTime.Now - ForgetiveServer.Users[i].LastRecv) > MaxWaitingTime)
                {
                    Logout(ForgetiveServer.Users[i]);
                }
            }
            return true;
        }

        public static void Logout(UserSocketState sock)
        {
            if (sock.Map != null && sock.PlayerContext != null)
            {
                sock.Map.QuitMap(sock.PlayerContext);
                ForgetiveServer.Close(sock, "客户端无响应");
                sock.Close();
            }
        }
    }
}
