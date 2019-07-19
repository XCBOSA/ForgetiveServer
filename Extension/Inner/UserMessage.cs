using Forgetive.Server.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forgetive.Server.Extension.Inner
{
    public class UserMessage : ExecutionContent
    {
        public List<Key<string, string>> userSendMsgs;
        ClientCommand handler;

        public override void Init()
        {
            userSendMsgs = new List<Key<string, string>>();
            GetSudo();
            if (!Sudo)
            {
                SudoContent sudoContent = new SudoContent(this);
                sudoContent.GetExtHash();
                throw new CannotLoadExecutionContentException();
            }
            handler = GetClientCommandHandler();
            handler.Add("sendUserMsg", SendUserMsg);
        }

        public void SendUserMsg(Player player, string[] args)
        {
            if (args.Length != 1) return;
            string msg = ForgetiveServer.Base64Decode(args[0]);
            if (msg[0] == '@')
            {
                string cmd = msg.Substring(1);
                Program.engine.ExecuteByPlayer(cmd, player);
                Logger.WriteLine(LogLevel.Info, "玩家调用指令 <" + player.Name + "> " + msg);
            }
            else
            {
                Logger.WriteLine(LogLevel.Info, "消息 <" + player.Name + "> " + msg);
                userSendMsgs.Add(new Key<string, string>(player.Name, msg));
                Player[] ps = player.MapContext.GetPlayers();
                for (int i = 0; i < ps.Length; i++)
                    ps[i].SendChat(msg, player.Name);
            }
        }
    }
}
