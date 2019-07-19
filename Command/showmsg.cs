using Forgetive.Server.Maps;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Forgetive.Server.Command
{
    public class showmsg : IForgetiveCommand
    {
        public showmsg()
        {
            Info.CommandNames = new string[]
            {
                "showmsg"
            };
            Info.HelpText = "发送剧情文本。用法 showmsg <玩家> <文字>";
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
                for (int i = 0; i < GlobalServer.Maps.Count; i++)
                {
                    Player c = GlobalServer.Maps[i].FindPlayer((string)obj[0].value);
                    if (c != null)
                    {
                        c.SendStoryMessage((string)obj[1].value, Color.GreenYellow, 10);
                        return;
                    }
                }
                Print("玩家不存在或不在线");
            }
        }
    }
}
