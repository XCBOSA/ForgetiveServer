using Forgetive.Server.Maps;
using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server.Command
{
    public class SetMaxPoolSize : IForgetiveCommand
    {
        public SetMaxPoolSize()
        {
            Info.CommandNames = new string[]
            {
                "smps"
            };
            Info.HelpText = "设置玩家最大消息池大小。用法 smps <大小>";
        }

        public override void OnInit()
        {

        }

        public override void OnInvoke(CommandObject[] obj)
        {
            if (obj.Length != 1)
            {
                Print(Info.HelpText);
                return;
            }
            if (obj[0].type == Program.engine.STRING && !obj[0].IsNull)
            {
                int count;
                if (int.TryParse((string)obj[0].value, out count))
                {
                    Player.MaxPoolSize = count;
                }
                else
                {
                    Print(Info.HelpText);
                }
            }
        }
    }
}
