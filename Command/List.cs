using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server.Command
{
    class Listp : IForgetiveCommand
    {
        public Listp() : base()
        {
            Info.CommandNames = new string[]
            {
                "list"
            };
            Info.HelpText = "显示在线的玩家列表";
            Info.CanPlayerUsed = true;
        }

        public override void OnInit()
        {

        }

        public override void OnInvoke(CommandObject[] obj)
        {
            Print("当前共 " + ForgetiveServer.UserCount + " 人在线。");
            for (int i = 0; i < ForgetiveServer.UserCount; i++)
            {
                UserSocketState user = ForgetiveServer.Users[i];
                if (user.NickName == null)
                    Print("没有设置名字的神秘玩家");
                else Print(i + "; " + user.NickName);
            }
        }
    }
}
