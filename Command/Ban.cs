using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server.Command
{
    public class Ban : IForgetiveCommand
    {
        public Ban()
        {
            Info.CommandNames = new string[]
            {
                "ban"
            };
            Info.HelpText = "封禁玩家。用法 ban <玩家1> [玩家2] [玩家3] ...";
        }

        public override void OnInit()
        {

        }

        public override void OnInvoke(CommandObject[] obj)
        {
            string[][] players = Program.key_banned.List();
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i][1] == "{notbanned}")
                {
                    string name = Program.key_nick.GetFirstYWithX(players[i][0]);
                    for (int j = 0; j < obj.Length; j++)
                    {
                        if (obj[j].type == Program.engine.STRING && !obj[j].IsNull)
                        {
                            string str = obj[j].GetValue<string>();
                            if (name == str)
                            {
                                Program.key_banned.SetXToY(players[i][0], "{banned}");
                                Print(i + "; nickname=" + name + "; key=" + players[i][0] + "; 已封禁玩家");
                                InvokeAnotherCommandUseEngine("kick \"" + name + "\"");
                            }
                        }
                    }
                }
            }
        }
    }
}
