using Forgetive.Server.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forgetive.Server.Extension
{
    public class ClientCommand
    {
        internal List<Key<string, Action<Player, string[]>>> ActionLib;

        internal ClientCommand()
        {
            ActionLib = new List<Key<string, Action<Player, string[]>>>();
        }

        /// <summary>
        /// 注册回调
        /// </summary>
        /// <param name="command">客户端调用的函数名称</param>
        /// <param name="handler">用于处理此函数的方法</param>
        /// <returns>是否注册成功</returns>
        public bool Add(string command, Action<Player, string[]> handler)
        {
            for (int i = 0; i < ActionLib.Count; i++)
            {
                if (ActionLib[i].Value0 == command)
                {
                    Logger.WriteLine(LogLevel.Warning, "未能注册回调: 函数{0}已经被其它处理程序注册", command);
                    return false;
                }
            }
            ActionLib.Add(new Key<string, Action<Player, string[]>>(command, handler));
            return true;
        }

        internal void Call(Player player, string[] msgs)
        {
            if (msgs.Length == 0) return;
            List<string> dstmsg = new List<string>();
            for (int i = 1; i < msgs.Length; i++)
                dstmsg.Add(msgs[i]);
            string[] dest = dstmsg.ToArray();
            for (int i = 0; i < ActionLib.Count; i++)
            {
                if (ActionLib[i].Value0 == msgs[0])
                {
                    try
                    {
                        ActionLib[i].Value1?.Invoke(player, dest);
                        return;
                    }
                    catch (Exception e)
                    {
                        Logger.WriteLine(LogLevel.Warning, "在Ext中执行回调方法时抛出异常");
                        Logger.WriteLine(LogLevel.Warning, e.ToString());
                    }
                }
            }
            Logger.WriteLine(LogLevel.Warning, "未处理客户端函数 {0}", msgs[0]);
        }
    }
}
