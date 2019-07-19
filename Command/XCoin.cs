using Forgetive.Server.Maps;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Forgetive.Server.Command
{
    public class XCoin : IForgetiveCommand
    {
        public XCoin()
        {
            Info.CommandNames = new string[]
            {
                "xcoin"
            };
            Info.HelpText = "给在线玩家X币。用法 xcoin <玩家> <X币数量>";
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
                string name = (string)obj[0].value;
                int count;
                if (!int.TryParse((string)obj[1].value, out count))
                {
                    Print(Info.HelpText);
                    return;
                }
                for (int i = 0; i < GlobalServer.Maps.Count; i++)
                {
                    Player player = GlobalServer.Maps[i].FindPlayer(name);
                    if (player != null)
                    {
                        player.XCoin += count;
                        player.SendStoryMessage("你获得了" + count + "X币", Color.Green, 8);
                        Print("操作成功");
                        return;
                    }
                }
                Print("玩家不存在或不在线");
            }
        }
    }
}
