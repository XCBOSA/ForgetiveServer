using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server.Command
{
    public class ListBan : IForgetiveCommand
    {
        public ListBan()
        {
            Info.CommandNames = new string[]
            {
                "listban"
            };
            Info.HelpText = "显示所有被封禁的玩家";
        }

        public override void OnInit()
        {

        }

        public override void OnInvoke(CommandObject[] obj)
        {
            Print("已封禁的玩家");
            string[][] players = Program.key_banned.List();
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i][1] != "{notbanned}")
                {
                    string name = Program.key_nick.GetFirstYWithX(players[i][0]);
                    Print(i + "; nickname=" + name + "; key=" + players[i][0] + "");
                }
            }
        }
    }
}
