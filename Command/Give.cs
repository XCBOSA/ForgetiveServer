using Forgetive.Server.Maps;
using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server.Command
{
    public class Give : IForgetiveCommand
    {
        public Give()
        {
            Info.CommandNames = new string[]
            {
                "give"
            };
            Info.HelpText = "给在线玩家物品。用法 give <玩家> <物品类型ID> <物品描述> <物品附加信息> [数量]";
        }

        public override void OnInit()
        {

        }

        public override void OnInvoke(CommandObject[] obj)
        {
            if (obj.Length != 4 && obj.Length != 5)
            {
                Print(Info.HelpText);
                return;
            }
            if (obj[0].type == Program.engine.STRING && !obj[0].IsNull &&
                obj[1].type == Program.engine.STRING && !obj[1].IsNull &&
                obj[2].type == Program.engine.STRING && !obj[2].IsNull &&
                obj[3].type == Program.engine.STRING && !obj[3].IsNull)
            {
                string name = (string)obj[0].value;
                int staticId;
                int count = 1;
                if (!int.TryParse((string)obj[1].value, out staticId))
                {
                    Print("给玩家物品。用法 give <玩家> <物品类型ID> <物品描述> <物品附加信息> [数量]");
                    return;
                }
                string desc = (string)obj[2].value;
                string extra = (string)obj[3].value;
                if (obj.Length == 5)
                {
                    if (!int.TryParse((string)obj[4].value, out count))
                    {
                        Print("给玩家物品。用法 give <玩家> <物品类型ID> <物品描述> <物品附加信息> [数量]");
                        return;
                    }
                }
                for (int i = 0; i < GlobalServer.Maps.Count; i++)
                {
                    Player player = GlobalServer.Maps[i].FindPlayer(name);
                    if (player != null)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            player.AddItem(staticId, desc, extra);
                        }
                        Print("操作成功");
                        return;
                    }
                }
                Print("玩家不存在或不在线");
            }
        }
    }
}
