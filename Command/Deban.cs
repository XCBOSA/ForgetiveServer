using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server.Command
{
    public class Deban : IForgetiveCommand
    {
        public Deban()
        {
            Info.CommandNames = new string[]
            {
                "deban"
            };
            Info.HelpText = "解除封禁玩家。用法 deban <玩家1> [玩家2] [玩家3] ...";
        }

        public override void OnInit()
        {

        }

        public override void OnInvoke(CommandObject[] obj)
        {
            string[][] players = Program.key_banned.List();
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i][1] != "{notbanned}")
                {
                    string name = Program.key_nick.GetFirstYWithX(players[i][0]);
                    for (int j = 0; j < obj.Length; j++)
                    {
                        if (obj[j].type == Program.engine.STRING && !obj[j].IsNull)
                        {
                            string str = obj[j].GetValue<string>();
                            if (name == str)
                            {
                                Program.key_banned.SetXToY(players[i][0], "{notbanned}");
                                Print(i + "; nickname=" + name + "; key=" + players[i][0] + "; 已经解除封禁");
                            }
                        }
                    }
                }
            }
        }
    }
}
