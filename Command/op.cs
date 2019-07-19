using Forgetive.Server.Maps;
using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server.Command
{
    public class op : IForgetiveCommand
    {
        public op()
        {
            Info.CommandNames = new string[]
            {
                "op"
            };
            Info.HelpText = "设置在线玩家是否为OP。用法 op <set/rem> <玩家>";
        }

        public override void OnInit()
        {

        }

        public override void OnInvoke(CommandObject[] obj)
        {
            if (obj.Length != 2)
            {
                Print(Info.HelpText);
                return;
            }
            if (obj[0].type == Program.engine.STRING && !obj[0].IsNull &&
                obj[1].type == Program.engine.STRING && !obj[1].IsNull)
            {
                bool isOp = false;
                if ((string)obj[0].value == "set") isOp = true;
                else if ((string)obj[0].value == "rem") isOp = false;
                else
                {
                    Print(Info.HelpText);
                    return;
                }
                for (int i = 0; i < GlobalServer.Maps.Count; i++)
                {
                    Player c = GlobalServer.Maps[i].FindPlayer((string)obj[1].value);
                    if (c != null)
                    {
                        c.IsOp = isOp;
                        Print("操作成功完成");
                        return;
                    }
                }
                Print("玩家不存在或不在线");
            }
        }
    }
}
